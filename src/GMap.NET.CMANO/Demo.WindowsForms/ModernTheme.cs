using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Demo.WindowsForms;

// Shared native controls keep text, focus and layout crisp at the monitor's DPI.
internal static class ModernTheme
{
    internal static readonly Color Background = Color.FromArgb(243, 246, 250);
    internal static readonly Color Ink = Color.FromArgb(25, 43, 62);
    internal static readonly Color Muted = Color.FromArgb(102, 119, 136);
    internal static readonly Color Border = Color.FromArgb(221, 229, 237);
    internal static readonly Color Accent = Color.FromArgb(8, 123, 137);
    internal static readonly Color AccentSoft = Color.FromArgb(230, 245, 246);
    internal static readonly Font Body = new Font("Microsoft YaHei UI", 9f);
    internal static readonly Font Strong = new Font("Microsoft YaHei UI", 9f, FontStyle.Bold);
    internal static readonly Font Heading = new Font("Microsoft YaHei UI", 13f, FontStyle.Bold);
    internal static readonly Font Brand = new Font("Segoe UI", 17f, FontStyle.Bold);

    internal static Label Label(string text, bool strong = false, Color? color = null) => new Label
    {
        Text = text, Dock = DockStyle.Fill, AutoSize = false, Margin = Padding.Empty,
        TextAlign = ContentAlignment.MiddleLeft, Font = strong ? Strong : Body,
        ForeColor = color ?? (strong ? Ink : Muted), UseMnemonic = false
    };

    internal static TableLayoutPanel Stack(string name = null) => new TableLayoutPanel
    {
        Name = name, Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
        ColumnCount = 1, RowCount = 0, Margin = Padding.Empty, BackColor = Color.White,
        ColumnStyles = { new ColumnStyle(SizeType.Percent, 100) }
    };

    internal static void Add(TableLayoutPanel parent, Control child, int height = 0)
    {
        int row = parent.RowCount++;
        parent.RowStyles.Add(new RowStyle(height == 0 ? SizeType.AutoSize : SizeType.Absolute, height));
        child.Dock = DockStyle.Fill;
        parent.Controls.Add(child, 0, row);
    }

    internal static TableLayoutPanel Columns(params Control[] controls)
    {
        var row = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = controls.Length, RowCount = 1, Margin = Padding.Empty };
        row.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        for (int i = 0; i < controls.Length; i++)
        {
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / controls.Length));
            controls[i].Dock = DockStyle.Fill;
            controls[i].Margin = new Padding(i == 0 ? 0 : 4, 0, i == controls.Length - 1 ? 0 : 4, 0);
            row.Controls.Add(controls[i], i, 0);
        }
        return row;
    }

    internal static TableLayoutPanel Card(TableLayoutPanel page, string title, string caption = null)
    {
        var card = new SurfaceCard { ColumnCount = 1, RowCount = 0, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 0, 0, 12), Padding = new Padding(14), BackColor = Color.White };
        card.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Add(card, Label(title, true), 27);
        if (caption != null) Add(card, Label(caption), 38);
        Add(page, card);
        return card;
    }

    internal static WorkspaceButton Button(string text, string name = null, bool primary = false) => new WorkspaceButton
    { Text = text, Name = name, Primary = primary, AccessibleName = text, Font = primary ? Strong : Body, Margin = new Padding(0, 4, 0, 4) };

    internal static void Style(Button button, string text, bool primary = false)
    {
        if (button is WorkspaceButton modern) modern.Primary = primary;
        button.Text = text; button.AccessibleName = text; button.AutoSize = false;
        button.FlatStyle = FlatStyle.Flat; button.FlatAppearance.BorderSize = primary ? 0 : 1;
        button.FlatAppearance.BorderColor = Border; button.FlatAppearance.MouseOverBackColor = primary ? Color.FromArgb(6, 103, 115) : AccentSoft;
        button.BackColor = primary ? Accent : Color.White; button.ForeColor = primary ? Color.White : Ink;
        button.Font = primary ? Strong : Body; button.UseVisualStyleBackColor = false;
        button.Padding = Padding.Empty; button.Margin = new Padding(0, 4, 0, 4);
    }

    internal static void Input(Control control)
    {
        control.Dock = DockStyle.Fill; control.Margin = new Padding(0, 2, 0, 4);
        control.Font = Body; control.ForeColor = Ink; control.BackColor = Color.White;
        if (control is TextBox text) { text.BorderStyle = BorderStyle.FixedSingle; text.Multiline = false; text.ReadOnly = false; }
        if (control is ComboBox combo) { combo.DropDownStyle = ComboBoxStyle.DropDownList; combo.FlatStyle = FlatStyle.Standard; }
    }

    internal static void Check(CheckBox check, string text)
    {
        check.Text = text; check.AutoSize = false; check.Dock = DockStyle.Fill;
        check.Font = Body; check.ForeColor = Ink; check.Margin = Padding.Empty;
        check.UseVisualStyleBackColor = true;
    }

    internal sealed class SurfaceCard : TableLayoutPanel
    {
        internal SurfaceCard() { DoubleBuffered = true; }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using var pen = new Pen(SystemInformation.HighContrast ? SystemColors.WindowText : Border);
            e.Graphics.DrawRectangle(pen, 0, 0, Math.Max(0, Width - 1), Math.Max(0, Height - 1));
        }
    }
}

