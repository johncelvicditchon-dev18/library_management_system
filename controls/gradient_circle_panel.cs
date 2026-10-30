using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace oop_project
{
    /// <summary>Controls where the orange glow circles are placed on the panel.</summary>
    public enum HeroStyle
    {
        /// <summary>Large glow bleeding off the top-right with rings - classic hero panel.</summary>
        Aurora,

        /// <summary>Centered glow, ideal behind a logo or title.</summary>
        Spotlight,

        /// <summary>Only a faint corner glow - very quiet and minimal.</summary>
        Minimal
    }

    /// <summary>
    /// Refined hero panel: deep green gradient, soft orange gradient glow circles,
    /// concentric rings, film grain and a gentle vignette.
    /// Drop it from the toolbox onto any form and dock it.
    /// </summary>
    [ToolboxItem(true)]
    [Description("Professional green gradient panel with soft orange gradient glow circles.")]
    [DefaultProperty(nameof(BackgroundTop))]
    [DefaultEvent(nameof(Click))]
    public class GradientCirclePanel : Panel
    {
        private static Bitmap? _grain;

        private Color _backgroundTop = Color.FromArgb(5, 61, 50);
        private Color _backgroundBottom = Color.FromArgb(12, 110, 79);

        private Color _circleHighlight = Color.FromArgb(253, 186, 116);
        private Color _circleBase = Color.FromArgb(234, 88, 12);

        private HeroStyle _style = HeroStyle.Spotlight;
        private int _circleOpacity = 88;
        private int _circleScale = 100;
        private int _circleCenterX = 50;
        private int _circleCenterY = 50;
        private int _cornerRadius;
        private int _vignetteStrength = 45;
        private bool _showAccentCircles = true;
        private bool _showRing = true;
        private bool _showGrain = true;
        private bool _showGrid;

        #region Background

        [Category("Appearance - Background")]
        [Description("Colour of the green gradient at the top of the panel.")]
        public Color BackgroundTop
        {
            get => _backgroundTop;
            set { _backgroundTop = value; Invalidate(); }
        }

        [Category("Appearance - Background")]
        [Description("Colour of the green gradient at the bottom of the panel.")]
        public Color BackgroundBottom
        {
            get => _backgroundBottom;
            set { _backgroundBottom = value; Invalidate(); }
        }

        [Category("Appearance - Background")]
        [Description("Rounded corner radius of the panel. 0 keeps sharp corners.")]
        [DefaultValue(0)]
        public int CornerRadius
        {
            get => _cornerRadius;
            set { _cornerRadius = Math.Max(0, value); Invalidate(); }
        }

        [Category("Appearance - Background")]
        [Description("Darkening around the edges (0 = flat, 100 = strong).")]
        [DefaultValue(45)]
        public int VignetteStrength
        {
            get => _vignetteStrength;
            set { _vignetteStrength = Math.Max(0, Math.Min(100, value)); Invalidate(); }
        }

        [Category("Appearance - Background")]
        [Description("Adds a very subtle film grain for a printed, premium feel.")]
        [DefaultValue(true)]
        public bool ShowGrain
        {
            get => _showGrain;
            set { _showGrain = value; Invalidate(); }
        }

        [Category("Appearance - Background")]
        [Description("Adds a faint dotted grid texture. Keep off for a clean look.")]
        [DefaultValue(false)]
        public bool ShowGrid
        {
            get => _showGrid;
            set { _showGrid = value; Invalidate(); }
        }

        #endregion

        #region Circles

        [Category("Appearance - Circles")]
        [Description("Where the orange gradient circle sits on the panel.")]
        [DefaultValue(HeroStyle.Spotlight)]
        public HeroStyle Style
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }

        [Category("Appearance - Circles")]
        [Description("Light orange used at the top-left of each glow.")]
        public Color CircleHighlight
        {
            get => _circleHighlight;
            set { _circleHighlight = value; Invalidate(); }
        }

        [Category("Appearance - Circles")]
        [Description("Deep orange used at the bottom-right of each glow.")]
        public Color CircleBase
        {
            get => _circleBase;
            set { _circleBase = value; Invalidate(); }
        }

        [Category("Appearance - Circles")]
        [Description("Strength of the orange glow (0 = invisible, 100 = bold).")]
        [DefaultValue(88)]
        public int CircleOpacity
        {
            get => _circleOpacity;
            set { _circleOpacity = Math.Max(0, Math.Min(100, value)); Invalidate(); }
        }

        [Category("Appearance - Circles")]
        [Description("Overall size of the glow design, as a percentage.")]
        [DefaultValue(100)]
        public int CircleScale
        {
            get => _circleScale;
            set { _circleScale = Math.Max(20, Math.Min(200, value)); Invalidate(); }
        }

        [Category("Appearance - Circles")]
        [Description("Moves the whole glow design horizontally (50 = default).")]
        [DefaultValue(50)]
        public int CircleCenterX
        {
            get => _circleCenterX;
            set { _circleCenterX = Math.Max(-50, Math.Min(150, value)); Invalidate(); }
        }

        [Category("Appearance - Circles")]
        [Description("Moves the whole glow design vertically (50 = default).")]
        [DefaultValue(50)]
        public int CircleCenterY
        {
            get => _circleCenterY;
            set { _circleCenterY = Math.Max(-50, Math.Min(150, value)); Invalidate(); }
        }

        [Category("Appearance - Circles")]
        [Description("Shows the second, quieter glow at the opposite corner.")]
        [DefaultValue(true)]
        public bool ShowAccentCircles
        {
            get => _showAccentCircles;
            set { _showAccentCircles = value; Invalidate(); }
        }

        [Category("Appearance - Circles")]
        [Description("Draws thin concentric rings around the main glow.")]
        [DefaultValue(true)]
        public bool ShowRing
        {
            get => _showRing;
            set { _showRing = value; Invalidate(); }
        }

        #endregion

        public GradientCirclePanel()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = _backgroundTop;
            DoubleBuffered = true;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Rectangle rect = ClientRectangle;
            if (rect.Width <= 0 || rect.Height <= 0)
            {
                return;
            }

            bool rounded = _cornerRadius > 0;
            if (rounded)
            {
                g.Clear(Parent?.BackColor ?? Color.White);
            }
            else
            {
                g.Clear(_backgroundTop);
            }

            using GraphicsPath? clip = rounded
                ? DrawHelper.CreateRoundedPath(rect, _cornerRadius)
                : null;

            GraphicsState? state = null;
            if (clip is not null)
            {
                state = g.Save();
                g.SetClip(clip);
            }

            try
            {
                PaintBase(g, rect);
                PaintVignette(g, rect);
                PaintCircleDesign(g, rect);

                if (_showGrid)
                {
                    PaintGrid(g, rect);
                }

                PaintSheen(g, rect);

                if (_showGrain)
                {
                    PaintGrain(g, rect);
                }
            }
            finally
            {
                if (state is not null && clip is not null)
                {
                    g.Restore(state);
                }
            }
        }

        private void PaintBase(Graphics g, Rectangle rect)
        {
            using (var brush = new LinearGradientBrush(
                       rect,
                       _backgroundTop,
                       _backgroundBottom,
                       LinearGradientMode.Vertical))
            {
                g.FillRectangle(brush, rect);
            }

            // Cool diagonal wash so the gradient is not a flat two-tone ramp.
            using (var wash = new LinearGradientBrush(
                       rect,
                       Color.FromArgb(38, 2, 44, 36),
                       Color.FromArgb(0, 2, 44, 36),
                       LinearGradientMode.ForwardDiagonal))
            {
                g.FillRectangle(wash, rect);
            }
        }

        private void PaintCircleDesign(Graphics g, Rectangle rect)
        {
            OrbSpec[] orbs = GetOrbs();

            float scale = _circleScale / 100f;
            float unit = Math.Min(rect.Width, rect.Height) * scale;

            float shiftX = rect.Width * ((_circleCenterX - 50) / 100f);
            float shiftY = rect.Height * ((_circleCenterY - 50) / 100f);

            bool first = true;
            foreach (OrbSpec spec in orbs)
            {
                if (!first && !_showAccentCircles)
                {
                    break;
                }

                float diameter = Math.Max(4f, unit * spec.Diameter);
                var circle = new RectangleF(
                    rect.Width * spec.CenterX + shiftX - diameter / 2f,
                    rect.Height * spec.CenterY + shiftY - diameter / 2f,
                    diameter,
                    diameter);

                if (spec.Fill)
                {
                    PaintGlow(g, circle, spec.Opacity);
                }

                if (_showRing && spec.Rings)
                {
                    PaintRings(g, circle, spec.Opacity);
                }
                first = false;
            }
        }

        private OrbSpec[] GetOrbs()
        {
            return _style switch
            {
                HeroStyle.Spotlight => new[]
                {
                    new OrbSpec(0.50f, 0.44f, 0.80f, 1.00f, true, true),
                    new OrbSpec(0.04f, 0.99f, 0.44f, 0.55f, false, true)
                },
                HeroStyle.Minimal => new[]
                {
                    new OrbSpec(0.98f, 0.06f, 0.66f, 0.50f, false, true)
                },
                _ => new[]
                {
                    new OrbSpec(0.82f, 0.13f, 0.90f, 1.00f, true, true),
                    new OrbSpec(0.02f, 0.96f, 0.58f, 0.55f, false, true)
                }
            };
        }

        private void PaintGlow(Graphics g, RectangleF circle, float strength)
        {
            if (circle.Width < 4f)
            {
                return;
            }

            int bodyAlpha = (int)(_circleOpacity / 100f * 255f * strength);
            if (bodyAlpha <= 0)
            {
                return;
            }

            float cx = circle.X + circle.Width / 2f;
            float cy = circle.Y + circle.Height / 2f;

            PaintHalo(g, circle, strength);

            // The orb: light amber to burnt orange, opaque enough to stay orange.
            using (var body = new LinearGradientBrush(
                       circle,
                       Color.FromArgb(bodyAlpha, _circleHighlight),
                       Color.FromArgb(bodyAlpha, _circleBase),
                       LinearGradientMode.ForwardDiagonal))
            {
                g.FillEllipse(body, circle);
            }

            // Warm inner shadow on the lower-right so the orb reads as a lit sphere.
            var shade = new RectangleF(
                circle.X + circle.Width * 0.015f,
                circle.Y + circle.Height * 0.03f,
                circle.Width * 0.97f,
                circle.Height * 0.97f);

            using (var shadePath = new GraphicsPath())
            {
                shadePath.AddEllipse(shade);

                using var shadeBrush = new PathGradientBrush(shadePath)
                {
                    CenterPoint = new PointF(
                        shade.X + shade.Width * 0.80f,
                        shade.Y + shade.Height * 0.84f),
                    CenterColor = Color.FromArgb(64, 124, 45, 18),
                    SurroundColors = new[] { Color.FromArgb(0, 124, 45, 18) },
                    FocusScales = new PointF(0.18f, 0.18f)
                };

                g.FillPath(shadeBrush, shadePath);
            }

            // Hairline rim for a crisp, finished edge.
            var rim = new RectangleF(
                circle.X + 0.7f,
                circle.Y + 0.7f,
                circle.Width - 1.4f,
                circle.Height - 1.4f);

            using (var rimPen = new Pen(Color.FromArgb(52, 255, 252, 245), 1.2f))
            {
                g.DrawEllipse(rimPen, rim);
            }
        }

        /// <summary>Soft orange glow behind an orb or around a ring.</summary>
        private void PaintHalo(Graphics g, RectangleF circle, float strength)
        {
            int alpha = (int)(_circleOpacity / 100f * 255f * strength);
            if (alpha <= 0)
            {
                return;
            }

            float cx = circle.X + circle.Width / 2f;
            float cy = circle.Y + circle.Height / 2f;

            float spread = circle.Width * 0.16f;
            var halo = new RectangleF(
                circle.X - spread,
                circle.Y - spread,
                circle.Width + spread * 2f,
                circle.Height + spread * 2f);

            using var haloPath = new GraphicsPath();
            haloPath.AddEllipse(halo);

            using var haloBrush = new PathGradientBrush(haloPath)
            {
                CenterPoint = new PointF(cx, cy),
                CenterColor = Color.FromArgb((int)(alpha * 0.28f), _circleBase),
                SurroundColors = new[] { Color.FromArgb(0, _circleBase) },
                FocusScales = new PointF(0.70f, 0.70f)
            };

            g.FillPath(haloBrush, haloPath);
        }

        private void PaintRings(Graphics g, RectangleF circle, float strength)
        {
            float cx = circle.X + circle.Width / 2f;
            float cy = circle.Y + circle.Height / 2f;
            int baseAlpha = (int)(_circleOpacity / 100f * 70f * strength) + 12;

            for (int i = 0; i < 2; i++)
            {
                float grow = 1.07f + i * 0.14f;
                float d = circle.Width * grow;
                var ringRect = new RectangleF(cx - d / 2f, cy - d / 2f, d, d);
                int alpha = Math.Max(0, baseAlpha / (i + 1));

                using var pen = new Pen(Color.FromArgb(alpha, 255, 246, 234), 1.2f);
                g.DrawEllipse(pen, ringRect);
            }
        }

        private void PaintVignette(Graphics g, Rectangle rect)
        {
            if (_vignetteStrength <= 0)
            {
                return;
            }

            int alpha = (int)(_vignetteStrength / 100f * 150f);

            using var path = new GraphicsPath();
            path.AddRectangle(rect);

            using var brush = new PathGradientBrush(path)
            {
                CenterColor = Color.FromArgb(0, 0, 0, 0),
                SurroundColors = new[] { Color.FromArgb(alpha, 0, 0, 0) },
                FocusScales = new PointF(0.62f, 0.62f)
            };

            g.FillPath(brush, path);
        }

        private void PaintGrid(Graphics g, Rectangle rect)
        {
            const int spacing = 26;

            using var brush = new SolidBrush(Color.FromArgb(16, 255, 255, 255));

            for (int y = spacing / 2; y < rect.Height; y += spacing)
            {
                for (int x = spacing / 2; x < rect.Width; x += spacing)
                {
                    g.FillEllipse(brush, x, y, 1.6f, 1.6f);
                }
            }
        }

        private static void PaintSheen(Graphics g, Rectangle rect)
        {
            // Wide diagonal light sweep from the top-left corner.
            using var brush = new LinearGradientBrush(
                rect,
                Color.FromArgb(26, 255, 255, 255),
                Color.FromArgb(0, 255, 255, 255),
                LinearGradientMode.ForwardDiagonal);

            g.FillRectangle(brush, rect);
        }

        private void PaintGrain(Graphics g, Rectangle rect)
        {
            using var brush = new TextureBrush(Grain, WrapMode.Tile);
            g.FillRectangle(brush, rect);
        }

        private static Bitmap Grain => _grain ??= CreateGrain();

        private static Bitmap CreateGrain()
        {
            const int size = 128;

            var bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, size, size),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb);

            var buffer = new byte[data.Stride * size];
            var random = new Random(20240517);

            for (int i = 0; i < buffer.Length; i += 4)
            {
                byte tone = (byte)random.Next(90, 220);
                byte alpha = (byte)random.Next(4, 12);

                buffer[i] = tone;
                buffer[i + 1] = tone;
                buffer[i + 2] = tone;
                buffer[i + 3] = alpha;
            }

            Marshal.Copy(buffer, 0, data.Scan0, buffer.Length);
            bitmap.UnlockBits(data);

            return bitmap;
        }

        private readonly record struct OrbSpec(
            float CenterX,
            float CenterY,
            float Diameter,
            float Opacity,
            bool Fill,
            bool Rings);
    }
}
