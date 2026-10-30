using System;
using System.Drawing;
using System.Windows.Forms;

namespace oop_project
{
    public enum DialogFieldKind
    {
        Text,
        ReadOnly,
        Choice,
        Date
    }

    /// <summary>
    /// Caption + rounded input + inline warning, so every dashboard pop up form
    /// uses exactly the same field style as the LOGIN and SIGNUP screens.
    /// </summary>
    public sealed class DialogField : Panel
    {
        private static readonly Color IdleBorderColor = Color.FromArgb(229, 231, 235);
        private static readonly Color ErrorBorderColor = Color.FromArgb(220, 38, 38);

        private readonly Label _caption = new Label();
        private readonly Label _error = new Label();
        private readonly FieldPanel _box = new FieldPanel();
        private readonly TextBox _text = new TextBox();
        private readonly ComboBox _choice = new ComboBox();
        private readonly DateTimePicker _date = new DateTimePicker();

        public DialogFieldKind Kind { get; private set; }

        public TextBox Input
        {
            get { return _text; }
        }

        public ComboBox ChoiceBox
        {
            get { return _choice; }
        }

        public DateTimePicker DateBox
        {
            get { return _date; }
        }

        public object? SelectedItem
        {
            get { return _choice.SelectedItem; }
        }

        public event EventHandler? ValueChanged;

        private DialogField(string caption, DialogFieldKind kind)
        {
            Kind = kind;

            BackColor = Color.White;
            Height = 88;
            Width = 300;
            TabStop = false;

            _caption.AutoSize = false;
            _caption.BackColor = Color.Transparent;
            _caption.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            _caption.ForeColor = Color.FromArgb(100, 116, 139);
            _caption.Location = new Point(2, 0);
            _caption.Size = new Size(Width, 18);
            _caption.Text = (caption ?? string.Empty).ToUpperInvariant();

            _box.BackColor = Color.White;
            _box.BorderColor = IdleBorderColor;
            _box.FillColor = Color.FromArgb(243, 244, 246);
            _box.FocusColor = Color.FromArgb(22, 163, 74);
            _box.CornerRadius = 10;
            _box.Location = new Point(0, 21);
            _box.Size = new Size(Width, 46);
            _box.TabIndex = 0;

            _error.AutoSize = false;
            _error.BackColor = Color.Transparent;
            _error.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            _error.ForeColor = ErrorBorderColor;
            _error.Location = new Point(2, 70);
            _error.Size = new Size(Width, 16);
            _error.TextAlign = ContentAlignment.MiddleLeft;
            _error.Visible = false;

            _text.BackColor = Color.FromArgb(243, 244, 246);
            _text.BorderStyle = BorderStyle.None;
            _text.Font = new Font("Segoe UI", 11F);
            _text.Location = new Point(18, 11);
            _text.Size = new Size(Width - 36, 25);
            _text.TextChanged += Input_Changed;
            _text.Enter += Box_Enter;
            _text.Leave += Box_Leave;

            _choice.BackColor = Color.FromArgb(243, 244, 246);
            _choice.DropDownStyle = ComboBoxStyle.DropDownList;
            _choice.FlatStyle = FlatStyle.Flat;
            _choice.Font = new Font("Segoe UI", 10.5F);
            _choice.Location = new Point(14, 9);
            _choice.Size = new Size(Width - 28, 28);
            _choice.SelectedIndexChanged += Input_Changed;
            _choice.Enter += Box_Enter;
            _choice.Leave += Box_Leave;

            _date.Font = new Font("Segoe UI", 10.5F);
            _date.Format = DateTimePickerFormat.Custom;
            _date.CustomFormat = "MMMM d, yyyy";
            _date.Location = new Point(14, 9);
            _date.Size = new Size(Width - 28, 26);
            _date.ValueChanged += Input_Changed;
            _date.Enter += Box_Enter;
            _date.Leave += Box_Leave;

            switch (kind)
            {
                case DialogFieldKind.Choice:
                    _box.Controls.Add(_choice);
                    _choice.TabIndex = 0;
                    break;

                case DialogFieldKind.Date:
                    _box.Controls.Add(_date);
                    _date.TabIndex = 0;
                    break;

                default:
                    _text.ReadOnly = kind == DialogFieldKind.ReadOnly;
                    _text.TabStop = kind != DialogFieldKind.ReadOnly;
                    _box.Controls.Add(_text);
                    _text.TabIndex = 0;
                    break;
            }

            Controls.Add(_box);
            Controls.Add(_caption);
            Controls.Add(_error);
        }

        public static DialogField ForText(string caption)
        {
            return new DialogField(caption, DialogFieldKind.Text);
        }

        public static DialogField ForReadOnlyText(string caption)
        {
            return new DialogField(caption, DialogFieldKind.ReadOnly);
        }

        public static DialogField ForChoice(string caption)
        {
            return new DialogField(caption, DialogFieldKind.Choice);
        }

        public static DialogField ForDate(string caption)
        {
            return new DialogField(caption, DialogFieldKind.Date);
        }

        public string Caption
        {
            get { return _caption.Text; }
            set { _caption.Text = (value ?? string.Empty).ToUpperInvariant(); }
        }

        public string Value
        {
            get
            {
                if (Kind == DialogFieldKind.Choice)
                {
                    return _choice.Text ?? string.Empty;
                }

                if (Kind == DialogFieldKind.Date)
                {
                    return _date.Value.ToString("MMMM dd, yyyy");
                }

                return _text.Text ?? string.Empty;
            }
            set
            {
                string text = value ?? string.Empty;

                if (Kind == DialogFieldKind.Choice)
                {
                    _choice.SelectedIndex = -1;
                    _choice.Text = text;
                }
                else if (Kind == DialogFieldKind.Date)
                {
                    if (DateTime.TryParse(text, out DateTime parsed))
                    {
                        _date.Value = parsed;
                    }
                }
                else
                {
                    _text.Text = text;
                }
            }
        }

        public void FocusInput()
        {
            if (Kind == DialogFieldKind.Choice)
            {
                _choice.Focus();
            }
            else if (Kind == DialogFieldKind.Date)
            {
                _date.Focus();
            }
            else
            {
                _text.Focus();
            }
        }

        public void ShowError(string message)
        {
            _error.Text = message ?? string.Empty;
            _error.Visible = !string.IsNullOrEmpty(message);
            _box.BorderColor = ErrorBorderColor;
            _box.Invalidate();
        }

        public void ClearError()
        {
            _error.Text = string.Empty;
            _error.Visible = false;
            _box.BorderColor = IdleBorderColor;
            _box.Invalidate();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            int width = Math.Max(80, Width);
            int innerWidth = Math.Max(40, width - 36);

            _caption.Width = width;
            _box.Width = width;
            _error.Width = width;

            _text.Width = innerWidth;
            _choice.Width = Math.Max(40, width - 28);
            _date.Width = Math.Max(40, width - 28);
        }

        private void Box_Enter(object? sender, EventArgs e)
        {
            _box.IsFocused = true;
        }

        private void Box_Leave(object? sender, EventArgs e)
        {
            _box.IsFocused = false;
        }

        private void Input_Changed(object? sender, EventArgs e)
        {
            if (_error.Visible)
            {
                ClearError();
            }

            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
