using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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
        private Image? _avatarPhoto;
        private bool _layingOutCards;

        // The name in the profile card shrinks instead of ending in "...". The
        // fitted font is owned by the form and disposed when the window closes.
        private const float ProfileNameMaxSize = 17f;
        private const float ProfileNameMinSize = 8f;
        private Font? _profileNameFont;
        private float _profileNameSize = ProfileNameMaxSize;

        // The QR caption stays on a single line: the font shrinks just enough to
        // fit, so the sentence is never wrapped or cut with an ellipsis.
        private const float QrCaptionMaxSize = 7.75f;
        private const float QrCaptionMinSize = 6f;
        private Font? _qrCaptionFont;
        private float _qrCaptionSize = QrCaptionMaxSize;

        private readonly List<BookCard> _bookCards = new List<BookCard>();
        private readonly List<RequestCard> _requestCards = new List<RequestCard>();
        private readonly List<BorrowedBookCard> _borrowedCards = new List<BorrowedBookCard>();

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
            _headerName.Text = _fullName.ToUpperInvariant();
            _headerRole.Text = _role;
            profileRoleBadge.Text = _role;
            _profileName.Text = _fullName;
            _headerAvatar.Initials = Initials(_fullName);
            _profileAvatar.Initials = Initials(_fullName);
            FitProfileName();
        }

        /// <summary>Hooks the interactions that used to live inside the layout builders.</summary>
        private void WireEvents()
        {
            BuildAttendanceTable();

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

            profileIdentityCard.Paint += PaintIdentityCard;
            profileDetailsCard.Paint += PaintDetailsCard;
            profileIdentityCard.Resize += (s, e) => LayoutProfileCard();
            profileDetailsCard.Resize += (s, e) => LayoutDetailsCard();
            downloadQrButton.Click += (s, e) => SaveQrCode();
            LayoutProfileCard();
            LayoutDetailsCard();

            // A scan can happen in the scanner window while the patron is on this
            // page, so the MY ATTENDANCE table is refreshed whenever the portal is
            // brought back to the front with that tab open.
            Activated += (s, e) =>
            {
                if (_tabs.SelectedIndex == 1)
                {
                    LoadAttendance();
                }
                else if (_tabs.SelectedIndex == 4)
                {
                    // The librarian may have released an approved request while the
                    // patron was in another window, so the borrowed list is rebuilt.
                    LoadCatalogData();
                    LoadBorrowedBooks();
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

        // The PROFILE header carries two actions and mirrors the librarian portal:
        // the green EDIT MY INFO button sits on the right edge and CHANGE PASSWORD
        // to its left, with the subtitle stopping before both.
        private void LayoutProfileActions()
        {
            int right = profileHead.ClientSize.Width;

            editProfileButton.Left = Math.Max(0, right - editProfileButton.Width);
            editProfileButton.Top = 10;

            changePasswordButton.Left = Math.Max(0, editProfileButton.Left - 12 - changePasswordButton.Width);
            changePasswordButton.Top = 10;

            profileSubtitle.Width = Math.Max(160, changePasswordButton.Left - 16);
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
                    dialog.Office,
                    dialog.ProfilePictureChanged ? dialog.ProfilePicture : null,
                    dialog.ProfilePictureChanged);

                // The portal chrome, the QR code and the profile card all read from
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

        // The identity card uses the same soft mint surface as the librarian
        // portal, so both dashboards present the profile the same way.
        private void PaintIdentityCard(object? sender, PaintEventArgs e)
        {
            Control card = (Control)sender!;
            CardPaint.DrawSurface(
                e.Graphics,
                card.Size,
                PageBack,
                Color.FromArgb(240, 253, 244),
                Color.FromArgb(187, 247, 208));
        }

        // The account details card stays white, matching the librarian portal.
        private void PaintDetailsCard(object? sender, PaintEventArgs e)
        {
            Control card = (Control)sender!;
            CardPaint.DrawSurface(e.Graphics, card.Size, PageBack, Color.White, CardBorder);
        }

        // IDENTITY CARD: picture, name and role badge at the top, the QR pass with
        // its DOWNLOAD QR button directly under the picture, and the privacy note
        // pinned to the bottom of the card.
        private void LayoutProfileCard()
        {
            if (profileIdentityCard == null || _qrPicture == null)
            {
                return;
            }

            int width = profileIdentityCard.ClientSize.Width;
            int height = profileIdentityCard.ClientSize.Height;

            if (width <= 0 || height <= 0)
            {
                return;
            }

            int avatarSize = Math.Clamp(width / 3, 110, 140);
            _profileAvatar.SetBounds((width - avatarSize) / 2, 16, avatarSize, avatarSize);

            _profileName.SetBounds(18, _profileAvatar.Bottom + 4, Math.Max(80, width - 36), 34);

            profileRoleBadge.Top = _profileName.Bottom + 4;
            profileRoleBadge.Left = (width - profileRoleBadge.Width) / 2;

            identityDivider.SetBounds(40, profileRoleBadge.Bottom + 10, Math.Max(40, width - 80), 1);

            qrHeading.SetBounds(24, identityDivider.Bottom + 8, Math.Max(80, width - 48), 20);

            int buttonTop = height - 40 - 18;
            int buttonWidth = Math.Min(200, Math.Max(130, width - 120));
            downloadQrButton.SetBounds((width - buttonWidth) / 2, buttonTop, buttonWidth, 40);

            int captionTop = buttonTop - 6 - 24;
            _qrCaption.SetBounds(12, captionTop, Math.Max(80, width - 24), 24);

            int qrTop = qrHeading.Bottom + 8;
            int available = captionTop - 8 - qrTop;
            int side = Math.Clamp(Math.Min(Math.Max(0, width - 110), available), 96, 320);

            _qrPicture.SetBounds((width - side) / 2, qrTop, side, side);
            _qrPicture.Visible = available >= 96;

            FitQrCaption();
            profileIdentityCard.Invalidate();
            FitProfileName();
        }

        /// <summary>
        /// Keeps the QR caption on one line. The font shrinks in small steps until
        /// the whole sentence fits the card width, so it is never wrapped or cut.
        /// </summary>
        private void FitQrCaption()
        {
            string caption = _qrCaption.Text ?? string.Empty;
            int available = Math.Max(60, _qrCaption.Width - 8);
            float size = QrCaptionMaxSize;

            while (size > QrCaptionMinSize)
            {
                using (var probe = new Font("Segoe UI", size, FontStyle.Regular))
                {
                    Size measured = TextRenderer.MeasureText(
                        caption,
                        probe,
                        Size.Empty,
                        TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);

                    if (measured.Width <= available)
                    {
                        break;
                    }
                }

                size -= 0.25f;
            }

            if (Math.Abs(size - _qrCaptionSize) < 0.01f)
            {
                return;
            }

            Font? previous = _qrCaptionFont;
            _qrCaptionFont = new Font("Segoe UI", size, FontStyle.Regular);
            _qrCaptionSize = size;
            _qrCaption.Font = _qrCaptionFont;
            previous?.Dispose();
        }

        /// <summary>
        /// Shrinks the profile name until the whole name fits on one line. A long
        /// name is therefore shown in full instead of ending in "...".
        /// </summary>
        private void FitProfileName()
        {
            string name = _profileName.Text ?? string.Empty;
            int available = Math.Max(60, _profileName.Width - 12);
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
            _profileName.Font = _profileNameFont;
            previous?.Dispose();
        }

        // ACCOUNT DETAILS: the six read only rows share the height of the card, so a
        // taller window spreads them instead of leaving a gap above the footer.
        private void LayoutDetailsCard()
        {
            if (profileDetailsCard == null || profileFooter == null)
            {
                return;
            }

            int width = profileDetailsCard.ClientSize.Width;
            int height = profileDetailsCard.ClientSize.Height;

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
                idNumberCaption,
                programCaption,
                yearCaption,
                instituteCaption,
                contactCaption,
                addressCaption
            };

            Label[] values =
            {
                _schoolIdValue,
                _programValue,
                _yearValue,
                _instituteValue,
                _contactValue,
                _addressValue
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

            int captionWidth = Math.Min(300, Math.Max(120, width / 2 - 34));
            int valueLeft = Math.Min(340, Math.Max(220, width / 2 + 10));
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

            profileDetailsCard.Invalidate();
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

            // A row with a single summary card (MY REQUESTS keeps only TOTAL
            // REQUESTS) stays a compact card instead of stretching across the page.
            if (count == 1)
            {
                width = Math.Min(width, 280);
            }

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
            else if (index == 3)
            {
                // The librarian claims a request from the BORROW REQUESTS tab, so
                // reopening MY REQUESTS picks up the CLAIMED status right away.
                LoadRequests();
            }
            else if (index == 4)
            {
                // Approval writes the borrow rows from the librarian window, so the
                // catalog is reloaded before MY BORROWED BOOKS is built. The tab then
                // shows the same titles and copies as MY REQUESTS without a refresh.
                LoadCatalogData();
                LoadBorrowedBooks();
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

            if (string.Equals(_role, "STAFF", StringComparison.OrdinalIgnoreCase))
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

            // A staff account has an OFFICE instead of a program, so the row is
            // labelled OFFICE and its value comes from staff_info_table.
            programCaption.Text = IsStaffRole(role) ? "OFFICE" : "PROGRAM / OFFICE";

            _profileName.Text = name;
            profileRoleBadge.Text = role.ToUpperInvariant();
            _headerAvatar.Initials = Initials(name);
            _profileAvatar.Initials = Initials(name);
            LayoutProfileCard();

            ApplyProfilePhoto();

            _schoolIdValue.Text = DisplayId();
            _programValue.Text = ProfileValue(DisplayProgramOrOffice());
            _yearValue.Text = ProfileValue(DisplayYearOrRole());
            _instituteValue.Text = ProfileValue(_profile?.Institute);
            _contactValue.Text = ProfileValue(_profile?.ContactNumber);
            _addressValue.Text = ProfileValue(_profile?.Address);

            BuildQrCode(name, role);
        }

        // An empty profile value shows an em dash so every row in the account
        // details card stays aligned and readable, exactly like the librarian tab.
        private static string ProfileValue(string? value)
        {
            return IsPlaceholder(value) ? "\u2014" : value!.Trim();
        }

        /// <summary>
        /// Decodes the portrait that EDIT MY INFO stored on the account. Both the
        /// banner avatar and the large profile avatar use the same image; when the
        /// account has no picture they fall back to the initials.
        /// </summary>
        private void ApplyProfilePhoto()
        {
            Image? next = ProfilePicture.Load(_profile?.ProfilePicture);
            Image? previous = _avatarPhoto;
            _avatarPhoto = next;

            _profileAvatar.Picture = next;
            _headerAvatar.Picture = next;

            previous?.Dispose();
            _profileAvatar.Invalidate();
            _headerAvatar.Invalidate();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            _avatarPhoto?.Dispose();
            _avatarPhoto = null;

            _qrImage?.Dispose();
            _qrImage = null;

            _profileNameFont?.Dispose();
            _profileNameFont = null;

            _qrCaptionFont?.Dispose();
            _qrCaptionFont = null;
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

        private static bool IsStaffRole(string role)
        {
            return string.Equals(role, "STAFF", StringComparison.OrdinalIgnoreCase);
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
                    _qrImage = code.GetGraphic(12, qrCodeColor, Color.White, true);
                    _qrPicture.Image = _qrImage;
                }

                _qrCaption.Text = "SHOW THIS QR CODE AT THE ENTRANCE TO RECORD THE LIBRARY VISIT.";
            }
            catch (Exception)
            {
                _qrImage?.Dispose();
                _qrImage = null;
                _qrPicture.Image = null;
                _qrCaption.Text = "THE QR CODE COULD NOT BE CREATED FOR THIS PROFILE.";
            }

            FitQrCaption();
        }

        // --------------------------------------------------------- DOWNLOAD QR

        /// <summary>Writes the profile QR pass to a PNG file chosen by the user.</summary>
        private void SaveQrCode()
        {
            if (_qrImage == null)
            {
                Authentication.ShowError("MY PROFILE", "THE QR CODE IS NOT READY YET. REOPEN THE PROFILE TAB AND TRY ONCE MORE.");
                return;
            }

            using SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "SAVE LIBRARY QR CODE",
                Filter = "PNG IMAGE (*.png)|*.png",
                DefaultExt = "png",
                AddExtension = true,
                OverwritePrompt = true,
                FileName = BuildQrFileName()
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                using Bitmap card = BuildQrCardImage();
                card.Save(dialog.FileName, ImageFormat.Png);
            }
            catch (Exception ex) when (ex is IOException ||
                                       ex is UnauthorizedAccessException ||
                                       ex is System.Runtime.InteropServices.ExternalException)
            {
                Authentication.ShowError("MY PROFILE", "THE QR CODE COULD NOT BE SAVED.\n" + ex.Message);
                return;
            }

            Authentication.ShowMessage(
                "YOUR QR CODE HAS BEEN SAVED AS A PNG FILE.",
                "QR CODE SAVED",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        /// <summary>
        /// Composes the saved PNG: the code on a white card with the account name
        /// under it, so the file still makes sense outside the dashboard.
        /// </summary>
        private Bitmap BuildQrCardImage()
        {
            const int padding = 40;
            const int qrSize = 420;
            const int headerHeight = 82;

            int width = qrSize + (padding * 2);
            int height = headerHeight + qrSize + 120;
            Bitmap card = new Bitmap(width, height);

            using Graphics graphics = Graphics.FromImage(card);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            graphics.PixelOffsetMode = PixelOffsetMode.Half;
            graphics.Clear(Color.White);

            using Font titleFont = new Font("Segoe UI", 13F, FontStyle.Bold);
            using Font nameFont = new Font("Segoe UI", 11.5F, FontStyle.Bold);
            using Font bodyFont = new Font("Segoe UI", 8.75F);

            TextRenderer.DrawText(
                graphics,
                "TCGC LIBRARY ATTENDANCE PASS",
                titleFont,
                new Rectangle(0, 30, width, 28),
                HeaderGreen,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);

            Rectangle codeBounds = new Rectangle(padding, headerHeight, qrSize, qrSize);

            using (var quietZone = new SolidBrush(Color.White))
            {
                graphics.FillRectangle(quietZone, Rectangle.Inflate(codeBounds, 12, 12));
            }

            if (_qrImage != null)
            {
                graphics.DrawImage(_qrImage, codeBounds);
            }

            TextRenderer.DrawText(
                graphics,
                _fullName.ToUpperInvariant(),
                nameFont,
                new Rectangle(0, codeBounds.Bottom + 24, width, 26),
                HeaderGreen,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);

            TextRenderer.DrawText(
                graphics,
                _role.ToUpperInvariant() + "  \u2022  ACCOUNT " +
                    _accountId.ToString(CultureInfo.InvariantCulture),
                bodyFont,
                new Rectangle(0, codeBounds.Bottom + 54, width, 20),
                Color.FromArgb(100, 116, 139),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);

            TextRenderer.DrawText(
                graphics,
                "SHOW THIS CODE AT THE LRC ENTRANCE TO RECORD YOUR VISIT.",
                bodyFont,
                new Rectangle(padding, height - 36, qrSize, 20),
                Color.FromArgb(100, 116, 139),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);

            using (var border = new Pen(Color.FromArgb(226, 232, 240), 2f))
            {
                graphics.DrawRectangle(border, 1, 1, width - 3, height - 3);
            }

            return card;
        }

        private string BuildQrFileName()
        {
            char[] invalid = Path.GetInvalidFileNameChars();
            StringBuilder builder = new StringBuilder("TCGC-LIBRARY-QR-");

            foreach (char character in _fullName.ToUpperInvariant())
            {
                builder.Append(
                    Array.IndexOf(invalid, character) >= 0 || char.IsWhiteSpace(character)
                        ? '-'
                        : character);
            }

            builder.Append(".png");
            return builder.ToString();
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
                    : "SHOW YOUR QR CODE TO THE LIBRARIAN AT THE LIBRARY OFFICE ENTRANCE.";
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

            // One card per title: the copies written by an approved request are
            // folded into a single line, so the tab shows how many copies the
            // patron borrowed instead of repeating the same title per copy.
            List<BorrowedBookGroup> groups = GetMyBorrowedTitleGroups()
                .OrderBy(group => group.IsFullyReturned)
                .ThenByDescending(group => group.DateBorrowed)
                .ToList();

            foreach (BorrowedBookGroup group in groups)
            {
                if (term.Length > 0 && !MatchesBorrowGroup(group, term))
                {
                    continue;
                }

                Book? coverBook = _catalog.Books.FirstOrDefault(book => book.BookId == group.BookId);
                BorrowedBookCard card = new BorrowedBookCard(group, coverBook?.CoverImage);
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

            _catalogStatTitles.Value = _catalog.Books.Count.ToString(CultureInfo.InvariantCulture);
            _catalogStatCopies.Value = _catalog.AvailableCopies.ToString(CultureInfo.InvariantCulture);
            _catalogStatBorrowed.Value = currentlyBorrowed.ToString(CultureInfo.InvariantCulture);
            _catalogStatPending.Value = pending.ToString(CultureInfo.InvariantCulture);

            _requestStatTotal.Value = _studentRequests.Count.ToString(CultureInfo.InvariantCulture);

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

        /// <summary>
        /// Folds the patron's borrow rows into one line per title. Approving a
        /// request for several copies writes one row per copy, so grouping keeps
        /// MY BORROWED BOOKS reading like MY REQUESTS: one card per title with the
        /// number of copies behind it.
        /// </summary>
        private List<BorrowedBookGroup> GetMyBorrowedTitleGroups()
        {
            List<BorrowedBookGroup> groups = new List<BorrowedBookGroup>();

            foreach (IGrouping<string, BorrowRecord> bucket in
                     GetMyBorrowRecords().GroupBy(TitleKeyFor))
            {
                groups.Add(new BorrowedBookGroup(bucket));
            }

            return groups;
        }

        private static string TitleKeyFor(BorrowRecord record)
        {
            // A deleted title keeps no book id, so its upper case title is the key.
            return record.BookId > 0
                ? "BOOK:" + record.BookId.ToString(CultureInfo.InvariantCulture)
                : "TITLE:" + record.BookTitle.ToUpperInvariant();
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

        private static bool MatchesBorrowGroup(BorrowedBookGroup group, string term)
        {
            return group.BookTitle.ToUpperInvariant().Contains(term) ||
                   group.Author.ToUpperInvariant().Contains(term) ||
                   group.DisplayStatus.ToUpperInvariant().Contains(term);
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

            // The width comes from the host panel, not from the scrolling flow.
            // Adding cards can make a vertical scrollbar appear; measuring the
            // flow again would then give the later cards a smaller width than the
            // first ones and they would look like they overlapped the edge.
            Control host = flow.Parent ?? flow;
            int hostWidth = host.ClientSize.Width > 0
                ? host.ClientSize.Width
                : flow.ClientSize.Width;
            int available = Math.Max(320, hostWidth - flow.Padding.Horizontal - 24);

            flow.SuspendLayout();
            try
            {
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
            finally
            {
                flow.ResumeLayout();
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
            return IsPlaceholder(value) ? "-" : value!.Trim();
        }

        private static string CleanValue(string? value)
        {
            return IsPlaceholder(value) ? string.Empty : value!.Trim();
        }

        /// <summary>
        /// AccountRow stores a missing profile value as an em dash, while the
        /// designer uses a plain hyphen. Both count as empty here, so a staff
        /// account without a program falls through to its OFFICE instead of
        /// showing the dash.
        /// </summary>
        private static bool IsPlaceholder(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            string trimmed = value.Trim();
            return trimmed == "-" || trimmed == "\u2013" || trimmed == "\u2014";
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
