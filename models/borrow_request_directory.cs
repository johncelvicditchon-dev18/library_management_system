using System;
using System.Collections.Generic;
using System.Globalization;
using MySql.Data.MySqlClient;
using oop_project.Database;

namespace oop_project.Models
{
    /// <summary>
    /// All SQL for the student request workflow. Students only see their own
    /// requests; the librarian tab reads the pending queue and approves one.
    /// </summary>
    public class BorrowRequestDirectory
    {
        private readonly IDatabaseConnection _database = new MySqlDatabaseConnection();

        private const string RequestSelect =
            "SELECT r.`request_id`, r.`account_id`, r.`book_id`, " +
            "       COALESCE(b.`book_title`, ''), COALESCE(b.`author`, ''), COALESCE(b.`year_published`, 0), " +
            "       a.`name`, COALESCE(s.`school_id`, ''), COALESCE(p.`program_name`, ''), " +
            "       COALESCE(s.`year_level`, ''), COALESCE(i.`institute_code`, ''), " +
            "       r.`request_date`, COALESCE(rs.`status_code`, 'PENDING'), " +
            "       COALESCE(r.`processed_by`, 0), r.`processed_date`, COALESCE(r.`borrow_id`, 0), " +
            "       COALESCE(r.`copies_requested`, 1), COALESCE(r.`claimed_by`, 0), r.`claim_date` " +
            "FROM `borrow_requests_table` r " +
            "JOIN `account_table` a ON a.`account_id` = r.`account_id` " +
            "LEFT JOIN `request_status_table` rs ON rs.`request_status_id` = r.`status_id` " +
            "LEFT JOIN `books_table` b ON b.`book_id` = r.`book_id` " +
            "LEFT JOIN `student_info_table` s ON s.`account_id` = r.`account_id` " +
            "LEFT JOIN `program_table` p ON p.`program_id` = s.`program_id` " +
            "LEFT JOIN `institute_table` i ON i.`institute_id` = p.`institute_id` ";

        private static bool _schemaReady;

        public int Create(int accountId, Book book, int copies = 1)
        {
            if (accountId <= 0)
            {
                throw new ArgumentException("A signed in account is required.", nameof(accountId));
            }

            if (book == null)
            {
                throw new ArgumentNullException(nameof(book));
            }

            if (copies < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(copies), "Request at least one copy.");
            }

            if (!book.IsAvailable || book.Quantity <= 0 || book.BookId <= 0)
            {
                throw new InvalidOperationException("That book has no copy available to request.");
            }

            using (MySqlConnection connection = _database.CreateConnection())
            {
                connection.Open();
                EnsureSchema(connection);

                // Re-check the shelf in MySQL, not just the grid the student is
                // looking at, so a copy borrowed a second ago is not requested.
                const string availabilitySql =
                    "SELECT b.`copies`, " +
                    "       (SELECT COUNT(*) FROM `borrow_transactions_table` t " +
                    "        WHERE t.`book_id` = b.`book_id` AND t.`date_return` IS NULL) " +
                    "FROM `books_table` b WHERE b.`book_id` = @bookId;";

                using (MySqlCommand availability = new MySqlCommand(availabilitySql, connection))
                {
                    availability.Parameters.AddWithValue("@bookId", book.BookId);

                    using (MySqlDataReader reader = availability.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            throw new InvalidOperationException("That book is no longer in the catalog.");
                        }

                        int shelfCopies = reader.IsDBNull(0)
                            ? 0
                            : Convert.ToInt32(reader.GetValue(0), CultureInfo.InvariantCulture);
                        int openLoans = reader.IsDBNull(1)
                            ? 0
                            : Convert.ToInt32(reader.GetValue(1), CultureInfo.InvariantCulture);

                        int available = shelfCopies - openLoans;

                        if (available <= 0)
                        {
                            throw new InvalidOperationException("That book has no copy available to request.");
                        }

                        if (copies > available)
                        {
                            throw new InvalidOperationException(
                                "ONLY " + available.ToString(CultureInfo.InvariantCulture) +
                                " COPY(IES) OF THAT BOOK CAN BE REQUESTED.");
                        }
                    }
                }

                // A patron may borrow the same title more than once while copies
                // remain on the shelf, so only an existing PENDING request is
                // checked here. The shelf count above still blocks an empty title.
                const string pendingSql = "SELECT COUNT(*) FROM `borrow_requests_table` " +
                                          "WHERE `account_id` = @accountId AND `book_id` = @bookId " +
                                          "AND `status_id` = (SELECT `request_status_id` FROM `request_status_table` " +
                                          "                    WHERE `status_code` = 'PENDING');";

