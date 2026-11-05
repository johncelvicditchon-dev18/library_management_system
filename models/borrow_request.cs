using System;
using System.Globalization;

namespace oop_project.Models
{
    /// <summary>
    /// One row of borrow_requests_table joined with the book and the student
    /// profile. A student creates a PENDING row; the librarian approves or
    /// rejects it from the BORROW REQUESTS tab.
    /// </summary>
    public class BorrowRequestRow
    {
        public int RequestId { get; private set; }
        public int AccountId { get; private set; }
        public int BookId { get; private set; }
        public string BookTitle { get; private set; }
        public string BookAuthor { get; private set; }
        public int BookYear { get; private set; }
        public string StudentName { get; private set; }
        public string StudentIdNumber { get; private set; }
        public string Program { get; private set; }
        public string YearLevel { get; private set; }
        public string Institute { get; private set; }
        public DateTime RequestDate { get; private set; }
        public string Status { get; private set; }
        public string Note { get; private set; }
        public int ProcessedBy { get; private set; }
        public DateTime? ProcessedDate { get; private set; }
        public int BorrowId { get; private set; }
        public int Copies { get; private set; }

        public string CopiesText
        {
            get { return Copies.ToString(CultureInfo.InvariantCulture); }
        }

        public string RequestDateText
        {
            get { return RequestDate.ToString("MMM dd, yyyy  h:mm tt", CultureInfo.InvariantCulture).ToUpperInvariant(); }
        }

        public string ProcessedDateText
        {
            get
            {
                return ProcessedDate.HasValue
                    ? ProcessedDate.Value.ToString("MMM dd, yyyy  h:mm tt", CultureInfo.InvariantCulture).ToUpperInvariant()
                    : "—";
            }
        }

        public string NoteText
        {
            get { return string.IsNullOrWhiteSpace(Note) ? "—" : Note; }
        }

        public string ProgramText
        {
            get { return string.IsNullOrWhiteSpace(Program) ? "—" : Program; }
        }

        public string InstituteText
        {
            get { return string.IsNullOrWhiteSpace(Institute) ? "—" : Institute; }
        }

        public string StudentIdText
        {
            get { return string.IsNullOrWhiteSpace(StudentIdNumber) ? "—" : StudentIdNumber; }
        }

        public string BookYearText
        {
            get { return BookYear > 0 ? BookYear.ToString(CultureInfo.InvariantCulture) : "—"; }
        }

        public bool IsPending
        {
            get { return string.Equals(Status, "PENDING", StringComparison.OrdinalIgnoreCase); }
        }

        public bool IsApproved
        {
            get { return string.Equals(Status, "APPROVED", StringComparison.OrdinalIgnoreCase); }
        }

        public bool IsRejected
        {
            get { return string.Equals(Status, "REJECTED", StringComparison.OrdinalIgnoreCase); }
        }

        public BorrowRequestRow(
            int requestId,
            int accountId,
            int bookId,
            string bookTitle,
            string bookAuthor,
            int bookYear,
            string studentName,
            string studentIdNumber,
            string program,
            string yearLevel,
            string institute,
            DateTime requestDate,
            string status,
            string note,
            int processedBy,
            DateTime? processedDate,
            int borrowId,
            int copies = 1)
        {
            RequestId = requestId < 0 ? 0 : requestId;
            AccountId = accountId < 0 ? 0 : accountId;
            BookId = bookId < 0 ? 0 : bookId;
            BookTitle = string.IsNullOrWhiteSpace(bookTitle) ? "(DELETED BOOK #" + bookId + ")" : bookTitle.Trim();
            BookAuthor = string.IsNullOrWhiteSpace(bookAuthor) ? "UNKNOWN AUTHOR" : bookAuthor.Trim();
            BookYear = bookYear < 0 ? 0 : bookYear;
            StudentName = string.IsNullOrWhiteSpace(studentName) ? "UNNAMED STUDENT" : studentName.Trim();
            StudentIdNumber = (studentIdNumber ?? string.Empty).Trim();
            Program = (program ?? string.Empty).Trim();
            YearLevel = (yearLevel ?? string.Empty).Trim();
            Institute = (institute ?? string.Empty).Trim();
            RequestDate = requestDate == DateTime.MinValue ? DateTime.Now : requestDate;
            Status = string.IsNullOrWhiteSpace(status) ? "PENDING" : status.Trim().ToUpperInvariant();
            Note = (note ?? string.Empty).Trim();
            ProcessedBy = processedBy < 0 ? 0 : processedBy;
            ProcessedDate = processedDate;
            BorrowId = borrowId < 0 ? 0 : borrowId;
            Copies = copies < 1 ? 1 : copies;
        }
    }
}
