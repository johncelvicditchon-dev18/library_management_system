using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using MySql.Data.MySqlClient;
using oop_project.Database;

namespace oop_project.Models
{
    // ABSTRACTION: the dashboard asks this catalog for books, borrowers, loans and
    // totals. The three MySQL tables (books_table, borrowers_table,
    // borrow_transactions_table) and the in-session fallback stay hidden behind it.
    public class LibraryCatalog
    {
        private readonly IDatabaseConnection _database = new MySqlDatabaseConnection();
        private readonly List<Book> _books = new List<Book>();
        private readonly List<BorrowRecord> _borrowRecords = new List<BorrowRecord>();

        public IReadOnlyList<Book> Books
        {
            get { return _books; }
        }

        public IReadOnlyList<BorrowRecord> BorrowRecords
        {
            get { return _borrowRecords; }
        }

        public bool IsConnected { get; private set; }

        // Set when a write could not be saved, so the dashboard can tell the user
        // that the change is only kept until the app closes.
        public string? LastWarning { get; private set; }

        public int TotalBooks
        {
            get { return _books.Count; }
        }

        public int AvailableBooks
        {
            get
            {
                int count = 0;
                foreach (Book book in _books)
                {
                    if (book.IsAvailable)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        /// <summary>
        /// Total number of copies that are still on the shelf. Unlike
        /// <see cref="AvailableBooks"/> (which counts titles) this adds up every
        /// available copy of every title.
        /// </summary>
        public int AvailableCopies
        {
            get
            {
                int count = 0;
                foreach (Book book in _books)
                {
                    count += AvailableCopyCount(book);
                }

                return count;
            }
        }

        public int BorrowedBooks
        {
            get { return _books.Count - AvailableBooks; }
        }

        public int ActiveLoans
        {
            get { return CountRecords("BORROWED"); }
        }

        public int ReturnedLoans
        {
            get { return CountRecords("RETURNED"); }
        }

        public int BorrowerCount
        {
            get
            {
                List<int> seen = new List<int>();
                foreach (BorrowRecord record in _borrowRecords)
                {
                    if (record.BorrowerId > 0 && !seen.Contains(record.BorrowerId))
                    {
                        seen.Add(record.BorrowerId);
                    }
                }

                return seen.Count;
            }
        }

        public string ConnectionText
        {
            get { return IsConnected ? "MYSQL CONNECTED" : "SESSION ONLY • MYSQL OFFLINE"; }
        }

        public void ClearWarning()
        {
            LastWarning = null;
        }

        /// <summary>
        /// Reads books_table and the borrow transactions, and works out which titles
        /// are on loan. If MySQL is not reachable the dashboard keeps working with
        /// the rows added during the session.
        /// </summary>
        public void Load()
        {
            _books.Clear();
            _borrowRecords.Clear();
            IsConnected = false;
            LastWarning = null;

            try
            {
                using (MySqlConnection connection = _database.CreateConnection())
                {
                    connection.Open();
                    LoadBooks(connection);
                    LoadBorrowRecords(connection);
                    MarkUnavailableBooks();
                    IsConnected = true;
                }
            }
            catch (MySqlException ex)
            {
                LastWarning = "MYSQL IS NOT AVAILABLE. BOOKS AND BORROWS ARE KEPT FOR THIS SESSION ONLY.\n" + ex.Message;
            }

            // The stored day counter is refreshed on every load, so the column is
            // correct even when the app is opened days after the book was taken.
            SyncDaysBorrowed();
        }

        /// <summary>
        /// Copies each borrower's whole-day count into borrowers_table.day_before_borrow.
        /// A day is added only after 24 full hours, so a book borrowed at 1:15 AM
        /// shows 1 day at 1:15 AM the next morning.
        /// </summary>
        public void SyncDaysBorrowed()
        {
            if (!IsConnected)
            {
                return;
            }

            Dictionary<int, int> days = new Dictionary<int, int>();

            foreach (BorrowRecord record in _borrowRecords)
            {
                if (record.BorrowerId <= 0)
                {
                    continue;
                }

                int stored;
                if (!days.TryGetValue(record.BorrowerId, out stored) || record.DaysBorrowed > stored)
                {
                    days[record.BorrowerId] = record.DaysBorrowed;
                }
            }

            if (days.Count == 0)
            {
                return;
            }

            try
            {
                using (MySqlConnection connection = _database.CreateConnection())
                {
                    connection.Open();

                    const string sql = "UPDATE `borrowers_table` SET `day_before_borrow` = @days " +
                                       "WHERE `borrower_id` = @borrowerId AND `day_before_borrow` <> @days;";

                    foreach (KeyValuePair<int, int> entry in days)
                    {
                        using (MySqlCommand command = new MySqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@days", entry.Value);
                            command.Parameters.AddWithValue("@borrowerId", entry.Key);
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                GoOffline(ex);
            }
        }

        public Book? FindBookById(int bookId)
        {
            foreach (Book book in _books)
            {
                if (book.BookId == bookId && book.BookId > 0)
                {
                    return book;
                }
            }

            return null;
        }

        /// <summary>
        /// Number of copies of one title that are still out. The AVAILABLE COPY
        /// column is books_table.copy minus this number.
        /// </summary>
        public int OpenLoanCount(int bookId)
        {
            int count = 0;

            foreach (BorrowRecord record in _borrowRecords)
            {
                if (!record.DateReturned.HasValue && record.BookId == bookId && bookId > 0)
                {
                    count++;
                }
            }

            return count;
        }

        public int AvailableCopyCount(Book book)
        {
            if (book == null)
            {
                throw new ArgumentNullException(nameof(book));
            }

            int available = book.Quantity - OpenLoanCount(book.BookId);
            return available < 0 ? 0 : available;
        }

        public bool HasBorrowerWithIdNumber(string idNumber)
        {
            foreach (BorrowRecord record in _borrowRecords)
            {
                if (string.Equals(record.BorrowerIdNumber, idNumber, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public void AddBook(Book book)
        {
            if (book == null)
            {
                throw new ArgumentNullException(nameof(book));
            }

            if (IsConnected)
            {
                try
                {
                    using (MySqlConnection connection = _database.CreateConnection())
                    {
                        connection.Open();

                        const string sql = "INSERT INTO `books_table` (`book_title`, `author`, `year_published`, `copy`, `date_added`, `book_image`) " +
                                           "VALUES (@title, @author, @year, @quantity, @dateAdded, @image);";

                        using (MySqlCommand command = new MySqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@title", book.Title);
                            command.Parameters.AddWithValue("@author", book.Author);
                            command.Parameters.AddWithValue("@year", book.Year);
                            command.Parameters.AddWithValue("@quantity", book.QuantityText);
                            command.Parameters.AddWithValue("@dateAdded", book.DateAdded);
                            command.Parameters.AddWithValue("@image", (object?)book.CoverImage ?? DBNull.Value);
                            command.ExecuteNonQuery();
                            book.AssignBookId((int)command.LastInsertedId);
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    GoOffline(ex);
                }
            }

            _books.Add(book);
        }

        /// <summary>
        /// UPDATE books_table for the row the EDIT pop up changed. The values are
        /// validated by the Book model before the SQL runs and the in-memory row is
        /// only rewritten after the statement succeeds.
        /// </summary>
        public void UpdateBook(Book book, string title, string author, int year, int quantity, byte[]? coverImage = null)
        {
            if (book == null)
            {
                throw new ArgumentNullException(nameof(book));
            }

            // A throw away copy validates the edited values with the same rules.
            Book validated = new Book(title, author, year, quantity, coverImage);

            if (IsConnected && book.BookId > 0)
            {
                using (MySqlConnection connection = _database.CreateConnection())
                {
                    connection.Open();

                    const string sql = "UPDATE `books_table` SET `book_title` = @title, `author` = @author, " +
                                       "`year_published` = @year, `copy` = @quantity, `book_image` = @image " +
                                       "WHERE `book_id` = @bookId;";

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@title", validated.Title);
                        command.Parameters.AddWithValue("@author", validated.Author);
                        command.Parameters.AddWithValue("@year", validated.Year);
                        command.Parameters.AddWithValue("@quantity", validated.QuantityText);
                        command.Parameters.AddWithValue("@image", (object?)validated.CoverImage ?? DBNull.Value);
                        command.Parameters.AddWithValue("@bookId", book.BookId);
                        command.ExecuteNonQuery();
                    }
                }
            }

            book.ApplyChanges(validated.Title, validated.Author, validated.Year, validated.Quantity, validated.CoverImage);
        }

        /// <summary>
        /// DELETE one row from books_table. A title that still has borrow rows is
        /// refused, because borrow_transactions_table points at it with a foreign key.
        /// </summary>
        public void DeleteBook(Book book)
        {
            if (book == null)
            {
                throw new ArgumentNullException(nameof(book));
            }

            foreach (BorrowRecord record in _borrowRecords)
            {
                if (record.BookId == book.BookId && book.BookId > 0)
                {
                    throw new InvalidOperationException(
                        "\"" + book.Title + "\" still has borrow records. Delete those records first.");
                }
            }

            if (IsConnected && book.BookId > 0)
            {
                using (MySqlConnection connection = _database.CreateConnection())
                {
                    connection.Open();

                    const string sql = "DELETE FROM `books_table` WHERE `book_id` = @bookId;";

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@bookId", book.BookId);
                        command.ExecuteNonQuery();
                    }
                }
            }

            _books.Remove(book);
        }

        public BorrowRecord RecordBorrow(Book book, Borrower borrower, DateTime dateBorrowed)
        {
            if (book == null)
            {
                throw new ArgumentNullException(nameof(book));
            }

            if (borrower == null)
            {
                throw new ArgumentNullException(nameof(borrower));
            }

            if (book.Quantity <= 0 || OpenLoanCount(book.BookId) >= book.Quantity)
            {
                throw new InvalidOperationException("Every copy of that book is already borrowed.");
            }

            int borrowerId = borrower.BorrowerId;

            if (IsConnected)
            {
                try
                {
                    using (MySqlConnection connection = _database.CreateConnection())
                    {
                        connection.Open();
                        borrowerId = FindOrCreateBorrower(connection, borrower);

                        BorrowRecord stored = new BorrowRecord(
                            0,
                            book.BookId,
                            book.Title,
                            book.Author,
                            book.Year,
                            borrowerId,
                            borrower.Name,
                            borrower.IdNumber,
                            borrower.Program,
                            borrower.YearLevel,
                            borrower.Institute,
                            dateBorrowed,
                            null);

                        const string sql = "INSERT INTO `borrow_transactions_table` (`borrower_id`, `book_id`, `date_borrow`, `date_return`) " +
                                           "VALUES (@borrowerId, @bookId, @dateBorrow, NULL);";

                        using (MySqlCommand command = new MySqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@borrowerId", borrowerId);
                            command.Parameters.AddWithValue("@bookId", book.BookId);
                            command.Parameters.AddWithValue("@dateBorrow", dateBorrowed);
                            command.ExecuteNonQuery();
                            stored.AssignBorrowId((int)command.LastInsertedId);
                        }

                        borrower.AssignBorrowerId(borrowerId);
                        _borrowRecords.Insert(0, stored);
                        RefreshAvailability();
                        return stored;
                    }
                }
                catch (MySqlException ex)
                {
                    GoOffline(ex);
                }
            }

            BorrowRecord record = new BorrowRecord(
                book.BookId,
                book.Title,
                book.Author,
                book.Year,
                borrower,
                dateBorrowed);

            _borrowRecords.Insert(0, record);
            RefreshAvailability();
            return record;
        }

        public void MarkReturned(BorrowRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            if (record.DateReturned.HasValue)
            {
                return;
            }

            record.MarkReturned(DateTime.Now);

            Book? book = FindBookById(record.BookId);
            if (book != null)
            {
                book.MarkAvailable();
            }

            if (!IsConnected)
            {
                return;
            }

            try
            {
                using (MySqlConnection connection = _database.CreateConnection())
                {
                    connection.Open();

                    const string sql = "UPDATE `borrow_transactions_table` SET `date_return` = @dateReturn WHERE `borrow_id` = @borrowId;";

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@dateReturn", record.DateReturned ?? DateTime.Now);
                        command.Parameters.AddWithValue("@borrowId", record.BorrowId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                GoOffline(ex);
            }
        }

        /// <summary>
        /// UPDATE the borrower row and the loan row behind the EDIT pop up of the
        /// ATTENDANCE tab. A loan that is set back to BORROWED clears date_return.
        /// </summary>
        public void UpdateBorrowRecord(
            BorrowRecord record,
            Book book,
            Borrower borrower,
            DateTime dateBorrowed,
            DateTime? dateReturned)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            if (book == null)
            {
                throw new ArgumentException("Choose the book for this loan.");
            }

            if (borrower == null)
            {
                throw new ArgumentNullException(nameof(borrower));
            }

            if (dateReturned.HasValue && dateReturned.Value < dateBorrowed)
            {
                throw new ArgumentException("The return date cannot be earlier than the borrow date.");
            }

            if (IsConnected && record.BorrowId > 0)
            {
                using (MySqlConnection connection = _database.CreateConnection())
                {
                    connection.Open();

                    if (record.BorrowerId > 0)
                    {
                        const string borrowerSql =
                            "UPDATE `borrowers_table` SET `borrower_name` = @name, `program` = @program, " +
                            "`year_level` = @yearLevel, `institute` = @institute, `id_number` = @idNumber " +
                            "WHERE `borrower_id` = @borrowerId;";

                        using (MySqlCommand borrowerCommand = new MySqlCommand(borrowerSql, connection))
                        {
                            borrowerCommand.Parameters.AddWithValue("@name", borrower.Name);
                            borrowerCommand.Parameters.AddWithValue("@program", NullIfEmpty(borrower.Program));
                            borrowerCommand.Parameters.AddWithValue("@yearLevel", NullIfEmpty(borrower.YearLevel));
                            borrowerCommand.Parameters.AddWithValue("@institute", NullIfEmpty(borrower.Institute));
                            borrowerCommand.Parameters.AddWithValue("@idNumber", borrower.IdNumber);
                            borrowerCommand.Parameters.AddWithValue("@borrowerId", record.BorrowerId);
                            borrowerCommand.ExecuteNonQuery();
                        }
                    }

                    const string loanSql =
                        "UPDATE `borrow_transactions_table` SET `book_id` = @bookId, `date_borrow` = @dateBorrow, " +
                        "`date_return` = @dateReturn WHERE `borrow_id` = @borrowId;";

                    using (MySqlCommand loanCommand = new MySqlCommand(loanSql, connection))
                    {
                        loanCommand.Parameters.AddWithValue("@bookId", book.BookId);
                        loanCommand.Parameters.AddWithValue("@dateBorrow", dateBorrowed);
                        loanCommand.Parameters.AddWithValue(
                            "@dateReturn",
                            dateReturned.HasValue ? (object)dateReturned.Value : DBNull.Value);
                        loanCommand.Parameters.AddWithValue("@borrowId", record.BorrowId);
                        loanCommand.ExecuteNonQuery();
                    }
                }
            }

            record.ApplyChanges(
                book.BookId,
                book.Title,
                book.Author,
                book.Year,
                borrower.Name,
                borrower.IdNumber,
                borrower.Program,
                borrower.YearLevel,
                borrower.Institute,
                dateBorrowed,
                dateReturned);

            RefreshAvailability();
        }

        /// <summary>
        /// DELETE the loan row of the ATTENDANCE tab. The borrower row is kept,
        /// because another loan may still point at it.
        /// </summary>
        public void DeleteBorrowRecord(BorrowRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            if (IsConnected && record.BorrowId > 0)
            {
                using (MySqlConnection connection = _database.CreateConnection())
                {
                    connection.Open();

                    const string sql = "DELETE FROM `borrow_transactions_table` WHERE `borrow_id` = @borrowId;";

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@borrowId", record.BorrowId);
                        command.ExecuteNonQuery();
                    }
                }
            }

            _borrowRecords.Remove(record);
            RefreshAvailability();
        }

        /// <summary>
        /// Works the AVAILABLE / BORROWED column out again from the open loans, so a
        /// deleted or edited loan is shown on the shelf straight away, even offline.
        /// </summary>
        public void RefreshAvailability()
        {
            foreach (Book book in _books)
            {
                book.MarkAvailable();
            }

            MarkUnavailableBooks();
        }

        public string BuildReportText(string generatedBy, DateTime generatedOn)
        {
            StringBuilder report = new StringBuilder();

            report.AppendLine("TCGC LEARNING RESOURCE CENTER");
            report.AppendLine("LIBRARY MANAGEMENT SYSTEM — GENERATED REPORT");
            report.AppendLine("==============================================================");
            report.AppendLine("GENERATED BY : " + (string.IsNullOrWhiteSpace(generatedBy) ? "LIBRARY STAFF" : generatedBy.ToUpperInvariant()));
            report.AppendLine("GENERATED ON : " + generatedOn.ToString("MMMM dd, yyyy hh:mm tt", CultureInfo.InvariantCulture).ToUpperInvariant());
            report.AppendLine("DATA SOURCE  : " + ConnectionText);
            report.AppendLine();
            report.AppendLine("SUMMARY");
            report.AppendLine("--------------------------------------------------------------");
            report.AppendLine("TOTAL BOOK TITLES        : " + TotalBooks);
            report.AppendLine("AVAILABLE ON THE SHELF   : " + AvailableBooks);
            report.AppendLine("CURRENTLY BORROWED       : " + BorrowedBooks);
            report.AppendLine("BORROW RECORDS ON FILE   : " + _borrowRecords.Count);
            report.AppendLine("REGISTERED BORROWERS     : " + BorrowerCount);
            report.AppendLine("ACTIVE LOANS             : " + ActiveLoans);
            report.AppendLine("RETURNED LOANS           : " + ReturnedLoans);
            report.AppendLine();
            report.AppendLine("BOOK CATALOG (books_table)");
            report.AppendLine("--------------------------------------------------------------");

            if (_books.Count == 0)
            {
                report.AppendLine("NO BOOKS HAVE BEEN ADDED YET.");
            }
            else
            {
                List<Book> sorted = new List<Book>(_books);
                sorted.Sort(delegate (Book left, Book right)
                {
                    return string.Compare(left.Title, right.Title, StringComparison.OrdinalIgnoreCase);
                });

                int index = 1;
                foreach (Book book in sorted)
                {
                    report.AppendLine(index.ToString(CultureInfo.InvariantCulture).PadLeft(2) + ". " + book.Title);
                    report.AppendLine("    AUTHOR : " + book.Author + "     YEAR : " + book.YearText +
                                      "     QUANTITY : " + book.QuantityText);
                    report.AppendLine("    DATE ADDED : " + book.DateAdded + "     STATUS : " + book.Status);
                    index++;
                }
            }

            report.AppendLine();
            report.AppendLine("BORROW TRANSACTIONS (borrow_transactions_table)");
            report.AppendLine("--------------------------------------------------------------");

            if (_borrowRecords.Count == 0)
            {
                report.AppendLine("NO BOOK HAS BEEN BORROWED YET.");
            }
            else
            {
                List<BorrowRecord> sorted = new List<BorrowRecord>(_borrowRecords);
                sorted.Sort(delegate (BorrowRecord left, BorrowRecord right)
                {
                    return right.DateBorrowed.CompareTo(left.DateBorrowed);
                });

                int index = 1;
                foreach (BorrowRecord record in sorted)
                {
                    report.AppendLine(index.ToString(CultureInfo.InvariantCulture).PadLeft(2) + ". " + record.BookTitle +
                                      "  (" + record.Author + ")");
                    report.AppendLine("    BORROWED BY : " + record.BorrowerName + "     ID NUMBER : " + record.BorrowerIdNumber);
                    report.AppendLine("    BORROWED : " + record.DateBorrowedStampText +
                                      "     DAYS BORROWED : " + record.DaysBorrowedText +
                                      "     RETURNED : " + record.DateReturnedText + "     STATUS : " + record.Status);
                    index++;
                }
            }

            report.AppendLine();
            report.AppendLine("==============================================================");
            report.AppendLine("END OF REPORT");

            return report.ToString();
        }

        private int CountRecords(string status)
        {
            int count = 0;
            foreach (BorrowRecord record in _borrowRecords)
            {
                if (string.Equals(record.Status, status, StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// The dashboard never changes the schema of the existing tables: the DDL
        /// lives in database/libsys_database.sql and is run by the developer.
        /// </summary>
        private void LoadBooks(MySqlConnection connection)
        {
            const string sql = "SELECT `book_id`, `book_title`, `author`, `year_published`, `copy`, `date_added`, `book_image` " +
                               "FROM `books_table` ORDER BY `book_title`;";

            using (MySqlCommand command = new MySqlCommand(sql, connection))
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    try
                    {
                        _books.Add(new Book(
                            reader.GetInt32(0),
                            reader.GetString(1),
                            reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            reader.IsDBNull(3) ? 0 : Convert.ToInt32(reader.GetValue(3), CultureInfo.InvariantCulture),
                            reader.IsDBNull(4) ? 0 : ParseQuantity(Convert.ToString(reader.GetValue(4), CultureInfo.InvariantCulture)),
                            reader.IsDBNull(5) ? string.Empty : Convert.ToString(reader.GetValue(5), CultureInfo.InvariantCulture) ?? string.Empty,
                            true,
                            reader.IsDBNull(6) ? null : (byte[])reader.GetValue(6)));
                    }
                    catch (ArgumentException)
                    {
                        // A row that breaks the model rules is skipped instead of crashing the dashboard.
                    }
                }
            }
        }

        private static int ParseQuantity(string? text)
        {
            int quantity;

            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out quantity)
                ? quantity
                : 0;
        }

        private void LoadBorrowRecords(MySqlConnection connection)
        {
            const string sql =
                "SELECT t.`borrow_id`, t.`book_id`, bk.`book_title`, bk.`author`, bk.`year_published`, " +
                "       b.`borrower_id`, b.`borrower_name`, b.`id_number`, b.`program`, b.`year_level`, b.`institute`, " +
                "       t.`date_borrow`, t.`date_return` " +
                "FROM `borrow_transactions_table` t " +
                "JOIN `borrowers_table` b ON b.`borrower_id` = t.`borrower_id` " +
                "LEFT JOIN `books_table` bk ON bk.`book_id` = t.`book_id` " +
                "ORDER BY t.`date_borrow` DESC, t.`borrow_id` DESC;";

            using (MySqlCommand command = new MySqlCommand(sql, connection))
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    try
                    {
                        int bookId = reader.GetInt32(1);
                        string title = reader.IsDBNull(2) ? "(DELETED BOOK #" + bookId + ")" : reader.GetString(2);
                        string author = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                        int bookYear = reader.IsDBNull(4)
                            ? 0
                            : Convert.ToInt32(reader.GetValue(4), CultureInfo.InvariantCulture);

                        _borrowRecords.Add(new BorrowRecord(
                            reader.GetInt32(0),
                            bookId,
                            title,
                            author,
                            bookYear,
                            reader.GetInt32(5),
                            reader.GetString(6),
                            reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                            reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                            reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                            reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                            reader.GetDateTime(11),
                            reader.IsDBNull(12) ? null : reader.GetDateTime(12)));
                    }
                    catch (ArgumentException)
                    {
                        // Broken rows are ignored so one bad record cannot hide the whole list.
                    }
                }
            }
        }

        private void MarkUnavailableBooks()
        {
            // A title with several copies stays AVAILABLE while at least one copy
            // is still on the shelf. Only the last open loan marks it borrowed.
            foreach (Book book in _books)
            {
                if (OpenLoanCount(book.BookId) >= book.Quantity)
                {
                    book.MarkBorrowed();
                }
                else
                {
                    book.MarkAvailable();
                }
            }
        }

        private static int FindOrCreateBorrower(MySqlConnection connection, Borrower borrower)
        {
            const string findSql = "SELECT `borrower_id`, `borrower_name` FROM `borrowers_table` WHERE `id_number` = @idNumber LIMIT 1;";

            using (MySqlCommand find = new MySqlCommand(findSql, connection))
            {
                find.Parameters.AddWithValue("@idNumber", borrower.IdNumber);

                using (MySqlDataReader reader = find.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int existingId = reader.GetInt32(0);
                        reader.Close();

                        // An existing ID number keeps its borrower row; the details that
                        // the pop up filled in are refreshed, empty boxes change nothing.
                        const string updateSql =
                            "UPDATE `borrowers_table` SET " +
                            "  `borrower_name` = @name, " +
                            "  `program` = COALESCE(NULLIF(@program, ''), `program`), " +
                            "  `year_level` = COALESCE(NULLIF(@yearLevel, ''), `year_level`), " +
                            "  `institute` = COALESCE(NULLIF(@institute, ''), `institute`) " +
                            "WHERE `borrower_id` = @id;";

                        using (MySqlCommand update = new MySqlCommand(updateSql, connection))
                        {
                            update.Parameters.AddWithValue("@name", borrower.Name);
                            update.Parameters.AddWithValue("@program", borrower.Program);
                            update.Parameters.AddWithValue("@yearLevel", borrower.YearLevel);
                            update.Parameters.AddWithValue("@institute", borrower.Institute);
                            update.Parameters.AddWithValue("@id", existingId);
                            update.ExecuteNonQuery();
                        }

                        return existingId;
                    }
                }
            }

            const string insertSql = "INSERT INTO `borrowers_table` (`borrower_name`, `program`, `year_level`, `institute`, `id_number`, `day_before_borrow`) " +
                                     "VALUES (@name, @program, @yearLevel, @institute, @idNumber, @dayBeforeBorrow);";

            using (MySqlCommand insert = new MySqlCommand(insertSql, connection))
            {
                insert.Parameters.AddWithValue("@name", borrower.Name);
                insert.Parameters.AddWithValue("@program", NullIfEmpty(borrower.Program));
                insert.Parameters.AddWithValue("@yearLevel", NullIfEmpty(borrower.YearLevel));
                insert.Parameters.AddWithValue("@institute", NullIfEmpty(borrower.Institute));
                insert.Parameters.AddWithValue("@idNumber", borrower.IdNumber);
                insert.Parameters.AddWithValue("@dayBeforeBorrow", borrower.DaysBorrowed);
                insert.ExecuteNonQuery();

                return (int)insert.LastInsertedId;
            }
        }

        private static object NullIfEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value;
        }

        private void GoOffline(MySqlException ex)
        {
            IsConnected = false;
            LastWarning = "THE DATABASE CONNECTION WAS LOST, SO THIS CHANGE IS KEPT FOR THE SESSION ONLY.\n" + ex.Message;
        }
    }
}
