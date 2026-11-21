using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using MySql.Data.MySqlClient;
using oop_project.Database;

namespace oop_project.Models
{
    /// <summary>
    /// SECURITY: the sign-in password is never stored as typed. account_table
    /// keeps a PBKDF2-SHA256 hash written as
    /// "PBKDF2-SHA256$iterations$salt$hash". Accounts created before the hash
    /// existed still hold the plain text, so <see cref="Verify"/> accepts both
    /// forms and <see cref="NeedsUpgrade"/> tells the sign in code when an old
    /// row can be rewritten as a hash after a correct password.
    /// </summary>
    public static class PasswordHasher
    {
        private const string Prefix = "PBKDF2-SHA256";
        private const int Iterations = 120000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public static string Hash(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password ?? string.Empty),
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            return Prefix + "$" + Iterations.ToString(CultureInfo.InvariantCulture) + "$" +
                   Convert.ToBase64String(salt) + "$" + Convert.ToBase64String(hash);
        }

        public static bool IsHashed(string storedPassword)
        {
            return !string.IsNullOrEmpty(storedPassword) &&
                   storedPassword.StartsWith(Prefix + "$", StringComparison.Ordinal);
        }

        public static bool Verify(string password, string storedPassword)
        {
            if (string.IsNullOrEmpty(storedPassword))
            {
                return false;
            }

            // LEGACY ROWS: the plain text an older build saved. The comparison is
            // exact, which is what the old BINARY comparison in SQL did.
            if (!IsHashed(storedPassword))
            {
                return string.Equals(password, storedPassword, StringComparison.Ordinal);
            }

            string[] parts = storedPassword.Split('$');

            if (parts.Length != 4 ||
                !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int iterations) ||
                iterations < 1000)
            {
                return false;
            }

            try
            {
                byte[] salt = Convert.FromBase64String(parts[2]);
                byte[] expected = Convert.FromBase64String(parts[3]);

                if (salt.Length == 0 || expected.Length == 0)
                {
                    return false;
                }

                byte[] actual = Rfc2898DeriveBytes.Pbkdf2(
                    Encoding.UTF8.GetBytes(password ?? string.Empty),
                    salt,
                    iterations,
                    HashAlgorithmName.SHA256,
                    expected.Length);

                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>
        /// True when the stored value is still plain text, so the caller can
        /// replace it after a successful sign in.
        /// </summary>
        public static bool NeedsUpgrade(string storedPassword)
        {
            return !IsHashed(storedPassword);
        }

        /// <summary>
        /// Best effort migration of one account. A failed upgrade must never stop
        /// a person signing in, so the error is ignored and retried next time.
        /// </summary>
        public static void UpgradeStoredPassword(int accountId, string password)
        {
            if (accountId <= 0)
            {
                return;
            }

            try
            {
                using (MySqlConnection connection = new MySqlDatabaseConnection().CreateConnection())
                {
                    connection.Open();

                    const string sql =
                        "UPDATE `account_table` SET `password` = @password " +
                        "WHERE `account_id` = @accountId;";

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@password", Hash(password));
                        command.Parameters.AddWithValue("@accountId", accountId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException)
            {
                // The plain text still works, so the sign in can continue.
            }
        }
    }
}
