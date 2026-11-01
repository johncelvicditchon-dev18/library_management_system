using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace oop_project
{
    /// <summary>
    /// A small rounded search field used above every dashboard table.
    ///
    /// The control only collects the text. The form that owns it decides which
    /// MySQL query to run, so the search rules stay next to the table they filter.
    /// Press Enter (or click the magnifier) to raise <see cref="SearchRequested"/>.
    /// </summary>
    [ToolboxItem(true)]
    [Description("Rounded search field with a magnifier, a clear button and an Enter shortcut.")]
    public sealed class SearchBox : Panel
    {
        private readonly TextBox _input;
        private readonly Label _magnifier;
        private readonly Label _clearButton;

        /// <summary>Raised when the user presses Enter, clicks the magnifier or clears the box.</summary>
        public event EventHandler? SearchRequested;

        /// <summary>Raised after every typed character so a card list can filter live.</summary>
        public event EventHandler? SearchTextChanged;

        public SearchBox()
        {
            DoubleBuffered = true;
            BackColor = Color.White;
            Height = 44;
            Width = 420;
            Padding = new Padding(0);

            _magnifier = new Label();
            _magnifier.BackColor = Color.Transparent;
            _magnifier.Cursor = Cursors.Hand;
            _magnifier.Font = new Font("Segoe MDL2 Assets", 12F);
            _magnifier.ForeColor = Color.FromArgb(100, 116, 139);
            _magnifier.Text = "\uE721";
            _magnifier.TextAlign = ContentAlignment.MiddleCenter;
            _magnifier.Click += delegate { RaiseSearch(); };
            ToolTip tip = new ToolTip();
            tip.SetToolTip(_magnifier, "Search this table using MySQL LIKE.");

            _input = new TextBox();
            _input.BackColor = Color.White;
            _input.BorderStyle = BorderStyle.None;
            _input.Font = new Font("Segoe UI", 9.5F);
            _input.ForeColor = Color.FromArgb(15, 23, 42);
            _input.PlaceholderText = "SEARCH";
            _input.KeyDown += Input_KeyDown;

            _clearButton = new Label();
            _clearButton.BackColor = Color.Transparent;
            _clearButton.Cursor = Cursors.Hand;
            _clearButton.Font = new Font("Segoe MDL2 Assets", 9F);
            _clearButton.ForeColor = Color.FromArgb(148, 163, 184);
            _clearButton.Text = "\uE711";
            _clearButton.TextAlign = ContentAlignment.MiddleCenter;
            _clearButton.Visible = false;
            _clearButton.Click += delegate
            {
                _input.Clear();
                RaiseSearch();
            };
            tip.SetToolTip(_clearButton, "Clear the search and show every row again.");

            _input.TextChanged += delegate
            {
                _clearButton.Visible = _input.Text.Length > 0;
                SearchTextChanged?.Invoke(this, EventArgs.Empty);
            };

            Controls.Add(_input);
            Controls.Add(_clearButton);
            Controls.Add(_magnifier);

            Resize += delegate { LayoutChildren(); };
            LayoutChildren();
        }

        /// <summary>Grey hint shown before the user types.</summary>
        [Category("Appearance")]
        [Description("Hint text shown while the search box is empty.")]
        public string PlaceholderText
        {
            get { return _input.PlaceholderText; }
            set { _input.PlaceholderText = value ?? string.Empty; }
        }

        /// <summary>The trimmed text the form should send to the database.</summary>
        [Browsable(false)]
        public string SearchText
        {
            get { return _input.Text.Trim(); }
            set
            {
                _input.Text = (value ?? string.Empty).Trim();
                _input.SelectionStart = _input.TextLength;
            }
        }

        /// <summary>Puts the caret in the field without changing the current search.</summary>
        public void FocusInput()
        {
            _input.Focus();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (Width < 4 || Height < 4)
            {
                return;
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle bounds = new Rectangle(1, 1, Width - 3, Height - 3);
            using GraphicsPath path = DrawHelper.CreateRoundedPath(bounds, 14);
            using Pen border = new Pen(Color.FromArgb(203, 213, 225), 1.4F);
            e.Graphics.DrawPath(border, path);
        }

        private void LayoutChildren()
        {
            const int horizontalPadding = 12;
            int centerTop = Math.Max(6, (Height - 22) / 2);

            _magnifier.Left = horizontalPadding;
            _magnifier.Top = centerTop;
            _magnifier.Size = new Size(24, 22);

            int inputLeft = _magnifier.Right + 2;
            int inputRight = Width - 38;
            _input.Left = inputLeft;
            _input.Top = centerTop + 1;
            _input.Width = Math.Max(30, inputRight - inputLeft);

            _clearButton.Left = Width - 32;
            _clearButton.Top = centerTop;
            _clearButton.Size = new Size(22, 22);
        }

        private void Input_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                RaiseSearch();
            }
            else if (e.KeyCode == Keys.Escape && _input.Text.Length > 0)
            {
                _input.Clear();
                RaiseSearch();
            }
        }

        private void RaiseSearch()
        {
            SearchRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
