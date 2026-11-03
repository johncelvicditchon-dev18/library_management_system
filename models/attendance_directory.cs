using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using oop_project.Database;

namespace oop_project.Models
{
    /// <summary>
    /// One entry in the library office attendance log. The librarian records it by
    /// scanning the QR pass that a student, faculty member or staff member shows
    /// at the entrance.
    /// </summary>
    public class AttendanceRow
    {
        public int AttendanceId { get; private set; }
        public int AccountId { get; private set; }
        public string Name { get; private set; }
        public string Role { get; private set; }
        public string IdNumber { get; private set; }
        public string ProgramOrOffice { get; private set; }
        public string Institute { get; private set; }
        public DateTime ScanTime { get; private set; }
        public string Gate { get; private set; }

        public AttendanceRow(
            int attendanceId,
            int accountId,
            string name,
            string role,
            string idNumber,
            string programOrOffice,
            string institute,
            DateTime scanTime,
            string gate)
        {
            AttendanceId = attendanceId;
            AccountId = accountId;
            Name = string.IsNullOrWhiteSpace(name) ? "UNNAMED ACCOUNT" : name.Trim();
            Role = string.IsNullOrWhiteSpace(role) ? "STUDENT" : role.Trim().ToUpperInvariant();
            IdNumber = string.IsNullOrWhiteSpace(idNumber) ? "—" : idNumber.Trim();
            ProgramOrOffice = string.IsNullOrWhiteSpace(programOrOffice) ? "—" : programOrOffice.Trim();
            Institute = string.IsNullOrWhiteSpace(institute) ? "—" : institute.Trim();
            ScanTime = scanTime;
            Gate = string.IsNullOrWhiteSpace(gate) ? "LIBRARY OFFICE" : gate.Trim();
        }

        public bool IsStudent
        {
            get { return Role == "STUDENT"; }
        }

        public bool IsFaculty
        {
            get { return Role == "FACULTY"; }
        }

        public bool IsStaff
        {
            get { return Role == "STAFF" || Role == "OFFICER"; }
        }

        public bool IsToday
        {
            get { return ScanTime.Date == DateTime.Today; }
        }

        public bool IsThisMonth
        {
            get { return ScanTime.Year == DateTime.Today.Year && ScanTime.Month == DateTime.Today.Month; }
        }

        public string DateText
        {
            get { return ScanTime.ToString("MMM dd, yyyy", CultureInfo.InvariantCulture).ToUpperInvariant(); }
        }

        public string TimeText
        {
            get { return ScanTime.ToString("h:mm:ss tt", CultureInfo.InvariantCulture).ToUpperInvariant(); }
        }

        public string StampText
        {
            get { return DateText + "  " + TimeText; }
        }
    }

    /// <summary>
    /// The outcome of one scan. A repeated scan inside the duplicate window is
    /// reported as AlreadyRecorded instead of creating a second visit.
    /// </summary>
    public class AttendanceScanResult
    {
        public AttendanceRow Attendance { get; private set; }
        public bool AlreadyRecorded { get; private set; }

        public AttendanceScanResult(AttendanceRow attendance, bool alreadyRecorded)
        {
            Attendance = attendance;
            AlreadyRecorded = alreadyRecorded;
        }
    }

    /// <summary>
    /// All SQL for the library office attendance log. Only the LIBRARIAN scans a
    /// pass; students, faculty and staff read their own visit history.
    ///
    /// The QR pass contains an "ACCOUNT: n" line, so the scanner (which behaves
    /// like a keyboard) can send the whole text and this directory picks the
    /// account id out of it. A bare account number is accepted as well for a
    /// manual entry at the desk.
    /// </summary>
    public class AttendanceDirectory
    {
        private const int DuplicateWindowSeconds = 60;
        private const string GateName = "LIBRARY OFFICE";

