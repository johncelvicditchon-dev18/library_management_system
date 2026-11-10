using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using oop_project.Models;
using oop_project.Reports;
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

        private readonly LibraryCatalog _catalog = new LibraryCatalog();
        private readonly AccountDirectory _directory = new AccountDirectory();
        private readonly BorrowRequestDirectory _requestDirectory = new BorrowRequestDirectory();
        private readonly AttendanceDirectory _attendanceDirectory = new AttendanceDirectory();

        private readonly int _accountId;
        private string _fullName;
        private readonly string _role;
        private string _username;
        private readonly string _status;

        private AccountRow? _profile;
        private List<BorrowRequestRow> _studentRequests = new List<BorrowRequestRow>();
        private List<AttendanceRow> _myAttendance = new List<AttendanceRow>();
        private Image? _qrImage;
        private bool _layingOutCards;

        private readonly List<BookCard> _bookCards = new List<BookCard>();
        private readonly List<RequestCard> _requestCards = new List<RequestCard>();
        private readonly List<BorrowedBookCard> _borrowedCards = new List<BorrowedBookCard>();
        private readonly List<Label> _profileValueLabels = new List<Label>();

        // MY ATTENDANCE is shown as a table so every scanned visit lists the full
        // account record (name, school id, program / office and institute) with the
        // date and time of the scan.
        private readonly DataGridView _attendanceTable = new DataGridView();
        private RoundedButton attendanceExport = null!;

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
            WirePageHead(borrowedHead, _borrowedSearch, borrowedRefresh);
            WirePageHead(attendanceHead, _attendanceSearch, attendanceRefresh, attendanceSubtitle);

            BuildAttendanceExportButton();
            attendanceHead.Resize += (s, e) => LayoutAttendanceExport();
            LayoutAttendanceExport();

            catalogRefresh.Click += (s, e) => { LoadCatalogData(); LoadRequests(); LoadBorrowedBooks(); };
            requestsRefresh.Click += (s, e) => LoadRequests();
            borrowedRefresh.Click += (s, e) => { LoadCatalogData(); LoadBorrowedBooks(); };
            attendanceRefresh.Click += (s, e) => LoadAttendance();
            changePasswordButton.Click += (s, e) => ShowChangePasswordDialog();
            editProfileButton.Click += (s, e) => ShowEditProfileDialog();
            profileHead.Resize += (s, e) => LayoutProfileActions();
            LayoutProfileActions();

            _catalogSearch.SearchTextChanged += (s, e) => { if (!_layingOutCards) RebuildCatalogCards(); };
            _catalogSearch.SearchRequested += (s, e) => RebuildCatalogCards();
            _requestsSearch.SearchTextChanged += (s, e) => { if (!_layingOutCards) RebuildRequestCards(); };
            _requestsSearch.SearchRequested += (s, e) => RebuildRequestCards();
            _borrowedSearch.SearchTextChanged += (s, e) => { if (!_layingOutCards) RebuildBorrowedCards(); };
            _borrowedSearch.SearchRequested += (s, e) => RebuildBorrowedCards();
            _attendanceSearch.SearchTextChanged += (s, e) => { if (!_layingOutCards) RebuildAttendanceCards(); };
            _attendanceSearch.SearchRequested += (s, e) => RebuildAttendanceCards();

            _catalogFlow.ClientSizeChanged += (s, e) => RelayoutCards(_catalogFlow, true);
            _requestFlow.ClientSizeChanged += (s, e) => RelayoutCards(_requestFlow, false);
            _borrowedFlow.ClientSizeChanged += (s, e) => RelayoutCards(_borrowedFlow, false);
            _attendanceFlow.ClientSizeChanged += (s, e) => RelayoutCards(_attendanceFlow, false);

            catalogStatRow.Resize += (s, e) => LayoutStatRow(catalogStatRow);
            requestsStatRow.Resize += (s, e) => LayoutStatRow(requestsStatRow);
            borrowedStatRow.Resize += (s, e) => LayoutStatRow(borrowedStatRow);
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

            AddAttendanceColumn("attendanceNumber", "ATTENDANCE #", 10F, 90);
            AddAttendanceColumn("attendanceStamp", "DATE & TIME", 26F, 160);
            AddAttendanceColumn("attendanceName", "NAME", 32F, 150);
            AddAttendanceColumn("attendanceProgram", "PROGRAM / OFFICE", 22F, 120);
            AddAttendanceColumn("attendanceInstitute", "INSTITUTE", 14F, 100);

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

        /// <summary>
        /// MY ATTENDANCE: creates the EXPORT PDF action beside REFRESH and wires it
        /// to the patron's own attendance report.
        /// </summary>
        private void BuildAttendanceExportButton()
        {
            attendanceExport = new RoundedButton
            {
                BackColor = Color.Transparent,
                CornerRadius = 11,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.White,
                HoverColor = Color.FromArgb(154, 52, 18),
                NormalColor = Color.FromArgb(194, 65, 12),
                PressedColor = Color.FromArgb(120, 45, 15),
                Size = new Size(132, 44),
                Text = "EXPORT PDF",
                UseVisualStyleBackColor = false
            };
            attendanceExport.FlatAppearance.BorderSize = 0;
            attendanceExport.Click += (s, e) => ExportAttendancePdf();

            attendanceHead.Controls.Add(attendanceExport);
            attendanceExport.BringToFront();
        }

        /// <summary>
        /// Keeps the EXPORT PDF button next to REFRESH and shifts the search box and
        /// subtitle left so the header never overlaps.
        /// </summary>
        private void LayoutAttendanceExport()
        {
            if (attendanceExport == null)
            {
                return;
            }

            int right = attendanceHead.ClientSize.Width;

            attendanceExport.Left = Math.Max(
                0, right - attendanceRefresh.Width - 12 - attendanceExport.Width);
            attendanceExport.Top = attendanceRefresh.Top;

            _attendanceSearch.Left = Math.Max(0, attendanceExport.Left - 12 - _attendanceSearch.Width);
            _attendanceSearch.Top = 10;

            attendanceSubtitle.Width = Math.Max(160, _attendanceSearch.Left - 16);
        }

        /// <summary>
        /// Writes the patron's own attendance log to the LRC PDF form, newest years
        /// last and every visit in ascending date and time order.
        /// </summary>
        private void ExportAttendancePdf()
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "PDF file (*.pdf)|*.pdf";
                dialog.FileName = "my-attendance-" +
                                  DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".pdf";
                dialog.Title = "EXPORT MY ATTENDANCE AS PDF";
                dialog.OverwritePrompt = true;

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    MyAttendancePdfExporter.Export(
                        _fullName, _role, _myAttendance, dialog.FileName);

                    DialogChrome.ShowAlert(
                        "ATTENDANCE EXPORTED TO:\n" + dialog.FileName,
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

        // The PROFILE header carries two actions, so EDIT MY INFO and CHANGE
        // PASSWORD sit on the right edge and the subtitle stops before them.
        private void LayoutProfileActions()
        {
            int right = profileHead.ClientSize.Width;

            changePasswordButton.Left = Math.Max(0, right - changePasswordButton.Width);
            changePasswordButton.Top = 10;

            editProfileButton.Left = Math.Max(0, changePasswordButton.Left - 12 - editProfileButton.Width);
            editProfileButton.Top = 10;

            profileSubtitle.Width = Math.Max(160, editProfileButton.Left - 16);
        }

        private void ShowChangePasswordDialog()
        {
            using ChangePasswordDialog dialog = new ChangePasswordDialog(_accountId, _username, _role);
            dialog.ShowDialog(this);
        }

        // SELF SERVICE: every patron may update the personal details behind the
        // signed in account (name, username and the whole role profile). The role,
        // status and date created stay read only because the library staff owns them.
        private void ShowEditProfileDialog()
        {
            if (_accountId <= 0)
            {
                Authentication.ShowError("MY PROFILE", "YOUR PROFILE COULD NOT BE LOADED.\nSIGN IN AGAIN AND TRY ONCE MORE.");
                return;
            }

            AccountRow? row = _profile;

            if (row == null)
            {
                try
                {
                    row = _directory.LoadProfile(_accountId);
                }
                catch (MySqlException ex)
                {
                    Authentication.ShowError("MY PROFILE", "YOUR PROFILE COULD NOT BE LOADED.\n" + ex.Message);
                    return;
                }
            }

            if (row == null)
            {
                Authentication.ShowError("MY PROFILE", "YOUR PROFILE COULD NOT BE LOADED.");
                return;
            }

            using MyProfileDialog dialog = new MyProfileDialog(row);

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                _directory.UpdateOwnProfile(
                    row,
                    dialog.FullName,
                    dialog.Username,
                    dialog.SchoolId,
                    dialog.Program,
                    dialog.YearLevel,
                    dialog.Institute,
                    dialog.ContactNumber,
                    dialog.Address,
                    dialog.Office);

                // The portal chrome, the QR pass and the profile card all read from
                // these values, so they are refreshed before the next paint.
                _fullName = dialog.FullName;
                _username = dialog.Username;

                ApplyIdentity();
                LoadProfile();

                Authentication.ShowMessage(
                    "YOUR INFORMATION HAS BEEN UPDATED.",
                    "PROFILE UPDATED",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                Authentication.ShowError("MY PROFILE", ex.Message);
            }
            catch (MySqlException ex)
            {
                Authentication.ShowError("MY PROFILE", "YOUR INFORMATION COULD NOT BE UPDATED.\n" + ex.Message);
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
            else if (index == 2)
            {
                // The shelf changes when the librarian approves a request, so the
                // catalog is reloaded when the patron opens this tab. A title that
                // is already borrowed is then marked ALREADY BORROWED before the
                // patron can press REQUEST TO BORROW again.
                LoadCatalogData();
                LoadRequests();
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
            LoadBorrowedBooks();
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
                    HasActiveBorrow(book.BookId));

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
            int available = _catalog.AvailableCopyCount(book);

            // ONE ACTIVE LOAN PER TITLE IS NOT REQUIRED: a patron may borrow the
            // same title again while another copy is still on the shelf. Only a
            // duplicate PENDING request or an empty shelf blocks the action.
            if (HasPendingRequest(book.BookId))
            {
                MessageBox.Show(
                    "YOU ALREADY HAVE A PENDING REQUEST FOR \"" + book.Title.ToUpperInvariant() + "\".\n" +
                    "WAIT FOR THE LIBRARIAN TO PROCESS IT.",
                    "REQUEST PENDING",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                RefreshCatalogStates();
                return;
            }

            if (available <= 0)
            {
                MessageBox.Show(
                    "EVERY COPY OF \"" + book.Title.ToUpperInvariant() + "\" IS ALREADY BORROWED.",
                    "NO COPY AVAILABLE",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                RefreshCatalogStates();
                return;
            }

            int copies;

            // The pop up asks for one thing only: how many copies to request.
            using (RequestCopiesDialog dialog = new RequestCopiesDialog(book.Title, available))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                copies = dialog.Copies;
            }

            try
            {
                _requestDirectory.Create(_accountId, book, copies);
                LoadRequests();
                RefreshCatalogStates();
                MessageBox.Show(
                    "YOUR REQUEST FOR " + (copies == 1 ? "1 COPY" : copies + " COPIES") +
                    " OF \"" + book.Title.ToUpperInvariant() + "\" WAS SENT.\n" +
                    "WAIT FOR THE LIBRARIAN TO APPROVE IT.",
                    "REQUEST SENT",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                MessageBox.Show(ex.Message.ToUpperInvariant(), "REQUEST NOT SENT", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // The request was refused by the database rule (already borrowed,
                // pending, or no copy left). Reload so the card shows the real state.
                LoadCatalogData();
                LoadRequests();
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

        private void LoadBorrowedBooks()
        {
            RebuildBorrowedCards();

            int count = GetMyBorrowRecords().Count;
          
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
        /// attendance number, the date and time, the name, the program / office and
        /// the institute.
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
                    visit.ProgramOrOffice,
                    visit.Institute);

                DataGridViewRow row = _attendanceTable.Rows[index];
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
                    ? "TRY ANOTHER DATE, TIME OR NAME."
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
                   visit.ProgramOrOffice.ToUpperInvariant().Contains(term) ||
                   visit.Institute.ToUpperInvariant().Contains(term) ||
                   visit.DateText.Contains(term) ||
                   visit.TimeText.Contains(term);
        }

        private void RebuildBorrowedCards()
        {
            string term = _borrowedSearch.SearchText.ToUpperInvariant();
            int shown = 0;

            ClearCards(_borrowedFlow, _borrowedCards);

            List<BorrowRecord> borrowRecords = GetMyBorrowRecords()
                .OrderBy(record => record.DateReturned.HasValue)
                .ThenByDescending(record => record.DateBorrowed)
                .ToList();

            foreach (BorrowRecord record in borrowRecords)
            {
                if (term.Length > 0 && !MatchesBorrowRecord(record, term))
                {
                    continue;
                }

                Book? coverBook = _catalog.Books.FirstOrDefault(book => book.BookId == record.BookId);
                BorrowedBookCard card = new BorrowedBookCard(record, coverBook?.CoverImage);
                _borrowedFlow.Controls.Add(card);
                _borrowedCards.Add(card);
                shown++;
            }

            _borrowedFlow.Visible = shown > 0;
            _borrowedEmpty.Visible = shown == 0;
            if (_borrowedEmpty.Visible)
            {
                _borrowedEmpty.Title = term.Length > 0 ? "NO MATCHING BORROW RECORDS" : "NO BORROW RECORDS YET";
                _borrowedEmpty.Caption = term.Length > 0
                    ? "TRY ANOTHER TITLE, AUTHOR OR STATUS."
                    : "WHEN THE LIBRARIAN RELEASES AN APPROVED REQUEST, THE BORROWED BOOK APPEARS HERE.";
                _borrowedEmpty.BringToFront();
            }

            LayoutCardColumns(_borrowedFlow, false);

            if (borrowRecords.Count > 0)
            {
                int active = borrowRecords.Count(record => !record.DateReturned.HasValue);
                int returned = borrowRecords.Count - active;

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
                    HasActiveBorrow(card.Book.BookId));
            }
        }

        private void UpdateStats()
        {
            List<BorrowRecord> borrowRecords = GetMyBorrowRecords();
            int currentlyBorrowed = borrowRecords.Count(record => !record.DateReturned.HasValue);
            int alreadyReturned = borrowRecords.Count - currentlyBorrowed;
            int pending = _studentRequests.Count(request => request.IsPending);
            int approved = _studentRequests.Count(request => request.IsApproved);
            int closed = _studentRequests.Count - pending - approved;

            _catalogStatTitles.Value = _catalog.Books.Count.ToString(CultureInfo.InvariantCulture);
            _catalogStatCopies.Value = _catalog.AvailableCopies.ToString(CultureInfo.InvariantCulture);
            _catalogStatBorrowed.Value = currentlyBorrowed.ToString(CultureInfo.InvariantCulture);
            _catalogStatPending.Value = pending.ToString(CultureInfo.InvariantCulture);

            _requestStatTotal.Value = _studentRequests.Count.ToString(CultureInfo.InvariantCulture);
            _requestStatPending.Value = pending.ToString(CultureInfo.InvariantCulture);
            _requestStatApproved.Value = approved.ToString(CultureInfo.InvariantCulture);
            _requestStatClosed.Value = Math.Max(0, closed).ToString(CultureInfo.InvariantCulture);

            int longest = currentlyBorrowed == 0
                ? 0
                : borrowRecords.Where(record => !record.DateReturned.HasValue).Max(record => record.DaysBorrowed);

            _borrowedStatActive.Value = currentlyBorrowed.ToString(CultureInfo.InvariantCulture);
            _borrowedStatReturned.Value = alreadyReturned.ToString(CultureInfo.InvariantCulture);
            _borrowedStatTotal.Value = borrowRecords.Count.ToString(CultureInfo.InvariantCulture);
            _borrowedStatLongest.Value = longest.ToString(CultureInfo.InvariantCulture) + "D";

            _tabs.TabPages[3].Text = "MY REQUESTS" + (pending > 0 ? "  (" + pending.ToString(CultureInfo.InvariantCulture) + ")" : string.Empty);
            _tabs.TabPages[4].Text = "MY BORROWED BOOKS" + (currentlyBorrowed > 0 ? "  (" + currentlyBorrowed.ToString(CultureInfo.InvariantCulture) + ")" : string.Empty);
        }

        // ------------------------------------------------------------------ HELPERS

        private List<BorrowRecord> GetMyBorrowRecords()
        {
            List<BorrowRecord> mine = new List<BorrowRecord>();

            foreach (BorrowRecord record in _catalog.BorrowRecords)
            {
                if (IsMyBorrowRecord(record))
                {
                    mine.Add(record);
                }
            }

            return mine;
        }

        private bool IsMyBorrowRecord(BorrowRecord record)
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

        private bool HasActiveBorrow(int bookId)
        {
            return _catalog.BorrowRecords.Any(record =>
                !record.DateReturned.HasValue && record.BookId == bookId && IsMyBorrowRecord(record));
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

        private static bool MatchesBorrowRecord(BorrowRecord record, string term)
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

        private static void ClearCards(FlowLayoutPanel flow, List<BorrowedBookCard> cards)
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
