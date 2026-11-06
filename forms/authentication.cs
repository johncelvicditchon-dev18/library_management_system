using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using oop_project.Database;

namespace oop_project
{
    // ABSTRACTION: shared connection, dialog handling and field validation for
    // login and signup.
    //
    // NOTE: this is a static helper class on purpose, NOT a Form base class.
    // Login and Signup therefore derive from System.Windows.Forms.Form directly,
    // so the WinForms designer keeps every one of their controls editable (a
    // custom Form base would mark the whole surface as inherited and lock it).
    public static class Authentication
    {
        private static readonly IDatabaseConnection _database = new MySqlDatabaseConnection();

        // WARNING STYLE: every invalid input uses the same red text and red border.
        private static readonly Color ErrorColor = Color.FromArgb(220, 38, 38);
        // SUCCESS STYLE: a satisfied input uses the same green as the focused border.
        private static readonly Color SuccessColor = Color.FromArgb(22, 163, 74);
        private static readonly Color IdleBorderColor = Color.FromArgb(229, 231, 235);

        public static MySqlConnection OpenConnection()
        {
            return _database.CreateConnection();
        }

        // SCHEMA: faculty_info_table, staff_info_table and librarian_info_table
        // keep the contact number collected by the PROFILE tab. A database created
        // with the older schema gets the columns added automatically, so the
        // account query and the profile saves always find them. The names are
        // internal constants, never typed by the user.
        public static void EnsureContactNumberColumns(MySqlConnection conn)
        {
            EnsureColumn(conn, "faculty_info_table", "contact_number", "VARCHAR(20) NOT NULL DEFAULT ''");
            EnsureColumn(conn, "staff_info_table", "contact_number", "VARCHAR(20) NOT NULL DEFAULT ''");
            EnsureColumn(conn, "librarian_info_table", "contact_number", "VARCHAR(20) NOT NULL DEFAULT ''");
        }

        // PROFILE CLEANUP: signup no longer asks a FACULTY member for a program
        // (a faculty member only belongs to an institute) and no longer asks a
        // LIBRARIAN for an office. An older database may still keep those values,
        // so they are removed once instead of being shown again in the records.
        public static void EnsureProfileColumnsRemoved(MySqlConnection conn)
        {
            DropForeignKeyForColumn(conn, "faculty_info_table", "program_id");
            DropColumn(conn, "faculty_info_table", "program_id");
            DropColumn(conn, "librarian_info_table", "office");
        }

        private static bool ColumnExists(MySqlConnection conn, string table, string column)
        {
            const string checkQuery = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS " +
                                      "WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @table AND COLUMN_NAME = @column;";

            using (MySqlCommand check = new MySqlCommand(checkQuery, conn))
            {
                check.Parameters.AddWithValue("@table", table);
                check.Parameters.AddWithValue("@column", column);
                return Convert.ToInt64(check.ExecuteScalar()) > 0;
            }
        }

        private static bool TableExists(MySqlConnection conn, string table)
        {
            const string checkQuery = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES " +
                                      "WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @table;";

            using (MySqlCommand check = new MySqlCommand(checkQuery, conn))
            {
                check.Parameters.AddWithValue("@table", table);
                return Convert.ToInt64(check.ExecuteScalar()) > 0;
            }
        }

        private static void DropForeignKeyForColumn(MySqlConnection conn, string table, string column)
        {
            if (!ColumnExists(conn, table, column))
            {
                return;
            }

            const string checkQuery = "SELECT CONSTRAINT_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE " +
                                      "WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @table " +
                                      "AND COLUMN_NAME = @column AND REFERENCED_TABLE_NAME IS NOT NULL LIMIT 1;";

            object? constraint;
            using (MySqlCommand check = new MySqlCommand(checkQuery, conn))
            {
                check.Parameters.AddWithValue("@table", table);
                check.Parameters.AddWithValue("@column", column);
                constraint = check.ExecuteScalar();
            }

            if (constraint == null || constraint == DBNull.Value)
            {
                return;
            }

            string name = Convert.ToString(constraint) ?? string.Empty;
            if (name.Length == 0)
            {
                return;
            }

            using (MySqlCommand drop = new MySqlCommand("ALTER TABLE `" + table + "` DROP FOREIGN KEY `" + name + "`;", conn))
            {
                drop.ExecuteNonQuery();
            }
        }

