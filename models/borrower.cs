using System;

namespace oop_project.Models
{
    // INHERITANCE / ENCAPSULATION: a borrower owns the details that
    // borrowers_table stores for every person who takes a book home.
    public class Borrower
    {
        public int BorrowerId { get; private set; }

        // ACCOUNT LINK: a borrower row created from an approved request belongs to
        // a real account, so the row keeps its account_id. That lets the system pull
        // the contact number and profile details straight from the account.
        public int AccountId { get; private set; }

        public string Name { get; private set; }

        // BORROW POP UP: the name is collected as LAST / FIRST / MIDDLE, so each
        // part is stored in its own borrowers_table column. The one line name is
        // composed from those parts when it is shown; it is never stored twice.
        public string LastName { get; private set; }
        public string FirstName { get; private set; }
        public string MiddleName { get; private set; }

        public string Program { get; private set; }
        public string YearLevel { get; private set; }
        public string Institute { get; private set; }
        public string IdNumber { get; private set; }

        // BORROW POP UP: the role decides which of the school details above the
        // borrower has to give. STAFF adds an office and every role gives a contact
        // number, so the library can reach the borrower about the loan.
        public string Role { get; private set; }
        public string Office { get; private set; }
        public string ContactNumber { get; private set; }

        // BORROWED DAYS: calculated from borrow_transactions_table.date_borrow.
        // No derived day counter is stored on the borrower row any more.
        public int DaysBorrowed { get; private set; }

        public Borrower(string name, string idNumber, string program, string yearLevel, string institute)
        {
            BorrowerId = 0;
            Name = Require(name, "Borrower name", 150);

            string last;
            string first;
            string middle;
            SplitName(Name, out last, out first, out middle);

            LastName = last;
            FirstName = first;
            MiddleName = middle;
            IdNumber = Limit(idNumber, 50);
            Program = Limit(program, 100);
            YearLevel = Limit(yearLevel, 50);
            Institute = Limit(institute, 100);
            Role = string.Empty;
            Office = string.Empty;
            ContactNumber = string.Empty;
            DaysBorrowed = 0;
        }

        /// <summary>
        /// The EDIT BORROW RECORD pop up keeps the borrower name on one line, so the
        /// role and the office are applied to a borrower that already exists instead
        /// of going through the three box constructor.
        /// </summary>
        public void ApplyRoleDetails(string role, string office, string? contactNumber = null)
        {
            Role = NormalizeRole(role);
            Office = ToUppercase(Limit(CleanProfileValue(office), 100));

            // An approved request has the contact number on the account profile;
            // an edit that leaves the box empty must not erase a stored number.
            // The profile reader shows a missing value as an em dash, which is not
            // a phone number, so it is ignored like an empty box.
            if (!string.IsNullOrWhiteSpace(contactNumber) &&
                !string.Equals(contactNumber.Trim(), "—", StringComparison.Ordinal))
            {
                ContactNumber = Limit(contactNumber, 30);
            }
        }

        /// <summary>
        /// Ties this borrower row to the account that requested the book. The row is
        /// written with the account_id, so the contact number and profile details
        /// stay reachable from the borrowers_table row.
        /// </summary>
        public void AssignAccountId(int accountId)
        {
            AccountId = accountId < 0 ? 0 : accountId;
        }

        /// <summary>
        /// BORROW BOOK pop up: the name arrives as three boxes. The combined
        /// "LAST, FIRST MIDDLE" text is kept in <see cref="Name"/> so every card,
        /// report and table that shows one borrower line keeps working.
        /// </summary>
        public Borrower(
            string lastName,
            string firstName,
            string middleName,
            string idNumber,
            string program,
            string yearLevel,
            string institute)
        {
            BorrowerId = 0;
            LastName = Require(ToUppercase(lastName), "Last name", 75);
            FirstName = Require(ToUppercase(firstName), "First name", 75);
            MiddleName = ToUppercase(Limit(middleName, 50));
            Name = Require(ComposeName(LastName, FirstName, MiddleName), "Borrower name", 150);
            IdNumber = Limit(idNumber, 50);
            Program = Limit(program, 100);
            YearLevel = Limit(yearLevel, 50);
            Institute = Limit(institute, 100);
            Role = string.Empty;
            Office = string.Empty;
            ContactNumber = string.Empty;
            DaysBorrowed = 0;
        }

        /// <summary>
        /// BORROW BOOK pop up with the role of the borrower. Only the details that
        /// belong to the role arrive filled in: a student has the school details, a
        /// staff member has the office, and everyone gives a contact number. The ID
        /// number stays optional here because a staff member or a faculty member
        /// without one can still borrow, using the name and contact number.
        /// </summary>
        public Borrower(
            string lastName,
            string firstName,
            string middleName,
            string idNumber,
            string program,
            string yearLevel,
            string institute,
            string role,
            string office,
            string contactNumber)
            : this(lastName, firstName, middleName, idNumber, program, yearLevel, institute)
        {
            Role = NormalizeRole(role);
            Office = ToUppercase(Limit(office, 100));
            ContactNumber = Limit(contactNumber, 30);
        }

