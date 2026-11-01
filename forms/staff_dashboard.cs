using System;
using System.Drawing;
using System.Windows.Forms;

namespace oop_project.forms
{
    /// <summary>
    /// Staff keep the library management tools: books, loans, requests and the
    /// staff / faculty / student account tabs. The librarian-only account tab is
    /// removed so staff cannot manage librarian accounts.
    ///
    /// This is deliberately a management dashboard, not the student/faculty
    /// patron portal. The two audiences need different jobs done.
    /// </summary>
    public sealed partial class staff_dashboard : admin_dashboard
    {
        private static readonly Color TabSelectedText = Color.FromArgb(4, 120, 87);
        private static readonly Color TabNormalText = Color.FromArgb(100, 116, 139);
        private static readonly Color TabSelectedFill = Color.FromArgb(240, 253, 244);
        private static readonly Color TabBarColor = Color.FromArgb(22, 163, 74);

        public staff_dashboard(int accountID, string fullName, string role, string username = "", string status = "")
            : base(accountID, fullName, role, username, status)
        {
            InitializeComponent();

            Text = "TCGC - STAFF DASHBOARD";

            HideLibrarianAccountsTab();
            ApplyStaffHeader();
            StyleNavigation();
        }

        /// <summary>
        /// The base dashboard is shared by librarians and staff. A staff account
        /// must not see the librarian account tab, so it is removed by its visible
        /// title instead of by a hard-coded tab index.
        /// </summary>
        private void HideLibrarianAccountsTab()
        {
            TabControl? tabs = FindControl<TabControl>(this);
            if (tabs == null)
            {
                return;
            }

            for (int i = tabs.TabPages.Count - 1; i >= 0; i--)
            {
                if (string.Equals(tabs.TabPages[i].Text, "LIBRARIAN ACCOUNTS", StringComparison.OrdinalIgnoreCase))
                {
                    tabs.TabPages.RemoveAt(i);
                }
            }
        }

        private void ApplyStaffHeader()
        {
            Label? subtitle = FindControl<Label>(this, "brandSubtitle");
            if (subtitle != null)
            {
                subtitle.Text = "STAFF OPERATIONS CENTER";
            }

            Label? overviewHeading = FindControl<Label>(this, "overviewHeading");
            if (overviewHeading != null)
            {
                overviewHeading.Text = "STAFF OPERATIONS CENTER";
            }
        }

        /// <summary>
        /// The management tabs get the same flat, rounded look as the patron
        /// navigation: white strip, green selected tab and a short underline.
        /// </summary>
        private void StyleNavigation()
        {
            TabControl? tabs = FindControl<TabControl>(this);
            if (tabs == null)
            {
                return;
            }

            tabs.Appearance = TabAppearance.Normal;
            tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabs.SizeMode = TabSizeMode.Fixed;
            tabs.ItemSize = new Size(150, 40);
            tabs.Padding = new Point(12, 6);
            tabs.DrawItem += Tabs_DrawItem;
            tabs.Resize += delegate { ResizeTabs(tabs); };
            ResizeTabs(tabs);
        }

        private static void ResizeTabs(TabControl tabs)
        {
            int count = Math.Max(1, tabs.TabPages.Count);
            int width = Math.Max(96, Math.Min(184, (tabs.ClientSize.Width - 10) / count));

            if (tabs.ItemSize.Width != width)
            {
                tabs.ItemSize = new Size(width, 40);
            }
        }

        private void Tabs_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (sender is not TabControl tabs || e.Index < 0 || e.Index >= tabs.TabPages.Count)
            {
                return;
            }

            Rectangle bounds = e.Bounds;
            bool selected = tabs.SelectedIndex == e.Index;

            using (SolidBrush fill = new SolidBrush(selected ? TabSelectedFill : Color.White))
            {
                e.Graphics.FillRectangle(fill, bounds);
            }

            if (selected)
            {
                using SolidBrush bar = new SolidBrush(TabBarColor);
                e.Graphics.FillRectangle(bar, new Rectangle(bounds.X + 12, bounds.Bottom - 4, bounds.Width - 24, 3));
            }

            string text = tabs.TabPages[e.Index].Text.ToUpperInvariant();
            using Font font = new Font("Segoe UI", 8.5F, selected ? FontStyle.Bold : FontStyle.Regular);
            TextRenderer.DrawText(
                e.Graphics,
                text,
                font,
                bounds,
                selected ? TabSelectedText : TabNormalText,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPadding);
        }

        private static T? FindControl<T>(Control root, string? name = null) where T : Control
        {
            if (root is T match &&
                (name == null || string.Equals(root.Name, name, StringComparison.Ordinal)))
            {
                return match;
            }

            foreach (Control child in root.Controls)
            {
                T? found = FindControl<T>(child, name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