        private static void DropColumn(MySqlConnection conn, string table, string column)
        {
            if (!ColumnExists(conn, table, column))
            {
                return;
            }

            using (MySqlCommand drop = new MySqlCommand("ALTER TABLE `" + table + "` DROP COLUMN `" + column + "`;", conn))
            {
                drop.ExecuteNonQuery();
            }
        }

        private static void EnsureColumn(MySqlConnection conn, string table, string column, string definition)
        {
            if (!TableExists(conn, table) || ColumnExists(conn, table, column))
            {
                return;
            }

            using (MySqlCommand alter = new MySqlCommand("ALTER TABLE `" + table + "` ADD COLUMN `" + column + "` " + definition + ";", conn))
            {
                alter.ExecuteNonQuery();
            }
        }

        // ENCAPSULATION: every dialog is shown in UPPER CASE from one place.
        public static void ShowMessage(string? message, string? caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            string text = message == null ? string.Empty : message.ToUpperInvariant();
            string title = caption == null ? string.Empty : caption.ToUpperInvariant();
            MessageBox.Show(text, title, buttons, icon);
        }

        public static void ShowError(string formName, string? message)
        {
            ShowMessage(message, formName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // ENCAPSULATION: one method decides how an invalid input looks.
        // Passing a null/empty message hides the warning and restores the normal border.
        public static void SetFieldError(Label? messageLabel, FieldPanel? field, string? message)
        {
            bool hasError = !string.IsNullOrEmpty(message);
            if (messageLabel != null)
            {
                messageLabel.Text = hasError ? message! : string.Empty;
                messageLabel.ForeColor = ErrorColor;
                messageLabel.Visible = hasError;
            }
            if (field != null)
            {
                field.HasError = hasError;
                field.BorderColor = hasError ? ErrorColor : IdleBorderColor;
                field.Invalidate();
            }
        }

        // ENCAPSULATION: the matching counterpart of SetFieldError.
        // It keeps the message visible, but paints it green to confirm the input is correct.
        public static void SetFieldSuccess(Label? messageLabel, FieldPanel? field, string message)
        {
            if (messageLabel != null)
            {
                messageLabel.Text = message;
                messageLabel.ForeColor = SuccessColor;
                messageLabel.Visible = true;
            }
            if (field != null)
            {
                field.HasError = false;
                field.BorderColor = SuccessColor;
                field.Invalidate();
            }
        }

        // LAYOUT: keeps the composition centred when the window is maximised. Each
        // form owns one instance, so its designer-captured positions stay separate.
        public sealed class WindowLayout
        {
            private readonly Dictionary<Control, Point> _designLocations = new Dictionary<Control, Point>();
            private Size _designClientSize;
            private bool _designCaptured;

            // Remembers where every control sits in the designer, once the form is
            // loaded (so any Windows DPI scaling has already been applied).
            public void Capture(Size designSize, params Control[]? controls)
            {
                if (_designCaptured || controls == null)
                {
                    return;
                }

                _designClientSize = designSize;
                _designLocations.Clear();

                foreach (Control control in controls)
                {
                    if (control != null)
                    {
                        _designLocations[control] = control.Location;
                    }
                }

                _designCaptured = true;
            }

            // When the window grows or is maximised, the gradient background
            // stretches to fill it and the composition stays centred. The design size
            // is passed in earlier because the window opens maximised, so ClientSize
            // must not be used as the reference for centring.
            public void Apply(Form form, Control? background, params Control[]? controls)
            {
                if (!_designCaptured || background == null || controls == null)
                {
                    return;
                }

                Size area = form.ClientSize;
                if (area.Width <= 0 || area.Height <= 0)
                {
                    return;
                }

                int offsetX = (area.Width - _designClientSize.Width) / 2;
                int offsetY = (area.Height - _designClientSize.Height) / 2;

                background.Bounds = new Rectangle(
                    background.Left,
                    background.Top,
                    Math.Max(1, area.Width - background.Left),
                    Math.Max(1, area.Height - background.Top));

                foreach (Control control in controls)
                {
                    if (control == null || !_designLocations.TryGetValue(control, out Point design))
                    {
                        continue;
                    }

                    control.Location = new Point(design.X + offsetX, design.Y + offsetY);
                }
            }
        }
    }
}
