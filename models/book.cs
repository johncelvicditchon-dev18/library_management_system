using System;
using System.Globalization;

namespace oop_project.Models
{
    // ENCAPSULATION: a book validates itself, so an empty title or an impossible
    // year can never reach books_table.
    public class Book
    {
        public const string AvailableStatus = "AVAILABLE";
        public const string BorrowedStatus = "BORROWED";
        public const int MinimumYear = 1000;
        public const int MaximumQuantity = 9999;

        public int BookId { get; private set; }
        public string Title { get; private set; }
        public string Author { get; private set; }
        public int Year { get; private set; }

        // books_table.quantity: how many copies of this title the library owns.
        public int Quantity { get; private set; }

        // books_table.date_added keeps the readable text the other tables use,
        // e.g. OCTOBER 28, 2026.
        public string DateAdded { get; private set; }

        // books_table has no status column: the catalog works the availability out
        // from the open rows of borrow_transactions_table.
        public bool IsAvailable { get; private set; }

        public string Status
        {
            get { return IsAvailable ? AvailableStatus : BorrowedStatus; }
        }

        public string YearText
        {
            get { return Year > 0 ? Year.ToString(CultureInfo.InvariantCulture) : "—"; }
        }

        public string QuantityText
        {
            get { return Quantity.ToString(CultureInfo.InvariantCulture); }
        }

        // Shown inside the book drop down of the BORROW pop up.
        public string DisplayText
        {
            get { return Title + "  •  " + Author + " (" + YearText + ")"; }
        }

        // Used by the ADD BOOK pop up: a brand new title starts as available, and
        // the year and the number of copies are validated before anything is saved.
        public Book(string title, string author, int year, int quantity)
        {
            BookId = 0;
            Title = Require(title, "Book title");
            Author = string.IsNullOrWhiteSpace(author) ? "UNKNOWN AUTHOR" : author.Trim();
            Year = ValidateYear(year);
            Quantity = ValidateQuantity(quantity);
            DateAdded = TodayText();
            IsAvailable = true;
        }

        // Used when a row is read back from MySQL.
        public Book(int bookId, string title, string author, int year, int quantity, string dateAdded, bool isAvailable)
        {
            BookId = bookId < 0 ? 0 : bookId;
            Title = Require(title, "Book title");
            Author = string.IsNullOrWhiteSpace(author) ? "UNKNOWN AUTHOR" : author.Trim();
            Year = SanitizeStoredYear(year);
            Quantity = quantity < 0 ? 0 : quantity;
            DateAdded = string.IsNullOrWhiteSpace(dateAdded) ? "—" : dateAdded.Trim();
            IsAvailable = isAvailable;
        }

        private static string Require(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(fieldName + " is required.");
            }

            return value.Trim();
        }

        private static int ValidateYear(int year)
        {
            int latestYear = DateTime.Today.Year;

            if (year < MinimumYear || year > latestYear)
            {
                throw new ArgumentException(
                    "Year must be a number between " + MinimumYear + " and " + latestYear + ".");
            }

            return year;
        }

        private static int ValidateQuantity(int quantity)
        {
            if (quantity < 1 || quantity > MaximumQuantity)
            {
                throw new ArgumentException(
                    "Quantity must be a number between 1 and " + MaximumQuantity + ".");
            }

            return quantity;
        }

        internal void AssignBookId(int bookId)
        {
            if (bookId > 0)
            {
                BookId = bookId;
            }
        }

        // A stored year may be 0 or out of range; it is shown as "—" instead of
        // rejecting the whole row when the catalog loads.
        private static int SanitizeStoredYear(int year)
        {
            int latestYear = DateTime.Today.Year;
            return year < MinimumYear || year > latestYear + 1 ? 0 : year;
        }

        public static string TodayText()
        {
            return DateTime.Today.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture).ToUpperInvariant();
        }

        public void MarkBorrowed()
        {
            IsAvailable = false;
        }

        public void MarkAvailable()
        {
            IsAvailable = true;
        }

        // ENCAPSULATION: the UPDATE pop up sends the edited values through the same
        // rules the constructor uses, so an edited book is validated exactly like a
        // brand new one. The id and the date_added stamp never change.
        public void ApplyChanges(string title, string author, int year, int quantity)
        {
            Title = Require(title, "Book title");
            Author = string.IsNullOrWhiteSpace(author) ? "UNKNOWN AUTHOR" : author.Trim();
            Year = ValidateYear(year);
            Quantity = ValidateQuantity(quantity);
        }

        public override string ToString()
        {
            return DisplayText;
        }
    }
}
