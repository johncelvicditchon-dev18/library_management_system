using System;
using System.Collections.Generic;
using System.Globalization;
using MySql.Data.MySqlClient;
using oop_project.Database;

namespace oop_project.Models
{
    /// <summary>
    /// Stores the LIBRARY RESOURCE CENTER EVALUATION FORM responses. One row in
    /// evaluation_table records the submission and optional account, one row in
    /// evaluation_respondent_table records the role and name snapshot, and one row
    /// in evaluation_answer_table keeps each 1-5 rating, so the answers stay
    /// normalised and can be averaged per question or per section.
    ///
    /// The survey is opened and closed by the librarian per semester, so every
    /// submission is tagged with the period (evaluation_period_table) it belongs
    /// to and one account may answer only once inside the same period.
    /// </summary>
    public class EvaluationDirectory
    {
        public const int SectionCount = 5;
        public const int QuestionsPerSection = 5;
        public const int QuestionCount = SectionCount * QuestionsPerSection;

        private static bool _schemaReady;

        private readonly IDatabaseConnection _database = new MySqlDatabaseConnection();

        public void Submit(string respondentRole, string respondentName, int[] ratings, int accountId = 0, int periodId = 0)
        {
            if (string.IsNullOrWhiteSpace(respondentRole))
            {
                throw new ArgumentException("A respondent role is required.", nameof(respondentRole));
            }

            if (ratings == null || ratings.Length != QuestionCount)
            {
                throw new ArgumentException("All " + QuestionCount + " statements must be rated.", nameof(ratings));
            }

            for (int index = 0; index < ratings.Length; index++)
            {
                if (ratings[index] < 1 || ratings[index] > 5)
                {
                    throw new ArgumentException("Every rating must be between 1 and 5.", nameof(ratings));
                }
            }

            string role = respondentRole.Trim().ToUpperInvariant();
            string name = (respondentName ?? string.Empty).Trim();
            name = name.Length > 150 ? name.Substring(0, 150) : name;

            using (MySqlConnection connection = _database.CreateConnection())
            {
                connection.Open();
                EnsureSchema(connection);

                // The survey only accepts answers while the library keeps the
                // semester open, and only once per account inside that semester.
                EvaluationPeriod period = ResolveOpenPeriod(connection, periodId);

                if (accountId > 0 && HasResponded(connection, accountId, period.PeriodId))
                {
                    throw new InvalidOperationException(
                        "You have already submitted an evaluation for " + period.Title +
                        ". The form will be available again next semester.");
                }

                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    long evaluationId;

                    const string evaluationSql =
                        "INSERT INTO `evaluation_table` (`account_id`, `period_id`, `submitted_at`) " +
                        "VALUES (@accountId, @periodId, @submittedAt);";

                    using (MySqlCommand command = new MySqlCommand(evaluationSql, connection, transaction))
                    {
                        command.Parameters.AddWithValue(
                            "@accountId",
                            accountId > 0 ? (object)accountId : DBNull.Value);
                        command.Parameters.AddWithValue("@periodId", period.PeriodId);
                        command.Parameters.AddWithValue("@submittedAt", DateTime.Now);
                        command.ExecuteNonQuery();
                        evaluationId = command.LastInsertedId;
                    }

                    const string respondentSql =
                        "INSERT INTO `evaluation_respondent_table` (`evaluation_id`, `role_id`, `respondent_name`) " +
                        "VALUES (@evaluationId, " +
                        "        (SELECT `role_id` FROM `role_table` WHERE `role_code` = @role), @name);";

                    using (MySqlCommand command = new MySqlCommand(respondentSql, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@evaluationId", evaluationId);
                        command.Parameters.AddWithValue("@role", role);
                        command.Parameters.AddWithValue("@name", name);
                        command.ExecuteNonQuery();
                    }

                    const string answerSql =
                        "INSERT INTO `evaluation_answer_table` (`evaluation_id`, `question_id`, `rating`) " +
                        "VALUES (@evaluationId, @questionId, @rating);";

                    using (MySqlCommand command = new MySqlCommand(answerSql, connection, transaction))
                    {
                        command.Parameters.Add("@evaluationId", MySqlDbType.Int32);
                        command.Parameters.Add("@questionId", MySqlDbType.Int32);
                        command.Parameters.Add("@rating", MySqlDbType.Int32);

                        for (int index = 0; index < ratings.Length; index++)
                        {
                            command.Parameters["@evaluationId"].Value = evaluationId;
                            command.Parameters["@questionId"].Value = index + 1;
                            command.Parameters["@rating"].Value = ratings[index];
                            command.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
            }
        }

        // ------------------------------------------------------------------ PERIODS

        /// <summary>
        /// The semester the librarian has open right now, or null when the survey
        /// is closed. Students, faculty and staff may only answer while it exists.
        /// </summary>
        public EvaluationPeriod? GetOpenPeriod()
        {
            using (MySqlConnection connection = _database.CreateConnection())
            {
                connection.Open();
                EnsureSchema(connection);
                return ReadOpenPeriod(connection);
            }
        }

        /// <summary>
        /// Every semester that was ever opened, newest first, with the open one on
        /// top. The ANALYTICS tab lists them so past semesters stay reviewable.
        /// </summary>
        public List<EvaluationPeriod> LoadPeriods()
        {
            List<EvaluationPeriod> periods = new List<EvaluationPeriod>();

            using (MySqlConnection connection = _database.CreateConnection())
            {
                connection.Open();
                EnsureSchema(connection);

                const string sql =
                    "SELECT `period_id`, `semester`, `school_year`, `is_open`, `opened_at`, `closed_at` " +
                    "FROM `evaluation_period_table` " +
                    "ORDER BY `is_open` DESC, `opened_at` DESC, `period_id` DESC;";

                using (MySqlCommand command = new MySqlCommand(sql, connection))
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        periods.Add(ReadPeriod(reader));
                    }
                }
            }

            return periods;
        }

        /// <summary>
        /// Only the semesters the librarian currently allows answers for, newest
        /// first. The evaluation sign in pop up lists them so a respondent can see
        /// (and pick) which semester the answers will belong to. Normally there is
        /// exactly one, because opening a semester closes the previous one.
        /// </summary>
        public List<EvaluationPeriod> LoadOpenPeriods()
        {
            List<EvaluationPeriod> periods = new List<EvaluationPeriod>();

            using (MySqlConnection connection = _database.CreateConnection())
            {
                connection.Open();
                EnsureSchema(connection);

                const string sql =
                    "SELECT `period_id`, `semester`, `school_year`, `is_open`, `opened_at`, `closed_at` " +
                    "FROM `evaluation_period_table` " +
                    "WHERE `is_open` = 1 " +
                    "ORDER BY `opened_at` DESC, `period_id` DESC;";

                using (MySqlCommand command = new MySqlCommand(sql, connection))
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        periods.Add(ReadPeriod(reader));
                    }
                }
            }