        private static readonly Regex AccountPattern =
            new Regex(@"ACCOUNT\s*[-:#]?\s*(\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static bool _schemaReady;

        private readonly IDatabaseConnection _database = new MySqlDatabaseConnection();

        // PROGRAM / OFFICE is role aware: only a STUDENT shows a program and only a
        // STAFF member shows an office. FACULTY no longer keeps a program and the
        // LIBRARIAN no longer keeps an office, so both columns show "—" for them.
        private const string ProgramOfficeSelect =
            "CASE " +
            "  WHEN COALESCE(r.`role`, 'STUDENT') = 'STUDENT' THEN COALESCE(p.`program_name`, '—') " +
            "  WHEN COALESCE(r.`role`, 'STUDENT') IN ('STAFF', 'OFFICER') THEN COALESCE(st.`office`, '—') " +
            "  ELSE '—' " +
            "END";

        private const string AttendanceSelect =
            "SELECT t.`attendance_id`, t.`account_id`, a.`name`, " +
            "       COALESCE(r.`role`, 'STUDENT'), " +
            "       COALESCE(NULLIF(s.`school_id`, ''), CONCAT('ACCOUNT-', a.`account_id`)), " +
            "       " + ProgramOfficeSelect + ", " +
            "       COALESCE(i.`institute_code`, fi.`institute_code`, '—'), " +
            "       t.`scan_time`, COALESCE(t.`gate`, 'LIBRARY OFFICE') " +
            "FROM `attendance_table` t " +
            "JOIN `account_table` a ON a.`account_id` = t.`account_id` " +
            "LEFT JOIN `account_role_table` r ON r.`account_id` = a.`account_id` " +
            "LEFT JOIN `student_info_table` s ON s.`account_id` = a.`account_id` " +
            "LEFT JOIN `program_table` p ON p.`program_id` = s.`program_id` " +
            "LEFT JOIN `institute_table` i ON i.`institute_id` = s.`institute_id` " +
            "LEFT JOIN `faculty_info_table` f ON f.`account_id` = a.`account_id` " +
            "LEFT JOIN `institute_table` fi ON fi.`institute_id` = f.`institute_id` " +
            "LEFT JOIN `staff_info_table` st ON st.`account_id` = a.`account_id` ";

        /// <summary>
        /// Records one entrance scan. The QR text is parsed for the account id,
        /// the account is checked, and a visit is written with the server clock.
        /// </summary>
        public AttendanceScanResult RecordEntry(string scannedCode)
        {
            int accountId = ParseAccountId(scannedCode);

            using (MySqlConnection connection = _database.CreateConnection())
            {
                connection.Open();
                EnsureSchema(connection);

                AccountSnapshot account = LoadAccount(connection, accountId);

                if (!string.Equals(account.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        "THE ACCOUNT OF " + account.Name.ToUpperInvariant() + " IS " + account.Status + " AND CANNOT BE RECORDED.");
                }

                AttendanceRow? recent = LoadRecent(connection, accountId);
                if (recent != null)
                {
                    return new AttendanceScanResult(recent, true);
                }

                const string insertSql =
                    "INSERT INTO `attendance_table` (`account_id`, `scan_time`, `role`, `gate`) " +
                    "VALUES (@accountId, NOW(), @role, @gate);";

                using (MySqlCommand command = new MySqlCommand(insertSql, connection))
                {
                    command.Parameters.AddWithValue("@accountId", accountId);
                    command.Parameters.AddWithValue("@role", account.Role);
                    command.Parameters.AddWithValue("@gate", GateName);
                    command.ExecuteNonQuery();

                    AttendanceRow row = new AttendanceRow(
                        (int)command.LastInsertedId,
                        account.AccountId,
                        account.Name,
                        account.Role,
                        account.IdNumber,
                        account.ProgramOrOffice,
                        account.Institute,
                        DateTime.Now,
                        GateName);

                    return new AttendanceScanResult(row, false);
                }
            }
        }

        /// <summary>The signed in patron's own visit history, newest first.</summary>
        public List<AttendanceRow> LoadForAccount(int accountId)
        {
            using (MySqlConnection connection = _database.CreateConnection())
            {
                connection.Open();
                EnsureSchema(connection);

                using (MySqlCommand command = new MySqlCommand(
                    AttendanceSelect + "WHERE t.`account_id` = @accountId ORDER BY t.`scan_time` DESC, t.`attendance_id` DESC LIMIT 300;",
                    connection))
                {
                    command.Parameters.AddWithValue("@accountId", accountId);
                    return ReadRows(command);
                }
            }
        }

        /// <summary>
        /// The librarian's full attendance log. The optional search term is applied
        /// by MySQL across the visible columns (name, role, id, office, date, gate).
        /// </summary>
        public List<AttendanceRow> LoadAll(string? search = null)
        {
            string term = string.IsNullOrWhiteSpace(search) ? string.Empty : search.Trim();

            string sql = AttendanceSelect;

            if (term.Length > 0)
            {
                sql += "WHERE (a.`name` LIKE @search OR " +
                       "       COALESCE(r.`role`, 'STUDENT') LIKE @search OR " +
                       "       COALESCE(NULLIF(s.`school_id`, ''), CONCAT('ACCOUNT-', a.`account_id`)) LIKE @search OR " +
                       "       " + ProgramOfficeSelect + " LIKE @search OR " +
                       "       COALESCE(i.`institute_code`, fi.`institute_code`, '—') LIKE @search OR " +
                       "       COALESCE(t.`gate`, '') LIKE @search OR " +
                       "       DATE_FORMAT(t.`scan_time`, '%M %e, %Y %h:%i %p') LIKE @search) ";
            }

            sql += "ORDER BY t.`scan_time` DESC, t.`attendance_id` DESC LIMIT 500;";

            using (MySqlConnection connection = _database.CreateConnection())
            {
                connection.Open();
                EnsureSchema(connection);

                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    if (term.Length > 0)
                    {
                        command.Parameters.AddWithValue("@search", "%" + term + "%");
                    }

                    return ReadRows(command);
                }
            }
        }

