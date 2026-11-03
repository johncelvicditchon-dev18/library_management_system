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
            _logout = new RoundedButton();
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
            profileRefresh = new RoundedButton();
            profileColumns = new TableLayoutPanel();
            profileIdentityCard = new Panel();
            _schoolIdValue = new Label();
            idNumberCaption = new Label();
            _programValue = new Label();
            programCaption = new Label();
            _yearValue = new Label();
            yearCaption = new Label();
            _instituteValue = new Label();
            instituteCaption = new Label();
            _contactValue = new Label();
            contactCaption = new Label();
            _addressValue = new Label();
            addressCaption = new Label();
            identityDivider = new Panel();
            _profileRole = new PillBadge();
            _profileMeta = new Label();
            _profileName = new Label();
            _profileAvatar = new AvatarBadge();
            identityHeading = new Label();
            profileQrCard = new Panel();
            qrKeepPrivate = new Label();
            qrStepThree = new Label();
            qrStepTwo = new Label();
            qrStepOne = new Label();
            qrHowTo = new Label();
            qrDivider = new Panel();
            _qrCaption = new Label();
            _qrPicture = new PictureBox();
            qrHeading = new Label();
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
            _catalogStatOnLoan = new StatCard();
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
            _requestStatPending = new StatCard();
            _requestStatApproved = new StatCard();
            _requestStatClosed = new StatCard();
            requestsListHost = new Panel();
            _requestFlow = new FlowLayoutPanel();
            _requestsEmpty = new EmptyState();
            tabPageLoans = new TabPage();
            _loansPage = new Panel();
            loansGrid = new TableLayoutPanel();
            loansHead = new Panel();
            _loansSearch = new SearchBox();
            loansTitle = new Label();
            loansRefresh = new RoundedButton();
            loansStatRow = new FlowLayoutPanel();
            _loanStatActive = new StatCard();
            _loanStatReturned = new StatCard();
            _loanStatTotal = new StatCard();
            _loanStatLongest = new StatCard();
            loansListHost = new Panel();
            _loanFlow = new FlowLayoutPanel();
            _loansEmpty = new EmptyState();
            rootGrid.SuspendLayout();
            headerPanel.SuspendLayout();
            _tabs.SuspendLayout();
            tabPageProfile.SuspendLayout();
            _profilePage.SuspendLayout();
            profileGrid.SuspendLayout();
            profileHead.SuspendLayout();
            profileColumns.SuspendLayout();
            profileIdentityCard.SuspendLayout();
            profileQrCard.SuspendLayout();
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
            tabPageLoans.SuspendLayout();
            _loansPage.SuspendLayout();
            loansGrid.SuspendLayout();
            loansHead.SuspendLayout();
            loansStatRow.SuspendLayout();
            loansListHost.SuspendLayout();
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
            _logout.CornerRadius = 11;
            _logout.FlatAppearance.BorderSize = 0;
            _logout.FlatStyle = FlatStyle.Flat;
            _logout.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            _logout.ForeColor = Color.White;
            _logout.HoverColor = Color.FromArgb(194, 65, 12);
            _logout.Location = new Point(2036, 30);
            _logout.Name = "_logout";
            _logout.NormalColor = Color.FromArgb(234, 88, 12);
            _logout.PressedColor = Color.FromArgb(154, 52, 18);
            _logout.Size = new Size(124, 40);
            _logout.TabIndex = 1;
            _logout.Text = "LOGOUT";
            _logout.UseVisualStyleBackColor = false;
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
            _tabs.Controls.Add(tabPageLoans);
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
            profileHead.Controls.Add(profileRefresh);
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
            profileSubtitle.Location = new Point(2, 53);
            profileSubtitle.Name = "profileSubtitle";
            profileSubtitle.Size = new Size(794, 22);
            profileSubtitle.TabIndex = 0;
            profileSubtitle.Text = "SCAN THE QR CODE UPON ENTERING THE LRC OFFICE TO RECORD ATTENDANCE.";
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
            // profileRefresh
            // 
            profileRefresh.BackColor = Color.Transparent;
            profileRefresh.CornerRadius = 11;
            profileRefresh.FlatAppearance.BorderSize = 0;
            profileRefresh.FlatStyle = FlatStyle.Flat;
            profileRefresh.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            profileRefresh.ForeColor = Color.White;
            profileRefresh.HoverColor = Color.FromArgb(21, 128, 61);
            profileRefresh.Location = new Point(28, 10);
            profileRefresh.Name = "profileRefresh";
            profileRefresh.NormalColor = Color.FromArgb(22, 163, 74);
            profileRefresh.PressedColor = Color.FromArgb(20, 83, 45);
            profileRefresh.Size = new Size(114, 44);
            profileRefresh.TabIndex = 2;
            profileRefresh.Text = "REFRESH";
            profileRefresh.UseVisualStyleBackColor = false;
            // 
            // profileColumns
            // 
            profileColumns.ColumnCount = 2;
            profileColumns.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 440F));
            profileColumns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            profileColumns.Controls.Add(profileIdentityCard, 0, 0);
            profileColumns.Controls.Add(profileQrCard, 1, 0);
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
            profileIdentityCard.Controls.Add(_schoolIdValue);
            profileIdentityCard.Controls.Add(idNumberCaption);
            profileIdentityCard.Controls.Add(_programValue);
            profileIdentityCard.Controls.Add(programCaption);
            profileIdentityCard.Controls.Add(_yearValue);
            profileIdentityCard.Controls.Add(yearCaption);
            profileIdentityCard.Controls.Add(_instituteValue);
            profileIdentityCard.Controls.Add(instituteCaption);
            profileIdentityCard.Controls.Add(_contactValue);
            profileIdentityCard.Controls.Add(contactCaption);
            profileIdentityCard.Controls.Add(_addressValue);
            profileIdentityCard.Controls.Add(addressCaption);
            profileIdentityCard.Controls.Add(identityDivider);
            profileIdentityCard.Controls.Add(_profileRole);
            profileIdentityCard.Controls.Add(_profileMeta);
            profileIdentityCard.Controls.Add(_profileName);
            profileIdentityCard.Controls.Add(_profileAvatar);
            profileIdentityCard.Controls.Add(identityHeading);
            profileIdentityCard.Dock = DockStyle.Fill;
            profileIdentityCard.Location = new Point(0, 0);
            profileIdentityCard.Margin = new Padding(0, 0, 18, 0);
            profileIdentityCard.Name = "profileIdentityCard";
            profileIdentityCard.Size = new Size(422, 517);
            profileIdentityCard.TabIndex = 0;
            // 
            // _schoolIdValue
            // 
            _schoolIdValue.AutoEllipsis = true;
            _schoolIdValue.BackColor = Color.White;
            _schoolIdValue.Font = new Font("Segoe UI", 8.75F);
            _schoolIdValue.ForeColor = Color.FromArgb(15, 23, 42);
            _schoolIdValue.Location = new Point(164, 280);
            _schoolIdValue.Name = "_schoolIdValue";
            _schoolIdValue.Size = new Size(232, 22);
            _schoolIdValue.TabIndex = 0;
            _schoolIdValue.Text = "—";
            _schoolIdValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // idNumberCaption
            // 
            idNumberCaption.AutoSize = true;
            idNumberCaption.BackColor = Color.White;
            idNumberCaption.Font = new Font("Segoe UI", 7.75F, FontStyle.Bold);
            idNumberCaption.ForeColor = Color.FromArgb(100, 116, 139);
            idNumberCaption.Location = new Point(26, 280);
            idNumberCaption.MinimumSize = new Size(0, 22);
            idNumberCaption.Name = "idNumberCaption";
            idNumberCaption.Size = new Size(80, 22);
            idNumberCaption.TabIndex = 1;
            idNumberCaption.Text = "ID NUMBER";
            idNumberCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _programValue
            // 
            _programValue.AutoEllipsis = true;
            _programValue.BackColor = Color.White;
            _programValue.Font = new Font("Segoe UI", 8.75F);
            _programValue.ForeColor = Color.FromArgb(15, 23, 42);
            _programValue.Location = new Point(164, 316);
            _programValue.Name = "_programValue";
            _programValue.Size = new Size(232, 22);
            _programValue.TabIndex = 2;
            _programValue.Text = "—";
            _programValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // programCaption
            // 
            programCaption.AutoSize = true;
            programCaption.BackColor = Color.White;
            programCaption.Font = new Font("Segoe UI", 7.75F, FontStyle.Bold);
            programCaption.ForeColor = Color.FromArgb(100, 116, 139);
            programCaption.Location = new Point(26, 316);
            programCaption.MinimumSize = new Size(0, 22);
            programCaption.Name = "programCaption";
            programCaption.Size = new Size(129, 22);
            programCaption.TabIndex = 3;
            programCaption.Text = "PROGRAM / OFFICE";
            programCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _yearValue
            // 
            _yearValue.AutoEllipsis = true;
            _yearValue.BackColor = Color.White;
            _yearValue.Font = new Font("Segoe UI", 8.75F);
            _yearValue.ForeColor = Color.FromArgb(15, 23, 42);
            _yearValue.Location = new Point(164, 352);
            _yearValue.Name = "_yearValue";
            _yearValue.Size = new Size(232, 22);
            _yearValue.TabIndex = 4;
            _yearValue.Text = "—";
            _yearValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // yearCaption
            // 
            yearCaption.AutoSize = true;
            yearCaption.BackColor = Color.White;
            yearCaption.Font = new Font("Segoe UI", 7.75F, FontStyle.Bold);
            yearCaption.ForeColor = Color.FromArgb(100, 116, 139);
            yearCaption.Location = new Point(26, 352);
            yearCaption.MinimumSize = new Size(0, 22);
            yearCaption.Name = "yearCaption";
            yearCaption.Size = new Size(40, 22);
            yearCaption.TabIndex = 5;
            yearCaption.Text = "YEAR";
            yearCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _instituteValue
            // 
            _instituteValue.AutoEllipsis = true;
            _instituteValue.BackColor = Color.White;
            _instituteValue.Font = new Font("Segoe UI", 8.75F);
            _instituteValue.ForeColor = Color.FromArgb(15, 23, 42);
            _instituteValue.Location = new Point(164, 388);
            _instituteValue.Name = "_instituteValue";
            _instituteValue.Size = new Size(232, 22);
            _instituteValue.TabIndex = 6;
            _instituteValue.Text = "—";
            _instituteValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // instituteCaption
            // 
            instituteCaption.AutoSize = true;
            instituteCaption.BackColor = Color.White;
            instituteCaption.Font = new Font("Segoe UI", 7.75F, FontStyle.Bold);
            instituteCaption.ForeColor = Color.FromArgb(100, 116, 139);
            instituteCaption.Location = new Point(26, 388);
            instituteCaption.MinimumSize = new Size(0, 22);
            instituteCaption.Name = "instituteCaption";
            instituteCaption.Size = new Size(73, 22);
            instituteCaption.TabIndex = 7;
            instituteCaption.Text = "INSTITUTE";
            instituteCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _contactValue
            // 
            _contactValue.AutoEllipsis = true;
            _contactValue.BackColor = Color.White;
            _contactValue.Font = new Font("Segoe UI", 8.75F);
            _contactValue.ForeColor = Color.FromArgb(15, 23, 42);
            _contactValue.Location = new Point(164, 424);
            _contactValue.Name = "_contactValue";
            _contactValue.Size = new Size(232, 22);
            _contactValue.TabIndex = 8;
            _contactValue.Text = "—";
            _contactValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // contactCaption
            // 
            contactCaption.AutoSize = true;
            contactCaption.BackColor = Color.White;
            contactCaption.Font = new Font("Segoe UI", 7.75F, FontStyle.Bold);
            contactCaption.ForeColor = Color.FromArgb(100, 116, 139);
            contactCaption.Location = new Point(26, 424);
            contactCaption.MinimumSize = new Size(0, 22);
            contactCaption.Name = "contactCaption";
            contactCaption.Size = new Size(68, 22);
            contactCaption.TabIndex = 9;
            contactCaption.Text = "CONTACT";
            contactCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _addressValue
            // 
            _addressValue.AutoEllipsis = true;
            _addressValue.BackColor = Color.White;
            _addressValue.Font = new Font("Segoe UI", 8.75F);
            _addressValue.ForeColor = Color.FromArgb(15, 23, 42);
            _addressValue.Location = new Point(164, 460);
            _addressValue.Name = "_addressValue";
            _addressValue.Size = new Size(232, 22);
            _addressValue.TabIndex = 10;
            _addressValue.Text = "—";
            _addressValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // addressCaption
            // 
            addressCaption.AutoSize = true;
            addressCaption.BackColor = Color.White;
            addressCaption.Font = new Font("Segoe UI", 7.75F, FontStyle.Bold);
            addressCaption.ForeColor = Color.FromArgb(100, 116, 139);
            addressCaption.Location = new Point(26, 460);
            addressCaption.MinimumSize = new Size(0, 22);
            addressCaption.Name = "addressCaption";
            addressCaption.Size = new Size(66, 22);
            addressCaption.TabIndex = 11;
            addressCaption.Text = "ADDRESS";
            addressCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // identityDivider
            // 
            identityDivider.BackColor = Color.FromArgb(226, 232, 240);
            identityDivider.Location = new Point(26, 266);
            identityDivider.Name = "identityDivider";
            identityDivider.Size = new Size(370, 1);
            identityDivider.TabIndex = 12;
            // 
            // _profileRole
            // 
            _profileRole.BackColor = Color.White;
            _profileRole.BorderColor = Color.FromArgb(167, 243, 208);
            _profileRole.FillColor = Color.FromArgb(220, 252, 231);
            _profileRole.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            _profileRole.Location = new Point(0, 226);
            _profileRole.Name = "_profileRole";
            _profileRole.Size = new Size(113, 34);
            _profileRole.TabIndex = 13;
            _profileRole.TabStop = false;
            _profileRole.Text = "STUDENT";
            _profileRole.TextColor = Color.FromArgb(4, 120, 87);
            // 
            // _profileMeta
            // 
            _profileMeta.AutoEllipsis = true;
            _profileMeta.BackColor = Color.White;
            _profileMeta.Font = new Font("Segoe UI", 8.5F);
            _profileMeta.ForeColor = Color.FromArgb(71, 85, 105);
            _profileMeta.Location = new Point(26, 198);
            _profileMeta.Name = "_profileMeta";
            _profileMeta.Size = new Size(370, 22);
            _profileMeta.TabIndex = 14;
            _profileMeta.Text = "STUDENT";
            _profileMeta.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // _profileName
            // 
            _profileName.AutoEllipsis = true;
            _profileName.BackColor = Color.White;
            _profileName.Font = new Font("Segoe UI", 14.5F, FontStyle.Bold);
            _profileName.ForeColor = Color.FromArgb(6, 78, 59);
            _profileName.Location = new Point(26, 154);
            _profileName.Name = "_profileName";
            _profileName.Size = new Size(370, 44);
            _profileName.TabIndex = 15;
            _profileName.Text = "PATRON";
            _profileName.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // _profileAvatar
            // 
            _profileAvatar.BackColor = Color.White;
            _profileAvatar.FillBottomColor = Color.FromArgb(4, 90, 66);
            _profileAvatar.FillTopColor = Color.FromArgb(34, 197, 94);
            _profileAvatar.Initials = "PA";
            _profileAvatar.InitialsColor = Color.White;
            _profileAvatar.Location = new Point(163, 48);
            _profileAvatar.Name = "_profileAvatar";
            _profileAvatar.RingColor = Color.FromArgb(70, 255, 255, 255);
            _profileAvatar.Size = new Size(96, 96);
            _profileAvatar.TabIndex = 16;
            _profileAvatar.TabStop = false;
            // 
            // identityHeading
            // 
            identityHeading.AutoSize = true;
            identityHeading.BackColor = Color.White;
            identityHeading.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            identityHeading.ForeColor = Color.FromArgb(4, 120, 87);
            identityHeading.Location = new Point(26, 20);
            identityHeading.Name = "identityHeading";
            identityHeading.Size = new Size(95, 20);
            identityHeading.TabIndex = 17;
            identityHeading.Text = "MY PROFILE";
            // 
            // profileQrCard
            // 
            profileQrCard.Controls.Add(qrKeepPrivate);
            profileQrCard.Controls.Add(qrStepThree);
            profileQrCard.Controls.Add(qrStepTwo);
            profileQrCard.Controls.Add(qrStepOne);
            profileQrCard.Controls.Add(qrHowTo);
            profileQrCard.Controls.Add(qrDivider);
            profileQrCard.Controls.Add(_qrCaption);
            profileQrCard.Controls.Add(_qrPicture);
            profileQrCard.Controls.Add(qrHeading);
            profileQrCard.Dock = DockStyle.Fill;
            profileQrCard.Location = new Point(443, 3);
            profileQrCard.Name = "profileQrCard";
            profileQrCard.Size = new Size(756, 511);
            profileQrCard.TabIndex = 1;
            // 
            // qrKeepPrivate
            // 
            qrKeepPrivate.AutoEllipsis = true;
            qrKeepPrivate.BackColor = Color.White;
            qrKeepPrivate.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            qrKeepPrivate.ForeColor = Color.FromArgb(100, 116, 139);
            qrKeepPrivate.Location = new Point(26, 494);
            qrKeepPrivate.Name = "qrKeepPrivate";
            qrKeepPrivate.Size = new Size(607, 40);
            qrKeepPrivate.TabIndex = 0;
            qrKeepPrivate.Text = "KEEP THIS PASS PRIVATE. IT POINTS TO YOUR LIBRARY ATTENDANCE RECORD.";
            // 
            // qrStepThree
            // 
            qrStepThree.AutoEllipsis = true;
            qrStepThree.BackColor = Color.White;
            qrStepThree.Font = new Font("Segoe UI", 8.5F);
            qrStepThree.ForeColor = Color.FromArgb(71, 85, 105);
            qrStepThree.Location = new Point(26, 456);
            qrStepThree.Name = "qrStepThree";
            qrStepThree.Size = new Size(120, 20);
            qrStepThree.TabIndex = 1;
            qrStepThree.Text = "3.  YOUR ATTENDANCE AUTOMATICALLY RECORDED IN THE LMS.";
            // 
            // qrStepTwo
            // 
            qrStepTwo.AutoEllipsis = true;
            qrStepTwo.BackColor = Color.White;
            qrStepTwo.Font = new Font("Segoe UI", 8.5F);
            qrStepTwo.ForeColor = Color.FromArgb(71, 85, 105);
            qrStepTwo.Location = new Point(26, 430);
            qrStepTwo.Name = "qrStepTwo";
            qrStepTwo.Size = new Size(120, 20);
            qrStepTwo.TabIndex = 2;
            qrStepTwo.Text = "2.  SCAN YOUR QR CODE IN LIBRARY.";
            // 
            // qrStepOne
            // 
            qrStepOne.AutoEllipsis = true;
            qrStepOne.BackColor = Color.White;
            qrStepOne.Font = new Font("Segoe UI", 8.5F);
            qrStepOne.ForeColor = Color.FromArgb(71, 85, 105);
            qrStepOne.Location = new Point(26, 404);
            qrStepOne.Name = "qrStepOne";
            qrStepOne.Size = new Size(120, 20);
            qrStepOne.TabIndex = 3;
            qrStepOne.Text = "1.  ACCESS YOUR QR CODE IN THE LMS.";
            // 
            // qrHowTo
            // 
            qrHowTo.AutoSize = true;
            qrHowTo.BackColor = Color.White;
            qrHowTo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            qrHowTo.ForeColor = Color.FromArgb(15, 23, 42);
            qrHowTo.Location = new Point(26, 60);
            qrHowTo.Name = "qrHowTo";
            qrHowTo.Size = new Size(287, 20);
            qrHowTo.TabIndex = 4;
            qrHowTo.Text = "HOW TO USE YOUR ATTENDANCE PASS";
            // 
            // qrDivider
            // 
            qrDivider.BackColor = Color.FromArgb(226, 232, 240);
            qrDivider.Location = new Point(26, 384);
            qrDivider.Name = "qrDivider";
            qrDivider.Size = new Size(120, 1);
            qrDivider.TabIndex = 5;
            // 
            // _qrCaption
            // 
            _qrCaption.BackColor = Color.White;
            _qrCaption.Font = new Font("Segoe UI", 8.75F);
            _qrCaption.ForeColor = Color.FromArgb(100, 116, 139);
            _qrCaption.Location = new Point(26, 330);
            _qrCaption.Name = "_qrCaption";
            _qrCaption.Size = new Size(120, 42);
            _qrCaption.TabIndex = 6;
            _qrCaption.Text = "SHOW THIS QR PASS AT THE LIBRARY OFFICE ENTRANCE. THE LIBRARIAN SCANS IT TO RECORD YOUR ATTENDANCE.";
            _qrCaption.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // _qrPicture
            // 
            _qrPicture.BackColor = Color.White;
            _qrPicture.BorderStyle = BorderStyle.FixedSingle;
            _qrPicture.Location = new Point(26, 60);
            _qrPicture.Name = "_qrPicture";
            _qrPicture.Size = new Size(260, 260);
            _qrPicture.SizeMode = PictureBoxSizeMode.Zoom;
            _qrPicture.TabIndex = 7;
            _qrPicture.TabStop = false;
            // 
            // qrHeading
            // 
            qrHeading.AutoSize = true;
            qrHeading.BackColor = Color.White;
            qrHeading.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            qrHeading.ForeColor = Color.FromArgb(4, 120, 87);
            qrHeading.Location = new Point(26, 20);
            qrHeading.Name = "qrHeading";
            qrHeading.Size = new Size(248, 20);
            qrHeading.TabIndex = 8;
            qrHeading.Text = "STUDENT ATTENDANCE QR CODE";
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
            attendanceRefresh.FlatAppearance.BorderSize = 0;
            attendanceRefresh.FlatStyle = FlatStyle.Flat;
            attendanceRefresh.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            attendanceRefresh.ForeColor = Color.White;
            attendanceRefresh.HoverColor = Color.FromArgb(21, 128, 61);
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
            _attendanceEmpty.Caption = "SHOW YOUR QR PASS TO THE LIBRARIAN AT THE LIBRARY OFFICE ENTRANCE.";
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
            catalogRefresh.FlatAppearance.BorderSize = 0;
            catalogRefresh.FlatStyle = FlatStyle.Flat;
            catalogRefresh.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            catalogRefresh.ForeColor = Color.White;
            catalogRefresh.HoverColor = Color.FromArgb(21, 128, 61);
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
            catalogStatRow.Controls.Add(_catalogStatOnLoan);
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
            // _catalogStatOnLoan
            // 
            _catalogStatOnLoan.AccentColor = Color.FromArgb(37, 99, 235);
            _catalogStatOnLoan.BackColor = Color.FromArgb(241, 245, 249);
            _catalogStatOnLoan.Caption = "YOUR BOOKS ON LOAN";
            _catalogStatOnLoan.Location = new Point(534, 4);
            _catalogStatOnLoan.Margin = new Padding(0, 0, 14, 0);
            _catalogStatOnLoan.Name = "_catalogStatOnLoan";
            _catalogStatOnLoan.Size = new Size(253, 78);
            _catalogStatOnLoan.TabIndex = 2;
            _catalogStatOnLoan.TabStop = false;
            _catalogStatOnLoan.Value = "0";
            _catalogStatOnLoan.ValueColor = Color.FromArgb(15, 23, 42);
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
            requestsRefresh.FlatAppearance.BorderSize = 0;
            requestsRefresh.FlatStyle = FlatStyle.Flat;
            requestsRefresh.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            requestsRefresh.ForeColor = Color.White;
            requestsRefresh.HoverColor = Color.FromArgb(21, 128, 61);
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
            requestsStatRow.Controls.Add(_requestStatPending);
            requestsStatRow.Controls.Add(_requestStatApproved);
            requestsStatRow.Controls.Add(_requestStatClosed);
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
            // _requestStatPending
            // 
            _requestStatPending.AccentColor = Color.FromArgb(217, 119, 6);
            _requestStatPending.BackColor = Color.FromArgb(241, 245, 249);
            _requestStatPending.Caption = "STILL PENDING";
            _requestStatPending.Location = new Point(124, 4);
            _requestStatPending.Margin = new Padding(0, 0, 14, 0);
            _requestStatPending.Name = "_requestStatPending";
            _requestStatPending.Size = new Size(110, 78);
            _requestStatPending.TabIndex = 1;
            _requestStatPending.TabStop = false;
            _requestStatPending.Value = "0";
            _requestStatPending.ValueColor = Color.FromArgb(15, 23, 42);
            // 
            // _requestStatApproved
            // 
            _requestStatApproved.AccentColor = Color.FromArgb(4, 120, 87);
            _requestStatApproved.BackColor = Color.FromArgb(241, 245, 249);
            _requestStatApproved.Caption = "APPROVED";
            _requestStatApproved.Location = new Point(248, 4);
            _requestStatApproved.Margin = new Padding(0, 0, 14, 0);
            _requestStatApproved.Name = "_requestStatApproved";
            _requestStatApproved.Size = new Size(110, 78);
            _requestStatApproved.TabIndex = 2;
            _requestStatApproved.TabStop = false;
            _requestStatApproved.Value = "0";
            _requestStatApproved.ValueColor = Color.FromArgb(15, 23, 42);
            // 
            // _requestStatClosed
            // 
            _requestStatClosed.AccentColor = Color.FromArgb(220, 38, 38);
            _requestStatClosed.BackColor = Color.FromArgb(241, 245, 249);
            _requestStatClosed.Caption = "REJECTED / CLOSED";
            _requestStatClosed.Location = new Point(372, 4);
            _requestStatClosed.Margin = new Padding(0);
            _requestStatClosed.Name = "_requestStatClosed";
            _requestStatClosed.Size = new Size(110, 78);
            _requestStatClosed.TabIndex = 3;
            _requestStatClosed.TabStop = false;
            _requestStatClosed.Value = "0";
            _requestStatClosed.ValueColor = Color.FromArgb(15, 23, 42);
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
            // tabPageLoans
            // 
            tabPageLoans.BackColor = Color.FromArgb(241, 245, 249);
            tabPageLoans.Controls.Add(_loansPage);
            tabPageLoans.Location = new Point(4, 29);
            tabPageLoans.Name = "tabPageLoans";
            tabPageLoans.Padding = new Padding(3);
            tabPageLoans.Size = new Size(1266, 661);
            tabPageLoans.TabIndex = 4;
            tabPageLoans.Text = "MY BORROWED BOOKS";
            // 
            // _loansPage
            // 
            _loansPage.Controls.Add(loansGrid);
            _loansPage.Dock = DockStyle.Fill;
            _loansPage.Location = new Point(3, 3);
            _loansPage.Name = "_loansPage";
            _loansPage.Padding = new Padding(26, 18, 26, 22);
            _loansPage.Size = new Size(1260, 655);
            _loansPage.TabIndex = 2;
            // 
            // loansGrid
            // 
            loansGrid.ColumnCount = 1;
            loansGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            loansGrid.Controls.Add(loansHead, 0, 0);
            loansGrid.Controls.Add(loansStatRow, 0, 1);
            loansGrid.Controls.Add(loansListHost, 0, 3);
            loansGrid.Dock = DockStyle.Fill;
            loansGrid.Location = new Point(26, 18);
            loansGrid.Name = "loansGrid";
            loansGrid.RowCount = 4;
            loansGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 92F));
            loansGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));
            loansGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            loansGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            loansGrid.Size = new Size(1208, 615);
            loansGrid.TabIndex = 0;
            // 
            // loansHead
            // 
            loansHead.Controls.Add(_loansSearch);
            loansHead.Controls.Add(loansTitle);
            loansHead.Controls.Add(loansRefresh);
            loansHead.Dock = DockStyle.Fill;
            loansHead.Location = new Point(3, 3);
            loansHead.Name = "loansHead";
            loansHead.Size = new Size(1202, 86);
            loansHead.TabIndex = 0;
            // 
            // _loansSearch
            // 
            _loansSearch.BackColor = Color.White;
            _loansSearch.Location = new Point(825, 10);
            _loansSearch.Name = "_loansSearch";
            _loansSearch.PlaceholderText = "SEARCH LOANS...";
            _loansSearch.SearchText = "";
            _loansSearch.Size = new Size(360, 44);
            _loansSearch.TabIndex = 0;
            // 
            // loansTitle
            // 
            loansTitle.AutoSize = true;
            loansTitle.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            loansTitle.ForeColor = Color.FromArgb(15, 23, 42);
            loansTitle.Location = new Point(0, 0);
            loansTitle.Name = "loansTitle";
            loansTitle.Size = new Size(517, 60);
            loansTitle.TabIndex = 2;
            loansTitle.Text = "MY BORROWED BOOKS";
            // 
            // loansRefresh
            // 
            loansRefresh.BackColor = Color.Transparent;
            loansRefresh.CornerRadius = 11;
            loansRefresh.FlatAppearance.BorderSize = 0;
            loansRefresh.FlatStyle = FlatStyle.Flat;
            loansRefresh.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            loansRefresh.ForeColor = Color.White;
            loansRefresh.HoverColor = Color.FromArgb(21, 128, 61);
            loansRefresh.Location = new Point(28, 10);
            loansRefresh.Name = "loansRefresh";
            loansRefresh.NormalColor = Color.FromArgb(22, 163, 74);
            loansRefresh.PressedColor = Color.FromArgb(20, 83, 45);
            loansRefresh.Size = new Size(114, 44);
            loansRefresh.TabIndex = 3;
            loansRefresh.Text = "REFRESH";
            loansRefresh.UseVisualStyleBackColor = false;
            // 
            // loansStatRow
            // 
            loansStatRow.Controls.Add(_loanStatActive);
            loansStatRow.Controls.Add(_loanStatReturned);
            loansStatRow.Controls.Add(_loanStatTotal);
            loansStatRow.Controls.Add(_loanStatLongest);
            loansStatRow.Dock = DockStyle.Fill;
            loansStatRow.Location = new Point(3, 95);
            loansStatRow.Name = "loansStatRow";
            loansStatRow.Padding = new Padding(0, 4, 0, 4);
            loansStatRow.Size = new Size(1202, 84);
            loansStatRow.TabIndex = 1;
            loansStatRow.WrapContents = false;
            // 
            // _loanStatActive
            // 
            _loanStatActive.AccentColor = Color.FromArgb(22, 163, 74);
            _loanStatActive.BackColor = Color.FromArgb(241, 245, 249);
            _loanStatActive.Caption = "ON BORROWED RIGHT NOW";
            _loanStatActive.Location = new Point(0, 4);
            _loanStatActive.Margin = new Padding(0, 0, 14, 0);
            _loanStatActive.Name = "_loanStatActive";
            _loanStatActive.Size = new Size(110, 78);
            _loanStatActive.TabIndex = 0;
            _loanStatActive.TabStop = false;
            _loanStatActive.Value = "0";
            _loanStatActive.ValueColor = Color.FromArgb(15, 23, 42);
            // 
            // _loanStatReturned
            // 
            _loanStatReturned.AccentColor = Color.FromArgb(100, 116, 139);
            _loanStatReturned.BackColor = Color.FromArgb(241, 245, 249);
            _loanStatReturned.Caption = "ALREADY RETURNED";
            _loanStatReturned.Location = new Point(124, 4);
            _loanStatReturned.Margin = new Padding(0, 0, 14, 0);
            _loanStatReturned.Name = "_loanStatReturned";
            _loanStatReturned.Size = new Size(110, 78);
            _loanStatReturned.TabIndex = 1;
            _loanStatReturned.TabStop = false;
            _loanStatReturned.Value = "0";
            _loanStatReturned.ValueColor = Color.FromArgb(15, 23, 42);
            // 
            // _loanStatTotal
            // 
            _loanStatTotal.AccentColor = Color.FromArgb(37, 99, 235);
            _loanStatTotal.BackColor = Color.FromArgb(241, 245, 249);
            _loanStatTotal.Caption = "TOTAL LOANS ON FILE";
            _loanStatTotal.Location = new Point(248, 4);
            _loanStatTotal.Margin = new Padding(0, 0, 14, 0);
            _loanStatTotal.Name = "_loanStatTotal";
            _loanStatTotal.Size = new Size(110, 78);
            _loanStatTotal.TabIndex = 2;
            _loanStatTotal.TabStop = false;
            _loanStatTotal.Value = "0";
            _loanStatTotal.ValueColor = Color.FromArgb(15, 23, 42);
            // 
            // _loanStatLongest
            // 
            _loanStatLongest.AccentColor = Color.FromArgb(217, 119, 6);
            _loanStatLongest.BackColor = Color.FromArgb(241, 245, 249);
            _loanStatLongest.Caption = "LONGEST OPEN LOAN";
            _loanStatLongest.Location = new Point(372, 4);
            _loanStatLongest.Margin = new Padding(0);
            _loanStatLongest.Name = "_loanStatLongest";
            _loanStatLongest.Size = new Size(110, 78);
            _loanStatLongest.TabIndex = 3;
            _loanStatLongest.TabStop = false;
            _loanStatLongest.Value = "0";
            _loanStatLongest.ValueColor = Color.FromArgb(15, 23, 42);
            // 
            // loansListHost
            // 
            loansListHost.Controls.Add(_loanFlow);
            loansListHost.Controls.Add(_loansEmpty);
            loansListHost.Dock = DockStyle.Fill;
            loansListHost.Location = new Point(3, 213);
            loansListHost.Name = "loansListHost";
            loansListHost.Size = new Size(1202, 399);
            loansListHost.TabIndex = 3;
            // 
            // _loanFlow
            // 
            _loanFlow.AutoScroll = true;
            _loanFlow.Dock = DockStyle.Fill;
            _loanFlow.Location = new Point(0, 0);
            _loanFlow.Name = "_loanFlow";
            _loanFlow.Padding = new Padding(0, 0, 4, 4);
            _loanFlow.Size = new Size(1202, 399);
            _loanFlow.TabIndex = 0;
            // 
            // _loansEmpty
            // 
            _loansEmpty.BackColor = Color.Transparent;
            _loansEmpty.BorderColor = Color.FromArgb(226, 232, 240);
            _loansEmpty.Caption = "WHEN THE LIBRARIAN RELEASES AN APPROVED REQUEST, THE LOAN APPEARS HERE.";
            _loansEmpty.CaptionColor = Color.FromArgb(148, 163, 184);
            _loansEmpty.CornerRadius = 20;
            _loansEmpty.Dock = DockStyle.Fill;
            _loansEmpty.FillColor = Color.FromArgb(248, 250, 252);
            _loansEmpty.Location = new Point(0, 0);
            _loansEmpty.Name = "_loansEmpty";
            _loansEmpty.Size = new Size(1202, 399);
            _loansEmpty.TabIndex = 1;
            _loansEmpty.TabStop = false;
            _loansEmpty.Title = "NO BORROW RECORDS YET";
            _loansEmpty.TitleColor = Color.FromArgb(100, 116, 139);
            _loansEmpty.Visible = false;
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
            profileQrCard.ResumeLayout(false);
            profileQrCard.PerformLayout();
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
            tabPageLoans.ResumeLayout(false);
            _loansPage.ResumeLayout(false);
            loansGrid.ResumeLayout(false);
            loansHead.ResumeLayout(false);
            loansHead.PerformLayout();
            loansStatRow.ResumeLayout(false);
            loansListHost.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel rootGrid;
        private PatronHeaderPanel headerPanel;
        private RoundedButton _logout;
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
        private StatCard _catalogStatOnLoan;
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
        private StatCard _requestStatPending;
        private StatCard _requestStatApproved;
        private StatCard _requestStatClosed;
        private Panel requestsListHost;
        private FlowLayoutPanel _requestFlow;
        private EmptyState _requestsEmpty;
        private Panel _loansPage;
        private TableLayoutPanel loansGrid;
        private Panel loansHead;
        private SearchBox _loansSearch;
        private Label loansTitle;
        private RoundedButton loansRefresh;
        private FlowLayoutPanel loansStatRow;
        private StatCard _loanStatActive;
        private StatCard _loanStatReturned;
        private StatCard _loanStatTotal;
        private StatCard _loanStatLongest;
        private Panel loansListHost;
        private FlowLayoutPanel _loanFlow;
        private EmptyState _loansEmpty;
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
        private RoundedButton profileRefresh;
        private TableLayoutPanel profileColumns;
        private Panel profileIdentityCard;
        private Label _schoolIdValue;
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
        private PillBadge _profileRole;
        private Label _profileMeta;
        private Label _profileName;
        private AvatarBadge _profileAvatar;
        private Label identityHeading;
        private Panel profileQrCard;
        private Label qrKeepPrivate;
        private Label qrStepThree;
        private Label qrStepTwo;
        private Label qrStepOne;
        private Label qrHowTo;
        private Panel qrDivider;
        private Label _qrCaption;
        private PictureBox _qrPicture;
        private Label qrHeading;
        private TabPage tabPageProfile;
        private TabPage tabPageAttendance;
        private TabPage tabPageCatalog;
        private TabPage tabPageRequests;
        private TabPage tabPageLoans;
    }
}
