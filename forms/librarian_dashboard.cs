using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using library_management_system.forms;
using MySql.Data.MySqlClient;
using oop_project.Models;
using oop_project.Reports;

namespace oop_project.forms
{
    public partial class librarian_dashboard : Form
    {
        private static readonly Color HeaderGreen = Color.FromArgb(6, 78, 59);
        private static readonly Color MutedText = Color.FromArgb(148, 163, 184);
        private static readonly Color SuccessText = Color.FromArgb(4, 120, 87);
        private static readonly Color WarningText = Color.FromArgb(194, 65, 12);
        private static readonly Color DangerText = Color.FromArgb(185, 28, 28);

        private static readonly Font StatusCellFont = new Font("Segoe UI", 8.75F, FontStyle.Bold);

        // ANALYTICS: the programs offered by the school, grouped by institute.
        // They are merged into the LIBRARY VISITS BY PROGRAM chart, so a program
        // that has no attendance record yet is still listed with 0 visits.
        private static readonly Dictionary<string, string[]> FallbackPrograms =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                  { "ICS", new[] { "BSCS" } },
                { "IAS", new[] { "AB COMM", "AB ENGLISH", "AB POLSCI" } },
                { "ICJE", new[] { "BS-CRIM", "BSISM" } },
                { "IBFS", new[] { "BSBA-FM", "BSBA-HRM", "BSBA-MM", "BSOA" } },
                { "IHS", new[] { "BS-MIDWIFERY" } },
                { "ITE", new[] { "BEED", "BSED-ENGLISH", "BSED-FILIPINO", "BSED-MATH", "BSED-SOCSTUD" } }
            };

        // The EDIT / DELETE column of every table shows one Segoe MDL2 Assets glyph,
        // so the two options are icons instead of words.
        private const string EditColumnName = "editActionColumn";
        private const string DeleteColumnName = "deleteActionColumn";
        private const string AccessColumnName = "accessActionColumn";
        private const string BookCoverColumnName = "bookCoverColumn";
        private const string RequestApproveColumnName = "requestApproveColumn";
        private const string RequestRejectColumnName = "requestRejectColumn";
        private const string RequestClaimColumnName = "requestClaimColumn";
        private const string EditGlyph = "\uE70F";
        private const string DeleteGlyph = "\uE74D";
        private const string AccessGlyph = "\uE7E8";
        private static readonly Font IconCellFont = new Font("Segoe MDL2 Assets", 11.5F);

        private readonly LibraryCatalog _catalog = new LibraryCatalog();
        private readonly AccountDirectory _accountDirectory = new AccountDirectory();
        private readonly BorrowRequestDirectory _requestDirectory = new BorrowRequestDirectory();
        private readonly AttendanceDirectory _attendanceDirectory = new AttendanceDirectory();
        private List<AccountRow> _accounts = new List<AccountRow>();
        private bool _accountsLoaded;
        private bool _canManageAccounts;

        // The signed in person's own row, kept after the PROFILE tab loads so the
        // EDIT MY INFO pop up can be filled without another round trip.
        private AccountRow? _profileRow;

        // LIBRARY ATTENDANCE tab, built in code because the designer tab is empty.
        private DataGridView libraryAttendanceGrid = null!;
        private Label libraryAttendanceHeading = null!;
        private Label libraryAttendanceSubtitle = null!;
        private Label libraryAttendanceStatus = null!;
        private Label libraryAttendanceEmpty = null!;
        private SearchBox libraryAttendanceSearch = null!;
        private RoundedButton libraryAttendanceRefreshButton = null!;
        private RoundedButton libraryAttendanceScanButton = null!;
        private RoundedButton libraryAttendanceReportButton = null!;
        private List<AttendanceRow> _attendanceRows = new List<AttendanceRow>();
        private bool _attendanceLogOnline = true;

        // ANALYTICS tab: the page itself is declared in the designer so it appears in
        // Visual Studio's View Designer. The header and the smooth GDI+ line charts
        // are built in code because they are custom drawn.
        private Panel analyticsHead = null!;
        private Label analyticsTitle = null!;

        // The header carries no description or counter line, so this label only
        // appears when the charts could not be loaded and the librarian has to be
        // told why.
        private Label analyticsStatus = null!;
        private RoundedButton analyticsRefreshButton = null!;
        private LineChart programVisitsChart = null!;
        private LineChart topBorrowersChart = null!;
        private LineChart mostBorrowedBooksChart = null!;

