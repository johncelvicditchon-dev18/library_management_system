using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using oop_project.Models;
using QRCoder;

namespace oop_project.forms
{
    /// <summary>
    /// Shared portal for library patrons: students and faculty.
    ///
    /// The page follows the LRC reference screen: the account identity sits in
    /// the green banner, the three jobs a borrower needs are one click away in
    /// the navigation bar, and the catalog itself is a grid of book cards with
    /// the green REQUEST TO BORROW button under every cover.
    ///
    /// The layout is produced by <see cref="InitializeUi"/>, which the designer
    /// file invokes from InitializeComponent. Because the Windows Forms designer
    /// creates an instance of the form (and therefore runs its base class
    /// constructor), patron_dashboard, student_dashboard and faculty_dashboard all
    /// open with the complete portal visible on the design surface.
    /// </summary>
    public partial class PatronDashboardBase : Form
    {
        private static readonly Color PageBack = Color.FromArgb(241, 245, 249);
        private static readonly Color CardBorder = Color.FromArgb(226, 232, 240);
        private static readonly Color HeaderGreen = Color.FromArgb(6, 78, 59);
        private static readonly Color MutedText = Color.FromArgb(100, 116, 139);
        private static readonly Color SuccessText = Color.FromArgb(4, 120, 87);
        private static readonly Color WarningText = Color.FromArgb(180, 83, 9);
        private static readonly Color DangerText = Color.FromArgb(185, 28, 28);

        private readonly LibraryCatalog _catalog = new LibraryCatalog();
        private readonly AccountDirectory _directory = new AccountDirectory();
        private readonly BorrowRequestDirectory _requestDirectory = new BorrowRequestDirectory();
        private readonly AttendanceDirectory _attendanceDirectory = new AttendanceDirectory();

        private readonly int _accountId;
        private readonly string _fullName;
        private readonly string _role;
        private readonly string _username;
        private readonly string _status;

        private AccountRow? _profile;
        private List<BorrowRequestRow> _studentRequests = new List<BorrowRequestRow>();
        private List<AttendanceRow> _myAttendance = new List<AttendanceRow>();
        private Image? _qrImage;
        private bool _layingOutCards;

        // HEADER + NAVIGATION
        private AvatarBadge _headerAvatar = null!;
        private AvatarBadge _profileAvatar = null!;
        private Label _headerName = null!;
        private PillBadge _headerRole = null!;
        private Label _headerAccount = null!;
        private RoundedButton _logout = null!;
        private NavTab _navCatalog = null!;
        private NavTab _navRequests = null!;
        private NavTab _navLoans = null!;
        private NavTab _navAttendance = null!;
        private NavTab _navProfile = null!;

        // PAGES
        private Panel _pageHost = null!;
        private Panel _catalogPage = null!;
        private Panel _requestsPage = null!;
        private Panel _loansPage = null!;
        private Panel _attendancePage = null!;
        private Panel _profilePage = null!;

        // CATALOG PAGE
        private SearchBox _catalogSearch = null!;
        private Label _catalogStatus = null!;
        private FlowLayoutPanel _catalogFlow = null!;
        private EmptyState _catalogEmpty = null!;
        private StatCard _catalogStatTitles = null!;
        private StatCard _catalogStatCopies = null!;
        private StatCard _catalogStatOnLoan = null!;
        private StatCard _catalogStatPending = null!;
        private readonly List<BookCard> _bookCards = new List<BookCard>();

        // REQUESTS PAGE
        private SearchBox _requestsSearch = null!;
        private Label _requestsStatus = null!;
        private FlowLayoutPanel _requestFlow = null!;
        private EmptyState _requestsEmpty = null!;
        private StatCard _requestStatTotal = null!;
        private StatCard _requestStatPending = null!;
        private StatCard _requestStatApproved = null!;
        private StatCard _requestStatClosed = null!;
        private readonly List<RequestCard> _requestCards = new List<RequestCard>();

        // LOANS PAGE
        private SearchBox _loansSearch = null!;
        private Label _loansStatus = null!;
        private FlowLayoutPanel _loanFlow = null!;
        private EmptyState _loansEmpty = null!;
        private StatCard _loanStatActive = null!;
        private StatCard _loanStatReturned = null!;
        private StatCard _loanStatTotal = null!;
        private StatCard _loanStatLongest = null!;
        private readonly List<LoanCard> _loanCards = new List<LoanCard>();

        // ATTENDANCE PAGE
        private SearchBox _attendanceSearch = null!;
        private Label _attendanceStatus = null!;
        private FlowLayoutPanel _attendanceFlow = null!;
        private EmptyState _attendanceEmpty = null!;
        private StatCard _attendanceStatTotal = null!;
        private StatCard _attendanceStatToday = null!;
        private StatCard _attendanceStatMonth = null!;
        private StatCard _attendanceStatLast = null!;
        private readonly List<AttendanceCard> _attendanceCards = new List<AttendanceCard>();

        // PROFILE PAGE
        private Label _profileName = null!;
        private Label _profileMeta = null!;
        private PillBadge _profileRole = null!;
        private Label _schoolIdValue = null!;
        private Label _programValue = null!;
        private Label _yearValue = null!;
        private Label _instituteValue = null!;
        private Label _contactValue = null!;
        private Label _addressValue = null!;
        private PictureBox _qrPicture = null!;
        private Label _qrCaption = null!;
        private readonly List<Label> _profileValueLabels = new List<Label>();

        /// <summary>
        /// Design time constructor. The Windows Forms designer creates an instance of
        /// this base type while rendering the patron, student and faculty portals, so
        /// it builds the complete layout with sample identity values.
        /// </summary>
        public PatronDashboardBase()
        {
            _accountId = 0;
            _fullName = "PATRON";
            _role = "STUDENT";
            _username = string.Empty;
            _status = "ACTIVE";

            InitializeComponent();
        }

        public PatronDashboardBase(int accountID, string fullName, string role, string username = "", string status = "")
        {
            _accountId = accountID;
            _role = string.IsNullOrWhiteSpace(role) ? "STUDENT" : role.Trim().ToUpperInvariant();
            _fullName = string.IsNullOrWhiteSpace(fullName) ? PortalName() : fullName.Trim();
            _username = (username ?? string.Empty).Trim();
            _status = string.IsNullOrWhiteSpace(status) ? "ACTIVE" : status.Trim().ToUpperInvariant();

            InitializeComponent();
        }

        /// <summary>
        /// Puts the role specific caption back after the derived form's designer
        /// file has applied its default window properties.
        /// </summary>
        protected void ApplyDashboardTitle()
        {
            Text = "TCGC - " + PortalName() + " DASHBOARD";
        }

        /// <summary>
        /// Applies the runtime window mode and hooks the data loading that has to
        /// wait until the window is shown.
        /// </summary>
        protected void StartRuntime()
        {
            ScreenMode.Apply(this);
            Shown += PatronDashboard_Shown;
        }

