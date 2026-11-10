using library_management_system.forms;
using oop_project.forms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace oop_project
{
    public partial class Welcome : Form
    {
        // Design sizes are expressed at 96 DPI and scaled again at runtime for high-DPI displays.
        private const int CardWidth = 780;
        private const int CardHeight = 540;
        private const int ContentMargin = 52;
        private const int LogoSize = 106;
        private const int LogoGap = 24;

        private readonly System.Windows.Forms.Timer _fadeTimer = new System.Windows.Forms.Timer();
        private bool _fadeStarted;

        public Welcome()
        {
            InitializeComponent();
            DoubleBuffered = true;

            // FULL SCREEN: the welcome page opens maximised and F11 toggles a
            // borderless view that covers the whole monitor.
            ScreenMode.Apply(this);

            // The window fades in once, so the page feels intentional instead of popping.
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                Opacity = 0d;
            }

            ApplyLayout();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ApplyLayout();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            FitToWorkingArea();
            ApplyLayout();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            FitToWorkingArea();
            ApplyLayout();
            StartFadeIn();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _fadeTimer.Dispose();
            base.OnFormClosed(e);
        }

        /// <summary>
        /// Opens as a large window (about 90% of the desktop) so the title bar and the
        /// close/minimise buttons stay available, instead of hiding the whole screen.
        /// While the window is maximised (or in F11 full screen) the window manager
        /// already owns the size, so this method leaves it alone.
        /// </summary>
        private void FitToWorkingArea()
        {
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }

            if (WindowState != FormWindowState.Normal || FormBorderStyle == FormBorderStyle.None)
            {
                return;
            }

            Screen? screen = Screen.FromControl(this) ?? Screen.PrimaryScreen;
            if (screen is null)
            {
                return;
            }

            Rectangle area = screen.WorkingArea;

            int width = Math.Max(S(CardWidth) + S(240), (int)Math.Round(area.Width * 0.90));
            int height = Math.Max(S(CardHeight) + S(160), (int)Math.Round(area.Height * 0.88));

            width = Math.Min(width, area.Width - S(32));
            height = Math.Min(height, area.Height - S(32));

            var clientSize = new Size(width, height);
            if (ClientSize != clientSize)
            {
                ClientSize = clientSize;
            }

            Location = new Point(
                area.Left + Math.Max(0, (area.Width - Width) / 2),
                area.Top + Math.Max(0, (area.Height - Height) / 2));
        }

        private int S(int value)
        {
            return (int)Math.Round(value * DeviceDpi / 96d);
        }

        /// <summary>
        /// Keeps the hero card perfectly centred, re-centres everything inside it and pins the
        /// credit line to the bottom, so the page still looks right on smaller or scaled displays.
        /// </summary>
        private void ApplyLayout()
        {
            if (card == null || backdrop == null || creditLabel == null)
            {
                return;
            }

            Size area = backdrop.ClientSize;
            if (area.Width <= 0 || area.Height <= 0)
            {
                return;
            }

            int cardWidth = S(CardWidth);
            int cardHeight = S(CardHeight);

            card.CornerRadius = S(32);
            card.Size = new Size(cardWidth, cardHeight);
            card.Location = new Point(
                Math.Max(0, (area.Width - cardWidth) / 2),
                Math.Max(0, (area.Height - cardHeight) / 2));

            backdrop.CardBounds = card.Bounds;

            creditLabel.Bounds = new Rectangle(
                0,
                Math.Max(0, area.Height - S(48)),
                area.Width,
                S(20));

            LayoutCardContents();
        }

        /// <summary>Centres every element of the card on a shared vertical rhythm.</summary>
        private void LayoutCardContents()
        {
            Size area = card.ClientSize;
            int center = area.Width / 2;
            int margin = S(ContentMargin);
            int contentWidth = Math.Max(S(240), area.Width - margin * 2);

            // Brand chip.
            eyebrowChip.HorizontalPadding = S(20);
            eyebrowChip.VerticalPadding = S(8);
            eyebrowChip.Location = new Point(
                Math.Max(0, (area.Width - eyebrowChip.Width) / 2),
                S(44));

            // Institution logos with a hairline divider between them.
            int logoSize = S(LogoSize);
            int gap = S(LogoGap);

            tcgcLogoBadge.Size = new Size(logoSize, logoSize);
            lrcLogoBadge.Size = new Size(logoSize, logoSize);
            tcgcLogoBadge.Location = new Point(center - gap - logoSize, S(90));
            lrcLogoBadge.Location = new Point(center + gap, S(90));

            int dividerHeight = S(48);
            logoDivider.Bounds = new Rectangle(
                center - 1,
                S(90) + (logoSize - dividerHeight) / 2,
                1,
                dividerHeight);

            tcgcCaption.Bounds = new Rectangle(tcgcLogoBadge.Left, S(202), logoSize, S(16));
            lrcCaption.Bounds = new Rectangle(lrcLogoBadge.Left, S(202), logoSize, S(16));

            // Accent rule, headline and supporting copy.
            ///  accentDivider.Bounds = new Rectangle(center - S(28), S(238), S(56), Math.Max(2, S(4)));
            headingLabel.Bounds = new Rectangle(margin, S(256), contentWidth, S(44));
            subtitleLabel.Bounds = new Rectangle(margin, S(310), contentWidth, S(24));

            start_button.CornerRadius = S(15);
            start_button.Bounds = new Rectangle(center - S(160), S(366), S(320), S(56));

            // QR CODE SCANNER and LIBRARY EVALUATION share one row under the
            // PROCEED TO SIGN IN button, and stay centred at any window size.
            int buttonWidth = S(260);
            int buttonHeight = S(35);
            int buttonGap = S(14);
            int pairWidth = (buttonWidth * 2) + buttonGap;
            int buttonTop = S(366) + S(56) + S(10);
            int buttonLeft = center - (pairWidth / 2);

            qr_scanner_button.CornerRadius = S(1);
            evaluation_button.CornerRadius = S(1);
            qr_scanner_button.Bounds = new Rectangle(buttonLeft, buttonTop, buttonWidth, buttonHeight);
            evaluation_button.Bounds = new Rectangle(buttonLeft + buttonWidth + buttonGap, buttonTop, buttonWidth, buttonHeight);

            footerDivider.Bounds = new Rectangle(center - S(170), S(470), S(340), 1);
            footerLabel.Bounds = new Rectangle(margin, S(482), contentWidth, S(18));
        }

        private void StartFadeIn()
        {
            if (_fadeStarted)
            {
                return;
            }

            _fadeStarted = true;
            _fadeTimer.Interval = 15;
            _fadeTimer.Tick += (sender, args) =>
            {
                Opacity = Math.Min(1d, Opacity + 0.08d);
                if (Opacity >= 1d)
                {
                    _fadeTimer.Stop();
                }
            };
            _fadeTimer.Start();
        }

        private void start_button_Click(object sender, EventArgs e)
        {
            Login loginForm = new Login();

            // Closing the sign-in form ends the session instead of leaving this page hidden.
            loginForm.FormClosed += (s, args) => Close();

            loginForm.Show();
            this.Hide();
        }

        private void roundedButton1_Click(object sender, EventArgs e)
        {
            // The scanner stays on top of this page, so closing it returns to the
            // welcome screen instead of leaving the application with no window.
            using (qr_code_scanner qr_form = new qr_code_scanner())
            {
                qr_form.ShowDialog(this);
            }
        }

        private void evaluation_button_Click(object sender, EventArgs e)
        {
            // Students, faculty and staff sign in with their library account first.
            // Only an active account may continue to the evaluation form.
            using (EvaluationLogin signIn = new EvaluationLogin())
            {
                if (signIn.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                using (EvaluationForm form = new EvaluationForm(
                    signIn.RespondentRole, signIn.RespondentIdNumber,
                    signIn.RespondentName, signIn.AccountId,
                    signIn.EvaluationPeriodId, signIn.EvaluationPeriodTitle))
                {
                    form.ShowDialog(this);
                }
            }
        }

        private void card_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
