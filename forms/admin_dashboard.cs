using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using oop_project.Models;

namespace oop_project.forms
{
    public partial class admin_dashboard : Form
    {
        private static readonly Color HeaderGreen = Color.FromArgb(6, 78, 59);
        private static readonly Color MutedText = Color.FromArgb(148, 163, 184);
        private static readonly Color SuccessText = Color.FromArgb(4, 120, 87);
        private static readonly Color WarningText = Color.FromArgb(194, 65, 12);
        private static readonly Color DangerText = Color.FromArgb(185, 28, 28);

        private static readonly Font StatusCellFont = new Font("Segoe UI", 8.75F, FontStyle.Bold);

        // The EDIT / DELETE column of every table shows one Segoe MDL2 Assets glyph,
        // so the two options are icons instead of words.
        private const string EditColumnName = "editActionColumn";
        private const string DeleteColumnName = "deleteActionColumn";
        private const string AccessColumnName = "accessActionColumn";
        private const string BookCoverColumnName = "bookCoverColumn";
        private const string RequestApproveColumnName = "requestApproveColumn";
        private const string RequestRejectColumnName = "requestRejectColumn";
        private const string EditGlyph = "\uE70F";
        private const string DeleteGlyph = "\uE74D";
        private const string AccessGlyph = "\uE7E8";
        private static readonly Font IconCellFont = new Font("Segoe MDL2 Assets", 11.5F);

        private readonly LibraryCatalog _catalog = new LibraryCatalog();
        private readonly AccountDirectory _accountDirectory = new AccountDirectory();
        private readonly BorrowRequestDirectory _requestDirectory = new BorrowRequestDirectory();
        private List<AccountRow> _accounts = new List<AccountRow>();
        private bool _accountsLoaded;
        private bool _canManageAccounts;

        private DataGridViewImageColumn bookCoverColumn = null!;

        // BORROW REQUESTS tab, built in code so the large designer file stays as it is.
        private TabPage requestsTab = null!;
        private DataGridView requestsGrid = null!;
        private DataGridViewTextBoxColumn requestIdColumn = null!;
        private DataGridViewTextBoxColumn requestBookColumn = null!;
        private DataGridViewTextBoxColumn requestStudentColumn = null!;
        private DataGridViewTextBoxColumn requestStudentIdColumn = null!;
        private DataGridViewTextBoxColumn requestProgramColumn = null!;
        private DataGridViewTextBoxColumn requestDateColumn = null!;
        private DataGridViewTextBoxColumn requestStatusColumn = null!;
        private Label requestsStatusLabel = null!;
        private RoundedButton requestsRefreshButton = null!;

        // STAFF ACCOUNTS tab, built in code so the large designer file stays as it is.
        private TabPage staffTab = null!;
        private DataGridView staffGrid = null!;
        private DataGridViewTextBoxColumn staffIdColumn = null!;
        private DataGridViewTextBoxColumn staffNameColumn = null!;
        private DataGridViewTextBoxColumn staffUsernameColumn = null!;
        private DataGridViewTextBoxColumn staffPasswordColumn = null!;
        private DataGridViewTextBoxColumn staffStatusColumn = null!;
        private DataGridViewTextBoxColumn staffRoleColumn = null!;
        private Label staffStatusLabel = null!;
        private Label staffEmptyLabel = null!;
        private RoundedButton staffActivateButton = null!;
        private RoundedButton staffDeactivateButton = null!;
        private RoundedButton staffRefreshButton = null!;

        public int LoggedInAccountId { get; private set; }
        public string LoggedInRole { get; private set; } = string.Empty;
        public string LoggedInFullName { get; private set; } = string.Empty;
        public string LoggedInUsername { get; private set; } = string.Empty;
        public string LoggedInStatus { get; private set; } = string.Empty;

        public admin_dashboard(int accountID, string fullName, string role, string username = "", string status = "")
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
            CreateBorrowRequestsTab();
            CreateStaffAccountsTab();
            ConfigureGrids();

            tabControl1.SelectedIndexChanged += tabControl1_SelectedIndexChanged;
        }

        public admin_dashboard()
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

            profileMeta.Text = "ACCOUNT ID  #" + LoggedInAccountId +
                (LoggedInUsername.Length > 0 ? "     •     @" + LoggedInUsername : string.Empty);

            valueFullName.Text = LoggedInFullName;
            valueUsername.Text = LoggedInUsername.Length > 0 ? LoggedInUsername : "—";
            valueRole.Text = ToDisplayCase(LoggedInRole);
            valueAccountId.Text = "#" + LoggedInAccountId;
            valueStatus.Text = LoggedInStatus.Length > 0 ? ToDisplayCase(LoggedInStatus) : "—";

            bool isActive = string.Equals(LoggedInStatus, "ACTIVE", StringComparison.OrdinalIgnoreCase);
            valueStatus.ForeColor = isActive ? SuccessText : Color.FromArgb(180, 83, 9);