            return periods;
        }

        /// <summary>
        /// Opens a semester for evaluation and closes whichever semester was open
        /// before it, because only one period may collect answers at a time.
        /// </summary>
        public EvaluationPeriod OpenPeriod(string semester, string schoolYear)
        {
            string normalizedSemester = (semester ?? string.Empty).Trim().ToUpperInvariant();
            string normalizedYear = (schoolYear ?? string.Empty).Trim();

            if (normalizedSemester.Length == 0 || normalizedYear.Length == 0)
            {
                throw new ArgumentException("A semester and a school year are both required.");
            }

            using (MySqlConnection connection = _database.CreateConnection())
            {
                connection.Open();
                EnsureSchema(connection);

                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    const string closeSql =
                        "UPDATE `evaluation_period_table` " +
                        "SET `closed_at` = @now " +
                        "WHERE `is_open` = 1;";

                    using (MySqlCommand command = new MySqlCommand(closeSql, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@now", DateTime.Now);
                        command.ExecuteNonQuery();
                    }

                    int periodId = FindPeriodId(connection, transaction, normalizedSemester, normalizedYear);

                    if (periodId > 0)
                    {
                        const string reopenSql =
                            "UPDATE `evaluation_period_table` " +
                            "SET `opened_at` = @now, `closed_at` = NULL " +
                            "WHERE `period_id` = @periodId;";

                        using (MySqlCommand command = new MySqlCommand(reopenSql, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@now", DateTime.Now);
                            command.Parameters.AddWithValue("@periodId", periodId);
                            command.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        const string insertSql =
                            "INSERT INTO `evaluation_period_table` (`semester`, `school_year`, `opened_at`) " +
                            "VALUES (@semester, @schoolYear, @now);";

                        using (MySqlCommand command = new MySqlCommand(insertSql, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@semester", normalizedSemester);
                            command.Parameters.AddWithValue("@schoolYear", normalizedYear);
                            command.Parameters.AddWithValue("@now", DateTime.Now);
                            command.ExecuteNonQuery();
                            periodId = (int)command.LastInsertedId;
                        }
                    }

                    transaction.Commit();

                    EvaluationPeriod period = new EvaluationPeriod
                    {
                        PeriodId = periodId,
                        Semester = normalizedSemester,
                        SchoolYear = normalizedYear,
                        IsOpen = true,
                        OpenedAt = DateTime.Now
                    };

                    return period;
                }
            }
        }

        /// <summary>Stops collecting answers for a semester without deleting them.</summary>
        public void ClosePeriod(int periodId)
        {
            if (periodId <= 0)
            {
                return;
            }

            using (MySqlConnection connection = _database.CreateConnection())
            {
                connection.Open();
                EnsureSchema(connection);

                const string sql =
                    "UPDATE `evaluation_period_table` " +
                    "SET `closed_at` = @now " +
                    "WHERE `period_id` = @periodId AND `is_open` = 1;";

                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@now", DateTime.Now);
                    command.Parameters.AddWithValue("@periodId", periodId);
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// True when the account already answered the survey inside that semester,
        /// which is what keeps one student to one evaluation per period.
        /// </summary>
        public bool HasResponded(int accountId, int periodId)
        {
            if (accountId <= 0 || periodId <= 0)
            {
                return false;
            }

            using (MySqlConnection connection = _database.CreateConnection())
            {
                connection.Open();
                EnsureSchema(connection);
                return HasResponded(connection, accountId, periodId);
            }
        }

        private static bool HasResponded(MySqlConnection connection, int accountId, int periodId)
        {
            const string sql =
                "SELECT COUNT(*) FROM `evaluation_table` " +
                "WHERE `account_id` = @accountId AND `period_id` = @periodId;";

            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@accountId", accountId);
                command.Parameters.AddWithValue("@periodId", periodId);

                object? result = command.ExecuteScalar();

                return result != null &&
                       result != DBNull.Value &&
                       Convert.ToInt32(result, CultureInfo.InvariantCulture) > 0;
            }
        }

        private static EvaluationPeriod? ReadOpenPeriod(MySqlConnection connection)
        {
            const string sql =
                "SELECT `period_id`, `semester`, `school_year`, `is_open`, `opened_at`, `closed_at` " +
                "FROM `evaluation_period_table` " +
                "WHERE `is_open` = 1 " +
                "ORDER BY `period_id` DESC LIMIT 1;";

            using (MySqlCommand command = new MySqlCommand(sql, connection))
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                return reader.Read() ? ReadPeriod(reader) : null;
            }
        }

        private static EvaluationPeriod ReadPeriod(MySqlDataReader reader)
        {
            int periodId = reader.IsDBNull(reader.GetOrdinal("period_id"))
                ? 0
                : reader.GetInt32("period_id");

            string semester = reader.IsDBNull(reader.GetOrdinal("semester"))
                ? string.Empty
                : reader.GetString("semester");

            string schoolYear = reader.IsDBNull(reader.GetOrdinal("school_year"))
                ? string.Empty
                : reader.GetString("school_year");

            bool isOpen = false;
            int openOrdinal = reader.GetOrdinal("is_open");

            if (!reader.IsDBNull(openOrdinal))
            {
                isOpen = Convert.ToBoolean(reader.GetValue(openOrdinal), CultureInfo.InvariantCulture);
            }

            DateTime? openedAt = reader.IsDBNull(reader.GetOrdinal("opened_at"))
                ? (DateTime?)null
                : reader.GetDateTime("opened_at");

            DateTime? closedAt = reader.IsDBNull(reader.GetOrdinal("closed_at"))
                ? (DateTime?)null
                : reader.GetDateTime("closed_at");

            return new EvaluationPeriod
            {
                PeriodId = periodId,
                Semester = semester.Trim(),
                SchoolYear = schoolYear.Trim(),
                IsOpen = isOpen,
                OpenedAt = openedAt,
                ClosedAt = closedAt
            };
        }

        private static int FindPeriodId(
            MySqlConnection connection, MySqlTransaction transaction, string semester, string schoolYear)
        {
            const string sql =
                "SELECT `period_id` FROM `evaluation_period_table` " +
                "WHERE `semester` = @semester AND `school_year` = @schoolYear LIMIT 1;";

            using (MySqlCommand command = new MySqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue("@semester", semester);
                command.Parameters.AddWithValue("@schoolYear", schoolYear);

                object? result = command.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                {
                    return 0;
                }

                return Convert.ToInt32(result, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Makes sure the survey really is open for the semester the form was
        /// started with. A period that was closed or replaced while the form sat
        /// on screen is refused instead of silently saving under the wrong term.
        /// </summary>
        private static EvaluationPeriod ResolveOpenPeriod(MySqlConnection connection, int requestedPeriodId)
        {
            EvaluationPeriod? open = ReadOpenPeriod(connection);

            if (open == null)
            {
                throw new InvalidOperationException(
                    "The library evaluation is currently closed. Please wait for the library to open it for this semester.");
            }

            if (requestedPeriodId > 0 && requestedPeriodId != open.PeriodId)
            {
                throw new InvalidOperationException(
                    "The semester you started this survey with is no longer open. Please open the evaluation form again.");
            }

            return open;
        }

        /// <summary>
        /// ANALYTICS: the people who answered one semester, listed the way the
        /// RESPONDENTS window shows them. The institute and program come from the
        /// student or faculty profile and the office comes from the staff profile,
        /// so a STAFF row shows its office instead of an institute. The student id
        /// is only filled for a STUDENT account.
        /// </summary>
        public List<EvaluationRespondent> LoadRespondents(int periodId)
        {
            List<EvaluationRespondent> respondents = new List<EvaluationRespondent>();

            if (periodId <= 0)
            {
                return respondents;
            }

            using (MySqlConnection connection = _database.CreateConnection())
            {
                connection.Open();
                EnsureSchema(connection);

                const string sql =
                    "SELECT er.`respondent_name`, rt.`role_code` AS `respondent_role`, " +
                    "       IFNULL(si.`school_id`, '') AS `school_id`, " +
                    "       IFNULL(p.`program_name`, '') AS `program_name`, " +
                    "       IFNULL(i.`institute_name`, '') AS `institute_name`, " +
                    "       IFNULL(st.`office`, '') AS `office` " +
                    "FROM `evaluation_table` e " +
                    "INNER JOIN `evaluation_respondent_table` er ON er.`evaluation_id` = e.`evaluation_id` " +
                    "LEFT JOIN `role_table` rt ON rt.`role_id` = er.`role_id` " +
                    "LEFT JOIN `student_info_table` si ON si.`account_id` = e.`account_id` " +
                    "LEFT JOIN `faculty_info_table` fi ON fi.`account_id` = e.`account_id` " +
                    "LEFT JOIN `staff_info_table` st ON st.`account_id` = e.`account_id` " +
                    "LEFT JOIN `program_table` p ON p.`program_id` = si.`program_id` " +
                    "LEFT JOIN `institute_table` i " +
                    "       ON i.`institute_id` = COALESCE(p.`institute_id`, fi.`institute_id`) " +
                    "WHERE e.`period_id` = @periodId " +
                    "ORDER BY CASE rt.`role_code` " +
                    "             WHEN 'STUDENT' THEN 1 " +
                    "             WHEN 'FACULTY' THEN 2 " +
                    "             ELSE 3 " +
                    "         END, er.`respondent_name`;";

                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@periodId", periodId);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string role = ReadText(reader, "respondent_role").ToUpperInvariant();

                            respondents.Add(new EvaluationRespondent
                            {
                                Name = ReadText(reader, "respondent_name"),
                                Role = role,
                                StudentId = ReadText(reader, "school_id"),
                                Program = ReadText(reader, "program_name"),
                                Institute = ReadText(reader, "institute_name"),
                                Office = ReadText(reader, "office")
                            });
                        }
                    }
                }
            }

            return respondents;
        }

        private static string ReadText(MySqlDataReader reader, string column)
        {
            int ordinal = reader.GetOrdinal(column);

            return reader.IsDBNull(ordinal)
                ? string.Empty
                : (Convert.ToString(reader.GetValue(ordinal), CultureInfo.InvariantCulture) ?? string.Empty).Trim();
        }

        /// <summary>
        /// ANALYTICS: reads every submitted evaluation and returns the figures the
        /// librarian dashboard charts need: how many students, faculty and staff
        /// answered, the overall average score, the average per section and the
        /// number of times each 1-5 rating was given.
        ///
        /// Passing a period id limits every figure to that semester, which is how
        /// the ANALYTICS tab reads it. A null id keeps the all time totals and is
        /// only a safety fallback; the dashboard always passes a semester.
        /// </summary>
        public EvaluationSummary LoadSummary(int? periodId = null)
        {
            EvaluationSummary summary = new EvaluationSummary();

            using (MySqlConnection connection = _database.CreateConnection())
            {
                connection.Open();

                try
                {
                    EnsureSchema(connection);
                }
                catch (MySqlException)
                {
                    // A read-only account cannot create the survey tables. If they
                    // already exist the SELECTs below still work, so the analytics
                    // stay available instead of failing on the CREATE statement.
                }

                LoadRespondentCounts(connection, summary, periodId);
                LoadAnswerCounts(connection, summary, periodId);
                LoadSectionAverages(connection, summary, periodId);
            }

            summary.OverallAverage = summary.TotalAnswers == 0
                ? 0
                : summary.RatingTotal / (double)summary.TotalAnswers;

            return summary;
        }

        private static void LoadRespondentCounts(
            MySqlConnection connection, EvaluationSummary summary, int? periodId)
        {
            string sql =
                "SELECT rt.`role_code`, COUNT(*) AS `total` " +
                "FROM `evaluation_table` e " +
                "INNER JOIN `evaluation_respondent_table` er ON er.`evaluation_id` = e.`evaluation_id` " +
                "INNER JOIN `role_table` rt ON rt.`role_id` = er.`role_id` " +
                (periodId.HasValue ? "WHERE e.`period_id` = @periodId " : string.Empty) +
                "GROUP BY rt.`role_code`;";

            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                if (periodId.HasValue)
                {
                    command.Parameters.AddWithValue("@periodId", periodId.Value);
                }

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string role = reader.IsDBNull(0)
                            ? string.Empty
                            : (Convert.ToString(reader.GetValue(0), CultureInfo.InvariantCulture) ?? string.Empty)
                                .Trim()
                                .ToUpperInvariant();

                        int total = reader.IsDBNull(1)
                            ? 0
                            : Convert.ToInt32(reader.GetValue(1), CultureInfo.InvariantCulture);

                        summary.TotalResponses += total;

                        switch (role)
                        {
                            case "STUDENT":
                                summary.StudentResponses += total;
                                break;
                            case "FACULTY":
                                summary.FacultyResponses += total;
                                break;
                            case "STAFF":
                                summary.StaffResponses += total;
                                break;
                            default:
                                summary.OtherResponses += total;
                                break;
                        }
                    }
                }
            }
        }

