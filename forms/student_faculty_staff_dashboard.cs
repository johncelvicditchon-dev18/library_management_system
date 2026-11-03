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
    /// The single portal used by library patrons: students, faculty and staff.
    ///
    /// The page follows the LRC reference screen: the account identity sits in
    /// the green banner, the three jobs a borrower needs are one click away in
    /// the navigation bar, and the catalog itself is a grid of book cards with
    /// the green REQUEST TO BORROW button under every cover.
    ///
    /// The complete layout is serialized in patron_dashboard.designer.cs so the
    /// Windows Forms designer renders the whole portal; the constructors only apply
    /// the role and account identity (<see cref="ApplyIdentity"/>) and wire the
    /// interactive behaviour (<see cref="WireEvents"/>).
    /// </summary>
    public partial class student_faculty_staff_dashboard : Form
    {
        private static readonly Color PageBack = Color.FromArgb(241, 245, 249);
        private static readonly Color CardBorder = Color.FromArgb(226, 232, 240);
        private static readonly Color HeaderGreen = Color.FromArgb(6, 78, 59);
        private static readonly Color qrCodeColor = Color.FromArgb(0, 0, 0); // black
        private static readonly Color MutedText = Color.FromArgb(100, 116, 139);
        private static readonly Color SuccessText = Color.FromArgb(4, 120, 87);
        private static readonly Color WarningText = Color.FromArgb(180, 83, 9);
        private static readonly Color DangerText = Color.FromArgb(185, 28, 28);
        private static readonly Font RoleCellFont = new Font("Segoe UI", 8.75F, FontStyle.Bold);

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

        private readonly List<BookCard> _bookCards = new List<BookCard>();
        private readonly List<RequestCard> _requestCards = new List<RequestCard>();
        private readonly List<LoanCard> _loanCards = new List<LoanCard>();
        private readonly List<Label> _profileValueLabels = new List<Label>();

        // MY ATTENDANCE is shown as a table so every scanned visit lists the full
        // account record (name, role, school id, program / office and institute).
        private readonly DataGridView _attendanceTable = new DataGridView();

        /// <summary>
        /// Design time constructor. The complete portal layout is serialized in
        /// InitializeComponent, so this only applies the sample identity and wires the
        /// interactive behaviour the design surface does not need.
        /// </summary>
        public student_faculty_staff_dashboard()
        {
            _accountId = 0;
            _fullName = "PATRON";
            _role = "STUDENT";
            _username = string.Empty;
            _status = "ACTIVE";

            InitializeComponent();
            ApplyIdentity();
            WireEvents();
            ShowPage(0);
        }

        public student_faculty_staff_dashboard(int accountID, string fullName, string role, string username = "", string status = "")
        {
            _accountId = accountID;
            _role = string.IsNullOrWhiteSpace(role) ? "STUDENT" : role.Trim().ToUpperInvariant();
            _fullName = string.IsNullOrWhiteSpace(fullName) ? PortalName() : fullName.Trim();
            _username = (username ?? string.Empty).Trim();
            _status = string.IsNullOrWhiteSpace(status) ? "ACTIVE" : status.Trim().ToUpperInvariant();

            InitializeComponent();
            ApplyIdentity();
            WireEvents();
            ShowPage(0);
            StartRuntime();
        }

        /// <summary>Writes the role and account identity into the portal chrome.</summary>
        private void ApplyIdentity()
        {
            Text = "TCGC - " + PortalName() + " DASHBOARD";
            qrHeading.Text = PortalName() + " ATTENDANCE QR CODE";
            _headerName.Text = _fullName.ToUpperInvariant();
            _headerRole.Text = _role;
            _profileName.Text = _fullName;
            _profileMeta.Text = _role + (_username.Length > 0 ? "     \u2022     @" + _username : string.Empty);
            _profileRole.Text = _role;
            _headerAvatar.Initials = Initials(_fullName);
            _profileAvatar.Initials = Initials(_fullName);
        }

        /// <summary>Hooks the interactions that used to live inside the layout builders.</summary>
        private void WireEvents()
        {
            BuildAttendanceTable();

            _profileValueLabels.Clear();
            _profileValueLabels.Add(_schoolIdValue);
            _profileValueLabels.Add(_programValue);
            _profileValueLabels.Add(_yearValue);
            _profileValueLabels.Add(_instituteValue);
            _profileValueLabels.Add(_contactValue);
            _profileValueLabels.Add(_addressValue);

            _logout.Click += SignOut_Click;

            headerPanel.Resize += (s, e) => LayoutHeaderCluster();
            _tabs.SelectedIndexChanged += (s, e) => OnTabSelected(_tabs.SelectedIndex);

            WirePageHead(catalogHead, _catalogSearch, catalogRefresh, catalogSubtitle);
            WirePageHead(requestsHead, _requestsSearch, requestsRefresh);
            WirePageHead(loansHead, _loansSearch, loansRefresh);
            WirePageHead(attendanceHead, _attendanceSearch, attendanceRefresh, attendanceSubtitle);
            WirePageHead(profileHead, null, profileRefresh, profileSubtitle);

            catalogRefresh.Click += (s, e) => { LoadCatalogData(); LoadRequests(); LoadLoans(); };
            requestsRefresh.Click += (s, e) => LoadRequests();
            loansRefresh.Click += (s, e) => { LoadCatalogData(); LoadLoans(); };
            attendanceRefresh.Click += (s, e) => LoadAttendance();
            profileRefresh.Click += (s, e) => LoadProfile();

            _catalogSearch.SearchTextChanged += (s, e) => { if (!_layingOutCards) RebuildCatalogCards(); };
            _catalogSearch.SearchRequested += (s, e) => RebuildCatalogCards();
            _requestsSearch.SearchTextChanged += (s, e) => { if (!_layingOutCards) RebuildRequestCards(); };
            _requestsSearch.SearchRequested += (s, e) => RebuildRequestCards();
            _loansSearch.SearchTextChanged += (s, e) => { if (!_layingOutCards) RebuildLoanCards(); };
            _loansSearch.SearchRequested += (s, e) => RebuildLoanCards();
            _attendanceSearch.SearchTextChanged += (s, e) => { if (!_layingOutCards) RebuildAttendanceCards(); };
            _attendanceSearch.SearchRequested += (s, e) => RebuildAttendanceCards();

            _catalogFlow.ClientSizeChanged += (s, e) => RelayoutCards(_catalogFlow, true);
            _requestFlow.ClientSizeChanged += (s, e) => RelayoutCards(_requestFlow, false);
            _loanFlow.ClientSizeChanged += (s, e) => RelayoutCards(_loanFlow, false);
            _attendanceFlow.ClientSizeChanged += (s, e) => RelayoutCards(_attendanceFlow, false);

            catalogStatRow.Resize += (s, e) => LayoutStatRow(catalogStatRow);
            requestsStatRow.Resize += (s, e) => LayoutStatRow(requestsStatRow);
            loansStatRow.Resize += (s, e) => LayoutStatRow(loansStatRow);
            attendanceStatRow.Resize += (s, e) => LayoutStatRow(attendanceStatRow);

            profileIdentityCard.Paint += PaintRoundedCard;
            profileQrCard.Paint += PaintRoundedCard;
            profileIdentityCard.Resize += (s, e) => LayoutIdentityCard();
            profileQrCard.Resize += (s, e) => LayoutQrCard();

            // A scan can happen in the scanner window while the patron is on this
            // page, so the MY ATTENDANCE table is refreshed whenever the portal is
            // brought back to the front with that tab open.
            Activated += (s, e) =>
            {
                if (_tabs.SelectedIndex == 1)
                {
                    LoadAttendance();
                }
            };
        }

        private void WirePageHead(Panel requestsHead, SearchBox requestsSearch, RoundedButton requestsRefresh)
        {
            // MY REQUESTS and MY BORROWED BOOKS have no subtitle line, so only the
            // search box and the refresh button are aligned in their header.
            requestsHead.Resize += (s, e) => LayoutPageHead(requestsHead, requestsSearch, requestsRefresh, null);
            LayoutPageHead(requestsHead, requestsSearch, requestsRefresh, null);
        }

        /// <summary>
        /// Builds the MY ATTENDANCE table once. It is docked over the card flow in
        /// the attendance page and shows one row for every recorded visit.
        /// </summary>
        private void BuildAttendanceTable()
        {
            _attendanceTable.Name = "attendanceTable";
            _attendanceTable.Dock = DockStyle.Fill;
            _attendanceTable.BackgroundColor = Color.White;
            _attendanceTable.BorderStyle = BorderStyle.None;
            _attendanceTable.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            _attendanceTable.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            _attendanceTable.ReadOnly = true;
            _attendanceTable.AllowUserToAddRows = false;
            _attendanceTable.AllowUserToDeleteRows = false;
            _attendanceTable.AllowUserToResizeRows = false;
            _attendanceTable.RowHeadersVisible = false;
            _attendanceTable.MultiSelect = false;
            _attendanceTable.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _attendanceTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _attendanceTable.EnableHeadersVisualStyles = false;
            _attendanceTable.GridColor = Color.FromArgb(241, 245, 249);
            _attendanceTable.ColumnHeadersHeight = 44;
            _attendanceTable.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            _attendanceTable.ColumnHeadersDefaultCellStyle.BackColor = HeaderGreen;
            _attendanceTable.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _attendanceTable.ColumnHeadersDefaultCellStyle.SelectionBackColor = HeaderGreen;
            _attendanceTable.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;
            _attendanceTable.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.75F, FontStyle.Bold);
            _attendanceTable.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            _attendanceTable.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            _attendanceTable.DefaultCellStyle.BackColor = Color.White;
            _attendanceTable.DefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
            _attendanceTable.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            _attendanceTable.DefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            _attendanceTable.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 252, 231);
            _attendanceTable.DefaultCellStyle.SelectionForeColor = HeaderGreen;
            _attendanceTable.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            _attendanceTable.RowTemplate.Height = 38;
            _attendanceTable.Visible = false;

            AddAttendanceColumn("attendanceNumber", "ATTENDANCE #", 9F, 90);
            AddAttendanceColumn("attendanceStamp", "DATE & TIME", 24F, 160);
            AddAttendanceColumn("attendanceName", "NAME", 30F, 150);
            AddAttendanceColumn("attendanceRole", "ROLE", 12F, 90);
            AddAttendanceColumn("attendanceProgram", "PROGRAM / OFFICE", 20F, 120);
            AddAttendanceColumn("attendanceInstitute", "INSTITUTE", 13F, 100);

            attendanceListHost.Controls.Add(_attendanceTable);
            _attendanceTable.BringToFront();
        }

        private void AddAttendanceColumn(string name, string header, float weight, int minimumWidth)
        {
            DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = header,
                FillWeight = weight,
                MinimumWidth = minimumWidth,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            };

            _attendanceTable.Columns.Add(column);
        }

        private static void WirePageHead(Panel head, SearchBox? search, RoundedButton refresh, Label? subtitle)
        {
            head.Resize += (s, e) => LayoutPageHead(head, search, refresh, subtitle);
            LayoutPageHead(head, search, refresh, subtitle);
        }

        private static void LayoutPageHead(Panel head, SearchBox? search, RoundedButton refresh, Label? subtitle)
        {
            int right = head.ClientSize.Width;
            refresh.Left = Math.Max(0, right - refresh.Width);
            refresh.Top = 10;

            if (search != null)
            {
                search.Left = Math.Max(0, refresh.Left - 12 - search.Width);
                search.Top = 10;
            }

            if (subtitle != null)
            {
                subtitle.Width = Math.Max(160, (search == null ? refresh.Left : search.Left) - 16);
            }
        }

        private void LayoutHeaderCluster()
        {
            int right = headerPanel.ClientSize.Width - 28;
            _logout.Left = right - _logout.Width;
            _headerName.Width = Math.Max(180, _logout.Left - 140);
        }

        private void PaintRoundedCard(object? sender, PaintEventArgs e)
        {
            CardPaint.DrawSurface(e.Graphics, ((Control)sender!).Size, PageBack, Color.White, CardBorder);
        }

        private void LayoutIdentityCard()
        {
            int width = Math.Max(200, profileIdentityCard.ClientSize.Width - 52);
            _profileName.Left = 26;
            _profileName.Width = width;
            _profileMeta.Left = 26;
            _profileMeta.Width = width;
            _profileAvatar.Left = 26 + Math.Max(0, (width - _profileAvatar.Width) / 2);
            _profileRole.Left = 26 + Math.Max(0, (width - _profileRole.Width) / 2);
            identityDivider.Width = width;

            foreach (Label value in _profileValueLabels)
            {
                value.Width = Math.Max(80, width - 138);
            }
        }

        private void LayoutQrCard()
        {
            int width = Math.Max(120, profileQrCard.ClientSize.Width - 52);
            bool wide = width >= 700;

            if (wide)
            {
                _qrPicture.Left = 26;
                _qrCaption.Left = 26;
                _qrCaption.Width = 260;
                qrDivider.SetBounds(312, 64, 1, 320);
                qrHowTo.Left = 340;
                qrStepOne.SetBounds(340, 92, width - 314, 20);
                qrStepTwo.SetBounds(340, 118, width - 314, 20);
                qrStepThree.SetBounds(340, 144, width - 314, 20);
                qrKeepPrivate.SetBounds(340, 186, width - 314, 40);
            }
            else
            {
                _qrPicture.Left = 26 + Math.Max(0, (width - _qrPicture.Width) / 2);
                _qrCaption.Left = 26;
                _qrCaption.Width = width;
                qrDivider.SetBounds(26, 384, width, 1);
                qrHowTo.Left = 26;
                qrStepOne.SetBounds(26, 404, width, 20);
                qrStepTwo.SetBounds(26, 430, width, 20);
                qrStepThree.SetBounds(26, 456, width, 20);
                qrKeepPrivate.SetBounds(26, 494, width, 40);
            }
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

        private void ShowPage(int index)
        {
            if (_tabs.SelectedIndex == index)
            {
                OnTabSelected(index);
            }
            else
            {
                _tabs.SelectedIndex = index;
            }
        }

        /// <summary>
        /// Reacts to a tab becoming active. The attendance log changes at the
        /// entrance, so it is refreshed every time the patron opens the tab instead
        /// of only once when the form loads.
        /// </summary>
        private void OnTabSelected(int index)
        {
            // Tab order: 0 MY PROFILE, 1 MY ATTENDANCE, 2 BOOK CATALOG,
            // 3 MY REQUESTS, 4 MY BORROWED BOOKS.
            if (index == 1)
            {
                LoadAttendance();
            }
        }

        /// <summary>
        /// Applies the runtime window mode and hooks the data loading that has to
        /// wait until the window is shown.
        /// </summary>
        private void StartRuntime()
        {
            ScreenMode.Apply(this);
            Shown += PatronDashboard_Shown;
        }

        /// <summary>The role name used in the title, subtitle and QR code.</summary>
        private string PortalName()
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
            _profileMeta.Text = role.ToUpperInvariant() +
                                (_username.Length > 0 ? "     \u2022     @" + _username : string.Empty);
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
                    _qrImage = code.GetGraphic(6, qrCodeColor, Color.White, true);
                    _qrPicture.Image = _qrImage;
                }

                _qrCaption.Text = "SCAN THIS QR CODE AT THE ENTRANCE TO RECORD THE VISIT OF " +
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

                Book? requestBook = _catalog.Books.FirstOrDefault(book => book.BookId == request.BookId);
                RequestCard card = new RequestCard(request, requestBook?.CoverImage);
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
               
            }
            else
            {
                int pending = _studentRequests.Count(r => r.IsPending);
                int approved = _studentRequests.Count(r => r.IsApproved);
                int closed = _studentRequests.Count - pending - approved;

            }

          
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
             
                _attendanceTable.Rows.Clear();
                _attendanceTable.Visible = false;
                _attendanceEmpty.Visible = false;
                UpdateStats();
                return;
            }

            RebuildAttendanceCards();
        }

        /// <summary>
        /// Fills the MY ATTENDANCE table. Every recorded scan carries the full
        /// account record returned by the attendance query, so the table shows the
        /// name, role, school id, program / office, institute, gate and time.
        /// </summary>
        private void RebuildAttendanceCards()
        {
            string term = _attendanceSearch.SearchText.ToUpperInvariant();
            int shown = 0;

            _attendanceTable.Rows.Clear();

            foreach (AttendanceRow visit in _myAttendance)
            {
                if (term.Length > 0 && !MatchesAttendance(visit, term))
                {
                    continue;
                }

                int index = _attendanceTable.Rows.Add(
                    visit.AttendanceId,
                    visit.StampText,
                    visit.Name,
                    visit.Role,
                    visit.ProgramOrOffice,
                    visit.Institute);

                DataGridViewRow row = _attendanceTable.Rows[index];
                row.Cells["attendanceRole"].Style.Font = RoleCellFont;
                row.Cells["attendanceRole"].Style.ForeColor = HeaderGreen;
                row.Cells["attendanceNumber"].Style.ForeColor = MutedText;
                shown++;
            }

            bool hasRows = shown > 0;

            _attendanceTable.Visible = hasRows;
            _attendanceEmpty.Visible = !hasRows;

            if (hasRows)
            {
                _attendanceTable.ClearSelection();
                _attendanceTable.CurrentCell = null;
                _attendanceTable.BringToFront();
            }
            else
            {
                _attendanceEmpty.Title = term.Length > 0 ? "NO MATCHING VISITS" : "NO ATTENDANCE RECORDED YET";
                _attendanceEmpty.Caption = term.Length > 0
                    ? "TRY ANOTHER DATE, TIME, NAME OR ROLE."
                    : "SHOW YOUR QR PASS TO THE LIBRARIAN AT THE LIBRARY OFFICE ENTRANCE.";
                _attendanceEmpty.BringToFront();
            }

            if (_myAttendance.Count == 0)
            {
               
            }
            else
            {
                int today = _myAttendance.Count(visit => visit.IsToday);
                int month = _myAttendance.Count(visit => visit.IsThisMonth);

               
            }

            UpdateStats();
        }

        private static bool MatchesAttendance(AttendanceRow visit, string term)
        {
            // Matches only the columns the MY ATTENDANCE table actually shows.
            return visit.Name.ToUpperInvariant().Contains(term) ||
                   visit.Role.Contains(term) ||
                   visit.ProgramOrOffice.ToUpperInvariant().Contains(term) ||
                   visit.Institute.ToUpperInvariant().Contains(term) ||
                   visit.DateText.Contains(term) ||
                   visit.TimeText.Contains(term);
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

            _tabs.TabPages[3].Text = "MY REQUESTS" + (pending > 0 ? "  (" + pending.ToString(CultureInfo.InvariantCulture) + ")" : string.Empty);
            _tabs.TabPages[4].Text = "MY BORROWED BOOKS" + (activeLoans > 0 ? "  (" + activeLoans.ToString(CultureInfo.InvariantCulture) + ")" : string.Empty);
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
            return string.IsNullOrWhiteSpace(value) || value == "-" ? "-" : value.Trim();
        }

        private static string CleanValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value) || value == "-" ? string.Empty : value.Trim();
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

        private void _catalogStatPending_Click(object sender, EventArgs e)
        {

        }
    }
}
