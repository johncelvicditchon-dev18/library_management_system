using System;
using System.Globalization;

namespace oop_project.Models
{
    // One row of borrow_transactions_table joined with borrowers_table and
    // books_table, so the ATTENDANCE tab and the report show who took what.
    public class BorrowRecord
    {
        // The three roles a borrower can have. The role decides which borrower
        // details the EDIT BORROW RECORD pop up must collect.
        public const string StudentRole = "STUDENT";
        public const string StaffRole = "STAFF";
        public const string FacultyRole = "FACULTY";

        public int BorrowId { get; private set; }
        public int BookId { get; private set; }
        public string BookTitle { get; private set; }
        public string Author { get; private set; }
        public int BookYear { get; private set; }
        public int BorrowerId { get; private set; }
        public string BorrowerName { get; private set; }
        public string BorrowerIdNumber { get; private set; }
        public string BorrowerProgram { get; private set; }
        public string BorrowerYearLevel { get; private set; }
        public string BorrowerInstitute { get; private set; }

        // borrowers_table keeps the office for a STAFF borrower. The role is a
        // foreign key (role_id) and is read through role_table.
        public string BorrowerRole { get; private set; }
        public string BorrowerOffice { get; private set; }

        // The contact number from borrowers_table, so an old row that never stored
        // one can be completed from the EDIT BORROW RECORD pop up.
        public string BorrowerContactNumber { get; private set; }

        /// <summary>
        /// The role of this borrower. A row saved before the ROLE box existed has no
        /// stored role, so the details the row kept are used to work it out: an
        /// office means STAFF, a year level means STUDENT, and an institute or a
        /// program means FACULTY. Anything else is treated as a STUDENT.
        /// </summary>
        public string Role
        {
            get
            {
                if (BorrowerRole.Length > 0)
                {
                    return BorrowerRole;
                }

                if (BorrowerOffice.Length > 0)
                {
                    return StaffRole;
                }

                if (BorrowerYearLevel.Length > 0)
                {
                    return StudentRole;
                }

                if (BorrowerInstitute.Length > 0 || BorrowerProgram.Length > 0)
                {
                    return FacultyRole;
                }

                return StudentRole;
            }
        }


        public string ProgramText
        {
            get { return string.IsNullOrWhiteSpace(BorrowerProgram) ? "—" : BorrowerProgram; }
        }

        public string YearLevelText
        {
            get { return string.IsNullOrWhiteSpace(BorrowerYearLevel) ? "—" : BorrowerYearLevel; }
        }

        // The borrow flow records one copy per loan, so BORROWED COPY is 1.
        public int BorrowedCopy { get; private set; }

        public string BorrowedCopyText
        {
            get { return BorrowedCopy.ToString(CultureInfo.InvariantCulture); }
        }

        /// <summary>
        /// The value for the INSTITUTE/OFFICE column of BORROWED BOOKS. A STUDENT or
        /// a FACULTY member shows the institute; a STAFF borrower has no institute,
        /// so the office they belong to is shown instead.
        /// </summary>
        public string InstituteText
        {
            get
            {
                string value = string.Equals(Role, StaffRole, StringComparison.OrdinalIgnoreCase)
                    ? BorrowerOffice
                    : BorrowerInstitute;

                return string.IsNullOrWhiteSpace(value) ? "—" : value;
            }
        }

        public string BookYearText
        {
            get { return BookYear > 0 ? BookYear.ToString(CultureInfo.InvariantCulture) : "—"; }
        }
        public DateTime DateBorrowed { get; private set; }
        public DateTime? DateReturned { get; private set; }

        // No due date is set for a loan. The record is measured by how many whole
        // days it has run instead: a day is only added after 24 full hours, so a
        // book taken at 1:15 AM shows 1 day at 1:15 AM the next morning.
        public int DaysBorrowed
        {
            get
            {
                DateTime until = DateReturned.HasValue ? DateReturned.Value : DateTime.Now;
                double days = Math.Floor((until - DateBorrowed).TotalDays);

                return days < 0 ? 0 : (int)days;
            }
        }

        public string DaysBorrowedText
        {
            get
            {
                return DaysBorrowed.ToString(CultureInfo.InvariantCulture) +
                       (DaysBorrowed == 1 ? " DAY" : " DAYS");
            }
        }

        // POLYMORPHISM: one record reports BORROWED or RETURNED from its dates,
        // so no status flag has to be stored twice.
        public string Status
        {
            get { return DateReturned.HasValue ? "RETURNED" : "BORROWED"; }
        }

        public string DateBorrowedText
        {
            get { return DateBorrowed.ToString("MMM dd, yyyy", CultureInfo.InvariantCulture).ToUpperInvariant(); }
        }

        // Date and time together, so the report shows the exact start of the
        // 24 hour counter used by DaysBorrowed.
        public string DateBorrowedStampText
        {
            get { return DateBorrowed.ToString("MMM dd, yyyy  h:mm tt", CultureInfo.InvariantCulture).ToUpperInvariant(); }
        }

        public string DateReturnedText
        {
            get
            {
                return DateReturned.HasValue
                    ? DateReturned.Value.ToString("MMM dd, yyyy", CultureInfo.InvariantCulture).ToUpperInvariant()
                    : "—";
            }
        }

