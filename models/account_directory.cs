using System;
using System.Collections.Generic;
using System.Globalization;
using MySql.Data.MySqlClient;
using oop_project.Database;

namespace oop_project.Models
{
    // ENCAPSULATION: the ACCOUNTS tabs only see finished rows; the SQL that reads
    // and approves accounts stays inside AccountDirectory.
    public class AccountRow
    {
        public int AccountId { get; private set; }
        public string Name { get; private set; }
        public string Username { get; private set; }
        public string Role { get; private set; }
        public string Status { get; private set; }
        public string DateCreated { get; private set; }

        // Profile values, read through the foreign keys of student_info_table,
        // faculty_info_table and staff_info_table. They stay empty for roles that
        // do not own the matching value (a FACULTY member has no program and a
        // LIBRARIAN has no office).
        public string SchoolId { get; private set; }
        public string Program { get; private set; }
        public string YearLevel { get; private set; }
        public string Institute { get; private set; }
        public string ContactNumber { get; private set; }
        public string Address { get; private set; }
        public string Office { get; private set; }

        // True when the account has a profile row behind it, so the PROFILE tab can
        // decide between showing the record block and showing the account only.
        public bool HasProfile { get; private set; }

        public AccountRow(int accountId, string name, string username, string role, string status, string dateCreated,
                          string schoolId = "", string program = "", string yearLevel = "", string institute = "",
                          string contactNumber = "", string address = "", string office = "")
        {
            AccountId = accountId;
            Name = string.IsNullOrWhiteSpace(name) ? "UNNAMED ACCOUNT" : name.Trim();
            Username = string.IsNullOrWhiteSpace(username) ? "—" : username.Trim();
            Role = string.IsNullOrWhiteSpace(role) ? "STUDENT" : role.Trim().ToUpperInvariant();
            Status = string.IsNullOrWhiteSpace(status) ? "PENDING" : status.Trim().ToUpperInvariant();
            DateCreated = string.IsNullOrWhiteSpace(dateCreated) ? "—" : dateCreated.Trim();

            HasProfile = !string.IsNullOrWhiteSpace(schoolId) ||
                         !string.IsNullOrWhiteSpace(program) ||
                         !string.IsNullOrWhiteSpace(yearLevel) ||
                         !string.IsNullOrWhiteSpace(institute) ||
                         !string.IsNullOrWhiteSpace(contactNumber);

            SchoolId = Normalize(schoolId);
            Program = Normalize(program);
            YearLevel = Normalize(yearLevel);
            Institute = Normalize(institute);
            ContactNumber = Normalize(contactNumber);
            Address = Normalize(address);
            Office = Normalize(office);
        }

        public bool IsActive
        {
            get { return string.Equals(Status, "ACTIVE", StringComparison.OrdinalIgnoreCase); }
        }

        public bool IsStudent
        {
            get { return Role == "STUDENT"; }
        }

        public bool IsFaculty
        {
            get { return Role == "FACULTY"; }
        }

        // The librarian runs the system, so the LIBRARIAN tab shows this role (the
        // legacy ADMIN rows belong there too). STAFF / legacy OFFICER rows get their
        // own tab.
        public bool IsLibrarian
        {
            get { return Role == "LIBRARIAN" || Role == "ADMIN"; }
        }

        public bool IsStaff
        {
            get { return Role == "STAFF" || Role == "OFFICER"; }
        }

        // LIBRARIAN / STAFF (and the legacy ADMIN / OFFICER rows) run the library,
        // so they get the two library account tabs: LIBRARIAN ACCOUNTS and STAFF
        // ACCOUNTS.
        public bool IsLibraryStaff
        {
            get { return !IsStudent && !IsFaculty; }
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "—" : value.Trim();
        }
    }

