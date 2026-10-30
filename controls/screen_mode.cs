using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace oop_project
{
    /// <summary>
    /// Gives every window in the app the same full screen behaviour: it opens
    /// maximised and F11 switches between the maximised window and a borderless
    /// view that covers the whole monitor. Pressing F11 again restores the window.
    /// </summary>
    public static class ScreenMode
    {
        private sealed class Snapshot
        {
            public FormBorderStyle BorderStyle { get; set; }
            public FormWindowState WindowState { get; set; }
            public Rectangle Bounds { get; set; }
        }

        private static readonly ConditionalWeakTable<Form, Snapshot> Snapshots =
            new ConditionalWeakTable<Form, Snapshot>();

        public static void Apply(Form form, Size? minimumSize = null)
        {
            if (form == null)
            {
                throw new ArgumentNullException(nameof(form));
            }

            form.FormBorderStyle = FormBorderStyle.Sizable;
            form.MaximizeBox = true;
            form.MinimizeBox = true;

            if (minimumSize.HasValue)
            {
                form.MinimumSize = minimumSize.Value;
            }
            else if (form.MinimumSize.Width <= 0 || form.MinimumSize.Height <= 0)
            {
                form.MinimumSize = new Size(1100, 700);
            }

            form.WindowState = FormWindowState.Maximized;
            EnableFullScreenToggle(form);
        }

        public static void EnableFullScreenToggle(Form form)
        {
            if (form == null)
            {
                throw new ArgumentNullException(nameof(form));
            }

            form.KeyPreview = true;
            form.KeyDown -= Form_KeyDown;
            form.KeyDown += Form_KeyDown;
        }

        /// <summary>Switches one window between borderless full screen and its normal frame.</summary>
        public static void Toggle(Form form)
        {
            if (form == null)
            {
                return;
            }

            if (Snapshots.TryGetValue(form, out Snapshot? snapshot))
            {
                form.WindowState = FormWindowState.Normal;
                form.FormBorderStyle = snapshot.BorderStyle;
                form.Bounds = snapshot.Bounds;
                form.WindowState = snapshot.WindowState;
                Snapshots.Remove(form);
                return;
            }

            Screen? screen = Screen.FromControl(form) ?? Screen.PrimaryScreen;
            if (screen == null)
            {
                return;
            }

            Snapshots.Add(form, new Snapshot
            {
                BorderStyle = form.FormBorderStyle,
                WindowState = form.WindowState,
                Bounds = form.WindowState == FormWindowState.Maximized ? form.RestoreBounds : form.Bounds
            });

            form.WindowState = FormWindowState.Normal;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Bounds = screen.Bounds;
        }

        private static void Form_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.F11)
            {
                return;
            }

            e.Handled = true;
            e.SuppressKeyPress = true;

            if (sender is Form form)
            {
                Toggle(form);
            }
        }
    }
}