        // ANALYTICS: hovering a point of the MOST BORROWED BOOKS curve fades in that
        // book's cover, and hovering a TOP LIBRARY BOOK BORROWERS point fades in the
        // borrower's initials. Both are read or drawn the first time they are hovered
        // and kept here afterwards, so moving the mouse over the same point is instant.
        private const int HoverCoverWidth = 62;
        private const int HoverCoverHeight = 84;
        private readonly Dictionary<int, Image> _bookCoverCache = new Dictionary<int, Image>();
        private readonly Dictionary<string, Image> _borrowerAvatarCache =
            new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);

        // ANALYTICS: EVALUATION FORM FEEDBACK block. The KPI cards summarise who
        // answered the survey and the three charts show the respondent split and
        // the ratings the students, faculty and staff gave.
        private readonly EvaluationDirectory _evaluationDirectory = new EvaluationDirectory();
        private AnalyticsStatCard evalTotalCard = null!;
        private AnalyticsStatCard evalStudentCard = null!;
        private AnalyticsStatCard evalFacultyCard = null!;
        private AnalyticsStatCard evalStaffCard = null!;
        private DonutChart evaluationRoleChart = null!;
        private LineChart evaluationSectionChart = null!;
        private LineChart evaluationRatingChart = null!;

        // ANALYTICS: the line charts stay readable by drawing the ten highest
        // values of a ranking; the subtitle always says how many were left out.
        private const int ChartPointLimit = 10;

        // ANALYTICS: the SEMESTER strip. The librarian opens one semester at a time,
        // closes it when the term ends and starts the next one; the combo lets the
        // charts show either one semester or every semester together.
        private ComboBox evaluationPeriodBox = null!;
        private RoundedButton evaluationPeriodButton = null!;
        private RoundedButton evaluationNewPeriodButton = null!;
        private RoundedButton evaluationRespondentsButton = null!;
        private Label evaluationPeriodStatus = null!;
        private List<EvaluationPeriod> _evaluationPeriods = new List<EvaluationPeriod>();
        private bool _fillingEvaluationPeriods;

        private static readonly string[] EvaluationSectionLabels =
        {
            "FACILITIES",
            "RESOURCES",
            "SERVICES",
            "ACCESSIBILITY",
            "OVERALL SATISFACTION"
        };

        private DataGridViewImageColumn bookCoverColumn = null!;

        // SEARCH: every table tab has its own field. The typed term is kept here so a
        // refresh, a tab switch or an edit never loses the current filter.
        private SearchBox accountsSearch = null!;
        private SearchBox staffSearch = null!;
        private SearchBox facultySearch = null!;
        private SearchBox studentSearch = null!;
        private SearchBox requestsSearch = null!;
        private SearchBox booksSearch = null!;
        private SearchBox borrowedSearch = null!;
        private List<BorrowRequestRow> _requestQueue = new List<BorrowRequestRow>();

        private int _booksShown;
        private int _booksTotal;
        private int _borrowsShown;
        private int _borrowsTotal;
        private bool _requestsOnline = true;
        private Label requestsEmptyLabel = null!;

        // The profile name shrinks instead of ending in "...". The fitted font is
        // owned by the form and disposed when the window closes.
        private const float ProfileNameMaxSize = 17f;
        private const float ProfileNameMinSize = 8f;
        private Font? _profileNameFont;
        private float _profileNameSize = ProfileNameMaxSize;
        private Image? _avatarImage;

        public int LoggedInAccountId { get; private set; }
        public string LoggedInRole { get; private set; } = string.Empty;
        public string LoggedInFullName { get; private set; } = string.Empty;
        public string LoggedInUsername { get; private set; } = string.Empty;
        public string LoggedInStatus { get; private set; } = string.Empty;

        public librarian_dashboard(int accountID, string fullName, string role, string username = "", string status = "")
        {
            InitializeComponent();

            LoggedInAccountId = accountID;
            LoggedInFullName = string.IsNullOrWhiteSpace(fullName) ? "LIBRARY USER" : fullName.Trim();
            LoggedInRole = string.IsNullOrWhiteSpace(role) ? "MEMBER" : role.Trim();
            LoggedInUsername = username == null ? string.Empty : username.Trim();
            LoggedInStatus = status == null ? string.Empty : status.Trim();

            // The staff check also accepts the built-in "admin" username, because an
            // account can still be stored with the default STUDENT role in MySQL.
            _canManageAccounts = IsStaffRole(LoggedInRole) ||
                                 string.Equals(LoggedInUsername, "admin", StringComparison.OrdinalIgnoreCase);

            // FULL SCREEN: every window opens maximised and F11 toggles a borderless view.
            ScreenMode.Apply(this);

            ApplyAccountDetails();
            changePasswordButton.Click += (s, e) => ShowChangePasswordDialog();
            editProfileButton.Click += (s, e) => ShowEditProfileDialog();

            // PROFILE: the header and the identity card are re-aligned whenever the
            // window changes size.
            profileHeader.Resize += (s, e) => LayoutProfileHeader();
            identityCard.Resize += (s, e) => LayoutProfileCard();
            detailsCard.Resize += (s, e) => LayoutDetailsCard();
            LayoutProfileHeader();
            LayoutProfileCard();
            LayoutDetailsCard();
            ConfigureGrids();
            BuildAttendanceTab();
            BuildAnalyticsTab();
            BuildRecordSearches();
            LayoutRecordSearches();

            tabControl1.SelectedIndexChanged += tabControl1_SelectedIndexChanged;

            // The ANALYTICS hover pictures live as long as the window, so they are
            // released together with the form instead of leaking bitmaps.
            Disposed += (s, e) => DisposeHoverImages();

            // Bring the attendance table up to date when the window is re-opened
            // from the scanner or from another application window.
            Activated += (s, e) =>
            {
                if (tabControl1.SelectedTab == tabPage7)
                {
                    LoadAttendanceLog();
                }
            };
        }

        public librarian_dashboard()
        {
            InitializeComponent();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // The window is painted first, then the catalog and the accounts are read,
            // so a slow MySQL server never freezes the dashboard while it opens.
            LoadCatalog();
            LoadAccounts();
            LoadProfileRecord();
            LoadBorrowRequests();
            LoadAttendanceLog();
            LoadAnalytics();
        }

        // ENCAPSULATION: one place that pushes the signed in person's details into
        // the header and the profile tab.
        private void ApplyAccountDetails()
        {
            accountNameLabel.Text = LoggedInFullName.ToUpperInvariant();
            headerRoleBadge.Text = LoggedInRole.ToUpperInvariant();
            profileRoleBadge.Text = LoggedInRole.ToUpperInvariant();

            profileName.Text = LoggedInFullName;
            avatarBadge.Initials = BuildInitials(LoggedInFullName);
            FitProfileName();

            valueFullName.Text = LoggedInFullName;
            valueRole.Text = LoggedInRole.ToUpperInvariant();
            valueStatus.Text = LoggedInStatus.Length > 0 ? LoggedInStatus.ToUpperInvariant() : "\u2014";

            bool isActive = string.Equals(LoggedInStatus, "ACTIVE", StringComparison.OrdinalIgnoreCase);
            valueStatus.ForeColor = isActive ? SuccessText : Color.FromArgb(180, 83, 9);

            LayoutHeaderCluster();
            LayoutProfileCard();
            ApplyAccountPermissions();
        }

        /// <summary>
        /// Only the library staff (librarian / staff) may approve or deactivate an
        /// account; a student login sees a read only ACCOUNTS tab.
        /// </summary>
        private static bool IsStaffRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                return false;
            }

            string value = role.ToUpperInvariant();

            return value.Contains("LIBRARIAN") || value.Contains("STAFF");
        }

        private void ApplyAccountPermissions()
        {
            // Activate / deactivate now lives on the ACCESS power icon of every
            // account row, so there is no stand-alone button left to enable here.

            // Only library staff may change the catalog or the borrow records.
            // Students and faculty still see the BOOKS and BORROWED BOOKS tabs, read only.
            addBookButton.Enabled = _canManageAccounts;
            borrowBookButton.Enabled = _canManageAccounts;
        }

        /// <summary>
        /// One shared look for every dashboard table: green header, soft alternating
        /// rows, no row headers and a highlighted full row selection.
        /// </summary>
        private void ConfigureGrids()
        {
            StyleGrid(accountsGrid);
            StyleGrid(staffGrid);
            StyleGrid(requestsGrid);
            StyleGrid(booksGrid);
            StyleGrid(attendanceGrid);
            StyleGrid(facultyGrid);
            StyleGrid(studentGrid);

            EnsureBookCoverColumn();

            CenterColumn(accountsGrid, accountIdColumn);
            CenterColumn(accountsGrid, accountStatusColumn);
            CenterColumn(accountsGrid, accountRoleColumn);

            CenterColumn(staffGrid, staffIdColumn);
            CenterColumn(staffGrid, staffStatusColumn);
            CenterColumn(staffGrid, staffRoleColumn);

            CenterColumn(requestsGrid, requestIdColumn);
            CenterColumn(requestsGrid, requestStudentIdColumn);
            CenterColumn(requestsGrid, requestDateColumn);
            CenterColumn(requestsGrid, requestCopiesColumn);
            CenterColumn(requestsGrid, requestStatusColumn);

            CenterColumn(facultyGrid, facultyIdColumn);
            CenterColumn(facultyGrid, facultyStatusColumn);

            CenterColumn(studentGrid, studentIdColumn);
            CenterColumn(studentGrid, studentSchoolIdColumn);
            CenterColumn(studentGrid, studentYearColumn);
            CenterColumn(studentGrid, studentContactColumn);
            CenterColumn(studentGrid, studentStatusColumn);

            CenterColumn(booksGrid, bookIdColumn);
            CenterColumn(booksGrid, bookYearColumn);
            CenterColumn(booksGrid, quantity);

            CenterColumn(attendanceGrid, loanIdColumn);
            CenterColumn(attendanceGrid, year);
            CenterColumn(attendanceGrid, loanCopyColumn);
            CenterColumn(attendanceGrid, studentid);
            CenterColumn(attendanceGrid, loanDateColumn);
            CenterColumn(attendanceGrid, loanDaysColumn);
            CenterColumn(attendanceGrid, loanReturnedColumn);
            CenterColumn(attendanceGrid, loanStatusColumn);

            // Every table gets the same two icon columns at the far right, so the
            // staff can edit or delete one row without leaving the tab.
            AddAccessColumn(accountsGrid);
            AddAccessColumn(staffGrid);
            AddAccessColumn(facultyGrid);
            AddAccessColumn(studentGrid);

            AddActionColumns(accountsGrid);
            AddActionColumns(staffGrid);
            AddActionColumns(booksGrid);
            AddActionColumns(attendanceGrid);
            AddActionColumns(facultyGrid);
            AddActionColumns(studentGrid);

            accountsGrid.CellClick += GridActionCellClick;
            staffGrid.CellClick += GridActionCellClick;
            booksGrid.CellClick += GridActionCellClick;
            attendanceGrid.CellClick += GridActionCellClick;
            facultyGrid.CellClick += GridActionCellClick;
            studentGrid.CellClick += GridActionCellClick;

            // The STATUS cell of an account row is a second shortcut for the ACCESS
            // icon: one double click flips the account between ACTIVE and INACTIVE.
            accountsGrid.CellDoubleClick += GridStatusCellDoubleClick;
            staffGrid.CellDoubleClick += GridStatusCellDoubleClick;
            facultyGrid.CellDoubleClick += GridStatusCellDoubleClick;
            studentGrid.CellDoubleClick += GridStatusCellDoubleClick;

            // The icon columns behave like small buttons, so the pointer turns into a
            // hand while it hovers over EDIT, DELETE, ACCESS, APPROVE or REJECT.
            DataGridView[] actionGrids =
            {
                accountsGrid,
                staffGrid,
                booksGrid,
                attendanceGrid,
                facultyGrid,
                studentGrid,
                requestsGrid
            };

            foreach (DataGridView grid in actionGrids)
            {
                grid.CellMouseEnter += GridActionCellMouseEnter;
                grid.CellMouseLeave += GridActionCellMouseLeave;
            }

            BalanceGridWidths();
        }

        private static void GridActionCellMouseEnter(object? sender, DataGridViewCellEventArgs e)
        {
            if (!(sender is DataGridView grid) || e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            string name = grid.Columns[e.ColumnIndex].Name;
            bool action = string.Equals(name, EditColumnName, StringComparison.Ordinal) ||
                          string.Equals(name, DeleteColumnName, StringComparison.Ordinal) ||
                          string.Equals(name, AccessColumnName, StringComparison.Ordinal) ||
                          string.Equals(name, RequestApproveColumnName, StringComparison.Ordinal) ||
                          string.Equals(name, RequestRejectColumnName, StringComparison.Ordinal) ||
                          string.Equals(name, RequestClaimColumnName, StringComparison.Ordinal);

            grid.Cursor = action ? Cursors.Hand : Cursors.Default;
        }

        private static void GridActionCellMouseLeave(object? sender, DataGridViewCellEventArgs e)
        {
            if (sender is DataGridView grid)
            {
                grid.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Double-clicking the STATUS cell of an account row activates or deactivates
        /// that exact record, so the staff never has to select a row first.
        /// </summary>
        private void GridStatusCellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (!(sender is DataGridView grid) || e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            DataGridViewColumn? statusColumn = StatusColumnFor(grid);

            if (statusColumn == null || e.ColumnIndex != statusColumn.Index)
            {
                return;
            }

            if (!_canManageAccounts)
            {
                MessageBox.Show(
                    "ONLY LIBRARY STAFF ACCOUNTS CAN ACTIVATE OR DEACTIVATE RECORDS.",
                    "READ ONLY",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (grid.Rows[e.RowIndex].Tag is AccountRow account)
            {
                ToggleAccountAccess(grid, account);
            }
        }

        private DataGridViewColumn? StatusColumnFor(DataGridView grid)
        {
            if (grid == accountsGrid)
            {
                return accountStatusColumn;
            }

            if (grid == staffGrid)
            {
                return staffStatusColumn;
            }

            if (grid == facultyGrid)
            {
                return facultyStatusColumn;
            }

            if (grid == studentGrid)
            {
                return studentStatusColumn;
            }

            return null;
        }

        // ------------------------------------------------------- LIBRARY ATTENDANCE

        /// <summary>
        /// Builds the LIBRARY ATTENDANCE tab. The designer leaves this tab empty,
        /// so the heading, the search field, the refresh / scanner buttons and the
        /// table are created here and follow the same look as the other tabs.
        /// </summary>
        private void BuildAttendanceTab()
        {
            if (libraryAttendanceGrid != null)
            {
                return;
            }

            tabPage7.BackColor = Color.White;
            tabPage7.Padding = new Padding(3);

            libraryAttendanceHeading = new Label
            {
                Text = "LIBRARY ATTENDANCE",
                AutoSize = true,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 14.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(30, 22)
            };

            libraryAttendanceSubtitle = new Label
            {
                Text = "Every QR code scanned at the library office entrance. Only registered and approved accounts are recorded.",
                AutoSize = true,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9F),
                ForeColor = MutedText,
                Location = new Point(32, 56)
            };

            libraryAttendanceRefreshButton = new RoundedButton
            {
                Text = "REFRESH",
                Location = new Point(32, 92),
                Size = new Size(126, 44),
                CornerRadius = 12,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                NormalColor = Color.FromArgb(22, 163, 74),
                HoverColor = Color.FromArgb(21, 128, 61),
                PressedColor = Color.FromArgb(20, 83, 45),
                ForeColor = Color.White,
                UseVisualStyleBackColor = false
            };

            libraryAttendanceRefreshButton.Click += (s, e) => LoadAttendanceLog();

            libraryAttendanceScanButton = new RoundedButton
            {
                Text = "OPEN QR SCANNER",
                Location = new Point(170, 92),
                Size = new Size(190, 44),
                CornerRadius = 12,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                NormalColor = Color.FromArgb(13, 148, 136),
                HoverColor = Color.FromArgb(15, 118, 110),
                PressedColor = Color.FromArgb(17, 94, 89),
                ForeColor = Color.White,
                UseVisualStyleBackColor = false
            };

            libraryAttendanceScanButton.Click += (s, e) => OpenAttendanceScanner();

            libraryAttendanceReportButton = new RoundedButton
            {
                Text = "EXPORT PDF",
                Location = new Point(372, 92),
                Size = new Size(150, 44),
                CornerRadius = 12,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                NormalColor = Color.FromArgb(194, 65, 12),
                HoverColor = Color.FromArgb(154, 52, 18),
                PressedColor = Color.FromArgb(120, 45, 15),
                ForeColor = Color.White,
                UseVisualStyleBackColor = false
            };

            libraryAttendanceReportButton.Click += (s, e) => ExportAttendancePdf();

            libraryAttendanceSearch = new SearchBox
            {
                PlaceholderText = "SEARCH ATTENDANCE...",
                Height = 44,
                Width = 360,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            libraryAttendanceSearch.SearchTextChanged += (s, e) => RebuildAttendanceLog();
            libraryAttendanceSearch.SearchRequested += (s, e) => RebuildAttendanceLog();

            libraryAttendanceStatus = new Label
            {
                Text = "ATTENDANCE HAS NOT BEEN LOADED YET.",
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 8.25F, FontStyle.Bold),
                ForeColor = MutedText,
                Height = 20,
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            libraryAttendanceGrid = new DataGridView
            {
                Name = "libraryAttendanceGrid",
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            StyleGrid(libraryAttendanceGrid);
            libraryAttendanceGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            AddLibraryAttendanceColumns();

            libraryAttendanceEmpty = new Label
            {
                Text = "NO ATTENDANCE RECORDED YET.\r\nSCAN A STUDENT, FACULTY OR STAFF QR CODE WITH THE SCANNER.",
                BackColor = Color.White,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(148, 163, 184),
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            tabPage7.Controls.Add(libraryAttendanceEmpty);
            tabPage7.Controls.Add(libraryAttendanceGrid);
            tabPage7.Controls.Add(libraryAttendanceStatus);
            tabPage7.Controls.Add(libraryAttendanceSearch);
            tabPage7.Controls.Add(libraryAttendanceReportButton);
            tabPage7.Controls.Add(libraryAttendanceScanButton);
            tabPage7.Controls.Add(libraryAttendanceRefreshButton);
            tabPage7.Controls.Add(libraryAttendanceSubtitle);
            tabPage7.Controls.Add(libraryAttendanceHeading);

            tabPage7.Resize += (s, e) => LayoutAttendanceTab();
            LayoutAttendanceTab();
        }

        private void AddLibraryAttendanceColumns()
        {
            libraryAttendanceGrid.Columns.Add(CreateAttendanceColumn("libraryAttendanceId", "ATTENDANCE #", 9F, 90));
            libraryAttendanceGrid.Columns.Add(CreateAttendanceColumn("libraryAttendanceStamp", "DATE & TIME", 24F, 160));
            libraryAttendanceGrid.Columns.Add(CreateAttendanceColumn("libraryAttendanceName", "NAME", 30F, 150));

            // ID NUMBER: the school ID recorded for a student. FACULTY, STAFF and
            // the LIBRARIAN have none, so their cell reads N/A. The values keep the
            // grid's left alignment and padding, so they line up under the header.
            DataGridViewTextBoxColumn idNumberColumn =
                CreateAttendanceColumn("libraryAttendanceIdNumber", "ID NUMBER", 14F, 110);
            libraryAttendanceGrid.Columns.Add(idNumberColumn);

            libraryAttendanceGrid.Columns.Add(CreateAttendanceColumn("libraryAttendanceRole", "ROLE", 12F, 90));
            libraryAttendanceGrid.Columns.Add(CreateAttendanceColumn("libraryAttendanceProgram", "PROGRAM / OFFICE", 20F, 120));
            libraryAttendanceGrid.Columns.Add(CreateAttendanceColumn("libraryAttendanceInstitute", "INSTITUTE", 13F, 100));

            EnsureHeaderFits(libraryAttendanceGrid);
        }

        private static DataGridViewTextBoxColumn CreateAttendanceColumn(string name, string header, float weight, int minimumWidth)
        {
            return new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = header,
                FillWeight = weight,
                MinimumWidth = minimumWidth,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            };
        }

        private void LayoutAttendanceTab()
        {
            if (libraryAttendanceGrid == null || tabPage7 == null || booksHeading == null ||
                libraryAttendanceReportButton == null)
            {
                return;
            }

            // The LIBRARY ATTENDANCE tab is built in code, so it borrows the geometry
            // of the BORROWED BOOKS tab: same heading, subtitle, toolbar and table
            // bands. That keeps every tab aligned at any window size.
            int left = attendanceGrid.Left;
            int right = attendanceGrid.Right;
            int toolbarTop = attendanceReportButton.Top;
            int toolbarHeight = Math.Max(32, attendanceReportButton.Height);
            int buttonWidth = Math.Max(100, (attendanceReportButton.Width * 3) / 4);

            libraryAttendanceHeading.Location = new Point(left, booksHeading.Top);
            libraryAttendanceSubtitle.Location = new Point(left + 2, booksSubtitle.Top);

            libraryAttendanceRefreshButton.SetBounds(left, toolbarTop, buttonWidth, toolbarHeight);
            libraryAttendanceScanButton.SetBounds(
                libraryAttendanceRefreshButton.Right + 12,
                toolbarTop,
                buttonWidth + 60,
                toolbarHeight);

            libraryAttendanceReportButton.SetBounds(
                libraryAttendanceScanButton.Right + 12,
                toolbarTop,
                buttonWidth + 24,
                toolbarHeight);

            int searchWidth = Math.Max(220, Math.Min(380, (attendanceGrid.Width / 3) + 40));
            libraryAttendanceSearch.SetBounds(right - searchWidth, toolbarTop, searchWidth, toolbarHeight);

            int statusLeft = libraryAttendanceReportButton.Right + 14;
            int statusWidth = libraryAttendanceSearch.Left - 14 - statusLeft;

            libraryAttendanceStatus.Anchor = AnchorStyles.Top;
            libraryAttendanceStatus.TextAlign = ContentAlignment.MiddleLeft;
            libraryAttendanceStatus.Visible = statusWidth >= 80;
            libraryAttendanceStatus.SetBounds(
                statusLeft,
                toolbarTop + ((toolbarHeight - libraryAttendanceStatus.Height) / 2),
                Math.Max(0, statusWidth),
                libraryAttendanceStatus.Height);

            libraryAttendanceGrid.SetBounds(
                left,
                attendanceGrid.Top,
                right - left,
                Math.Max(120, attendanceGrid.Height));

            libraryAttendanceEmpty.SetBounds(
                libraryAttendanceGrid.Left,
                libraryAttendanceGrid.Top,
                libraryAttendanceGrid.Width,
                libraryAttendanceGrid.Height);
        }

        private void OpenAttendanceScanner()
        {
            using (qr_code_scanner scanner = new qr_code_scanner())
            {
                scanner.ShowDialog(this);
            }

            // A scan writes straight into attendance_table, so the table is read
            // again as soon as the scanner closes.
            LoadAttendanceLog();
        }

        /// <summary>
        /// LIBRARY ATTENDANCE tab: writes the scan log to a PDF that follows the
        /// Learning Resource Center form. Every row is printed in full, so names,
        /// programs and institutes never wrap or get cut.
        /// </summary>
        private void ExportAttendancePdf()
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "PDF file (*.pdf)|*.pdf";
                dialog.FileName = "library-attendance-" +
                                  DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".pdf";
                dialog.Title = "EXPORT LIBRARY ATTENDANCE AS PDF";
                dialog.OverwritePrompt = true;

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    AttendancePdfExporter.Export(_attendanceRows, dialog.FileName);

                    DialogChrome.ShowAlert(
                        "LIBRARY ATTENDANCE EXPORTED TO:\n" + dialog.FileName,
                        "PDF SAVED",
                        MessageBoxIcon.Information);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    DialogChrome.ShowAlert(
                        "THE PDF COULD NOT BE SAVED.\n" + ex.Message,
                        "EXPORT FAILED",
                        MessageBoxIcon.Error);
                }
            }
        }

        private void LoadAttendanceLog()
        {
            try
            {
                _attendanceRows = _attendanceDirectory.LoadAll();
                _attendanceLogOnline = true;
            }
            catch (MySqlException)
            {
                _attendanceRows = new List<AttendanceRow>();
                _attendanceLogOnline = false;
            }

            RebuildAttendanceLog();
        }

        private void RebuildAttendanceLog()
        {
            if (libraryAttendanceGrid == null)
            {
                return;
            }

            string term = libraryAttendanceSearch.SearchText.ToUpperInvariant();
            int shown = 0;
            int today = 0;

            libraryAttendanceGrid.Rows.Clear();

            foreach (AttendanceRow visit in _attendanceRows)
            {
                if (term.Length > 0 && !MatchesAttendance(visit, term))
                {
                    continue;
                }

                int index = libraryAttendanceGrid.Rows.Add(
                    visit.AttendanceId,
                    visit.StampText,
                    visit.Name,
                    visit.DisplayIdNumber,
                    visit.Role,
                    visit.ProgramOrOffice,
                    visit.Institute);

                DataGridViewRow row = libraryAttendanceGrid.Rows[index];
                row.Cells["libraryAttendanceRole"].Style.Font = StatusCellFont;
                row.Cells["libraryAttendanceRole"].Style.ForeColor = RoleColor(visit.Role);
                row.Cells["libraryAttendanceIdNumber"].Style.ForeColor = MutedText;
                row.Cells["libraryAttendanceId"].Style.ForeColor = MutedText;
                row.Cells["libraryAttendanceStamp"].ToolTipText = visit.StampText;

                if (visit.IsToday)
                {
                    today++;
                }

                shown++;
            }

            libraryAttendanceGrid.ClearSelection();
            libraryAttendanceGrid.CurrentCell = null;

            bool hasRows = shown > 0;
            libraryAttendanceGrid.Visible = hasRows;
            libraryAttendanceEmpty.Visible = !hasRows;

            if (hasRows)
            {
                libraryAttendanceGrid.BringToFront();
            }
            else
            {
                libraryAttendanceEmpty.Text = term.Length > 0
                    ? "NO MATCHING ATTENDANCE RECORDS.\r\nTRY ANOTHER NAME, ROLE, ID NUMBER OR DATE."
                    : "NO ATTENDANCE RECORDED YET.\r\nSCAN A STUDENT, FACULTY OR STAFF QR CODE WITH THE SCANNER.";
                libraryAttendanceEmpty.BringToFront();
            }

            if (!_attendanceLogOnline)
            {
                libraryAttendanceStatus.Text = "MYSQL OFFLINE \u2022 ATTENDANCE CANNOT BE LOADED";
                libraryAttendanceStatus.ForeColor = WarningText;
                return;
            }

            if (_attendanceRows.Count == 0)
            {
                libraryAttendanceStatus.Text = "NO SCANS YET \u2022 OPEN THE QR SCANNER TO RECORD THE FIRST VISIT.";
            }
            else
            {
                libraryAttendanceStatus.Text =
                    _attendanceRows.Count.ToString(CultureInfo.InvariantCulture) + " SCAN(S) ON FILE  \u2022  " +
                    today.ToString(CultureInfo.InvariantCulture) + " TODAY  \u2022  SHOWING " +
                    shown.ToString(CultureInfo.InvariantCulture);
            }

            libraryAttendanceStatus.ForeColor = _attendanceRows.Count == 0 ? WarningText : MutedText;
        }

        private static bool MatchesAttendance(AttendanceRow visit, string term)
        {
            // NAME, ID NUMBER, ROLE, PROGRAM / OFFICE, INSTITUTE and the date and
            // time are the columns the table shows, so they are what SEARCH checks.
            return visit.Name.ToUpperInvariant().Contains(term) ||
                   visit.DisplayIdNumber.ToUpperInvariant().Contains(term) ||
                   visit.Role.Contains(term) ||
                   visit.ProgramOrOffice.ToUpperInvariant().Contains(term) ||
                   visit.Institute.ToUpperInvariant().Contains(term) ||
                   visit.DateText.Contains(term) ||
                   visit.TimeText.Contains(term);
        }

        private static Color RoleColor(string role)
        {
            switch (role)
            {
                case "FACULTY":
                    return Color.FromArgb(37, 99, 235);
                case "STAFF":
                    return Color.FromArgb(194, 65, 12);
                default:
                    return HeaderGreen;
            }
        }

        // ------------------------------------------------------------------ SEARCH

        /// <summary>
        /// Adds one search field to every table tab. Each field filters only its own
        /// table and keeps the typed term while the tab is refreshed or reopened.
        /// </summary>
        private void BuildRecordSearches()
        {
            if (accountsSearch != null)
            {
                return;
            }

            accountsSearch = AddRecordSearch(tabPage2, "SEARCH LIBRARIAN ACCOUNTS...", FillAccountGrids);
            staffSearch = AddRecordSearch(staffTab, "SEARCH STAFF ACCOUNTS...", FillAccountGrids);
            facultySearch = AddRecordSearch(tabPage5, "SEARCH FACULTY ACCOUNTS...", FillAccountGrids);
            studentSearch = AddRecordSearch(tabPage6, "SEARCH STUDENT ACCOUNTS...", FillAccountGrids);
            requestsSearch = AddRecordSearch(requestsTab, "SEARCH REQUESTS...", FillRequestGrid);
            booksSearch = AddRecordSearch(tabPage3, "SEARCH BOOKS...", RefreshCatalogViews);
            borrowedSearch = AddRecordSearch(tabPage4, "SEARCH BORROWED BOOKS...", RefreshCatalogViews);

            // BORROW REQUESTS is the only designer tab without an empty state label.
            requestsEmptyLabel = new Label
            {
                Name = "requestsEmptyLabel",
                Text = "NO PENDING REQUESTS. STUDENT REQUESTS WILL APPEAR HERE.",
                BackColor = Color.White,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(148, 163, 184),
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            requestsTab.Controls.Add(requestsEmptyLabel);
            requestsEmptyLabel.BringToFront();
        }

        private static SearchBox AddRecordSearch(TabPage page, string placeholder, Action rebuild)
        {
            SearchBox search = new SearchBox
            {
                Name = page.Name + "Search",
                PlaceholderText = placeholder,
                Height = 44,
                Width = 360,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            search.SearchTextChanged += (s, e) => rebuild();
            search.SearchRequested += (s, e) => rebuild();

            page.Controls.Add(search);
            search.BringToFront();

            return search;
        }

        /// <summary>
        /// Places every search field on the right edge of its table toolbar and lets
        /// the status line fill the free space between the buttons and the field.
        /// </summary>
        private void LayoutRecordSearches()
        {
            if (accountsSearch == null)
            {
                return;
            }

            LayoutSearchRow(accountsSearch, accountsStatusLabel, accountsRefreshButton, accountsGrid);
            LayoutSearchRow(staffSearch, staffStatusLabel, staffRefreshButton, staffGrid);
            LayoutSearchRow(facultySearch, facultyStatusLabel, facultyRefreshButton, facultyGrid);
            LayoutSearchRow(studentSearch, studentStatusLabel, studentRefreshButton, studentGrid);
            LayoutSearchRow(requestsSearch, requestsStatusLabel, requestsRefreshButton, requestsGrid);
            LayoutSearchRow(borrowedSearch, attendanceStatusLabel, attendanceReportButton, attendanceGrid);

            // The BOOKS tab fills the action row with its four summary cards, so its
            // search field sits in the page header instead of the toolbar.
            LayoutHeaderSearch(booksSearch, booksGrid, booksHeading);
            LayoutEmptyPage(requestsEmptyLabel, requestsGrid);

            LayoutAttendanceTab();
        }

        private static void LayoutEmptyPage(Label? label, DataGridView grid)
        {
            if (label == null || label.Parent == null)
            {
                return;
            }

            label.SetBounds(grid.Left, grid.Top, grid.Width, grid.Height);
        }

        /// <summary>
        /// Docks one search field to the right edge of its table and lets the status
        /// line fill the gap between the last toolbar button and the field. The
        /// positions come from the real controls, so the row stays aligned whatever
        /// scaling Windows applies to the form.
        /// </summary>
        private static void LayoutSearchRow(SearchBox search, Label status, Control toolbarEnd, DataGridView grid)
        {
            if (search.Parent == null)
            {
                return;
            }

            int searchWidth = Math.Max(220, Math.Min(380, (grid.Width / 3) + 40));
            int height = Math.Max(32, toolbarEnd.Height);
            int top = toolbarEnd.Top;

            search.SetBounds(grid.Right - searchWidth, top, searchWidth, height);

            int statusLeft = toolbarEnd.Right + 14;
            int statusWidth = search.Left - 14 - statusLeft;

            status.Anchor = AnchorStyles.Top;
            status.TextAlign = ContentAlignment.MiddleLeft;
            status.Visible = statusWidth >= 80;
            status.SetBounds(statusLeft, top + ((height - status.Height) / 2), Math.Max(0, statusWidth), status.Height);
        }

        private static void LayoutHeaderSearch(SearchBox search, DataGridView grid, Control heading)
        {
            if (search.Parent == null)
            {
                return;
            }

            int searchWidth = Math.Max(220, Math.Min(360, grid.Width / 3));
            int height = Math.Max(32, heading.Height + 8);

            search.SetBounds(grid.Right - searchWidth, Math.Max(4, heading.Top - 2), searchWidth, height);
        }

        private static string Term(SearchBox? search)
        {
            return search == null ? string.Empty : search.SearchText.Trim().ToUpperInvariant();
        }

        private static bool MatchesTerm(string term, params string?[] values)
        {
            if (term.Length == 0)
            {
                return true;
            }

            foreach (string? value in values)
            {
                if (!string.IsNullOrWhiteSpace(value) &&
                    value.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static void SetEmptyText(Label label, string term, string noMatch, string empty)
        {
            label.Text = term.Length > 0 ? noMatch : empty;
        }

        /// <summary>
        /// BALANCED WIDTHS: every data column shares the table width with a weight that
        /// matches its content, so one long title or one short ID can no longer squeeze
        /// the other columns. The icon columns keep a fixed narrow width.
        /// </summary>
        private void BalanceGridWidths()
        {
            booksGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            BalanceColumn(bookIdColumn, 6F, 50);
            BalanceColumn(bookTitleColumn, 34F, 170);
            BalanceColumn(bookAuthorColumn, 22F, 130);
            BalanceColumn(bookYearColumn, 8F, 70);
            BalanceColumn(quantity, 12F, 105);
            BalanceColumn(bookAddedColumn, 18F, 120);

            attendanceGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            BalanceColumn(loanIdColumn, 4F, 44);
            BalanceColumn(loanTitleColumn, 16F, 150);
            BalanceColumn(year, 6F, 65);
            BalanceColumn(loanCopyColumn, 8F, 95);
            BalanceColumn(loanBorrowerColumn, 15F, 140);
            BalanceColumn(program_, 9F, 90);
            BalanceColumn(institute, 10F, 110);
            BalanceColumn(studentid, 9F, 95);
            BalanceColumn(loanDateColumn, 8F, 95);
            BalanceColumn(loanDaysColumn, 5F, 50);
            BalanceColumn(loanReturnedColumn, 8F, 90);
            BalanceColumn(loanStatusColumn, 9F, 105);

            requestsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            BalanceColumn(requestIdColumn, 5F, 50);
            BalanceColumn(requestBookColumn, 22F, 150);
            BalanceColumn(requestStudentColumn, 20F, 150);
            BalanceColumn(requestStudentIdColumn, 10F, 90);
            BalanceColumn(requestProgramColumn, 12F, 95);
            BalanceColumn(requestDateColumn, 14F, 135);
            BalanceColumn(requestCopiesColumn, 6F, 70);
            BalanceColumn(requestStatusColumn, 9F, 90);

            accountsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            BalanceColumn(accountIdColumn, 6F, 60);
            BalanceColumn(accountNameColumn, 28F, 150);
            BalanceColumn(accountUsernameColumn, 18F, 110);
            BalanceColumn(accountPasswordColumn, 16F, 110);
            BalanceColumn(accountStatusColumn, 16F, 100);
            BalanceColumn(accountRoleColumn, 16F, 100);

            staffGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            BalanceColumn(staffIdColumn, 5F, 60);
            BalanceColumn(staffNameColumn, 24F, 150);
            BalanceColumn(staffUsernameColumn, 15F, 110);
            BalanceColumn(staffPasswordColumn, 13F, 110);
            BalanceColumn(staffOfficeColumn, 20F, 130);
            BalanceColumn(staffStatusColumn, 12F, 100);
            BalanceColumn(staffRoleColumn, 11F, 100);

            facultyGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            BalanceColumn(facultyIdColumn, 6F, 60);
            BalanceColumn(facultyNameColumn, 30F, 150);
            BalanceColumn(facultyUsernameColumn, 20F, 110);
            BalanceColumn(facultyInstituteColumn, 22F, 120);
            BalanceColumn(facultyStatusColumn, 12F, 95);

            studentGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            BalanceColumn(studentIdColumn, 5F, 55);
            BalanceColumn(studentNameColumn, 17F, 130);
            BalanceColumn(studentUsernameColumn, 13F, 90);
            BalanceColumn(studentSchoolIdColumn, 12F, 110);
            BalanceColumn(studentProgramColumn, 16F, 120);
            BalanceColumn(studentYearColumn, 8F, 80);
            BalanceColumn(studentInstituteColumn, 13F, 100);
            BalanceColumn(studentContactColumn, 12F, 110);
            BalanceColumn(studentStatusColumn, 10F, 80);

            EnsureHeaderFits(accountsGrid);
            EnsureHeaderFits(staffGrid);
            EnsureHeaderFits(requestsGrid);
            EnsureHeaderFits(booksGrid);
            EnsureHeaderFits(attendanceGrid);
            EnsureHeaderFits(facultyGrid);
            EnsureHeaderFits(studentGrid);
        }

        /// <summary>
        /// HEADERS ON ONE LINE: every header caption is measured once, so the column
        /// width is always wide enough to show the whole name and WinForms never wraps
        /// it onto a second line or cuts it with an ellipsis.
        /// </summary>
        private static void EnsureHeaderFits(DataGridView grid)
        {
            Font headerFont = grid.ColumnHeadersDefaultCellStyle.Font ?? grid.Font;

            foreach (DataGridViewColumn column in grid.Columns)
            {
                if (column.Index < 0)
                {
                    continue;
                }

                column.HeaderCell.Style.WrapMode = DataGridViewTriState.False;

                int textWidth = TextRenderer.MeasureText(
                    column.HeaderText ?? string.Empty,
                    headerFont,
                    new Size(int.MaxValue, grid.ColumnHeadersHeight),
                    TextFormatFlags.SingleLine | TextFormatFlags.NoPadding).Width;

                // 10 px of header padding on the left + breathing room on the right.
                int needed = textWidth + 26;

                if (column.MinimumWidth < needed)
                {
                    column.MinimumWidth = needed;
                }

                if (column.AutoSizeMode == DataGridViewAutoSizeColumnMode.None && column.Width < needed)
                {
                    column.Width = needed;
                }
            }
        }

        private static void BalanceColumn(DataGridViewColumn? column, float weight, int minimumWidth)
        {
            if (column == null || column.Index < 0)
            {
                return;
            }

            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            column.FillWeight = weight;
            column.MinimumWidth = minimumWidth;
            column.HeaderCell.Style.WrapMode = DataGridViewTriState.False;
        }

        private static void AddActionColumns(DataGridView grid)
        {
            grid.Columns.Add(CreateIconColumn(EditColumnName, "EDIT", EditGlyph,
                Color.FromArgb(4, 120, 87), "Click the pencil to edit this row."));
            grid.Columns.Add(CreateIconColumn(DeleteColumnName, "DELETE", DeleteGlyph,
                Color.FromArgb(185, 28, 28), "Click the trash can to delete this row."));
        }

        /// <summary>
        /// Adds the cover picture column to BOOKS once. The rest of the table keeps
        /// its designer columns; the image is inserted after the ID.
        /// </summary>
        private void EnsureBookCoverColumn()
        {
            if (booksGrid.Columns.Contains(BookCoverColumnName))
            {
                bookCoverColumn = (DataGridViewImageColumn)booksGrid.Columns[BookCoverColumnName]!;
                return;
            }

            bookCoverColumn = new DataGridViewImageColumn();
            bookCoverColumn.Name = BookCoverColumnName;
            bookCoverColumn.HeaderText = "COVER";
            bookCoverColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            bookCoverColumn.Width = 78;
            bookCoverColumn.MinimumWidth = 72;
            bookCoverColumn.ReadOnly = true;
            bookCoverColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            bookCoverColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
            bookCoverColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            bookCoverColumn.DefaultCellStyle.Padding = new Padding(4);
            bookCoverColumn.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            bookCoverColumn.HeaderCell.Style.WrapMode = DataGridViewTriState.False;

            booksGrid.Columns.Insert(1, bookCoverColumn);
            booksGrid.RowTemplate.Height = 64;
        }

        private static Image CreateBookThumbnail(Book book)
        {
            return BookImage.CreateThumbnail(book.CoverImage, 70, 58, true);
        }

        private void DisposeBookCoverImages()
        {
            if (bookCoverColumn == null || !booksGrid.Columns.Contains(BookCoverColumnName))
            {
                return;
            }

            foreach (DataGridViewRow row in booksGrid.Rows)
            {
                if (row.Cells[BookCoverColumnName].Value is Image image)
                {
                    image.Dispose();
                }
            }
        }

        // Only the account tables have an approval status, so only they get the
        // power icon that activates or deactivates the account in one click.
        private static void AddAccessColumn(DataGridView grid)
        {
            grid.Columns.Add(CreateIconColumn(AccessColumnName, "ACCESS", AccessGlyph,
                Color.FromArgb(100, 116, 139), "Click the power icon to activate or deactivate this account."));
        }

        private static DataGridViewTextBoxColumn CreateIconColumn(
            string name, string headerText, string glyph, Color color, string toolTip)
        {
            DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
            column.Name = name;
            column.HeaderText = headerText;
            // The icon columns keep a fixed, narrow width while the data columns fill.
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            column.Width = 64;
            column.MinimumWidth = 56;
            column.ReadOnly = true;
            column.SortMode = DataGridViewColumnSortMode.NotSortable;
            column.ToolTipText = toolTip;
            column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            column.DefaultCellStyle.Padding = new Padding(0);
            column.DefaultCellStyle.Font = IconCellFont;
            column.DefaultCellStyle.ForeColor = color;
            column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            column.HeaderCell.Style.Padding = new Padding(0);
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
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.75F, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            grid.ColumnHeadersHeight = 44;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            grid.DefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 252, 231);
            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(6, 78, 59);

            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 252, 231);
            grid.RowTemplate.Height = 38;
        }

        private static void CenterColumn(DataGridView grid, DataGridViewColumn column)
        {
            if (column == null || column.Index < 0)
            {
                return;
            }

            column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        // ---------------------------------------------------------------- CATALOG

        private void LoadCatalog()
        {
            _catalog.Load();
            _catalog.ClearWarning();

            ApplyCatalogStatus();
            RefreshCatalogViews();
        }

        private void RefreshCatalogViews()
        {
            string booksTerm = Term(booksSearch);
            string borrowTerm = Term(borrowedSearch);

            DisposeBookCoverImages();
            booksGrid.Rows.Clear();

            int booksShown = 0;
            int booksTotal = 0;

            foreach (Book book in _catalog.Books)
            {
                booksTotal++;

                if (!MatchesTerm(booksTerm, book.Title, book.Author, book.YearText))
                {
                    continue;
                }

                int availableCopies = _catalog.AvailableCopyCount(book);

                int index = booksGrid.Rows.Add(
                    book.BookId,
                    CreateBookThumbnail(book),
                    book.Title,
                    book.Author,
                    book.YearText,
                    availableCopies.ToString(CultureInfo.InvariantCulture),
                    book.DateAdded,
                    EditGlyph,
                    DeleteGlyph);

                booksGrid.Rows[index].Tag = book;
                booksShown++;
            }

            attendanceGrid.Rows.Clear();

            int borrowsShown = 0;
            int borrowsTotal = 0;

            // One line per borrower + title + borrow date: the copies of a request all
            // stay as separate rows in borrow_transactions_table, but the table reports
            // the number of copies in BORROWED COPY instead of repeating the loan.
            foreach (BorrowedBookGroup group in BorrowedBookGroup.BuildGroups(_catalog.BorrowRecords))
            {
                borrowsTotal++;

                if (!MatchesTerm(borrowTerm, group.BookTitle, group.Author, group.BookYearText,
                                 group.BorrowerName, group.BorrowerIdNumber, group.ProgramText,
                                 group.InstituteText, group.DateBorrowedText, group.DateReturnedText,
                                 group.Status))
                {
                    continue;
                }

                int index = attendanceGrid.Rows.Add(
                    group.BorrowIdText,
                    group.BookTitle,
                    group.BookYearText,
                    group.BorrowerIdNumber,
                    group.BorrowerName,
                    group.ProgramText,
                    group.InstituteText,
                    group.CopyCountText,
                    group.DateBorrowedText,
                    group.DateReturnedText,
                    group.DaysBorrowedText,
                    group.Status,
                    EditGlyph,
                    DeleteGlyph);

                DataGridViewRow row = attendanceGrid.Rows[index];
                row.Tag = group;
                row.Cells[loanBorrowerColumn.Index].ToolTipText =
                    "BORROWER: " + group.BorrowerName + " (" + group.BorrowerIdNumber + ")";
                row.Cells[loanIdColumn.Index].ToolTipText = group.CopyCount > 1
                    ? group.CopyCount + " COPY RECORDS: " + group.BorrowIdListText
                    : "BORROW RECORD " + group.BorrowIdListText;
                row.Cells[loanCopyColumn.Index].ToolTipText = group.CopyCount == 1
                    ? "1 COPY OF \"" + group.BookTitle + "\""
                    : group.CopyCount + " COPIES OF \"" + group.BookTitle + "\"";
                row.Cells[loanDateColumn.Index].ToolTipText =
                    "BORROWED " + group.DateBorrowedStampText;
                row.Cells[loanDaysColumn.Index].ToolTipText =
                    group.DaysBorrowedText + " SINCE " + group.DateBorrowedStampText;
                row.Cells[loanStatusColumn.Index].ToolTipText = group.CopyCount > 1
                    ? group.OpenCount + " OF " + group.CopyCount + " COPIES STILL BORROWED"
                    : "STATUS: " + group.Status;
                borrowsShown++;
            }

            _booksShown = booksShown;
            _booksTotal = booksTotal;
            _borrowsShown = borrowsShown;
            _borrowsTotal = borrowsTotal;

            ColorBorrowStatuses();

            bookStatTotalValue.Text = _catalog.TotalBooks.ToString(CultureInfo.InvariantCulture);
            bookStatAvailableValue.Text = _catalog.AvailableBooks.ToString(CultureInfo.InvariantCulture);
            bookStatBorrowedValue.Text = _catalog.BorrowedBooks.ToString(CultureInfo.InvariantCulture);
            bookStatLoansValue.Text = _catalog.BorrowRecords.Count.ToString(CultureInfo.InvariantCulture);

            SetEmptyText(booksEmptyLabel, booksTerm,
                "NO BOOK MATCHES THE SEARCH.\r\nTRY ANOTHER TITLE, AUTHOR OR YEAR.",
                "NO BOOKS IN THE CATALOG YET.\r\nUSE + ADD BOOK TO REGISTER THE FIRST TITLE.");
            SetEmptyText(attendanceEmptyLabel, borrowTerm,
                "NO BORROW RECORD MATCHES THE SEARCH.\r\nTRY ANOTHER TITLE, BORROWER, PROGRAM OR STATUS.",
                "NO BORROW RECORDS YET.\r\nUSE BORROW BOOK IN THE BOOK CATALOG TAB TO LOG THE FIRST LOAN.");

            booksGrid.ClearSelection();
            attendanceGrid.ClearSelection();
            booksGrid.CurrentCell = null;
            attendanceGrid.CurrentCell = null;

            ApplyCatalogStatus();
            UpdateEmptyStates();
        }

        /// <summary>
        /// WinForms ignores Visible = true while the owning tab page is hidden, so the
        /// empty state is re-applied once the tab page has actually been shown.
        /// </summary>
        private void UpdateEmptyStates()
        {
            if (!IsHandleCreated)
            {
                return;
            }

            BeginInvoke(new Action(ApplyEmptyStates));
        }

        private void ApplyEmptyStates()
        {
            // The grid and the empty message share the same area. Only one of them is
            // shown at a time, so the message can never be painted behind the grid.
            bool booksEmpty = booksGrid.Rows.Count == 0;
            booksGrid.Visible = !booksEmpty;
            booksEmptyLabel.Visible = booksEmpty;

            bool attendanceEmpty = attendanceGrid.Rows.Count == 0;
            attendanceGrid.Visible = !attendanceEmpty;
            attendanceEmptyLabel.Visible = attendanceEmpty;

            bool accountsEmpty = accountsGrid.Rows.Count == 0;
            accountsGrid.Visible = !accountsEmpty;
            accountsEmptyLabel.Visible = accountsEmpty;

            bool staffEmpty = staffGrid.Rows.Count == 0;
            staffGrid.Visible = !staffEmpty;
            staffEmptyLabel.Visible = staffEmpty;

            bool facultyEmpty = facultyGrid.Rows.Count == 0;
            facultyGrid.Visible = !facultyEmpty;
            facultyEmptyLabel.Visible = facultyEmpty;

            bool studentEmpty = studentGrid.Rows.Count == 0;
            studentGrid.Visible = !studentEmpty;
            studentEmptyLabel.Visible = studentEmpty;

            if (booksEmpty)
            {
                booksEmptyLabel.BringToFront();
            }

            if (attendanceEmpty)
            {
                attendanceEmptyLabel.BringToFront();
            }

            if (accountsEmpty)
            {
                accountsEmptyLabel.BringToFront();
            }

            if (staffEmpty)
            {
                staffEmptyLabel.BringToFront();
            }

            if (facultyEmpty)
            {
                facultyEmptyLabel.BringToFront();
            }

            if (studentEmpty)
            {
                studentEmptyLabel.BringToFront();
            }
        }

        private void ApplyCatalogStatus()
        {
            bool connected = _catalog.IsConnected;
            string readOnly = _canManageAccounts ? string.Empty : " \u2022 READ ONLY";

            string booksFilter = Term(booksSearch).Length > 0
                ? " \u2022 SHOWING " + _booksShown.ToString(CultureInfo.InvariantCulture) +
                  " OF " + _booksTotal.ToString(CultureInfo.InvariantCulture)
                : string.Empty;

            string borrowFilter = Term(borrowedSearch).Length > 0
                ? " \u2022 SHOWING " + _borrowsShown.ToString(CultureInfo.InvariantCulture) +
                  " OF " + _borrowsTotal.ToString(CultureInfo.InvariantCulture)
                : string.Empty;

            booksStatusLabel.Text = connected
                ? "CATALOG SYNCED WITH MYSQL" + readOnly + booksFilter
                : "MYSQL OFFLINE \u2022 CHANGES ARE KEPT FOR THIS SESSION ONLY";
            booksStatusLabel.ForeColor = connected ? MutedText : WarningText;

            attendanceStatusLabel.Text = connected
                ? "ONE DAY IS ADDED EVERY 24 HOURS." + readOnly + borrowFilter
                : "MYSQL OFFLINE \u2022 CHANGES ARE KEPT FOR THIS SESSION ONLY";
            attendanceStatusLabel.ForeColor = connected ? MutedText : WarningText;
        }

        private void ColorBorrowStatuses()
        {
            foreach (DataGridViewRow row in attendanceGrid.Rows)
            {
                DataGridViewCell cell = row.Cells[loanStatusColumn.Index];
                string status = Convert.ToString(cell.Value, CultureInfo.InvariantCulture) ?? string.Empty;

                if (string.Equals(status, "RETURNED", StringComparison.OrdinalIgnoreCase))
                {
                    cell.Style.ForeColor = MutedText;
                }
                else
                {
                    cell.Style.ForeColor = SuccessText;
                }

                cell.Style.Font = StatusCellFont;
            }
        }

        private void ShowCatalogWarning()
        {
            if (_catalog.LastWarning == null)
            {
                return;
            }

            string warning = _catalog.LastWarning.ToUpperInvariant();
            _catalog.ClearWarning();

            MessageBox.Show(warning, "DATABASE NOTICE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        // ---------------------------------------------------- BORROW REQUESTS

        private void LoadBorrowRequests()
        {
            if (requestsGrid == null)
            {
                return;
            }

            try
            {
                _requestQueue = _requestDirectory.LoadQueue();
                _requestsOnline = true;
            }
            catch (MySqlException)
            {
                _requestQueue = new List<BorrowRequestRow>();
                _requestsOnline = false;
                FillRequestGrid();
                return;
            }

            FillRequestGrid();
        }

        /// <summary>
        /// Rebuilds the BORROW REQUESTS table from the cached list and applies the
        /// term typed in the requests search field.
        /// </summary>
        private void FillRequestGrid()
        {
            if (requestsGrid == null)
            {
                return;
            }

            string term = Term(requestsSearch);
            int shown = 0;

            requestsGrid.Rows.Clear();

            if (_requestsOnline)
            {
                foreach (BorrowRequestRow request in _requestQueue)
                {
                    if (!MatchesTerm(term, request.BookTitle, request.StudentName, request.StudentIdText,
                                     request.ProgramText, request.RequestDateText, request.Status))
                    {
                        continue;
                    }

                    // PENDING rows offer APPROVE / REJECT. APPROVED rows stay in the
                    // queue with one CLAIM action until the student collects the books.
                    bool pending = request.IsPending;
                    bool approved = request.IsApproved;

                    int index = requestsGrid.Rows.Add(
                        request.RequestId,
                        request.BookTitle,
                        request.StudentName,
                        request.StudentIdText,
                        request.ProgramText,
                        request.RequestDateText,
                        request.CopiesText,
                        request.Status,
                        pending ? "APPROVE" : "\u2014",
                        pending ? "REJECT" : "\u2014",
                        approved ? "CLAIM" : "\u2014");

                    DataGridViewRow row = requestsGrid.Rows[index];
                    row.Tag = request;

                    DataGridViewCell statusCell = row.Cells[requestStatusColumn.Index];
                    statusCell.Style.ForeColor = approved ? SuccessText : WarningText;
                    statusCell.Style.Font = StatusCellFont;

                    row.Cells[RequestApproveColumnName].ToolTipText =
                        "Approve this request and create the borrow record.";
                    row.Cells[RequestRejectColumnName].ToolTipText =
                        "Reject this request without creating a borrow record.";
                    row.Cells[RequestClaimColumnName].ToolTipText = approved
                        ? "Confirm that the student personally received the books in the library."
                        : "Available once the request is approved.";
                    shown++;
                }
            }

            int pendingCount = _requestQueue.Count(request => request.IsPending);
            int readyCount = _requestQueue.Count(request => request.IsApproved);

            if (!_requestsOnline)
            {
                requestsStatusLabel.Text = "MYSQL OFFLINE \u2022 REQUESTS CANNOT BE LOADED";
                requestsStatusLabel.ForeColor = WarningText;
            }
            else if (_requestQueue.Count == 0)
            {
                requestsStatusLabel.Text = "NO PENDING OR READY-TO-CLAIM REQUESTS. STUDENT REQUESTS WILL APPEAR HERE.";
                requestsStatusLabel.ForeColor = MutedText;
            }
            else if (shown == 0)
            {
                requestsStatusLabel.Text = "NO REQUEST MATCHES \"" + term + "\" \u2022 PRESS THE X IN THE SEARCH BOX.";
                requestsStatusLabel.ForeColor = WarningText;
            }
            else
            {
                string countText = shown.ToString(CultureInfo.InvariantCulture);

                if (shown != _requestQueue.Count)
                {
                    countText += " OF " + _requestQueue.Count.ToString(CultureInfo.InvariantCulture);
                }

                requestsStatusLabel.Text = countText + " REQUEST(S)  \u2022  " +
                    pendingCount.ToString(CultureInfo.InvariantCulture) + " PENDING  \u2022  " +
                    readyCount.ToString(CultureInfo.InvariantCulture) + " READY TO CLAIM";
                requestsStatusLabel.ForeColor = MutedText;
            }

            ClearGridSelection(requestsGrid);

            requestsGrid.Visible = shown > 0;
            requestsEmptyLabel.Visible = _requestsOnline && shown == 0;

            if (requestsEmptyLabel.Visible)
            {
                requestsEmptyLabel.Text = term.Length > 0
                    ? "NO REQUEST MATCHES THE SEARCH.\r\nTRY ANOTHER BOOK, NAME, PROGRAM OR STATUS."
                    : "NO PENDING OR READY-TO-CLAIM REQUESTS.\r\nSTUDENT REQUESTS WILL APPEAR HERE.";
                requestsEmptyLabel.BringToFront();
            }

            UpdateEmptyStates();
        }

        private void RequestsGrid_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            string columnName = requestsGrid.Columns[e.ColumnIndex].Name;
            bool approve = string.Equals(columnName, RequestApproveColumnName, StringComparison.Ordinal);
            bool reject = string.Equals(columnName, RequestRejectColumnName, StringComparison.Ordinal);
            bool claim = string.Equals(columnName, RequestClaimColumnName, StringComparison.Ordinal);

            if (!approve && !reject && !claim)
            {
                return;
            }

            if (!_canManageAccounts)
            {
                MessageBox.Show(
                    "ONLY LIBRARY STAFF ACCOUNTS CAN APPROVE, REJECT OR CLAIM BORROW REQUESTS.",
                    "READ ONLY",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            BorrowRequestRow? request = requestsGrid.Rows[e.RowIndex].Tag as BorrowRequestRow;

            if (request == null)
            {
                return;
            }

            // The cells that do not apply to the row's state show a dash; clicking
            // them should do nothing instead of opening a warning dialog.
            if ((approve || reject) && !request.IsPending)
            {
                return;
            }

            if (claim && !request.IsApproved)
            {
                return;
            }

            if (claim)
            {
                ClaimBorrowRequest(request);
            }
            else if (approve)
            {
                ApproveBorrowRequest(request);
            }
            else
            {
                RejectBorrowRequest(request);
            }
        }

        private void ApproveBorrowRequest(BorrowRequestRow request)
        {
            if (!request.IsPending)
            {
                ShowError("REQUEST #" + request.RequestId + " IS NO LONGER PENDING.");
                return;
            }

            string copiesText = request.Copies == 1 ? "1 COPY" : request.Copies + " COPIES";

            DialogResult answer = MessageBox.Show(
                "APPROVE REQUEST #" + request.RequestId + "?\n\n" +
                request.StudentName.ToUpperInvariant() + " WILL BORROW " + copiesText + " OF \"" +
                request.BookTitle.ToUpperInvariant() + "\".",
                "APPROVE BORROW REQUEST",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (answer != DialogResult.Yes)
            {
                return;
            }

            try
            {
                System.Collections.Generic.List<BorrowRecord> records = _requestDirectory.Approve(
                    request, LoggedInAccountId, _catalog, _accountDirectory);

                _catalog.SyncDaysBorrowed();
                RefreshCatalogViews();
                ApplyCatalogStatus();
                LoadBorrowRequests();

                requestsStatusLabel.Text = "REQUEST #" + request.RequestId + " APPROVED. " +
                                           (records.Count == 1 ? "1 BORROW RECORD" : records.Count + " BORROW RECORDS") +
                                           " CREATED.";
                requestsStatusLabel.ForeColor = SuccessText;
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                ShowError(ex.Message);
            }
            catch (MySqlException ex)
            {
                ShowError("THE REQUEST COULD NOT BE APPROVED.\n" + ex.Message);
            }
        }

        private void RejectBorrowRequest(BorrowRequestRow request)
        {
            if (!request.IsPending)
            {
                ShowError("REQUEST #" + request.RequestId + " IS NO LONGER PENDING.");
                return;
            }

            DialogResult answer = MessageBox.Show(
                "REJECT REQUEST #" + request.RequestId + " FROM " +
                request.StudentName.ToUpperInvariant() + "?\nNO LOAN WILL BE CREATED.",
                "REJECT BORROW REQUEST",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (answer != DialogResult.Yes)
            {
                return;
            }

            try
            {
                _requestDirectory.Reject(request, LoggedInAccountId);
                LoadBorrowRequests();

                requestsStatusLabel.Text = "REQUEST #" + request.RequestId + " WAS REJECTED.";
                requestsStatusLabel.ForeColor = DangerText;
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                ShowError(ex.Message);
            }
            catch (MySqlException ex)
            {
                ShowError("THE REQUEST COULD NOT BE REJECTED.\n" + ex.Message);
            }
        }

        /// <summary>
        /// Second step of the request workflow: the librarian confirms that the
        /// student personally collected the approved books in the library. The
        /// request becomes CLAIMED, which is the status MY REQUESTS then shows.
        /// </summary>
        private void ClaimBorrowRequest(BorrowRequestRow request)
        {
            if (!request.IsApproved)
            {
                ShowError(request.IsClaimed
                    ? "REQUEST #" + request.RequestId + " HAS ALREADY BEEN CLAIMED."
                    : "REQUEST #" + request.RequestId + " MUST BE APPROVED BEFORE IT CAN BE CLAIMED.");
                return;
            }

            string copiesText = request.Copies == 1 ? "1 COPY" : request.Copies + " COPIES";

            DialogResult answer = MessageBox.Show(
                "CONFIRM THAT " + request.StudentName.ToUpperInvariant() + " PERSONALLY RECEIVED " +
                copiesText + " OF \"" + request.BookTitle.ToUpperInvariant() + "\" IN THE LIBRARY?\n\n" +
                "THE REQUEST WILL BE MARKED AS CLAIMED.",
                "CONFIRM BOOK CLAIM",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (answer != DialogResult.Yes)
            {
                return;
            }

            try
            {
                _requestDirectory.Claim(request, LoggedInAccountId);
                LoadBorrowRequests();

                requestsStatusLabel.Text = "REQUEST #" + request.RequestId +
                    " CLAIMED. THE STUDENT NOW SEES CLAIMED IN MY REQUESTS.";
                requestsStatusLabel.ForeColor = SuccessText;
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                ShowError(ex.Message);
            }
            catch (MySqlException ex)
            {
                ShowError("THE REQUEST COULD NOT BE MARKED AS CLAIMED.\n" + ex.Message);
            }
        }

        // ------------------------------------------------------- EDIT / DELETE
        /// <summary>
        /// One click handler for the ACCESS, EDIT and DELETE icons of the tables. The
        /// column name decides the action; the row Tag carries the model object.
        /// </summary>
        private void GridActionCellClick(object? sender, DataGridViewCellEventArgs e)
        {
            DataGridView? grid = sender as DataGridView;

            if (grid == null || e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            string columnName = grid.Columns[e.ColumnIndex].Name;
            bool edit = string.Equals(columnName, EditColumnName, StringComparison.Ordinal);
            bool delete = string.Equals(columnName, DeleteColumnName, StringComparison.Ordinal);
            bool access = string.Equals(columnName, AccessColumnName, StringComparison.Ordinal);

            if (!edit && !delete && !access)
            {
                return;
            }

            if (!_canManageAccounts)
            {
                MessageBox.Show(
                    "ONLY LIBRARY STAFF ACCOUNTS CAN ACTIVATE, DEACTIVATE, EDIT OR DELETE RECORDS.",
                    "READ ONLY",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            DataGridViewRow row = grid.Rows[e.RowIndex];

            if (grid == booksGrid)
            {
                Book? book = row.Tag as Book;

                if (book == null)
                {
                    return;
                }

                if (edit)
                {
                    EditBook(book);
                }
                else
                {
                    DeleteBook(book);
                }

                return;
            }

            if (grid == attendanceGrid)
            {
                BorrowedBookGroup? group = row.Tag as BorrowedBookGroup;

                if (group == null)
                {
                    return;
                }

                if (edit)
                {
                    EditBorrowRecord(group);
                }
                else
                {
                    DeleteBorrowRecord(group);
                }

                return;
            }

            AccountRow? account = row.Tag as AccountRow;

            if (account == null)
            {
                return;
            }

            if (access)
            {
                ToggleAccountAccess(grid, account);
            }
            else if (edit)
            {
                EditAccount(grid, account);
            }
            else
            {
                DeleteAccount(grid, account);
            }
        }

        /// <summary>
        /// The ACCESS power icon flips one account between ACTIVE and INACTIVE, using
        /// the same confirm + reload path as the ACTIVATE / DEACTIVATE buttons.
        /// </summary>
        private void ToggleAccountAccess(DataGridView grid, AccountRow row)
        {
            if (row.IsActive)
            {
                if (row.AccountId == LoggedInAccountId)
                {
                    MessageBox.Show(
                        "YOU CANNOT DEACTIVATE THE ACCOUNT YOU ARE SIGNED IN WITH.",
                        "NOT ALLOWED",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                UpdateAccountStatus(
                    grid,
                    row,
                    "INACTIVE",
                    "DEACTIVATE \"" + row.Name.ToUpperInvariant() + "\"?\nTHEY WILL NO LONGER BE ABLE TO SIGN IN.");
            }
            else
            {
                UpdateAccountStatus(
                    grid,
                    row,
                    "ACTIVE",
                    "ACTIVATE \"" + row.Name.ToUpperInvariant() + "\"?\nTHEY WILL BE ABLE TO SIGN IN TO THE SYSTEM.");
            }
        }

        private void EditBook(Book book)
        {
            using (EditBookDialog dialog = new EditBookDialog(book))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    _catalog.UpdateBook(book, dialog.BookTitle, dialog.Author, dialog.Year, dialog.Quantity, dialog.CoverImage);
                    RefreshCatalogViews();
                    ApplyCatalogStatus();
                    ShowCatalogWarning();

                    booksStatusLabel.Text = "\"" + book.Title.ToUpperInvariant() + "\" WAS UPDATED.";
                    booksStatusLabel.ForeColor = SuccessText;
                }
                catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
                {
                    ShowError(ex.Message);
                }
                catch (MySqlException ex)
                {
                    ShowError("THE BOOK COULD NOT BE UPDATED.\n" + ex.Message);
                }
            }
        }

        private void DeleteBook(Book book)
        {
            DialogResult answer = MessageBox.Show(
                "DELETE \"" + book.Title.ToUpperInvariant() + "\" FROM THE CATALOG?\nTHIS CANNOT BE UNDONE.",
                "CONFIRM DELETE BOOK",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (answer != DialogResult.Yes)
            {
                return;
            }

            try
            {
                _catalog.DeleteBook(book);
                RefreshCatalogViews();
                ApplyCatalogStatus();
                ShowCatalogWarning();

                booksStatusLabel.Text = "\"" + book.Title.ToUpperInvariant() + "\" WAS DELETED.";
                booksStatusLabel.ForeColor = DangerText;
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                ShowError(ex.Message);
            }
            catch (MySqlException ex)
            {
                ShowError("THE BOOK COULD NOT BE DELETED.\n" + ex.Message);
            }
        }

        private void EditBorrowRecord(BorrowedBookGroup group)
        {
            using (EditBorrowRecordDialog dialog = new EditBorrowRecordDialog(group.FirstRecord, _catalog.Books))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                Book? book = dialog.SelectedBook;

                if (book == null)
                {
                    return;
                }

                try
                {
                    // The pop up collected the borrowers_table attributes; the model
                    // validates them before the two tables are updated. A grouped row
                    // edits every copy together so the line stays one group.
                    Borrower borrower = new Borrower(
                        dialog.BorrowerName,
                        dialog.BorrowerIdNumber,
                        dialog.BorrowerProgram,
                        dialog.BorrowerYearLevel,
                        dialog.BorrowerInstitute);

                    // The pop up already worked out the role, so the borrower row is
                    // updated with the role and (for STAFF) the office as well.
                    borrower.ApplyRoleDetails(dialog.BorrowerRole, dialog.BorrowerOffice);

                    foreach (BorrowRecord record in group.Records)
                    {
                        _catalog.UpdateBorrowRecord(record, book, borrower, dialog.DateBorrowed, dialog.ReturnedOn);
                    }

                    _catalog.SyncDaysBorrowed();
                    RefreshCatalogViews();
                    ApplyCatalogStatus();
                    ShowCatalogWarning();

                    attendanceStatusLabel.Text = group.CopyCount > 1
                        ? group.CopyCount + " COPY LOAN RECORDS WERE UPDATED."
                        : "BORROW RECORD #" + group.FirstRecord.BorrowId + " WAS UPDATED.";
                    attendanceStatusLabel.ForeColor = SuccessText;
                }
                catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
                {
                    ShowError(ex.Message);
                }
                catch (MySqlException ex)
                {
                    ShowError("THE BORROW RECORD COULD NOT BE UPDATED.\n" + ex.Message);
                }
            }
        }

        private void DeleteBorrowRecord(BorrowedBookGroup group)
        {
            string copies = group.CopyCount == 1
                ? "BORROW RECORD #" + group.FirstRecord.BorrowId
                : group.CopyCount + " COPY RECORDS (" + group.BorrowIdListText + ")";

            DialogResult answer = MessageBox.Show(
                "DELETE " + copies + " OF \"" + group.BookTitle.ToUpperInvariant() + "\" (" +
                group.BorrowerName.ToUpperInvariant() + ")?\n" +
                "THE " + (group.CopyCount == 1 ? "BOOK COUNTS" : "BOOKS COUNT") +
                " AS AVAILABLE AGAIN. THIS CANNOT BE UNDONE.",
                "CONFIRM DELETE RECORD",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (answer != DialogResult.Yes)
            {
                return;
            }

            try
            {
                foreach (BorrowRecord record in group.Records)
                {
                    _catalog.DeleteBorrowRecord(record);
                }

                _catalog.SyncDaysBorrowed();
                RefreshCatalogViews();
                ApplyCatalogStatus();
                ShowCatalogWarning();

                attendanceStatusLabel.Text = group.CopyCount > 1
                    ? group.CopyCount + " COPY LOAN RECORDS WERE DELETED."
                    : "BORROW RECORD #" + group.FirstRecord.BorrowId + " WAS DELETED.";
                attendanceStatusLabel.ForeColor = DangerText;
            }
            catch (MySqlException ex)
            {
                ShowError("THE BORROW RECORD COULD NOT BE DELETED.\n" + ex.Message);
            }
        }

        private void EditAccount(DataGridView grid, AccountRow row)
        {
            using (EditAccountDialog dialog = new EditAccountDialog(row))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    _accountDirectory.UpdateAccount(
                        row,
                        dialog.AccountName,
                        dialog.AccountUsername,
                        dialog.Role,
                        dialog.Status,
                        dialog.SchoolId,
                        dialog.Program,
                        dialog.YearLevel,
                        dialog.Institute,
                        dialog.ContactNumber,
                        dialog.Office,
                        row.HasProfile && row.IsStudent,
                        row.HasProfile && row.IsFaculty,
                        string.Equals(dialog.Role, "STAFF", StringComparison.OrdinalIgnoreCase),
                        string.Equals(dialog.Role, "LIBRARIAN", StringComparison.OrdinalIgnoreCase));

                    LoadAccounts();

                    Label label = StatusLabelFor(grid);
                    label.Text = dialog.AccountName.ToUpperInvariant() + " WAS UPDATED.";
                    label.ForeColor = SuccessText;
                }
                catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
                {
                    ShowError(ex.Message);
                }
                catch (MySqlException ex)
                {
                    ShowError("THE ACCOUNT COULD NOT BE UPDATED.\n" + ex.Message);
                }
            }
        }

        private void DeleteAccount(DataGridView grid, AccountRow row)
        {
            if (row.AccountId == LoggedInAccountId)
            {
                MessageBox.Show(
                    "YOU CANNOT DELETE THE ACCOUNT YOU ARE SIGNED IN WITH.",
                    "NOT ALLOWED",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            DialogResult answer = MessageBox.Show(
                "DELETE THE ACCOUNT OF \"" + row.Name.ToUpperInvariant() + "\"?\n" +
                "ITS ROLE AND PROFILE ROWS ARE REMOVED WITH IT. THIS CANNOT BE UNDONE.",
                "CONFIRM DELETE ACCOUNT",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (answer != DialogResult.Yes)
            {
                return;
            }

            try
            {
                _accountDirectory.DeleteAccount(row.AccountId);
                LoadAccounts();

                Label label = StatusLabelFor(grid);
                label.Text = row.Name.ToUpperInvariant() + " WAS DELETED.";
                label.ForeColor = DangerText;
            }
            catch (MySqlException ex)
            {
                ShowError("THE ACCOUNT COULD NOT BE DELETED.\n" + ex.Message);
            }
        }

        // ---------------------------------------------------------------- ACCOUNTS

        /// <summary>
        /// One read feeds all three account tabs: the librarian / staff tab, the
        /// faculty tab and the student tab. Each tab shows only its own role, so a
        /// student never appears next to the staff accounts.
        /// </summary>
        private void LoadAccounts()
        {
            try
            {
                _accounts = _accountDirectory.Load();
            }
            catch (MySqlException)
            {
                _accountsLoaded = false;
                _accounts.Clear();

                accountsGrid.Rows.Clear();
                staffGrid.Rows.Clear();
                facultyGrid.Rows.Clear();
                studentGrid.Rows.Clear();

                accountsStatusLabel.Text = "MYSQL OFFLINE \u2022 ACCOUNTS CANNOT BE LOADED";
                staffStatusLabel.Text = "MYSQL OFFLINE \u2022 ACCOUNTS CANNOT BE LOADED";
                facultyStatusLabel.Text = "MYSQL OFFLINE \u2022 ACCOUNTS CANNOT BE LOADED";
                studentStatusLabel.Text = "MYSQL OFFLINE \u2022 ACCOUNTS CANNOT BE LOADED";

                accountsStatusLabel.ForeColor = WarningText;
                staffStatusLabel.ForeColor = WarningText;
                facultyStatusLabel.ForeColor = WarningText;
                studentStatusLabel.ForeColor = WarningText;

                UpdateEmptyStates();
                return;
            }

            _accountsLoaded = true;
            FillAccountGrids();
        }

        /// <summary>
        /// Rebuilds the four account tables from the cached list, honouring the term
        /// typed in each tab's search field. Nothing is read from MySQL here, so the
        /// filter answers instantly while the staff member types.
        /// </summary>
        private void FillAccountGrids()
        {
            if (accountsGrid == null || !_accountsLoaded)
            {
                return;
            }

            accountsGrid.Rows.Clear();
            staffGrid.Rows.Clear();
            facultyGrid.Rows.Clear();
            studentGrid.Rows.Clear();

            string librarianTerm = Term(accountsSearch);
            string staffTerm = Term(staffSearch);
            string facultyTerm = Term(facultySearch);
            string studentTerm = Term(studentSearch);

            accountsGrid.SuspendLayout();
            staffGrid.SuspendLayout();
            facultyGrid.SuspendLayout();
            studentGrid.SuspendLayout();

            int librarianCount = 0;
            int staffCount = 0;
            int facultyCount = 0;
            int studentCount = 0;

            int librarianTotal = 0;
            int staffTotal = 0;
            int facultyTotal = 0;
            int studentTotal = 0;

            foreach (AccountRow row in _accounts)
            {
                if (row.IsStudent)
                {
                    studentTotal++;

                    if (!MatchesTerm(studentTerm, row.Name, row.Username, row.SchoolId, row.Program,
                                     row.YearLevel, row.Institute, row.ContactNumber, row.Status))
                    {
                        continue;
                    }

                    int index = studentGrid.Rows.Add(
                        row.AccountId,
                        row.Name,
                        row.Username,
                        row.SchoolId,
                        row.Program,
                        row.YearLevel,
                        row.Institute,
                        row.ContactNumber,
                        row.Status,
                        AccessGlyph,
                        EditGlyph,
                        DeleteGlyph);

                    studentGrid.Rows[index].Tag = row;
                    studentCount++;
                }
                else if (row.IsFaculty)
                {
                    facultyTotal++;

                    if (!MatchesTerm(facultyTerm, row.Name, row.Username, row.Institute, row.Status))
                    {
                        continue;
                    }

                    int index = facultyGrid.Rows.Add(
                        row.AccountId,
                        row.Name,
                        row.Username,
                        row.Institute,
                        row.Status,
                        AccessGlyph,
                        EditGlyph,
                        DeleteGlyph);

                    facultyGrid.Rows[index].Tag = row;
                    facultyCount++;
                }
                else if (row.IsLibrarian)
                {
                    librarianTotal++;

                    if (!MatchesTerm(librarianTerm, row.Name, row.Username, row.Status, row.Role))
                    {
                        continue;
                    }

                    int index = accountsGrid.Rows.Add(
                        row.AccountId,
                        row.Name,
                        row.Username,
                        "\u2022\u2022\u2022\u2022\u2022\u2022",
                        row.Status,
                        row.Role,
                        AccessGlyph,
                        EditGlyph,
                        DeleteGlyph);

                    accountsGrid.Rows[index].Tag = row;
                    librarianCount++;
                }
                else
                {
                    staffTotal++;

                    if (!MatchesTerm(staffTerm, row.Name, row.Username, row.Office, row.Status, row.Role))
                    {
                        continue;
                    }

                    int index = staffGrid.Rows.Add(
                        row.AccountId,
                        row.Name,
                        row.Username,
                        "\u2022\u2022\u2022\u2022\u2022\u2022",
                        row.Office,
                        row.Status,
                        row.Role,
                        AccessGlyph,
                        EditGlyph,
                        DeleteGlyph);

                    staffGrid.Rows[index].Tag = row;
                    staffCount++;
                }
            }

            ApplyAccountStatusColors(accountsGrid, accountStatusColumn);
            ApplyAccountStatusColors(staffGrid, staffStatusColumn);
            ApplyAccountStatusColors(facultyGrid, facultyStatusColumn);
            ApplyAccountStatusColors(studentGrid, studentStatusColumn);

            accountsStatusLabel.Text = AccountSummary(librarianCount, librarianTotal, "LIBRARIAN ACCOUNT", librarianTerm);
            staffStatusLabel.Text = AccountSummary(staffCount, staffTotal, "STAFF ACCOUNT", staffTerm);
            facultyStatusLabel.Text = AccountSummary(facultyCount, facultyTotal, "FACULTY ACCOUNT", facultyTerm);
            studentStatusLabel.Text = AccountSummary(studentCount, studentTotal, "STUDENT ACCOUNT", studentTerm);

            accountsStatusLabel.ForeColor = librarianCount == 0 ? WarningText : MutedText;
            staffStatusLabel.ForeColor = staffCount == 0 ? WarningText : MutedText;
            facultyStatusLabel.ForeColor = facultyCount == 0 ? WarningText : MutedText;
            studentStatusLabel.ForeColor = studentCount == 0 ? WarningText : MutedText;

            SetEmptyText(accountsEmptyLabel, librarianTerm,
                "NO LIBRARIAN ACCOUNT MATCHES THE SEARCH.",
                "NO LIBRARIAN ACCOUNTS HAVE BEEN REGISTERED YET.");
            SetEmptyText(staffEmptyLabel, staffTerm,
                "NO STAFF ACCOUNT MATCHES THE SEARCH.",
                "NO STAFF ACCOUNTS HAVE BEEN REGISTERED YET.");
            SetEmptyText(facultyEmptyLabel, facultyTerm,
                "NO FACULTY ACCOUNT MATCHES THE SEARCH.",
                "NO FACULTY ACCOUNTS HAVE BEEN REGISTERED YET.");
            SetEmptyText(studentEmptyLabel, studentTerm,
                "NO STUDENT ACCOUNT MATCHES THE SEARCH.",
                "NO STUDENT ACCOUNTS HAVE BEEN REGISTERED YET.");

            ClearGridSelection(accountsGrid);
            ClearGridSelection(staffGrid);
            ClearGridSelection(facultyGrid);
            ClearGridSelection(studentGrid);

            accountsGrid.ResumeLayout();
            staffGrid.ResumeLayout();
            facultyGrid.ResumeLayout();
            studentGrid.ResumeLayout();

            UpdateEmptyStates();
        }

        private string AccountSummary(int shown, int total, string label, string term)
        {
            if (total == 0)
            {
                return "NO " + label + "S HAVE BEEN REGISTERED YET.";
            }

            if (shown == 0)
            {
                return "NO " + label + "S MATCH \"" + term + "\" \u2022 PRESS THE X IN THE SEARCH BOX.";
            }

            string countText = shown.ToString(CultureInfo.InvariantCulture);

            if (shown != total)
            {
                countText += " OF " + total.ToString(CultureInfo.InvariantCulture);
            }

            countText += " " + label + "(S) ON FILE.";

            return _canManageAccounts
                ? countText + " CLICK THE POWER ICON ON A ROW TO ACTIVATE OR DEACTIVATE."
                : countText + " READ ONLY \u2022 ONLY LIBRARY STAFF CAN MANAGE ACCOUNTS.";
        }

        private static void ApplyAccountStatusColors(DataGridView grid, DataGridViewColumn statusColumn)
        {
            foreach (DataGridViewRow row in grid.Rows)
            {
                DataGridViewCell statusCell = row.Cells[statusColumn.Index];
                string status = Convert.ToString(statusCell.Value, CultureInfo.InvariantCulture) ?? string.Empty;

                if (string.Equals(status, "ACTIVE", StringComparison.OrdinalIgnoreCase))
                {
                    statusCell.Style.ForeColor = SuccessText;
                }
                else if (string.Equals(status, "REJECTED", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(status, "INACTIVE", StringComparison.OrdinalIgnoreCase))
                {
                    statusCell.Style.ForeColor = DangerText;
                }
                else
                {
                    statusCell.Style.ForeColor = Color.FromArgb(180, 83, 9);
                }

                statusCell.Style.Font = StatusCellFont;
                statusCell.ToolTipText =
                    "Double-click this cell (or the power icon) to activate or deactivate this account.";

                if (grid.Columns.Contains(AccessColumnName))
                {
                    AccountRow? account = row.Tag as AccountRow;
                    bool active = account != null && account.IsActive;
                    DataGridViewCell accessCell = row.Cells[AccessColumnName];

                    accessCell.Style.ForeColor = active ? WarningText : Color.FromArgb(4, 120, 87);
                    accessCell.Style.Font = IconCellFont;
                    accessCell.ToolTipText = active
                        ? "Click to deactivate this account."
                        : "Click to activate this account.";
                }
            }
        }

        private static void ClearGridSelection(DataGridView grid)
        {
            grid.ClearSelection();
            grid.CurrentCell = null;
        }

        // ---------------------------------------------------------------- ACTIONS

        private void addBookButton_Click(object? sender, EventArgs e)
        {
            using (AddBookDialog dialog = new AddBookDialog())
            {
                if (dialog.ShowDialog(this) != DialogResult.OK || dialog.CreatedBook == null)
                {
                    return;
                }

                try
                {
                    _catalog.AddBook(dialog.CreatedBook);
                    RefreshCatalogViews();
                    ApplyCatalogStatus();
                    ShowCatalogWarning();
                }
                catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
                {
                    ShowError(ex.Message);
                }
                catch (MySqlException ex)
                {
                    ShowError(ex.Message);
                }
            }
        }

        private void borrowBookButton_Click(object? sender, EventArgs e)
        {
            List<Book> available = new List<Book>();
            foreach (Book book in _catalog.Books)
            {
                if (book.IsAvailable)
                {
                    available.Add(book);
                }
            }

            if (available.Count == 0)
            {
                MessageBox.Show(
                    "THERE ARE NO AVAILABLE BOOKS TO BORROW.\nADD A BOOK TO THE CATALOG FIRST, OR SET A LOAN'S RETURNED DATE IN THE BORROWED BOOKS TAB.",
                    "NO BOOKS AVAILABLE",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            using (BorrowBookDialog dialog = new BorrowBookDialog(available, _catalog.AvailableCopyCount))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK || dialog.SelectedBook == null)
                {
                    return;
                }

                try
                {
                    // The pop up collected the borrowers_table attributes; the model
                    // validates them before LibraryCatalog writes the two tables.
                    Borrower borrower = new Borrower(
                        dialog.BorrowerLastName,
                        dialog.BorrowerFirstName,
                        dialog.BorrowerMiddleName,
                        dialog.BorrowerIdNumber,
                        dialog.BorrowerProgram,
                        dialog.BorrowerYearLevel,
                        dialog.BorrowerInstitute,
                        dialog.BorrowerRole,
                        dialog.BorrowerOffice,
                        dialog.BorrowerContactNumber);

                    // One borrow row per copy, so every copy keeps its own RETURNED
                    // date and the AVAILABLE COPY count follows the loans.
                    int copies = Math.Max(1, dialog.CopiesToBorrow);

                    for (int copy = 0; copy < copies; copy++)
                    {
                        _catalog.RecordBorrow(dialog.SelectedBook, borrower, dialog.DateBorrowed);
                    }

                    _catalog.SyncDaysBorrowed();
                    RefreshCatalogViews();
                    ApplyCatalogStatus();
                    ShowCatalogWarning();
                }
                catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
                {
                    ShowError(ex.Message);
                }
                catch (MySqlException ex)
                {
                    ShowError(ex.Message);
                }
            }
        }

        private void generateReportButton_Click(object? sender, EventArgs e)
        {
            ExportBooksPdf();
        }

        /// <summary>
        /// BOOKS tab: writes the catalog to a PDF that follows the Learning Resource
        /// Center form instead of opening the old preview window.
        /// </summary>
        private void ExportBooksPdf()
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "PDF file (*.pdf)|*.pdf";
                dialog.FileName = "books-report-" +
                                  DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".pdf";
                dialog.Title = "EXPORT BOOKS AS PDF";
                dialog.OverwritePrompt = true;

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    BooksPdfExporter.Export(_catalog.Books, dialog.FileName);

                    DialogChrome.ShowAlert(
                        "BOOKS EXPORTED TO:\n" + dialog.FileName,
                        "PDF SAVED",
                        MessageBoxIcon.Information);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    DialogChrome.ShowAlert(
                        "THE PDF COULD NOT BE SAVED.\n" + ex.Message,
                        "EXPORT FAILED",
                        MessageBoxIcon.Error);
                }
            }
        }

        private void attendanceReportButton_Click(object? sender, EventArgs e)
        {
            ExportBorrowedBooksPdf();
        }

        /// <summary>
        /// BORROWED BOOKS tab: writes the borrow groups to a PDF that follows the
        /// Learning Resource Center form instead of opening the old preview window.
        /// </summary>
        private void ExportBorrowedBooksPdf()
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "PDF file (*.pdf)|*.pdf";
                dialog.FileName = "borrowed-books-" +
                                  DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".pdf";
                dialog.Title = "EXPORT BORROWED BOOKS AS PDF";
                dialog.OverwritePrompt = true;

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    List<BorrowedBookGroup> groups = BorrowedBookGroup.BuildGroups(_catalog.BorrowRecords);
                    BorrowedBooksPdfExporter.Export(groups, dialog.FileName);

                    DialogChrome.ShowAlert(
                        "BORROWED BOOKS EXPORTED TO:\n" + dialog.FileName,
                        "PDF SAVED",
                        MessageBoxIcon.Information);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    DialogChrome.ShowAlert(
                        "THE PDF COULD NOT BE SAVED.\n" + ex.Message,
                        "EXPORT FAILED",
                        MessageBoxIcon.Error);
                }
            }
        }

        private void accountsRefreshButton_Click(object? sender, EventArgs e)
        {
            LoadAccounts();
        }

        private void requestsRefreshButton_Click(object? sender, EventArgs e)
        {
            LoadBorrowRequests();
        }

        private void staffRefreshButton_Click(object? sender, EventArgs e)
        {
            LoadAccounts();
        }

        private void facultyRefreshButton_Click(object? sender, EventArgs e)
        {
            LoadAccounts();
        }

        private void studentRefreshButton_Click(object? sender, EventArgs e)
        {
            LoadAccounts();
        }

        // ------------------------------------------------- ACCOUNT MANAGEMENT
        //
        // Activate / deactivate is handled per row by the ACCESS power icon
        // (ToggleAccountAccess). The old ACTIVATE / DEACTIVATE buttons were removed
        // so the staff acts on the exact record they see instead of the selection.

        // Each tab has its own status line, so the result is reported where the
        // staff member clicked instead of jumping to another tab.
        private Label StatusLabelFor(DataGridView grid)
        {
            if (grid == facultyGrid)
            {
                return facultyStatusLabel;
            }

            if (grid == studentGrid)
            {
                return studentStatusLabel;
            }

            if (grid == staffGrid)
            {
                return staffStatusLabel;
            }

            return accountsStatusLabel;
        }

        private void UpdateAccountStatus(DataGridView grid, AccountRow row, string status, string question)
        {
            DialogResult answer = MessageBox.Show(
                question,
                "CONFIRM ACCOUNT UPDATE",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (answer != DialogResult.Yes)
            {
                return;
            }

            try
            {
                _accountDirectory.SetStatus(row.AccountId, status);
                LoadAccounts();

                Label label = StatusLabelFor(grid);
                label.Text = row.Name.ToUpperInvariant() + " IS NOW " + status + ".";
                label.ForeColor = string.Equals(status, "ACTIVE", StringComparison.OrdinalIgnoreCase)
                    ? SuccessText
                    : DangerText;
            }
            catch (MySqlException ex)
            {
                ShowError("THE ACCOUNT COULD NOT BE UPDATED.\n" + ex.Message);
            }
            catch (ArgumentException ex)
            {
                ShowError(ex.Message);
            }
        }

        private void tabControl1_SelectedIndexChanged(object? sender, EventArgs e)
        {
            TabPage? page = tabControl1.SelectedTab;

            if ((page == tabPage2 || page == staffTab || page == tabPage5 || page == tabPage6) && !_accountsLoaded)
            {
                LoadAccounts();
            }

            if (page == tabPage3 || page == tabPage4)
            {
                RefreshCatalogViews();
                ApplyCatalogStatus();
            }

            if (page == requestsTab)
            {
                LoadBorrowRequests();
            }

            if (page == tabPage7)
            {
                LoadAttendanceLog();
            }

            if (page == analyticsTab)
            {
                LoadAnalytics();
            }

            UpdateEmptyStates();
        }

        // ---------------------------------------------------------------- PROFILE

        /// <summary>
        /// Fills the PROFILE tab with the signed in person's own record. The values
        /// arrive through the foreign keys (program_table / institute_table), so the
        /// card shows exactly what the signup form stored.
        /// </summary>
        private void LoadProfileRecord()
        {
            try
            {
                AccountRow? row = _accountDirectory.LoadProfile(LoggedInAccountId);

                if (row == null)
                {
                    return;
                }

                _profileRow = row;
                ApplyProfileRecord(row);
            }
            catch (MySqlException)
            {
                // The header still shows the values passed by the login form.
            }
        }

        private void ApplyProfileRecord(AccountRow row)
        {
            valueDateCreated.Text = row.DateCreated;
            ApplyProfileAvatar(row);

            bool isStudent = row.IsStudent;
            bool isFaculty = row.IsFaculty;

            // The two middle rows show the profile facts the account actually has:
            // a student has a program, a staff member an office, and the other
            // roles show the contact number stored in their profile row.
            if (isStudent)
            {
                captionProgramOffice.Text = "PROGRAM";
                valueProgramOffice.Text = ProfileValue(row.Program);
            }
            else if (row.IsStaff)
            {
                captionProgramOffice.Text = "OFFICE";
                valueProgramOffice.Text = ProfileValue(row.Office);
            }
            else
            {
                captionProgramOffice.Text = "CONTACT NUMBER";
                valueProgramOffice.Text = ProfileValue(row.ContactNumber);
            }

            if (row.IsStaff)
            {
                captionInstitute.Text = "CONTACT NUMBER";
                valueInstitute.Text = ProfileValue(row.ContactNumber);
            }
            else
            {
                captionInstitute.Text = "INSTITUTE";
                valueInstitute.Text = ProfileValue(row.Institute);
            }
        }

        // AccountRow uses "-" for an empty value; the profile card shows a dash
        // instead, so every row stays aligned and readable.
        private static string ProfileValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value) || value == "-" ? "\u2014" : value.Trim();
        }

        private void ShowError(string message)
        {
            MessageBox.Show(
                (message ?? string.Empty).ToUpperInvariant(),
                "LIBRARY SYSTEM",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        // The header keeps the account name and the role badge aligned to the
        // right edge of the sign out button, at any window size.
        private void LayoutHeaderCluster()
        {
            if (gradientPanel1 == null || logoutButton == null || accountNameLabel == null || headerRoleBadge == null)
            {
                return;
            }

            int margin = ScaleValue(30);
            int gap = ScaleValue(20);

            int rightEdge = gradientPanel1.ClientSize.Width - margin - logoutButton.Width - gap;

            headerRoleBadge.Left = rightEdge - headerRoleBadge.Width;
            headerRoleBadge.Top = ScaleValue(56);

            accountNameLabel.Left = rightEdge - accountNameLabel.Width;
            accountNameLabel.Top = ScaleValue(24);
        }

        // PROFILE HEADER: the title and the subtitle span the page; the two profile
        // actions now sit inside the identity card under the picture.
        private void LayoutProfileHeader()
        {
            if (profileHeader == null || profileTitleLabel == null)
            {
                return;
            }

            profileSubtitleLabel.Width = Math.Max(160, profileHeader.ClientSize.Width - 8);
        }

        // IDENTITY CARD: the picture, name and role badge sit above CHANGE PASSWORD
        // and EDIT MY INFO, all centred as one block inside the card.
        private void LayoutProfileCard()
        {
            if (identityCard == null)
            {
                return;
            }

            int width = identityCard.ClientSize.Width;
            int height = identityCard.ClientSize.Height;

            if (width <= 0 || height <= 0)
            {
                return;
            }

            const int buttonHeight = 44;
            int avatarSize = Math.Clamp(width * 2 / 5, 120, 170);
            int nameHeight = 34;

            int blockHeight = avatarSize + 4 + nameHeight + 4 + profileRoleBadge.Height +
                              12 + 1 + 12 + buttonHeight + 10 + buttonHeight;
            int top = Math.Max(24, (height - blockHeight) / 2);

            avatarBadge.SetBounds((width - avatarSize) / 2, top, avatarSize, avatarSize);

            profileName.SetBounds(18, avatarBadge.Bottom + 4, Math.Max(80, width - 36), nameHeight);

            profileRoleBadge.Top = profileName.Bottom + 4;
            profileRoleBadge.Left = (width - profileRoleBadge.Width) / 2;

            identityDivider.SetBounds(40, profileRoleBadge.Bottom + 12, Math.Max(40, width - 80), 1);

            int buttonWidth = Math.Min(240, Math.Max(140, width - 120));
            int buttonLeft = (width - buttonWidth) / 2;

            changePasswordButton.SetBounds(
                buttonLeft,
                identityDivider.Bottom + 12,
                buttonWidth,
                buttonHeight);

            editProfileButton.SetBounds(
                buttonLeft,
                changePasswordButton.Bottom + 10,
                buttonWidth,
                buttonHeight);

            FitProfileName();
        }

        // ACCOUNT DETAILS: the six read only rows share the height of the card, so a
        // taller window spreads them instead of leaving a gap above the footer.
        private void LayoutDetailsCard()
        {
            if (detailsCard == null || profileFooter == null)
            {
                return;
            }

            int width = detailsCard.ClientSize.Width;
            int height = detailsCard.ClientSize.Height;

            if (width <= 0 || height <= 0)
            {
                return;
            }

            int dividerWidth = Math.Max(40, width - 68);
            detailsTopDivider.SetBounds(34, 92, dividerWidth, 1);

            profileFooter.Left = 34;
            profileFooter.Top = height - 28;
            profileFooter.MaximumSize = new Size(dividerWidth, 0);

            Label[] captions =
            {
                captionFullName,
                captionRole,
                captionStatus,
                captionProgramOffice,
                captionInstitute,
                captionDateCreated
            };

            Label[] values =
            {
                valueFullName,
                valueRole,
                valueStatus,
                valueProgramOffice,
                valueInstitute,
                valueDateCreated
            };

            Panel[] dividers =
            {
                dividerRow1,
                dividerRow2,
                dividerRow3,
                dividerRow4,
                dividerRow5,
                dividerRow6
            };

            int rowTop = 92;
            int rowsBottom = profileFooter.Top - 8;
            int step = Math.Max(52, (rowsBottom - rowTop) / captions.Length);

            int captionWidth = Math.Min(300, Math.Max(120, width / 2 - 40));
            int valueLeft = 340;
            int valueWidth = Math.Max(120, width - valueLeft - 34);

            for (int index = 0; index < captions.Length; index++)
            {
                captions[index].SetBounds(
                    34,
                    rowTop + Math.Max(0, (step - 34) / 2),
                    captionWidth,
                    34);

                values[index].SetBounds(
                    valueLeft,
                    rowTop + Math.Max(0, (step - 44) / 2),
                    valueWidth,
                    44);

                if (index < dividers.Length)
                {
                    dividers[index].SetBounds(34, rowTop + step, dividerWidth, 1);
                }

                rowTop += step;
            }
        }

        private void ShowChangePasswordDialog()
        {
            using ChangePasswordDialog dialog =
                new ChangePasswordDialog(LoggedInAccountId, LoggedInUsername, LoggedInRole);
            dialog.ShowDialog(this);
        }

        /// <summary>
        /// Shrinks the profile name until the whole name fits on one line, so a
        /// long name is shown in full instead of ending in "...".
        /// </summary>
        private void FitProfileName()
        {
            if (profileName == null)
            {
                return;
            }

            string name = profileName.Text ?? string.Empty;
            int available = Math.Max(60, profileName.Width - 12);
            float size = ProfileNameMaxSize;

            while (size > ProfileNameMinSize)
            {
                using (var probe = new Font("Segoe UI", size, FontStyle.Bold))
                {
                    Size measured = TextRenderer.MeasureText(
                        name,
                        probe,
                        Size.Empty,
                        TextFormatFlags.NoPadding);

                    if (measured.Width <= available)
                    {
                        break;
                    }
                }

                size -= 0.5f;
            }

            if (Math.Abs(size - _profileNameSize) < 0.01f)
            {
                return;
            }

            Font? previous = _profileNameFont;
            _profileNameFont = new Font("Segoe UI", size, FontStyle.Bold);
            _profileNameSize = size;
            profileName.Font = _profileNameFont;
            previous?.Dispose();
        }

        /// <summary>Shows the portrait saved through EDIT MY INFO, or the initials.</summary>
        private void ApplyProfileAvatar(AccountRow row)
        {
            if (avatarBadge == null)
            {
                return;
            }

            Image? next = ProfilePicture.Load(row.ProfilePicture);
            Image? previous = _avatarImage;
            _avatarImage = next;
            avatarBadge.Picture = next;
            previous?.Dispose();
            avatarBadge.Invalidate();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            _avatarImage?.Dispose();
            _avatarImage = null;

            _profileNameFont?.Dispose();
            _profileNameFont = null;
        }

        // SELF SERVICE: the signed in librarian may update every personal detail of
        // the account (name, username and the whole librarian profile) without
        // leaving the dashboard. Role, status and date created stay read only.
        private void ShowEditProfileDialog()
        {
            if (LoggedInAccountId <= 0)
            {
                ShowError("YOUR PROFILE COULD NOT BE LOADED. SIGN IN AGAIN AND TRY ONCE MORE.");
                return;
            }

            AccountRow? row = _profileRow;

            if (row == null)
            {
                try
                {
                    row = _accountDirectory.LoadProfile(LoggedInAccountId);
                }
                catch (MySqlException ex)
                {
                    ShowError("YOUR PROFILE COULD NOT BE LOADED.\n" + ex.Message);
                    return;
                }
            }

            if (row == null)
            {
                ShowError("YOUR PROFILE COULD NOT BE LOADED.");
                return;
            }

            using (MyProfileDialog dialog = new MyProfileDialog(row))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    _accountDirectory.UpdateOwnProfile(
                        row,
                        dialog.FullName,
                        dialog.Username,
                        dialog.SchoolId,
                        dialog.Program,
                        dialog.YearLevel,
                        dialog.Institute,
                        dialog.ContactNumber,
                        dialog.Address,
                        dialog.Office,
                        dialog.ProfilePictureChanged ? dialog.ProfilePicture : null,
                        dialog.ProfilePictureChanged);

                    // Keep the header, the sign in name and the profile card in
                    // step with the values that were just saved.
                    LoggedInFullName = dialog.FullName;
                    LoggedInUsername = dialog.Username;

                    ApplyAccountDetails();
                    LoadProfileRecord();

                    Authentication.ShowMessage(
                        "YOUR INFORMATION HAS BEEN UPDATED.",
                        "PROFILE UPDATED",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
                {
                    ShowError(ex.Message);
                }
                catch (MySqlException ex)
                {
                    ShowError("YOUR INFORMATION COULD NOT BE UPDATED.\n" + ex.Message);
                }
            }
        }

        private float UiScale
        {
            get
            {
                float design = AutoScaleDimensions.Height;
                float current = CurrentAutoScaleDimensions.Height;
                return design <= 0f || current <= 0f ? 1f : current / design;
            }
        }

        private int ScaleValue(int value)
        {
            return (int)Math.Round(value * UiScale);
        }

        private static string BuildInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return "?";
            }

            string[] parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
            {
                return parts[0].Substring(0, 1).ToUpperInvariant();
            }

            return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutHeaderCluster();
            LayoutProfileHeader();
            LayoutProfileCard();
            LayoutDetailsCard();
            LayoutRecordSearches();
        }

        // --------------------------------------------------------------- ANALYTICS

        /// <summary>
        /// Fills the ANALYTICS tab declared in the designer. The page is visible in
        /// Visual Studio's View Designer; the header, the KPI tiles and the smooth
        /// line charts are created here because they are custom drawn.
        /// </summary>
        private void BuildAnalyticsTab()
        {
            // Fallback for a host that did not run the designer: create the page when
            // it is missing. Normal runs already have analyticsTab from InitializeComponent.
            if (analyticsTab == null)
            {
                analyticsTab = new TabPage
                {
                    BackColor = Color.White,
                    Padding = new Padding(3),
                    Text = "ANALYTICS",
                    UseVisualStyleBackColor = false
                };

                tabControl1.TabPages.Add(analyticsTab);
            }

            analyticsHead = new Panel
            {
                BackColor = Color.White,
                Dock = DockStyle.Top,
                Height = ScaleValue(88)
            };

            analyticsTitle = new Label
            {
                AutoSize = false,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(8, 29),
                Size = new Size(520, 30),
                Text = "LIBRARY ANALYTICS",
                TextAlign = ContentAlignment.MiddleLeft
            };

            analyticsStatus = new Label
            {
                AutoEllipsis = true,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 8.25F, FontStyle.Bold),
                ForeColor = MutedText,
                Location = new Point(8, 60),
                Size = new Size(500, 18),
                Text = string.Empty,
                TextAlign = ContentAlignment.MiddleLeft,
                Visible = false
            };

            analyticsRefreshButton = new RoundedButton
            {
                BackColor = Color.Transparent,
                CornerRadius = 11,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.White,
                HoverColor = Color.FromArgb(21, 128, 61),
                NormalColor = Color.FromArgb(22, 163, 74),
                PressedColor = Color.FromArgb(20, 83, 45),
                Size = new Size(130, 40),
                Text = "REFRESH",
                UseVisualStyleBackColor = false
            };
            analyticsRefreshButton.FlatAppearance.BorderSize = 0;
            analyticsRefreshButton.Click += (s, e) => LoadAnalytics();

            // A hairline under the header separates the title block from the cards.
            Panel headDivider = new Panel
            {
                BackColor = Color.FromArgb(226, 232, 240),
                Dock = DockStyle.Bottom,
                Height = 1
            };

            analyticsHead.Controls.Add(analyticsTitle);
            analyticsHead.Controls.Add(analyticsStatus);
            analyticsHead.Controls.Add(analyticsRefreshButton);
            analyticsHead.Controls.Add(headDivider);
            analyticsHead.Resize += (s, e) => LayoutAnalyticsHead();
            LayoutAnalyticsHead();

            // The page scrolls: the KPI strip, the three usage charts and the
            // evaluation feedback block need more height than a laptop screen.
            Panel analyticsScroll = new Panel
            {
                AutoScroll = true,
                BackColor = Color.White,
                Dock = DockStyle.Fill
            };

            const int KpiHeight = 100;
            const int ChartHeight = 306;
            const int EvaluationHeadHeight = 74;
            const int FeedbackHeight = 370;
            const int StackGap = 12;

            TableLayoutPanel body = new TableLayoutPanel
            {
                AutoSize = false,
                BackColor = Color.White,
                ColumnCount = 1,
                Dock = DockStyle.Top,
                Height = 6 + KpiHeight +
                         ChartHeight + StackGap +
                         ChartHeight + StackGap +
                         ChartHeight + StackGap +
                         EvaluationHeadHeight + FeedbackHeight + 16,
                Padding = new Padding(0, 6, 0, 16),
                RowCount = 6
            };

            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, KpiHeight));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, ChartHeight + StackGap));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, ChartHeight + StackGap));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, ChartHeight + StackGap));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, EvaluationHeadHeight));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, FeedbackHeight));

            // ------------------------------------------------- EVALUATION KPIs
            // One summary card per respondent group, so the librarian can see at a
            // glance whether students, faculty or staff actually answer the form.
            TableLayoutPanel kpiStrip = new TableLayoutPanel
            {
                BackColor = Color.White,
                ColumnCount = 4,
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                RowCount = 1
            };

            for (int index = 0; index < 4; index++)
            {
                kpiStrip.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            }

            evalTotalCard = CreateEvaluationCard("EVALUATION RESPONSES", HeaderGreen);
            evalStudentCard = CreateEvaluationCard("STUDENT RESPONDENTS", Color.FromArgb(22, 163, 74));
            evalFacultyCard = CreateEvaluationCard("FACULTY RESPONDENTS", Color.FromArgb(37, 99, 235));
            evalStaffCard = CreateEvaluationCard("STAFF RESPONDENTS", Color.FromArgb(194, 65, 12));

            // The last card drops its right margin so the strip ends flush with the
            // chart cards below it.
            evalStaffCard.Margin = new Padding(0);

            kpiStrip.Controls.Add(evalTotalCard, 0, 0);
            kpiStrip.Controls.Add(evalStudentCard, 1, 0);
            kpiStrip.Controls.Add(evalFacultyCard, 2, 0);
            kpiStrip.Controls.Add(evalStaffCard, 3, 0);

            // ------------------------------------------- USAGE CHARTS
            // The visits curve, the borrower curve and the book bars share the page
            // width and one height, so they line up with each other and with the
            // cards below.
            programVisitsChart = new LineChart
            {
                BackColor = Color.White,
                EmptyText = "NO PROGRAMS HAVE BEEN REGISTERED YET.",
                LineColor = Color.FromArgb(22, 163, 74),
                Subtitle = "STUDENT VISITS TO THE LIBRARY, BY PROGRAM",
                Title = "LIBRARY VISITS BY PROGRAM"
            };

            topBorrowersChart = new LineChart
            {
                BackColor = Color.White,
                EmptyText = "NO BORROW RECORDS HAVE BEEN RECORDED YET.",
                HoverImageProvider = LoadBorrowerAvatar,
                LineColor = Color.FromArgb(13, 148, 136),
                Subtitle = "THE STUDENTS WHO BORROWED THE MOST BOOKS",
                Title = "TOP LIBRARY BOOK BORROWERS"
            };

            // ------------------------------------------ MOST BORROWED BOOKS
            // A line chart like the two above, plus a hover preview: pointing at a
            // book's point fades in its cover, so the librarian can recognise the
            // title without reading the angled axis label.
            mostBorrowedBooksChart = new LineChart
            {
                BackColor = Color.White,
                EmptyText = "NO BOOK HAS BEEN BORROWED YET.",
                HoverImageProvider = LoadBookCover,
                LineColor = Color.FromArgb(37, 99, 235),
                Subtitle = "THE BOOKS BORROWED THE MOST TIMES",
                Title = "MOST BORROWED BOOKS"
            };

            // --------------------------------------------- EVALUATION BLOCK HEAD
            Panel evaluationHead = new Panel
            {
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 4)
            };

            Label evaluationTitle = new Label
            {
                AutoSize = false,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
                ForeColor = HeaderGreen,
                Location = new Point(8, 8),
                Size = new Size(600, 24),
                Text = "EVALUATION FORM FEEDBACK",
                TextAlign = ContentAlignment.MiddleLeft
            };

            // The second line of the evaluation header is the live semester state:
            // which period is selected, whether it is open and how many answers it
            // holds. The three controls on the right run that period.
            evaluationPeriodStatus = new Label
            {
                AutoEllipsis = true,
                AutoSize = false,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 8.25F, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(8, 34),
                Size = new Size(520, 20),
                Text = "NO EVALUATION PERIOD HAS BEEN OPENED YET.",
                TextAlign = ContentAlignment.MiddleLeft
            };

            evaluationPeriodBox = new ComboBox
            {
                BackColor = Color.FromArgb(248, 250, 252),
                DrawMode = DrawMode.OwnerDrawFixed,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DropDownWidth = 400,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.25F),
                ForeColor = Color.FromArgb(30, 41, 59),
                ItemHeight = 24,
                Size = new Size(400, 32)
            };
            evaluationPeriodBox.DrawItem += EvaluationPeriodBoxDrawItem;
            evaluationPeriodBox.SelectedIndexChanged += (s, e) => EvaluationPeriodChanged();

            evaluationPeriodButton = new RoundedButton
            {
                BackColor = Color.Transparent,
                CornerRadius = 10,
                DisabledTextColor = Color.FromArgb(100, 116, 139),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.25F, FontStyle.Bold),
                ForeColor = Color.White,
                HoverColor = Color.FromArgb(21, 128, 61),
                NormalColor = Color.FromArgb(22, 163, 74),
                PressedColor = Color.FromArgb(20, 83, 45),
                Size = new Size(158, 34),
                Text = "OPEN EVALUATION",
                UseVisualStyleBackColor = false
            };
            evaluationPeriodButton.FlatAppearance.BorderSize = 0;
            evaluationPeriodButton.Click += (s, e) => ToggleEvaluationPeriod();

            evaluationNewPeriodButton = new RoundedButton
            {
                BackColor = Color.Transparent,
                CornerRadius = 10,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.25F, FontStyle.Bold),
                ForeColor = Color.White,
                HoverColor = Color.FromArgb(51, 65, 85),
                NormalColor = Color.FromArgb(71, 85, 105),
                PressedColor = Color.FromArgb(30, 41, 59),
                Size = new Size(136, 34),
                Text = "ADD SEMESTER",
                UseVisualStyleBackColor = false
            };
            evaluationNewPeriodButton.FlatAppearance.BorderSize = 0;
            evaluationNewPeriodButton.Click += (s, e) => ShowAddSemesterDialog();

            // RESPONDENTS opens the list of people who answered the selected term.
            evaluationRespondentsButton = new RoundedButton
            {
                BackColor = Color.Transparent,
                CornerRadius = 10,
                DisabledTextColor = Color.FromArgb(148, 163, 184),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.25F, FontStyle.Bold),
                ForeColor = Color.White,
                HoverColor = Color.FromArgb(4, 60, 45),
                NormalColor = Color.FromArgb(6, 78, 59),
                PressedColor = Color.FromArgb(3, 45, 34),
                Size = new Size(128, 34),
                Text = "RESPONDENTS",
                UseVisualStyleBackColor = false
            };
            evaluationRespondentsButton.FlatAppearance.BorderSize = 0;
            evaluationRespondentsButton.Click += (s, e) => ShowEvaluationRespondents();

            evaluationHead.Controls.Add(evaluationTitle);
            evaluationHead.Controls.Add(evaluationPeriodStatus);
            evaluationHead.Controls.Add(evaluationPeriodBox);
            evaluationHead.Controls.Add(evaluationPeriodButton);
            evaluationHead.Controls.Add(evaluationNewPeriodButton);
            evaluationHead.Controls.Add(evaluationRespondentsButton);

            evaluationHead.Resize += (s, e) => LayoutEvaluationHead(evaluationHead, evaluationTitle);
            LayoutEvaluationHead(evaluationHead, evaluationTitle);

            // ------------------------------------------------ FEEDBACK CHARTS
            TableLayoutPanel feedbackRow = new TableLayoutPanel
            {
                BackColor = Color.White,
                ColumnCount = 3,
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                RowCount = 1
            };

            feedbackRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            feedbackRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            feedbackRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));

            evaluationRoleChart = new DonutChart
            {
                BackColor = Color.White,
                CenterCaption = "RESPONSES",
                EmptyText = "NO EVALUATION RESPONSES HAVE BEEN SUBMITTED YET.",
                Subtitle = "STUDENT, FACULTY AND STAFF SUBMISSIONS",
                Title = "RESPONSES BY RESPONDENT"
            };

            evaluationSectionChart = new LineChart
            {
                AxisSteps = 5,
                BackColor = Color.White,
                EmptyText = "NO EVALUATION ANSWERS HAVE BEEN SUBMITTED YET.",
                LineColor = Color.FromArgb(37, 99, 235),
                MaximumValue = 5,
                Subtitle = "MEAN SCORE PER SECTION (5 = EXCELLENT)",
                Title = "AVERAGE RATING",
                ValueDecimals = 2
            };

            evaluationRatingChart = new LineChart
            {
                BackColor = Color.White,
                EmptyText = "NO EVALUATION ANSWERS HAVE BEEN SUBMITTED YET.",
                LineColor = Color.FromArgb(234, 88, 12),
                Subtitle = "EVERY RATING GIVEN, FROM 1 (POOR) TO 5 (EXCELLENT)",
                Title = "FEEDBACK RATING DISTRIBUTION"
            };

            body.Controls.Add(kpiStrip, 0, 0);
            body.Controls.Add(CreateAnalyticsCard(programVisitsChart, new Padding(0, 0, 0, StackGap)), 0, 1);
            body.Controls.Add(CreateAnalyticsCard(topBorrowersChart, new Padding(0, 0, 0, StackGap)), 0, 2);
            body.Controls.Add(CreateAnalyticsCard(mostBorrowedBooksChart, new Padding(0, 0, 0, StackGap)), 0, 3);
            body.Controls.Add(evaluationHead, 0, 4);
            body.Controls.Add(feedbackRow, 0, 5);

            feedbackRow.Controls.Add(
                CreateAnalyticsCard(evaluationRoleChart, new Padding(0, 0, 12, 0)), 0, 0);
            feedbackRow.Controls.Add(
                CreateAnalyticsCard(evaluationSectionChart, new Padding(0, 0, 12, 0)), 1, 0);
            feedbackRow.Controls.Add(
                CreateAnalyticsCard(evaluationRatingChart, new Padding(0, 0, 0, 0)), 2, 0);

            analyticsScroll.Controls.Add(body);
            analyticsTab.Controls.Add(analyticsScroll);
            analyticsTab.Controls.Add(analyticsHead);
        }

        private static AnalyticsCard CreateAnalyticsCard(Control content, Padding margin)
        {
            AnalyticsCard card = new AnalyticsCard
            {
                Dock = DockStyle.Fill,
                Margin = margin
            };

            content.Dock = DockStyle.Fill;
            card.Controls.Add(content);

            return card;
        }

        private static AnalyticsStatCard CreateEvaluationCard(string caption, Color accent)
        {
            return new AnalyticsStatCard
            {
                AccentColor = accent,
                Caption = caption,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 12, 0),
                Value = "\u2014"
            };
        }

        private void LayoutAnalyticsHead()
        {
            if (analyticsHead == null || analyticsRefreshButton == null ||
                analyticsTitle == null || analyticsStatus == null)
            {
                return;
            }

            int right = analyticsHead.ClientSize.Width;

            analyticsRefreshButton.Left = Math.Max(0, right - analyticsRefreshButton.Width - 2);
            analyticsRefreshButton.Top = 24;

            int textWidth = Math.Max(240, analyticsRefreshButton.Left - 16);

            analyticsTitle.Width = textWidth;
            analyticsStatus.Width = textWidth;
        }

        // ------------------------------------------------------ EVALUATION PERIOD

        /// <summary>
        /// Places the semester strip on the right of the EVALUATION FORM FEEDBACK
        /// header: the period combo first, then OPEN/CLOSE and NEW SEMESTER, all on
        /// one line so the feedback block stays open and uncluttered.
        /// </summary>
        private void LayoutEvaluationHead(Panel head, Label title)
        {
            if (head == null || title == null || evaluationPeriodBox == null ||
                evaluationPeriodButton == null || evaluationNewPeriodButton == null ||
                evaluationRespondentsButton == null)
            {
                return;
            }

            const int gap = 8;
            const int top = 15;

            int right = Math.Max(0, head.ClientSize.Width - 2);

            evaluationRespondentsButton.Left = Math.Max(0, right - evaluationRespondentsButton.Width);
            evaluationNewPeriodButton.Left = Math.Max(
                0, evaluationRespondentsButton.Left - gap - evaluationNewPeriodButton.Width);
            evaluationPeriodButton.Left = Math.Max(
                0, evaluationNewPeriodButton.Left - gap - evaluationPeriodButton.Width);
            evaluationPeriodBox.Left = Math.Max(
                0, evaluationPeriodButton.Left - gap - evaluationPeriodBox.Width);

            evaluationRespondentsButton.Top = top;
            evaluationNewPeriodButton.Top = top;
            evaluationPeriodButton.Top = top;

            // The flat combo is a little shorter than the 34 px buttons, so it is
            // centred against them instead of sitting flush with their top edge.
            evaluationPeriodBox.Top = top + Math.Max(0, (34 - evaluationPeriodBox.Height) / 2);

            int textWidth = Math.Max(220, evaluationPeriodBox.Left - 12);

            title.Width = textWidth;

            if (evaluationPeriodStatus != null)
            {
                evaluationPeriodStatus.Width = textWidth;
            }
        }

        /// <summary>
        /// Draws a semester entry with a left inset, because a flat combo otherwise
        /// puts the text right against its border. The larger font and the padding
        /// keep the whole "1ST SEMESTER 2026-2027" readable in the closed box.
        /// </summary>
        private void EvaluationPeriodBoxDrawItem(object? sender, DrawItemEventArgs e)
        {
            if (!(sender is ComboBox box))
            {
                return;
            }

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            bool disabled = (e.State & DrawItemState.Disabled) == DrawItemState.Disabled;

            Color fill = selected && !disabled ? SystemColors.Highlight : box.BackColor;

            using (SolidBrush background = new SolidBrush(fill))
            {
                e.Graphics.FillRectangle(background, e.Bounds);
            }

            if (e.Index >= 0 && e.Index < box.Items.Count)
            {
                string text = Convert.ToString(box.Items[e.Index], CultureInfo.InvariantCulture) ?? string.Empty;

                Color foreColor = disabled
                    ? Color.FromArgb(148, 163, 184)
                    : selected ? SystemColors.HighlightText : box.ForeColor;

                Rectangle bounds = new Rectangle(
                    e.Bounds.X + 10,
                    e.Bounds.Y,
                    Math.Max(0, e.Bounds.Width - 12),
                    e.Bounds.Height);

                TextRenderer.DrawText(
                    e.Graphics,
                    text,
                    box.Font,
                    bounds,
                    foreColor,
                    TextFormatFlags.Left |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis |
                    TextFormatFlags.NoPrefix);
            }

            if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
            {
                e.DrawFocusRectangle();
            }
        }

        /// <summary>
        /// Fills the PERIOD combo from evaluation_period_table with exactly the
        /// semesters the librarian created, for example "1ST SEMESTER 2026-2027".
        /// The open semester is picked by default so opening ANALYTICS always lands
        /// on the live term, while a closed semester can still be picked to review
        /// its old answers.
        /// </summary>
        private void LoadEvaluationPeriods(bool keepSelection)
        {
            if (evaluationPeriodBox == null)
            {
                return;
            }

            int? previous = SelectedEvaluationPeriodId();
            _evaluationPeriods = _evaluationDirectory.LoadPeriods();

            _fillingEvaluationPeriods = true;

            try
            {
                evaluationPeriodBox.Items.Clear();

                foreach (EvaluationPeriod period in _evaluationPeriods)
                {
                    evaluationPeriodBox.Items.Add(
                        period.Title + (period.IsOpen ? "  -  OPEN" : "  -  CLOSED"));
                }

                // Only real semesters are listed, so there is no combined view: every
                // figure on the page belongs to one semester at a time.
                int index = -1;

                if (_evaluationPeriods.Count > 0)
                {
                    if (keepSelection && previous.HasValue)
                    {
                        index = _evaluationPeriods.FindIndex(period => period.PeriodId == previous.Value);
                    }

                    if (index < 0)
                    {
                        index = _evaluationPeriods.FindIndex(period => period.IsOpen);
                    }

                    if (index < 0)
                    {
                        index = 0;
                    }
                }

                evaluationPeriodBox.SelectedIndex = index;
                evaluationPeriodBox.Enabled = _evaluationPeriods.Count > 0;
            }
            finally
            {
                _fillingEvaluationPeriods = false;
            }

            UpdateEvaluationPeriodLabels(null);
        }

        private EvaluationPeriod? SelectedEvaluationPeriod()
        {
            int index = evaluationPeriodBox == null ? -1 : evaluationPeriodBox.SelectedIndex;

            return index >= 0 && index < _evaluationPeriods.Count
                ? _evaluationPeriods[index]
                : null;
        }

        private int? SelectedEvaluationPeriodId()
        {
            EvaluationPeriod? period = SelectedEvaluationPeriod();

            return period == null ? (int?)null : period.PeriodId;
        }

        private void EvaluationPeriodChanged()
        {
            if (_fillingEvaluationPeriods)
            {
                return;
            }

            LoadEvaluationFeedback();
        }

        /// <summary>Reads the feedback figures for the semester picked in the combo.</summary>
        private void LoadEvaluationFeedback()
        {
            int? periodId = SelectedEvaluationPeriodId();

            if (!periodId.HasValue)
            {
                // No semester has been created yet, so there is nothing to summarise.
                ResetEvaluationSummary();
                UpdateEvaluationPeriodLabels(null);
                return;
            }

            try
            {
                EvaluationSummary summary = _evaluationDirectory.LoadSummary(periodId.Value);
                ApplyEvaluationSummary(summary);
            }
            catch (MySqlException)
            {
                ResetEvaluationSummary();
                UpdateEvaluationPeriodLabels(null);
            }
        }

        private void ToggleEvaluationPeriod()
        {
            EvaluationPeriod? period = SelectedEvaluationPeriod();

            if (period == null)
            {
                return;
            }

            try
            {
                if (period.IsOpen)
                {
                    if (!ConfirmPeriodChange(
                        "CLOSE THE EVALUATION FOR\n" + period.Title + "?\n\n" +
                        "STUDENTS, FACULTY AND STAFF WILL NO LONGER BE ABLE TO SUBMIT ANSWERS.",
                        "CLOSE EVALUATION"))
                    {
                        return;
                    }

                    _evaluationDirectory.ClosePeriod(period.PeriodId);
                }
                else
                {
                    if (!ConfirmPeriodChange(
                        "OPEN THE EVALUATION FOR\n" + period.Title + "?\n\n" +
                        "IF ANOTHER SEMESTER IS STILL OPEN IT WILL BE CLOSED FIRST.",
                        "OPEN EVALUATION"))
                    {
                        return;
                    }

                    _evaluationDirectory.OpenPeriod(period.Semester, period.SchoolYear);
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(
                    this,
                    "THE SEMESTER COULD NOT BE UPDATED.\n" + ex.Message,
                    "EVALUATION PERIOD",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            LoadEvaluationPeriods(true);
            LoadEvaluationFeedback();
        }

        /// <summary>
        /// Opens the ADD SEMESTER pop up, prefilled with the term that matches
        /// today, so the librarian types (or just confirms) the semester the
        /// students are allowed to evaluate. Adding it opens it straight away.
        /// </summary>
        private void ShowAddSemesterDialog()
        {
            (string Semester, string SchoolYear) suggested = CurrentSemesterTerm(DateTime.Now);

            using (AddSemesterDialog dialog = new AddSemesterDialog(suggested.Semester, suggested.SchoolYear))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                ApplyNewSemester(dialog.Semester, dialog.SchoolYear);
            }
        }

        /// <summary>
        /// Saves the semester chosen in the pop up. Because OpenPeriod closes any
        /// other open semester, this is also what takes the vote away from the
        /// previous term: students can answer only while a semester stays open.
        /// </summary>
        private void ApplyNewSemester(string semester, string schoolYear)
        {
            try
            {
                _evaluationDirectory.OpenPeriod(semester, schoolYear);
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(
                    this,
                    "THE SEMESTER COULD NOT BE OPENED.\n" + ex.Message,
                    "EVALUATION PERIOD",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // A brand new semester is opened, so the charts jump to it instead of
            // staying on the semester the librarian was reviewing.
            LoadEvaluationPeriods(false);
            LoadEvaluationFeedback();
        }

        /// <summary>
        /// Opens the RESPONDENTS window for the semester in the combo, so the
        /// librarian can see who answered and which institute and program they come
        /// from. The rows are read here, so the window only has to show them.
        /// </summary>
        private void ShowEvaluationRespondents()
        {
            EvaluationPeriod? period = SelectedEvaluationPeriod();

            if (period == null)
            {
                return;
            }

            try
            {
                List<EvaluationRespondent> respondents = _evaluationDirectory.LoadRespondents(period.PeriodId);

                using (EvaluationRespondentsDialog dialog =
                    new EvaluationRespondentsDialog(period.Title, respondents))
                {
                    dialog.ShowDialog(this);
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(
                    this,
                    "THE RESPONDENTS COULD NOT BE LOADED.\n" + ex.Message,
                    "EVALUATION RESPONDENTS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private bool ConfirmPeriodChange(string message, string caption)
        {
            return MessageBox.Show(
                this,
                message,
                caption,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }

        private void UpdateEvaluationPeriodLabels(EvaluationSummary? summary)
        {
            if (evaluationPeriodStatus == null || evaluationPeriodButton == null ||
                evaluationRespondentsButton == null)
            {
                return;
            }

            EvaluationPeriod? period = SelectedEvaluationPeriod();
            string text;
            Color color;

            if (period == null)
            {
                // Nothing selected means no semester exists yet, because the combo
                // only ever lists semesters the librarian created.
                text = "NO EVALUATION PERIOD YET  \u2022  PRESS ADD SEMESTER TO OPEN THE FIRST ONE.";
                color = WarningText;
            }
            else
            {
                // The combo right beside the label already names the semester, so the
                // strip only has to say whether it collects answers and how many.
                text = period.IsOpen ? "EVALUATION OPEN" : "EVALUATION CLOSED";
                color = period.IsOpen ? SuccessText : WarningText;
            }

            if (summary != null)
            {
                text += "  \u2022  " +
                        summary.TotalResponses.ToString("N0", CultureInfo.CurrentCulture) +
                        (summary.TotalResponses == 1 ? " RESPONSE" : " RESPONSES");
            }

            evaluationPeriodStatus.Text = text;
            evaluationPeriodStatus.ForeColor = color;

            evaluationPeriodButton.Enabled = period != null;
            evaluationPeriodButton.Text = period == null
                ? "OPEN / CLOSE"
                : period.IsOpen ? "CLOSE EVALUATION" : "OPEN EVALUATION";

            evaluationRespondentsButton.Enabled = period != null;

            // Closing a period is the amber action; opening keeps the library green.
            if (period != null && period.IsOpen)
            {
                evaluationPeriodButton.NormalColor = Color.FromArgb(217, 119, 6);
                evaluationPeriodButton.HoverColor = Color.FromArgb(180, 83, 9);
                evaluationPeriodButton.PressedColor = Color.FromArgb(146, 64, 14);
            }
            else
            {
                evaluationPeriodButton.NormalColor = Color.FromArgb(22, 163, 74);
                evaluationPeriodButton.HoverColor = Color.FromArgb(21, 128, 61);
                evaluationPeriodButton.PressedColor = Color.FromArgb(20, 83, 45);
            }

            evaluationPeriodButton.Invalidate();
        }

        /// <summary>
        /// The semester for a date. A school year starts in June: the first semester
        /// runs June to October, the second November to March, and April and May are
        /// the summer term.
        /// </summary>
        private static (string Semester, string SchoolYear) CurrentSemesterTerm(DateTime date)
        {
            int year = date.Year;

            if (date.Month >= 6 && date.Month <= 10)
            {
                return ("1ST SEMESTER", year + "-" + (year + 1));
            }

            if (date.Month >= 11)
            {
                return ("2ND SEMESTER", year + "-" + (year + 1));
            }

            if (date.Month <= 3)
            {
                return ("2ND SEMESTER", (year - 1) + "-" + year);
            }

            return ("SUMMER", (year - 1) + "-" + year);
        }

        private void LoadAnalytics()
        {
            if (programVisitsChart == null || topBorrowersChart == null ||
                mostBorrowedBooksChart == null || analyticsStatus == null)
            {
                return;
            }

            analyticsRefreshButton.Enabled = false;

            try
            {
                List<ChartItem> programs = LoadProgramVisitChart();
                List<ChartItem> borrowers = LoadTopBorrowerChart();
                List<ChartItem> books = LoadMostBorrowedBookChart();
                List<ChartItem> topPrograms = TakeChartPoints(programs);

                // The subtitle always says how many rows the curve shows, so the
                // librarian knows when a long program list is summarised.
                programVisitsChart.Subtitle = programs.Count > topPrograms.Count
                    ? "TOP " + topPrograms.Count + " OF " + programs.Count + " PROGRAMS BY STUDENT VISITS"
                    : "STUDENT VISITS TO THE LIBRARY, BY PROGRAM";
                programVisitsChart.SetData(topPrograms);

                topBorrowersChart.Subtitle = borrowers.Count == 1
                    ? "THE STUDENT WHO BORROWED THE MOST BOOKS"
                    : "THE " + Math.Min(ChartPointLimit, borrowers.Count) + " STUDENTS WHO BORROWED THE MOST BOOKS";
                topBorrowersChart.SetData(borrowers);

                // The bars are ranked the same way and start at the highest count,
                // so the longest bar is always the book borrowed the most times.
                if (books.Count == 0)
                {
                    mostBorrowedBooksChart.Subtitle = "THE BOOKS BORROWED THE MOST TIMES";
                }
                else if (books.Count == 1)
                {
                    mostBorrowedBooksChart.Subtitle = "THE BOOK BORROWED THE MOST TIMES";
                }
                else
                {
                    mostBorrowedBooksChart.Subtitle =
                        "THE " + Math.Min(ChartPointLimit, books.Count) + " BOOKS BORROWED THE MOST TIMES";
                }

                mostBorrowedBooksChart.SetData(books);

                // The survey lives in its own tables, so a missing or unreadable
                // evaluation schema only empties the feedback block instead of
                // hiding the visits and borrower charts that loaded fine.
                EvaluationSummary? feedback = null;
                int? feedbackPeriod = null;

                try
                {
                    // The semester strip is refreshed first, then the feedback is read
                    // for the semester the combo is showing. Before any semester is
                    // created there is nothing to summarise.
                    LoadEvaluationPeriods(true);
                    feedbackPeriod = SelectedEvaluationPeriodId();

                    if (feedbackPeriod.HasValue)
                    {
                        feedback = _evaluationDirectory.LoadSummary(feedbackPeriod.Value);
                    }
                }
                catch (MySqlException)
                {
                    feedback = null;
                }

                if (feedback != null)
                {
                    ApplyEvaluationSummary(feedback);
                    SetAnalyticsStatus(string.Empty, WarningText);
                }
                else
                {
                    ResetEvaluationSummary();

                    // A semester that has no answers yet is a normal state, so only a
                    // semester that exists but cannot be read raises a warning.
                    SetAnalyticsStatus(
                        feedbackPeriod.HasValue ? "EVALUATION FEEDBACK UNAVAILABLE" : string.Empty,
                        WarningText);
                }
            }
            catch (MySqlException)
            {
                programVisitsChart.SetData(new List<ChartItem>());
                topBorrowersChart.SetData(new List<ChartItem>());
                mostBorrowedBooksChart.SetData(new List<ChartItem>());
                ResetEvaluationSummary();
                SetAnalyticsStatus("MYSQL OFFLINE \u2022 THE CHARTS CANNOT BE LOADED", WarningText);
            }
            finally
            {
                analyticsRefreshButton.Enabled = true;
            }
        }

        /// <summary>
        /// Shows or clears the single line under the ANALYTICS title. The header is
        /// deliberately free of counters and timestamps, so the line is only used to
        /// explain why the charts could not be refreshed.
        /// </summary>
        private void SetAnalyticsStatus(string text, Color color)
        {
            if (analyticsStatus == null)
            {
                return;
            }

            analyticsStatus.Text = text ?? string.Empty;
            analyticsStatus.ForeColor = color;
            analyticsStatus.Visible = analyticsStatus.Text.Length > 0;
        }

        /// <summary>
        /// Pushes one evaluation snapshot into the KPI cards and the three
        /// feedback charts. The respondent split comes from evaluation_table and
        /// the scores from evaluation_answer_table, so the charts always match the
        /// answers the students, faculty and staff actually submitted.
        /// </summary>
        private void ApplyEvaluationSummary(EvaluationSummary summary)
        {
            if (summary == null)
            {
                return;
            }

            if (evalTotalCard != null)
            {
                evalTotalCard.Value = summary.TotalResponses.ToString("N0", CultureInfo.CurrentCulture);
            }

            if (evalStudentCard != null)
            {
                evalStudentCard.Value = summary.StudentResponses.ToString("N0", CultureInfo.CurrentCulture);
            }

            if (evalFacultyCard != null)
            {
                evalFacultyCard.Value = summary.FacultyResponses.ToString("N0", CultureInfo.CurrentCulture);
            }

            if (evalStaffCard != null)
            {
                evalStaffCard.Value = summary.StaffResponses.ToString("N0", CultureInfo.CurrentCulture);
            }

            if (evaluationRoleChart != null)
            {
                List<ChartItem> roles = new List<ChartItem>();

                if (summary.StudentResponses > 0)
                {
                    roles.Add(new ChartItem("STUDENT", summary.StudentResponses));
                }

                if (summary.FacultyResponses > 0)
                {
                    roles.Add(new ChartItem("FACULTY", summary.FacultyResponses));
                }

                if (summary.StaffResponses > 0)
                {
                    roles.Add(new ChartItem("STAFF", summary.StaffResponses));
                }

                if (summary.OtherResponses > 0)
                {
                    roles.Add(new ChartItem("OTHER", summary.OtherResponses));
                }

                evaluationRoleChart.SetData(roles);
            }

            if (evaluationSectionChart != null)
            {
                List<ChartItem> sections = new List<ChartItem>();

                for (int section = 0; section < EvaluationDirectory.SectionCount; section++)
                {
                    if (summary.SectionAnswerCounts[section] <= 0)
                    {
                        continue;
                    }

                    sections.Add(new ChartItem(
                        EvaluationSectionLabels[section],
                        summary.SectionAverages[section]));
                }

                evaluationSectionChart.SetData(sections);
            }

            if (evaluationRatingChart != null)
            {
                List<ChartItem> ratings = new List<ChartItem>();

                // The curve rises from 1 (POOR) to 5 (EXCELLENT), so the line
                // reads left to right the same way the scale does.
                if (summary.TotalAnswers > 0)
                {
                    ratings.Add(new ChartItem("1  \u2022  POOR", summary.RatingCounts[1]));
                    ratings.Add(new ChartItem("2  \u2022  FAIR", summary.RatingCounts[2]));
                    ratings.Add(new ChartItem("3  \u2022  GOOD", summary.RatingCounts[3]));
                    ratings.Add(new ChartItem("4  \u2022  VERY GOOD", summary.RatingCounts[4]));
                    ratings.Add(new ChartItem("5  \u2022  EXCELLENT", summary.RatingCounts[5]));
                }

                evaluationRatingChart.SetData(ratings);
            }

            // The header strip shows the period, its state and how many answers it holds.
            UpdateEvaluationPeriodLabels(summary);
        }

        private void ResetEvaluationSummary()
        {
            ApplyEvaluationSummary(new EvaluationSummary());
        }

        /// <summary>
        /// Keeps the ten highest values of a ranked list, so a long program or
        /// borrower list still turns into a readable curve with room for labels.
        /// </summary>
        private static List<ChartItem> TakeChartPoints(List<ChartItem> items)
        {
            List<ChartItem> top = new List<ChartItem>(Math.Min(ChartPointLimit, items.Count));

            for (int index = 0; index < items.Count && index < ChartPointLimit; index++)
            {
                top.Add(items[index]);
            }

            return top;
        }

        /// <summary>
        /// SQL #1: lists every program in program_table and counts the library
        /// visits of its STUDENTS only (attendance rows whose stored role is
        /// STUDENT). LEFT JOINs keep a program with zero visits in the list, and
        /// the school's built-in program list is merged in as well, so every known
        /// program is shown even before anyone enrolls in it.
        /// </summary>
        private static List<ChartItem> LoadProgramVisitChart()
        {
            const string sql =
                "SELECT p.`program_name`, COUNT(a.`attendance_id`) AS `visits` " +
                "FROM `program_table` p " +
                "LEFT JOIN `student_info_table` s ON s.`program_id` = p.`program_id` " +
                "LEFT JOIN `account_role_table` ar ON ar.`account_id` = s.`account_id` " +
                "LEFT JOIN `role_table` rt ON rt.`role_id` = ar.`role_id` " +
                "LEFT JOIN `attendance_table` a ON a.`account_id` = s.`account_id` AND rt.`role_code` = 'STUDENT' " +
                "GROUP BY p.`program_id`, p.`program_name` " +
                "ORDER BY `visits` DESC, p.`program_name` ASC;";

            Dictionary<string, int> visits = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            using (MySqlConnection connection = Authentication.OpenConnection())
            {
                connection.Open();

                using (MySqlCommand command = new MySqlCommand(sql, connection))
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string label = reader.IsDBNull(0)
                            ? "UNASSIGNED PROGRAM"
                            : (Convert.ToString(reader.GetValue(0), CultureInfo.InvariantCulture) ?? string.Empty).Trim();

                        int value = reader.IsDBNull(1)
                            ? 0
                            : Convert.ToInt32(reader.GetValue(1), CultureInfo.InvariantCulture);

                        if (label.Length == 0)
                        {
                            continue;
                        }

                        int current;
                        visits.TryGetValue(label, out current);
                        visits[label] = current + value;
                    }
                }
            }

            // A program that has no row in program_table yet (a fresh install)
            // still appears with 0 visits, so the list is complete from day one.
            foreach (KeyValuePair<string, string[]> group in FallbackPrograms)
            {
                foreach (string program in group.Value)
                {
                    if (!visits.ContainsKey(program))
                    {
                        visits[program] = 0;
                    }
                }
            }

            List<ChartItem> items = new List<ChartItem>();

            foreach (KeyValuePair<string, int> entry in visits)
            {
                items.Add(new ChartItem(entry.Key.ToUpperInvariant(), entry.Value));
            }

            items.Sort(delegate (ChartItem left, ChartItem right)
            {
                int byValue = right.Value.CompareTo(left.Value);

                return byValue != 0
                    ? byValue
                    : string.Compare(left.Label, right.Label, StringComparison.OrdinalIgnoreCase);
            });

            return items;
        }

        /// <summary>
        /// SQL #2: ranks borrowers by the number of borrow_transactions_table rows
        /// linked to them, so the person who borrowed the most books comes first.
        /// The INNER JOIN on student_info_table keeps STUDENT records only, and the
        /// HAVING clause removes anyone with 0 borrowed books.
        /// </summary>
        private static List<ChartItem> LoadTopBorrowerChart()
        {
            const string sql =
                "SELECT CONCAT(b.`last_name`, ', ', b.`first_name`, " +
                "              CASE WHEN b.`middle_name` IS NULL OR b.`middle_name` = '' THEN '' " +
                "                   ELSE CONCAT(' ', b.`middle_name`) END) AS `borrower_name`, " +
                "       COUNT(t.`borrow_id`) AS `total` " +
                "FROM `borrowers_table` b " +
                "INNER JOIN `borrow_transactions_table` t ON t.`borrower_id` = b.`borrower_id` " +
                "INNER JOIN `student_info_table` s ON s.`school_id` = b.`id_number` " +
                "GROUP BY b.`borrower_id`, b.`last_name`, b.`first_name`, b.`middle_name` " +
                "HAVING COUNT(t.`borrow_id`) > 0 " +
                "ORDER BY `total` DESC, `borrower_name` ASC " +
                "LIMIT 10;";

            List<ChartItem> items = new List<ChartItem>();

            using (MySqlConnection connection = Authentication.OpenConnection())
            {
                connection.Open();

                using (MySqlCommand command = new MySqlCommand(sql, connection))
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string label = reader.IsDBNull(0)
                            ? "UNKNOWN BORROWER"
                            : (Convert.ToString(reader.GetValue(0), CultureInfo.InvariantCulture) ?? string.Empty).Trim();

                        int value = reader.IsDBNull(1)
                            ? 0
                            : Convert.ToInt32(reader.GetValue(1), CultureInfo.InvariantCulture);

                        if (label.Length > 0 && value > 0)
                        {
                            items.Add(new ChartItem(label.ToUpperInvariant(), value));
                        }
                    }
                }
            }

            return items;
        }

        /// <summary>
        /// SQL #3: ranks the catalog by how many times each book was borrowed.
        /// Every borrow_transactions_table row is one loan, so the book with the
        /// most rows is the most borrowed one. The HAVING clause keeps books that
        /// were never borrowed out of the chart, and the LIMIT matches the number
        /// of points the chart can label.
        /// </summary>
        private static List<ChartItem> LoadMostBorrowedBookChart()
        {
            string sql =
                "SELECT b.`book_id`, b.`book_title`, COUNT(t.`borrow_id`) AS `total` " +
                "FROM `books_table` b " +
                "INNER JOIN `borrow_transactions_table` t ON t.`book_id` = b.`book_id` " +
                "GROUP BY b.`book_id`, b.`book_title` " +
                "HAVING COUNT(t.`borrow_id`) > 0 " +
                "ORDER BY `total` DESC, b.`book_title` ASC " +
                "LIMIT " + ChartPointLimit + ";";

            List<ChartItem> items = new List<ChartItem>();

            using (MySqlConnection connection = Authentication.OpenConnection())
            {
                connection.Open();

                using (MySqlCommand command = new MySqlCommand(sql, connection))
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int bookId = reader.IsDBNull(0)
                            ? 0
                            : Convert.ToInt32(reader.GetValue(0), CultureInfo.InvariantCulture);

                        string label = reader.IsDBNull(1)
                            ? "UNTITLED BOOK"
                            : (Convert.ToString(reader.GetValue(1), CultureInfo.InvariantCulture) ?? string.Empty).Trim();

                        int value = reader.IsDBNull(2)
                            ? 0
                            : Convert.ToInt32(reader.GetValue(2), CultureInfo.InvariantCulture);

                        if (label.Length > 0 && value > 0)
                        {
                            // The book id rides along as the tag, so the hover preview
                            // can fetch exactly this title's cover from books_table.
                            items.Add(new ChartItem(label.ToUpperInvariant(), value, bookId));
                        }
                    }
                }
            }

            return items;
        }

        /// <summary>
        /// ANALYTICS: the cover shown when a MOST BORROWED BOOKS point is hovered.
        /// The first hover reads books_table.book_image for that book, shrinks it to
        /// the preview size and caches it; a title without a stored picture gets the
        /// neutral placeholder instead of an empty box.
        /// </summary>
        private Image? LoadBookCover(ChartItem item)
        {
            int bookId = item.Tag is int ? (int)item.Tag : 0;

            if (bookId <= 0)
            {
                return BookImage.CreateThumbnail(null, HoverCoverWidth, HoverCoverHeight, true);
            }

            Image? thumbnail;

            if (_bookCoverCache.TryGetValue(bookId, out thumbnail))
            {
                return thumbnail;
            }

            thumbnail = BookImage.CreateThumbnail(
                LoadBookCoverBytes(bookId), HoverCoverWidth, HoverCoverHeight, true);

            _bookCoverCache[bookId] = thumbnail;

            return thumbnail;
        }

        /// <summary>
        /// Reads one cover straight from books_table. A hover happens long after the
        /// charts loaded, but the server can still go away, so a failed read simply
        /// falls back to the placeholder cover instead of breaking the mouse move.
        /// </summary>
        private static byte[]? LoadBookCoverBytes(int bookId)
        {
            const string sql =
                "SELECT `book_image` FROM `books_table` WHERE `book_id` = @bookId LIMIT 1;";

            try
            {
                using (MySqlConnection connection = Authentication.OpenConnection())
                {
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@bookId", bookId);

                        return command.ExecuteScalar() as byte[];
                    }
                }
            }
            catch (MySqlException)
            {
                return null;
            }
        }

        /// <summary>
        /// ANALYTICS: the initials shown when a TOP LIBRARY BOOK BORROWERS point is
        /// hovered. A borrower has no picture, so a round badge carries their initials
        /// instead. The badge is built once per name and kept with the book covers.
        /// </summary>
        private Image LoadBorrowerAvatar(ChartItem item)
        {
            string name = item.Label ?? string.Empty;

            if (_borrowerAvatarCache.TryGetValue(name, out Image? avatar))
            {
                return avatar;
            }

            avatar = CreateInitialsAvatar(BorrowerInitials(name), HoverCoverWidth);
            _borrowerAvatarCache[name] = avatar;

            return avatar;
        }

        /// <summary>Draws a round avatar badge with the borrower's initials.</summary>
        private static Bitmap CreateInitialsAvatar(string initials, int size)
        {
            int side = Math.Max(24, size);
            Bitmap bitmap = new Bitmap(side, side);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                graphics.Clear(Color.Transparent);

                Rectangle bounds = new Rectangle(0, 0, side - 1, side - 1);

                using (GraphicsPath circle = DrawHelper.CreateRoundedPath(bounds, side / 2))
                using (LinearGradientBrush fill = new LinearGradientBrush(
                    bounds,
                    Color.FromArgb(15, 118, 110),
                    Color.FromArgb(13, 148, 136),
                    LinearGradientMode.Vertical))
                {
                    graphics.FillPath(fill, circle);
                }

                using Font font = new Font("Segoe UI", side * 0.36f, FontStyle.Bold, GraphicsUnit.Pixel);
                using SolidBrush brush = new SolidBrush(Color.White);
                using StringFormat format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                graphics.DrawString(initials, font, brush, new RectangleF(0, 0, side, side), format);
            }

            return bitmap;
        }

        /// <summary>
        /// Initials of a borrower label written as LAST, FIRST MIDDLE: the last name
        /// and the first name give the two letters, e.g. DITCHON, JOHN CARL -> DJ.
        /// </summary>
        private static string BorrowerInitials(string label)
        {
            string name = (label ?? string.Empty).Trim();

            if (name.Length == 0)
            {
                return "?";
            }

            int comma = name.IndexOf(',');

            if (comma > 0)
            {
                string last = name.Substring(0, comma).Trim();
                string rest = name.Substring(comma + 1).Trim();
                int space = rest.IndexOf(' ');
                string first = space > 0 ? rest.Substring(0, space) : rest;

                if (last.Length > 0 && first.Length > 0)
                {
                    return (last.Substring(0, 1) + first.Substring(0, 1)).ToUpperInvariant();
                }
            }

            return BuildInitials(name);
        }

        /// <summary>
        /// Releases the cached covers and avatars when the dashboard closes. They are
        /// only held for the hover preview, so nothing else needs them afterwards.
        /// </summary>
        private void DisposeHoverImages()
        {
            foreach (Image cover in _bookCoverCache.Values)
            {
                cover.Dispose();
            }

            _bookCoverCache.Clear();

            foreach (Image avatar in _borrowerAvatarCache.Values)
            {
                avatar.Dispose();
            }

            _borrowerAvatarCache.Clear();
        }

        private sealed class ChartItem
        {
            public ChartItem(string label, double value, object? tag = null)
            {
                Label = label;
                Value = value;
                Tag = tag;
            }

            public string Label { get; }

            // A double keeps whole counts (visits, borrowers, responses) and the
            // 1-5 section means in the same item type.
            public double Value { get; }

            // Optional row id behind the item. The MOST BORROWED BOOKS chart keeps
            // the book id here so hovering a point can fetch that cover.
            public object? Tag { get; }
        }

        /// <summary>
        /// Soft shadow + white rounded body shared by every ANALYTICS card. The
        /// shadow is painted inside the control (the page is white like the card),
        /// so the cards keep a crisp 1 px edge and need no transparency support.
        /// </summary>
        private static void PaintAnalyticsSurface(Graphics graphics, Size size, int radius, Color border)
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(Color.White);

            const int inset = 6;

            if (size.Width <= (inset * 2) + 24 || size.Height <= (inset * 2) + 24)
            {
                return;
            }

            Rectangle body = new Rectangle(
                inset,
                inset - 2,
                size.Width - (inset * 2),
                size.Height - (inset * 2) + 2);

            // Four 1 px rings with a low alpha build the soft shadow under the body.
            for (int layer = 4; layer >= 1; layer--)
            {
                Rectangle ring = new Rectangle(
                    body.X - layer,
                    body.Y - layer + 2,
                    body.Width + (layer * 2),
                    body.Height + (layer * 2));

                using GraphicsPath shadowPath = DrawHelper.CreateRoundedPath(ring, radius + layer);
                using Pen shadowPen = new Pen(Color.FromArgb(layer == 1 ? 18 : 9, 15, 23, 42), 1f);
                graphics.DrawPath(shadowPen, shadowPath);
            }

            using (GraphicsPath card = DrawHelper.CreateRoundedPath(body, radius))
            {
                using (SolidBrush fill = new SolidBrush(Color.White))
                {
                    graphics.FillPath(fill, card);
                }

                using Pen edge = new Pen(border, 1f);
                graphics.DrawPath(edge, card);
            }
        }

        /// <summary>Rounded card with a subtle drop shadow used by the ANALYTICS tab.</summary>
        private sealed class AnalyticsCard : Panel
        {
            private int _cornerRadius = 16;
            private Color _borderColor = Color.FromArgb(226, 232, 240);

            public AnalyticsCard()
            {
                SetStyle(ControlStyles.UserPaint |
                         ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw, true);

                BackColor = Color.White;
                Padding = new Padding(8);
            }

            public int CornerRadius
            {
                get { return _cornerRadius; }
                set { _cornerRadius = Math.Max(0, value); Invalidate(); }
            }

            public Color BorderColor
            {
                get { return _borderColor; }
                set { _borderColor = value; Invalidate(); }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                PaintAnalyticsSurface(e.Graphics, ClientSize, _cornerRadius, _borderColor);
            }
        }

        /// <summary>
        /// KPI tile of the ANALYTICS tab: rounded white card with a soft shadow, a
        /// coloured accent bar, the caption and the large value.
        /// </summary>
        private sealed class AnalyticsStatCard : Control
        {
            private string _caption = "TOTAL";
            private string _value = "\u2014";
            private Color _accentColor = Color.FromArgb(22, 163, 74);

            public AnalyticsStatCard()
            {
                SetStyle(ControlStyles.UserPaint |
                         ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw, true);

                BackColor = Color.White;
                Margin = new Padding(0, 0, 12, 0);
                TabStop = false;
            }

            public string Caption
            {
                get { return _caption; }
                set { _caption = value ?? string.Empty; Invalidate(); }
            }

            public string Value
            {
                get { return _value; }
                set { _value = value ?? string.Empty; Invalidate(); }
            }

            public Color AccentColor
            {
                get { return _accentColor; }
                set { _accentColor = value; Invalidate(); }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics graphics = e.Graphics;
                PaintAnalyticsSurface(graphics, ClientSize, 16, Color.FromArgb(226, 232, 240));

                const int inset = 6;
                Rectangle body = new Rectangle(
                    inset,
                    inset - 2,
                    Width - (inset * 2),
                    Height - (inset * 2) + 2);

                if (body.Width < 110 || body.Height < 54)
                {
                    return;
                }

                Rectangle accent = new Rectangle(body.X + 14, body.Y + 18, 4, Math.Max(14, body.Height - 36));

                using (GraphicsPath accentPath = DrawHelper.CreateRoundedPath(accent, 2))
                using (SolidBrush accentBrush = new SolidBrush(AccentColor))
                {
                    graphics.FillPath(accentBrush, accentPath);
                }

                using Font captionFont = new Font("Segoe UI", 7.25F, FontStyle.Bold);
                TextRenderer.DrawText(
                    graphics,
                    (Caption ?? string.Empty).ToUpperInvariant(),
                    captionFont,
                    new Rectangle(body.X + 27, body.Y + 14, body.Width - 40, 16),
                    Color.FromArgb(100, 116, 139),
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                using Font valueFont = new Font("Segoe UI", 20F, FontStyle.Bold);
                TextRenderer.DrawText(
                    graphics,
                    Value ?? "\u2014",
                    valueFont,
                    new Rectangle(body.X + 26, body.Y + 32, body.Width - 38, 36),
                    Color.FromArgb(15, 23, 42),
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
        }

        /// <summary>
        /// Draws the title and the colour dotted subtitle at the top of an ANALYTICS
        /// chart. The line and bar cards share this heading, so every chart in the
        /// tab reads as part of the same family.
        /// </summary>
        private static void DrawChartHeading(
            Graphics graphics,
            string title,
            string subtitle,
            Color accent,
            int left,
            int top,
            int right)
        {
            using Font titleFont = new Font("Segoe UI", 11.25F, FontStyle.Bold);
            using Font subtitleFont = new Font("Segoe UI", 8F);

            TextRenderer.DrawText(
                graphics,
                title,
                titleFont,
                new Rectangle(left, top, Math.Max(40, right - left), 24),
                Color.FromArgb(15, 23, 42),
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            // A colour dot in front of the subtitle ties the card to its data.
            using (GraphicsPath dot = DrawHelper.CreateRoundedPath(new Rectangle(left, top + 36, 7, 7), 3))
            using (SolidBrush dotBrush = new SolidBrush(accent))
            {
                graphics.FillPath(dotBrush, dot);
            }

            TextRenderer.DrawText(
                graphics,
                subtitle,
                subtitleFont,
                new Rectangle(left + 13, top + 29, Math.Max(40, right - left - 13), 20),
                Color.FromArgb(100, 116, 139),
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        /// <summary>
        /// Smooth line chart drawn with GDI+ (no external chart package), used for
        /// every ANALYTICS figure: title, subtitle in the chart colour, clean axis
        /// ticks, subtle gridlines, a soft gradient area under the curve, visible
        /// points and a value label above each point. A chart that sets
        /// <see cref="HoverImageProvider"/> also fades a small picture card in when a
        /// point is hovered.
        /// </summary>
        private sealed class LineChart : Control
        {
            private List<ChartItem> _items = new List<ChartItem>();

            // Hover preview: the chart that sets HoverImageProvider fades a small
            // picture card in next to the point under the mouse. The card is built
            // once per hover, then blended with a rising alpha so the hover reads as
            // a smooth fade instead of a pop.
            private readonly System.Windows.Forms.Timer _previewTimer =
                new System.Windows.Forms.Timer();
            private readonly Dictionary<ChartItem, Image?> _previewImages =
                new Dictionary<ChartItem, Image?>();

            private PointF[] _points = Array.Empty<PointF>();
            private Rectangle _plotArea;
            private float _columnWidth;
            private int _previewIndex = -1;
            private Image? _previewImage;
            private Bitmap? _previewCard;
            private float _previewOpacity;
            private bool _previewHiding;

            public LineChart()
            {
                SetStyle(ControlStyles.UserPaint |
                         ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw, true);

                BackColor = Color.White;
                Font = new Font("Segoe UI", 9F);
                Padding = new Padding(18, 14, 18, 10);

                _previewTimer.Interval = 16;
                _previewTimer.Tick += (s, e) => AdvancePreview();
            }

            public string Title { get; set; } = string.Empty;
            public string Subtitle { get; set; } = string.Empty;
            public string EmptyText { get; set; } = "NO DATA TO DISPLAY YET.";
            public Color LineColor { get; set; } = Color.FromArgb(22, 163, 74);

            // A fixed maximum keeps the 1-5 rating scale honest (the axis always
            // runs from 0 to 5), while a step count of 5 keeps its ticks whole.
            public double MaximumValue { get; set; }
            public int AxisSteps { get; set; } = 4;
            public int ValueDecimals { get; set; }

            // Returns the picture shown beside the hovered point. When it is null the
            // chart keeps the classic behaviour and shows no hover card at all.
            public Func<ChartItem, Image?>? HoverImageProvider { get; set; }

            public void SetData(List<ChartItem> items)
            {
                _items = items ?? new List<ChartItem>();
                _points = Array.Empty<PointF>();
                _previewImages.Clear();
                HidePreview();
                Invalidate();
            }

            public int ItemCount
            {
                get { return _items.Count; }
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);

                if (HoverImageProvider == null || _points.Length == 0)
                {
                    return;
                }

                int index = HitTest(e.Location);

                if (index < 0)
                {
                    HidePreview();
                    return;
                }

                if (index == _previewIndex && !_previewHiding)
                {
                    return;
                }

                _previewIndex = index;
                _previewHiding = false;
                _previewImage = ResolvePreviewImage(_items[index]);
                DisposePreviewCard();
                _previewTimer.Start();
                Invalidate();
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);
                HidePreview();
            }

            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);
                DisposePreviewCard();
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _previewTimer.Stop();
                    _previewTimer.Dispose();
                    DisposePreviewCard();
                }

                base.Dispose(disposing);
            }

            /// <summary>
            /// The point closest to the mouse. Anywhere inside the plot counts, so the
            /// librarian does not have to land exactly on the 8 px marker; the column
            /// around the nearest point decides which book is previewed.
            /// </summary>
            private int HitTest(Point location)
            {
                if (_points.Length == 0 || !_plotArea.Contains(location))
                {
                    return -1;
                }

                int best = -1;
                float bestDistance = float.MaxValue;

                for (int index = 0; index < _points.Length; index++)
                {
                    float distance = Math.Abs(location.X - _points[index].X);

                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        best = index;
                    }
                }

                return bestDistance <= Math.Max(28f, _columnWidth) ? best : -1;
            }

            private Image? ResolvePreviewImage(ChartItem item)
            {
                Image? image;

                if (_previewImages.TryGetValue(item, out image))
                {
                    return image;
                }

                image = HoverImageProvider?.Invoke(item);
                _previewImages[item] = image;

                return image;
            }

            /// <summary>Starts (or finishes) the fade in or the fade out of the card.</summary>
            private void AdvancePreview()
            {
                _previewOpacity = _previewHiding
                    ? Math.Max(0f, _previewOpacity - 0.2f)
                    : Math.Min(1f, _previewOpacity + 0.16f);

                if (!_previewHiding && _previewOpacity >= 1f)
                {
                    _previewTimer.Stop();
                }
                else if (_previewHiding && _previewOpacity <= 0f)
                {
                    _previewTimer.Stop();
                    _previewIndex = -1;
                    _previewImage = null;
                    DisposePreviewCard();
                }

                Invalidate();
            }

            /// <summary>
            /// Fades the card out instead of removing it at once, so moving the mouse
            /// off a point is as smooth as moving onto it.
            /// </summary>
            private void HidePreview()
            {
                if (_previewIndex < 0 || _previewHiding)
                {
                    return;
                }

                _previewHiding = true;
                _previewTimer.Start();
                Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics graphics = e.Graphics;
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                graphics.Clear(BackColor);

                int left = Padding.Left;
                int right = Width - Padding.Right;
                int top = Padding.Top;

                using Font subtitleFont = new Font("Segoe UI", 8F);
                using Font tickFont = new Font("Segoe UI", 7.5F);
                using Font labelFont = new Font("Segoe UI", 7.25F);
                using Font valueFont = new Font("Segoe UI", 8F, FontStyle.Bold);

                DrawChartHeading(graphics, Title, Subtitle, LineColor, left, top, right);

                int plotTop = top + 60;
                int plotLeft = left + 38;
                int plotRight = right - 4;
                const int HorizontalLabelHeight = 34;

                if (plotRight - plotLeft < 70 ||
                    Height - Padding.Bottom - HorizontalLabelHeight - plotTop < 56)
                {
                    return;
                }

                if (_items.Count == 0)
                {
                    _points = Array.Empty<PointF>();

                    TextRenderer.DrawText(
                        graphics,
                        EmptyText,
                        subtitleFont,
                        new Rectangle(left, plotTop, Math.Max(40, right - left),
                            Math.Max(30, Height - Padding.Bottom - HorizontalLabelHeight - plotTop)),
                        Color.FromArgb(148, 163, 184),
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);

                    return;
                }

                double maximum = 0;

                foreach (ChartItem item in _items)
                {
                    maximum = Math.Max(maximum, item.Value);
                }

                double axisMax;
                int axisSteps;
                BuildAxis(maximum, out axisMax, out axisSteps);

                int count = _items.Count;
                double columnWidth = (plotRight - plotLeft) / (double)count;

                // When a flat row of category names would run into itself, the
                // labels are turned on an angle instead of being cut or overlapped,
                // so the chart stays readable at any window width.
                int widestLabel = MeasureWidestLabel();
                bool rotateLabels = count > 1 && widestLabel > columnWidth - 4;
                const float LabelAngle = -35F;
                double labelRadians = Math.Abs(LabelAngle) * Math.PI / 180.0;
                int labelHeight = HorizontalLabelHeight;

                if (rotateLabels)
                {
                    // The angled labels end at their point and run down to the
                    // left, so the plot gives up room on the left instead of
                    // letting the first name run under the card edge.
                    int leftSpan = (int)Math.Ceiling(Math.Cos(labelRadians) * widestLabel) + 6;
                    int extraLeft = Math.Max(0, leftSpan - (plotLeft - left));

                    if (extraLeft < (plotRight - plotLeft) - 160)
                    {
                        plotLeft += extraLeft;
                        columnWidth = (plotRight - plotLeft) / (double)count;
                    }

                    int needed = 18 + (int)Math.Ceiling(Math.Sin(labelRadians) * widestLabel);
                    int available = Math.Max(40, Height - Padding.Bottom - plotTop - 56);
                    labelHeight = Math.Min(available, Math.Min(110, needed));
                }

                int plotBottom = Height - Padding.Bottom - labelHeight;
                int plotHeight = plotBottom - plotTop;

                using (Pen gridPen = new Pen(Color.FromArgb(241, 245, 249), 1f))
                using (Pen baselinePen = new Pen(Color.FromArgb(226, 232, 240), 1.2f))
                {
                    for (int step = 0; step <= axisSteps; step++)
                    {
                        int y = plotBottom - (int)Math.Round(plotHeight * step / (double)axisSteps);

                        if (step > 0)
                        {
                            graphics.DrawLine(gridPen, plotLeft, y, plotRight, y);
                        }

                        TextRenderer.DrawText(
                            graphics,
                            FormatTick(axisMax * step / axisSteps),
                            tickFont,
                            new Rectangle(left, y - 9, plotLeft - left - 10, 18),
                            Color.FromArgb(148, 163, 184),
                            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                    }

                    graphics.DrawLine(baselinePen, plotLeft, plotBottom, plotRight, plotBottom);
                }

                PointF[] points = new PointF[count];

                for (int index = 0; index < count; index++)
                {
                    double value = Math.Max(0, _items[index].Value);
                    float x = (float)(plotLeft + (columnWidth * (index + 0.5)));
                    float y = (float)(plotBottom - (plotHeight * Math.Min(1.0, value / axisMax)));
                    points[index] = new PointF(x, y);
                }

                // The hover hit test reuses the geometry this paint just worked out,
                // so the picture always lands on the point it belongs to.
                _points = points;
                _plotArea = new Rectangle(plotLeft, plotTop, plotRight - plotLeft, plotBottom - plotTop);
                _columnWidth = (float)columnWidth;

                // Soft area under the curve, from the line colour to white.
                using (GraphicsPath area = new GraphicsPath())
                {
                    AddCurve(area, points);
                    area.AddLine(points[count - 1].X, plotBottom, points[0].X, plotBottom);
                    area.CloseFigure();

                    using LinearGradientBrush areaBrush = new LinearGradientBrush(
                        new Rectangle(plotLeft, plotTop, Math.Max(1, plotRight - plotLeft), Math.Max(1, plotHeight)),
                        Color.FromArgb(64, LineColor),
                        Color.FromArgb(6, LineColor),
                        LinearGradientMode.Vertical);

                    graphics.FillPath(areaBrush, area);
                }

                using (GraphicsPath curve = new GraphicsPath())
                {
                    AddCurve(curve, points);

                    using Pen linePen = new Pen(LineColor, 2.6f)
                    {
                        LineJoin = LineJoin.Round,
                        StartCap = LineCap.Round,
                        EndCap = LineCap.Round
                    };

                    graphics.DrawPath(linePen, curve);
                }

                using (Pen pointRing = new Pen(LineColor, 2.2f))
                using (SolidBrush pointFill = new SolidBrush(Color.White))
                {
                    // One value has no line to follow, so a dashed guideline shows
                    // its level across the whole plot instead of a lonely dot.
                    if (count == 1)
                    {
                        using Pen guidePen = new Pen(Color.FromArgb(110, LineColor), 1.2f)
                        {
                            DashStyle = DashStyle.Dash
                        };

                        graphics.DrawLine(guidePen, plotLeft, points[0].Y, plotRight, points[0].Y);
                    }

                    for (int index = 0; index < count; index++)
                    {
                        PointF point = points[index];
                        int valueTop = (int)Math.Round(point.Y) - 24;
                        int valueLeft = (int)Math.Round(point.X) - 30;

                        if (valueTop < plotTop - 4)
                        {
                            valueTop = (int)Math.Round(point.Y) + 8;
                        }

                        // Keep the value label inside the plot area, so the last
                        // point never pushes its number over the card edge.
                        valueLeft = Math.Min(Math.Max(valueLeft, left), Math.Max(left, right - 60));

                        TextRenderer.DrawText(
                            graphics,
                            FormatValue(_items[index].Value),
                            valueFont,
                            new Rectangle(valueLeft, valueTop, 60, 18),
                            Color.FromArgb(51, 65, 85),
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

                        graphics.FillEllipse(pointFill, point.X - 4.2f, point.Y - 4.2f, 8.4f, 8.4f);
                        graphics.DrawEllipse(pointRing, point.X - 4.2f, point.Y - 4.2f, 8.4f, 8.4f);
                    }
                }

                // The category names sit under their points and wrap onto two lines,
                // or turn on an angle when a flat row would not fit.
                if (rotateLabels)
                {
                    using Font angledFont = new Font("Segoe UI", 7.25F);
                    using SolidBrush angledBrush = new SolidBrush(Color.FromArgb(71, 85, 105));
                    using StringFormat angledFormat = new StringFormat(StringFormatFlags.NoWrap)
                    {
                        Alignment = StringAlignment.Far,
                        LineAlignment = StringAlignment.Near
                    };

                    for (int index = 0; index < count; index++)
                    {
                        GraphicsState state = graphics.Save();
                        graphics.TranslateTransform(points[index].X - 1, plotBottom + 8);
                        graphics.RotateTransform(LabelAngle);
                        graphics.DrawString(
                            _items[index].Label,
                            angledFont,
                            angledBrush,
                            new RectangleF(-300F, 0F, 300F, 18F),
                            angledFormat);
                        graphics.Restore(state);
                    }
                }
                else
                {
                    for (int index = 0; index < count; index++)
                    {
                        int labelX = (int)Math.Round(points[index].X - (columnWidth / 2)) + 1;
                        int labelWidth = Math.Max(24, (int)Math.Round(columnWidth) - 2);

                        TextRenderer.DrawText(
                            graphics,
                            _items[index].Label,
                            labelFont,
                            new Rectangle(labelX, plotBottom + 8, labelWidth, labelHeight - 8),
                            Color.FromArgb(71, 85, 105),
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.WordBreak);
                    }
                }

                // The hover picture is painted last so it floats above the curve,
                // its points and its labels instead of being hidden behind them.
                DrawPreview(graphics);
            }

            /// <summary>
            /// Paints the hover card: the picture on the left, the label and the value
            /// on the right. The whole card is one bitmap, so the fading alpha applies
            /// to the picture and the text together.
            /// </summary>
            private void DrawPreview(Graphics graphics)
            {
                if (_previewIndex < 0 || _previewIndex >= _items.Count ||
                    _previewIndex >= _points.Length || _previewOpacity <= 0.01f)
                {
                    return;
                }

                if (_previewCard == null)
                {
                    _previewCard = BuildPreviewCard(_items[_previewIndex], _previewImage);
                }

                int left = Padding.Left;
                int right = Width - Padding.Right;
                PointF point = _points[_previewIndex];

                // A soft halo on the hovered point ties the floating card to the
                // data it belongs to, so a wide chart never leaves the librarian
                // guessing which book is previewed.
                float haloRadius = 8f + (2f * _previewOpacity);

                using (SolidBrush halo = new SolidBrush(
                    Color.FromArgb((int)(44 * _previewOpacity), LineColor)))
                using (Pen haloRing = new Pen(
                    Color.FromArgb((int)(150 * _previewOpacity), LineColor), 1.6f))
                {
                    graphics.FillEllipse(
                        halo,
                        point.X - haloRadius,
                        point.Y - haloRadius,
                        haloRadius * 2,
                        haloRadius * 2);

                    graphics.DrawEllipse(
                        haloRing,
                        point.X - haloRadius,
                        point.Y - haloRadius,
                        haloRadius * 2,
                        haloRadius * 2);
                }

                // The card floats above its point and slides up a few pixels while it
                // fades in, which reads as a smooth hover instead of a pop.
                int x = (int)Math.Round(point.X - (_previewCard.Width / 2f));
                int y = (int)Math.Round(point.Y - _previewCard.Height - 18 - ((1f - _previewOpacity) * 6));

                if (y < 4)
                {
                    y = (int)Math.Round(point.Y + 18 + ((1f - _previewOpacity) * 6));
                }

                x = Math.Min(Math.Max(x, left), Math.Max(left, right - _previewCard.Width));

                using ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(
                    new ColorMatrix { Matrix33 = _previewOpacity },
                    ColorMatrixFlag.Default,
                    ColorAdjustType.Bitmap);

                graphics.DrawImage(
                    _previewCard,
                    new Rectangle(x, y, _previewCard.Width, _previewCard.Height),
                    0,
                    0,
                    _previewCard.Width,
                    _previewCard.Height,
                    GraphicsUnit.Pixel,
                    attributes);
            }

            private static Bitmap BuildPreviewCard(ChartItem item, Image? cover)
            {
                const int CoverWidth = 62;
                const int CoverHeight = 84;
                const int PaddingSize = 12;
                const int CardWidth = 280;

                using Font titleFont = new Font("Segoe UI", 8.25F, FontStyle.Bold);
                using Font countFont = new Font("Segoe UI", 8F, FontStyle.Bold);

                int textWidth = CardWidth - (PaddingSize * 3) - CoverWidth;

                // GDI+ measures the wrapped title itself, so the height the card is
                // built with is exactly the height the title is drawn with. Three
                // lines is the cap; a longer title ends with an ellipsis.
                int titleHeight;
                using (Bitmap scratch = new Bitmap(1, 1))
                using (Graphics measure = Graphics.FromImage(scratch))
                {
                    SizeF measured = measure.MeasureString(
                        item.Label, titleFont, new SizeF(textWidth, 600f));

                    titleHeight = (int)Math.Ceiling(Math.Min(
                        measured.Height,
                        titleFont.GetHeight(measure) * 3));
                }

                int contentHeight = Math.Max(CoverHeight, titleHeight + 6 + 16);
                int cardHeight = contentHeight + (PaddingSize * 2);

                Bitmap card = new Bitmap(CardWidth, cardHeight);

                using (Graphics graphics = Graphics.FromImage(card))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;

                    // Anti aliased grayscale text is used instead of ClearType: the
                    // card is composited with a fading alpha, and sub pixel colours
                    // would fringe while it fades in.
                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    graphics.Clear(Color.Transparent);

                    using (GraphicsPath body = DrawHelper.CreateRoundedPath(
                        new Rectangle(0, 0, CardWidth - 1, cardHeight - 1), 10))
                    {
                        using SolidBrush fill = new SolidBrush(Color.White);
                        using Pen edge = new Pen(Color.FromArgb(203, 213, 225), 1f);

                        graphics.FillPath(fill, body);
                        graphics.DrawPath(edge, body);
                    }

                    Rectangle coverBox = new Rectangle(
                        PaddingSize,
                        PaddingSize + ((contentHeight - CoverHeight) / 2),
                        CoverWidth,
                        CoverHeight);

                    if (cover != null)
                    {
                        // Book covers already arrive at the cover size, while the
                        // borrower avatar is square: fitting keeps both undistorted.
                        graphics.DrawImage(cover, FitInside(cover.Size, coverBox));
                    }

                    int textLeft = PaddingSize + CoverWidth + PaddingSize;

                    using (SolidBrush titleBrush = new SolidBrush(Color.FromArgb(15, 23, 42)))
                    using (StringFormat titleFormat = new StringFormat(StringFormatFlags.LineLimit)
                    {
                        Trimming = StringTrimming.EllipsisWord
                    })
                    {
                        graphics.DrawString(
                            item.Label,
                            titleFont,
                            titleBrush,
                            new RectangleF(textLeft, PaddingSize, textWidth, titleHeight + 2),
                            titleFormat);
                    }

                    string loans = ((long)Math.Round(item.Value)).ToString(CultureInfo.InvariantCulture) +
                                   (item.Value == 1 ? " BORROW" : " BORROWS");

                    // GDI+ text is used for both lines: TextRenderer draws GDI text
                    // that ghosts on the transparent card bitmap, while DrawString
                    // stays crisp and keeps the transparent rounded corners.
                    using (SolidBrush loansBrush = new SolidBrush(Color.FromArgb(37, 99, 235)))
                    {
                        graphics.DrawString(
                            loans,
                            countFont,
                            loansBrush,
                            new PointF(textLeft, PaddingSize + titleHeight + 6));
                    }
                }

                return card;
            }

            /// <summary>
            /// Centres a picture inside the card's picture box without stretching it,
            /// so the square borrower avatar and the portrait book cover both keep
            /// their true proportions.
            /// </summary>
            private static Rectangle FitInside(Size source, Rectangle box)
            {
                if (source.Width <= 0 || source.Height <= 0)
                {
                    return box;
                }

                double scale = Math.Min((double)box.Width / source.Width, (double)box.Height / source.Height);
                int width = Math.Max(1, (int)Math.Round(source.Width * scale));
                int height = Math.Max(1, (int)Math.Round(source.Height * scale));

                return new Rectangle(
                    box.X + ((box.Width - width) / 2),
                    box.Y + ((box.Height - height) / 2),
                    width,
                    height);
            }

            private void DisposePreviewCard()
            {
                if (_previewCard == null)
                {
                    return;
                }

                _previewCard.Dispose();
                _previewCard = null;
            }

            private int MeasureWidestLabel()
            {
                using Font measureFont = new Font("Segoe UI", 7.25F);
                int widest = 0;

                foreach (ChartItem item in _items)
                {
                    int width = TextRenderer.MeasureText(
                        item.Label,
                        measureFont,
                        new Size(int.MaxValue, 18),
                        TextFormatFlags.SingleLine | TextFormatFlags.NoPadding).Width;

                    widest = Math.Max(widest, width);
                }

                return widest;
            }

            private static void AddCurve(GraphicsPath path, PointF[] points)
            {
                if (points.Length == 1)
                {
                    // A single point has no line yet; the marker shows it.
                    path.AddLine(points[0].X - 0.5f, points[0].Y, points[0].X + 0.5f, points[0].Y);
                    return;
                }

                if (points.Length == 2)
                {
                    path.AddLine(points[0], points[1]);
                    return;
                }

                // Smooth Bezier curve whose control points are clamped inside the
                // two values they connect. A plain cardinal spline would dip under
                // the baseline between two low points, which would draw data that
                // does not exist.
                for (int index = 0; index < points.Length - 1; index++)
                {
                    PointF previous = index == 0 ? points[index] : points[index - 1];
                    PointF current = points[index];
                    PointF next = points[index + 1];
                    PointF after = index + 2 < points.Length ? points[index + 2] : next;

                    float firstX = current.X + ((next.X - previous.X) / 6F);
                    float firstY = current.Y + ((next.Y - previous.Y) / 6F);
                    float secondX = next.X - ((after.X - current.X) / 6F);
                    float secondY = next.Y - ((after.Y - current.Y) / 6F);

                    float low = Math.Min(current.Y, next.Y);
                    float high = Math.Max(current.Y, next.Y);

                    path.AddBezier(
                        current,
                        new PointF(firstX, Clamp(firstY, low, high)),
                        new PointF(secondX, Clamp(secondY, low, high)),
                        next);
                }
            }

            private static float Clamp(float value, float minimum, float maximum)
            {
                return value < minimum ? minimum : value > maximum ? maximum : value;
            }

            private string FormatValue(double value)
            {
                if (ValueDecimals <= 0)
                {
                    return ((long)Math.Round(value)).ToString(CultureInfo.InvariantCulture);
                }

                return value.ToString("0." + new string('0', Math.Min(4, ValueDecimals)),
                    CultureInfo.InvariantCulture);
            }

            private static string FormatTick(double value)
            {
                if (Math.Abs(value - Math.Round(value)) < 0.001)
                {
                    return ((long)Math.Round(value)).ToString(CultureInfo.InvariantCulture);
                }

                return value.ToString("0.##", CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// Chooses a round axis maximum (and a matching tick count) so the
            /// labels read 0 / 2 / 4 / 6 instead of 0 / 1.7 / 3.4 / 5.1.
            /// </summary>
            private void BuildAxis(double maximum, out double axisMax, out int axisSteps)
            {
                axisSteps = Math.Max(2, AxisSteps);

                if (MaximumValue > 0)
                {
                    axisMax = MaximumValue;
                    return;
                }

                if (maximum <= 0)
                {
                    axisMax = axisSteps;
                    return;
                }

                bool wholeNumbers = ValueDecimals <= 0;
                double bestMax = double.MaxValue;
                int bestSteps = axisSteps;

                for (int candidate = 3; candidate <= 6; candidate++)
                {
                    double step = NiceStep(maximum / candidate, wholeNumbers);
                    double candidateMax = step * candidate;

                    if (candidateMax < maximum - 0.0001 || candidateMax >= bestMax)
                    {
                        continue;
                    }

                    bestMax = candidateMax;
                    bestSteps = candidate;
                }

                if (bestMax >= double.MaxValue)
                {
                    bestMax = maximum;
                    bestSteps = axisSteps;
                }

                axisMax = bestMax;
                axisSteps = bestSteps;
            }

            private static double NiceStep(double raw, bool wholeNumbers)
            {
                if (raw <= 0)
                {
                    return 1;
                }

                double magnitude = Math.Pow(10, Math.Floor(Math.Log10(raw)));
                double normalized = raw / magnitude;
                double step;

                if (normalized <= 1)
                {
                    step = 1;
                }
                else if (normalized <= 2)
                {
                    step = 2;
                }
                else if (!wholeNumbers && normalized <= 2.5)
                {
                    step = 2.5;
                }
                else if (normalized <= 5)
                {
                    step = 5;
                }
                else
                {
                    step = 10;
                }

                step *= magnitude;

                if (wholeNumbers)
                {
                    step = Math.Max(1, Math.Ceiling(step));
                }

                return step;
            }
        }

        /// <summary>
        /// Donut chart drawn with GDI+ for the EVALUATION RESPONSES BY RESPONDENT
        /// card. The ring is split in proportion to each respondent group and the
        /// legend prints the count and percentage next to a colour dot, so the
        /// student / faculty / staff split can be read without a tooltip.
        /// </summary>
        private sealed class DonutChart : Control
        {
            private static readonly Color[] DefaultPalette =
            {
                Color.FromArgb(22, 163, 74),
                Color.FromArgb(37, 99, 235),
                Color.FromArgb(234, 88, 12),
                Color.FromArgb(147, 51, 234),
                Color.FromArgb(100, 116, 139)
            };

            private List<ChartItem> _items = new List<ChartItem>();

            public DonutChart()
            {
                SetStyle(ControlStyles.UserPaint |
                         ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw, true);

                BackColor = Color.White;
                Font = new Font("Segoe UI", 9F);
                Padding = new Padding(22, 18, 22, 18);
            }

            public string Title { get; set; } = string.Empty;
            public string Subtitle { get; set; } = string.Empty;
            public string EmptyText { get; set; } = "NO DATA TO DISPLAY YET.";
            public string CenterCaption { get; set; } = "TOTAL";
            public Color[] Palette { get; set; } = DefaultPalette;

            public void SetData(List<ChartItem> items)
            {
                _items = items ?? new List<ChartItem>();
                Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics graphics = e.Graphics;
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                graphics.Clear(BackColor);

                int left = Padding.Left + 6;
                int top = Padding.Top;

                using Font titleFont = new Font("Segoe UI", 11.5F, FontStyle.Bold);
                using Font subtitleFont = new Font("Segoe UI", 8.25F);
                using Font legendFont = new Font("Segoe UI", 8.5F);
                using Font legendValueFont = new Font("Segoe UI", 8.25F, FontStyle.Bold);
                using Font totalFont = new Font("Segoe UI", 17F, FontStyle.Bold);
                using Font totalCaptionFont = new Font("Segoe UI", 6.75F, FontStyle.Bold);

                TextRenderer.DrawText(graphics, Title, titleFont,
                    new Rectangle(left, top, Math.Max(40, Width - left - Padding.Right), 24),
                    Color.FromArgb(15, 23, 42),
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                TextRenderer.DrawText(graphics, Subtitle, subtitleFont,
                    new Rectangle(left, top + 26, Math.Max(40, Width - left - Padding.Right), 18),
                    Color.FromArgb(100, 116, 139),
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                int chartTop = top + 58;
                int chartHeight = Height - Padding.Bottom - chartTop;

                double total = 0;

                foreach (ChartItem item in _items)
                {
                    total += Math.Max(0, item.Value);
                }

                if (_items.Count == 0 || total <= 0 || chartHeight < 90)
                {
                    TextRenderer.DrawText(graphics, EmptyText, subtitleFont,
                        new Rectangle(left, chartTop, Math.Max(40, Width - left - Padding.Right),
                            Math.Max(24, chartHeight)),
                        Color.FromArgb(148, 163, 184),
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
                    return;
                }

                int availableWidth = Width - left - Padding.Right;
                int donutSize = Math.Min(chartHeight, Math.Max(90, (int)(availableWidth * 0.46)));
                int donutLeft = left + 4;

                Rectangle donut = new Rectangle(
                    donutLeft,
                    chartTop + ((chartHeight - donutSize) / 2),
                    donutSize,
                    donutSize);

                // On a very narrow window there is no room for the ring and the
                // legend side by side, so the legend is drawn on its own instead
                // of letting the two overlap.
                bool showRing = Width - Padding.Right - (donut.Right + 16) >= 110;

                if (showRing)
                {
                    float startAngle = -90F;

                    for (int index = 0; index < _items.Count; index++)
                    {
                        ChartItem item = _items[index];
                        double slice = Math.Max(0, item.Value);

                        if (slice <= 0)
                        {
                            continue;
                        }

                        float sweep = (float)(slice / total * 360.0);

                        using (SolidBrush brush = new SolidBrush(Palette[index % Palette.Length]))
                        {
                            graphics.FillPie(brush, donut, startAngle, sweep);
                        }

                        startAngle += sweep;
                    }

                    int innerSize = Math.Max(40, (int)(donutSize * 0.58));
                    Rectangle inner = new Rectangle(
                        donut.Left + ((donutSize - innerSize) / 2),
                        donut.Top + ((donutSize - innerSize) / 2),
                        innerSize,
                        innerSize);

                    using (SolidBrush hole = new SolidBrush(BackColor))
                    {
                        graphics.FillEllipse(hole, inner);
                    }

                    using (Pen ring = new Pen(Color.FromArgb(241, 245, 249), 1.4f))
                    {
                        graphics.DrawEllipse(ring, donut);
                    }

                    string totalText = ((long)Math.Round(total)).ToString(CultureInfo.InvariantCulture);

                    TextRenderer.DrawText(graphics, totalText, totalFont,
                        new Rectangle(inner.Left - 8, inner.Top + (innerSize / 2) - 26, innerSize + 16, 32),
                        Color.FromArgb(15, 23, 42),
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

                    TextRenderer.DrawText(graphics, CenterCaption, totalCaptionFont,
                        new Rectangle(inner.Left - 8, inner.Top + (innerSize / 2) + 6, innerSize + 16, 18),
                        Color.FromArgb(100, 116, 139),
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.NoPadding);
                }

                int legendLeft = showRing ? donut.Right + 16 : left;
                int legendWidth = showRing ? Width - Padding.Right - legendLeft : availableWidth;

                int rowHeight = Math.Min(38, Math.Max(26, chartHeight / Math.Max(1, _items.Count)));
                int legendTop = chartTop + Math.Max(0, (chartHeight - (rowHeight * _items.Count)) / 2);

                for (int index = 0; index < _items.Count; index++)
                {
                    ChartItem item = _items[index];
                    Color color = Palette[index % Palette.Length];
                    int rowTop = legendTop + (index * rowHeight);

                    using (GraphicsPath dot = oop_project.DrawHelper.CreateRoundedPath(
                               new Rectangle(legendLeft, rowTop + 4, 10, 10), 3))
                    using (SolidBrush dotBrush = new SolidBrush(color))
                    {
                        graphics.FillPath(dotBrush, dot);
                    }

                    string caption = item.Label;
                    double percent = total <= 0 ? 0 : (Math.Max(0, item.Value) / total) * 100.0;
                    string value = ((long)Math.Round(item.Value)).ToString(CultureInfo.InvariantCulture) +
                                   "  \u2022  " + percent.ToString("0", CultureInfo.InvariantCulture) + "%";

                    TextRenderer.DrawText(graphics, caption, legendFont,
                        new Rectangle(legendLeft + 16, rowTop, Math.Max(20, legendWidth - 16), 18),
                        Color.FromArgb(51, 65, 85),
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                    TextRenderer.DrawText(graphics, value, legendValueFont,
                        new Rectangle(legendLeft + 16, rowTop + 16, Math.Max(20, legendWidth - 16), 18),
                        Color.FromArgb(100, 116, 139),
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                }
            }
        }

        private void logoutButton_Click(object sender, EventArgs e)
        {
            Welcome welcomeForm = new Welcome();
            welcomeForm.Show();
            Close();
        }
    }
}