        /// <summary>The role name used in the title, subtitle and QR code.</summary>
        protected string PortalName()
        {
            if (string.Equals(_role, "FACULTY", StringComparison.OrdinalIgnoreCase))
            {
                return "FACULTY";
            }

            if (string.Equals(_role, "STAFF", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(_role, "OFFICER", StringComparison.OrdinalIgnoreCase))
            {
                return "STAFF";
            }

            return "STUDENT";
        }

        private void InitializeUi()
        {
            Text = "TCGC - " + PortalName() + " DASHBOARD";
            BackColor = PageBack;
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterScreen;

            TableLayoutPanel root = new TableLayoutPanel();
            root.ColumnCount = 1;
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            root.Dock = DockStyle.Fill;
            root.RowCount = 3;
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.Controls.Add(CreateHeader(), 0, 0);
            root.Controls.Add(CreateNavBar(), 0, 1);

            _pageHost = new Panel();
            _pageHost.BackColor = PageBack;
            _pageHost.Dock = DockStyle.Fill;

            _catalogPage = BuildCatalogPage();
            _requestsPage = BuildRequestsPage();
            _loansPage = BuildLoansPage();
            _attendancePage = BuildAttendancePage();
            _profilePage = BuildProfilePage();

            _pageHost.Controls.Add(_catalogPage);
            _pageHost.Controls.Add(_requestsPage);
            _pageHost.Controls.Add(_loansPage);
            _pageHost.Controls.Add(_attendancePage);
            _pageHost.Controls.Add(_profilePage);

            root.Controls.Add(_pageHost, 0, 2);
            Controls.Add(root);

            ShowPage(0);
        }

        private Panel CreateHeader()
        {
            PatronHeaderPanel header = new PatronHeaderPanel();
            header.Dock = DockStyle.Fill;

            _headerAvatar = new AvatarBadge();
            _headerAvatar.BackColor = Color.Transparent;
            _headerAvatar.FillTopColor = Color.FromArgb(34, 197, 94);
            _headerAvatar.FillBottomColor = Color.FromArgb(4, 90, 66);
            _headerAvatar.Initials = Initials(_fullName);
            _headerAvatar.Location = new Point(28, 20);
            _headerAvatar.Size = new Size(60, 60);

            _headerName = new Label();
            _headerName.AutoEllipsis = true;
            _headerName.BackColor = Color.Transparent;
            _headerName.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            _headerName.ForeColor = Color.White;
            _headerName.Location = new Point(104, 20);
            _headerName.Size = new Size(620, 30);
            _headerName.Text = _fullName.ToUpperInvariant();
            _headerName.TextAlign = ContentAlignment.MiddleLeft;

            _headerRole = new PillBadge();
            _headerRole.BackColor = Color.Transparent;
            _headerRole.BorderColor = Color.FromArgb(167, 243, 208);
            _headerRole.FillColor = Color.White;
            _headerRole.TextColor = HeaderGreen;
            _headerRole.Text = _role;
            _headerRole.Location = new Point(106, 54);

            _headerAccount = new Label();
            _headerAccount.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _headerAccount.BackColor = Color.Transparent;
            _headerAccount.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            _headerAccount.ForeColor = Color.FromArgb(209, 250, 229);
            _headerAccount.TextAlign = ContentAlignment.MiddleRight;
            _headerAccount.Text = "ACCOUNT #" + _accountId.ToString(CultureInfo.InvariantCulture);

            _logout = CreateButton(
                "LOGOUT", new Point(0, 30), new Size(124, 40),
                Color.FromArgb(234, 88, 12), Color.FromArgb(194, 65, 12), Color.FromArgb(154, 52, 18));
            _logout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _logout.Click += SignOut_Click;

            header.Resize += delegate
            {
                int right = header.ClientSize.Width - 28;
                _logout.Left = right - _logout.Width;
                _headerAccount.Left = _logout.Left - 190;
                _headerAccount.Width = 176;
                _headerAccount.Top = 30;
                _headerName.Width = Math.Max(180, _headerAccount.Left - 140);
            };

            header.Controls.Add(_headerAccount);
            header.Controls.Add(_logout);
            header.Controls.Add(_headerRole);
            header.Controls.Add(_headerName);
            header.Controls.Add(_headerAvatar);
            return header;
        }

        private Panel CreateNavBar()
        {
            Panel nav = new Panel();
            nav.BackColor = Color.White;
            nav.Dock = DockStyle.Fill;

            nav.Paint += delegate (object? sender, PaintEventArgs e)
            {
                using Pen pen = new Pen(CardBorder, 1.2f);
                e.Graphics.DrawLine(pen, 0, nav.ClientSize.Height - 1, nav.ClientSize.Width, nav.ClientSize.Height - 1);
            };

            _navCatalog = CreateNavTab("BOOK CATALOG", 26, 150);
            _navRequests = CreateNavTab("MY REQUESTS", 188, 176);
            _navLoans = CreateNavTab("MY BORROWED BOOKS", 376, 214);
            _navAttendance = CreateNavTab("MY ATTENDANCE", 602, 168);
            _navProfile = CreateNavTab("MY PROFILE", 782, 148);

            _navCatalog.Click += delegate { ShowPage(0); };
            _navRequests.Click += delegate { ShowPage(1); };
            _navLoans.Click += delegate { ShowPage(2); };
            _navAttendance.Click += delegate { ShowPage(3); };
            _navProfile.Click += delegate { ShowPage(4); };

            nav.Controls.Add(_navCatalog);
            nav.Controls.Add(_navRequests);
            nav.Controls.Add(_navLoans);
            nav.Controls.Add(_navAttendance);
            nav.Controls.Add(_navProfile);
            return nav;
        }

        private static NavTab CreateNavTab(string text, int left, int width)
        {
            NavTab tab = new NavTab();
            tab.BackColor = Color.Transparent;
            tab.Font = new Font("Segoe UI", 9.25F, FontStyle.Bold);
            tab.Location = new Point(left, 0);
            tab.Size = new Size(width, 58);
            tab.Text = text;
            return tab;
        }

        private void ShowPage(int index)
        {
            _catalogPage.Visible = index == 0;
            _requestsPage.Visible = index == 1;
            _loansPage.Visible = index == 2;
            _attendancePage.Visible = index == 3;
            _profilePage.Visible = index == 4;

            _navCatalog.Selected = index == 0;
            _navRequests.Selected = index == 1;
            _navLoans.Selected = index == 2;
            _navAttendance.Selected = index == 3;
            _navProfile.Selected = index == 4;

            Panel page = index == 0 ? _catalogPage
                : index == 1 ? _requestsPage
                : index == 2 ? _loansPage
                : index == 3 ? _attendancePage
                : _profilePage;

            page.BringToFront();

            // The attendance log changes at the entrance, so it is refreshed every
            // time the patron opens the page instead of only once when the form loads.
            if (index == 3)
            {
                LoadAttendance();
            }
        }

        // ---------------------------------------------------------------- CATALOG

        private Panel BuildCatalogPage()
        {
            Panel page = CreatePagePanel();
            TableLayoutPanel grid = CreatePageGrid();

            Panel head = CreatePageHead(
                "BOOK CATALOG",
                "Browse the shelf, open a cover and request the title from the librarian.",
                "SEARCH BOOKS...",
                out Label title,
                out Label subtitle,
                out _catalogSearch,
                out RoundedButton refresh);

            title.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            refresh.Click += delegate
            {
                LoadCatalogData();
                LoadRequests();
                LoadLoans();
            };
            _catalogSearch.SearchTextChanged += delegate { if (!_layingOutCards) RebuildCatalogCards(); };
            _catalogSearch.SearchRequested += delegate { RebuildCatalogCards(); };

            _catalogStatTitles = CreateStat("TITLES IN CATALOG", Color.FromArgb(22, 163, 74));
            _catalogStatCopies = CreateStat("COPIES ON THE SHELF", Color.FromArgb(13, 148, 136));
            _catalogStatOnLoan = CreateStat("YOUR BOOKS ON LOAN", Color.FromArgb(37, 99, 235));
            _catalogStatPending = CreateStat("YOUR PENDING REQUESTS", Color.FromArgb(217, 119, 6));

            FlowLayoutPanel stats = CreateStatRow(
                _catalogStatTitles, _catalogStatCopies, _catalogStatOnLoan, _catalogStatPending);

            _catalogStatus = CreateStatusLabel();

            Panel listHost = CreateListHost(out _catalogFlow, out _catalogEmpty, "NO BOOKS FOUND", "TRY ANOTHER TITLE, AUTHOR OR YEAR.");
            _catalogEmpty.Title = "NO BOOKS IN THE CATALOG YET";
            _catalogEmpty.Caption = "PLEASE ASK THE LIBRARIAN TO ADD A TITLE, THEN PRESS REFRESH.";
            _catalogFlow.ClientSizeChanged += delegate { RelayoutCards(_catalogFlow, true); };

            grid.Controls.Add(head, 0, 0);
            grid.Controls.Add(stats, 0, 1);
            grid.Controls.Add(_catalogStatus, 0, 2);
            grid.Controls.Add(listHost, 0, 3);
            page.Controls.Add(grid);
            return page;
        }

        // --------------------------------------------------------------- REQUESTS

        private Panel BuildRequestsPage()
        {
            Panel page = CreatePagePanel();
            TableLayoutPanel grid = CreatePageGrid();

            Panel head = CreatePageHead(
                "MY REQUESTS",
                "Follow every request from PENDING to APPROVED, and cancel one while the librarian has not decided yet.",
                "SEARCH REQUESTS...",
                out Label title,
                out Label subtitle,
                out _requestsSearch,
                out RoundedButton refresh);

            title.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            refresh.Click += delegate { LoadRequests(); };
            _requestsSearch.SearchTextChanged += delegate { if (!_layingOutCards) RebuildRequestCards(); };
            _requestsSearch.SearchRequested += delegate { RebuildRequestCards(); };

            _requestStatTotal = CreateStat("TOTAL REQUESTS", Color.FromArgb(22, 163, 74));
            _requestStatPending = CreateStat("STILL PENDING", Color.FromArgb(217, 119, 6));
            _requestStatApproved = CreateStat("APPROVED", Color.FromArgb(4, 120, 87));
            _requestStatClosed = CreateStat("REJECTED / CLOSED", Color.FromArgb(220, 38, 38));

            FlowLayoutPanel stats = CreateStatRow(
                _requestStatTotal, _requestStatPending, _requestStatApproved, _requestStatClosed);

            _requestsStatus = CreateStatusLabel();

            Panel listHost = CreateListHost(out _requestFlow, out _requestsEmpty, "NO REQUESTS YET", "OPEN THE BOOK CATALOG AND PRESS REQUEST TO BORROW ON A TITLE.");
            _requestFlow.ClientSizeChanged += delegate { RelayoutCards(_requestFlow, false); };

            grid.Controls.Add(head, 0, 0);
            grid.Controls.Add(stats, 0, 1);
            grid.Controls.Add(_requestsStatus, 0, 2);
            grid.Controls.Add(listHost, 0, 3);
            page.Controls.Add(grid);
            return page;
        }

        // ----------------------------------------------------------------- LOANS

        private Panel BuildLoansPage()
        {
            Panel page = CreatePagePanel();
            TableLayoutPanel grid = CreatePageGrid();

            Panel head = CreatePageHead(
                "MY BORROWED BOOKS",
                "The same loan records the librarian sees at the desk, arranged by the books still out and the ones already returned.",
                "SEARCH LOANS...",
                out Label title,
                out Label subtitle,
                out _loansSearch,
                out RoundedButton refresh);

            title.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            refresh.Click += delegate
            {
                LoadCatalogData();
                LoadLoans();
            };
            _loansSearch.SearchTextChanged += delegate { if (!_layingOutCards) RebuildLoanCards(); };
            _loansSearch.SearchRequested += delegate { RebuildLoanCards(); };

            _loanStatActive = CreateStat("ON LOAN RIGHT NOW", Color.FromArgb(22, 163, 74));
            _loanStatReturned = CreateStat("ALREADY RETURNED", Color.FromArgb(100, 116, 139));
            _loanStatTotal = CreateStat("TOTAL LOANS ON FILE", Color.FromArgb(37, 99, 235));
            _loanStatLongest = CreateStat("LONGEST OPEN LOAN", Color.FromArgb(217, 119, 6));

            FlowLayoutPanel stats = CreateStatRow(
                _loanStatActive, _loanStatReturned, _loanStatTotal, _loanStatLongest);

            _loansStatus = CreateStatusLabel();

            Panel listHost = CreateListHost(out _loanFlow, out _loansEmpty, "NO BORROW RECORDS YET", "WHEN THE LIBRARIAN RELEASES AN APPROVED REQUEST, THE LOAN APPEARS HERE.");
            _loanFlow.ClientSizeChanged += delegate { RelayoutCards(_loanFlow, false); };

            grid.Controls.Add(head, 0, 0);
            grid.Controls.Add(stats, 0, 1);
            grid.Controls.Add(_loansStatus, 0, 2);
            grid.Controls.Add(listHost, 0, 3);
            page.Controls.Add(grid);
            return page;
        }

        // --------------------------------------------------------------- ATTENDANCE

        private Panel BuildAttendancePage()
        {
            Panel page = CreatePagePanel();
            TableLayoutPanel grid = CreatePageGrid();

            Panel head = CreatePageHead(
                "MY ATTENDANCE",
                "Every scan of your QR pass at the library office entrance is listed here. Show the QR pass on MY PROFILE when you arrive.",
                "SEARCH VISITS...",
                out Label title,
                out Label subtitle,
                out _attendanceSearch,
                out RoundedButton refresh);

            title.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            refresh.Click += delegate { LoadAttendance(); };
            _attendanceSearch.SearchTextChanged += delegate { if (!_layingOutCards) RebuildAttendanceCards(); };
            _attendanceSearch.SearchRequested += delegate { RebuildAttendanceCards(); };

            _attendanceStatTotal = CreateStat("TOTAL VISITS", Color.FromArgb(22, 163, 74));
            _attendanceStatToday = CreateStat("VISITS TODAY", Color.FromArgb(13, 148, 136));
            _attendanceStatMonth = CreateStat("VISITS THIS MONTH", Color.FromArgb(37, 99, 235));
            _attendanceStatLast = CreateStat("LAST VISIT", Color.FromArgb(217, 119, 6));

            FlowLayoutPanel stats = CreateStatRow(
                _attendanceStatTotal, _attendanceStatToday, _attendanceStatMonth, _attendanceStatLast);

            _attendanceStatus = CreateStatusLabel();

            Panel listHost = CreateListHost(
                out _attendanceFlow,
                out _attendanceEmpty,
                "NO ATTENDANCE RECORDED YET",
                "SHOW YOUR QR PASS TO THE LIBRARIAN AT THE LIBRARY OFFICE ENTRANCE.");
            _attendanceFlow.ClientSizeChanged += delegate { RelayoutCards(_attendanceFlow, false); };

            grid.Controls.Add(head, 0, 0);
            grid.Controls.Add(stats, 0, 1);
            grid.Controls.Add(_attendanceStatus, 0, 2);
            grid.Controls.Add(listHost, 0, 3);
            page.Controls.Add(grid);
            return page;
        }

        // --------------------------------------------------------------- PROFILE

        private Panel BuildProfilePage()
        {
            Panel page = CreatePagePanel();

            TableLayoutPanel grid = new TableLayoutPanel();
            grid.Dock = DockStyle.Fill;
            grid.BackColor = PageBack;
            grid.ColumnCount = 1;
            grid.RowCount = 2;
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 92F));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            Panel head = CreatePageHead(
                "MY PROFILE",
                "Your library identity and the QR attendance pass the librarian scans when you enter the library office.",
                null,
                out Label title,
                out Label subtitle,
                out SearchBox _,
                out RoundedButton refresh);