    public class AccountDirectory
    {
        private const string AccountSelect =
            "SELECT a.`account_id`, a.`name`, a.`username`, " +
            "IFNULL(r.`role`, 'STUDENT'), IFNULL(r.`status`, 'PENDING'), IFNULL(r.`date_created`, ''), " +
            "IFNULL(s.`school_id`, ''), " +
            "IFNULL(p.`program_name`, ''), " +
            "IFNULL(s.`year_level`, ''), " +
            "IFNULL(COALESCE(i.`institute_code`, fi.`institute_code`), ''), " +
            "IFNULL(COALESCE(s.`contact_number`, f.`contact_number`, st.`contact_number`, li.`contact_number`), ''), " +
            "IFNULL(s.`address`, ''), " +
            "IFNULL(st.`office`, '') " +
            "FROM `account_table` a " +
            "LEFT JOIN `account_role_table` r ON r.`account_id` = a.`account_id` " +
            "LEFT JOIN `student_info_table` s ON s.`account_id` = a.`account_id` " +
            "LEFT JOIN `program_table` p ON p.`program_id` = s.`program_id` " +
            "LEFT JOIN `institute_table` i ON i.`institute_id` = s.`institute_id` " +
            "LEFT JOIN `faculty_info_table` f ON f.`account_id` = a.`account_id` " +
            "LEFT JOIN `institute_table` fi ON fi.`institute_id` = f.`institute_id` " +
            "LEFT JOIN `staff_info_table` st ON st.`account_id` = a.`account_id` " +
            "LEFT JOIN `librarian_info_table` li ON li.`account_id` = a.`account_id` ";

        private readonly IDatabaseConnection _database = new MySqlDatabaseConnection();

      
        public List<AccountRow> Load()
        {
            List<AccountRow> rows = new List<AccountRow>();

            using (MySqlConnection connection = _database.CreateConnection())
            {
                connection.Open();

                using (MySqlCommand command = new MySqlCommand(AccountSelect + "ORDER BY a.`account_id`;", connection))
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rows.Add(ReadRow(reader));
                    }
                }
            }