        public BorrowRecord(
            int bookId,
            string bookTitle,
            string author,
            int bookYear,
            Borrower borrower,
            DateTime dateBorrowed)
            : this(
                0,
                bookId,
                bookTitle,
                author,
                bookYear,
                borrower == null ? 0 : borrower.BorrowerId,
                borrower == null ? string.Empty : borrower.Name,
                borrower == null ? string.Empty : borrower.IdNumber,
                borrower == null ? string.Empty : borrower.Program,
                borrower == null ? string.Empty : borrower.YearLevel,
                borrower == null ? string.Empty : borrower.Institute,
                dateBorrowed,
                null,
                borrower == null ? string.Empty : borrower.Role,
                borrower == null ? string.Empty : borrower.Office,
                borrower == null ? string.Empty : borrower.ContactNumber)
        {
        }

        public BorrowRecord(
            int borrowId,
            int bookId,
            string bookTitle,
            string author,
            int bookYear,
            int borrowerId,
            string borrowerName,
            string borrowerIdNumber,
            string borrowerProgram,
            string borrowerYearLevel,
            string borrowerInstitute,
            DateTime dateBorrowed,
            DateTime? dateReturned,
            string borrowerRole = "",
            string borrowerOffice = "",
            string borrowerContactNumber = "")
        {
            BorrowId = borrowId < 0 ? 0 : borrowId;
            BookId = bookId < 0 ? 0 : bookId;
            BookTitle = Require(bookTitle, "Book title");
            Author = string.IsNullOrWhiteSpace(author) ? "UNKNOWN AUTHOR" : author.Trim();
            BookYear = bookYear < 0 ? 0 : bookYear;
            BorrowerId = borrowerId < 0 ? 0 : borrowerId;
            BorrowerName = Require(borrowerName, "Borrower name");
            BorrowerIdNumber = string.IsNullOrWhiteSpace(borrowerIdNumber) ? "—" : borrowerIdNumber.Trim();
            BorrowerProgram = borrowerProgram == null ? string.Empty : borrowerProgram.Trim();
            BorrowerYearLevel = borrowerYearLevel == null ? string.Empty : borrowerYearLevel.Trim();
            BorrowerInstitute = borrowerInstitute == null ? string.Empty : borrowerInstitute.Trim();
            BorrowerRole = Borrower.NormalizeRole(borrowerRole);
            BorrowerOffice = (borrowerOffice ?? string.Empty).Trim().ToUpperInvariant();
            BorrowerContactNumber = (borrowerContactNumber ?? string.Empty).Trim();
            BorrowedCopy = 1;
            DateBorrowed = dateBorrowed == DateTime.MinValue ? DateTime.Now : dateBorrowed;
            DateReturned = dateReturned;
        }

        private static string Require(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(fieldName + " is required.");
            }

            return value.Trim();
        }

        internal void AssignBorrowId(int borrowId)
        {
            if (borrowId > 0)
            {
                BorrowId = borrowId;
            }
        }

        public void MarkReturned(DateTime returnedOn)
        {
            if (DateReturned.HasValue)
            {
                return;
            }

            DateReturned = returnedOn == DateTime.MinValue ? DateTime.Now : returnedOn;
        }

        // ENCAPSULATION: the EDIT LOAN pop up rewrites the row through this method,
        // so the required values are checked again before the grid is refreshed.
        public void ApplyChanges(
            int bookId,
            string bookTitle,
            string author,
            int bookYear,
            string borrowerName,
            string borrowerIdNumber,
            string borrowerProgram,
            string borrowerYearLevel,
            string borrowerInstitute,
            DateTime dateBorrowed,
            DateTime? dateReturned,
            string borrowerRole = "",
            string borrowerOffice = "",
            string borrowerContactNumber = "")
        {
            BookId = bookId < 0 ? 0 : bookId;
            BookTitle = Require(bookTitle, "Book title");
            Author = string.IsNullOrWhiteSpace(author) ? "UNKNOWN AUTHOR" : author.Trim();
            BookYear = bookYear < 0 ? 0 : bookYear;
            BorrowerName = Require(borrowerName, "Borrower name");
            BorrowerIdNumber = string.IsNullOrWhiteSpace(borrowerIdNumber) ? "—" : borrowerIdNumber.Trim();
            BorrowerProgram = borrowerProgram == null ? string.Empty : borrowerProgram.Trim();
            BorrowerYearLevel = borrowerYearLevel == null ? string.Empty : borrowerYearLevel.Trim();
            BorrowerInstitute = borrowerInstitute == null ? string.Empty : borrowerInstitute.Trim();
            BorrowerRole = Borrower.NormalizeRole(borrowerRole);
            BorrowerOffice = (borrowerOffice ?? string.Empty).Trim().ToUpperInvariant();
            BorrowerContactNumber = (borrowerContactNumber ?? string.Empty).Trim();
            DateBorrowed = dateBorrowed == DateTime.MinValue ? DateTime.Now : dateBorrowed;
            DateReturned = dateReturned;
        }
    }
}