            title.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            refresh.Click += delegate { LoadProfile(); };

            TableLayoutPanel columns = new TableLayoutPanel();
            columns.Dock = DockStyle.Fill;
            columns.BackColor = PageBack;
            columns.ColumnCount = 2;
            columns.RowCount = 1;
            columns.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 440F));
            columns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            columns.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            columns.Controls.Add(BuildIdentityCard(), 0, 0);
            columns.Controls.Add(BuildQrCard(), 1, 0);

            grid.Controls.Add(head, 0, 0);
            grid.Controls.Add(columns, 0, 1);
            page.Controls.Add(grid);
            return page;
        }

        private Panel BuildIdentityCard()
        {
            Panel card = CreateRoundedCard();
            card.Margin = new Padding(0, 0, 18, 0);

            Label heading = new Label();
            heading.AutoSize = true;
            heading.BackColor = Color.White;
            heading.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            heading.ForeColor = SuccessText;
            heading.Location = new Point(26, 20);
            heading.Text = "MY PROFILE";

            _profileAvatar = new AvatarBadge();
            _profileAvatar.BackColor = Color.White;
            _profileAvatar.FillTopColor = Color.FromArgb(34, 197, 94);
            _profileAvatar.FillBottomColor = Color.FromArgb(4, 90, 66);
            _profileAvatar.Initials = Initials(_fullName);
            _profileAvatar.Size = new Size(96, 96);
            _profileAvatar.Location = new Point(150, 48);

            _profileName = new Label();
            _profileName.AutoEllipsis = true;
            _profileName.BackColor = Color.White;
            _profileName.Font = new Font("Segoe UI", 14.5F, FontStyle.Bold);
            _profileName.ForeColor = Color.FromArgb(6, 78, 59);
            _profileName.Location = new Point(24, 154);
            _profileName.Size = new Size(392, 44);
            _profileName.Text = _fullName;
            _profileName.TextAlign = ContentAlignment.MiddleCenter;

            _profileMeta = new Label();
            _profileMeta.AutoEllipsis = true;
            _profileMeta.BackColor = Color.White;
            _profileMeta.Font = new Font("Segoe UI", 8.5F);
            _profileMeta.ForeColor = Color.FromArgb(71, 85, 105);
            _profileMeta.Location = new Point(24, 198);
            _profileMeta.Size = new Size(392, 22);
            _profileMeta.Text = "ACCOUNT #" + _accountId + (_username.Length > 0 ? "     •     @" + _username : string.Empty);
            _profileMeta.TextAlign = ContentAlignment.MiddleCenter;

            _profileRole = new PillBadge();
            _profileRole.BackColor = Color.White;
            _profileRole.BorderColor = Color.FromArgb(167, 243, 208);
            _profileRole.FillColor = Color.FromArgb(220, 252, 231);
            _profileRole.TextColor = SuccessText;
            _profileRole.Text = _role;
            _profileRole.Location = new Point(190, 226);

            Panel divider = new Panel();
            divider.BackColor = Color.FromArgb(226, 232, 240);
            divider.Location = new Point(26, 266);
            divider.Size = new Size(388, 1);

            _schoolIdValue = AddProfileRow(card, 280, "ID NUMBER");
            _programValue = AddProfileRow(card, 316, "PROGRAM / OFFICE");
            _yearValue = AddProfileRow(card, 352, "YEAR / ROLE");
            _instituteValue = AddProfileRow(card, 388, "INSTITUTE");
            _contactValue = AddProfileRow(card, 424, "CONTACT");
            _addressValue = AddProfileRow(card, 460, "ADDRESS");

            card.Controls.Add(divider);
            card.Controls.Add(_profileRole);
            card.Controls.Add(_profileMeta);
            card.Controls.Add(_profileName);
            card.Controls.Add(_profileAvatar);
            card.Controls.Add(heading);

            card.Resize += delegate
            {
                int width = Math.Max(200, card.ClientSize.Width - 52);
                _profileName.Left = 26;
                _profileName.Width = width;
                _profileMeta.Left = 26;
                _profileMeta.Width = width;
                _profileAvatar.Left = 26 + Math.Max(0, (width - _profileAvatar.Width) / 2);
                _profileRole.Left = 26 + Math.Max(0, (width - _profileRole.Width) / 2);
                divider.Width = width;

                foreach (Label value in _profileValueLabels)
                {
                    value.Width = Math.Max(80, width - 138);
                }
            };

            return card;
        }

        private Panel BuildQrCard()
        {
            Panel card = CreateRoundedCard();

            Label heading = new Label();
            heading.AutoSize = true;
            heading.BackColor = Color.White;
            heading.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            heading.ForeColor = SuccessText;
            heading.Location = new Point(26, 20);
            heading.Text = PortalName() + " ATTENDANCE QR PASS";

            _qrPicture = new PictureBox();
            _qrPicture.BackColor = Color.White;
            _qrPicture.BorderStyle = BorderStyle.FixedSingle;
            _qrPicture.Location = new Point(26, 60);
            _qrPicture.Size = new Size(260, 260);
            _qrPicture.SizeMode = PictureBoxSizeMode.Zoom;
            _qrPicture.TabStop = false;

            _qrCaption = new Label();
            _qrCaption.BackColor = Color.White;
            _qrCaption.Font = new Font("Segoe UI", 8.75F);
            _qrCaption.ForeColor = MutedText;
            _qrCaption.Location = new Point(26, 330);
            _qrCaption.Size = new Size(260, 42);
            _qrCaption.Text = "SHOW THIS QR PASS AT THE LIBRARY OFFICE ENTRANCE. THE LIBRARIAN SCANS IT TO RECORD YOUR ATTENDANCE.";
            _qrCaption.TextAlign = ContentAlignment.MiddleCenter;

            Panel divider = new Panel();
            divider.BackColor = Color.FromArgb(226, 232, 240);
            divider.Location = new Point(312, 64);
            divider.Size = new Size(1, 308);

            Label howTo = new Label();
            howTo.AutoSize = true;
            howTo.BackColor = Color.White;
            howTo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            howTo.ForeColor = Color.FromArgb(15, 23, 42);
            howTo.Location = new Point(340, 60);
            howTo.Text = "HOW TO USE YOUR ATTENDANCE PASS";

            Label stepOne = CreateInstructionLabel(340, 92, "1.  ACCESS YOUR QR CODE IN THE LMS.");
            Label stepTwo = CreateInstructionLabel(340, 118, "2.  SCAN YOUR QR CODE IN LIBRARY.");
            Label stepThree = CreateInstructionLabel(340, 144, "3.  YOUR ATTENDANCE AUTOMATICALLY RECORDED IN THE LMS.");

            Label keepPrivate = new Label();
            keepPrivate.AutoEllipsis = true;
            keepPrivate.BackColor = Color.White;
            keepPrivate.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            keepPrivate.ForeColor = MutedText;
            keepPrivate.Location = new Point(340, 186);
            keepPrivate.Size = new Size(460, 40);
            keepPrivate.Text = "KEEP THIS PASS PRIVATE — IT POINTS TO YOUR LIBRARY ATTENDANCE RECORD.";

            card.Controls.Add(keepPrivate);
            card.Controls.Add(stepThree);
            card.Controls.Add(stepTwo);
            card.Controls.Add(stepOne);
            card.Controls.Add(howTo);
            card.Controls.Add(divider);
            card.Controls.Add(_qrCaption);
            card.Controls.Add(_qrPicture);
            card.Controls.Add(heading);

            card.Resize += delegate
            {
                int width = Math.Max(120, card.ClientSize.Width - 52);
                bool wide = width >= 700;

                if (wide)
                {
                    _qrPicture.Left = 26;
                    _qrCaption.Left = 26;
                    _qrCaption.Width = 260;
                    divider.SetBounds(312, 64, 1, 320);
                    howTo.Left = 340;
                    stepOne.SetBounds(340, 92, width - 314, 20);
                    stepTwo.SetBounds(340, 118, width - 314, 20);
                    stepThree.SetBounds(340, 144, width - 314, 20);
                    keepPrivate.SetBounds(340, 186, width - 314, 40);
                }
                else
                {
                    _qrPicture.Left = 26 + Math.Max(0, (width - _qrPicture.Width) / 2);
                    _qrCaption.Left = 26;
                    _qrCaption.Width = width;
                    divider.SetBounds(26, 384, width, 1);
                    howTo.Left = 26;
                    stepOne.SetBounds(26, 404, width, 20);
                    stepTwo.SetBounds(26, 430, width, 20);
                    stepThree.SetBounds(26, 456, width, 20);
                    keepPrivate.SetBounds(26, 494, width, 40);
                }
            };

            return card;
        }

        private static Label CreateInstructionLabel(int left, int top, string text)
        {
            Label label = new Label();
            label.AutoEllipsis = true;
            label.BackColor = Color.White;
            label.Font = new Font("Segoe UI", 8.5F);
            label.ForeColor = Color.FromArgb(71, 85, 105);
            label.Location = new Point(left, top);
            label.Size = new Size(480, 20);
            label.Text = text;
            return label;
        }

        private Label AddProfileRow(Panel card, int top, string caption)
        {
            Label captionLabel = new Label();
            captionLabel.AutoSize = true;
            captionLabel.MinimumSize = new Size(0, 22);
            captionLabel.BackColor = Color.White;
            captionLabel.Font = new Font("Segoe UI", 7.75F, FontStyle.Bold);
            captionLabel.ForeColor = MutedText;
            captionLabel.Location = new Point(26, top);
            captionLabel.Text = caption;
            captionLabel.TextAlign = ContentAlignment.MiddleLeft;

            Label value = new Label();
            value.AutoEllipsis = true;
            value.BackColor = Color.White;
            value.Font = new Font("Segoe UI", 8.75F);
            value.ForeColor = Color.FromArgb(15, 23, 42);
            value.Location = new Point(164, top);
            value.Size = new Size(250, 22);
            value.Text = "—";
            value.TextAlign = ContentAlignment.MiddleLeft;

            card.Controls.Add(value);
            card.Controls.Add(captionLabel);
            _profileValueLabels.Add(value);
            return value;
        }

        // ------------------------------------------------------- SHARED PAGE PARTS

        private static Panel CreatePagePanel()
        {
            Panel page = new Panel();
            page.BackColor = PageBack;
            page.Dock = DockStyle.Fill;
            page.Padding = new Padding(26, 18, 26, 22);
            page.Visible = false;
            return page;
        }

        private static TableLayoutPanel CreatePageGrid()
        {
            TableLayoutPanel grid = new TableLayoutPanel();
            grid.BackColor = PageBack;
            grid.ColumnCount = 1;
            grid.Dock = DockStyle.Fill;
            grid.RowCount = 4;
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 92F));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            return grid;
        }

        private Panel CreatePageHead(
            string title,
            string subtitle,
            string? searchPlaceholder,
            out Label titleLabel,
            out Label subtitleLabel,
            out SearchBox search,
            out RoundedButton refresh)
        {
            Panel head = new Panel();
            head.BackColor = PageBack;
            head.Dock = DockStyle.Fill;

            titleLabel = new Label();
            titleLabel.AutoSize = true;
            titleLabel.BackColor = PageBack;
            titleLabel.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(15, 23, 42);
            titleLabel.Location = new Point(0, 0);
            titleLabel.Text = title;

            subtitleLabel = new Label();
            subtitleLabel.AutoEllipsis = true;
            subtitleLabel.BackColor = PageBack;
            subtitleLabel.Font = new Font("Segoe UI", 9F);
            subtitleLabel.ForeColor = MutedText;
            subtitleLabel.Location = new Point(2, 46);
            subtitleLabel.Size = new Size(620, 22);
            subtitleLabel.Text = subtitle;

            refresh = CreateButton(
                "REFRESH", new Point(0, 10), new Size(114, 44),
                Color.FromArgb(22, 163, 74), Color.FromArgb(21, 128, 61), Color.FromArgb(20, 83, 45));

            if (string.IsNullOrWhiteSpace(searchPlaceholder))
            {
                search = null!;
            }
            else
            {
                search = new SearchBox();
                search.BackColor = Color.White;
                search.PlaceholderText = searchPlaceholder;
                search.Size = new Size(360, 44);
                head.Controls.Add(search);
            }

            SearchBox? searchBox = search;
            RoundedButton refreshButton = refresh;
            Label subtitleCopy = subtitleLabel;

            head.Resize += delegate
            {
                int right = head.ClientSize.Width;
                refreshButton.Left = Math.Max(0, right - refreshButton.Width);
                refreshButton.Top = 10;

                if (searchBox != null)
                {
                    searchBox.Left = Math.Max(0, refreshButton.Left - 12 - searchBox.Width);
                    searchBox.Top = 10;
                }

                subtitleCopy.Width = Math.Max(160, (searchBox == null ? refreshButton.Left : searchBox.Left) - 16);
            };

            head.Controls.Add(subtitleLabel);
            head.Controls.Add(titleLabel);
            head.Controls.Add(refresh);
            return head;
        }

        private static StatCard CreateStat(string caption, Color accent)
        {
            StatCard card = new StatCard();
            card.Caption = caption;
            card.AccentColor = accent;
            card.Value = "0";
            return card;
        }

        private static FlowLayoutPanel CreateStatRow(params StatCard[] cards)
        {
            FlowLayoutPanel row = new FlowLayoutPanel();
            row.BackColor = PageBack;
            row.Dock = DockStyle.Fill;
            row.FlowDirection = FlowDirection.LeftToRight;
            row.Padding = new Padding(0, 4, 0, 4);
            row.WrapContents = false;

            foreach (StatCard card in cards)
            {
                row.Controls.Add(card);
            }

            row.Resize += delegate { LayoutStatRow(row); };
            LayoutStatRow(row);
            return row;
        }

        private static void LayoutStatRow(FlowLayoutPanel row)
        {
            const int gap = 14;
            int count = row.Controls.Count;

            if (count == 0)
            {
                return;
            }

            int available = Math.Max(320, row.ClientSize.Width - row.Padding.Horizontal);
            int width = Math.Max(110, (available - gap * (count - 1)) / count);

            for (int i = 0; i < count; i++)
            {
                Control card = row.Controls[i];
                card.Width = width;
                card.Height = 78;
                card.Margin = new Padding(0, 0, i == count - 1 ? 0 : gap, 0);
            }
        }

        private static Label CreateStatusLabel()
        {
            Label status = new Label();
            status.AutoEllipsis = true;
            status.BackColor = PageBack;
            status.Dock = DockStyle.Fill;
            status.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            status.ForeColor = MutedText;
            status.Padding = new Padding(4, 0, 0, 0);
            status.TextAlign = ContentAlignment.MiddleLeft;
            status.Text = "LOADING…";
            return status;
        }

        private static Panel CreateListHost(
            out FlowLayoutPanel flow,
            out EmptyState empty,
            string emptyTitle,
            string emptyCaption)
        {
            Panel host = new Panel();
            host.BackColor = PageBack;
            host.Dock = DockStyle.Fill;

            flow = new FlowLayoutPanel();
            flow.AutoScroll = true;
            flow.BackColor = PageBack;
            flow.Dock = DockStyle.Fill;
            flow.FlowDirection = FlowDirection.LeftToRight;
            flow.Padding = new Padding(0, 0, 4, 4);
            flow.WrapContents = true;

            empty = new EmptyState();
            empty.BackColor = Color.Transparent;
            empty.Caption = emptyCaption;
            empty.Dock = DockStyle.Fill;
            empty.Title = emptyTitle;
            empty.Visible = false;

            host.Controls.Add(flow);
            host.Controls.Add(empty);
            return host;
        }

        private static Panel CreateRoundedCard()
        {
            Panel card = new Panel();
            card.BackColor = PageBack;
            card.Dock = DockStyle.Fill;

            card.Paint += delegate (object? sender, PaintEventArgs e)
            {
                CardPaint.DrawSurface(e.Graphics, card.Size, PageBack, Color.White, CardBorder);
            };

            return card;
        }

        // ------------------------------------------------------------- PAGE DATA

        private void PatronDashboard_Shown(object? sender, EventArgs e)
        {
            LoadProfile();
            LoadCatalogData();
            LoadRequests();
            LoadLoans();
            LoadAttendance();
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

            _headerName.Text = name.ToUpperInvariant();
            _headerRole.Text = role.ToUpperInvariant();

            _profileName.Text = name;
            _profileMeta.Text = "ACCOUNT #" + _accountId.ToString(CultureInfo.InvariantCulture) +
                                (_username.Length > 0 ? "     •     @" + _username : string.Empty);
            _profileRole.Text = role.ToUpperInvariant();
            _headerAvatar.Initials = Initials(name);
            _profileAvatar.Initials = Initials(name);

            _schoolIdValue.Text = DisplayId();
            _programValue.Text = DisplayProgramOrOffice();
            _yearValue.Text = DisplayYearOrRole();
            _instituteValue.Text = Value(_profile?.Institute);
            _contactValue.Text = Value(_profile?.ContactNumber);
            _addressValue.Text = Value(_profile?.Address);

            BuildQrCode(name, role);
        }

        /// <summary>
        /// A student uses the school id. Faculty and staff do not have one, so the
        /// account number is the stable identifier shown in the profile and QR.
        /// </summary>
        private string DisplayId()
        {
            string schoolId = CleanValue(_profile?.SchoolId);
            return schoolId.Length > 0
                ? schoolId
                : "ACCOUNT-" + _accountId.ToString(CultureInfo.InvariantCulture);
        }

        private string DisplayProgramOrOffice()
        {
            string program = CleanValue(_profile?.Program);
            return program.Length > 0 ? program : Value(_profile?.Office);
        }

        private string DisplayYearOrRole()
        {
            string year = CleanValue(_profile?.YearLevel);
            return year.Length > 0 ? year : Value(_profile?.Role);
        }

        private void BuildQrCode(string name, string role)
        {
            // The librarian's scanner reads this pass as text. The ACCOUNT line is
            // the stable key the attendance directory parses, so the pass keeps
            // working even when a name or program is edited later.
            StringBuilder text = new StringBuilder();
            text.AppendLine("TCGC LIBRARY ATTENDANCE PASS");
            text.AppendLine("ACCOUNT: " + _accountId.ToString(CultureInfo.InvariantCulture));
            text.AppendLine("NAME: " + name);
            text.AppendLine("ROLE: " + role);
            text.AppendLine("ID NUMBER: " + DisplayId());
            text.AppendLine("PROGRAM / OFFICE: " + DisplayProgramOrOffice());
            text.AppendLine("INSTITUTE: " + Value(_profile?.Institute));
            text.AppendLine("GATE: LIBRARY OFFICE");

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

                _qrCaption.Text = "SCAN THIS PASS AT THE ENTRANCE TO RECORD THE VISIT OF " +
                                  name.ToUpperInvariant() + " (" + PortalName() + ").";
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
            RebuildCatalogCards();
        }

        private void RebuildCatalogCards()
        {
            string term = _catalogSearch.SearchText.ToUpperInvariant();
            int shown = 0;

            ClearCards(_catalogFlow, _bookCards);

            foreach (Book book in _catalog.Books)
            {
                if (term.Length > 0 && !MatchesBook(book, term))
                {
                    continue;
                }

                BookCard card = new BookCard();
                card.Bind(
                    book,
                    _catalog.AvailableCopyCount(book),
                    HasPendingRequest(book.BookId),
                    HasActiveLoan(book.BookId));

                Book captured = book;
                card.RequestClicked += delegate { RequestBook(captured); };
                card.DescriptionClicked += delegate { ShowBookDescription(captured); };

                _catalogFlow.Controls.Add(card);
                _bookCards.Add(card);
                shown++;
            }

            _catalogFlow.Visible = shown > 0;
            _catalogEmpty.Visible = shown == 0;
            if (_catalogEmpty.Visible)
            {
                _catalogEmpty.Title = term.Length > 0 ? "NO MATCHING BOOKS" : "NO BOOKS IN THE CATALOG YET";
                _catalogEmpty.Caption = term.Length > 0
                    ? "TRY ANOTHER TITLE, AUTHOR OR YEAR."
                    : "PLEASE ASK THE LIBRARIAN TO ADD A TITLE, THEN PRESS REFRESH.";
                _catalogEmpty.BringToFront();
            }

            LayoutCardColumns(_catalogFlow, true);

            _catalogStatus.ForeColor = _catalog.IsConnected ? MutedText : WarningText;
            _catalogStatus.Text = _catalog.IsConnected
                ? _catalog.Books.Count.ToString(CultureInfo.InvariantCulture) + " TITLE(S) IN THE CATALOG  •  " +
                  _catalog.AvailableCopies.ToString(CultureInfo.InvariantCulture) + " COPY(IES) ON THE SHELF  •  SHOWING " +
                  shown.ToString(CultureInfo.InvariantCulture)
                : "MYSQL OFFLINE • THE CATALOG COULD NOT BE LOADED";

            UpdateStats();
        }

        private void ShowBookDescription(Book book)
        {
            using BookDescriptionDialog dialog =
                new BookDescriptionDialog(book, _catalog.AvailableCopyCount(book), PortalName());
            dialog.ShowDialog(this);
        }

        private void RequestBook(Book book)
        {
            DialogResult answer = MessageBox.Show(
                "REQUEST \"" + book.Title.ToUpperInvariant() + "\" FROM THE LIBRARY?\n\n" +
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
                _requestDirectory.Create(_accountId, book);
                LoadRequests();
                RefreshCatalogStates();
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
                ClearCards(_requestFlow, _requestCards);
                _requestsEmpty.Visible = false;
                UpdateStats();
                return;
            }

            RebuildRequestCards();
            RefreshCatalogStates();
        }

        private void RebuildRequestCards()
        {
            string term = _requestsSearch.SearchText.ToUpperInvariant();
            int shown = 0;

            ClearCards(_requestFlow, _requestCards);

            foreach (BorrowRequestRow request in _studentRequests)
            {
                if (term.Length > 0 && !MatchesRequest(request, term))
                {
                    continue;
                }

                RequestCard card = new RequestCard(request);
                BorrowRequestRow captured = request;
                card.CancelClicked += delegate { CancelRequest(captured); };
                _requestFlow.Controls.Add(card);
                _requestCards.Add(card);
                shown++;
            }

            _requestFlow.Visible = shown > 0;
            _requestsEmpty.Visible = shown == 0;
            if (_requestsEmpty.Visible)
            {
                _requestsEmpty.Title = term.Length > 0 ? "NO MATCHING REQUESTS" : "NO REQUESTS YET";
                _requestsEmpty.Caption = term.Length > 0
                    ? "TRY ANOTHER TITLE OR STATUS."
                    : "OPEN THE BOOK CATALOG AND PRESS REQUEST TO BORROW ON A TITLE.";
                _requestsEmpty.BringToFront();
            }

            LayoutCardColumns(_requestFlow, false);

            if (_studentRequests.Count == 0)
            {
                _requestsStatus.Text = "NO REQUESTS YET. OPEN THE BOOK CATALOG TO REQUEST A TITLE.";
            }
            else
            {
                int pending = _studentRequests.Count(r => r.IsPending);
                int approved = _studentRequests.Count(r => r.IsApproved);
                int closed = _studentRequests.Count - pending - approved;

                _requestsStatus.Text =
                    _studentRequests.Count.ToString(CultureInfo.InvariantCulture) + " REQUEST(S) ON FILE  •  " +
                    pending.ToString(CultureInfo.InvariantCulture) + " PENDING  •  " +
                    approved.ToString(CultureInfo.InvariantCulture) + " APPROVED  •  " +
                    closed.ToString(CultureInfo.InvariantCulture) + " CLOSED  •  SHOWING " +
                    shown.ToString(CultureInfo.InvariantCulture);
            }

            _requestsStatus.ForeColor = MutedText;
            UpdateStats();
        }

        private void CancelRequest(BorrowRequestRow request)
        {
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
                RefreshCatalogStates();
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
            RebuildLoanCards();

            int count = GetMyLoans().Count;
            _loansStatus.ForeColor = MutedText;
            _loansStatus.Text = count == 0
                ? "YOU HAVE NO BORROW RECORDS YET."
                : count.ToString(CultureInfo.InvariantCulture) + " BORROW RECORD(S) FOUND FOR " +
                  DisplayId().ToUpperInvariant();
        }

        // --------------------------------------------------------------- ATTENDANCE

        private void LoadAttendance()
        {
            try
            {
                _myAttendance = _attendanceDirectory.LoadForAccount(_accountId);
            }
            catch (MySqlException)
            {
                _myAttendance = new List<AttendanceRow>();
                _attendanceStatus.ForeColor = WarningText;
                _attendanceStatus.Text = "MYSQL OFFLINE • ATTENDANCE CANNOT BE LOADED";
                ClearCards(_attendanceFlow, _attendanceCards);
                _attendanceEmpty.Visible = false;
                UpdateStats();
                return;
            }

            RebuildAttendanceCards();
        }

        private void RebuildAttendanceCards()
        {
            string term = _attendanceSearch.SearchText.ToUpperInvariant();
            int shown = 0;

            ClearCards(_attendanceFlow, _attendanceCards);

            foreach (AttendanceRow visit in _myAttendance)
            {
                if (term.Length > 0 && !MatchesAttendance(visit, term))
                {
                    continue;
                }

                AttendanceCard card = new AttendanceCard(visit);
                _attendanceFlow.Controls.Add(card);
                _attendanceCards.Add(card);
                shown++;
            }

            _attendanceFlow.Visible = shown > 0;
            _attendanceEmpty.Visible = shown == 0;
            if (_attendanceEmpty.Visible)
            {
                _attendanceEmpty.Title = term.Length > 0 ? "NO MATCHING VISITS" : "NO ATTENDANCE RECORDED YET";
                _attendanceEmpty.Caption = term.Length > 0
                    ? "TRY ANOTHER DATE, TIME OR GATE."
                    : "SHOW YOUR QR PASS TO THE LIBRARIAN AT THE LIBRARY OFFICE ENTRANCE.";
                _attendanceEmpty.BringToFront();
            }

            LayoutCardColumns(_attendanceFlow, false);

            if (_myAttendance.Count == 0)
            {
                _attendanceStatus.Text = "NO VISITS YET. SHOW YOUR QR PASS TO THE LIBRARIAN WHEN YOU ENTER THE LIBRARY OFFICE.";
            }
            else
            {
                int today = _myAttendance.Count(visit => visit.IsToday);
                int month = _myAttendance.Count(visit => visit.IsThisMonth);

                _attendanceStatus.Text =
                    _myAttendance.Count.ToString(CultureInfo.InvariantCulture) + " VISIT(S) ON FILE  •  " +
                    today.ToString(CultureInfo.InvariantCulture) + " TODAY  •  " +
                    month.ToString(CultureInfo.InvariantCulture) + " THIS MONTH  •  LAST VISIT " +
                    _myAttendance[0].StampText + "  •  SHOWING " +
                    shown.ToString(CultureInfo.InvariantCulture);
            }

            _attendanceStatus.ForeColor = MutedText;
            UpdateStats();
        }

        private static bool MatchesAttendance(AttendanceRow visit, string term)
        {
            return visit.DateText.Contains(term) ||
                   visit.TimeText.Contains(term) ||
                   visit.Gate.ToUpperInvariant().Contains(term) ||
                   visit.Role.Contains(term);
        }

        private void RebuildLoanCards()
        {
            string term = _loansSearch.SearchText.ToUpperInvariant();
            int shown = 0;

            ClearCards(_loanFlow, _loanCards);

            List<BorrowRecord> loans = GetMyLoans()
                .OrderBy(record => record.DateReturned.HasValue)
                .ThenByDescending(record => record.DateBorrowed)
                .ToList();

            foreach (BorrowRecord record in loans)
            {
                if (term.Length > 0 && !MatchesLoan(record, term))
                {
                    continue;
                }

                Book? coverBook = _catalog.Books.FirstOrDefault(book => book.BookId == record.BookId);
                LoanCard card = new LoanCard(record, coverBook?.CoverImage);
                _loanFlow.Controls.Add(card);
                _loanCards.Add(card);
                shown++;
            }

            _loanFlow.Visible = shown > 0;
            _loansEmpty.Visible = shown == 0;
            if (_loansEmpty.Visible)
            {
                _loansEmpty.Title = term.Length > 0 ? "NO MATCHING LOANS" : "NO BORROW RECORDS YET";
                _loansEmpty.Caption = term.Length > 0
                    ? "TRY ANOTHER TITLE, AUTHOR OR STATUS."
                    : "WHEN THE LIBRARIAN RELEASES AN APPROVED REQUEST, THE LOAN APPEARS HERE.";
                _loansEmpty.BringToFront();
            }

            LayoutCardColumns(_loanFlow, false);

            if (loans.Count > 0)
            {
                int active = loans.Count(record => !record.DateReturned.HasValue);
                int returned = loans.Count - active;

                _loansStatus.Text =
                    active.ToString(CultureInfo.InvariantCulture) + " BOOK(S) ON LOAN  •  " +
                    returned.ToString(CultureInfo.InvariantCulture) + " RETURNED  •  " +
                    loans.Count.ToString(CultureInfo.InvariantCulture) + " TOTAL RECORD(S)  •  SHOWING " +
                    shown.ToString(CultureInfo.InvariantCulture);
            }

            UpdateStats();
        }

        private void RefreshCatalogStates()
        {
            foreach (BookCard card in _bookCards)
            {
                if (card.Book == null)
                {
                    continue;
                }

                card.UpdateState(
                    _catalog.AvailableCopyCount(card.Book),
                    HasPendingRequest(card.Book.BookId),
                    HasActiveLoan(card.Book.BookId));
            }
        }

        private void UpdateStats()
        {
            List<BorrowRecord> loans = GetMyLoans();
            int activeLoans = loans.Count(record => !record.DateReturned.HasValue);
            int returnedLoans = loans.Count - activeLoans;
            int pending = _studentRequests.Count(request => request.IsPending);
            int approved = _studentRequests.Count(request => request.IsApproved);
            int closed = _studentRequests.Count - pending - approved;

            _catalogStatTitles.Value = _catalog.Books.Count.ToString(CultureInfo.InvariantCulture);
            _catalogStatCopies.Value = _catalog.AvailableCopies.ToString(CultureInfo.InvariantCulture);
            _catalogStatOnLoan.Value = activeLoans.ToString(CultureInfo.InvariantCulture);
            _catalogStatPending.Value = pending.ToString(CultureInfo.InvariantCulture);

            _requestStatTotal.Value = _studentRequests.Count.ToString(CultureInfo.InvariantCulture);
            _requestStatPending.Value = pending.ToString(CultureInfo.InvariantCulture);
            _requestStatApproved.Value = approved.ToString(CultureInfo.InvariantCulture);
            _requestStatClosed.Value = Math.Max(0, closed).ToString(CultureInfo.InvariantCulture);

            int longest = activeLoans == 0
                ? 0
                : loans.Where(record => !record.DateReturned.HasValue).Max(record => record.DaysBorrowed);

            _loanStatActive.Value = activeLoans.ToString(CultureInfo.InvariantCulture);
            _loanStatReturned.Value = returnedLoans.ToString(CultureInfo.InvariantCulture);
            _loanStatTotal.Value = loans.Count.ToString(CultureInfo.InvariantCulture);
            _loanStatLongest.Value = longest.ToString(CultureInfo.InvariantCulture) + "D";

            _navRequests.Text = "MY REQUESTS" + (pending > 0 ? "  •  " + pending.ToString(CultureInfo.InvariantCulture) : string.Empty);
            _navLoans.Text = "MY BORROWED BOOKS" + (activeLoans > 0 ? "  •  " + activeLoans.ToString(CultureInfo.InvariantCulture) : string.Empty);
        }

        // ------------------------------------------------------------------ HELPERS

        private List<BorrowRecord> GetMyLoans()
        {
            List<BorrowRecord> mine = new List<BorrowRecord>();

            foreach (BorrowRecord record in _catalog.BorrowRecords)
            {
                if (IsMyLoan(record))
                {
                    mine.Add(record);
                }
            }

            return mine;
        }

        private bool IsMyLoan(BorrowRecord record)
        {
            string myId = DisplayId();
            if (string.Equals(record.BorrowerIdNumber, myId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Older rows may have been written before the account id fallback.
            // The name is a safe second key because approval uses the profile name.
            string myName = CleanValue(_profile?.Name);
            return myName.Length > 0 && string.Equals(record.BorrowerName, myName, StringComparison.OrdinalIgnoreCase);
        }

        private bool HasPendingRequest(int bookId)
        {
            return _studentRequests.Any(request => request.IsPending && request.BookId == bookId);
        }

        private bool HasActiveLoan(int bookId)
        {
            return _catalog.BorrowRecords.Any(record =>
                !record.DateReturned.HasValue && record.BookId == bookId && IsMyLoan(record));
        }

        private static bool MatchesBook(Book book, string term)
        {
            return book.Title.ToUpperInvariant().Contains(term) ||
                   book.Author.ToUpperInvariant().Contains(term) ||
                   book.YearText.Contains(term);
        }

        private static bool MatchesRequest(BorrowRequestRow request, string term)
        {
            return request.BookTitle.ToUpperInvariant().Contains(term) ||
                   request.BookAuthor.ToUpperInvariant().Contains(term) ||
                   request.Status.ToUpperInvariant().Contains(term);
        }

        private static bool MatchesLoan(BorrowRecord record, string term)
        {
            return record.BookTitle.ToUpperInvariant().Contains(term) ||
                   record.Author.ToUpperInvariant().Contains(term) ||
                   record.Status.ToUpperInvariant().Contains(term);
        }

        private void RelayoutCards(FlowLayoutPanel flow, bool grid)
        {
            if (_layingOutCards)
            {
                return;
            }

            _layingOutCards = true;
            try
            {
                LayoutCardColumns(flow, grid);
            }
            finally
            {
                _layingOutCards = false;
            }
        }

        private static void LayoutCardColumns(FlowLayoutPanel flow, bool grid)
        {
            if (flow.Controls.Count == 0)
            {
                return;
            }

            int available = Math.Max(320, flow.ClientSize.Width - flow.Padding.Horizontal - 22);

            if (!grid)
            {
                int width = Math.Max(300, available);
                foreach (Control card in flow.Controls)
                {
                    card.Width = width;
                }

                return;
            }

            const int gap = 16;
            const int minimumCard = 330;
            int columns = Math.Max(1, Math.Min(4, (available + gap) / (minimumCard + gap)));
            int cardWidth = Math.Max(300, (available - gap * columns) / columns);

            foreach (Control card in flow.Controls)
            {
                card.Width = cardWidth;
                card.Margin = new Padding(0, 0, gap, 16);
            }
        }

        private static void ClearCards(FlowLayoutPanel flow, List<BookCard>? bookCards = null)
        {
            while (flow.Controls.Count > 0)
            {
                Control card = flow.Controls[0];
                flow.Controls.RemoveAt(0);
                card.Dispose();
            }

            bookCards?.Clear();
        }

        private static void ClearCards(FlowLayoutPanel flow, List<RequestCard> cards)
        {
            while (flow.Controls.Count > 0)
            {
                Control card = flow.Controls[0];
                flow.Controls.RemoveAt(0);
                card.Dispose();
            }

            cards.Clear();
        }

        private static void ClearCards(FlowLayoutPanel flow, List<LoanCard> cards)
        {
            while (flow.Controls.Count > 0)
            {
                Control card = flow.Controls[0];
                flow.Controls.RemoveAt(0);
                card.Dispose();
            }

            cards.Clear();
        }

        private static void ClearCards(FlowLayoutPanel flow, List<AttendanceCard> cards)
        {
            while (flow.Controls.Count > 0)
            {
                Control card = flow.Controls[0];
                flow.Controls.RemoveAt(0);
                card.Dispose();
            }

            cards.Clear();
        }

        private void SignOut_Click(object? sender, EventArgs e)
        {
            DialogResult answer = MessageBox.Show(
                "SIGN OUT OF THE " + PortalName() + " PORTAL?",
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
    }
}
