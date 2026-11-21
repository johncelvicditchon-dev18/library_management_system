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

        // PROFILE PICTURE: a small JPEG stored on account_table. It stays null for
        // accounts that never uploaded a portrait, so the avatars fall back to the
        // initials of the person.
        public byte[]? ProfilePicture { get; private set; }

        internal void SetProfilePicture(byte[]? picture)
        {
            ProfilePicture = picture;
        }

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

        // The librarian runs the system, so the LIBRARIAN tab shows this role.
        // STAFF rows get their own tab.
        public bool IsLibrarian
        {
            get { return Role == "LIBRARIAN"; }
        }

        public bool IsStaff
        {
            get { return Role == "STAFF"; }
        }

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
            "IFNULL(rt.`role_code`, 'STUDENT'), IFNULL(ast.`status_code`, 'PENDING'), " +
            "IFNULL(UPPER(DATE_FORMAT(ar.`date_created`, '%M %e, %Y')), ''), " +
            "IFNULL(s.`school_id`, ''), " +
            "IFNULL(p.`program_name`, ''), " +
            "IFNULL(s.`year_level`, ''), " +
            "IFNULL(COALESCE(i.`institute_code`, fi.`institute_code`), ''), " +
            "IFNULL(COALESCE(s.`contact_number`, f.`contact_number`, st.`contact_number`, li.`contact_number`), ''), " +
            "IFNULL(s.`address`, ''), " +
            "IFNULL(st.`office`, '') " +
            "FROM `account_table` a " +
            "LEFT JOIN `account_role_table` ar ON ar.`account_id` = a.`account_id` " +
            "LEFT JOIN `role_table` rt ON rt.`role_id` = ar.`role_id` " +
            "LEFT JOIN `account_status_table` ast ON ast.`status_id` = ar.`status_id` " +
            "LEFT JOIN `student_info_table` s ON s.`account_id` = a.`account_id` " +
            "LEFT JOIN `program_table` p ON p.`program_id` = s.`program_id` " +
            "LEFT JOIN `institute_table` i ON i.`institute_id` = p.`institute_id` " +
            "LEFT JOIN `faculty_info_table` f ON f.`account_id` = a.`account_id` " +
            "LEFT JOIN `institute_table` fi ON fi.`institute_id` = f.`institute_id` " +
            "LEFT JOIN `staff_info_table` st ON st.`account_id` = a.`account_id` " +
            "LEFT JOIN `librarian_info_table` li ON li.`account_id` = a.`account_id` ";

        private readonly IDatabaseConnection _database = new MySqlDatabaseConnection();

        // The profile picture column only exists in databases created (or upgraded)
        // after the photo feature, so it is checked once per process.
        private static readonly object PictureColumnGate = new object();
        private static bool _pictureColumnChecked;
        private static bool _pictureColumnAvailable;

        private static bool EnsurePictureColumn(MySqlConnection connection)
        {
            if (_pictureColumnChecked)
            {
                return _pictureColumnAvailable;
            }

            lock (PictureColumnGate)
            {
                if (!_pictureColumnChecked)
                {
                    try
                    {
                        Authentication.EnsureAccountPictureColumn(connection);
                        _pictureColumnAvailable = true;
                    }
                    catch (MySqlException)
                    {
                        // A database user without ALTER rights keeps text-only profiles.
                        _pictureColumnAvailable = false;
                    }

                    _pictureColumnChecked = true;
                }

                return _pictureColumnAvailable;
            }
        }

        private static byte[]? ReadPicture(MySqlConnection connection, int accountId)
        {
            using (MySqlCommand command = new MySqlCommand(
                       "SELECT `profile_picture` FROM `account_table` WHERE `account_id` = @accountId;",
                       connection))
            {
                command.Parameters.AddWithValue("@accountId", accountId);
                object? value = command.ExecuteScalar();

                return value == null || value == DBNull.Value ? null : value as byte[];
            }
        }

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

                bool pictureColumn = EnsurePictureColumn(connection);
                AccountRow? row = null;

                using (MySqlCommand command = new MySqlCommand(AccountSelect + "WHERE a.`account_id` = @accountId;", connection))
                {
                    command.Parameters.AddWithValue("@accountId", accountId);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            row = ReadRow(reader);
                        }
                    }
                }

                if (row != null && pictureColumn)
                {
                    row.SetProfilePicture(ReadPicture(connection, accountId));
                }

                return row;
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
        /// SECURITY: account_table keeps a PBKDF2 hash (or the plain text of an
        /// account made by an older build), and both forms are accepted here
        /// before the new password is written as a hash. Returns false when the
        /// current password is wrong.
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

                const string selectSql =
                    "SELECT `password` FROM `account_table` WHERE `account_id` = @accountId;";

                string storedPassword;
                using (MySqlCommand select = new MySqlCommand(selectSql, connection))
                {
                    select.Parameters.AddWithValue("@accountId", accountId);
                    object? value = select.ExecuteScalar();

                    if (value == null || value == DBNull.Value)
                    {
                        return false;
                    }

                    storedPassword = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
                }

                if (!PasswordHasher.Verify(currentPassword, storedPassword))
                {
                    return false;
                }

                const string updateSql =
                    "UPDATE `account_table` SET `password` = @newPassword WHERE `account_id` = @accountId;";

                using (MySqlCommand update = new MySqlCommand(updateSql, connection))
                {
                    update.Parameters.AddWithValue("@newPassword", PasswordHasher.Hash(newPassword));
                    update.Parameters.AddWithValue("@accountId", accountId);
                    return update.ExecuteNonQuery() == 1;
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
                    const string insertSql =
                        "INSERT INTO `account_role_table` (`account_id`, `role_id`, `status_id`, `date_created`) " +
                        "VALUES (@accountId, " +
                        "        (SELECT `role_id` FROM `role_table` WHERE `role_code` = 'STUDENT'), " +
                        "        (SELECT `status_id` FROM `account_status_table` WHERE `status_code` = @status), " +
                        "        CURDATE());";

                    using (MySqlCommand insert = new MySqlCommand(insertSql, connection))
                    {
                        insert.Parameters.AddWithValue("@accountId", accountId);
                        insert.Parameters.AddWithValue("@status", normalized);
                        insert.ExecuteNonQuery();
                    }

                    return;
                }

                const string updateSql =
                    "UPDATE `account_role_table` SET `status_id` = " +
                    "(SELECT `status_id` FROM `account_status_table` WHERE `status_code` = @status) " +
                    "WHERE `account_id` = @accountId;";

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

                        // NAME PROPAGATION: the account row owns the display name,
                        // so the copies kept by the profile, the borrow records and
                        // the survey answers are rewritten with the new one.
                        PropagateName(
                            connection,
                            transaction,
                            row,
                            name,
                            updateStudentProfile ? schoolId : string.Empty);

                        SaveRoleRow(connection, transaction, row.AccountId, role, status);

                        if (updateStudentProfile)
                        {
                            long instituteId = ResolveInstituteId(connection, transaction, institute);
                            long programId = ResolveProgramId(connection, transaction, program, instituteId);

                            const string studentSql =
                                "UPDATE `student_info_table` SET `school_id` = @schoolId, `program_id` = @programId, " +
                                "`year_level` = @yearLevel, `contact_number` = @contact " +
                                "WHERE `account_id` = @accountId;";

                            using (MySqlCommand studentCommand = new MySqlCommand(studentSql, connection, transaction))
                            {
                                studentCommand.Parameters.AddWithValue("@schoolId", schoolId.Trim());
                                studentCommand.Parameters.AddWithValue("@programId", programId);
                                studentCommand.Parameters.AddWithValue("@yearLevel", Limit(yearLevel, 20));
                                studentCommand.Parameters.AddWithValue("@contact", Limit(contactNumber, 20));
                                studentCommand.Parameters.AddWithValue("@accountId", row.AccountId);
                                studentCommand.ExecuteNonQuery();
                            }
                        }

                        if (updateFacultyProfile)
                        {
                            long instituteId = ResolveInstituteId(connection, transaction, institute);

                            const string facultySql =
                                "UPDATE `faculty_info_table` SET `institute_id` = @instituteId, " +
                                "`contact_number` = @contact WHERE `account_id` = @accountId;";

                            using (MySqlCommand facultyCommand = new MySqlCommand(facultySql, connection, transaction))
                            {
                                facultyCommand.Parameters.AddWithValue("@instituteId", instituteId);
                                facultyCommand.Parameters.AddWithValue("@contact", Limit(contactNumber, 20));
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

                        // CONTACT PROPAGATION: the profile is the source of truth for
                        // the phone number, so existing borrow records follow it.
                        PropagateContact(connection, transaction, row, schoolId, contactNumber);

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
            string office,
            byte[]? profilePicture = null,
            bool updateProfilePicture = false)
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

            if (updateProfilePicture && profilePicture != null && profilePicture.Length > 8 * 1024 * 1024)
            {
                throw new ArgumentException("The profile picture is too large.", nameof(profilePicture));
            }

            try
            {
                using (MySqlConnection connection = _database.CreateConnection())
                {
                    connection.Open();

                    bool pictureColumn = EnsurePictureColumn(connection);

                    using (MySqlTransaction transaction = connection.BeginTransaction())
                    {
                        string accountSql =
                            "UPDATE `account_table` SET `name` = @name, `username` = @username";

                        if (updateProfilePicture && pictureColumn)
                        {
                            accountSql += ", `profile_picture` = @picture";
                        }

                        accountSql += " WHERE `account_id` = @accountId;";

                        using (MySqlCommand accountCommand = new MySqlCommand(accountSql, connection, transaction))
                        {
                            accountCommand.Parameters.AddWithValue("@name", name.Trim());
                            accountCommand.Parameters.AddWithValue("@username", username.Trim());
                            accountCommand.Parameters.AddWithValue("@accountId", row.AccountId);

                            if (updateProfilePicture && pictureColumn)
                            {
                                accountCommand.Parameters.Add("@picture", MySqlDbType.MediumBlob).Value =
                                    (object?)profilePicture ?? DBNull.Value;
                            }

                            accountCommand.ExecuteNonQuery();
                        }

                        // NAME PROPAGATION: the person may have changed the full
                        // name from the PROFILE tab, so the copies kept by the
                        // profile, the borrow records and the survey answers are
                        // rewritten with the new one.
                        PropagateName(
                            connection,
                            transaction,
                            row,
                            name,
                            row.HasProfile && row.IsStudent ? schoolId : string.Empty);

                        if (row.HasProfile && row.IsStudent)
                        {
                            long instituteId = ResolveInstituteId(connection, transaction, institute);
                            long programId = ResolveProgramId(connection, transaction, program, instituteId);

                            const string studentSql =
                                "UPDATE `student_info_table` SET `school_id` = @schoolId, `program_id` = @programId, " +
                                "`year_level` = @yearLevel, `contact_number` = @contact, " +
                                "`address` = @address WHERE `account_id` = @accountId;";

                            using (MySqlCommand studentCommand = new MySqlCommand(studentSql, connection, transaction))
                            {
                                studentCommand.Parameters.AddWithValue("@schoolId", schoolId.Trim());
                                studentCommand.Parameters.AddWithValue("@programId", programId);
                                studentCommand.Parameters.AddWithValue("@yearLevel", Limit(yearLevel, 20));
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

                        // CONTACT PROPAGATION: a new phone number saved on the profile
                        // is written to the borrower rows too.
                        PropagateContact(connection, transaction, row, schoolId, contactNumber);

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
                const string insertSql =
                    "INSERT INTO `account_role_table` (`account_id`, `role_id`, `status_id`, `date_created`) " +
                    "VALUES (@accountId, " +
                    "        (SELECT `role_id` FROM `role_table` WHERE `role_code` = @role), " +
                    "        (SELECT `status_id` FROM `account_status_table` WHERE `status_code` = @status), " +
                    "        CURDATE());";

                using (MySqlCommand insert = new MySqlCommand(insertSql, connection, transaction))
                {
                    insert.Parameters.AddWithValue("@accountId", accountId);
                    insert.Parameters.AddWithValue("@role", normalizedRole);
                    insert.Parameters.AddWithValue("@status", normalizedStatus);
                    insert.ExecuteNonQuery();
                }

                return;
            }

            const string updateSql =
                "UPDATE `account_role_table` SET " +
                "`role_id` = (SELECT `role_id` FROM `role_table` WHERE `role_code` = @role), " +
                "`status_id` = (SELECT `status_id` FROM `account_status_table` WHERE `status_code` = @status) " +
                "WHERE `account_id` = @accountId;";

            using (MySqlCommand update = new MySqlCommand(updateSql, connection, transaction))
            {
                update.Parameters.AddWithValue("@role", normalizedRole);
                update.Parameters.AddWithValue("@status", normalizedStatus);
                update.Parameters.AddWithValue("@accountId", accountId);
                update.ExecuteNonQuery();
            }
        }

        // NORMALISATION: the librarian profile is a 1:1 extension of the account.
        // The role is read through account_id, so no role_id is stored here.
        private static void EnsureLibrarianProfile(
            MySqlConnection connection,
            MySqlTransaction transaction,
            int accountId)
        {
            const string sql = "INSERT IGNORE INTO `librarian_info_table` (`account_id`) " +
                               "VALUES (@accountId);";

            using (MySqlCommand command = new MySqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue("@accountId", accountId);
                command.ExecuteNonQuery();
            }
        }

        // NORMALISATION: the staff profile points at the account only. The role
        // stays in account_role_table, so it is never copied into this row.
        private static void SaveOfficeProfile(
            MySqlConnection connection,
            MySqlTransaction transaction,
            string table,
            int accountId,
            string office)
        {
            string sql = "INSERT INTO `" + table + "` (`account_id`, `office`) " +
                         "VALUES (@accountId, @office) " +
                         "ON DUPLICATE KEY UPDATE `office` = @office;";

            using (MySqlCommand command = new MySqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue("@accountId", accountId);
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

        // ------------------------------------------------------------
        // NAME PROPAGATION: account_table owns the one display name of a
        // person, but student_info_table (name parts), borrowers_table
        // (name parts copied when a book is borrowed) and
        // evaluation_respondent_table (name snapshot taken when a survey is
        // submitted) keep their own copy. When the FULL NAME is edited,
        // every copy is rewritten in the same transaction, so no tab of
        // the librarian dashboard keeps showing the old spelling.
        // ------------------------------------------------------------
        private static void PropagateName(
            MySqlConnection connection,
            MySqlTransaction transaction,
            AccountRow row,
            string newName,
            string schoolId)
        {
            string updated = (newName ?? string.Empty).Trim();
            string previous = (row.Name ?? string.Empty).Trim();

            if (updated.Length == 0 || string.Equals(updated, previous, StringComparison.Ordinal))
            {
                return;
            }

            Borrower.SplitName(updated, out string lastName, out string firstName, out string middleName);
            Borrower.SplitName(previous, out string oldLastName, out string oldFirstName, out _);

            // 1. STUDENT PROFILE: student_info_table keeps the name parts next to
            //    the school details. Its middle column only holds the initial.
            if (ColumnExists(connection, transaction, "student_info_table", "last_name"))
            {
                const string studentSql =
                    "UPDATE `student_info_table` SET `last_name` = @lastName, " +
                    "`first_name` = @firstName, `middle_initial` = @middleInitial " +
                    "WHERE `account_id` = @accountId;";

                using (MySqlCommand studentCommand = new MySqlCommand(studentSql, connection, transaction))
                {
                    studentCommand.Parameters.AddWithValue("@lastName", Limit(lastName, 80));
                    studentCommand.Parameters.AddWithValue("@firstName", Limit(firstName, 80));
                    studentCommand.Parameters.Add("@middleInitial", MySqlDbType.VarChar).Value =
                        NullIfEmpty(MiddleInitial(middleName));
                    studentCommand.Parameters.AddWithValue("@accountId", row.AccountId);
                    studentCommand.ExecuteNonQuery();
                }
            }

            // 2. BORROW RECORDS: borrowers_table stores the name parts, so the
            //    ATTENDANCE tab and the TOP BORROWERS chart follow the new name.
            //    A row belongs to the person when it points at the account, when
            //    it carries the student's school id (or the ACCOUNT-<id> key used
            //    by borrow requests), or when it was saved without an id number
            //    under the old name and the same role.
            if (ColumnExists(connection, transaction, "borrowers_table", "middle_name"))
            {
                const string borrowerSql =
                    "UPDATE `borrowers_table` SET `last_name` = @lastName, " +
                    "`first_name` = @firstName, `middle_name` = @middleName " +
                    "WHERE `account_id` = @accountId " +
                    "   OR `id_number` IN (NULLIF(@schoolId, ''), NULLIF(@oldSchoolId, ''), @accountKey) " +
                    "   OR (`account_id` IS NULL AND (`id_number` IS NULL OR `id_number` = '') " +
                    "       AND `last_name` = @oldLastName AND `first_name` = @oldFirstName " +
                    "       AND `role_id` = (SELECT `role_id` FROM `role_table` " +
                    "                       WHERE `role_code` = @oldRole));";

                using (MySqlCommand borrowerCommand = new MySqlCommand(borrowerSql, connection, transaction))
                {
                    borrowerCommand.Parameters.AddWithValue("@lastName", Limit(lastName, 80));
                    borrowerCommand.Parameters.AddWithValue("@firstName", Limit(firstName, 80));
                    borrowerCommand.Parameters.Add("@middleName", MySqlDbType.VarChar).Value =
                        NullIfEmpty(Limit(middleName, 50));
                    borrowerCommand.Parameters.AddWithValue("@accountId", row.AccountId);
                    borrowerCommand.Parameters.AddWithValue("@schoolId", CleanProfileValue(schoolId));
                    borrowerCommand.Parameters.AddWithValue("@oldSchoolId", CleanProfileValue(row.SchoolId));
                    borrowerCommand.Parameters.AddWithValue(
                        "@accountKey",
                        "ACCOUNT-" + row.AccountId.ToString(CultureInfo.InvariantCulture));
                    borrowerCommand.Parameters.AddWithValue("@oldLastName", oldLastName);
                    borrowerCommand.Parameters.AddWithValue("@oldFirstName", oldFirstName);
                    borrowerCommand.Parameters.AddWithValue("@oldRole", Borrower.NormalizeRole(row.Role));
                    borrowerCommand.ExecuteNonQuery();
                }
            }

            // 3. SURVEY RESPONDENTS: the respondent row keeps the name shown by the
            //    EVALUATION tab, so it follows the new spelling.
            if (ColumnExists(connection, transaction, "evaluation_respondent_table", "respondent_name"))
            {
                const string evaluationSql =
                    "UPDATE `evaluation_respondent_table` er " +
                    "INNER JOIN `evaluation_table` e ON e.`evaluation_id` = er.`evaluation_id` " +
                    "SET er.`respondent_name` = @name " +
                    "WHERE e.`account_id` = @accountId " +
                    "   OR (e.`account_id` IS NULL AND er.`respondent_name` = @previousName);";

                using (MySqlCommand evaluationCommand = new MySqlCommand(evaluationSql, connection, transaction))
                {
                    evaluationCommand.Parameters.AddWithValue("@name", Limit(updated, 150));
                    evaluationCommand.Parameters.AddWithValue("@accountId", row.AccountId);
                    evaluationCommand.Parameters.AddWithValue("@previousName", previous);
                    evaluationCommand.ExecuteNonQuery();
                }
            }
            else if (ColumnExists(connection, transaction, "evaluation_table", "respondent_name"))
            {
                const string evaluationSql =
                    "UPDATE `evaluation_table` SET `respondent_name` = @name " +
                    "WHERE `account_id` = @accountId " +
                    "   OR (`account_id` IS NULL AND `respondent_name` = @previousName);";

                using (MySqlCommand evaluationCommand = new MySqlCommand(evaluationSql, connection, transaction))
                {
                    evaluationCommand.Parameters.AddWithValue("@name", Limit(updated, 150));
                    evaluationCommand.Parameters.AddWithValue("@accountId", row.AccountId);
                    evaluationCommand.Parameters.AddWithValue("@previousName", previous);
                    evaluationCommand.ExecuteNonQuery();
                }
            }
        }

        // CONTACT PROPAGATION: the account profile owns the phone number. When it
        // is edited, every borrower row that belongs to the person is updated, so
        // the BORROWED BOOKS tab and the reports never show an old number.
        private static void PropagateContact(
            MySqlConnection connection,
            MySqlTransaction transaction,
            AccountRow row,
            string schoolId,
            string contactNumber)
        {
            string contact = CleanProfileValue(contactNumber);

            if (contact.Length == 0 ||
                !ColumnExists(connection, transaction, "borrowers_table", "contact_number"))
            {
                return;
            }

            const string sql =
                "UPDATE `borrowers_table` SET `contact_number` = @contact " +
                "WHERE `account_id` = @accountId " +
                "   OR `id_number` IN (NULLIF(@schoolId, ''), NULLIF(@oldSchoolId, ''), @accountKey);";

            using (MySqlCommand command = new MySqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue("@contact", Limit(contact, 30));
                command.Parameters.AddWithValue("@accountId", row.AccountId);
                command.Parameters.AddWithValue("@schoolId", CleanProfileValue(schoolId));
                command.Parameters.AddWithValue("@oldSchoolId", CleanProfileValue(row.SchoolId));
                command.Parameters.AddWithValue(
                    "@accountKey",
                    "ACCOUNT-" + row.AccountId.ToString(CultureInfo.InvariantCulture));
                command.ExecuteNonQuery();
            }
        }

        // A profile value is stored as "—" by AccountRow when the role does not
        // own it, so it never reaches a WHERE clause as a real id.
        private static string CleanProfileValue(string value)
        {
            string trimmed = (value ?? string.Empty).Trim();
            return trimmed == "—" ? string.Empty : trimmed;
        }

        // student_info_table stores the middle initial only, so "VIC D." and
        // "V." both become "V" while "VIC" also becomes "V".
        private static string MiddleInitial(string middleName)
        {
            string value = (middleName ?? string.Empty).Trim();

            foreach (char letter in value)
            {
                if (char.IsLetter(letter))
                {
                    return letter.ToString().ToUpperInvariant();
                }
            }

            return string.Empty;
        }

        private static object NullIfEmpty(string value)
        {
            return value.Length == 0 ? DBNull.Value : (object)value;
        }

        // Older databases may not have the profile, borrower or survey table
        // yet. Asking information_schema keeps the name in step where the copy
        // exists and never fails the edit because a table is missing.
        private static bool ColumnExists(
            MySqlConnection connection,
            MySqlTransaction transaction,
            string tableName,
            string columnName)
        {
            const string sql =
                "SELECT COUNT(*) FROM `information_schema`.`COLUMNS` " +
                "WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = @table " +
                "AND `COLUMN_NAME` = @column;";

            using (MySqlCommand command = new MySqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue("@table", tableName);
                command.Parameters.AddWithValue("@column", columnName);
                return Convert.ToInt64(command.ExecuteScalar(), CultureInfo.InvariantCulture) > 0;
            }
        }

        private static string Limit(string value, int maximumLength)
        {
            string trimmed = (value ?? string.Empty).Trim();
            return trimmed.Length > maximumLength ? trimmed.Substring(0, maximumLength) : trimmed;
        }
    }
}
