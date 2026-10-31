using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using oop_project.Models;

namespace oop_project
{
    /// <summary>
    /// Small reusable picker used by ADD BOOK and EDIT BOOK. It previews the
    /// selected cover and keeps the bytes that LibraryCatalog writes to
    /// books_table.book_image.
    /// </summary>
    public sealed class BookCoverPicker : Panel
    {
        private readonly PictureBox _preview;
        private readonly Label _placeholder;
        private readonly Label _fileName;
        private readonly RoundedButton _chooseButton;
        private readonly RoundedButton _removeButton;
        private Image? _previewImage;

        public byte[]? ImageBytes { get; private set; }

        public bool HasImage
        {
            get { return ImageBytes != null && ImageBytes.Length > 0; }
        }

        public BookCoverPicker()
        {
            BackColor = Color.Transparent;
            Size = new Size(576, 172);

            Label caption = new Label();
            caption.AutoSize = true;
            caption.BackColor = Color.Transparent;
            caption.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            caption.ForeColor = Color.FromArgb(71, 85, 105);
            caption.Location = new Point(0, 0);
            caption.Text = "BOOK COVER (OPTIONAL)";

            Panel frame = new Panel();
            frame.BackColor = Color.FromArgb(241, 245, 249);
            frame.BorderStyle = BorderStyle.FixedSingle;
            frame.Location = new Point(0, 24);
            frame.Size = new Size(128, 136);

            _preview = new PictureBox();
            _preview.BackColor = Color.White;
            _preview.Dock = DockStyle.Fill;
            _preview.SizeMode = PictureBoxSizeMode.Zoom;
            _preview.TabIndex = 0;
            _preview.TabStop = false;

            _placeholder = new Label();
            _placeholder.BackColor = Color.White;
            _placeholder.Dock = DockStyle.Fill;
            _placeholder.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            _placeholder.ForeColor = Color.FromArgb(148, 163, 184);
            _placeholder.Text = "NO COVER\r\nSELECTED";
            _placeholder.TextAlign = ContentAlignment.MiddleCenter;

            frame.Controls.Add(_placeholder);
            frame.Controls.Add(_preview);
            _placeholder.BringToFront();

            _chooseButton = CreateButton("CHOOSE IMAGE", new Point(146, 26), new Size(168, 40),
                Color.FromArgb(22, 163, 74), Color.FromArgb(21, 128, 61), Color.FromArgb(20, 83, 45));
            _chooseButton.Click += ChooseButton_Click;

            _removeButton = CreateButton("REMOVE", new Point(146, 76), new Size(118, 38),
                Color.FromArgb(100, 116, 139), Color.FromArgb(71, 85, 105), Color.FromArgb(51, 65, 85));
            _removeButton.Click += RemoveButton_Click;

            _fileName = new Label();
            _fileName.AutoEllipsis = true;
            _fileName.BackColor = Color.Transparent;
            _fileName.Font = new Font("Segoe UI", 8.25F);
            _fileName.ForeColor = Color.FromArgb(148, 163, 184);
            _fileName.Location = new Point(146, 124);
            _fileName.Size = new Size(390, 20);
            _fileName.Text = "JPG, PNG OR BMP • MAX 5 MB";

            Label hint = new Label();
            hint.AutoSize = false;
            hint.BackColor = Color.Transparent;
            hint.Font = new Font("Segoe UI", 8.25F);
            hint.ForeColor = Color.FromArgb(148, 163, 184);
            hint.Location = new Point(146, 146);
            hint.Size = new Size(400, 20);
            hint.Text = "THE PICTURE IS STORED WITH THE BOOK ROW IN MYSQL.";

            Controls.Add(hint);
            Controls.Add(_fileName);
            Controls.Add(_removeButton);
            Controls.Add(_chooseButton);
            Controls.Add(frame);
            Controls.Add(caption);

            Resize += BookCoverPicker_Resize;
            RefreshPreview();
        }

        private static RoundedButton CreateButton(
            string text, Point location, Size size, Color normal, Color hover, Color pressed)
        {
            RoundedButton button = new RoundedButton();
            button.CornerRadius = 10;
            button.FlatAppearance.BorderSize = 0;
            button.FlatStyle = FlatStyle.Flat;
            button.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            button.ForeColor = Color.White;
            button.HoverColor = hover;
            button.Location = location;
            button.NormalColor = normal;
            button.PressedColor = pressed;
            button.Size = size;
            button.Text = text;
            button.UseVisualStyleBackColor = false;
            return button;
        }

        public void LoadImage(byte[]? bytes)
        {
            ImageBytes = bytes == null || bytes.Length == 0 ? null : (byte[])bytes.Clone();
            RefreshPreview();
        }

        public void ClearImage()
        {
            ImageBytes = null;
            _fileName.Text = "JPG, PNG OR BMP • MAX 5 MB";
            RefreshPreview();
        }

        private void ChooseButton_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files|*.jpg;*.jpeg;*.png;*.bmp|All files|*.*";
                dialog.Multiselect = false;
                dialog.Title = "CHOOSE THE BOOK COVER";

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    FileInfo info = new FileInfo(dialog.FileName);

                    if (info.Length > Book.MaximumCoverBytes)
                    {
                        MessageBox.Show(
                            "THE PICTURE IS LARGER THAN 5 MB. PLEASE CHOOSE A SMALLER IMAGE.",
                            "COVER TOO LARGE",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }

                    byte[] bytes = File.ReadAllBytes(dialog.FileName);

                    using (Image? check = BookImage.Load(bytes))
                    {
                        if (check == null)
                        {
                            throw new ArgumentException("The selected file is not a readable image.");
                        }
                    }

                    ImageBytes = bytes;
                    _fileName.Text = info.Name.ToUpperInvariant() + " • " +
                                     (info.Length / 1024d).ToString("0.0") + " KB";
                    RefreshPreview();
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException)
                {
                    MessageBox.Show(
                        "THE COVER COULD NOT BE OPENED.\n" + ex.Message.ToUpperInvariant(),
                        "INVALID COVER",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
        }

        private void RemoveButton_Click(object? sender, EventArgs e)
        {
            ClearImage();
        }

        private void RefreshPreview()
        {
            _previewImage?.Dispose();
            _previewImage = BookImage.Load(ImageBytes);
            _preview.Image = _previewImage;
            _preview.Visible = _previewImage != null;
            _placeholder.Visible = _previewImage == null;
            _removeButton.Enabled = HasImage;

            if (HasImage)
            {
                _placeholder.SendToBack();
                _preview.BringToFront();
            }
            else
            {
                _preview.SendToBack();
                _placeholder.BringToFront();
            }
        }

        private void BookCoverPicker_Resize(object? sender, EventArgs e)
        {
            _fileName.Width = Math.Max(120, Width - 150);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _previewImage?.Dispose();
                _previewImage = null;
            }

            base.Dispose(disposing);
        }
    }
}