                using (MySqlCommand pending = new MySqlCommand(pendingSql, connection))
                {
                    pending.Parameters.AddWithValue("@accountId", accountId);
                    pending.Parameters.AddWithValue("@bookId", book.BookId);
                    long count = Convert.ToInt64(pending.ExecuteScalar(), CultureInfo.InvariantCulture);

                    if (count > 0)
                    {
                        throw new InvalidOperationException("You already have a pending request for that book.");
                    }
                }

                const string sql = "INSERT INTO `borrow_requests_table` " +
                                   "(`account_id`, `book_id`, `request_date`, `status_id`, `copies_requested`) " +
                                   "VALUES (@accountId, @bookId, NOW(), " +
                                   "        (SELECT `request_status_id` FROM `request_status_table` WHERE `status_code` = 'PENDING'), " +
                                   "        @copies);";

                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@accountId", accountId);
                    command.Parameters.AddWithValue("@bookId", book.BookId);
                    command.Parameters.AddWithValue("@copies", copies);
                    command.ExecuteNonQuery();
                    return (int)command.LastInsertedId;
                }
            }
        }

        public List<BorrowRequestRow> LoadForAccount(int accountId)
        {
            using (MySqlConnection connection = _database.CreateConnection())
            {
                connection.Open();
                EnsureSchema(connection);

                using (MySqlCommand command = new MySqlCommand(
                    RequestSelect + "WHERE r.`account_id` = @accountId ORDER BY r.`request_date` DESC, r.`request_id` DESC;", connection))
                {
                    command.Parameters.AddWithValue("@accountId", accountId);
                    return ReadRows(command);
                }
            }
        }

        public List<BorrowRequestRow> LoadPending()
        {
            using (MySqlConnection connection = _database.CreateConnection())
            {
                connection.Open();
                EnsureSchema(connection);

                using (MySqlCommand command = new MySqlCommand(
                    RequestSelect + "WHERE rs.`status_code` = 'PENDING' ORDER BY r.`request_date` ASC, r.`request_id` ASC;", connection))
                {
                    return ReadRows(command);
                }
            }
        }

        /// <summary>
        /// The librarian work queue: everything still waiting for an APPROVE
        /// decision plus every approved request that the student has not claimed
        /// yet. Claimed, rejected and cancelled rows leave the queue.
        /// </summary>
        public List<BorrowRequestRow> LoadQueue()
        {
            using (MySqlConnection connection = _database.CreateConnection())
            {
                connection.Open();
                EnsureSchema(connection);

                const string order =
                    "WHERE rs.`status_code` IN ('PENDING', 'APPROVED') " +
                    "ORDER BY CASE WHEN rs.`status_code` = 'PENDING' THEN 0 ELSE 1 END, " +
                    "         r.`request_date` ASC, r.`request_id` ASC;";

                using (MySqlCommand command = new MySqlCommand(RequestSelect + order, connection))
                {
                    return ReadRows(command);
                }
            }
        }

        public void Cancel(int requestId, int accountId)
        {
            using (MySqlConnection connection = _database.CreateConnection())
            {
                connection.Open();

                const string sql =
                    "UPDATE `borrow_requests_table` " +
                    "SET `status_id` = (SELECT `request_status_id` FROM `request_status_table` WHERE `status_code` = 'CANCELLED'), " +
                    "    `processed_date` = NOW() " +
                    "WHERE `request_id` = @requestId AND `account_id` = @accountId " +
                    "  AND `status_id` = (SELECT `request_status_id` FROM `request_status_table` WHERE `status_code` = 'PENDING');";

                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@requestId", requestId);
                    command.Parameters.AddWithValue("@accountId", accountId);

                    if (command.ExecuteNonQuery() == 0)
                    {
                        throw new InvalidOperationException("That request is no longer pending.");
                    }
                }
            }
        }

        public void Reject(BorrowRequestRow request, int processedBy)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            using (MySqlConnection connection = _database.CreateConnection())
            {
                connection.Open();

                const string sql =
                    "UPDATE `borrow_requests_table` " +
                    "SET `status_id` = (SELECT `request_status_id` FROM `request_status_table` WHERE `status_code` = 'REJECTED'), " +
                    "    `processed_by` = @processedBy, `processed_date` = NOW() " +
                    "WHERE `request_id` = @requestId " +
                    "  AND `status_id` = (SELECT `request_status_id` FROM `request_status_table` WHERE `status_code` = 'PENDING');";

                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@processedBy", processedBy);
                    command.Parameters.AddWithValue("@requestId", request.RequestId);

                    if (command.ExecuteNonQuery() == 0)
                    {
                        throw new InvalidOperationException("That request is no longer pending.");
                    }
                }
            }
        }

        /// <summary>
        /// Approves one pending request: one borrow record is written per requested
        /// copy, then the request row is linked to the first of those records.
        /// </summary>
        public List<BorrowRecord> Approve(
            BorrowRequestRow request,
            int processedBy,
            LibraryCatalog catalog,
            AccountDirectory directory)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            if (!request.IsPending)
            {
                throw new InvalidOperationException("That request has already been processed.");
            }

            AccountRow? profile = directory.LoadProfile(request.AccountId);

            if (profile == null)
            {
                throw new InvalidOperationException("The student profile for this request could not be found.");
            }

            Book? book = catalog.FindBookById(request.BookId);

            if (book == null)
            {
                throw new InvalidOperationException("The requested book is no longer in the catalog.");
            }

            int copies = request.Copies < 1 ? 1 : request.Copies;
            int available = catalog.AvailableCopyCount(book);

            if (copies > available)
            {
                throw new InvalidOperationException(
                    "ONLY " + available.ToString(CultureInfo.InvariantCulture) +
                    " COPY(IES) OF \"" + book.Title.ToUpperInvariant() + "\" ARE STILL AVAILABLE, BUT " +
                    copies.ToString(CultureInfo.InvariantCulture) + " WERE REQUESTED.");
            }

            string idNumber = Clean(profile.SchoolId);
            if (idNumber.Length == 0)
            {
                idNumber = "ACCOUNT-" + request.AccountId.ToString(CultureInfo.InvariantCulture);
            }

            Borrower borrower = new Borrower(
                profile.Name,
                idNumber,
                Clean(profile.Program),
                Clean(profile.YearLevel),
                Clean(profile.Institute));

            // The account already knows its role and contact number, so the borrower
            // row keeps them and the EDIT BORROW RECORD pop up can require the right
            // details later.
            borrower.AssignAccountId(request.AccountId);
            borrower.ApplyRoleDetails(profile.Role, profile.Office, profile.ContactNumber);

            List<BorrowRecord> records = new List<BorrowRecord>();

            for (int copy = 0; copy < copies; copy++)
            {
                records.Add(catalog.RecordBorrow(book, borrower, DateTime.Now));
            }

            BorrowRecord record = records[0];

            using (MySqlConnection connection = _database.CreateConnection())
            {
                connection.Open();

                const string sql =
                    "UPDATE `borrow_requests_table` " +
                    "SET `status_id` = (SELECT `request_status_id` FROM `request_status_table` WHERE `status_code` = 'APPROVED'), " +
                    "    `processed_by` = @processedBy, `processed_date` = NOW(), `borrow_id` = @borrowId " +
                    "WHERE `request_id` = @requestId " +
                    "  AND `status_id` = (SELECT `request_status_id` FROM `request_status_table` WHERE `status_code` = 'PENDING');";

                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@processedBy", processedBy);
                    command.Parameters.AddWithValue("@borrowId", record.BorrowId);
                    command.Parameters.AddWithValue("@requestId", request.RequestId);

                    if (command.ExecuteNonQuery() == 0)
                    {
                        throw new InvalidOperationException("That request was already processed by another librarian.");
                    }
                }
            }

            return records;
        }

        /// <summary>
        /// Marks an approved request as CLAIMED once the student has collected
        /// the books in person. The borrow records created by Approve stay
        /// untouched; this only records who handed the books over and when, so
        /// the student's MY REQUESTS tab shows CLAIMED.
        /// </summary>
        public void Claim(BorrowRequestRow request, int claimedBy)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!request.IsApproved)
            {
                throw new InvalidOperationException(
                    request.IsClaimed
                        ? "That request has already been claimed."
                        : "That request must be approved before it can be claimed.");
            }

            using (MySqlConnection connection = _database.CreateConnection())
            {
                connection.Open();
                EnsureSchema(connection);

                const string sql = "UPDATE `borrow_requests_table` " +
                                   "SET `status_id` = (SELECT `request_status_id` FROM `request_status_table` WHERE `status_code` = 'CLAIMED'), " +
                                   "    `claimed_by` = @claimedBy, `claim_date` = NOW() " +
                                   "WHERE `request_id` = @requestId " +
                                   "  AND `status_id` = (SELECT `request_status_id` FROM `request_status_table` WHERE `status_code` = 'APPROVED');";

                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@claimedBy", claimedBy);
                    command.Parameters.AddWithValue("@requestId", request.RequestId);

                    if (command.ExecuteNonQuery() == 0)
                    {
                        throw new InvalidOperationException("That request is no longer waiting to be claimed.");
                    }
                }
            }
        }

        /// <summary>
        /// Adds the COPIES_REQUESTED column to databases created before the patron
        /// could choose how many copies to request.
        /// </summary>
        private static void EnsureSchema(MySqlConnection connection)
        {
            if (_schemaReady)
            {
                return;
            }

            try
            {
                EnsureColumn(connection, "copies_requested",
                    "ADD COLUMN `copies_requested` INT(11) NOT NULL DEFAULT 1 AFTER `book_id`");

                // CLAIM: the librarian confirms the face to face hand over of an
                // approved request. Both columns are added to older databases here,
                // so an existing install keeps working without a manual migration.
                EnsureColumn(connection, "claimed_by",
                    "ADD COLUMN `claimed_by` INT(11) NULL AFTER `processed_date`");
                EnsureColumn(connection, "claim_date",
                    "ADD COLUMN `claim_date` DATETIME NULL AFTER `claimed_by`");
            }
            catch (MySqlException)
            {
                // A database user without ALTER rights keeps working with one copy;
                // the SELECT still falls back to 1 through COALESCE.
            }

            _schemaReady = true;
        }

        private static void EnsureColumn(MySqlConnection connection, string columnName, string alterClause)
        {
            const string checkSql =
                "SELECT COUNT(*) FROM information_schema.columns " +
                "WHERE table_schema = DATABASE() AND table_name = 'borrow_requests_table' " +
                "AND column_name = @column;";

            using (MySqlCommand check = new MySqlCommand(checkSql, connection))
            {
                check.Parameters.AddWithValue("@column", columnName);

                if (Convert.ToInt32(check.ExecuteScalar(), CultureInfo.InvariantCulture) > 0)
                {
                    return;
                }
            }

            using (MySqlCommand alter = new MySqlCommand(
                "ALTER TABLE `borrow_requests_table` " + alterClause + ";", connection))
            {
                alter.ExecuteNonQuery();
            }
        }

        private static List<BorrowRequestRow> ReadRows(MySqlCommand command)
        {
            List<BorrowRequestRow> rows = new List<BorrowRequestRow>();

            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    rows.Add(new BorrowRequestRow(
                        reader.GetInt32(0),
                        reader.GetInt32(1),
                        reader.GetInt32(2),
                        Text(reader, 3),
                        Text(reader, 4),
                        reader.IsDBNull(5) ? 0 : Convert.ToInt32(reader.GetValue(5), CultureInfo.InvariantCulture),
                        Text(reader, 6),
                        Text(reader, 7),
                        Text(reader, 8),
                        Text(reader, 9),
                        Text(reader, 10),
                        reader.GetDateTime(11),
                        Text(reader, 12, "PENDING"),
                        reader.IsDBNull(13) ? 0 : Convert.ToInt32(reader.GetValue(13), CultureInfo.InvariantCulture),
                        reader.IsDBNull(14) ? null : reader.GetDateTime(14),
                        reader.IsDBNull(15) ? 0 : Convert.ToInt32(reader.GetValue(15), CultureInfo.InvariantCulture),
                        reader.IsDBNull(16) ? 1 : Convert.ToInt32(reader.GetValue(16), CultureInfo.InvariantCulture),
                        reader.IsDBNull(17) ? 0 : Convert.ToInt32(reader.GetValue(17), CultureInfo.InvariantCulture),
                        reader.IsDBNull(18) ? null : reader.GetDateTime(18)));
                }
            }

            return rows;
        }

        private static string Text(MySqlDataReader reader, int index, string fallback = "")
        {
            if (reader.IsDBNull(index))
            {
                return fallback;
            }

            return Convert.ToString(reader.GetValue(index), CultureInfo.InvariantCulture) ?? fallback;
        }

        private static string Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value) || value == "—" ? string.Empty : value.Trim();
        }
    }
}