        /// <summary>
        /// Creates the attendance table on first use, so the feature also works on
        /// a database that was imported before this table was added to the script.
        /// </summary>
        private static void EnsureSchema(MySqlConnection connection)
        {
            if (_schemaReady)
            {
                return;
            }

            const string sql =
                "CREATE TABLE IF NOT EXISTS `attendance_table` (" +
                "  `attendance_id` INT(11) NOT NULL AUTO_INCREMENT," +
                "  `account_id` INT(11) NOT NULL," +
                "  `scan_time` DATETIME NOT NULL," +
                "  `role` VARCHAR(20) NOT NULL," +
                "  `gate` VARCHAR(60) NOT NULL DEFAULT 'LIBRARY OFFICE'," +
                "  PRIMARY KEY (`attendance_id`)," +
                "  KEY `idx_attendance_account` (`account_id`)," +
                "  KEY `idx_attendance_time` (`scan_time`)," +
                "  CONSTRAINT `fk_attendance_account`" +
                "      FOREIGN KEY (`account_id`) REFERENCES `account_table` (`account_id`)" +
                "      ON DELETE CASCADE" +
                ") ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;";

            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }

            _schemaReady = true;
        }

        private static int ParseAccountId(string scannedCode)
        {
            string value = (scannedCode ?? string.Empty).Trim();

            if (value.Length == 0)
            {
                throw new ArgumentException("SCAN A QR PASS OR TYPE AN ACCOUNT NUMBER FIRST.");
            }

            // A manual entry at the desk is just the account number.
            if (Regex.IsMatch(value, @"^\d+$"))
            {
                return int.Parse(value, CultureInfo.InvariantCulture);
            }

            Match match = AccountPattern.Match(value);
            if (match.Success)
            {
                int parsed;
                if (int.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed) && parsed > 0)
                {
                    return parsed;
                }
            }

