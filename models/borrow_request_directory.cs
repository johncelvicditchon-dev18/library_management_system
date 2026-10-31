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
            "       r.`request_date`, r.`status`, COALESCE(r.`note`, ''), " +
            "       COALESCE(r.`processed_by`, 0), r.`processed_date`, COALESCE(r.`borrow_id`, 0) " +
            "FROM `borrow_requests_table` r " +
            "JOIN `account_table` a ON a.`account_id` = r.`account_id` " +
            "LEFT JOIN `books_table` b ON b.`book_id` = r.`book_id` " +
            "LEFT JOIN `student_info_table` s ON s.`account_id` = r.`account_id` " +
            "LEFT JOIN `program_table` p ON p.`program_id` = s.`program_id` " +
            "LEFT JOIN `institute_table` i ON i.`institute_id` = s.`institute_id` ";

        public int Create(int accountId, Book book, string note = "")
        {
            if (accountId <= 0)
            {
                throw new ArgumentException("A signed in account is required.", nameof(accountId));
            }

            if (book == null)
            {
                throw new ArgumentNullException(nameof(book));
            }

            if (!book.IsAvailable || book.Quantity <= 0 || book.BookId <= 0)
            {
                throw new InvalidOperationException("That book has no copy available to request.");
            }

            using (MySqlConnection connection = _database.CreateConnection())
            {
                connection.Open();

                // Re-check the shelf in MySQL, not just the grid the student is
                // looking at, so a copy borrowed a second ago is not requested.
                const string availabilitySql =
                    "SELECT CAST(b.`copy` AS UNSIGNED), " +
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

                        int copies = reader.IsDBNull(0)
                            ? 0
                            : Convert.ToInt32(reader.GetValue(0), CultureInfo.InvariantCulture);
                        int openLoans = reader.IsDBNull(1)
                            ? 0
                            : Convert.ToInt32(reader.GetValue(1), CultureInfo.InvariantCulture);

                        if (copies - openLoans <= 0)
                        {
                            throw new InvalidOperationException("That book has no copy available to request.");
                        }
                    }
                }

                const string activeLoanSql =
                    "SELECT COUNT(*) FROM `borrow_transactions_table` t " +
                    "JOIN `borrowers_table` b ON b.`borrower_id` = t.`borrower_id` " +
                    "JOIN `student_info_table` s ON s.`school_id` = b.`id_number` " +
                    "WHERE s.`account_id` = @accountId AND t.`book_id` = @bookId AND t.`date_return` IS NULL;";

                using (MySqlCommand activeLoan = new MySqlCommand(activeLoanSql, connection))
                {
                    activeLoan.Parameters.AddWithValue("@accountId", accountId);
                    activeLoan.Parameters.AddWithValue("@bookId", book.BookId);

                    if (Convert.ToInt64(activeLoan.ExecuteScalar(), CultureInfo.InvariantCulture) > 0)
                    {
                        throw new InvalidOperationException("You already have that book on loan.");
                    }
                }

                const string pendingSql = "SELECT COUNT(*) FROM `borrow_requests_table` " +
                                          "WHERE `account_id` = @accountId AND `book_id` = @bookId AND `status` = 'PENDING';";

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

                const string sql = "INSERT INTO `borrow_requests_table` (`account_id`, `book_id`, `request_date`, `status`, `note`) " +
                                   "VALUES (@accountId, @bookId, NOW(), 'PENDING', @note);";

                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@accountId", accountId);
                    command.Parameters.AddWithValue("@bookId", book.BookId);
                    command.Parameters.AddWithValue("@note", string.IsNullOrWhiteSpace(note) ? (object)DBNull.Value : note.Trim());
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

                using (MySqlCommand command = new MySqlCommand(
                    RequestSelect + "WHERE r.`status` = 'PENDING' ORDER BY r.`request_date` ASC, r.`request_id` ASC;", connection))
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

                const string sql = "UPDATE `borrow_requests_table` SET `status` = 'CANCELLED', `processed_date` = NOW() " +
                                   "WHERE `request_id` = @requestId AND `account_id` = @accountId AND `status` = 'PENDING';";

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

        public void Reject(BorrowRequestRow request, int processedBy, string note = "")
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            using (MySqlConnection connection = _database.CreateConnection())
            {
                connection.Open();

                const string sql = "UPDATE `borrow_requests_table` SET `status` = 'REJECTED', `processed_by` = @processedBy, " +
                                   "`processed_date` = NOW(), `note` = @note " +
                                   "WHERE `request_id` = @requestId AND `status` = 'PENDING';";

                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@processedBy", processedBy);
                    command.Parameters.AddWithValue("@note", string.IsNullOrWhiteSpace(note) ? (object)DBNull.Value : note.Trim());
                    command.Parameters.AddWithValue("@requestId", request.RequestId);

                    if (command.ExecuteNonQuery() == 0)
                    {
                        throw new InvalidOperationException("That request is no longer pending.");
                    }
                }
            }
        }

        /// <summary>
        /// Approves one pending request: the copy is written to the normal borrow
        /// tables first, then the request row is linked to the new borrow_id.
        /// </summary>
        public BorrowRecord Approve(
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

            BorrowRecord record = catalog.RecordBorrow(book, borrower, DateTime.Now);

            using (MySqlConnection connection = _database.CreateConnection())
            {
                connection.Open();

                const string sql = "UPDATE `borrow_requests_table` SET `status` = 'APPROVED', `processed_by` = @processedBy, " +
                                   "`processed_date` = NOW(), `borrow_id` = @borrowId " +
                                   "WHERE `request_id` = @requestId AND `status` = 'PENDING';";

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

            return record;
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
                        Text(reader, 13),
                        reader.IsDBNull(14) ? 0 : Convert.ToInt32(reader.GetValue(14), CultureInfo.InvariantCulture),
                        reader.IsDBNull(15) ? null : reader.GetDateTime(15),
                        reader.IsDBNull(16) ? 0 : Convert.ToInt32(reader.GetValue(16), CultureInfo.InvariantCulture)));
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
