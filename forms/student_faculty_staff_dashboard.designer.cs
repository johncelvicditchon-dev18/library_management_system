namespace oop_project.forms
{
    partial class student_faculty_staff_dashboard
    {
        /// <summary>Required designer variable.</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>Clean up any resources being used.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            rootGrid = new TableLayoutPanel();
            headerPanel = new PatronHeaderPanel();
            _logout = new SignOutButton();
            _headerRole = new PillBadge();
            _headerName = new Label();
            _headerAvatar = new AvatarBadge();
            _tabs = new TabControl();
            tabPageProfile = new TabPage();
            _profilePage = new Panel();
            profileGrid = new TableLayoutPanel();
            profileHead = new Panel();
            profileSubtitle = new Label();
            profileTitle = new Label();
            changePasswordButton = new RoundedButton();
            editProfileButton = new RoundedButton();
            profileColumns = new TableLayoutPanel();
            profileIdentityCard = new Panel();
            downloadQrButton = new RoundedButton();
            _qrCaption = new Label();
            _qrPicture = new PictureBox();
            qrHeading = new Label();
            identityDivider = new Panel();
            profileRoleBadge = new PillBadge();
            _profileName = new Label();
            _profileAvatar = new AvatarBadge();
            profileDetailsCard = new Panel();
            profileFooter = new Label();
            dividerRow6 = new Panel();
            _addressValue = new Label();
            addressCaption = new Label();
            dividerRow5 = new Panel();
            _contactValue = new Label();
            contactCaption = new Label();
            dividerRow4 = new Panel();
            _instituteValue = new Label();
            instituteCaption = new Label();
            dividerRow3 = new Panel();
            _yearValue = new Label();
            yearCaption = new Label();
            dividerRow2 = new Panel();
            _programValue = new Label();
            programCaption = new Label();
            dividerRow1 = new Panel();
            _schoolIdValue = new Label();
            idNumberCaption = new Label();
            detailsTopDivider = new Panel();
            detailsSubtitle = new Label();
            detailsHeading = new Label();
            tabPageAttendance = new TabPage();
            _attendancePage = new Panel();
            attendanceGrid = new TableLayoutPanel();
            attendanceHead = new Panel();
            _attendanceSearch = new SearchBox();
            attendanceSubtitle = new Label();
            attendanceTitle = new Label();
            attendanceRefresh = new RoundedButton();
            attendanceStatRow = new FlowLayoutPanel();
            _attendanceStatTotal = new StatCard();
            _attendanceStatToday = new StatCard();
            _attendanceStatMonth = new StatCard();
            _attendanceStatLast = new StatCard();
            attendanceListHost = new Panel();
            _attendanceFlow = new FlowLayoutPanel();
            _attendanceEmpty = new EmptyState();
            tabPageCatalog = new TabPage();
            _catalogPage = new Panel();
            catalogGrid = new TableLayoutPanel();
            catalogHead = new Panel();
            _catalogSearch = new SearchBox();
            catalogSubtitle = new Label();
            catalogTitle = new Label();
            catalogRefresh = new RoundedButton();
            catalogStatRow = new FlowLayoutPanel();
            _catalogStatTitles = new StatCard();
            _catalogStatCopies = new StatCard();
            _catalogStatBorrowed = new StatCard();
            _catalogStatPending = new StatCard();
            catalogListHost = new Panel();
            _catalogFlow = new FlowLayoutPanel();
            _catalogEmpty = new EmptyState();
            tabPageRequests = new TabPage();
            _requestsPage = new Panel();
            requestsGrid = new TableLayoutPanel();
            requestsHead = new Panel();
            _requestsSearch = new SearchBox();
            requestsTitle = new Label();
            requestsRefresh = new RoundedButton();
            requestsStatRow = new FlowLayoutPanel();
            _requestStatTotal = new StatCard();
            requestsListHost = new Panel();
            _requestFlow = new FlowLayoutPanel();
            _requestsEmpty = new EmptyState();
            tabPageBorrowed = new TabPage();
            _borrowedPage = new Panel();
            borrowedGrid = new TableLayoutPanel();
            borrowedHead = new Panel();
            _borrowedSearch = new SearchBox();
            borrowedTitle = new Label();
            borrowedRefresh = new RoundedButton();
            borrowedStatRow = new FlowLayoutPanel();
            _borrowedStatActive = new StatCard();
            _borrowedStatReturned = new StatCard();
            _borrowedStatTotal = new StatCard();
            _borrowedStatLongest = new StatCard();
            borrowedListHost = new Panel();
            _borrowedFlow = new FlowLayoutPanel();
            _borrowedEmpty = new EmptyState();
            rootGrid.SuspendLayout();
            headerPanel.SuspendLayout();
            _tabs.SuspendLayout();
            tabPageProfile.SuspendLayout();
            _profilePage.SuspendLayout();
            profileGrid.SuspendLayout();
            profileHead.SuspendLayout();
            profileColumns.SuspendLayout();
            profileIdentityCard.SuspendLayout();
            profileDetailsCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_qrPicture).BeginInit();
            tabPageAttendance.SuspendLayout();
            _attendancePage.SuspendLayout();
            attendanceGrid.SuspendLayout();
            attendanceHead.SuspendLayout();
            attendanceStatRow.SuspendLayout();
            attendanceListHost.SuspendLayout();
            tabPageCatalog.SuspendLayout();
            _catalogPage.SuspendLayout();
            catalogGrid.SuspendLayout();
            catalogHead.SuspendLayout();
            catalogStatRow.SuspendLayout();
            catalogListHost.SuspendLayout();
            tabPageRequests.SuspendLayout();
            _requestsPage.SuspendLayout();
            requestsGrid.SuspendLayout();
            requestsHead.SuspendLayout();
            requestsStatRow.SuspendLayout();
            requestsListHost.SuspendLayout();
            tabPageBorrowed.SuspendLayout();
            _borrowedPage.SuspendLayout();
            borrowedGrid.SuspendLayout();
            borrowedHead.SuspendLayout();
            borrowedStatRow.SuspendLayout();
            borrowedListHost.SuspendLayout();
            SuspendLayout();
            // 
            // rootGrid
            // 
            rootGrid.ColumnCount = 1;
            rootGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            rootGrid.Controls.Add(headerPanel, 0, 0);
            rootGrid.Controls.Add(_tabs, 0, 1);
            rootGrid.Dock = DockStyle.Fill;
            rootGrid.Location = new Point(0, 0);
            rootGrid.Name = "rootGrid";
            rootGrid.RowCount = 2;
            rootGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
            rootGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            rootGrid.Size = new Size(1280, 800);
            rootGrid.TabIndex = 0;
            // 
            // headerPanel
            // 
            headerPanel.BackColor = Color.FromArgb(5, 84, 63);
            headerPanel.Controls.Add(_logout);
            headerPanel.Controls.Add(_headerRole);
            headerPanel.Controls.Add(_headerName);
            headerPanel.Controls.Add(_headerAvatar);
            headerPanel.Dock = DockStyle.Fill;
            headerPanel.EndColor = Color.FromArgb(22, 163, 74);
            headerPanel.Location = new Point(3, 3);
            headerPanel.Name = "headerPanel";
            headerPanel.Size = new Size(1274, 94);
            headerPanel.StartColor = Color.FromArgb(5, 84, 63);
            headerPanel.TabIndex = 0;
            // 
            // _logout
            // 
            _logout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _logout.BackColor = Color.Transparent;
            _logout.CornerRadius = 14;
            _logout.HoverColor = Color.FromArgb(194, 65, 12);
            _logout.Location = new Point(2114, 24);
            _logout.Name = "_logout";
            _logout.PressedColor = Color.FromArgb(154, 52, 18);
            _logout.Size = new Size(46, 46);
            _logout.TabIndex = 1;
            // 
            // _headerRole
            // 
            _headerRole.BackColor = Color.Transparent;
            _headerRole.BorderColor = Color.FromArgb(167, 243, 208);
            _headerRole.FillColor = Color.White;
            _headerRole.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            _headerRole.Location = new Point(106, 47);
            _headerRole.Name = "_headerRole";
            _headerRole.Size = new Size(113, 34);
            _headerRole.TabIndex = 2;
            _headerRole.TabStop = false;
            _headerRole.Text = "STUDENT";
            _headerRole.TextColor = Color.FromArgb(6, 78, 59);
            // 
            // _headerName
            // 
            _headerName.AutoEllipsis = true;
            _headerName.BackColor = Color.Transparent;
            _headerName.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            _headerName.ForeColor = Color.White;
            _headerName.Location = new Point(106, 9);
            _headerName.Name = "_headerName";
            _headerName.Size = new Size(632, 40);
            _headerName.TabIndex = 3;
            _headerName.Text = "PATRON";
            _headerName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _headerAvatar
            // 
            _headerAvatar.BackColor = Color.Transparent;
            _headerAvatar.FillBottomColor = Color.FromArgb(4, 90, 66);
            _headerAvatar.FillTopColor = Color.FromArgb(34, 197, 94);
            _headerAvatar.Initials = "PA";
            _headerAvatar.InitialsColor = Color.White;
            _headerAvatar.Location = new Point(28, 20);
            _headerAvatar.Name = "_headerAvatar";
            _headerAvatar.RingColor = Color.FromArgb(70, 255, 255, 255);
            _headerAvatar.Size = new Size(60, 60);
            _headerAvatar.TabIndex = 4;
            _headerAvatar.TabStop = false;
            // 
            // _tabs
            // 
            _tabs.Controls.Add(tabPageProfile);
            _tabs.Controls.Add(tabPageAttendance);
            _tabs.Controls.Add(tabPageCatalog);
            _tabs.Controls.Add(tabPageRequests);
            _tabs.Controls.Add(tabPageBorrowed);
            _tabs.Dock = DockStyle.Fill;
            _tabs.Font = new Font("Segoe UI", 9.25F, FontStyle.Bold);
            _tabs.Location = new Point(3, 103);
            _tabs.Name = "_tabs";
            _tabs.SelectedIndex = 0;
            _tabs.Size = new Size(1274, 694);
            _tabs.TabIndex = 1;
            // 
            // tabPageProfile
            // 
            tabPageProfile.BackColor = Color.FromArgb(241, 245, 249);
            tabPageProfile.Controls.Add(_profilePage);
            tabPageProfile.Location = new Point(4, 29);
            tabPageProfile.Name = "tabPageProfile";
            tabPageProfile.Padding = new Padding(3);
            tabPageProfile.Size = new Size(1266, 661);
            tabPageProfile.TabIndex = 0;
            tabPageProfile.Text = "MY PROFILE";
            // 
            // _profilePage
            // 
            _profilePage.Controls.Add(profileGrid);
            _profilePage.Dock = DockStyle.Fill;
            _profilePage.Location = new Point(3, 3);
            _profilePage.Name = "_profilePage";
            _profilePage.Padding = new Padding(26, 18, 26, 22);
            _profilePage.Size = new Size(1260, 655);
            _profilePage.TabIndex = 4;
            // 
            // profileGrid
            // 
            profileGrid.ColumnCount = 1;
            profileGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            profileGrid.Controls.Add(profileHead, 0, 0);
            profileGrid.Controls.Add(profileColumns, 0, 1);
            profileGrid.Dock = DockStyle.Fill;
            profileGrid.Location = new Point(26, 18);
            profileGrid.Name = "profileGrid";
            profileGrid.RowCount = 2;
            profileGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 92F));
            profileGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            profileGrid.Size = new Size(1208, 615);
            profileGrid.TabIndex = 0;
            // 
            // profileHead
            // 
            profileHead.Controls.Add(profileSubtitle);
            profileHead.Controls.Add(profileTitle);
            profileHead.Controls.Add(changePasswordButton);
            profileHead.Controls.Add(editProfileButton);
            profileHead.Dock = DockStyle.Fill;
            profileHead.Location = new Point(3, 3);
            profileHead.Name = "profileHead";
            profileHead.Size = new Size(1202, 86);
            profileHead.TabIndex = 0;
            // 
            // profileSubtitle
            // 
            profileSubtitle.AutoEllipsis = true;
            profileSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
            profileSubtitle.Location = new Point(2, 62);
            profileSubtitle.Name = "profileSubtitle";
            profileSubtitle.Size = new Size(794, 22);
            profileSubtitle.TabIndex = 0;
            profileSubtitle.Text = "THE INFORMATION BELOW COMES FROM YOUR LIBRARY ACCOUNT.";
            // 
            // profileTitle
            // 
            profileTitle.AutoSize = true;
            profileTitle.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            profileTitle.ForeColor = Color.FromArgb(15, 23, 42);
            profileTitle.Location = new Point(0, 1);
            profileTitle.Name = "profileTitle";
            profileTitle.Size = new Size(278, 60);
            profileTitle.TabIndex = 1;
            profileTitle.Text = "MY PROFILE";
            // 
            // changePasswordButton
            // 
            changePasswordButton.BackColor = Color.Transparent;
            changePasswordButton.CornerRadius = 11;
            changePasswordButton.DisabledTextColor = Color.FromArgb(248, 250, 252);
            changePasswordButton.FlatAppearance.BorderSize = 0;
            changePasswordButton.FlatStyle = FlatStyle.Flat;
            changePasswordButton.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            changePasswordButton.ForeColor = Color.FromArgb(51, 65, 85);
            changePasswordButton.HoverColor = Color.FromArgb(226, 232, 240);
            changePasswordButton.IconFallbackGlyph = "";
            changePasswordButton.IconGlyph = "";
            changePasswordButton.IconSize = 16F;
            changePasswordButton.Location = new Point(866, 10);
            changePasswordButton.Name = "changePasswordButton";
            changePasswordButton.NormalColor = Color.FromArgb(241, 245, 249);
            changePasswordButton.PressedColor = Color.FromArgb(203, 213, 225);
            changePasswordButton.Size = new Size(178, 44);
            changePasswordButton.TabIndex = 3;
            changePasswordButton.Text = "CHANGE PASSWORD";
            changePasswordButton.UseVisualStyleBackColor = false;
            // 
            // editProfileButton
            // 
            editProfileButton.BackColor = Color.Transparent;
            editProfileButton.CornerRadius = 11;
            editProfileButton.DisabledTextColor = Color.FromArgb(248, 250, 252);
            editProfileButton.FlatAppearance.BorderSize = 0;
            editProfileButton.FlatStyle = FlatStyle.Flat;
            editProfileButton.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            editProfileButton.ForeColor = Color.White;
            editProfileButton.HoverColor = Color.FromArgb(21, 128, 61);
            editProfileButton.IconFallbackGlyph = "";
            editProfileButton.IconGlyph = "";
            editProfileButton.IconSize = 16F;
            editProfileButton.Location = new Point(1054, 10);
            editProfileButton.Name = "editProfileButton";
            editProfileButton.NormalColor = Color.FromArgb(22, 163, 74);
            editProfileButton.PressedColor = Color.FromArgb(20, 83, 45);
            editProfileButton.Size = new Size(148, 44);
            editProfileButton.TabIndex = 4;
            editProfileButton.Text = "EDIT MY INFO";
            editProfileButton.UseVisualStyleBackColor = false;
            // 
            // profileColumns
            // 
            profileColumns.ColumnCount = 2;
            profileColumns.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 440F));
            profileColumns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            profileColumns.Controls.Add(profileIdentityCard, 0, 0);
            profileColumns.Controls.Add(profileDetailsCard, 1, 0);
            profileColumns.Dock = DockStyle.Fill;
            profileColumns.Location = new Point(3, 95);
            profileColumns.Name = "profileColumns";
            profileColumns.RowCount = 1;
            profileColumns.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            profileColumns.Size = new Size(1202, 517);
            profileColumns.TabIndex = 1;
            // 
            // profileIdentityCard
            // 
            profileIdentityCard.Controls.Add(downloadQrButton);
            profileIdentityCard.Controls.Add(_qrCaption);
            profileIdentityCard.Controls.Add(_qrPicture);
            profileIdentityCard.Controls.Add(qrHeading);
            profileIdentityCard.Controls.Add(identityDivider);
            profileIdentityCard.Controls.Add(profileRoleBadge);
            profileIdentityCard.Controls.Add(_profileName);
            profileIdentityCard.Controls.Add(_profileAvatar);
            profileIdentityCard.Dock = DockStyle.Fill;
            profileIdentityCard.Location = new Point(0, 0);
            profileIdentityCard.Margin = new Padding(0, 0, 18, 0);
            profileIdentityCard.Name = "profileIdentityCard";
            profileIdentityCard.Size = new Size(422, 517);
            profileIdentityCard.TabIndex = 0;
            // 
            // _profileAvatar
            // 
            _profileAvatar.BackColor = Color.Transparent;
            _profileAvatar.FillBottomColor = Color.FromArgb(4, 90, 66);
            _profileAvatar.FillTopColor = Color.FromArgb(34, 197, 94);
            _profileAvatar.Initials = "PA";
            _profileAvatar.InitialsColor = Color.White;
            _profileAvatar.Location = new Point(141, 16);
            _profileAvatar.Name = "_profileAvatar";
            _profileAvatar.RingColor = Color.FromArgb(70, 255, 255, 255);
            _profileAvatar.Size = new Size(140, 140);
            _profileAvatar.TabIndex = 1;
            _profileAvatar.TabStop = false;
            // 
            // _profileName
            // 
            _profileName.AutoEllipsis = true;
            _profileName.BackColor = Color.Transparent;
            _profileName.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
            _profileName.ForeColor = Color.FromArgb(6, 78, 59);
            _profileName.Location = new Point(18, 160);
            _profileName.Name = "_profileName";
            _profileName.Size = new Size(386, 34);
            _profileName.TabIndex = 2;
            _profileName.Text = "PATRON";
            _profileName.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // profileRoleBadge
            // 
            profileRoleBadge.BackColor = Color.Transparent;
            profileRoleBadge.BorderColor = Color.FromArgb(167, 243, 208);
            profileRoleBadge.CornerRadius = 14;
            profileRoleBadge.FillColor = Color.White;
            profileRoleBadge.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            profileRoleBadge.Location = new Point(160, 198);
            profileRoleBadge.Name = "profileRoleBadge";
            profileRoleBadge.Size = new Size(103, 33);
            profileRoleBadge.TabIndex = 3;
            profileRoleBadge.TabStop = false;
            profileRoleBadge.Text = "STUDENT";
            profileRoleBadge.TextColor = Color.FromArgb(4, 120, 87);
            // 
            // identityDivider
            // 
            identityDivider.BackColor = Color.FromArgb(187, 247, 208);
            identityDivider.Location = new Point(40, 241);
            identityDivider.Name = "identityDivider";
            identityDivider.Size = new Size(342, 1);
            identityDivider.TabIndex = 4;
            // 
            // qrHeading
            // 
            qrHeading.AutoSize = false;
            qrHeading.BackColor = Color.Transparent;
            qrHeading.Font = new Font("Segoe UI", 8.75F, FontStyle.Bold);
            qrHeading.ForeColor = Color.FromArgb(4, 120, 87);
            qrHeading.Location = new Point(24, 249);
            qrHeading.Name = "qrHeading";
            qrHeading.Size = new Size(374, 20);
            qrHeading.TabIndex = 5;
            qrHeading.Text = "MY LIBRARY ATTENDANCE QR";
            qrHeading.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // _qrPicture
            // 
            _qrPicture.BackColor = Color.White;
            _qrPicture.Location = new Point(142, 277);
            _qrPicture.Name = "_qrPicture";
            _qrPicture.Size = new Size(138, 138);
            _qrPicture.SizeMode = PictureBoxSizeMode.Zoom;
            _qrPicture.TabIndex = 6;
            _qrPicture.TabStop = false;
            // 
            // _qrCaption
            // 
            _qrCaption.AutoEllipsis = false;
            _qrCaption.BackColor = Color.Transparent;
            _qrCaption.Font = new Font("Segoe UI", 7.75F);
            _qrCaption.ForeColor = Color.FromArgb(71, 105, 92);
            _qrCaption.Location = new Point(12, 404);
            _qrCaption.Name = "_qrCaption";
            _qrCaption.Size = new Size(398, 24);
            _qrCaption.TabIndex = 7;
            _qrCaption.Text = "SHOW THIS QR CODE AT THE ENTRANCE TO RECORD THE LIBRARY VISIT.";
            _qrCaption.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // downloadQrButton
            // 
            downloadQrButton.BackColor = Color.Transparent;
            downloadQrButton.CornerRadius = 11;
            downloadQrButton.DisabledTextColor = Color.FromArgb(248, 250, 252);
            downloadQrButton.FlatAppearance.BorderSize = 0;
            downloadQrButton.FlatStyle = FlatStyle.Flat;
            downloadQrButton.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            downloadQrButton.ForeColor = Color.White;
            downloadQrButton.HoverColor = Color.FromArgb(21, 128, 61);
            downloadQrButton.IconFallbackGlyph = "\uE896";
            downloadQrButton.IconGlyph = "\uE896";
            downloadQrButton.IconSize = 15F;
            downloadQrButton.Location = new Point(111, 459);
            downloadQrButton.Name = "downloadQrButton";
            downloadQrButton.NormalColor = Color.FromArgb(22, 163, 74);
            downloadQrButton.PressedColor = Color.FromArgb(20, 83, 45);
            downloadQrButton.Size = new Size(200, 40);
            downloadQrButton.TabIndex = 8;
            downloadQrButton.Text = "DOWNLOAD QR";
            downloadQrButton.UseVisualStyleBackColor = false;
            // 
            // profileDetailsCard
            // 
            profileDetailsCard.Controls.Add(profileFooter);
            profileDetailsCard.Controls.Add(dividerRow6);
            profileDetailsCard.Controls.Add(_addressValue);
            profileDetailsCard.Controls.Add(addressCaption);
            profileDetailsCard.Controls.Add(dividerRow5);
            profileDetailsCard.Controls.Add(_contactValue);
            profileDetailsCard.Controls.Add(contactCaption);
            profileDetailsCard.Controls.Add(dividerRow4);
            profileDetailsCard.Controls.Add(_instituteValue);
            profileDetailsCard.Controls.Add(instituteCaption);
            profileDetailsCard.Controls.Add(dividerRow3);
            profileDetailsCard.Controls.Add(_yearValue);
            profileDetailsCard.Controls.Add(yearCaption);
            profileDetailsCard.Controls.Add(dividerRow2);
            profileDetailsCard.Controls.Add(_programValue);
            profileDetailsCard.Controls.Add(programCaption);
            profileDetailsCard.Controls.Add(dividerRow1);
            profileDetailsCard.Controls.Add(_schoolIdValue);
            profileDetailsCard.Controls.Add(idNumberCaption);
            profileDetailsCard.Controls.Add(detailsTopDivider);
            profileDetailsCard.Controls.Add(detailsSubtitle);
            profileDetailsCard.Controls.Add(detailsHeading);
            profileDetailsCard.Dock = DockStyle.Fill;
            profileDetailsCard.Location = new Point(440, 0);
            profileDetailsCard.Margin = new Padding(0);
            profileDetailsCard.Name = "profileDetailsCard";
            profileDetailsCard.Size = new Size(762, 517);
            profileDetailsCard.TabIndex = 1;
            // 
            // detailsHeading
            // 
            detailsHeading.AutoSize = true;
            detailsHeading.BackColor = Color.White;
            detailsHeading.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            detailsHeading.ForeColor = Color.FromArgb(15, 23, 42);
            detailsHeading.Location = new Point(34, 28);
            detailsHeading.Name = "detailsHeading";
            detailsHeading.Size = new Size(174, 25);
            detailsHeading.TabIndex = 0;
            detailsHeading.Text = "ACCOUNT DETAILS";
            // 
            // detailsSubtitle
            // 
            detailsSubtitle.AutoSize = true;
            detailsSubtitle.BackColor = Color.White;
            detailsSubtitle.Font = new Font("Segoe UI", 9F);
            detailsSubtitle.ForeColor = Color.FromArgb(148, 163, 184);
            detailsSubtitle.Location = new Point(34, 56);
            detailsSubtitle.Name = "detailsSubtitle";
            detailsSubtitle.Size = new Size(381, 20);
            detailsSubtitle.TabIndex = 1;
            detailsSubtitle.Text = "The information below comes from your library account.";
            // 
            // detailsTopDivider
            // 
            detailsTopDivider.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            detailsTopDivider.BackColor = Color.FromArgb(226, 232, 240);
            detailsTopDivider.Location = new Point(34, 92);
            detailsTopDivider.Name = "detailsTopDivider";
            detailsTopDivider.Size = new Size(694, 1);
            detailsTopDivider.TabIndex = 2;
            // 
            // idNumberCaption
            // 
            idNumberCaption.BackColor = Color.White;
            idNumberCaption.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            idNumberCaption.ForeColor = Color.FromArgb(148, 163, 184);
            idNumberCaption.Location = new Point(34, 130);
            idNumberCaption.Name = "idNumberCaption";
            idNumberCaption.Size = new Size(300, 34);
            idNumberCaption.TabIndex = 3;
            idNumberCaption.Text = "ID NUMBER";
            idNumberCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _schoolIdValue
            // 
            _schoolIdValue.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _schoolIdValue.AutoEllipsis = true;
            _schoolIdValue.BackColor = Color.White;
            _schoolIdValue.Font = new Font("Segoe UI", 10.75F);
            _schoolIdValue.ForeColor = Color.FromArgb(15, 23, 42);
            _schoolIdValue.Location = new Point(340, 122);
            _schoolIdValue.Name = "_schoolIdValue";
            _schoolIdValue.Size = new Size(388, 44);
            _schoolIdValue.TabIndex = 4;
            _schoolIdValue.Text = "—";
            _schoolIdValue.TextAlign = ContentAlignment.MiddleRight;
            // 
            // dividerRow1
            // 
            dividerRow1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dividerRow1.BackColor = Color.FromArgb(241, 245, 249);
            dividerRow1.Location = new Point(34, 174);
            dividerRow1.Name = "dividerRow1";
            dividerRow1.Size = new Size(694, 1);
            dividerRow1.TabIndex = 5;
            // 
            // programCaption
            // 
            programCaption.BackColor = Color.White;
            programCaption.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            programCaption.ForeColor = Color.FromArgb(148, 163, 184);
            programCaption.Location = new Point(34, 200);
            programCaption.Name = "programCaption";
            programCaption.Size = new Size(300, 34);
            programCaption.TabIndex = 6;
            programCaption.Text = "PROGRAM / OFFICE";
            programCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _programValue
            // 
            _programValue.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _programValue.AutoEllipsis = true;
            _programValue.BackColor = Color.White;
            _programValue.Font = new Font("Segoe UI", 10.75F);
            _programValue.ForeColor = Color.FromArgb(15, 23, 42);
            _programValue.Location = new Point(340, 192);
            _programValue.Name = "_programValue";
            _programValue.Size = new Size(388, 44);
            _programValue.TabIndex = 7;
            _programValue.Text = "—";
            _programValue.TextAlign = ContentAlignment.MiddleRight;
            // 
            // dividerRow2
            // 
            dividerRow2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dividerRow2.BackColor = Color.FromArgb(241, 245, 249);
            dividerRow2.Location = new Point(34, 244);
            dividerRow2.Name = "dividerRow2";
            dividerRow2.Size = new Size(694, 1);
            dividerRow2.TabIndex = 8;
            // 
            // yearCaption
            // 
            yearCaption.BackColor = Color.White;
            yearCaption.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            yearCaption.ForeColor = Color.FromArgb(148, 163, 184);
            yearCaption.Location = new Point(34, 270);
            yearCaption.Name = "yearCaption";
            yearCaption.Size = new Size(300, 34);
            yearCaption.TabIndex = 9;
            yearCaption.Text = "YEAR";
            yearCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _yearValue
            // 
            _yearValue.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _yearValue.AutoEllipsis = true;
            _yearValue.BackColor = Color.White;
            _yearValue.Font = new Font("Segoe UI", 10.75F);
            _yearValue.ForeColor = Color.FromArgb(15, 23, 42);
            _yearValue.Location = new Point(340, 262);
            _yearValue.Name = "_yearValue";
            _yearValue.Size = new Size(388, 44);
            _yearValue.TabIndex = 10;
            _yearValue.Text = "—";
            _yearValue.TextAlign = ContentAlignment.MiddleRight;
            // 
            // dividerRow3
            // 
            dividerRow3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dividerRow3.BackColor = Color.FromArgb(241, 245, 249);
            dividerRow3.Location = new Point(34, 314);
            dividerRow3.Name = "dividerRow3";
            dividerRow3.Size = new Size(694, 1);
            dividerRow3.TabIndex = 11;
            // 
            // instituteCaption
            // 
            instituteCaption.BackColor = Color.White;
            instituteCaption.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            instituteCaption.ForeColor = Color.FromArgb(148, 163, 184);
            instituteCaption.Location = new Point(34, 340);
            instituteCaption.Name = "instituteCaption";
            instituteCaption.Size = new Size(300, 34);
            instituteCaption.TabIndex = 12;
            instituteCaption.Text = "INSTITUTE";
            instituteCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _instituteValue
            // 
            _instituteValue.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _instituteValue.AutoEllipsis = true;
            _instituteValue.BackColor = Color.White;
            _instituteValue.Font = new Font("Segoe UI", 10.75F);
            _instituteValue.ForeColor = Color.FromArgb(15, 23, 42);
            _instituteValue.Location = new Point(340, 332);
            _instituteValue.Name = "_instituteValue";
            _instituteValue.Size = new Size(388, 44);
            _instituteValue.TabIndex = 13;
            _instituteValue.Text = "—";
            _instituteValue.TextAlign = ContentAlignment.MiddleRight;
            // 
            // dividerRow4
            // 
            dividerRow4.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dividerRow4.BackColor = Color.FromArgb(241, 245, 249);
            dividerRow4.Location = new Point(34, 384);
            dividerRow4.Name = "dividerRow4";
            dividerRow4.Size = new Size(694, 1);
            dividerRow4.TabIndex = 14;
            // 
            // contactCaption
            // 
            contactCaption.BackColor = Color.White;
            contactCaption.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            contactCaption.ForeColor = Color.FromArgb(148, 163, 184);
            contactCaption.Location = new Point(34, 410);
            contactCaption.Name = "contactCaption";
            contactCaption.Size = new Size(300, 34);
            contactCaption.TabIndex = 15;
            contactCaption.Text = "CONTACT";
            contactCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _contactValue
            // 
            _contactValue.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _contactValue.AutoEllipsis = true;
            _contactValue.BackColor = Color.White;
            _contactValue.Font = new Font("Segoe UI", 10.75F);
            _contactValue.ForeColor = Color.FromArgb(15, 23, 42);
            _contactValue.Location = new Point(340, 402);
            _contactValue.Name = "_contactValue";
            _contactValue.Size = new Size(388, 44);
            _contactValue.TabIndex = 16;
            _contactValue.Text = "—";
            _contactValue.TextAlign = ContentAlignment.MiddleRight;
            // 
            // dividerRow5
            // 
            dividerRow5.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dividerRow5.BackColor = Color.FromArgb(241, 245, 249);
            dividerRow5.Location = new Point(34, 454);
            dividerRow5.Name = "dividerRow5";
            dividerRow5.Size = new Size(694, 1);
            dividerRow5.TabIndex = 17;
            // 
            // addressCaption
            // 
            addressCaption.BackColor = Color.White;
            addressCaption.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            addressCaption.ForeColor = Color.FromArgb(148, 163, 184);
            addressCaption.Location = new Point(34, 480);
            addressCaption.Name = "addressCaption";
            addressCaption.Size = new Size(300, 34);
            addressCaption.TabIndex = 18;
            addressCaption.Text = "ADDRESS";
            addressCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _addressValue
            // 
            _addressValue.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _addressValue.AutoEllipsis = true;
            _addressValue.BackColor = Color.White;
            _addressValue.Font = new Font("Segoe UI", 10.75F);
            _addressValue.ForeColor = Color.FromArgb(15, 23, 42);
            _addressValue.Location = new Point(340, 472);
            _addressValue.Name = "_addressValue";
            _addressValue.Size = new Size(388, 44);
            _addressValue.TabIndex = 19;
            _addressValue.Text = "—";
            _addressValue.TextAlign = ContentAlignment.MiddleRight;
            // 
            // dividerRow6
            // 
            dividerRow6.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dividerRow6.BackColor = Color.FromArgb(241, 245, 249);
            dividerRow6.Location = new Point(34, 524);
            dividerRow6.Name = "dividerRow6";
            dividerRow6.Size = new Size(694, 1);
            dividerRow6.TabIndex = 20;
            // 
            // profileFooter
            // 
            profileFooter.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            profileFooter.AutoSize = true;
            profileFooter.BackColor = Color.White;
            profileFooter.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            profileFooter.ForeColor = Color.FromArgb(148, 163, 184);
            profileFooter.Location = new Point(34, 485);
            profileFooter.Name = "profileFooter";
            profileFooter.Size = new Size(687, 19);
            profileFooter.TabIndex = 21;
            profileFooter.Text = "ACCOUNT DETAILS ARE READ-ONLY. USE EDIT MY INFO TO UPDATE YOUR PERSONAL RECORD.";
            // 
            // tabPageAttendance
            // 
            tabPageAttendance.BackColor = Color.FromArgb(241, 245, 249);
            tabPageAttendance.Controls.Add(_attendancePage);
            tabPageAttendance.Location = new Point(4, 29);
            tabPageAttendance.Name = "tabPageAttendance";
            tabPageAttendance.Padding = new Padding(3);
            tabPageAttendance.Size = new Size(1266, 661);
            tabPageAttendance.TabIndex = 1;
            tabPageAttendance.Text = "MY ATTENDANCE";
            // 
            // _attendancePage
            // 
            _attendancePage.Controls.Add(attendanceGrid);
            _attendancePage.Dock = DockStyle.Fill;
            _attendancePage.Location = new Point(3, 3);
            _attendancePage.Name = "_attendancePage";
            _attendancePage.Padding = new Padding(26, 18, 26, 22);
            _attendancePage.Size = new Size(1260, 655);
            _attendancePage.TabIndex = 3;
            // 
            // attendanceGrid
            // 
            attendanceGrid.ColumnCount = 1;
            attendanceGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            attendanceGrid.Controls.Add(attendanceHead, 0, 0);
            attendanceGrid.Controls.Add(attendanceStatRow, 0, 1);
            attendanceGrid.Controls.Add(attendanceListHost, 0, 3);
            attendanceGrid.Dock = DockStyle.Fill;
            attendanceGrid.Location = new Point(26, 18);
            attendanceGrid.Name = "attendanceGrid";
            attendanceGrid.RowCount = 4;
            attendanceGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 92F));
            attendanceGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));
            attendanceGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            attendanceGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            attendanceGrid.Size = new Size(1208, 615);
            attendanceGrid.TabIndex = 0;
            // 
            // attendanceHead
            // 
            attendanceHead.Controls.Add(_attendanceSearch);
            attendanceHead.Controls.Add(attendanceSubtitle);
            attendanceHead.Controls.Add(attendanceTitle);
            attendanceHead.Controls.Add(attendanceRefresh);
            attendanceHead.Dock = DockStyle.Fill;
            attendanceHead.Location = new Point(3, 3);
            attendanceHead.Name = "attendanceHead";
            attendanceHead.Size = new Size(1202, 86);
            attendanceHead.TabIndex = 0;
            // 
            // _attendanceSearch
            // 
            _attendanceSearch.BackColor = Color.White;
            _attendanceSearch.Location = new Point(820, 10);
            _attendanceSearch.Name = "_attendanceSearch";
            _attendanceSearch.PlaceholderText = "SEARCH VISITS...";
            _attendanceSearch.SearchText = "";
            _attendanceSearch.Size = new Size(360, 44);
            _attendanceSearch.TabIndex = 0;
            // 
            // attendanceSubtitle
            // 
            attendanceSubtitle.AutoEllipsis = true;
            attendanceSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
            attendanceSubtitle.Location = new Point(2, 55);
            attendanceSubtitle.Name = "attendanceSubtitle";
            attendanceSubtitle.Size = new Size(791, 22);
            attendanceSubtitle.TabIndex = 1;
            attendanceSubtitle.Text = "EVERY QR CODE SCAN AT THE LRC OFFICE ENTRANCE IS RECORDED HERE. PRESENT YOUR QR CODE FROM MY PROFILE UPON ARRIVAL.\r\n";
            // 
            // attendanceTitle
            // 
            attendanceTitle.AutoSize = true;
            attendanceTitle.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            attendanceTitle.ForeColor = Color.FromArgb(15, 23, 42);
            attendanceTitle.Location = new Point(0, 0);
            attendanceTitle.Name = "attendanceTitle";
            attendanceTitle.Size = new Size(392, 60);
            attendanceTitle.TabIndex = 2;
            attendanceTitle.Text = "MY ATTENDANCE";
            // 
            // attendanceRefresh
            // 
            attendanceRefresh.BackColor = Color.Transparent;
            attendanceRefresh.CornerRadius = 11;
            attendanceRefresh.DisabledTextColor = Color.FromArgb(248, 250, 252);
            attendanceRefresh.FlatAppearance.BorderSize = 0;
            attendanceRefresh.FlatStyle = FlatStyle.Flat;
            attendanceRefresh.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            attendanceRefresh.ForeColor = Color.White;
            attendanceRefresh.HoverColor = Color.FromArgb(21, 128, 61);
            attendanceRefresh.IconFallbackGlyph = "";
            attendanceRefresh.IconGlyph = "";
            attendanceRefresh.IconSize = 16F;
            attendanceRefresh.Location = new Point(28, 10);
            attendanceRefresh.Name = "attendanceRefresh";
            attendanceRefresh.NormalColor = Color.FromArgb(22, 163, 74);
            attendanceRefresh.PressedColor = Color.FromArgb(20, 83, 45);
            attendanceRefresh.Size = new Size(114, 44);
            attendanceRefresh.TabIndex = 3;
            attendanceRefresh.Text = "REFRESH";
            attendanceRefresh.UseVisualStyleBackColor = false;
            // 
            // attendanceStatRow
            // 
            attendanceStatRow.Controls.Add(_attendanceStatTotal);
            attendanceStatRow.Controls.Add(_attendanceStatToday);
            attendanceStatRow.Controls.Add(_attendanceStatMonth);
            attendanceStatRow.Controls.Add(_attendanceStatLast);
            attendanceStatRow.Dock = DockStyle.Fill;
            attendanceStatRow.Location = new Point(3, 95);
            attendanceStatRow.Name = "attendanceStatRow";
            attendanceStatRow.Padding = new Padding(0, 4, 0, 4);
            attendanceStatRow.Size = new Size(1202, 84);
            attendanceStatRow.TabIndex = 1;
            attendanceStatRow.WrapContents = false;
            // 
            // _attendanceStatTotal
            // 
            _attendanceStatTotal.AccentColor = Color.FromArgb(22, 163, 74);
            _attendanceStatTotal.BackColor = Color.FromArgb(241, 245, 249);
            _attendanceStatTotal.Caption = "TOTAL VISITS";
            _attendanceStatTotal.Location = new Point(0, 4);
            _attendanceStatTotal.Margin = new Padding(0, 0, 14, 0);
            _attendanceStatTotal.Name = "_attendanceStatTotal";
            _attendanceStatTotal.Size = new Size(110, 78);
            _attendanceStatTotal.TabIndex = 0;
            _attendanceStatTotal.TabStop = false;
            _attendanceStatTotal.Value = "0";
            _attendanceStatTotal.ValueColor = Color.FromArgb(15, 23, 42);
            // 
            // _attendanceStatToday
            // 
            _attendanceStatToday.AccentColor = Color.FromArgb(13, 148, 136);
            _attendanceStatToday.BackColor = Color.FromArgb(241, 245, 249);
            _attendanceStatToday.Caption = "VISITS TODAY";
            _attendanceStatToday.Location = new Point(124, 4);
            _attendanceStatToday.Margin = new Padding(0, 0, 14, 0);
            _attendanceStatToday.Name = "_attendanceStatToday";
            _attendanceStatToday.Size = new Size(110, 78);
            _attendanceStatToday.TabIndex = 1;
            _attendanceStatToday.TabStop = false;
            _attendanceStatToday.Value = "0";
            _attendanceStatToday.ValueColor = Color.FromArgb(15, 23, 42);
            // 
            // _attendanceStatMonth
            // 
            _attendanceStatMonth.AccentColor = Color.FromArgb(37, 99, 235);
            _attendanceStatMonth.BackColor = Color.FromArgb(241, 245, 249);
            _attendanceStatMonth.Caption = "VISITS THIS MONTH";
            _attendanceStatMonth.Location = new Point(248, 4);
            _attendanceStatMonth.Margin = new Padding(0, 0, 14, 0);
            _attendanceStatMonth.Name = "_attendanceStatMonth";
            _attendanceStatMonth.Size = new Size(110, 78);
            _attendanceStatMonth.TabIndex = 2;
            _attendanceStatMonth.TabStop = false;
            _attendanceStatMonth.Value = "0";
            _attendanceStatMonth.ValueColor = Color.FromArgb(15, 23, 42);
            // 
            // _attendanceStatLast
            // 
            _attendanceStatLast.AccentColor = Color.FromArgb(217, 119, 6);
            _attendanceStatLast.BackColor = Color.FromArgb(241, 245, 249);
            _attendanceStatLast.Caption = "LAST VISIT";
            _attendanceStatLast.Location = new Point(372, 4);
            _attendanceStatLast.Margin = new Padding(0);
            _attendanceStatLast.Name = "_attendanceStatLast";
            _attendanceStatLast.Size = new Size(110, 78);
            _attendanceStatLast.TabIndex = 3;
            _attendanceStatLast.TabStop = false;
            _attendanceStatLast.Value = "0";
            _attendanceStatLast.ValueColor = Color.FromArgb(15, 23, 42);
            // 
            // attendanceListHost
            // 
            attendanceListHost.Controls.Add(_attendanceFlow);
            attendanceListHost.Controls.Add(_attendanceEmpty);
            attendanceListHost.Dock = DockStyle.Fill;
            attendanceListHost.Location = new Point(3, 213);
            attendanceListHost.Name = "attendanceListHost";
            attendanceListHost.Size = new Size(1202, 399);
            attendanceListHost.TabIndex = 3;
            // 
            // _attendanceFlow
            // 
            _attendanceFlow.AutoScroll = true;
            _attendanceFlow.Dock = DockStyle.Fill;
            _attendanceFlow.Location = new Point(0, 0);
            _attendanceFlow.Name = "_attendanceFlow";
            _attendanceFlow.Padding = new Padding(0, 0, 4, 4);
            _attendanceFlow.Size = new Size(1202, 399);
            _attendanceFlow.TabIndex = 0;
            // 
            // _attendanceEmpty
            // 
            _attendanceEmpty.BackColor = Color.Transparent;
            _attendanceEmpty.BorderColor = Color.FromArgb(226, 232, 240);
            _attendanceEmpty.Caption = "SHOW YOUR QR CODE TO THE LIBRARIAN AT THE LIBRARY OFFICE ENTRANCE.";
            _attendanceEmpty.CaptionColor = Color.FromArgb(148, 163, 184);
            _attendanceEmpty.CornerRadius = 20;
            _attendanceEmpty.Dock = DockStyle.Fill;
            _attendanceEmpty.FillColor = Color.FromArgb(248, 250, 252);
            _attendanceEmpty.Location = new Point(0, 0);
            _attendanceEmpty.Name = "_attendanceEmpty";
            _attendanceEmpty.Size = new Size(1202, 399);
            _attendanceEmpty.TabIndex = 1;
            _attendanceEmpty.TabStop = false;
            _attendanceEmpty.Title = "NO ATTENDANCE RECORDED YET";
            _attendanceEmpty.TitleColor = Color.FromArgb(100, 116, 139);
            _attendanceEmpty.Visible = false;
            // 
            // tabPageCatalog
            // 
            tabPageCatalog.BackColor = Color.FromArgb(241, 245, 249);
            tabPageCatalog.Controls.Add(_catalogPage);
            tabPageCatalog.Location = new Point(4, 29);
            tabPageCatalog.Name = "tabPageCatalog";
            tabPageCatalog.Padding = new Padding(3);
            tabPageCatalog.Size = new Size(1266, 661);
            tabPageCatalog.TabIndex = 2;
            tabPageCatalog.Text = "BOOK CATALOG";
            // 
            // _catalogPage
            // 
            _catalogPage.Controls.Add(catalogGrid);
            _catalogPage.Dock = DockStyle.Fill;
            _catalogPage.Location = new Point(3, 3);
            _catalogPage.Name = "_catalogPage";
            _catalogPage.Padding = new Padding(26, 18, 26, 22);
            _catalogPage.Size = new Size(1260, 655);
            _catalogPage.TabIndex = 0;
            // 
            // catalogGrid
            // 
            catalogGrid.ColumnCount = 1;
            catalogGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            catalogGrid.Controls.Add(catalogHead, 0, 0);
            catalogGrid.Controls.Add(catalogStatRow, 0, 1);
            catalogGrid.Controls.Add(catalogListHost, 0, 3);
            catalogGrid.Dock = DockStyle.Fill;
            catalogGrid.Location = new Point(26, 18);
            catalogGrid.Name = "catalogGrid";
            catalogGrid.RowCount = 4;
            catalogGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 92F));
            catalogGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));
            catalogGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            catalogGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            catalogGrid.Size = new Size(1208, 615);
            catalogGrid.TabIndex = 0;
            // 
            // catalogHead
            // 
            catalogHead.Controls.Add(_catalogSearch);
            catalogHead.Controls.Add(catalogSubtitle);
            catalogHead.Controls.Add(catalogTitle);
            catalogHead.Controls.Add(catalogRefresh);
            catalogHead.Dock = DockStyle.Fill;
            catalogHead.Location = new Point(3, 3);
            catalogHead.Name = "catalogHead";
            catalogHead.Size = new Size(1202, 86);
            catalogHead.TabIndex = 0;
            // 
            // _catalogSearch
            // 
            _catalogSearch.BackColor = Color.White;
            _catalogSearch.Location = new Point(570, 10);
            _catalogSearch.Name = "_catalogSearch";
            _catalogSearch.PlaceholderText = "SEARCH BOOKS...";
            _catalogSearch.SearchText = "";
            _catalogSearch.Size = new Size(360, 44);
            _catalogSearch.TabIndex = 0;
            // 
            // catalogSubtitle
            // 
            catalogSubtitle.AutoEllipsis = true;
            catalogSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
            catalogSubtitle.Location = new Point(1, 58);
            catalogSubtitle.Name = "catalogSubtitle";
            catalogSubtitle.Size = new Size(554, 22);
            catalogSubtitle.TabIndex = 1;
            catalogSubtitle.Text = "BROWSE THE SHELVES, SELECT A BOOK COVER, AND REQUEST THE TITLE FROM THE LIBRARIAN.";
            // 
            // catalogTitle
            // 
            catalogTitle.AutoSize = true;
            catalogTitle.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            catalogTitle.ForeColor = Color.FromArgb(15, 23, 42);
            catalogTitle.Location = new Point(0, 0);
            catalogTitle.Name = "catalogTitle";
            catalogTitle.Size = new Size(355, 60);
            catalogTitle.TabIndex = 2;
            catalogTitle.Text = "BOOK CATALOG";
            // 
            // catalogRefresh
            // 
            catalogRefresh.BackColor = Color.Transparent;
            catalogRefresh.CornerRadius = 11;
            catalogRefresh.DisabledTextColor = Color.FromArgb(248, 250, 252);
            catalogRefresh.FlatAppearance.BorderSize = 0;
            catalogRefresh.FlatStyle = FlatStyle.Flat;
            catalogRefresh.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            catalogRefresh.ForeColor = Color.White;
            catalogRefresh.HoverColor = Color.FromArgb(21, 128, 61);
            catalogRefresh.IconFallbackGlyph = "";
            catalogRefresh.IconGlyph = "";
            catalogRefresh.IconSize = 16F;
            catalogRefresh.Location = new Point(942, 10);
            catalogRefresh.Name = "catalogRefresh";
            catalogRefresh.NormalColor = Color.FromArgb(22, 163, 74);
            catalogRefresh.PressedColor = Color.FromArgb(20, 83, 45);
            catalogRefresh.Size = new Size(114, 44);
            catalogRefresh.TabIndex = 3;
            catalogRefresh.Text = "REFRESH";
            catalogRefresh.UseVisualStyleBackColor = false;
            // 
            // catalogStatRow
            // 
            catalogStatRow.Controls.Add(_catalogStatTitles);
            catalogStatRow.Controls.Add(_catalogStatCopies);
            catalogStatRow.Controls.Add(_catalogStatBorrowed);
            catalogStatRow.Controls.Add(_catalogStatPending);
            catalogStatRow.Dock = DockStyle.Fill;
            catalogStatRow.Location = new Point(3, 95);
            catalogStatRow.Name = "catalogStatRow";
            catalogStatRow.Padding = new Padding(0, 4, 0, 4);
            catalogStatRow.Size = new Size(1202, 84);
            catalogStatRow.TabIndex = 1;
            catalogStatRow.WrapContents = false;
            // 
            // _catalogStatTitles
            // 
            _catalogStatTitles.AccentColor = Color.FromArgb(22, 163, 74);
            _catalogStatTitles.BackColor = Color.FromArgb(241, 245, 249);
            _catalogStatTitles.Caption = "TITLES IN CATALOG";
            _catalogStatTitles.Location = new Point(0, 4);
            _catalogStatTitles.Margin = new Padding(0, 0, 14, 0);
            _catalogStatTitles.Name = "_catalogStatTitles";
            _catalogStatTitles.Size = new Size(253, 78);
            _catalogStatTitles.TabIndex = 0;
            _catalogStatTitles.TabStop = false;
            _catalogStatTitles.Value = "0";
            _catalogStatTitles.ValueColor = Color.FromArgb(15, 23, 42);
            // 
            // _catalogStatCopies
            // 
            _catalogStatCopies.AccentColor = Color.FromArgb(13, 148, 136);
            _catalogStatCopies.BackColor = Color.FromArgb(241, 245, 249);
            _catalogStatCopies.Caption = "COPIES ON THE SHELF";
            _catalogStatCopies.Location = new Point(267, 4);
            _catalogStatCopies.Margin = new Padding(0, 0, 14, 0);
            _catalogStatCopies.Name = "_catalogStatCopies";
            _catalogStatCopies.Size = new Size(253, 78);
            _catalogStatCopies.TabIndex = 1;
            _catalogStatCopies.TabStop = false;
            _catalogStatCopies.Value = "0";
            _catalogStatCopies.ValueColor = Color.FromArgb(15, 23, 42);
            // 
            // _catalogStatBorrowed
            // 
            _catalogStatBorrowed.AccentColor = Color.FromArgb(37, 99, 235);
            _catalogStatBorrowed.BackColor = Color.FromArgb(241, 245, 249);
            _catalogStatBorrowed.Caption = "YOUR BORROWED BOOKS";
            _catalogStatBorrowed.Location = new Point(534, 4);
            _catalogStatBorrowed.Margin = new Padding(0, 0, 14, 0);
            _catalogStatBorrowed.Name = "_catalogStatBorrowed";
            _catalogStatBorrowed.Size = new Size(253, 78);
            _catalogStatBorrowed.TabIndex = 2;
            _catalogStatBorrowed.TabStop = false;
            _catalogStatBorrowed.Value = "0";
            _catalogStatBorrowed.ValueColor = Color.FromArgb(15, 23, 42);
            // 
            // _catalogStatPending
            // 
            _catalogStatPending.AccentColor = Color.FromArgb(217, 119, 6);
            _catalogStatPending.BackColor = Color.FromArgb(241, 245, 249);
            _catalogStatPending.Caption = "YOUR PENDING REQUESTS";
            _catalogStatPending.Location = new Point(801, 4);
            _catalogStatPending.Margin = new Padding(0);
            _catalogStatPending.Name = "_catalogStatPending";
            _catalogStatPending.Size = new Size(253, 78);
            _catalogStatPending.TabIndex = 3;
            _catalogStatPending.TabStop = false;
            _catalogStatPending.Value = "0";
            _catalogStatPending.ValueColor = Color.FromArgb(15, 23, 42);
            _catalogStatPending.Click += _catalogStatPending_Click;
            // 
            // catalogListHost
            // 
            catalogListHost.Controls.Add(_catalogFlow);
            catalogListHost.Controls.Add(_catalogEmpty);
            catalogListHost.Dock = DockStyle.Fill;
            catalogListHost.Location = new Point(3, 213);
            catalogListHost.Name = "catalogListHost";
            catalogListHost.Size = new Size(1202, 399);
            catalogListHost.TabIndex = 3;
            // 
            // _catalogFlow
            // 
            _catalogFlow.AutoScroll = true;
            _catalogFlow.Dock = DockStyle.Fill;
            _catalogFlow.Location = new Point(0, 0);
            _catalogFlow.Name = "_catalogFlow";
            _catalogFlow.Padding = new Padding(0, 0, 4, 4);
            _catalogFlow.Size = new Size(1202, 399);
            _catalogFlow.TabIndex = 0;
            // 
            // _catalogEmpty
            // 
            _catalogEmpty.BackColor = Color.Transparent;
            _catalogEmpty.BorderColor = Color.FromArgb(226, 232, 240);
            _catalogEmpty.Caption = "PLEASE ASK THE LIBRARIAN TO ADD A TITLE, THEN PRESS REFRESH.";
            _catalogEmpty.CaptionColor = Color.FromArgb(148, 163, 184);
            _catalogEmpty.CornerRadius = 20;
            _catalogEmpty.Dock = DockStyle.Fill;
            _catalogEmpty.FillColor = Color.FromArgb(248, 250, 252);
            _catalogEmpty.Location = new Point(0, 0);
            _catalogEmpty.Name = "_catalogEmpty";
            _catalogEmpty.Size = new Size(1202, 399);
            _catalogEmpty.TabIndex = 1;
            _catalogEmpty.TabStop = false;
            _catalogEmpty.Title = "NO BOOKS IN THE CATALOG YET";
            _catalogEmpty.TitleColor = Color.FromArgb(100, 116, 139);
            _catalogEmpty.Visible = false;
            // 
            // tabPageRequests
            // 
            tabPageRequests.BackColor = Color.FromArgb(241, 245, 249);
            tabPageRequests.Controls.Add(_requestsPage);
            tabPageRequests.Location = new Point(4, 29);
            tabPageRequests.Name = "tabPageRequests";
            tabPageRequests.Padding = new Padding(3);
            tabPageRequests.Size = new Size(1266, 661);
            tabPageRequests.TabIndex = 3;
            tabPageRequests.Text = "MY REQUESTS";
            // 
            // _requestsPage
            // 
            _requestsPage.Controls.Add(requestsGrid);
            _requestsPage.Dock = DockStyle.Fill;
            _requestsPage.Location = new Point(3, 3);
            _requestsPage.Name = "_requestsPage";
            _requestsPage.Padding = new Padding(26, 18, 26, 22);
            _requestsPage.Size = new Size(1260, 655);
            _requestsPage.TabIndex = 1;
            // 
            // requestsGrid
            // 
            requestsGrid.ColumnCount = 1;
            requestsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            requestsGrid.Controls.Add(requestsHead, 0, 0);
            requestsGrid.Controls.Add(requestsStatRow, 0, 1);
            requestsGrid.Controls.Add(requestsListHost, 0, 3);
            requestsGrid.Dock = DockStyle.Fill;
            requestsGrid.Location = new Point(26, 18);
            requestsGrid.Name = "requestsGrid";
            requestsGrid.RowCount = 4;
            requestsGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 92F));
            requestsGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));
            requestsGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            requestsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            requestsGrid.Size = new Size(1208, 615);
            requestsGrid.TabIndex = 0;
            // 
            // requestsHead
            // 
            requestsHead.Controls.Add(_requestsSearch);
            requestsHead.Controls.Add(requestsTitle);
            requestsHead.Controls.Add(requestsRefresh);
            requestsHead.Dock = DockStyle.Fill;
            requestsHead.Location = new Point(3, 3);
            requestsHead.Name = "requestsHead";
            requestsHead.Size = new Size(1202, 86);
            requestsHead.TabIndex = 0;
            // 
            // _requestsSearch
            // 
            _requestsSearch.BackColor = Color.White;
            _requestsSearch.Location = new Point(816, 10);
            _requestsSearch.Name = "_requestsSearch";
            _requestsSearch.PlaceholderText = "SEARCH REQUESTS...";
            _requestsSearch.SearchText = "";
            _requestsSearch.Size = new Size(360, 44);
            _requestsSearch.TabIndex = 0;
            // 
            // requestsTitle
            // 
            requestsTitle.AutoSize = true;
            requestsTitle.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            requestsTitle.ForeColor = Color.FromArgb(15, 23, 42);
            requestsTitle.Location = new Point(0, 0);
            requestsTitle.Name = "requestsTitle";
            requestsTitle.Size = new Size(322, 60);
            requestsTitle.TabIndex = 2;
            requestsTitle.Text = "MY REQUESTS";
            // 
            // requestsRefresh
            // 
            requestsRefresh.BackColor = Color.Transparent;
            requestsRefresh.CornerRadius = 11;
            requestsRefresh.DisabledTextColor = Color.FromArgb(248, 250, 252);
            requestsRefresh.FlatAppearance.BorderSize = 0;
            requestsRefresh.FlatStyle = FlatStyle.Flat;
            requestsRefresh.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            requestsRefresh.ForeColor = Color.White;
            requestsRefresh.HoverColor = Color.FromArgb(21, 128, 61);
            requestsRefresh.IconFallbackGlyph = "";
            requestsRefresh.IconGlyph = "";
            requestsRefresh.IconSize = 16F;
            requestsRefresh.Location = new Point(28, 10);
            requestsRefresh.Name = "requestsRefresh";
            requestsRefresh.NormalColor = Color.FromArgb(22, 163, 74);
            requestsRefresh.PressedColor = Color.FromArgb(20, 83, 45);
            requestsRefresh.Size = new Size(114, 44);
            requestsRefresh.TabIndex = 3;
            requestsRefresh.Text = "REFRESH";
            requestsRefresh.UseVisualStyleBackColor = false;
            // 
            // requestsStatRow
            // 
            requestsStatRow.Controls.Add(_requestStatTotal);
            requestsStatRow.Dock = DockStyle.Fill;
            requestsStatRow.Location = new Point(3, 95);
            requestsStatRow.Name = "requestsStatRow";
            requestsStatRow.Padding = new Padding(0, 4, 0, 4);
            requestsStatRow.Size = new Size(1202, 84);
            requestsStatRow.TabIndex = 1;
            requestsStatRow.WrapContents = false;
            // 
            // _requestStatTotal
            // 
            _requestStatTotal.AccentColor = Color.FromArgb(22, 163, 74);
            _requestStatTotal.BackColor = Color.FromArgb(241, 245, 249);
            _requestStatTotal.Caption = "TOTAL REQUESTS";
            _requestStatTotal.Location = new Point(0, 4);
            _requestStatTotal.Margin = new Padding(0, 0, 14, 0);
            _requestStatTotal.Name = "_requestStatTotal";
            _requestStatTotal.Size = new Size(110, 78);
            _requestStatTotal.TabIndex = 0;
            _requestStatTotal.TabStop = false;
            _requestStatTotal.Value = "0";
            _requestStatTotal.ValueColor = Color.FromArgb(15, 23, 42);
            // 
            // requestsListHost
            // 
            requestsListHost.Controls.Add(_requestFlow);
            requestsListHost.Controls.Add(_requestsEmpty);
            requestsListHost.Dock = DockStyle.Fill;
            requestsListHost.Location = new Point(3, 213);
            requestsListHost.Name = "requestsListHost";
            requestsListHost.Size = new Size(1202, 399);
            requestsListHost.TabIndex = 3;
            // 
            // _requestFlow
            // 
            _requestFlow.AutoScroll = true;
            _requestFlow.Dock = DockStyle.Fill;
            _requestFlow.Location = new Point(0, 0);
            _requestFlow.Name = "_requestFlow";
            _requestFlow.Padding = new Padding(0, 0, 4, 4);
            _requestFlow.Size = new Size(1202, 399);
            _requestFlow.TabIndex = 0;
            // 
            // _requestsEmpty
            // 
            _requestsEmpty.BackColor = Color.Transparent;
            _requestsEmpty.BorderColor = Color.FromArgb(226, 232, 240);
            _requestsEmpty.Caption = "OPEN THE BOOK CATALOG AND PRESS REQUEST TO BORROW ON A TITLE.";
            _requestsEmpty.CaptionColor = Color.FromArgb(148, 163, 184);
            _requestsEmpty.CornerRadius = 20;
            _requestsEmpty.Dock = DockStyle.Fill;
            _requestsEmpty.FillColor = Color.FromArgb(248, 250, 252);
            _requestsEmpty.Location = new Point(0, 0);
            _requestsEmpty.Name = "_requestsEmpty";
            _requestsEmpty.Size = new Size(1202, 399);
            _requestsEmpty.TabIndex = 1;
            _requestsEmpty.TabStop = false;
            _requestsEmpty.Title = "NO REQUESTS YET";
            _requestsEmpty.TitleColor = Color.FromArgb(100, 116, 139);
            _requestsEmpty.Visible = false;
            // 
            // tabPageBorrowed
            // 
            tabPageBorrowed.BackColor = Color.FromArgb(241, 245, 249);
            tabPageBorrowed.Controls.Add(_borrowedPage);
            tabPageBorrowed.Location = new Point(4, 29);
            tabPageBorrowed.Name = "tabPageBorrowed";
            tabPageBorrowed.Padding = new Padding(3);
            tabPageBorrowed.Size = new Size(1266, 661);
            tabPageBorrowed.TabIndex = 4;
            tabPageBorrowed.Text = "MY BORROWED BOOKS";
            // 
            // _borrowedPage
            // 
            _borrowedPage.Controls.Add(borrowedGrid);
            _borrowedPage.Dock = DockStyle.Fill;
            _borrowedPage.Location = new Point(3, 3);
            _borrowedPage.Name = "_borrowedPage";
            _borrowedPage.Padding = new Padding(26, 18, 26, 22);
            _borrowedPage.Size = new Size(1260, 655);
            _borrowedPage.TabIndex = 2;
            // 
            // borrowedGrid
            // 
            borrowedGrid.ColumnCount = 1;
            borrowedGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            borrowedGrid.Controls.Add(borrowedHead, 0, 0);
            borrowedGrid.Controls.Add(borrowedStatRow, 0, 1);
            borrowedGrid.Controls.Add(borrowedListHost, 0, 3);
            borrowedGrid.Dock = DockStyle.Fill;
            borrowedGrid.Location = new Point(26, 18);
            borrowedGrid.Name = "borrowedGrid";
            borrowedGrid.RowCount = 4;
            borrowedGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 92F));
            borrowedGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));
            borrowedGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            borrowedGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            borrowedGrid.Size = new Size(1208, 615);
            borrowedGrid.TabIndex = 0;
            // 
            // borrowedHead
            // 
            borrowedHead.Controls.Add(_borrowedSearch);
            borrowedHead.Controls.Add(borrowedTitle);
            borrowedHead.Controls.Add(borrowedRefresh);
            borrowedHead.Dock = DockStyle.Fill;
            borrowedHead.Location = new Point(3, 3);
            borrowedHead.Name = "borrowedHead";
            borrowedHead.Size = new Size(1202, 86);
            borrowedHead.TabIndex = 0;
            // 
            // _borrowedSearch
            // 
            _borrowedSearch.BackColor = Color.White;
            _borrowedSearch.Location = new Point(825, 10);
            _borrowedSearch.Name = "_borrowedSearch";
            _borrowedSearch.PlaceholderText = "SEARCH BORROWED BOOKS...";
            _borrowedSearch.SearchText = "";
            _borrowedSearch.Size = new Size(360, 44);
            _borrowedSearch.TabIndex = 0;
            // 
            // borrowedTitle
            // 
            borrowedTitle.AutoSize = true;
            borrowedTitle.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            borrowedTitle.ForeColor = Color.FromArgb(15, 23, 42);
            borrowedTitle.Location = new Point(0, 0);
            borrowedTitle.Name = "borrowedTitle";
            borrowedTitle.Size = new Size(517, 60);
            borrowedTitle.TabIndex = 2;
            borrowedTitle.Text = "MY BORROWED BOOKS";
            // 
            // borrowedRefresh
            // 
            borrowedRefresh.BackColor = Color.Transparent;
            borrowedRefresh.CornerRadius = 11;
            borrowedRefresh.DisabledTextColor = Color.FromArgb(248, 250, 252);
            borrowedRefresh.FlatAppearance.BorderSize = 0;
            borrowedRefresh.FlatStyle = FlatStyle.Flat;
            borrowedRefresh.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            borrowedRefresh.ForeColor = Color.White;
            borrowedRefresh.HoverColor = Color.FromArgb(21, 128, 61);
            borrowedRefresh.IconFallbackGlyph = "";
            borrowedRefresh.IconGlyph = "";
            borrowedRefresh.IconSize = 16F;
            borrowedRefresh.Location = new Point(28, 10);
            borrowedRefresh.Name = "borrowedRefresh";
            borrowedRefresh.NormalColor = Color.FromArgb(22, 163, 74);
            borrowedRefresh.PressedColor = Color.FromArgb(20, 83, 45);
            borrowedRefresh.Size = new Size(114, 44);
            borrowedRefresh.TabIndex = 3;
            borrowedRefresh.Text = "REFRESH";
            borrowedRefresh.UseVisualStyleBackColor = false;
            // 
            // borrowedStatRow
            // 
            borrowedStatRow.Controls.Add(_borrowedStatActive);
            borrowedStatRow.Controls.Add(_borrowedStatReturned);
            borrowedStatRow.Controls.Add(_borrowedStatTotal);
            borrowedStatRow.Controls.Add(_borrowedStatLongest);
            borrowedStatRow.Dock = DockStyle.Fill;
            borrowedStatRow.Location = new Point(3, 95);
            borrowedStatRow.Name = "borrowedStatRow";
            borrowedStatRow.Padding = new Padding(0, 4, 0, 4);
            borrowedStatRow.Size = new Size(1202, 84);
            borrowedStatRow.TabIndex = 1;
            borrowedStatRow.WrapContents = false;
            // 
            // _borrowedStatActive
            // 
            _borrowedStatActive.AccentColor = Color.FromArgb(22, 163, 74);
            _borrowedStatActive.BackColor = Color.FromArgb(241, 245, 249);
            _borrowedStatActive.Caption = "CURRENTLY BORROWED";
            _borrowedStatActive.Location = new Point(0, 4);
            _borrowedStatActive.Margin = new Padding(0, 0, 14, 0);
            _borrowedStatActive.Name = "_borrowedStatActive";
            _borrowedStatActive.Size = new Size(110, 78);
            _borrowedStatActive.TabIndex = 0;
            _borrowedStatActive.TabStop = false;
            _borrowedStatActive.Value = "0";
            _borrowedStatActive.ValueColor = Color.FromArgb(15, 23, 42);
            // 
            // _borrowedStatReturned
            // 
            _borrowedStatReturned.AccentColor = Color.FromArgb(100, 116, 139);
            _borrowedStatReturned.BackColor = Color.FromArgb(241, 245, 249);
            _borrowedStatReturned.Caption = "ALREADY RETURNED";
            _borrowedStatReturned.Location = new Point(124, 4);
            _borrowedStatReturned.Margin = new Padding(0, 0, 14, 0);
            _borrowedStatReturned.Name = "_borrowedStatReturned";
            _borrowedStatReturned.Size = new Size(110, 78);
            _borrowedStatReturned.TabIndex = 1;
            _borrowedStatReturned.TabStop = false;
            _borrowedStatReturned.Value = "0";
            _borrowedStatReturned.ValueColor = Color.FromArgb(15, 23, 42);
            // 
            // _borrowedStatTotal
            // 
            _borrowedStatTotal.AccentColor = Color.FromArgb(37, 99, 235);
            _borrowedStatTotal.BackColor = Color.FromArgb(241, 245, 249);
            _borrowedStatTotal.Caption = "TOTAL BORROW RECORDS";
            _borrowedStatTotal.Location = new Point(248, 4);
            _borrowedStatTotal.Margin = new Padding(0, 0, 14, 0);
            _borrowedStatTotal.Name = "_borrowedStatTotal";
            _borrowedStatTotal.Size = new Size(110, 78);
            _borrowedStatTotal.TabIndex = 2;
            _borrowedStatTotal.TabStop = false;
            _borrowedStatTotal.Value = "0";
            _borrowedStatTotal.ValueColor = Color.FromArgb(15, 23, 42);
            // 
            // _borrowedStatLongest
            // 
            _borrowedStatLongest.AccentColor = Color.FromArgb(217, 119, 6);
            _borrowedStatLongest.BackColor = Color.FromArgb(241, 245, 249);
            _borrowedStatLongest.Caption = "LONGEST ACTIVE BORROW";
            _borrowedStatLongest.Location = new Point(372, 4);
            _borrowedStatLongest.Margin = new Padding(0);
            _borrowedStatLongest.Name = "_borrowedStatLongest";
            _borrowedStatLongest.Size = new Size(110, 78);
            _borrowedStatLongest.TabIndex = 3;
            _borrowedStatLongest.TabStop = false;
            _borrowedStatLongest.Value = "0";
            _borrowedStatLongest.ValueColor = Color.FromArgb(15, 23, 42);
            // 
            // borrowedListHost
            // 
            borrowedListHost.Controls.Add(_borrowedFlow);
            borrowedListHost.Controls.Add(_borrowedEmpty);
            borrowedListHost.Dock = DockStyle.Fill;
            borrowedListHost.Location = new Point(3, 213);
            borrowedListHost.Name = "borrowedListHost";
            borrowedListHost.Size = new Size(1202, 399);
            borrowedListHost.TabIndex = 3;
            // 
            // _borrowedFlow
            // 
            _borrowedFlow.AutoScroll = true;
            _borrowedFlow.Dock = DockStyle.Fill;
            _borrowedFlow.Location = new Point(0, 0);
            _borrowedFlow.Name = "_borrowedFlow";
            _borrowedFlow.Padding = new Padding(0, 0, 4, 4);
            _borrowedFlow.Size = new Size(1202, 399);
            _borrowedFlow.TabIndex = 0;
            // 
            // _borrowedEmpty
            // 
            _borrowedEmpty.BackColor = Color.Transparent;
            _borrowedEmpty.BorderColor = Color.FromArgb(226, 232, 240);
            _borrowedEmpty.Caption = "WHEN THE LIBRARIAN RELEASES AN APPROVED REQUEST, THE BORROWED BOOK APPEARS HERE.";
            _borrowedEmpty.CaptionColor = Color.FromArgb(148, 163, 184);
            _borrowedEmpty.CornerRadius = 20;
            _borrowedEmpty.Dock = DockStyle.Fill;
            _borrowedEmpty.FillColor = Color.FromArgb(248, 250, 252);
            _borrowedEmpty.Location = new Point(0, 0);
            _borrowedEmpty.Name = "_borrowedEmpty";
            _borrowedEmpty.Size = new Size(1202, 399);
            _borrowedEmpty.TabIndex = 1;
            _borrowedEmpty.TabStop = false;
            _borrowedEmpty.Title = "NO BORROW RECORDS YET";
            _borrowedEmpty.TitleColor = Color.FromArgb(100, 116, 139);
            _borrowedEmpty.Visible = false;
            // 
            // student_faculty_staff_dashboard
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(241, 245, 249);
            ClientSize = new Size(1280, 800);
            Controls.Add(rootGrid);
            MinimumSize = new Size(1100, 700);
            Name = "student_faculty_staff_dashboard";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "TCGC - STUDENT/FACULTY/STAFF DASHBOARD";
            rootGrid.ResumeLayout(false);
            headerPanel.ResumeLayout(false);
            headerPanel.PerformLayout();
            _tabs.ResumeLayout(false);
            tabPageProfile.ResumeLayout(false);
            _profilePage.ResumeLayout(false);
            profileGrid.ResumeLayout(false);
            profileHead.ResumeLayout(false);
            profileHead.PerformLayout();
            profileColumns.ResumeLayout(false);
            profileIdentityCard.ResumeLayout(false);
            profileIdentityCard.PerformLayout();
            profileDetailsCard.ResumeLayout(false);
            profileDetailsCard.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)_qrPicture).EndInit();
            tabPageAttendance.ResumeLayout(false);
            _attendancePage.ResumeLayout(false);
            attendanceGrid.ResumeLayout(false);
            attendanceHead.ResumeLayout(false);
            attendanceHead.PerformLayout();
            attendanceStatRow.ResumeLayout(false);
            attendanceListHost.ResumeLayout(false);
            tabPageCatalog.ResumeLayout(false);
            _catalogPage.ResumeLayout(false);
            catalogGrid.ResumeLayout(false);
            catalogHead.ResumeLayout(false);
            catalogHead.PerformLayout();
            catalogStatRow.ResumeLayout(false);
            catalogListHost.ResumeLayout(false);
            tabPageRequests.ResumeLayout(false);
            _requestsPage.ResumeLayout(false);
            requestsGrid.ResumeLayout(false);
            requestsHead.ResumeLayout(false);
            requestsHead.PerformLayout();
            requestsStatRow.ResumeLayout(false);
            requestsListHost.ResumeLayout(false);
            tabPageBorrowed.ResumeLayout(false);
            _borrowedPage.ResumeLayout(false);
            borrowedGrid.ResumeLayout(false);
            borrowedHead.ResumeLayout(false);
            borrowedHead.PerformLayout();
            borrowedStatRow.ResumeLayout(false);
            borrowedListHost.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel rootGrid;
        private PatronHeaderPanel headerPanel;
        private SignOutButton _logout;
        private PillBadge _headerRole;
        private Label _headerName;
        private AvatarBadge _headerAvatar;
        private TabControl _tabs;
        private Panel _catalogPage;
        private TableLayoutPanel catalogGrid;
        private Panel catalogHead;
        private SearchBox _catalogSearch;
        private Label catalogSubtitle;
        private Label catalogTitle;
        private RoundedButton catalogRefresh;
        private FlowLayoutPanel catalogStatRow;
        private StatCard _catalogStatTitles;
        private StatCard _catalogStatCopies;
        private StatCard _catalogStatBorrowed;
        private StatCard _catalogStatPending;
        private Panel catalogListHost;
        private FlowLayoutPanel _catalogFlow;
        private EmptyState _catalogEmpty;
        private Panel _requestsPage;
        private TableLayoutPanel requestsGrid;
        private Panel requestsHead;
        private SearchBox _requestsSearch;
        private Label requestsTitle;
        private RoundedButton requestsRefresh;
        private FlowLayoutPanel requestsStatRow;
        private StatCard _requestStatTotal;
        private Panel requestsListHost;
        private FlowLayoutPanel _requestFlow;
        private EmptyState _requestsEmpty;
        private Panel _borrowedPage;
        private TableLayoutPanel borrowedGrid;
        private Panel borrowedHead;
        private SearchBox _borrowedSearch;
        private Label borrowedTitle;
        private RoundedButton borrowedRefresh;
        private FlowLayoutPanel borrowedStatRow;
        private StatCard _borrowedStatActive;
        private StatCard _borrowedStatReturned;
        private StatCard _borrowedStatTotal;
        private StatCard _borrowedStatLongest;
        private Panel borrowedListHost;
        private FlowLayoutPanel _borrowedFlow;
        private EmptyState _borrowedEmpty;
        private Panel _attendancePage;
        private TableLayoutPanel attendanceGrid;
        private Panel attendanceHead;
        private SearchBox _attendanceSearch;
        private Label attendanceSubtitle;
        private Label attendanceTitle;
        private RoundedButton attendanceRefresh;
        private FlowLayoutPanel attendanceStatRow;
        private StatCard _attendanceStatTotal;
        private StatCard _attendanceStatToday;
        private StatCard _attendanceStatMonth;
        private StatCard _attendanceStatLast;
        private Panel attendanceListHost;
        private FlowLayoutPanel _attendanceFlow;
        private EmptyState _attendanceEmpty;
        private Panel _profilePage;
        private TableLayoutPanel profileGrid;
        private Panel profileHead;
        private Label profileSubtitle;
        private Label profileTitle;
        private RoundedButton changePasswordButton;
        private RoundedButton editProfileButton;
        private TableLayoutPanel profileColumns;
        private Panel profileIdentityCard;
        private Label idNumberCaption;
        private Label _programValue;
        private Label programCaption;
        private Label _yearValue;
        private Label yearCaption;
        private Label _instituteValue;
        private Label instituteCaption;
        private Label _contactValue;
        private Label contactCaption;
        private Label _addressValue;
        private Label addressCaption;
        private Panel identityDivider;
        private Label _profileName;
        private AvatarBadge _profileAvatar;
        private PillBadge profileRoleBadge;
        private Panel profileDetailsCard;
        private Label detailsHeading;
        private Label detailsSubtitle;
        private Panel detailsTopDivider;
        private Panel dividerRow1;
        private Panel dividerRow2;
        private Panel dividerRow3;
        private Panel dividerRow4;
        private Panel dividerRow5;
        private Panel dividerRow6;
        private Label profileFooter;
        private Label _qrCaption;
        private PictureBox _qrPicture;
        private Label qrHeading;
        private RoundedButton downloadQrButton;
        private TabPage tabPageProfile;
        private TabPage tabPageAttendance;
        private TabPage tabPageCatalog;
        private TabPage tabPageRequests;
        private TabPage tabPageBorrowed;
        private Label _schoolIdValue;
    }
}
