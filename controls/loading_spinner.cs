using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace oop_project
{
    /// <summary>
    /// The circular loading indicator used while an account is being checked.
    /// The ring sweeps a green to orange gradient so it matches the palette of
    /// the sign in screen (green card accents, orange "Sign Up" link).
    /// </summary>
    [ToolboxItem(true)]
    [Description("Circular loading indicator that sweeps a green to orange gradient around the ring.")]
    public sealed class GradientSpinner : Control
    {
        private const float DegreesPerTick = 6f;
        private const int Segments = 48;

        private readonly System.Windows.Forms.Timer _animation;
        private float _angle;
        private float _sweep = 300f;
        private int _thickness = 6;
        private Color _startColor = Color.FromArgb(34, 197, 94);
        private Color _endColor = Color.FromArgb(249, 115, 22);

        [Category("Appearance")]
        [DefaultValue(6)]
        [Description("Width of the ring in pixels.")]
        public int Thickness
        {
            get => _thickness;
            set
            {
                _thickness = Math.Max(1, value);
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Color at the head of the sweep.")]
        public Color StartColor
        {
            get => _startColor;
            set
            {
                _startColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Color at the tail of the sweep.")]
        public Color EndColor
        {
            get => _endColor;
            set
            {
                _endColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(300f)]
        [Description("How much of the circle the arc covers, in degrees.")]
        public float Sweep
        {
            get => _sweep;
            set
            {
                _sweep = Math.Clamp(value, 30f, 360f);
                Invalidate();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsSpinning => _animation.Enabled;

        public GradientSpinner()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.White;
            Size = new Size(52, 52);

            _animation = new System.Windows.Forms.Timer { Interval = 16 };
            _animation.Tick += Animation_Tick;
        }

        public void Start()
        {
            if (!_animation.Enabled)
            {
                _animation.Start();
            }
        }

        public void Stop()
        {
            if (_animation.Enabled)
            {
                _animation.Stop();
            }
        }

        private void Animation_Tick(object? sender, EventArgs e)
        {
            _angle = (_angle + DegreesPerTick) % 360f;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // A transparent background is painted by the parent control instead.
            if (BackColor.A == 255)
            {
                g.Clear(BackColor);
            }

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            float thickness = _thickness;
            float size = Math.Min(Width, Height) - thickness;
            if (size <= 2f)
            {
                return;
            }

            var ring = new RectangleF((Width - size) / 2f, (Height - size) / 2f, size, size);

            // Faint full circle so the empty part of the sweep still reads as a ring.
            using (var track = new Pen(Color.FromArgb(36, _startColor), thickness))
            {
                g.DrawEllipse(track, ring);
            }

            // The arc is drawn in small pieces so every piece can carry its own
            // color: green at the tail, orange at the head, fading slightly so the
            // sweep still reads as motion.
            float step = _sweep / Segments;
            for (int i = 0; i < Segments; i++)
            {
                float progress = Segments == 1 ? 1f : (float)i / (Segments - 1);
                Color color = Blend(_startColor, _endColor, progress);

                // Only the very tail fades out; the rest of the ring stays solid so
                // the green to orange gradient keeps its colour.
                float fade = progress < 0.22f ? 0.25f + 0.75f * (progress / 0.22f) : 1f;
                color = Color.FromArgb((int)Math.Round(255 * fade), color);

                using var pen = new Pen(color, thickness);
                g.DrawArc(pen, ring, _angle + i * step, step + 0.8f);
            }
        }

        internal static Color Blend(Color from, Color to, float amount)
        {
            amount = Math.Clamp(amount, 0f, 1f);

            return Color.FromArgb(
                (int)Math.Round(from.A + (to.A - from.A) * amount),
                (int)Math.Round(from.R + (to.R - from.R) * amount),
                (int)Math.Round(from.G + (to.G - from.G) * amount),
                (int)Math.Round(from.B + (to.B - from.B) * amount));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animation.Stop();
                _animation.Tick -= Animation_Tick;
                _animation.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// <summary>
    /// The loading state of the sign in form: a dimmed version of the page with a
    /// single spinner in the middle of the window - no card, no caption. It also
    /// swallows every click, so the form cannot be submitted twice.
    /// </summary>
    [ToolboxItem(true)]
    [Description("Dimmed loading screen with one centered green to orange spinner.")]
    public sealed class LoadingOverlay : Control
    {
        private const int SpinnerSize = 80;

        private readonly GradientSpinner _spinner;

        [Category("Appearance")]
        [Description("Top color of the dimmed background.")]
        public Color VeilTop
        {
            get => _veilTop;
            set { _veilTop = value; Invalidate(); }
        }

        [Category("Appearance")]
        [Description("Bottom color of the dimmed background.")]
        public Color VeilBottom
        {
            get => _veilBottom;
            set { _veilBottom = value; Invalidate(); }
        }

        private Color _veilTop = Color.FromArgb(4, 55, 41);
        private Color _veilBottom = Color.FromArgb(15, 114, 52);

        public LoadingOverlay()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            BackColor = _veilTop;

            // The spinner is a transparent child, so the veil is painted behind it
            // by OnPaintBackground and no square of a different color shows up.
            _spinner = new GradientSpinner
            {
                Size = new Size(SpinnerSize, SpinnerSize),
                Thickness = 8,
                BackColor = Color.Transparent,
                Visible = false
            };
            Controls.Add(_spinner);

            TabStop = false;
            Visible = false;
        }

        /// <summary>Shows the dimmed page and starts the circle turning.</summary>
        public void ShowLoading()
        {
            Visible = true;
            BringToFront();
            LayoutSpinner();

            _spinner.Visible = true;
            _spinner.Start();
            Invalidate();
        }

        /// <summary>Stops the circle but keeps it on screen (the sign in finished).</summary>
        public void StopSpinning()
        {
            _spinner.Stop();
        }

        public void HideLoading()
        {
            _spinner.Stop();
            _spinner.Visible = false;
            Visible = false;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutSpinner();
        }

        // Dead center of the window, whatever its size.
        private void LayoutSpinner()
        {
            _spinner.Location = new Point(
                (ClientSize.Width - _spinner.Width) / 2,
                (ClientSize.Height - _spinner.Height) / 2);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle rect = ClientRectangle;
            if (rect.Width <= 0 || rect.Height <= 0)
            {
                return;
            }

            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var veil = new LinearGradientBrush(rect, _veilTop, _veilBottom, LinearGradientMode.Vertical))
            {
                g.FillRectangle(veil, rect);
            }

            // The same soft highlight the sign in background uses, so the loading
            // state still feels like the same screen.
            using (var glow = new SolidBrush(Color.FromArgb(10, 255, 255, 255)))
            {
                g.FillEllipse(glow, rect.Width * 0.42f, -rect.Width * 0.38f, rect.Width * 1.05f, rect.Width * 1.05f);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _spinner.Stop();
            }

            base.Dispose(disposing);
        }
    }
}