            throw new ArgumentException("THAT IS NOT A TCGC LIBRARY ATTENDANCE QR PASS.");
        }

        private static AccountSnapshot LoadAccount(MySqlConnection connection, int accountId)
        {
            const string sql =
                "SELECT a.`account_id`, a.`name`, COALESCE(r.`role`, 'STUDENT'), COALESCE(r.`status`, 'PENDING'), " +
                "       COALESCE(NULLIF(s.`school_id`, ''), CONCAT('ACCOUNT-', a.`account_id`)), " +
                "       " + ProgramOfficeSelect + ", " +
                "       COALESCE(i.`institute_code`, fi.`institute_code`, '—') " +
                "FROM `account_table` a " +
                "LEFT JOIN `account_role_table` r ON r.`account_id` = a.`account_id` " +
                "LEFT JOIN `student_info_table` s ON s.`account_id` = a.`account_id` " +
                "LEFT JOIN `program_table` p ON p.`program_id` = s.`program_id` " +
                "LEFT JOIN `institute_table` i ON i.`institute_id` = s.`institute_id` " +
                "LEFT JOIN `faculty_info_table` f ON f.`account_id` = a.`account_id` " +
                "LEFT JOIN `institute_table` fi ON fi.`institute_id` = f.`institute_id` " +
                "LEFT JOIN `staff_info_table` st ON st.`account_id` = a.`account_id` " +
                "WHERE a.`account_id` = @accountId LIMIT 1;";

            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@accountId", accountId);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        throw new InvalidOperationException("THAT ATTENDANCE PASS DOES NOT BELONG TO A LIBRARY ACCOUNT.");
                    }

                    return new AccountSnapshot(
                        reader.GetInt32(0),
                        Text(reader, 1),
                        Text(reader, 2, "STUDENT"),
                        Text(reader, 3, "PENDING"),
                        Text(reader, 4),
                        Text(reader, 5),
                        Text(reader, 6));
                }
            }
        }

        private static AttendanceRow? LoadRecent(MySqlConnection connection, int accountId)
        {
            string sql = AttendanceSelect +
                "WHERE t.`account_id` = @accountId AND " +
                "      t.`scan_time` >= DATE_SUB(NOW(), INTERVAL " +
                DuplicateWindowSeconds.ToString(CultureInfo.InvariantCulture) + " SECOND) " +
                "ORDER BY t.`scan_time` DESC LIMIT 1;";

            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@accountId", accountId);
                List<AttendanceRow> rows = ReadRows(command);
                return rows.Count == 0 ? null : rows[0];
            }
        }

        private static List<AttendanceRow> ReadRows(MySqlCommand command)
        {
            List<AttendanceRow> rows = new List<AttendanceRow>();

            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    rows.Add(new AttendanceRow(
                        reader.GetInt32(0),
                        reader.GetInt32(1),
                        Text(reader, 2),
                        Text(reader, 3, "STUDENT"),
                        Text(reader, 4),
                        Text(reader, 5),
                        Text(reader, 6),
                        reader.GetDateTime(7),
                        Text(reader, 8, GateName)));
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

        private sealed class AccountSnapshot
        {
            public int AccountId { get; private set; }
            public string Name { get; private set; }
            public string Role { get; private set; }
            public string Status { get; private set; }
            public string IdNumber { get; private set; }
            public string ProgramOrOffice { get; private set; }
            public string Institute { get; private set; }

            public AccountSnapshot(
                int accountId, string name, string role, string status,
                string idNumber, string programOrOffice, string institute)
            {
                AccountId = accountId;
                Name = name;
                Role = role;
                Status = status;
                IdNumber = idNumber;
                ProgramOrOffice = programOrOffice;
                Institute = institute;
            }
        }
    }
}
