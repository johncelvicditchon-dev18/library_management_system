using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using oop_project.Models;
using QRCoder;

namespace oop_project.forms
{
    /// <summary>
    /// The student side of the library. A student sees the catalog, can ask the
    /// librarian to borrow a title, follows the request status, sees the books
    /// currently on loan, and carries a QR code with the student's own details.
    /// </summary>
    public sealed class student_dashboard : Form
    {
        private static readonly Color HeaderGreen = Color.FromArgb(6, 78, 59);
        private static readonly Color MutedText = Color.FromArgb(148, 163, 184);
        private static readonly Color SuccessText = Color.FromArgb(4, 120, 87);
        private static readonly Color WarningText = Color.FromArgb(180, 83, 9);
        private static readonly Color DangerText = Color.FromArgb(185, 28, 28);

        private readonly LibraryCatalog _catalog = new LibraryCatalog();
        private readonly AccountDirectory _directory = new AccountDirectory();
        private readonly BorrowRequestDirectory _requestDirectory = new BorrowRequestDirectory();

        private readonly int _accountId;
        private readonly string _fullName;
        private readonly string _role;
        private readonly string _username;
        private readonly string _status;

        private AccountRow? _profile;
        private List<BorrowRequestRow> _studentRequests = new List<BorrowRequestRow>();
        private Book? _selectedBook;

        private AvatarBadge _avatar = null!;
        private Label _profileName = null!;
        private Label _profileMeta = null!;
        private PillBadge _roleBadge = null!;
        private Label _schoolIdValue = null!;
        private Label _programValue = null!;
        private Label _yearValue = null!;
        private Label _instituteValue = null!;
        private Label _contactValue = null!;
        private Label _addressValue = null!;
        private PictureBox _qrPicture = null!;
        private Label _qrCaption = null!;
        private Image? _qrImage;

        private TabControl _tabs = null!;
        private TabPage _catalogPage = null!;
        private TabPage _requestsPage = null!;
        private TabPage _loansPage = null!;

        private DataGridView _catalogGrid = null!;
        private DataGridView _requestGrid = null!;
        private DataGridView _loanGrid = null!;
        private Label _catalogEmptyLabel = null!;
        private Label _catalogStatus = null!;
        private Label _requestsStatus = null!;
        private Label _loansStatus = null!;

        private SplitContainer _catalogSplit = null!;
        private PictureBox _detailCover = null!;
        private Label _detailTitle = null!;
        private Label _detailAuthor = null!;
        private Label _detailMeta = null!;
        private Label _detailAvailability = null!;
        private Label _detailHint = null!;
        private RoundedButton _requestButton = null!;
        private Image? _detailImage;

        public student_dashboard(int accountID, string fullName, string role, string username = "", string status = "")
        {
            _accountId = accountID;
            _fullName = string.IsNullOrWhiteSpace(fullName) ? "STUDENT" : fullName.Trim();
            _role = string.IsNullOrWhiteSpace(role) ? "STUDENT" : role.Trim().ToUpperInvariant();
            _username = (username ?? string.Empty).Trim();
            _status = string.IsNullOrWhiteSpace(status) ? "ACTIVE" : status.Trim().ToUpperInvariant();

            InitializeUi();
            ScreenMode.Apply(this);
            Shown += StudentDashboard_Shown;
        }

        private void InitializeUi()
        {
            Text = "TCGC - STUDENT DASHBOARD";
            BackColor = Color.FromArgb(241, 245, 249);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(1180, 720);
            StartPosition = FormStartPosition.CenterScreen;

            TableLayoutPanel root = new TableLayoutPanel();
            root.ColumnCount = 1;
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            root.Dock = DockStyle.Fill;
            root.RowCount = 2;
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 88F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.Controls.Add(CreateHeader(), 0, 0);

            TableLayoutPanel body = new TableLayoutPanel();
            body.ColumnCount = 2;
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 350F));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            body.Dock = DockStyle.Fill;
            body.RowCount = 1;
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            body.Controls.Add(CreateSidebar(), 0, 0);
            body.Controls.Add(CreateMainContent(), 1, 0);
            root.Controls.Add(body, 0, 1);

