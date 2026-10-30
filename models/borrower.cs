using System;

namespace oop_project.Models
{
    // INHERITANCE / ENCAPSULATION: a borrower owns the details that
    // borrowers_table stores for every person who takes a book home.
    public class Borrower
    {
        public int BorrowerId { get; private set; }
        public string Name { get; private set; }
        public string Program { get; private set; }
        public string YearLevel { get; private set; }
        public string Institute { get; private set; }
        public string IdNumber { get; private set; }

        // borrowers_table.day_before_borrow keeps the number of whole days the
        // latest loan has run. The dashboard refreshes it from the borrow time,
        // so "1" appears only after 24 full hours.
        public int DaysBorrowed { get; private set; }

        public Borrower(string name, string idNumber, string program, string yearLevel, string institute)
            : this(0, name, idNumber, program, yearLevel, institute, 0)
        {
        }

        public Borrower(int borrowerId, string name, string idNumber, string program, string yearLevel, string institute, int daysBorrowed)
        {
            BorrowerId = borrowerId < 0 ? 0 : borrowerId;
            Name = Require(name, "Borrower name", 150);
            IdNumber = Require(idNumber, "ID number", 50);
            Program = Limit(program, 100);
            YearLevel = Limit(yearLevel, 50);
            Institute = Limit(institute, 100);
            DaysBorrowed = daysBorrowed < 0 ? 0 : daysBorrowed;
        }

        internal void AssignBorrowerId(int borrowerId)
        {
            if (borrowerId > 0)
            {
                BorrowerId = borrowerId;
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