internal sealed class WorkspaceButton : Button
{
    private bool hovered;
    private bool pressed;
    internal bool Primary { get; set; }
    internal bool Selected { get; set; }
    internal WorkspaceButton()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Cursor = Cursors.Hand; FlatStyle = FlatStyle.Flat; TabStop = true; Size = new Size(100, 36);
    }
    protected override void OnMouseEnter(EventArgs e) { hovered = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hovered = false; pressed = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { pressed = true; Invalidate(); base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e) { pressed = false; Invalidate(); base.OnMouseUp(e); }
    protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
    protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }
    protected override void OnPaint(PaintEventArgs e)
    {
        float scale = DeviceDpi / 96f;
        var bounds = new RectangleF(0.5f, 0.5f, Width - 1, Height - 1);
        float radius = Math.Min(7 * scale, Math.Min(Width, Height) / 2f);
        using var path = new GraphicsPath();
        float diameter = radius * 2;
        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90); path.CloseFigure();
        Color fill = !Enabled ? Color.FromArgb(235, 240, 244) : Primary ? ModernTheme.Accent : Selected || hovered ? ModernTheme.AccentSoft : Color.White;
        Color ink = !Enabled ? ModernTheme.Muted : Primary ? Color.White : Selected ? ModernTheme.Accent : ModernTheme.Ink;
        if (pressed && Enabled) fill = Primary ? Color.FromArgb(6, 103, 115) : Color.FromArgb(212, 236, 238);
        if (SystemInformation.HighContrast) { fill = SystemColors.Control; ink = Enabled ? SystemColors.ControlText : SystemColors.GrayText; }
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using (var brush = new SolidBrush(fill)) e.Graphics.FillPath(brush, path);
        using (var pen = new Pen(Selected || Primary ? ModernTheme.Accent : ModernTheme.Border, scale)) e.Graphics.DrawPath(pen, path);
        TextRenderer.DrawText(e.Graphics, Text, Font, Rectangle.Inflate(ClientRectangle, -(int)(6 * scale), 0), ink,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        if (Focused && ShowFocusCues) ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(ClientRectangle, -(int)(4 * scale), -(int)(4 * scale)), ink, fill);
    }
}

internal sealed class MapBrandMark : Control
{
    internal MapBrandMark() { SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true); }
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        float scale = Math.Min(Width, Height) / 44f;
        e.Graphics.ScaleTransform(scale, scale);
        using var brush = new SolidBrush(ModernTheme.AccentSoft); e.Graphics.FillEllipse(brush, 0, 0, 43, 43);
        using var pen = new Pen(ModernTheme.Accent, 1.8f) { LineJoin = LineJoin.Round };
        e.Graphics.DrawPolygon(pen, new[] { new PointF(9, 14), new PointF(17, 10), new PointF(26, 14), new PointF(34, 10), new PointF(34, 30), new PointF(26, 34), new PointF(17, 30), new PointF(9, 34) });
        e.Graphics.DrawLine(pen, 17, 10, 17, 30); e.Graphics.DrawLine(pen, 26, 14, 26, 34);
    }
}