            return rows;
        }

        /// <summary>
        /// Reads one account, so the PROFILE tab can show the signed in person's own
        /// profile values (program, institute, school id...) without listing everyone.
        /// </summary>
        public AccountRow? LoadProfile(int accountId)
        {
            if (accountId <= 0)
            {
                return null;
            }

            using (MySqlConnection connection = _database.CreateConnection())
            {
                connection.Open();

                using (MySqlCommand command = new MySqlCommand(AccountSelect + "WHERE a.`account_id` = @accountId;", connection))
                {
                    command.Parameters.AddWithValue("@accountId", accountId);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        return reader.Read() ? ReadRow(reader) : null;
                    }
                }
            }
        }

        private static AccountRow ReadRow(MySqlDataReader reader)
        {
            return new AccountRow(
                reader.GetInt32(0),
                Text(reader, 1),
                Text(reader, 2),
                Text(reader, 3, "STUDENT"),
                Text(reader, 4, "PENDING"),
                Text(reader, 5),
                Text(reader, 6),
                Text(reader, 7),
                Text(reader, 8),
                Text(reader, 9),
                Text(reader, 10),
                Text(reader, 11),
                Text(reader, 12));
        }

        private static string Text(MySqlDataReader reader, int index, string fallback = "")
        {
            if (reader.IsDBNull(index))
            {
                return fallback;
            }

            return Convert.ToString(reader.GetValue(index), CultureInfo.InvariantCulture) ?? fallback;
        }

        /// <summary>
        /// Changes the sign-in password only when the current password matches.
        /// The comparison uses BINARY so the check is exact, just like the login
        /// query. Returns false when the current password is wrong.
        /// </summary>
        public bool ChangePassword(int accountId, string currentPassword, string newPassword)
        {
            if (accountId <= 0)
            {
                throw new ArgumentException("A signed in account is required.", nameof(accountId));
            }

            if (string.IsNullOrEmpty(currentPassword))
            {
                throw new ArgumentException("The current password is required.", nameof(currentPassword));
            }

            if (string.IsNullOrEmpty(newPassword))
            {
                throw new ArgumentException("The new password is required.", nameof(newPassword));
            }

            using (MySqlConnection connection = _database.CreateConnection())
            {
                connection.Open();

                const string sql =
                    "UPDATE `account_table` SET `password` = @newPassword " +
                    "WHERE `account_id` = @accountId AND BINARY `password` = BINARY @currentPassword;";

                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@newPassword", newPassword);
                    command.Parameters.AddWithValue("@accountId", accountId);
                    command.Parameters.AddWithValue("@currentPassword", currentPassword);

                    return command.ExecuteNonQuery() == 1;
                }
            }
        }

        /// <summary>
        /// Sets ACTIVE, PENDING, INACTIVE or REJECTED on one account. A role row is
        /// created when the account was registered without one.
        /// </summary>
        public void SetStatus(int accountId, string status)
        {
            if (accountId <= 0)
            {
                throw new ArgumentException("Account id is required.", nameof(accountId));
            }

            if (string.IsNullOrWhiteSpace(status))
            {
                throw new ArgumentException("Status is required.", nameof(status));
            }

            string normalized = status.Trim().ToUpperInvariant();

            using (MySqlConnection connection = _database.CreateConnection())
            {
                connection.Open();

                const string existsSql = "SELECT COUNT(*) FROM `account_role_table` WHERE `account_id` = @accountId;";

                long rows;
                using (MySqlCommand exists = new MySqlCommand(existsSql, connection))
                {
                    exists.Parameters.AddWithValue("@accountId", accountId);
                    rows = Convert.ToInt64(exists.ExecuteScalar(), CultureInfo.InvariantCulture);
                }

                if (rows == 0)
                {
                    const string insertSql = "INSERT INTO `account_role_table` (`account_id`, `role`, `status`, `date_created`) " +
                                             "VALUES (@accountId, @role, @status, @dateCreated);";

                    using (MySqlCommand insert = new MySqlCommand(insertSql, connection))
                    {
                        insert.Parameters.AddWithValue("@accountId", accountId);
                        insert.Parameters.AddWithValue("@role", "STUDENT");
                        insert.Parameters.AddWithValue("@status", normalized);
                        insert.Parameters.AddWithValue("@dateCreated", DateTime.Now.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture).ToUpperInvariant());
                        insert.ExecuteNonQuery();
                    }

                    return;
                }

                const string updateSql = "UPDATE `account_role_table` SET `status` = @status WHERE `account_id` = @accountId;";

                using (MySqlCommand update = new MySqlCommand(updateSql, connection))
                {
                    update.Parameters.AddWithValue("@status", normalized);
                    update.Parameters.AddWithValue("@accountId", accountId);
                    update.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// UPDATE the account, its role row and, when the role owns one, its profile
        /// row (student_info_table, faculty_info_table, staff_info_table or
        /// librarian_info_table). Used by the EDIT pop up of the three account tabs.
        /// </summary>
        public void UpdateAccount(
            AccountRow row,
            string name,
            string username,
            string role,
            string status,
            string schoolId,
            string program,
            string yearLevel,
            string institute,
            string contactNumber,
            string office,
            bool updateStudentProfile,
            bool updateFacultyProfile,
            bool updateStaffProfile,
            bool updateLibrarianProfile)
        {
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name is required.", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username is required.", nameof(username));
            }

            if (string.IsNullOrWhiteSpace(role))
            {
                throw new ArgumentException("Role is required.", nameof(role));
            }

            if (string.IsNullOrWhiteSpace(status))
            {
                throw new ArgumentException("Status is required.", nameof(status));
            }

            if (updateStudentProfile)
            {
                if (string.IsNullOrWhiteSpace(schoolId))
                {
                    throw new ArgumentException("School ID is required for a student account.", nameof(schoolId));
                }

                if (string.IsNullOrWhiteSpace(program))
                {
                    throw new ArgumentException("Program is required for a student account.", nameof(program));
                }
            }

            if (updateStaffProfile && string.IsNullOrWhiteSpace(office))
            {
                throw new ArgumentException("Office is required for a staff account.", nameof(office));
            }

            try
            {
                using (MySqlConnection connection = _database.CreateConnection())
                {
                    connection.Open();

                    using (MySqlTransaction transaction = connection.BeginTransaction())
                    {
                        const string accountSql = "UPDATE `account_table` SET `name` = @name, `username` = @username " +
                                                  "WHERE `account_id` = @accountId;";

                        using (MySqlCommand accountCommand = new MySqlCommand(accountSql, connection, transaction))
                        {
                            accountCommand.Parameters.AddWithValue("@name", name.Trim());
                            accountCommand.Parameters.AddWithValue("@username", username.Trim());
                            accountCommand.Parameters.AddWithValue("@accountId", row.AccountId);
                            accountCommand.ExecuteNonQuery();
                        }

                        SaveRoleRow(connection, transaction, row.AccountId, role, status);

                        if (updateStudentProfile)
                        {
                            long instituteId = ResolveInstituteId(connection, transaction, institute);
                            long programId = ResolveProgramId(connection, transaction, program, instituteId);

                            const string studentSql =
                                "UPDATE `student_info_table` SET `school_id` = @schoolId, `program_id` = @programId, " +
                                "`year_level` = @yearLevel, `institute_id` = @instituteId, `contact_number` = @contact " +
                                "WHERE `account_id` = @accountId;";

                            using (MySqlCommand studentCommand = new MySqlCommand(studentSql, connection, transaction))
                            {
                                studentCommand.Parameters.AddWithValue("@schoolId", schoolId.Trim());
                                studentCommand.Parameters.AddWithValue("@programId", programId);
                                studentCommand.Parameters.AddWithValue("@yearLevel", Limit(yearLevel, 20));
                                studentCommand.Parameters.AddWithValue("@instituteId", instituteId);
                                studentCommand.Parameters.AddWithValue("@contact", Limit(contactNumber, 20));
                                studentCommand.Parameters.AddWithValue("@accountId", row.AccountId);
                                studentCommand.ExecuteNonQuery();
                            }
                        }

                        if (updateFacultyProfile)
                        {
                            long instituteId = ResolveInstituteId(connection, transaction, institute);

                            const string facultySql =
                                "UPDATE `faculty_info_table` SET `institute_id` = @instituteId " +
                                "WHERE `account_id` = @accountId;";

                            using (MySqlCommand facultyCommand = new MySqlCommand(facultySql, connection, transaction))
                            {
                                facultyCommand.Parameters.AddWithValue("@instituteId", instituteId);
                                facultyCommand.Parameters.AddWithValue("@accountId", row.AccountId);
                                facultyCommand.ExecuteNonQuery();
                            }
                        }

                        if (updateStaffProfile)
                        {
                            SaveOfficeProfile(connection, transaction, "staff_info_table", row.AccountId, office);
                        }

                        if (updateLibrarianProfile)
                        {
                            EnsureLibrarianProfile(connection, transaction, row.AccountId);
                        }

                        transaction.Commit();
                    }
                }
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                throw new InvalidOperationException(
                    "THAT USERNAME OR SCHOOL ID IS ALREADY USED BY ANOTHER ACCOUNT.", ex);
            }
        }

        /// <summary>
        /// SELF SERVICE update used by the EDIT MY INFO button of the PROFILE tab.
        /// The signed in person edits every detail that belongs to the account:
        /// the name and username of account_table and the profile values of the
        /// student / faculty / staff / librarian profile row. The role, the
        /// status and the date created are never written here, so a patron cannot
        /// promote their own account.
        /// </summary>
        public void UpdateOwnProfile(
            AccountRow row,
            string name,
            string username,
            string schoolId,
            string program,
            string yearLevel,
            string institute,
            string contactNumber,
            string address,
            string office)
        {
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name is required.", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username is required.", nameof(username));
            }

            if (row.HasProfile && row.IsStudent)
            {
                if (string.IsNullOrWhiteSpace(schoolId))
                {
                    throw new ArgumentException("School ID is required for a student account.", nameof(schoolId));
                }

                if (string.IsNullOrWhiteSpace(program))
                {
                    throw new ArgumentException("Program is required for a student account.", nameof(program));
                }
            }

            if (row.IsStaff && string.IsNullOrWhiteSpace(office))
            {
                throw new ArgumentException("Office is required for a staff account.", nameof(office));
            }

            if ((row.HasProfile && row.IsStudent) || row.IsFaculty)
            {
                if (string.IsNullOrWhiteSpace(institute))
                {
                    throw new ArgumentException("Institute is required.", nameof(institute));
                }
            }

            try
            {
                using (MySqlConnection connection = _database.CreateConnection())
                {
                    connection.Open();

                    using (MySqlTransaction transaction = connection.BeginTransaction())
                    {
                        const string accountSql =
                            "UPDATE `account_table` SET `name` = @name, `username` = @username " +
                            "WHERE `account_id` = @accountId;";

                        using (MySqlCommand accountCommand = new MySqlCommand(accountSql, connection, transaction))
                        {
                            accountCommand.Parameters.AddWithValue("@name", name.Trim());
                            accountCommand.Parameters.AddWithValue("@username", username.Trim());
                            accountCommand.Parameters.AddWithValue("@accountId", row.AccountId);
                            accountCommand.ExecuteNonQuery();
                        }

                        if (row.HasProfile && row.IsStudent)
                        {
                            long instituteId = ResolveInstituteId(connection, transaction, institute);
                            long programId = ResolveProgramId(connection, transaction, program, instituteId);

                            const string studentSql =
                                "UPDATE `student_info_table` SET `school_id` = @schoolId, `program_id` = @programId, " +
                                "`year_level` = @yearLevel, `institute_id` = @instituteId, `contact_number` = @contact, " +
                                "`address` = @address WHERE `account_id` = @accountId;";

                            using (MySqlCommand studentCommand = new MySqlCommand(studentSql, connection, transaction))
                            {
                                studentCommand.Parameters.AddWithValue("@schoolId", schoolId.Trim());
                                studentCommand.Parameters.AddWithValue("@programId", programId);
                                studentCommand.Parameters.AddWithValue("@yearLevel", Limit(yearLevel, 20));
                                studentCommand.Parameters.AddWithValue("@instituteId", instituteId);
                                studentCommand.Parameters.AddWithValue("@contact", Limit(contactNumber, 20));
                                studentCommand.Parameters.AddWithValue("@address", Limit(address, 255));
                                studentCommand.Parameters.AddWithValue("@accountId", row.AccountId);
                                studentCommand.ExecuteNonQuery();
                            }
                        }
                        else if (row.IsFaculty)
                        {
                            long instituteId = ResolveInstituteId(connection, transaction, institute);

                            // NORMALISATION: the faculty profile is created on the
                            // first save when an older account never had one.
                            const string facultySql =
                                "INSERT INTO `faculty_info_table` (`account_id`, `institute_id`, `contact_number`) " +
                                "VALUES (@accountId, @instituteId, @contact) " +
                                "ON DUPLICATE KEY UPDATE `institute_id` = @instituteId, `contact_number` = @contact;";

                            using (MySqlCommand facultyCommand = new MySqlCommand(facultySql, connection, transaction))
                            {
                                facultyCommand.Parameters.AddWithValue("@accountId", row.AccountId);
                                facultyCommand.Parameters.AddWithValue("@instituteId", instituteId);
                                facultyCommand.Parameters.AddWithValue("@contact", Limit(contactNumber, 20));
                                facultyCommand.ExecuteNonQuery();
                            }
                        }
                        else if (row.IsStaff)
                        {
                            SaveOfficeProfile(connection, transaction, "staff_info_table", row.AccountId, office);
                            SaveProfileContact(connection, transaction, "staff_info_table", row.AccountId, contactNumber);
                        }
                        else if (row.IsLibrarian)
                        {
                            EnsureLibrarianProfile(connection, transaction, row.AccountId);
                            SaveProfileContact(connection, transaction, "librarian_info_table", row.AccountId, contactNumber);
                        }

                        transaction.Commit();
                    }
                }
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                throw new InvalidOperationException(
                    "THAT USERNAME OR SCHOOL ID IS ALREADY USED BY ANOTHER ACCOUNT.", ex);
            }
        }

        /// <summary>
        /// DELETE one account. The role row and the student / faculty / staff /
        /// librarian profile rows are removed with it through the ON DELETE CASCADE
        /// foreign keys.
        /// </summary>
        public void DeleteAccount(int accountId)
        {
            if (accountId <= 0)
            {
                throw new ArgumentException("Account id is required.", nameof(accountId));
            }

            using (MySqlConnection connection = _database.CreateConnection())
            {
                connection.Open();

                const string sql = "DELETE FROM `account_table` WHERE `account_id` = @accountId;";

                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@accountId", accountId);
                    command.ExecuteNonQuery();
                }
            }
        }

        private static void SaveRoleRow(
            MySqlConnection connection,
            MySqlTransaction transaction,
            int accountId,
            string role,
            string status)
        {
            string normalizedRole = role.Trim().ToUpperInvariant();
            string normalizedStatus = status.Trim().ToUpperInvariant();

            const string existsSql = "SELECT COUNT(*) FROM `account_role_table` WHERE `account_id` = @accountId;";

            long rows;
            using (MySqlCommand exists = new MySqlCommand(existsSql, connection, transaction))
            {
                exists.Parameters.AddWithValue("@accountId", accountId);
                rows = Convert.ToInt64(exists.ExecuteScalar(), CultureInfo.InvariantCulture);
            }

            if (rows == 0)
            {
                const string insertSql = "INSERT INTO `account_role_table` (`account_id`, `role`, `status`, `date_created`) " +
                                         "VALUES (@accountId, @role, @status, @dateCreated);";

                using (MySqlCommand insert = new MySqlCommand(insertSql, connection, transaction))
                {
                    insert.Parameters.AddWithValue("@accountId", accountId);
                    insert.Parameters.AddWithValue("@role", normalizedRole);
                    insert.Parameters.AddWithValue("@status", normalizedStatus);
                    insert.Parameters.AddWithValue(
                        "@dateCreated",
                        DateTime.Now.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture).ToUpperInvariant());
                    insert.ExecuteNonQuery();
                }

                return;
            }

            const string updateSql = "UPDATE `account_role_table` SET `role` = @role, `status` = @status " +
                                     "WHERE `account_id` = @accountId;";

            using (MySqlCommand update = new MySqlCommand(updateSql, connection, transaction))
            {
                update.Parameters.AddWithValue("@role", normalizedRole);
                update.Parameters.AddWithValue("@status", normalizedStatus);
                update.Parameters.AddWithValue("@accountId", accountId);
                update.ExecuteNonQuery();
            }
        }

        // NORMALISATION: the librarian profile only links the account to its role
        // row. No office is stored for the librarian, so the upsert keeps the links
        // and does not touch an office column.
        private static void EnsureLibrarianProfile(
            MySqlConnection connection,
            MySqlTransaction transaction,
            int accountId)
        {
            long roleId = ResolveRoleId(connection, transaction, accountId);

            const string sql = "INSERT INTO `librarian_info_table` (`account_id`, `role_id`) " +
                               "VALUES (@accountId, @roleId) " +
                               "ON DUPLICATE KEY UPDATE `role_id` = @roleId;";

            using (MySqlCommand command = new MySqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue("@accountId", accountId);
                command.Parameters.AddWithValue("@roleId", roleId);
                command.ExecuteNonQuery();
            }
        }

        // NORMALISATION: the librarian / staff profile points at the account and at
        // its role row. The role row is saved before this method runs, so its id is
        // read here and stored as the second foreign key. The upsert also gives an
        // older account a profile row the first time the admin saves it.
        private static void SaveOfficeProfile(
            MySqlConnection connection,
            MySqlTransaction transaction,
            string table,
            int accountId,
            string office)
        {
            long roleId = ResolveRoleId(connection, transaction, accountId);

            string sql = "INSERT INTO `" + table + "` (`account_id`, `role_id`, `office`) " +
                         "VALUES (@accountId, @roleId, @office) " +
                         "ON DUPLICATE KEY UPDATE `role_id` = @roleId, `office` = @office;";

            using (MySqlCommand command = new MySqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue("@accountId", accountId);
                command.Parameters.AddWithValue("@roleId", roleId);
                command.Parameters.AddWithValue("@office", Limit(office, 120));
                command.ExecuteNonQuery();
            }
        }

        // SELF SERVICE: the contact number of a staff or librarian profile row is
        // written after the row itself exists, so a single UPDATE is enough.
        private static void SaveProfileContact(
            MySqlConnection connection,
            MySqlTransaction transaction,
            string table,
            int accountId,
            string contactNumber)
        {
            string sql = "UPDATE `" + table + "` SET `contact_number` = @contact " +
                         "WHERE `account_id` = @accountId;";

            using (MySqlCommand command = new MySqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue("@contact", Limit(contactNumber, 20));
                command.Parameters.AddWithValue("@accountId", accountId);
                command.ExecuteNonQuery();
            }
        }

        private static long ResolveRoleId(MySqlConnection connection, MySqlTransaction transaction, int accountId)
        {
            const string sql = "SELECT `role_id` FROM `account_role_table` WHERE `account_id` = @accountId " +
                               "ORDER BY `role_id` LIMIT 1;";

            using (MySqlCommand command = new MySqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue("@accountId", accountId);
                object? result = command.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                {
                    throw new InvalidOperationException(
                        "The account has no role row, so a staff profile cannot be saved.");
                }

                return Convert.ToInt64(result, CultureInfo.InvariantCulture);
            }
        }

        private static long ResolveInstituteId(MySqlConnection connection, MySqlTransaction transaction, string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Institute is required.", nameof(code));
            }

            const string sql = "SELECT `institute_id` FROM `institute_table` WHERE `institute_code` = @code LIMIT 1;";

            using (MySqlCommand command = new MySqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue("@code", code.Trim().ToUpperInvariant());
                object? result = command.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                {
                    throw new ArgumentException(
                        "Unknown institute code \"" + code.Trim() + "\". Use a code from institute_table, e.g. ICS.");
                }

                return Convert.ToInt64(result, CultureInfo.InvariantCulture);
            }
        }

        private static long ResolveProgramId(
            MySqlConnection connection,
            MySqlTransaction transaction,
            string name,
            long instituteId)
        {
            const string sql = "INSERT INTO `program_table` (`program_name`, `institute_id`) VALUES (@name, @instituteId) " +
                               "ON DUPLICATE KEY UPDATE `program_id` = LAST_INSERT_ID(`program_id`);";

            using (MySqlCommand command = new MySqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue("@name", (name ?? string.Empty).Trim().ToUpperInvariant());
                command.Parameters.AddWithValue("@instituteId", instituteId);
                command.ExecuteNonQuery();
                return command.LastInsertedId;
            }
        }

        private static string Limit(string value, int maximumLength)
        {
            string trimmed = (value ?? string.Empty).Trim();
            return trimmed.Length > maximumLength ? trimmed.Substring(0, maximumLength) : trimmed;
        }
    }
}
