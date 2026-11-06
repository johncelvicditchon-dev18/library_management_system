using System;
using MySql.Data.MySqlClient;
using oop_project.Database;

namespace oop_project.Models
{
    /// <summary>
    /// Stores the LIBRARY RESOURCE CENTER EVALUATION FORM responses. One row in
    /// evaluation_table describes the submission (role, optional name, time) and
    /// one row in evaluation_answer_table keeps each 1-5 rating, so the answers
    /// stay normalised and can be averaged per question or per section.
    /// </summary>
    public class EvaluationDirectory
    {
        public const int SectionCount = 5;
        public const int QuestionsPerSection = 5;
        public const int QuestionCount = SectionCount * QuestionsPerSection;

        private static bool _schemaReady;

        private readonly IDatabaseConnection _database = new MySqlDatabaseConnection();

        public void Submit(string respondentRole, string respondentName, int[] ratings)
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

                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    long evaluationId;

                    const string evaluationSql =
                        "INSERT INTO `evaluation_table` (`account_id`, `respondent_role`, `respondent_name`, `submitted_at`) " +
                        "VALUES (NULL, @role, @name, @submittedAt);";

                    using (MySqlCommand command = new MySqlCommand(evaluationSql, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@role", role);
                        command.Parameters.AddWithValue("@name", name);
                        command.Parameters.AddWithValue("@submittedAt", DateTime.Now);
                        command.ExecuteNonQuery();
                        evaluationId = command.LastInsertedId;
                    }

                    const string answerSql =
                        "INSERT INTO `evaluation_answer_table` (`evaluation_id`, `section_no`, `question_no`, `rating`) " +
                        "VALUES (@evaluationId, @section, @question, @rating);";

                    using (MySqlCommand command = new MySqlCommand(answerSql, connection, transaction))
                    {
                        command.Parameters.Add("@evaluationId", MySqlDbType.Int32);
                        command.Parameters.Add("@section", MySqlDbType.Int32);
                        command.Parameters.Add("@question", MySqlDbType.Int32);
                        command.Parameters.Add("@rating", MySqlDbType.Int32);

                        for (int index = 0; index < ratings.Length; index++)
                        {
                            command.Parameters["@evaluationId"].Value = evaluationId;
                            command.Parameters["@section"].Value = (index / QuestionsPerSection) + 1;
                            command.Parameters["@question"].Value = (index % QuestionsPerSection) + 1;
                            command.Parameters["@rating"].Value = ratings[index];
                            command.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
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

            const string evaluationSql =
                "CREATE TABLE IF NOT EXISTS `evaluation_table` (" +
                "`evaluation_id` INT AUTO_INCREMENT PRIMARY KEY, " +
                "`account_id` INT NULL, " +
                "`respondent_role` VARCHAR(20) NOT NULL, " +
                "`respondent_name` VARCHAR(150) NOT NULL DEFAULT '', " +
                "`submitted_at` DATETIME NOT NULL, " +
                "KEY `idx_evaluation_account` (`account_id`), " +
                "CONSTRAINT `fk_evaluation_account` FOREIGN KEY (`account_id`) " +
                "REFERENCES `account_table` (`account_id`) ON DELETE SET NULL" +
                ") ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;";

            const string answerSql =
                "CREATE TABLE IF NOT EXISTS `evaluation_answer_table` (" +
                "`answer_id` INT AUTO_INCREMENT PRIMARY KEY, " +
                "`evaluation_id` INT NOT NULL, " +
                "`section_no` INT NOT NULL, " +
                "`question_no` INT NOT NULL, " +
                "`rating` TINYINT NOT NULL, " +
                "KEY `idx_answer_evaluation` (`evaluation_id`), " +
                "CONSTRAINT `fk_answer_evaluation` FOREIGN KEY (`evaluation_id`) " +
                "REFERENCES `evaluation_table` (`evaluation_id`) ON DELETE CASCADE" +
                ") ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;";

            using (MySqlCommand command = new MySqlCommand(evaluationSql, connection))
            {
                command.ExecuteNonQuery();
            }

            using (MySqlCommand command = new MySqlCommand(answerSql, connection))
            {
                command.ExecuteNonQuery();
            }

            _schemaReady = true;
        }
    }
}