            LayoutHeaderCluster();
            LayoutIdentityBadge();
            ApplyAccountPermissions();
        }

        /// <summary>
        /// Only the library staff (admin / officer / librarian) may approve or
        /// deactivate an account; a student login sees a read only ACCOUNTS tab.
        /// </summary>
        private static bool IsStaffRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                return false;
            }

            string value = role.ToUpperInvariant();

            return value.Contains("ADMIN") || value.Contains("OFFICER") ||
                   value.Contains("LIBRARIAN") || value.Contains("STAFF");
        }

        private void ApplyAccountPermissions()
        {
            RoundedButton[] buttons =
            {
                accountsActivateButton,
                accountsDeactivateButton,
                facultyActivateButton,
                facultyDeactivateButton,
                studentActivateButton,
                studentDeactivateButton
            };

            foreach (RoundedButton button in buttons)
            {
                button.Enabled = _canManageAccounts;
            }

            // Only library staff may change the catalog or the loans. Students and
            // faculty still see the BOOKS and BORROWED BOOKS tabs, read only.
            addBookButton.Enabled = _canManageAccounts;
            borrowBookButton.Enabled = _canManageAccounts;
            recordBorrowButton.Enabled = _canManageAccounts;
            markReturnedButton.Enabled = _canManageAccounts;
        }

        /// <summary>
        /// The pending student request queue. A student can only request a book;
        /// this tab is where a librarian turns a request into a real loan or
        /// rejects it. It is built in code next to the fixed designer tabs.
        /// </summary>
        private void CreateBorrowRequestsTab()
        {
            if (requestsTab != null)
            {
                return;
            }

            requestsTab = new TabPage();
            requestsTab.BackColor = Color.White;
            requestsTab.Location = new Point(4, 29);
            requestsTab.Name = "requestsTab";
            requestsTab.Padding = new Padding(3);
            requestsTab.Size = new Size(1344, 655);
            requestsTab.Text = "BORROW REQUESTS";

            Label heading = new Label();
            heading.AutoSize = true;
            heading.BackColor = Color.Transparent;
            heading.Font = new Font("Segoe UI", 14.5F, FontStyle.Bold);
            heading.ForeColor = Color.FromArgb(15, 23, 42);
            heading.Location = new Point(30, 22);
            heading.Name = "requestsHeading";
            heading.TabIndex = 0;
            heading.Text = "BORROW REQUESTS";

            Label subtitle = new Label();
            subtitle.AutoSize = true;
            subtitle.BackColor = Color.Transparent;
            subtitle.Font = new Font("Segoe UI", 9F);
            subtitle.ForeColor = Color.FromArgb(148, 163, 184);
            subtitle.Location = new Point(32, 56);
            subtitle.Name = "requestsSubtitle";
            subtitle.TabIndex = 1;
            subtitle.Text = "Student requests waiting for approval.";

            requestsRefreshButton = CreateTabButton(
                "requestsRefreshButton", "REFRESH REQUESTS", new Point(30, 92), new Size(188, 44),
                Color.FromArgb(22, 163, 74), Color.FromArgb(21, 128, 61), Color.FromArgb(20, 83, 45));
            requestsRefreshButton.Click += delegate { LoadBorrowRequests(); };

            requestsStatusLabel = new Label();
            requestsStatusLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            requestsStatusLabel.BackColor = Color.Transparent;
            requestsStatusLabel.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            requestsStatusLabel.ForeColor = Color.FromArgb(148, 163, 184);
            requestsStatusLabel.Location = new Point(630, 104);
            requestsStatusLabel.Name = "requestsStatusLabel";
            requestsStatusLabel.Size = new Size(684, 20);
            requestsStatusLabel.TabIndex = 3;
            requestsStatusLabel.Text = "REQUESTS HAVE NOT BEEN LOADED YET.";
            requestsStatusLabel.TextAlign = ContentAlignment.MiddleRight;

            requestsGrid = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)requestsGrid).BeginInit();
            requestsGrid.AllowUserToAddRows = false;
            requestsGrid.AllowUserToDeleteRows = false;
            requestsGrid.AllowUserToResizeRows = false;
            requestsGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            requestsGrid.BackgroundColor = Color.White;
            requestsGrid.BorderStyle = BorderStyle.None;
            requestsGrid.ColumnHeadersHeight = 44;
            requestsGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            requestsGrid.Location = new Point(30, 156);
            requestsGrid.MultiSelect = false;
            requestsGrid.Name = "requestsGrid";
            requestsGrid.ReadOnly = true;
            requestsGrid.RowHeadersVisible = false;
            requestsGrid.RowHeadersWidth = 51;
            requestsGrid.RowTemplate.Height = 38;
            requestsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            requestsGrid.Size = new Size(1284, 469);
            requestsGrid.TabIndex = 4;

            requestIdColumn = CreateAccountColumn("requestIdColumn", "ID", 60, 0F, 50);
            requestBookColumn = CreateAccountColumn("requestBookColumn", "BOOK TITLE", 0, 22F, 150);
            requestStudentColumn = CreateAccountColumn("requestStudentColumn", "STUDENT", 0, 20F, 150);
            requestStudentIdColumn = CreateAccountColumn("requestStudentIdColumn", "ID NO.", 112, 0F, 90);
            requestProgramColumn = CreateAccountColumn("requestProgramColumn", "PROGRAM", 0, 12F, 95);
            requestDateColumn = CreateAccountColumn("requestDateColumn", "REQUESTED", 148, 0F, 135);
            requestStatusColumn = CreateAccountColumn("requestStatusColumn", "STATUS", 104, 0F, 90);

            DataGridViewTextBoxColumn approveColumn = CreateRequestActionColumn(
                RequestApproveColumnName, "APPROVE", 108, Color.FromArgb(4, 120, 87));
            DataGridViewTextBoxColumn rejectColumn = CreateRequestActionColumn(
                RequestRejectColumnName, "REJECT", 92, Color.FromArgb(185, 28, 28));

            requestsGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                requestIdColumn, requestBookColumn, requestStudentColumn, requestStudentIdColumn,
                requestProgramColumn, requestDateColumn, requestStatusColumn, approveColumn, rejectColumn
            });

            requestsGrid.CellClick += RequestsGrid_CellClick;

            requestsTab.Controls.Add(requestsGrid);
            requestsTab.Controls.Add(requestsStatusLabel);
            requestsTab.Controls.Add(requestsRefreshButton);
            requestsTab.Controls.Add(subtitle);
            requestsTab.Controls.Add(heading);

            tabControl1.TabPages.Insert(3, requestsTab);
            ((System.ComponentModel.ISupportInitialize)requestsGrid).EndInit();
        }

        private static DataGridViewTextBoxColumn CreateRequestActionColumn(
            string name, string text, int width, Color color)
        {
            DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
            column.Name = name;
            column.HeaderText = text;
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            column.Width = width;
            column.MinimumWidth = width - 10;
            column.ReadOnly = true;
            column.SortMode = DataGridViewColumnSortMode.NotSortable;
            column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            column.DefaultCellStyle.Font = new Font("Segoe UI", 8.75F, FontStyle.Bold);
            column.DefaultCellStyle.ForeColor = color;
            column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            column.HeaderCell.Style.WrapMode = DataGridViewTriState.False;
            return column;
        }

        /// <summary>
        /// STAFF ACCOUNTS gets its own table next to LIBRARIAN ACCOUNTS, so the two
        /// library roles are no longer mixed in one grid. The tab mirrors the
        /// designer layout of the librarian tab but is created here, which keeps the
        /// large designer file untouched.
        /// </summary>
        private void CreateStaffAccountsTab()
        {
            if (staffTab != null)
            {
                return;
            }

            staffTab = new TabPage();
            staffTab.BackColor = Color.White;
            staffTab.Location = new Point(4, 29);
            staffTab.Name = "staffTab";
            staffTab.Padding = new Padding(3);
            staffTab.Size = new Size(1344, 655);
            staffTab.Text = "STAFF ACCOUNTS";

            Label heading = new Label();
            heading.AutoSize = true;
            heading.BackColor = Color.Transparent;
            heading.Font = new Font("Segoe UI", 14.5F, FontStyle.Bold);
            heading.ForeColor = Color.FromArgb(15, 23, 42);
            heading.Location = new Point(30, 22);
            heading.Name = "staffHeading";
            heading.TabIndex = 0;
            heading.Text = "STAFF ACCOUNTS";

            Label subtitle = new Label();
            subtitle.AutoSize = true;
            subtitle.BackColor = Color.Transparent;
            subtitle.Font = new Font("Segoe UI", 9F);
            subtitle.ForeColor = Color.FromArgb(148, 163, 184);
            subtitle.Location = new Point(32, 56);
            subtitle.Name = "staffSubtitle";
            subtitle.TabIndex = 1;
            subtitle.Text = "Staff accounts that support the library.";

            staffActivateButton = CreateTabButton(
                "staffActivateButton", "ACTIVATE ACCOUNT", new Point(30, 92), new Size(176, 44),
                Color.FromArgb(22, 163, 74), Color.FromArgb(21, 128, 61), Color.FromArgb(20, 83, 45));
            staffActivateButton.Click += staffActivateButton_Click;

            staffDeactivateButton = CreateTabButton(
                "staffDeactivateButton", "DEACTIVATE ACCOUNT", new Point(218, 92), new Size(186, 44),
                Color.FromArgb(185, 28, 28), Color.FromArgb(153, 27, 27), Color.FromArgb(127, 29, 29));
            staffDeactivateButton.Click += staffDeactivateButton_Click;

            staffRefreshButton = CreateTabButton(
                "staffRefreshButton", "REFRESH", new Point(416, 92), new Size(126, 44),
                Color.FromArgb(22, 163, 74), Color.FromArgb(21, 128, 61), Color.FromArgb(20, 83, 45));
            staffRefreshButton.Click += staffRefreshButton_Click;

            staffStatusLabel = new Label();
            staffStatusLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            staffStatusLabel.BackColor = Color.Transparent;
            staffStatusLabel.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            staffStatusLabel.ForeColor = Color.FromArgb(148, 163, 184);
            staffStatusLabel.Location = new Point(566, 104);
            staffStatusLabel.Name = "staffStatusLabel";
            staffStatusLabel.Size = new Size(748, 20);
            staffStatusLabel.TabIndex = 3;
            staffStatusLabel.Text = "STAFF ACCOUNTS HAVE NOT BEEN LOADED YET.";
            staffStatusLabel.TextAlign = ContentAlignment.MiddleRight;

            staffEmptyLabel = new Label();
            staffEmptyLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            staffEmptyLabel.BackColor = Color.White;
            staffEmptyLabel.Font = new Font("Segoe UI", 10F);
            staffEmptyLabel.ForeColor = Color.FromArgb(148, 163, 184);
            staffEmptyLabel.Location = new Point(30, 396);
            staffEmptyLabel.Name = "staffEmptyLabel";
            staffEmptyLabel.Size = new Size(1284, 60);
            staffEmptyLabel.TabIndex = 5;
            staffEmptyLabel.Text = "NO STAFF ACCOUNTS HAVE BEEN REGISTERED YET.";
            staffEmptyLabel.TextAlign = ContentAlignment.MiddleCenter;
            staffEmptyLabel.Visible = false;

            staffGrid = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)staffGrid).BeginInit();
            staffGrid.AllowUserToAddRows = false;
            staffGrid.AllowUserToDeleteRows = false;
            staffGrid.AllowUserToResizeRows = false;
            staffGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            staffGrid.BackgroundColor = Color.White;
            staffGrid.BorderStyle = BorderStyle.None;
            staffGrid.ColumnHeadersHeight = 44;
            staffGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            staffGrid.Location = new Point(30, 156);
            staffGrid.MultiSelect = false;
            staffGrid.Name = "staffGrid";
            staffGrid.ReadOnly = true;
            staffGrid.RowHeadersVisible = false;
            staffGrid.RowHeadersWidth = 51;
            staffGrid.RowTemplate.Height = 38;
            staffGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            staffGrid.Size = new Size(1284, 469);
            staffGrid.TabIndex = 4;

            staffIdColumn = CreateAccountColumn("staffIdColumn", "ID", 70, 0F, 6);
            staffNameColumn = CreateAccountColumn("staffNameColumn", "NAME", 0, 28F, 120);
            staffUsernameColumn = CreateAccountColumn("staffUsernameColumn", "USERNAME", 0, 22F, 80);
            staffPasswordColumn = CreateAccountColumn("staffPasswordColumn", "PASSWORD", 130, 0F, 6);
            staffStatusColumn = CreateAccountColumn("staffStatusColumn", "STATUS", 120, 0F, 6);
            staffRoleColumn = CreateAccountColumn("staffRoleColumn", "ROLE", 120, 0F, 6);

            staffGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                staffIdColumn, staffNameColumn, staffUsernameColumn, staffPasswordColumn,
                staffStatusColumn, staffRoleColumn
            });

            staffTab.Controls.Add(staffEmptyLabel);
            staffTab.Controls.Add(staffGrid);
            staffTab.Controls.Add(staffStatusLabel);
            staffTab.Controls.Add(staffRefreshButton);
            staffTab.Controls.Add(staffDeactivateButton);
            staffTab.Controls.Add(staffActivateButton);
            staffTab.Controls.Add(subtitle);
            staffTab.Controls.Add(heading);

            int librarianIndex = tabControl1.TabPages.IndexOf(tabPage2);
            tabControl1.TabPages.Insert(librarianIndex < 0 ? 1 : librarianIndex + 1, staffTab);
            ((System.ComponentModel.ISupportInitialize)staffGrid).EndInit();
        }

        private static DataGridViewTextBoxColumn CreateAccountColumn(
            string name, string header, int width, float fillWeight, int minimumWidth)
        {
            DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
            column.Name = name;
            column.HeaderText = header;
            column.ReadOnly = true;
            column.MinimumWidth = minimumWidth;

            if (fillWeight > 0F)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                column.FillWeight = fillWeight;
            }
            else
            {
                column.Width = width;
            }

            return column;
        }

        private static RoundedButton CreateTabButton(
            string name, string text, Point location, Size size, Color normal, Color hover, Color pressed)
        {
            RoundedButton button = new RoundedButton();
            button.BackColor = Color.Transparent;
            button.CornerRadius = 12;
            button.FlatAppearance.BorderSize = 0;
            button.FlatStyle = FlatStyle.Flat;
            button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            button.ForeColor = Color.White;
            button.HoverColor = hover;
            button.Location = location;
            button.Name = name;
            button.NormalColor = normal;
            button.PressedColor = pressed;
            button.Size = size;
            button.Text = text;
            button.UseVisualStyleBackColor = false;
            return button;
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

            BalanceGridWidths();
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
            BalanceColumn(loanTitleColumn, 18F, 150);
            BalanceColumn(year, 7F, 65);
            BalanceColumn(loanCopyColumn, 10F, 105);
            BalanceColumn(program_, 10F, 90);
            BalanceColumn(institute, 11F, 120);
            BalanceColumn(studentid, 10F, 95);
            BalanceColumn(loanDateColumn, 9F, 95);
            BalanceColumn(loanDaysColumn, 5F, 50);
            BalanceColumn(loanReturnedColumn, 9F, 90);
            BalanceColumn(loanStatusColumn, 8F, 80);

            requestsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            BalanceColumn(requestIdColumn, 5F, 50);
            BalanceColumn(requestBookColumn, 22F, 150);
            BalanceColumn(requestStudentColumn, 20F, 150);
            BalanceColumn(requestStudentIdColumn, 10F, 90);
            BalanceColumn(requestProgramColumn, 12F, 95);
            BalanceColumn(requestDateColumn, 14F, 135);
            BalanceColumn(requestStatusColumn, 9F, 90);

            accountsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            BalanceColumn(accountIdColumn, 6F, 60);
            BalanceColumn(accountNameColumn, 28F, 150);
            BalanceColumn(accountUsernameColumn, 18F, 110);
            BalanceColumn(accountPasswordColumn, 16F, 110);
            BalanceColumn(accountStatusColumn, 16F, 100);
            BalanceColumn(accountRoleColumn, 16F, 100);

            staffGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            BalanceColumn(staffIdColumn, 6F, 60);
            BalanceColumn(staffNameColumn, 28F, 150);
            BalanceColumn(staffUsernameColumn, 18F, 110);
            BalanceColumn(staffPasswordColumn, 16F, 110);
            BalanceColumn(staffStatusColumn, 16F, 100);
            BalanceColumn(staffRoleColumn, 16F, 100);

            facultyGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            BalanceColumn(facultyIdColumn, 6F, 60);
            BalanceColumn(facultyNameColumn, 24F, 140);
            BalanceColumn(facultyUsernameColumn, 18F, 110);
            BalanceColumn(facultyProgramColumn, 22F, 120);
            BalanceColumn(facultyInstituteColumn, 18F, 110);
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
            DisposeBookCoverImages();
            booksGrid.Rows.Clear();

            foreach (Book book in _catalog.Books)
            {
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
            }

            attendanceGrid.Rows.Clear();

            foreach (BorrowRecord record in _catalog.BorrowRecords)
            {
                int index = attendanceGrid.Rows.Add(
                    record.BorrowId,
                    record.BookTitle,
                    record.BookYearText,
                    record.BorrowedCopyText,
                    record.ProgramText,
                    record.InstituteText,
                    record.BorrowerIdNumber,
                    record.DateBorrowedText,
                    record.DaysBorrowedText,
                    record.DateReturnedText,
                    record.Status,
                    EditGlyph,
                    DeleteGlyph);

                attendanceGrid.Rows[index].Tag = record;
                attendanceGrid.Rows[index].Cells[loanDateColumn.Index].ToolTipText =
                    "BORROWED " + record.DateBorrowedStampText;
                attendanceGrid.Rows[index].Cells[loanDaysColumn.Index].ToolTipText =
                    record.DaysBorrowedText + " SINCE " + record.DateBorrowedStampText;
            }

            ColorBorrowStatuses();

            bookStatTotalValue.Text = _catalog.TotalBooks.ToString(CultureInfo.InvariantCulture);
            bookStatAvailableValue.Text = _catalog.AvailableBooks.ToString(CultureInfo.InvariantCulture);
            bookStatBorrowedValue.Text = _catalog.BorrowedBooks.ToString(CultureInfo.InvariantCulture);
            bookStatLoansValue.Text = _catalog.BorrowRecords.Count.ToString(CultureInfo.InvariantCulture);

            booksGrid.ClearSelection();
            attendanceGrid.ClearSelection();
            booksGrid.CurrentCell = null;
            attendanceGrid.CurrentCell = null;

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
            string readOnly = _canManageAccounts ? string.Empty : " • READ ONLY";

            booksStatusLabel.Text = connected
                ? "CATALOG SYNCED WITH MYSQL" + readOnly
                : "MYSQL OFFLINE • CHANGES ARE KEPT FOR THIS SESSION ONLY";
            booksStatusLabel.ForeColor = connected ? MutedText : WarningText;

            attendanceStatusLabel.Text = connected
                ? "NO DUE DATE IS SET • ONE DAY IS ADDED EVERY 24 HOURS." + readOnly
                : "MYSQL OFFLINE • CHANGES ARE KEPT FOR THIS SESSION ONLY";
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
                List<BorrowRequestRow> pending = _requestDirectory.LoadPending();
                requestsGrid.Rows.Clear();

                foreach (BorrowRequestRow request in pending)
                {
                    int index = requestsGrid.Rows.Add(
                        request.RequestId,
                        request.BookTitle,
                        request.StudentName,
                        request.StudentIdText,
                        request.ProgramText,
                        request.RequestDateText,
                        request.Status,
                        "APPROVE",
                        "REJECT");

                    DataGridViewRow row = requestsGrid.Rows[index];
                    row.Tag = request;
                    row.Cells[requestStatusColumn.Index].Style.ForeColor = Color.FromArgb(180, 83, 9);
                    row.Cells[requestStatusColumn.Index].Style.Font = StatusCellFont;
                    row.Cells[RequestApproveColumnName].ToolTipText =
                        "Approve this request and create the borrow record.";
                    row.Cells[RequestRejectColumnName].ToolTipText =
                        "Reject this request without creating a loan.";
                }

                requestsStatusLabel.ForeColor = MutedText;
                requestsStatusLabel.Text = pending.Count == 0
                    ? "NO PENDING REQUESTS. STUDENT REQUESTS WILL APPEAR HERE."
                    : pending.Count.ToString(CultureInfo.InvariantCulture) +
                      " PENDING REQUEST(S). CLICK APPROVE OR REJECT ON A ROW.";

                ClearGridSelection(requestsGrid);
            }
            catch (MySqlException)
            {
                requestsGrid.Rows.Clear();
                requestsStatusLabel.Text = "MYSQL OFFLINE • REQUESTS CANNOT BE LOADED";
                requestsStatusLabel.ForeColor = WarningText;
            }
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

            if (!approve && !reject)
            {
                return;
            }

            if (!_canManageAccounts)
            {
                MessageBox.Show(
                    "ONLY LIBRARY STAFF ACCOUNTS CAN APPROVE OR REJECT BORROW REQUESTS.",
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

            if (approve)
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
            DialogResult answer = MessageBox.Show(
                "APPROVE REQUEST #" + request.RequestId + "?\n\n" +
                request.StudentName.ToUpperInvariant() + " WILL BORROW \"" +
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
                BorrowRecord record = _requestDirectory.Approve(
                    request, LoggedInAccountId, _catalog, _accountDirectory);

                _catalog.SyncDaysBorrowed();
                RefreshCatalogViews();
                ApplyCatalogStatus();
                LoadBorrowRequests();

                requestsStatusLabel.Text = "REQUEST #" + request.RequestId +
                                           " APPROVED. LOAN #" + record.BorrowId + " WAS CREATED.";
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
                _requestDirectory.Reject(request, LoggedInAccountId, "Rejected by the librarian.");
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
                BorrowRecord? record = row.Tag as BorrowRecord;

                if (record == null)
                {
                    return;
                }

                if (edit)
                {
                    EditBorrowRecord(record);
                }
                else
                {
                    DeleteBorrowRecord(record);
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

        private void EditBorrowRecord(BorrowRecord record)
        {
            using (EditBorrowRecordDialog dialog = new EditBorrowRecordDialog(record, _catalog.Books))
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
                    // validates them before the two tables are updated.
                    Borrower borrower = new Borrower(
                        dialog.BorrowerName,
                        dialog.BorrowerIdNumber,
                        dialog.BorrowerProgram,
                        dialog.BorrowerYearLevel,
                        dialog.BorrowerInstitute);

                    _catalog.UpdateBorrowRecord(record, book, borrower, dialog.DateBorrowed, dialog.ReturnedOn);
                    _catalog.SyncDaysBorrowed();
                    RefreshCatalogViews();
                    ApplyCatalogStatus();
                    ShowCatalogWarning();

                    attendanceStatusLabel.Text = "BORROW RECORD #" + record.BorrowId + " WAS UPDATED.";
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

        private void DeleteBorrowRecord(BorrowRecord record)
        {
            DialogResult answer = MessageBox.Show(
                "DELETE BORROW RECORD #" + record.BorrowId + " (" + record.BorrowerName.ToUpperInvariant() + ")?\n" +
                "THE BOOK COUNTS AS AVAILABLE AGAIN. THIS CANNOT BE UNDONE.",
                "CONFIRM DELETE RECORD",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (answer != DialogResult.Yes)
            {
                return;
            }

            try
            {
                _catalog.DeleteBorrowRecord(record);
                _catalog.SyncDaysBorrowed();
                RefreshCatalogViews();
                ApplyCatalogStatus();
                ShowCatalogWarning();

                attendanceStatusLabel.Text = "BORROW RECORD #" + record.BorrowId + " WAS DELETED.";
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

                accountsStatusLabel.Text = "MYSQL OFFLINE • ACCOUNTS CANNOT BE LOADED";
                staffStatusLabel.Text = "MYSQL OFFLINE • ACCOUNTS CANNOT BE LOADED";
                facultyStatusLabel.Text = "MYSQL OFFLINE • ACCOUNTS CANNOT BE LOADED";
                studentStatusLabel.Text = "MYSQL OFFLINE • ACCOUNTS CANNOT BE LOADED";

                accountsStatusLabel.ForeColor = WarningText;
                staffStatusLabel.ForeColor = WarningText;
                facultyStatusLabel.ForeColor = WarningText;
                studentStatusLabel.ForeColor = WarningText;

                return;
            }

            _accountsLoaded = true;

            accountsGrid.Rows.Clear();
            staffGrid.Rows.Clear();
            facultyGrid.Rows.Clear();
            studentGrid.Rows.Clear();

            int librarianCount = 0;
            int staffCount = 0;
            int facultyCount = 0;
            int studentCount = 0;

            foreach (AccountRow row in _accounts)
            {
                if (row.IsStudent)
                {
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
                    int index = facultyGrid.Rows.Add(
                        row.AccountId,
                        row.Name,
                        row.Username,
                        row.Program,
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
                    int index = accountsGrid.Rows.Add(
                        row.AccountId,
                        row.Name,
                        row.Username,
                        "••••••",
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
                    int index = staffGrid.Rows.Add(
                        row.AccountId,
                        row.Name,
                        row.Username,
                        "••••••",
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

            accountsStatusLabel.Text = AccountSummary(librarianCount, "LIBRARIAN ACCOUNT");
            staffStatusLabel.Text = AccountSummary(staffCount, "STAFF ACCOUNT");
            facultyStatusLabel.Text = AccountSummary(facultyCount, "FACULTY ACCOUNT");
            studentStatusLabel.Text = AccountSummary(studentCount, "STUDENT ACCOUNT");

            accountsStatusLabel.ForeColor = MutedText;
            staffStatusLabel.ForeColor = MutedText;
            facultyStatusLabel.ForeColor = MutedText;
            studentStatusLabel.ForeColor = MutedText;

            ClearGridSelection(accountsGrid);
            ClearGridSelection(staffGrid);
            ClearGridSelection(facultyGrid);
            ClearGridSelection(studentGrid);

            UpdateEmptyStates();
        }

        private string AccountSummary(int count, string label)
        {
            if (count == 0)
            {
                return "NO " + label + "S HAVE BEEN REGISTERED YET.";
            }

            string countText = count.ToString(CultureInfo.InvariantCulture) + " " + label + "(S) ON FILE.";

            return _canManageAccounts
                ? countText + " SELECT A ROW, THEN ACTIVATE OR DEACTIVATE."
                : countText + " READ ONLY • ONLY LIBRARY STAFF CAN MANAGE ACCOUNTS.";
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
                    "THERE ARE NO AVAILABLE BOOKS TO BORROW.\nADD A BOOK TO THE CATALOG FIRST, OR MARK A LOAN AS RETURNED.",
                    "NO BOOKS AVAILABLE",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            using (BorrowBookDialog dialog = new BorrowBookDialog(available))
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
                        dialog.BorrowerName,
                        dialog.BorrowerIdNumber,
                        dialog.BorrowerProgram,
                        dialog.BorrowerYearLevel,
                        dialog.BorrowerInstitute);

                    _catalog.RecordBorrow(dialog.SelectedBook, borrower, dialog.DateBorrowed);
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

        private void markReturnedButton_Click(object? sender, EventArgs e)
        {
            if (attendanceGrid.CurrentRow == null || !(attendanceGrid.CurrentRow.Tag is BorrowRecord record))
            {
                MessageBox.Show(
                    "SELECT A BORROW RECORD IN THE TABLE FIRST.",
                    "NO RECORD SELECTED",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (record.DateReturned.HasValue)
            {
                MessageBox.Show(
                    "THAT RECORD IS ALREADY MARKED AS RETURNED.",
                    "ALREADY RETURNED",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            DialogResult answer = MessageBox.Show(
                "MARK \"" + record.BookTitle.ToUpperInvariant() + "\" AS RETURNED TODAY?",
                "CONFIRM RETURN",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (answer != DialogResult.Yes)
            {
                return;
            }

            _catalog.MarkReturned(record);
            _catalog.SyncDaysBorrowed();
            RefreshCatalogViews();
            ApplyCatalogStatus();
            ShowCatalogWarning();
        }

        private void generateReportButton_Click(object? sender, EventArgs e)
        {
            using (LibraryReportDialog dialog = new LibraryReportDialog(_catalog, LoggedInFullName))
            {
                dialog.ShowDialog(this);
            }
        }

        private void accountsRefreshButton_Click(object? sender, EventArgs e)
        {
            LoadAccounts();
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

        private void accountsActivateButton_Click(object? sender, EventArgs e)
        {
            ActivateSelected(accountsGrid);
        }

        private void accountsDeactivateButton_Click(object? sender, EventArgs e)
        {
            DeactivateSelected(accountsGrid);
        }

        private void staffActivateButton_Click(object? sender, EventArgs e)
        {
            ActivateSelected(staffGrid);
        }

        private void staffDeactivateButton_Click(object? sender, EventArgs e)
        {
            DeactivateSelected(staffGrid);
        }

        private void facultyActivateButton_Click(object? sender, EventArgs e)
        {
            ActivateSelected(facultyGrid);
        }

        private void facultyDeactivateButton_Click(object? sender, EventArgs e)
        {
            DeactivateSelected(facultyGrid);
        }

        private void studentActivateButton_Click(object? sender, EventArgs e)
        {
            ActivateSelected(studentGrid);
        }

        private void studentDeactivateButton_Click(object? sender, EventArgs e)
        {
            DeactivateSelected(studentGrid);
        }

        private void ActivateSelected(DataGridView grid)
        {
            AccountRow? row = SelectedAccountRow(grid);

            if (row == null)
            {
                return;
            }

            if (row.IsActive)
            {
                MessageBox.Show(
                    row.Name.ToUpperInvariant() + " IS ALREADY ACTIVE.",
                    "ACCOUNT ALREADY ACTIVE",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            UpdateAccountStatus(
                grid,
                row,
                "ACTIVE",
                "ACTIVATE \"" + row.Name.ToUpperInvariant() + "\"?\nTHEY WILL BE ABLE TO SIGN IN TO THE SYSTEM.");
        }

        private void DeactivateSelected(DataGridView grid)
        {
            AccountRow? row = SelectedAccountRow(grid);

            if (row == null)
            {
                return;
            }

            if (!row.IsActive && string.Equals(row.Status, "INACTIVE", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    row.Name.ToUpperInvariant() + " IS ALREADY DEACTIVATED.",
                    "ACCOUNT ALREADY INACTIVE",
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

        private AccountRow? SelectedAccountRow(DataGridView grid)
        {
            DataGridViewRow? current = grid.CurrentRow;

            if (current == null || !(current.Tag is AccountRow row))
            {
                MessageBox.Show(
                    "SELECT AN ACCOUNT IN THE TABLE FIRST.",
                    "NO ACCOUNT SELECTED",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return null;
            }

            return row;
        }

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

            bool isStudent = row.IsStudent;
            bool isFaculty = row.IsFaculty;

            bool showRecord = (isStudent || isFaculty) && row.HasProfile;
            recordDivider.Visible = showRecord;
            recordHeading.Visible = showRecord;

            recordHeading.Text = isStudent ? "STUDENT RECORD" : "FACULTY RECORD";

            recordSchoolIdValue.Text = row.SchoolId;
            recordProgramValue.Text = row.Program;
            recordYearValue.Text = row.YearLevel;
            recordInstituteValue.Text = row.Institute;
            recordContactValue.Text = row.ContactNumber;

            HideRecordRows();

            if (isStudent)
            {
                StackRecordRows(
                    new[] { recordSchoolIdCaption, recordProgramCaption, recordYearCaption, recordInstituteCaption, recordContactCaption },
                    new[] { recordSchoolIdValue, recordProgramValue, recordYearValue, recordInstituteValue, recordContactValue });
            }
            else if (isFaculty)
            {
                StackRecordRows(
                    new[] { recordProgramCaption, recordInstituteCaption },
                    new[] { recordProgramValue, recordInstituteValue });
            }
        }

        // Only the rows a role actually has are shown, without leaving blank gaps.
        private void StackRecordRows(Label[] captions, Label[] values)
        {
            int top = recordHeading.Bottom + 6;

            for (int i = 0; i < captions.Length; i++)
            {
                captions[i].Top = top;
                values[i].Top = top;
                captions[i].Visible = true;
                values[i].Visible = true;
                top += 28;
            }
        }

        private void HideRecordRows()
        {
            Label[] labels =
            {
                recordSchoolIdCaption, recordSchoolIdValue,
                recordProgramCaption, recordProgramValue,
                recordYearCaption, recordYearValue,
                recordInstituteCaption, recordInstituteValue,
                recordContactCaption, recordContactValue
            };

            foreach (Label label in labels)
            {
                label.Visible = false;
            }
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

        private void LayoutIdentityBadge()
        {
            if (identityCard == null || profileRoleBadge == null)
            {
                return;
            }

            profileRoleBadge.Left = (identityCard.ClientSize.Width - profileRoleBadge.Width) / 2;
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

        private static string ToDisplayCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "—";
            }

            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLowerInvariant());
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutHeaderCluster();
            LayoutIdentityBadge();
        }

        private void logoutButton_Click(object sender, EventArgs e)
        {
            Welcome welcomeForm = new Welcome();
            welcomeForm.Show();
            Close();
        }
    }
}