            Controls.Add(root);
        }

        private Panel CreateHeader()
        {
            Panel header = new Panel();
            header.BackColor = HeaderGreen;
            header.Dock = DockStyle.Fill;

            Label title = new Label();
            title.AutoSize = true;
            title.BackColor = Color.Transparent;
            title.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
            title.ForeColor = Color.White;
            title.Location = new Point(30, 16);
            title.Text = "TCGC LEARNING RESOURCE CENTER";

            Label subtitle = new Label();
            subtitle.AutoSize = true;
            subtitle.BackColor = Color.Transparent;
            subtitle.Font = new Font("Segoe UI", 9F);
            subtitle.ForeColor = Color.FromArgb(167, 243, 208);
            subtitle.Location = new Point(33, 52);
            subtitle.Text = "STUDENT PORTAL • BORROW A BOOK IN A FEW CLICKS";

            Panel right = new Panel();
            right.BackColor = Color.Transparent;
            right.Dock = DockStyle.Right;
            right.Width = 380;

            Label name = new Label();
            name.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            name.BackColor = Color.Transparent;
            name.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            name.ForeColor = Color.White;
            name.Location = new Point(20, 13);
            name.Size = new Size(340, 24);
            name.Text = _fullName.ToUpperInvariant();
            name.TextAlign = ContentAlignment.MiddleRight;

            _roleBadge = new PillBadge();
            _roleBadge.BackColor = Color.Transparent;
            _roleBadge.BorderColor = Color.FromArgb(167, 243, 208);
            _roleBadge.FillColor = Color.White;
            _roleBadge.TextColor = HeaderGreen;
            _roleBadge.Text = _role;
            _roleBadge.Location = new Point(205, 47);

            RoundedButton signOut = CreateButton(
                "SIGN OUT", new Point(286, 43), new Size(94, 34),
                Color.FromArgb(194, 65, 12), Color.FromArgb(154, 52, 18), Color.FromArgb(120, 45, 15));
            signOut.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            signOut.Click += SignOut_Click;

            Label account = new Label();
            account.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            account.BackColor = Color.Transparent;
            account.Font = new Font("Segoe UI", 8.25F);
            account.ForeColor = Color.FromArgb(209, 250, 229);
            account.Location = new Point(10, 47);
            account.Size = new Size(185, 24);
            account.Text = "ACCOUNT #" + _accountId.ToString(CultureInfo.InvariantCulture);
            account.TextAlign = ContentAlignment.MiddleRight;

            right.Controls.Add(account);
            right.Controls.Add(signOut);
            right.Controls.Add(_roleBadge);
            right.Controls.Add(name);

            header.Controls.Add(right);
            header.Controls.Add(title);
            header.Controls.Add(subtitle);
            return header;
        }

        private Panel CreateSidebar()
        {
            Panel sidebar = new Panel();
            sidebar.AutoScroll = true;
            sidebar.BackColor = Color.White;
            sidebar.Dock = DockStyle.Fill;
            sidebar.Padding = new Padding(24, 20, 24, 20);

            Label heading = new Label();
            heading.AutoSize = true;
            heading.BackColor = Color.Transparent;
            heading.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            heading.ForeColor = AccentGreen();
            heading.Location = new Point(24, 0);
            heading.Text = "MY PROFILE";

            _avatar = new AvatarBadge();
            _avatar.BackColor = Color.Transparent;
            _avatar.FillBottomColor = Color.FromArgb(4, 90, 66);
            _avatar.FillTopColor = Color.FromArgb(34, 197, 94);
            _avatar.Initials = Initials(_fullName);
            _avatar.Location = new Point(127, 34);
            _avatar.Size = new Size(96, 96);

            _profileName = new Label();
            _profileName.AutoEllipsis = true;
            _profileName.BackColor = Color.Transparent;
            _profileName.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            _profileName.ForeColor = Color.FromArgb(6, 78, 59);
            _profileName.Location = new Point(24, 140);
            _profileName.Size = new Size(302, 50);
            _profileName.Text = _fullName;
            _profileName.TextAlign = ContentAlignment.MiddleCenter;

            _profileMeta = new Label();
            _profileMeta.BackColor = Color.Transparent;
            _profileMeta.Font = new Font("Segoe UI", 8.5F);
            _profileMeta.ForeColor = Color.FromArgb(71, 85, 105);
            _profileMeta.Location = new Point(24, 214);
            _profileMeta.Size = new Size(302, 22);
            _profileMeta.Text = "ACCOUNT #" + _accountId + "     •     @" + _username;
            _profileMeta.TextAlign = ContentAlignment.MiddleCenter;

            Panel divider = new Panel();
            divider.BackColor = Color.FromArgb(226, 232, 240);
            divider.Location = new Point(24, 250);
            divider.Size = new Size(302, 1);

            _schoolIdValue = AddDetailRow(sidebar, 266, "SCHOOL ID");
            _programValue = AddDetailRow(sidebar, 298, "PROGRAM");
            _yearValue = AddDetailRow(sidebar, 330, "YEAR LEVEL");
            _instituteValue = AddDetailRow(sidebar, 362, "INSTITUTE");
            _contactValue = AddDetailRow(sidebar, 394, "CONTACT");
            _addressValue = AddDetailRow(sidebar, 426, "ADDRESS");

            Label qrHeading = new Label();
            qrHeading.AutoSize = true;
            qrHeading.BackColor = Color.Transparent;
            qrHeading.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            qrHeading.ForeColor = AccentGreen();
            qrHeading.Location = new Point(24, 470);
            qrHeading.Text = "STUDENT QR CODE";

            _qrPicture = new PictureBox();
            _qrPicture.BackColor = Color.White;
            _qrPicture.BorderStyle = BorderStyle.FixedSingle;
            _qrPicture.Location = new Point(70, 498);
            _qrPicture.Size = new Size(210, 210);
            _qrPicture.SizeMode = PictureBoxSizeMode.Zoom;
            _qrPicture.TabStop = false;

            _qrCaption = new Label();
            _qrCaption.BackColor = Color.Transparent;
            _qrCaption.Font = new Font("Segoe UI", 8.25F);
            _qrCaption.ForeColor = MutedText;
            _qrCaption.Location = new Point(24, 718);
            _qrCaption.Size = new Size(302, 36);
            _qrCaption.Text = "SHOW THIS CODE TO THE LIBRARIAN WHEN YOU PICK UP OR RETURN A BOOK.";
            _qrCaption.TextAlign = ContentAlignment.MiddleCenter;

            sidebar.Controls.Add(_qrCaption);
            sidebar.Controls.Add(_qrPicture);
            sidebar.Controls.Add(qrHeading);
            sidebar.Controls.Add(divider);
            sidebar.Controls.Add(_profileMeta);
            sidebar.Controls.Add(_profileName);
            sidebar.Controls.Add(_avatar);
            sidebar.Controls.Add(heading);

            sidebar.Resize += delegate
            {
                int left = sidebar.Padding.Left;
                int width = Math.Max(100, sidebar.ClientSize.Width - sidebar.Padding.Horizontal);

                _profileName.Left = left;
                _profileName.Width = width;
                _profileMeta.Left = left;
                _profileMeta.Width = width;
                divider.Left = left;
                divider.Width = width;
                _avatar.Left = left + Math.Max(0, (width - _avatar.Width) / 2);
                _qrPicture.Left = left + Math.Max(0, (width - _qrPicture.Width) / 2);
                _qrCaption.Left = left;
                _qrCaption.Width = width;
            };

            return sidebar;
        }

        private Label AddDetailRow(Control parent, int top, string caption)
        {
            Label captionLabel = new Label();
            captionLabel.BackColor = Color.Transparent;
            captionLabel.Font = new Font("Segoe UI", 7.75F, FontStyle.Bold);
            captionLabel.ForeColor = Color.FromArgb(100, 116, 139);
            captionLabel.Location = new Point(24, top);
            captionLabel.Size = new Size(98, 22);
            captionLabel.Text = caption;
            captionLabel.TextAlign = ContentAlignment.MiddleLeft;

            Label value = new Label();
            value.AutoEllipsis = true;
            value.BackColor = Color.Transparent;
            value.Font = new Font("Segoe UI", 8.75F);
            value.ForeColor = Color.FromArgb(15, 23, 42);
            value.Location = new Point(128, top);
            value.Size = new Size(198, 22);
            value.Text = "—";
            value.TextAlign = ContentAlignment.MiddleLeft;

            parent.Controls.Add(value);
            parent.Controls.Add(captionLabel);
            return value;
        }

        private Panel CreateMainContent()
        {
            Panel content = new Panel();
            content.BackColor = Color.FromArgb(241, 245, 249);
            content.Dock = DockStyle.Fill;
            content.Padding = new Padding(18, 14, 18, 18);

            _tabs = new TabControl();
            _tabs.Dock = DockStyle.Fill;
            _tabs.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _tabs.Padding = new Point(14, 6);

            _catalogPage = new TabPage("BOOK CATALOG");
            _catalogPage.BackColor = Color.White;
            _catalogPage.Padding = new Padding(16);
            BuildCatalogPage(_catalogPage);

            _requestsPage = new TabPage("MY REQUESTS");
            _requestsPage.BackColor = Color.White;
            _requestsPage.Padding = new Padding(16);
            BuildRequestsPage(_requestsPage);

            _loansPage = new TabPage("MY BORROWED BOOKS");
            _loansPage.BackColor = Color.White;
            _loansPage.Padding = new Padding(16);
            BuildLoansPage(_loansPage);

            _tabs.TabPages.Add(_catalogPage);
            _tabs.TabPages.Add(_requestsPage);
            _tabs.TabPages.Add(_loansPage);
            _tabs.SelectedIndexChanged += Tabs_SelectedIndexChanged;

            content.Controls.Add(_tabs);
            return content;
        }

        private void BuildCatalogPage(TabPage page)
        {
            TableLayoutPanel layout = new TableLayoutPanel();
            layout.BackColor = Color.White;
            layout.ColumnCount = 1;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.Dock = DockStyle.Fill;
            layout.RowCount = 2;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 88F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            Panel head = new Panel();
            head.BackColor = Color.White;
            head.Dock = DockStyle.Fill;

            Label heading = new Label();
            heading.AutoSize = true;
            heading.Font = new Font("Segoe UI", 14.5F, FontStyle.Bold);
            heading.ForeColor = Color.FromArgb(15, 23, 42);
            heading.Location = new Point(0, 0);
            heading.Text = "BOOK CATALOG";

            Label subtitle = new Label();
            subtitle.AutoSize = true;
            subtitle.Font = new Font("Segoe UI", 9F);
            subtitle.ForeColor = MutedText;
            subtitle.Location = new Point(2, 32);
            subtitle.Text = "Select a title, review the cover, then request it from the librarian.";

            RoundedButton refresh = CreateButton(
                "REFRESH", new Point(700, 4), new Size(116, 38),
                Color.FromArgb(22, 163, 74), Color.FromArgb(21, 128, 61), Color.FromArgb(20, 83, 45));
            refresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            refresh.Click += delegate { LoadCatalogData(); };

            _catalogStatus = new Label();
            _catalogStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _catalogStatus.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            _catalogStatus.ForeColor = MutedText;
            _catalogStatus.Location = new Point(2, 60);
            _catalogStatus.Size = new Size(800, 22);
            _catalogStatus.Text = "CATALOG HAS NOT BEEN LOADED YET.";

            head.Resize += delegate
            {
                refresh.Left = Math.Max(0, head.ClientSize.Width - refresh.Width);
                _catalogStatus.Width = Math.Max(100, head.ClientSize.Width - 130);
            };

            head.Controls.Add(_catalogStatus);
            head.Controls.Add(refresh);
            head.Controls.Add(subtitle);
            head.Controls.Add(heading);

            _catalogSplit = new SplitContainer();
            _catalogSplit.BackColor = Color.FromArgb(241, 245, 249);
            _catalogSplit.Dock = DockStyle.Fill;
            _catalogSplit.FixedPanel = FixedPanel.Panel2;
            _catalogSplit.Orientation = Orientation.Vertical;
            _catalogSplit.Panel1.BackColor = Color.White;
            _catalogSplit.Panel2.BackColor = Color.White;
            _catalogSplit.SplitterWidth = 8;

            _catalogGrid = new DataGridView();
            _catalogGrid.Dock = DockStyle.Fill;
            _catalogGrid.Name = "studentCatalogGrid";
            _catalogGrid.RowTemplate.Height = 92;
            StyleGrid(_catalogGrid);

            DataGridViewTextBoxColumn idColumn = CreateTextColumn("studentBookId", "ID", 55, false);
            DataGridViewImageColumn coverColumn = new DataGridViewImageColumn();
            coverColumn.Name = "studentBookCover";
            coverColumn.HeaderText = "COVER";
            coverColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            coverColumn.Width = 86;
            coverColumn.MinimumWidth = 80;
            coverColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
            coverColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            coverColumn.DefaultCellStyle.Padding = new Padding(4);
            coverColumn.ReadOnly = true;
            coverColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            DataGridViewTextBoxColumn titleColumn = CreateTextColumn("studentBookTitle", "BOOK TITLE", 160, true, 30F);
            DataGridViewTextBoxColumn authorColumn = CreateTextColumn("studentBookAuthor", "AUTHOR", 130, true, 20F);
            DataGridViewTextBoxColumn yearColumn = CreateTextColumn("studentBookYear", "YEAR", 68, false);
            DataGridViewTextBoxColumn availableColumn = CreateTextColumn("studentBookAvailable", "AVAILABLE COPY", 112, false);
            DataGridViewTextBoxColumn statusColumn = CreateTextColumn("studentBookStatus", "STATUS", 116, false);
            statusColumn.DefaultCellStyle.Font = new Font("Segoe UI", 8.75F, FontStyle.Bold);

            _catalogGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                idColumn, coverColumn, titleColumn, authorColumn, yearColumn, availableColumn, statusColumn
            });
            _catalogGrid.SelectionChanged += delegate { ShowSelectedBook(); };
            _catalogGrid.CellDoubleClick += delegate(object? sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex >= 0)
                {
                    RequestSelectedBook();
                }
            };

            _catalogEmptyLabel = new Label();
            _catalogEmptyLabel.BackColor = Color.White;
            _catalogEmptyLabel.Dock = DockStyle.Fill;
            _catalogEmptyLabel.Font = new Font("Segoe UI", 10F);
            _catalogEmptyLabel.ForeColor = MutedText;
            _catalogEmptyLabel.Text = "NO BOOKS IN THE CATALOG YET.\r\nPLEASE ASK THE LIBRARIAN TO ADD A TITLE.";
            _catalogEmptyLabel.TextAlign = ContentAlignment.MiddleCenter;
            _catalogEmptyLabel.Visible = false;

            _catalogSplit.Panel1.Controls.Add(_catalogEmptyLabel);
            _catalogSplit.Panel1.Controls.Add(_catalogGrid);
            _catalogSplit.Panel2.Controls.Add(CreateBookDetailsPanel());
            _catalogSplit.Resize += delegate
            {
                if (_catalogSplit.Width <= 760)
                {
                    return;
                }

                try
                {
                    int target = Math.Max(400, _catalogSplit.Width - 360);
                    if (target < _catalogSplit.Width - _catalogSplit.Panel2MinSize)
                    {
                        _catalogSplit.SplitterDistance = target;
                    }
                }
                catch (InvalidOperationException)
                {
                    // The splitter is measured before the page is visible; the
                    // next resize will place the details panel.
                }
            };

            layout.Controls.Add(head, 0, 0);
            layout.Controls.Add(_catalogSplit, 0, 1);
            page.Controls.Add(layout);
        }

        private Panel CreateBookDetailsPanel()
        {
            Panel details = new Panel();
            details.BackColor = Color.White;
            details.Dock = DockStyle.Fill;
            details.Padding = new Padding(16, 10, 16, 10);

            Label heading = new Label();
            heading.AutoSize = true;
            heading.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            heading.ForeColor = AccentGreen();
            heading.Location = new Point(0, 0);
            heading.Text = "BOOK DETAILS";

            _detailCover = new PictureBox();
            _detailCover.BackColor = Color.FromArgb(248, 250, 252);
            _detailCover.BorderStyle = BorderStyle.FixedSingle;
            _detailCover.Location = new Point(28, 30);
            _detailCover.Size = new Size(210, 250);
            _detailCover.SizeMode = PictureBoxSizeMode.Zoom;
            _detailCover.TabStop = false;

            _detailTitle = new Label();
            _detailTitle.AutoEllipsis = true;
            _detailTitle.Font = new Font("Segoe UI", 12.5F, FontStyle.Bold);
            _detailTitle.ForeColor = Color.FromArgb(15, 23, 42);
            _detailTitle.Location = new Point(0, 292);
            _detailTitle.Size = new Size(300, 46);

            _detailAuthor = new Label();
            _detailAuthor.AutoEllipsis = true;
            _detailAuthor.Font = new Font("Segoe UI", 9.25F);
            _detailAuthor.ForeColor = Color.FromArgb(71, 85, 105);
            _detailAuthor.Location = new Point(0, 342);
            _detailAuthor.Size = new Size(300, 22);

            _detailMeta = new Label();
            _detailMeta.AutoEllipsis = true;
            _detailMeta.Font = new Font("Segoe UI", 8.75F);
            _detailMeta.ForeColor = MutedText;
            _detailMeta.Location = new Point(0, 366);
            _detailMeta.Size = new Size(300, 22);

            _detailAvailability = new Label();
            _detailAvailability.AutoEllipsis = true;
            _detailAvailability.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _detailAvailability.ForeColor = SuccessText;
            _detailAvailability.Location = new Point(0, 392);
            _detailAvailability.Size = new Size(300, 22);

            _detailHint = new Label();
            _detailHint.Font = new Font("Segoe UI", 8.25F);
            _detailHint.ForeColor = MutedText;
            _detailHint.Location = new Point(0, 418);
            _detailHint.Size = new Size(300, 38);
            _detailHint.Text = "Choose a book in the catalog to see its picture and request it.";

            _requestButton = CreateButton(
                "REQUEST TO BORROW", new Point(0, 464), new Size(300, 46),
                Color.FromArgb(22, 163, 74), Color.FromArgb(21, 128, 61), Color.FromArgb(20, 83, 45));
            _requestButton.Enabled = false;
            _requestButton.Click += delegate { RequestSelectedBook(); };

            details.Controls.Add(_requestButton);
            details.Controls.Add(_detailHint);
            details.Controls.Add(_detailAvailability);
            details.Controls.Add(_detailMeta);
            details.Controls.Add(_detailAuthor);
            details.Controls.Add(_detailTitle);
            details.Controls.Add(_detailCover);
            details.Controls.Add(heading);

            details.Resize += delegate
            {
                int width = Math.Max(160, details.ClientSize.Width - details.Padding.Horizontal);
                int left = Math.Max(0, (width - _detailCover.Width) / 2);
                _detailCover.Left = left + details.Padding.Left;
                _detailTitle.Width = width;
                _detailAuthor.Width = width;
                _detailMeta.Width = width;
                _detailAvailability.Width = width;
                _detailHint.Width = width;
                _requestButton.Left = details.Padding.Left;
                _requestButton.Width = width;
            };

            ShowBookDetails(null);
            return details;
        }

        private void BuildRequestsPage(TabPage page)
        {
            TableLayoutPanel layout = new TableLayoutPanel();
            layout.BackColor = Color.White;
            layout.ColumnCount = 1;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.Dock = DockStyle.Fill;
            layout.RowCount = 2;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 88F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            Panel head = new Panel();
            head.BackColor = Color.White;
            head.Dock = DockStyle.Fill;

            Label heading = new Label();
            heading.AutoSize = true;
            heading.Font = new Font("Segoe UI", 14.5F, FontStyle.Bold);
            heading.ForeColor = Color.FromArgb(15, 23, 42);
            heading.Location = new Point(0, 0);
            heading.Text = "MY REQUESTS";

            Label subtitle = new Label();
            subtitle.AutoSize = true;
            subtitle.Font = new Font("Segoe UI", 9F);
            subtitle.ForeColor = MutedText;
            subtitle.Location = new Point(2, 32);
            subtitle.Text = "Pending requests are reviewed by the librarian.";

            RoundedButton refresh = CreateButton(
                "REFRESH", new Point(700, 4), new Size(116, 38),
                Color.FromArgb(22, 163, 74), Color.FromArgb(21, 128, 61), Color.FromArgb(20, 83, 45));
            refresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            refresh.Click += delegate { LoadRequests(); };

            _requestsStatus = new Label();
            _requestsStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _requestsStatus.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            _requestsStatus.ForeColor = MutedText;
            _requestsStatus.Location = new Point(2, 60);
            _requestsStatus.Size = new Size(800, 22);
            _requestsStatus.Text = "REQUESTS HAVE NOT BEEN LOADED YET.";

            head.Resize += delegate
            {
                refresh.Left = Math.Max(0, head.ClientSize.Width - refresh.Width);
                _requestsStatus.Width = Math.Max(100, head.ClientSize.Width - 130);
            };
            head.Controls.Add(_requestsStatus);
            head.Controls.Add(refresh);
            head.Controls.Add(subtitle);
            head.Controls.Add(heading);

            _requestGrid = new DataGridView();
            _requestGrid.Dock = DockStyle.Fill;
            _requestGrid.Name = "studentRequestGrid";
            StyleGrid(_requestGrid);

            DataGridViewTextBoxColumn idColumn = CreateTextColumn("studentRequestId", "ID", 58, false);
            DataGridViewTextBoxColumn titleColumn = CreateTextColumn("studentRequestTitle", "BOOK TITLE", 160, true, 28F);
            DataGridViewTextBoxColumn dateColumn = CreateTextColumn("studentRequestDate", "REQUESTED", 150, false);
            DataGridViewTextBoxColumn statusColumn = CreateTextColumn("studentRequestStatus", "STATUS", 104, false);
            statusColumn.DefaultCellStyle.Font = new Font("Segoe UI", 8.75F, FontStyle.Bold);
            DataGridViewTextBoxColumn processedColumn = CreateTextColumn("studentRequestProcessed", "PROCESSED", 150, false);
            DataGridViewTextBoxColumn noteColumn = CreateTextColumn("studentRequestNote", "NOTE", 130, true, 16F);
            DataGridViewTextBoxColumn actionColumn = CreateTextColumn("studentRequestAction", "ACTION", 96, false);
            actionColumn.DefaultCellStyle.Font = new Font("Segoe UI", 8.75F, FontStyle.Bold);
            actionColumn.DefaultCellStyle.ForeColor = DangerText;
            actionColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            _requestGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                idColumn, titleColumn, dateColumn, statusColumn, processedColumn, noteColumn, actionColumn
            });
            _requestGrid.CellClick += RequestGrid_CellClick;

            layout.Controls.Add(head, 0, 0);
            layout.Controls.Add(_requestGrid, 0, 1);
            page.Controls.Add(layout);
        }

        private void BuildLoansPage(TabPage page)
        {
            TableLayoutPanel layout = new TableLayoutPanel();
            layout.BackColor = Color.White;
            layout.ColumnCount = 1;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.Dock = DockStyle.Fill;
            layout.RowCount = 2;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 88F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            Panel head = new Panel();
            head.BackColor = Color.White;
            head.Dock = DockStyle.Fill;

            Label heading = new Label();
            heading.AutoSize = true;
            heading.Font = new Font("Segoe UI", 14.5F, FontStyle.Bold);
            heading.ForeColor = Color.FromArgb(15, 23, 42);
            heading.Location = new Point(0, 0);
            heading.Text = "MY BORROWED BOOKS";

            Label subtitle = new Label();
            subtitle.AutoSize = true;
            subtitle.Font = new Font("Segoe UI", 9F);
            subtitle.ForeColor = MutedText;
            subtitle.Location = new Point(2, 32);
            subtitle.Text = "The same borrow records the librarian sees at the desk.";

            RoundedButton refresh = CreateButton(
                "REFRESH", new Point(700, 4), new Size(116, 38),
                Color.FromArgb(22, 163, 74), Color.FromArgb(21, 128, 61), Color.FromArgb(20, 83, 45));
            refresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            refresh.Click += delegate { LoadLoans(); };

            _loansStatus = new Label();
            _loansStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _loansStatus.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            _loansStatus.ForeColor = MutedText;
            _loansStatus.Location = new Point(2, 60);
            _loansStatus.Size = new Size(800, 22);
            _loansStatus.Text = "BORROW RECORDS HAVE NOT BEEN LOADED YET.";

            head.Resize += delegate
            {
                refresh.Left = Math.Max(0, head.ClientSize.Width - refresh.Width);
                _loansStatus.Width = Math.Max(100, head.ClientSize.Width - 130);
            };
            head.Controls.Add(_loansStatus);
            head.Controls.Add(refresh);
            head.Controls.Add(subtitle);
            head.Controls.Add(heading);

            _loanGrid = new DataGridView();
            _loanGrid.Dock = DockStyle.Fill;
            _loanGrid.Name = "studentLoanGrid";
            StyleGrid(_loanGrid);

            DataGridViewTextBoxColumn idColumn = CreateTextColumn("studentLoanId", "ID", 58, false);
            DataGridViewTextBoxColumn titleColumn = CreateTextColumn("studentLoanTitle", "BOOK TITLE", 170, true, 30F);
            DataGridViewTextBoxColumn authorColumn = CreateTextColumn("studentLoanAuthor", "AUTHOR", 140, true, 20F);
            DataGridViewTextBoxColumn yearColumn = CreateTextColumn("studentLoanYear", "YEAR", 68, false);
            DataGridViewTextBoxColumn borrowedColumn = CreateTextColumn("studentLoanBorrowed", "BORROWED", 140, false);
            DataGridViewTextBoxColumn daysColumn = CreateTextColumn("studentLoanDays", "DAYS", 90, false);
            DataGridViewTextBoxColumn returnedColumn = CreateTextColumn("studentLoanReturned", "RETURNED", 140, false);
            DataGridViewTextBoxColumn statusColumn = CreateTextColumn("studentLoanStatus", "STATUS", 104, false);
            statusColumn.DefaultCellStyle.Font = new Font("Segoe UI", 8.75F, FontStyle.Bold);

            _loanGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                idColumn, titleColumn, authorColumn, yearColumn, borrowedColumn, daysColumn, returnedColumn, statusColumn
            });

            layout.Controls.Add(head, 0, 0);
            layout.Controls.Add(_loanGrid, 0, 1);
            page.Controls.Add(layout);
        }

        private void StudentDashboard_Shown(object? sender, EventArgs e)
        {
            LoadProfile();
            LoadCatalogData();
            LoadRequests();
            LoadLoans();
        }

        private void LoadProfile()
        {
            try
            {
                _profile = _directory.LoadProfile(_accountId);
            }
            catch (MySqlException)
            {
                _profile = null;
            }

            ApplyProfile();
        }

        private void ApplyProfile()
        {
            string name = _profile == null ? _fullName : _profile.Name;
            string role = _profile == null ? _role : _profile.Role;

            _profileName.Text = name;
            _profileMeta.Text = "ACCOUNT #" + _accountId.ToString(CultureInfo.InvariantCulture) +
                                (_username.Length > 0 ? "     •     @" + _username : string.Empty);
            _avatar.Initials = Initials(name);
            _roleBadge.Text = role;

            _schoolIdValue.Text = Value(_profile?.SchoolId);
            _programValue.Text = Value(_profile?.Program);
            _yearValue.Text = Value(_profile?.YearLevel);
            _instituteValue.Text = Value(_profile?.Institute);
            _contactValue.Text = Value(_profile?.ContactNumber);
            _addressValue.Text = Value(_profile?.Address);

            BuildQrCode(name, role);
        }

        private void BuildQrCode(string name, string role)
        {
            string schoolId = Value(_profile?.SchoolId);
            string program = Value(_profile?.Program);
            string year = Value(_profile?.YearLevel);
            string institute = Value(_profile?.Institute);
            string contact = Value(_profile?.ContactNumber);

            StringBuilder text = new StringBuilder();
            text.AppendLine("TCGC LIBRARY SYSTEM");
            text.AppendLine("STUDENT PROFILE");
            text.AppendLine("NAME: " + name);
            text.AppendLine("STUDENT ID: " + schoolId);
            text.AppendLine("PROGRAM: " + program);
            text.AppendLine("YEAR: " + year);
            text.AppendLine("INSTITUTE: " + institute);
            text.AppendLine("CONTACT: " + contact);
            text.AppendLine("ACCOUNT: " + _accountId.ToString(CultureInfo.InvariantCulture));
            text.AppendLine("ROLE: " + role);

            try
            {
                using (QRCodeGenerator generator = new QRCodeGenerator())
                using (QRCodeData data = generator.CreateQrCode(text.ToString(), QRCodeGenerator.ECCLevel.M))
                {
                    QRCode code = new QRCode(data);
                    _qrImage?.Dispose();
                    _qrImage = code.GetGraphic(6, HeaderGreen, Color.White, true);
                    _qrPicture.Image = _qrImage;
                }

                _qrCaption.Text = "SCAN FOR " + name.ToUpperInvariant() + "'S STUDENT INFORMATION.";
            }
            catch (Exception)
            {
                _qrImage?.Dispose();
                _qrImage = null;
                _qrPicture.Image = null;
                _qrCaption.Text = "THE QR CODE COULD NOT BE CREATED FOR THIS PROFILE.";
            }
        }

        private void LoadCatalogData()
        {
            _catalog.Load();

            DisposeCoverImages();
            _catalogGrid.Rows.Clear();

            foreach (Book book in _catalog.Books)
            {
                int available = _catalog.AvailableCopyCount(book);
                string status = available > 0 ? Book.AvailableStatus : "ALL COPIES BORROWED";

                int index = _catalogGrid.Rows.Add(
                    book.BookId,
                    BookImage.CreateThumbnail(book.CoverImage, 78, 84, true),
                    book.Title,
                    book.Author,
                    book.YearText,
                    available.ToString(CultureInfo.InvariantCulture),
                    status);

                DataGridViewRow row = _catalogGrid.Rows[index];
                row.Tag = book;
                row.Cells[6].Style.ForeColor = available > 0 ? SuccessText : DangerText;
                row.Cells[5].Style.ForeColor = available > 0 ? Color.FromArgb(15, 23, 42) : DangerText;
            }

            _catalogEmptyLabel.Visible = _catalogGrid.Rows.Count == 0;
            if (_catalogEmptyLabel.Visible)
            {
                _catalogEmptyLabel.BringToFront();
            }

            _catalogStatus.ForeColor = _catalog.IsConnected ? MutedText : WarningText;
            _catalogStatus.Text = _catalog.IsConnected
                ? "CATALOG SYNCED WITH MYSQL • " + _catalog.Books.Count.ToString(CultureInfo.InvariantCulture) + " TITLE(S)"
                : "MYSQL OFFLINE • THE CATALOG COULD NOT BE LOADED";

            _catalogGrid.ClearSelection();
            _catalogGrid.CurrentCell = null;
            ShowBookDetails(null);
        }

        private void ShowSelectedBook()
        {
            Book? book = null;

            if (_catalogGrid.CurrentRow != null)
            {
                book = _catalogGrid.CurrentRow.Tag as Book;
            }

            ShowBookDetails(book);
        }

        private void ShowBookDetails(Book? book)
        {
            _selectedBook = book;
            _detailImage?.Dispose();
            _detailImage = null;

            if (book == null)
            {
                _detailImage = BookImage.CreateThumbnail(null, 210, 250, true);
                _detailCover.Image = _detailImage;
                _detailTitle.Text = "SELECT A BOOK";
                _detailAuthor.Text = "Choose a row in the catalog.";
                _detailMeta.Text = string.Empty;
                _detailAvailability.Text = string.Empty;
                _detailHint.Text = "The cover, title, author and available copies are shown here.";
                _requestButton.Text = "REQUEST TO BORROW";
                _requestButton.Enabled = false;
                return;
            }

            _detailImage = BookImage.CreateThumbnail(book.CoverImage, 210, 250, true);
            _detailCover.Image = _detailImage;
            _detailTitle.Text = book.Title;
            _detailAuthor.Text = "BY " + book.Author;
            _detailMeta.Text = "YEAR " + book.YearText + "     •     " + _catalog.AvailableCopyCount(book) +
                               " OF " + book.Quantity + " COPIES AVAILABLE";

            bool pending = HasPendingRequest(book.BookId);
            bool activeLoan = HasActiveLoan(book.BookId);
            int available = _catalog.AvailableCopyCount(book);

            if (pending)
            {
                _detailAvailability.Text = "REQUEST PENDING LIBRARIAN APPROVAL";
                _detailAvailability.ForeColor = WarningText;
                _requestButton.Text = "REQUEST PENDING";
                _requestButton.Enabled = false;
            }
            else if (activeLoan)
            {
                _detailAvailability.Text = "YOU ALREADY HAVE THIS BOOK ON LOAN";
                _detailAvailability.ForeColor = WarningText;
                _requestButton.Text = "ALREADY BORROWED";
                _requestButton.Enabled = false;
            }
            else if (available <= 0)
            {
                _detailAvailability.Text = "ALL COPIES ARE CURRENTLY BORROWED";
                _detailAvailability.ForeColor = DangerText;
                _requestButton.Text = "NO COPY AVAILABLE";
                _requestButton.Enabled = false;
            }
            else
            {
                _detailAvailability.Text = "READY TO REQUEST";
                _detailAvailability.ForeColor = SuccessText;
                _requestButton.Text = "REQUEST TO BORROW";
                _requestButton.Enabled = true;
            }

            _detailHint.Text = "Click the button below and the librarian will review your request.";
        }

        private void RequestSelectedBook()
        {
            if (_selectedBook == null)
            {
                return;
            }

            DialogResult answer = MessageBox.Show(
                "REQUEST \"" + _selectedBook.Title.ToUpperInvariant() + "\" FROM THE LIBRARY?\n\n" +
                "THE LIBRARIAN WILL APPROVE OR REJECT YOUR REQUEST.",
                "CONFIRM BORROW REQUEST",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (answer != DialogResult.Yes)
            {
                return;
            }

            try
            {
                _requestDirectory.Create(_accountId, _selectedBook);
                LoadRequests();
                ShowBookDetails(_selectedBook);
                MessageBox.Show(
                    "YOUR REQUEST WAS SENT. WAIT FOR THE LIBRARIAN TO APPROVE IT.",
                    "REQUEST SENT",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                MessageBox.Show(ex.Message.ToUpperInvariant(), "REQUEST NOT SENT", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(
                    "THE REQUEST COULD NOT BE SAVED.\n" + ex.Message.ToUpperInvariant(),
                    "DATABASE ERROR",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void LoadRequests()
        {
            try
            {
                _studentRequests = _requestDirectory.LoadForAccount(_accountId);
            }
            catch (MySqlException)
            {
                _studentRequests = new List<BorrowRequestRow>();
                _requestsStatus.Text = "MYSQL OFFLINE • REQUESTS CANNOT BE LOADED";
                _requestsStatus.ForeColor = WarningText;
                _requestGrid.Rows.Clear();
                return;
            }

            _requestGrid.Rows.Clear();

            foreach (BorrowRequestRow request in _studentRequests)
            {
                int index = _requestGrid.Rows.Add(
                    request.RequestId,
                    request.BookTitle,
                    request.RequestDateText,
                    request.Status,
                    request.ProcessedDateText,
                    request.NoteText,
                    request.IsPending ? "CANCEL" : string.Empty);

                DataGridViewRow row = _requestGrid.Rows[index];
                row.Tag = request;
                DataGridViewCell statusCell = row.Cells[3];
                statusCell.Style.Font = new Font("Segoe UI", 8.75F, FontStyle.Bold);

                if (request.IsApproved)
                {
                    statusCell.Style.ForeColor = SuccessText;
                }
                else if (request.IsRejected)
                {
                    statusCell.Style.ForeColor = DangerText;
                }
                else if (string.Equals(request.Status, "CANCELLED", StringComparison.OrdinalIgnoreCase))
                {
                    statusCell.Style.ForeColor = MutedText;
                }
                else
                {
                    statusCell.Style.ForeColor = WarningText;
                }
            }

            int pending = 0;
            foreach (BorrowRequestRow request in _studentRequests)
            {
                if (request.IsPending)
                {
                    pending++;
                }
            }

            _requestsStatus.Text = _studentRequests.Count == 0
                ? "NO REQUESTS YET. OPEN THE BOOK CATALOG TO REQUEST A TITLE."
                : _studentRequests.Count.ToString(CultureInfo.InvariantCulture) + " REQUEST(S) ON FILE • " +
                  pending.ToString(CultureInfo.InvariantCulture) + " PENDING";
            _requestsStatus.ForeColor = MutedText;

            _requestGrid.ClearSelection();
            _requestGrid.CurrentCell = null;

            if (_selectedBook != null)
            {
                ShowBookDetails(_selectedBook);
            }
        }

        private void RequestGrid_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || _requestGrid.Columns[e.ColumnIndex].Name != "studentRequestAction")
            {
                return;
            }

            BorrowRequestRow? request = _requestGrid.Rows[e.RowIndex].Tag as BorrowRequestRow;

            if (request == null || !request.IsPending)
            {
                return;
            }

            DialogResult answer = MessageBox.Show(
                "CANCEL REQUEST #" + request.RequestId + " FOR \"" + request.BookTitle.ToUpperInvariant() + "\"?",
                "CANCEL REQUEST",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (answer != DialogResult.Yes)
            {
                return;
            }

            try
            {
                _requestDirectory.Cancel(request.RequestId, _accountId);
                LoadRequests();
                ShowBookDetails(_selectedBook);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                MessageBox.Show(ex.Message.ToUpperInvariant(), "REQUEST NOT CANCELLED", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message.ToUpperInvariant(), "DATABASE ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadLoans()
        {
            _loanGrid.Rows.Clear();

            int count = 0;
            foreach (BorrowRecord record in _catalog.BorrowRecords)
            {
                if (!IsMyLoan(record))
                {
                    continue;
                }

                int index = _loanGrid.Rows.Add(
                    record.BorrowId,
                    record.BookTitle,
                    record.Author,
                    record.BookYearText,
                    record.DateBorrowedText,
                    record.DaysBorrowedText,
                    record.DateReturnedText,
                    record.Status);

                DataGridViewRow row = _loanGrid.Rows[index];
                row.Tag = record;
                DataGridViewCell statusCell = row.Cells[7];
                statusCell.Style.Font = new Font("Segoe UI", 8.75F, FontStyle.Bold);
                statusCell.Style.ForeColor = record.DateReturned.HasValue ? MutedText : SuccessText;
                count++;
            }

            _loansStatus.Text = count == 0
                ? "YOU HAVE NO BORROW RECORDS YET."
                : count.ToString(CultureInfo.InvariantCulture) + " BORROW RECORD(S) FOUND FOR " +
                  Value(_profile?.SchoolId).ToUpperInvariant();
            _loansStatus.ForeColor = MutedText;

            _loanGrid.ClearSelection();
            _loanGrid.CurrentCell = null;
        }

        private bool IsMyLoan(BorrowRecord record)
        {
            string myId = CleanValue(_profile?.SchoolId);
            if (myId.Length > 0 && string.Equals(record.BorrowerIdNumber, myId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            string myName = CleanValue(_profile?.Name);
            return myName.Length > 0 && string.Equals(record.BorrowerName, myName, StringComparison.OrdinalIgnoreCase);
        }

        private bool HasPendingRequest(int bookId)
        {
            foreach (BorrowRequestRow request in _studentRequests)
            {
                if (request.IsPending && request.BookId == bookId)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasActiveLoan(int bookId)
        {
            foreach (BorrowRecord record in _catalog.BorrowRecords)
            {
                if (!record.DateReturned.HasValue && record.BookId == bookId && IsMyLoan(record))
                {
                    return true;
                }
            }

            return false;
        }

        private void Tabs_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_tabs.SelectedTab == _requestsPage)
            {
                LoadRequests();
            }
            else if (_tabs.SelectedTab == _loansPage)
            {
                LoadLoans();
            }
        }

        private void SignOut_Click(object? sender, EventArgs e)
        {
            DialogResult answer = MessageBox.Show(
                "SIGN OUT OF THE STUDENT PORTAL?",
                "SIGN OUT",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (answer != DialogResult.Yes)
            {
                return;
            }

            Hide();
            new Login().Show();
            Close();
        }

        private void DisposeCoverImages()
        {
            foreach (DataGridViewRow row in _catalogGrid.Rows)
            {
                if (row.Cells[1].Value is Image image)
                {
                    image.Dispose();
                }
            }
        }

        private static string Value(string? value)
        {
            return string.IsNullOrWhiteSpace(value) || value == "—" ? "—" : value.Trim();
        }

        private static string CleanValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value) || value == "—" ? string.Empty : value.Trim();
        }

        private static string Initials(string value)
        {
            string[] parts = (value ?? string.Empty).Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
            {
                return "ST";
            }

            if (parts.Length == 1)
            {
                return parts[0].Length >= 2 ? parts[0].Substring(0, 2).ToUpperInvariant() : parts[0].ToUpperInvariant();
            }

            return (parts[0][0].ToString() + parts[1][0].ToString()).ToUpperInvariant();
        }

        private static Color AccentGreen()
        {
            return Color.FromArgb(4, 120, 87);
        }

        private static RoundedButton CreateButton(
            string text, Point location, Size size, Color normal, Color hover, Color pressed)
        {
            RoundedButton button = new RoundedButton();
            button.CornerRadius = 11;
            button.FlatAppearance.BorderSize = 0;
            button.FlatStyle = FlatStyle.Flat;
            button.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            button.ForeColor = Color.White;
            button.HoverColor = hover;
            button.Location = location;
            button.NormalColor = normal;
            button.PressedColor = pressed;
            button.Size = size;
            button.Text = text;
            button.UseVisualStyleBackColor = false;
            return button;
        }

        private static DataGridViewTextBoxColumn CreateTextColumn(
            string name, string header, int width, bool fill, float weight = 0F)
        {
            DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
            column.Name = name;
            column.HeaderText = header;
            column.ReadOnly = true;
            column.SortMode = DataGridViewColumnSortMode.NotSortable;

            if (fill)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                column.FillWeight = weight;
                column.MinimumWidth = width;
            }
            else
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                column.Width = width;
                column.MinimumWidth = width;
            }

            column.HeaderCell.Style.WrapMode = DataGridViewTriState.False;
            return column;
        }

        private static void StyleGrid(DataGridView grid)
        {
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.ColumnHeadersHeight = 42;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.EnableHeadersVisualStyles = false;
            grid.GridColor = Color.FromArgb(241, 245, 249);
            grid.MultiSelect = false;
            grid.ReadOnly = true;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            grid.ColumnHeadersDefaultCellStyle.BackColor = HeaderGreen;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = HeaderGreen;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;

            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            grid.DefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 252, 231);
            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(6, 78, 59);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 252, 231);
        }
    }
}