        private static void LoadAnswerCounts(
            MySqlConnection connection, EvaluationSummary summary, int? periodId)
        {
            string sql =
                "SELECT a.`rating`, COUNT(*) AS `total` " +
                "FROM `evaluation_answer_table` a " +
                "INNER JOIN `evaluation_table` e ON e.`evaluation_id` = a.`evaluation_id` " +
                (periodId.HasValue ? "WHERE e.`period_id` = @periodId " : string.Empty) +
                "GROUP BY a.`rating`;";

            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                if (periodId.HasValue)
                {
                    command.Parameters.AddWithValue("@periodId", periodId.Value);
                }

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.IsDBNull(0) || reader.IsDBNull(1))
                        {
                            continue;
                        }

                        int rating = Convert.ToInt32(reader.GetValue(0), CultureInfo.InvariantCulture);
                        int total = Convert.ToInt32(reader.GetValue(1), CultureInfo.InvariantCulture);

                        if (rating < 1 || rating > 5 || total <= 0)
                        {
                            continue;
                        }

                        summary.RatingCounts[rating] += total;
                        summary.TotalAnswers += total;
                        summary.RatingTotal += (long)rating * total;
                    }
                }
            }
        }

        private static void LoadSectionAverages(
            MySqlConnection connection, EvaluationSummary summary, int? periodId)
        {
            string sql =
                "SELECT q.`section_no`, AVG(a.`rating`) AS `average_rating`, COUNT(*) AS `answers` " +
                "FROM `evaluation_answer_table` a " +
                "INNER JOIN `evaluation_table` e ON e.`evaluation_id` = a.`evaluation_id` " +
                "INNER JOIN `evaluation_question_table` q ON q.`question_id` = a.`question_id` " +
                (periodId.HasValue ? "WHERE e.`period_id` = @periodId " : string.Empty) +
                "GROUP BY q.`section_no` " +
                "ORDER BY q.`section_no`;";

            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                if (periodId.HasValue)
                {
                    command.Parameters.AddWithValue("@periodId", periodId.Value);
                }

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.IsDBNull(0))
                        {
                            continue;
                        }

                        int section = Convert.ToInt32(reader.GetValue(0), CultureInfo.InvariantCulture) - 1;

                        if (section < 0 || section >= SectionCount)
                        {
                            continue;
                        }

                        summary.SectionAverages[section] = reader.IsDBNull(1)
                            ? 0
                            : Convert.ToDouble(reader.GetValue(1), CultureInfo.InvariantCulture);

                        summary.SectionAnswerCounts[section] = reader.IsDBNull(2)
                            ? 0
                            : Convert.ToInt32(reader.GetValue(2), CultureInfo.InvariantCulture);
                    }
                }
            }
        }

        // The survey tables are created the first time a response is saved, so an
        // older database keeps working without a manual SQL import. The dashboard
        // still ships the CREATE TABLE statements in database/libsys_database.sql.
        private static void EnsureSchema(MySqlConnection connection)
        {
            if (_schemaReady)
            {
                return;
            }

            const string periodSql =
                "CREATE TABLE IF NOT EXISTS `evaluation_period_table` (" +
                "`period_id` INT AUTO_INCREMENT PRIMARY KEY, " +
                "`semester` VARCHAR(40) NOT NULL, " +
                "`school_year` VARCHAR(20) NOT NULL, " +
                "`opened_at` DATETIME NULL, " +
                "`closed_at` DATETIME NULL, " +
                "`is_open` TINYINT(1) AS (CASE WHEN `opened_at` IS NOT NULL AND `closed_at` IS NULL THEN 1 ELSE 0 END) STORED, " +
                "`open_marker` TINYINT AS (CASE WHEN `opened_at` IS NOT NULL AND `closed_at` IS NULL THEN 1 ELSE NULL END) STORED, " +
                "UNIQUE KEY `uq_evaluation_period` (`semester`, `school_year`), " +
                "UNIQUE KEY `ux_single_open_period` (`open_marker`)" +
                ") ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;";

            const string questionSql =
                "CREATE TABLE IF NOT EXISTS `evaluation_question_table` (" +
                "`question_id` INT AUTO_INCREMENT PRIMARY KEY, " +
                "`section_no` INT NOT NULL, " +
                "`question_no` INT NOT NULL, " +
                "`question_text` VARCHAR(255) NOT NULL, " +
                "UNIQUE KEY `ux_evaluation_question` (`section_no`, `question_no`)" +
                ") ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;";

            const string evaluationSql =
                "CREATE TABLE IF NOT EXISTS `evaluation_table` (" +
                "`evaluation_id` INT AUTO_INCREMENT PRIMARY KEY, " +
                "`account_id` INT NULL, " +
                "`period_id` INT NOT NULL, " +
                "`submitted_at` DATETIME NOT NULL, " +
                "UNIQUE KEY `uq_evaluation_period_account` (`period_id`, `account_id`), " +
                "KEY `idx_evaluation_account` (`account_id`), " +
                "KEY `idx_evaluation_period` (`period_id`), " +
                "CONSTRAINT `fk_evaluation_account` FOREIGN KEY (`account_id`) " +
                "REFERENCES `account_table` (`account_id`) ON DELETE SET NULL, " +
                "CONSTRAINT `fk_evaluation_period` FOREIGN KEY (`period_id`) " +
                "REFERENCES `evaluation_period_table` (`period_id`)" +
                ") ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;";

            const string respondentSql =
                "CREATE TABLE IF NOT EXISTS `evaluation_respondent_table` (" +
                "`evaluation_id` INT PRIMARY KEY, " +
                "`role_id` INT NOT NULL, " +
                "`respondent_name` VARCHAR(150) NOT NULL DEFAULT '', " +
                "KEY `idx_eval_respondent_role` (`role_id`), " +
                "CONSTRAINT `fk_eval_respondent_evaluation` FOREIGN KEY (`evaluation_id`) " +
                "REFERENCES `evaluation_table` (`evaluation_id`) ON DELETE CASCADE, " +
                "CONSTRAINT `fk_eval_respondent_role` FOREIGN KEY (`role_id`) " +
                "REFERENCES `role_table` (`role_id`)" +
                ") ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;";

            const string answerSql =
                "CREATE TABLE IF NOT EXISTS `evaluation_answer_table` (" +
                "`answer_id` INT AUTO_INCREMENT PRIMARY KEY, " +
                "`evaluation_id` INT NOT NULL, " +
                "`question_id` INT NOT NULL, " +
                "`rating` TINYINT NOT NULL, " +
                "UNIQUE KEY `ux_answer_evaluation_question` (`evaluation_id`, `question_id`), " +
                "KEY `idx_answer_evaluation` (`evaluation_id`), " +
                "KEY `idx_answer_question` (`question_id`), " +
                "CONSTRAINT `fk_answer_evaluation` FOREIGN KEY (`evaluation_id`) " +
                "REFERENCES `evaluation_table` (`evaluation_id`) ON DELETE CASCADE, " +
                "CONSTRAINT `fk_answer_question` FOREIGN KEY (`question_id`) " +
                "REFERENCES `evaluation_question_table` (`question_id`), " +
                "CONSTRAINT `chk_answer_rating` CHECK (`rating` BETWEEN 1 AND 5)" +
                ") ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;";

            Execute(connection, periodSql);
            Execute(connection, questionSql);
            Execute(connection, evaluationSql);
            Execute(connection, respondentSql);
            Execute(connection, answerSql);

            UpgradePeriodSchema(connection);
            UpgradeRespondentSchema(connection);

            // The 25 statements are reference data. The main schema file stores the
            // real wording; this fallback only makes sure the ids exist.
            for (int index = 1; index <= QuestionCount; index++)
            {
                using (MySqlCommand command = new MySqlCommand(
                    "INSERT IGNORE INTO `evaluation_question_table` " +
                    "(`question_id`, `section_no`, `question_no`, `question_text`) " +
                    "VALUES (@id, @section, @question, @text);", connection))
                {
                    command.Parameters.AddWithValue("@id", index);
                    command.Parameters.AddWithValue("@section", ((index - 1) / QuestionsPerSection) + 1);
                    command.Parameters.AddWithValue("@question", ((index - 1) % QuestionsPerSection) + 1);
                    command.Parameters.AddWithValue("@text", "QUESTION " + index);
                    command.ExecuteNonQuery();
                }
            }

            _schemaReady = true;
        }

        private static void Execute(MySqlConnection connection, string sql)
        {
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void UpgradePeriodSchema(MySqlConnection connection)
        {
            if (!ColumnIsGenerated(connection, "evaluation_period_table", "is_open"))
            {
                const string closeExtraSql =
                    "UPDATE `evaluation_period_table` SET `closed_at` = NOW() " +
                    "WHERE `opened_at` IS NOT NULL AND `closed_at` IS NULL " +
                    "AND `period_id` <> (SELECT `period_id` FROM (" +
                    "    SELECT `period_id` FROM `evaluation_period_table` " +
                    "    WHERE `opened_at` IS NOT NULL AND `closed_at` IS NULL " +
                    "    ORDER BY `opened_at` DESC, `period_id` DESC LIMIT 1" +
                    ") AS `latest_open_period`);";

                Execute(connection, closeExtraSql);
                Execute(connection, "ALTER TABLE `evaluation_period_table` DROP COLUMN `is_open`;");
                Execute(connection,
                    "ALTER TABLE `evaluation_period_table` ADD COLUMN `is_open` TINYINT(1) " +
                    "AS (CASE WHEN `opened_at` IS NOT NULL AND `closed_at` IS NULL THEN 1 ELSE 0 END) STORED;");
            }

            if (!ColumnExists(connection, "evaluation_period_table", "open_marker"))
            {
                Execute(connection,
                    "ALTER TABLE `evaluation_period_table` ADD COLUMN `open_marker` TINYINT " +
                    "AS (CASE WHEN `opened_at` IS NOT NULL AND `closed_at` IS NULL THEN 1 ELSE NULL END) STORED;");
            }

            if (!IndexExists(connection, "evaluation_period_table", "ux_single_open_period"))
            {
                Execute(connection,
                    "ALTER TABLE `evaluation_period_table` ADD UNIQUE KEY `ux_single_open_period` (`open_marker`);");
            }
        }

        private static void UpgradeRespondentSchema(MySqlConnection connection)
        {
            if (!ColumnExists(connection, "evaluation_table", "role_id"))
            {
                return;
            }

            bool hasName = ColumnExists(connection, "evaluation_table", "respondent_name");
            string nameColumn = hasName ? "`respondent_name`" : "''";

            Execute(connection,
                "INSERT IGNORE INTO `evaluation_respondent_table` (`evaluation_id`, `role_id`, `respondent_name`) " +
                "SELECT `evaluation_id`, `role_id`, " + nameColumn + " FROM `evaluation_table`;");

            if (ConstraintExists(connection, "evaluation_table", "fk_evaluation_role"))
            {
                Execute(connection, "ALTER TABLE `evaluation_table` DROP FOREIGN KEY `fk_evaluation_role`;");
            }

            if (IndexExists(connection, "evaluation_table", "idx_evaluation_role"))
            {
                Execute(connection, "ALTER TABLE `evaluation_table` DROP INDEX `idx_evaluation_role`;");
            }

            Execute(connection, "ALTER TABLE `evaluation_table` DROP COLUMN `role_id`;");

            if (hasName)
            {
                Execute(connection, "ALTER TABLE `evaluation_table` DROP COLUMN `respondent_name`;");
            }
        }

        private static bool ColumnIsGenerated(MySqlConnection connection, string tableName, string columnName)
        {
            const string sql =
                "SELECT COUNT(*) FROM `information_schema`.`COLUMNS` " +
                "WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = @table AND `COLUMN_NAME` = @column " +
                "AND `GENERATION_EXPRESSION` <> '';";

            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@table", tableName);
                command.Parameters.AddWithValue("@column", columnName);
                return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture) > 0;
            }
        }

        private static bool ConstraintExists(MySqlConnection connection, string tableName, string constraintName)
        {
            const string sql =
                "SELECT COUNT(*) FROM `information_schema`.`TABLE_CONSTRAINTS` " +
                "WHERE `CONSTRAINT_SCHEMA` = DATABASE() AND `TABLE_NAME` = @table " +
                "AND `CONSTRAINT_NAME` = @constraint;";

            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@table", tableName);
                command.Parameters.AddWithValue("@constraint", constraintName);
                return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture) > 0;
            }
        }

        private static bool ColumnExists(MySqlConnection connection, string tableName, string columnName)
        {
            const string sql =
                "SELECT COUNT(*) FROM `information_schema`.`COLUMNS` " +
                "WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = @table AND `COLUMN_NAME` = @column;";

            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@table", tableName);
                command.Parameters.AddWithValue("@column", columnName);
                return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture) > 0;
            }
        }

        private static bool IndexExists(MySqlConnection connection, string tableName, string indexName)
        {
            const string sql =
                "SELECT COUNT(*) FROM `information_schema`.`STATISTICS` " +
                "WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = @table AND `INDEX_NAME` = @index;";

            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@table", tableName);
                command.Parameters.AddWithValue("@index", indexName);
                return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture) > 0;
            }
        }
    }

    /// <summary>
    /// One semester of the evaluation form. The librarian opens it, students,
    /// faculty and staff answer once each while it is open, and it is closed when
    /// the semester ends so a new period can start clean.
    /// </summary>
    public sealed class EvaluationPeriod
    {
        public int PeriodId { get; set; }
        public string Semester { get; set; } = string.Empty;
        public string SchoolYear { get; set; } = string.Empty;
        public bool IsOpen { get; set; }
        public DateTime? OpenedAt { get; set; }
        public DateTime? ClosedAt { get; set; }

        /// <summary>
        /// The term written the way the librarian says it, with no bullet:
        /// "2ND SEMESTER 2025-2026". Used by the labels and the combo boxes.
        /// </summary>
        public string Title
        {
            get
            {
                string semester = string.IsNullOrWhiteSpace(Semester) ? "SEMESTER" : Semester.Trim();
                string year = (SchoolYear ?? string.Empty).Trim();

                return year.Length == 0 ? semester : semester + " " + year;
            }
        }
    }

    /// <summary>
    /// One person who answered the survey in a semester, shown in the RESPONDENTS
    /// window. Institute, program and student id are empty when the role has no
    /// such detail: faculty have an institute only, and staff have an office
    /// instead of an institute and no program or student id.
    /// </summary>
    public sealed class EvaluationRespondent
    {
        public string Name { get; set; } = string.Empty;
        public string Institute { get; set; } = string.Empty;
        public string Office { get; set; } = string.Empty;
        public string Program { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;

        /// <summary>
        /// The value of the INSTITUTE/OFFICE column: a STAFF member has no
        /// institute, so that column shows the office instead.
        /// </summary>
        public string InstituteOrOffice
        {
            get
            {
                if (string.Equals((Role ?? string.Empty).Trim(), "STAFF", StringComparison.OrdinalIgnoreCase))
                {
                    return (Office ?? string.Empty).Trim();
                }

                return (Institute ?? string.Empty).Trim();
            }
        }
    }

    /// <summary>
    /// Snapshot of every evaluation answer, already grouped the way the ANALYTICS
    /// tab displays it: respondents by role, the mean score per section and the
    /// count of each rating from 5 (excellent) down to 1 (poor).
    /// </summary>
    public sealed class EvaluationSummary
    {
        public int TotalResponses { get; set; }
        public int StudentResponses { get; set; }
        public int FacultyResponses { get; set; }
        public int StaffResponses { get; set; }
        public int OtherResponses { get; set; }

        public int TotalAnswers { get; set; }
        public long RatingTotal { get; set; }
        public double OverallAverage { get; set; }

        public double[] SectionAverages { get; } = new double[EvaluationDirectory.SectionCount];
        public int[] SectionAnswerCounts { get; } = new int[EvaluationDirectory.SectionCount];

        // Index 0 is unused; the ratings themselves run from 1 (poor) to 5 (excellent).
        public int[] RatingCounts { get; } = new int[6];
    }
}