        public Borrower(int borrowerId, string name, string idNumber, string program, string yearLevel, string institute, int daysBorrowed)
        {
            BorrowerId = borrowerId < 0 ? 0 : borrowerId;
            Name = Require(name, "Borrower name", 150);
            LastName = string.Empty;
            FirstName = string.Empty;
            MiddleName = string.Empty;
            IdNumber = Limit(idNumber, 50);
            Program = Limit(program, 100);
            YearLevel = Limit(yearLevel, 50);
            Institute = Limit(institute, 100);
            Role = string.Empty;
            Office = string.Empty;
            ContactNumber = string.Empty;
            DaysBorrowed = daysBorrowed < 0 ? 0 : daysBorrowed;
        }

        /// <summary>Builds the one line name, e.g. DELA CRUZ, JUAN SANTOS.</summary>
        public static string ComposeName(string lastName, string firstName, string middleName)
        {
            string last = (lastName ?? string.Empty).Trim();
            string first = (firstName ?? string.Empty).Trim();
            string middle = (middleName ?? string.Empty).Trim();

            string name = last + ", " + first;
            return middle.Length == 0 ? name : name + " " + middle;
        }

        /// <summary>
        /// Splits a "LAST, FIRST MIDDLE" name (or a plain full name) into the three
        /// borrowers_table columns. A pre-split name that arrives through the
        /// three box constructor never goes through this method. It is also used
        /// when the FULL NAME of an account is edited, so every table that keeps
        /// the name parts can be refreshed with the same rule.
        /// </summary>
        public static void SplitName(string fullName, out string last, out string first, out string middle)
        {
            string value = (fullName ?? string.Empty).Trim();
            string rest;

            int comma = value.IndexOf(',');
            if (comma >= 0)
            {
                last = value.Substring(0, comma).Trim();
                rest = value.Substring(comma + 1).Trim();
            }
            else
            {
                int space = value.IndexOf(' ');
                last = space < 0 ? value : value.Substring(0, space).Trim();
                rest = space < 0 ? string.Empty : value.Substring(space + 1).Trim();
            }

            int split = rest.IndexOf(' ');
            first = split < 0 ? rest : rest.Substring(0, split).Trim();
            middle = split < 0 ? string.Empty : rest.Substring(split + 1).Trim();

            if (last.Length == 0)
            {
                last = value;
            }
        }

        internal void AssignBorrowerId(int borrowerId)
        {
            if (borrowerId > 0)
            {
                BorrowerId = borrowerId;
            }
        }

        /// <summary>
        /// The name parts and the office are stored the way the rest of the app
        /// shows them: in capitals. The pop up already types in capitals, so this
        /// only makes sure a value that arrives another way matches.
        /// </summary>
        private static string ToUppercase(string value)
        {
            return (value ?? string.Empty).ToUpperInvariant();
        }

        /// <summary>
        /// The profile reader shows a missing value as an em dash. A stored borrower
        /// row must keep it as empty text instead of a dash.
        /// </summary>
        private static string CleanProfileValue(string value)
        {
            string trimmed = (value ?? string.Empty).Trim();
            return trimmed == "—" ? string.Empty : trimmed;
        }

        /// <summary>
        /// Keeps the role in the three values the pop up offers, so the column never
        /// stores a spelling variant. LIBRARIAN borrows with the STAFF rules, and
        /// anything else becomes an empty role, which is what the rows saved before
        /// the ROLE box existed read as.
        /// </summary>
        public static string NormalizeRole(string role)
        {
            string trimmed = (role ?? string.Empty).Trim().ToUpperInvariant();

            switch (trimmed)
            {
                case "STUDENT":
                    return "STUDENT";
                case "STAFF":
                case "LIBRARIAN":
                    return "STAFF";
                case "FACULTY":
                    return "FACULTY";
                default:
                    return string.Empty;
            }
        }

        private static string Require(string value, string fieldName, int maximumLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(fieldName + " is required.");
            }

            string trimmed = value.Trim();

            if (trimmed.Length > maximumLength)
            {
                throw new ArgumentException(fieldName + " must be " + maximumLength + " characters or fewer.");
            }

            return trimmed;
        }

        private static string Limit(string value, int maximumLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string trimmed = value.Trim();
            return trimmed.Length > maximumLength ? trimmed.Substring(0, maximumLength) : trimmed;
        }
    }
}
