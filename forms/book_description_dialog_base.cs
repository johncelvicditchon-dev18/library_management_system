using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using oop_project.Models;

namespace oop_project
{
    /// <summary>
    /// Read-only pop up behind the BOOK DESCRIPTION button. The catalog stores
    /// the cover, the author, the year and the number of copies, so this sheet
    /// presents those facts in a readable card instead of a single tooltip.
    ///
    /// The complete control tree is serialized in the designer file so the
    /// Windows Forms designer renders the real dialog instead of a blank form.
    /// </summary>
    public partial class BookDescriptionDialogBase : Form
    {
        /// <summary>Design time constructor with a sample book.</summary>
        public BookDescriptionDialogBase()
            : this(new Book("SAMPLE BOOK TITLE", "SAMPLE AUTHOR", 2024, 3), 2, "STUDENT")
        {
        }

        public BookDescriptionDialogBase(Book book, int available, string portalName)
        {
            InitializeComponent();

            _heading.Text = portalName + " PORTAL  •  BOOK DESCRIPTION";
            _cover.Image = BookImage.CreateThumbnail(book.CoverImage, 190, 280, true);
            _titleLabel.Text = book.Title.ToUpperInvariant();
            _authorLabel.Text = "BY " + book.Author.ToUpperInvariant();
            _publishedValue.Text = book.YearText;
            _copiesValue.Text = book.QuantityText;
            _availableValue.Text = available.ToString(CultureInfo.InvariantCulture) + " OF " + book.QuantityText;
            _dateValue.Text = book.DateAdded;
            _statusValue.Text = available > 0 ? "ON THE SHELF" : "ALL COPIES BORROWED";
            _descriptionLabel.Text =
                "THIS TITLE IS PART OF THE TCGC LEARNING RESOURCE CENTER COLLECTION. " +
                "THE LIBRARY HOLDS " + book.QuantityText + " COPY(IES); " +
                available.ToString(CultureInfo.InvariantCulture) + " ARE CURRENTLY ON THE SHELF. " +
                "REQUEST IT FROM THE CATALOG AND THE LIBRARIAN WILL PREPARE THE COPY FOR PICK UP.";

            _coverFrame.Paint += CoverFrame_Paint;
            _closeButton.Click += delegate { Close(); };
        }

        private void CoverFrame_Paint(object? sender, PaintEventArgs e)
        {
            using Pen pen = new Pen(Color.FromArgb(226, 232, 240), 1.2f);
            e.Graphics.DrawRectangle(pen, 0, 0, _coverFrame.Width - 1, _coverFrame.Height - 1);
        }
    }
}
