using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Demo.WindowsForms;

internal static class DpiChecks
{
    [DllImport("user32.dll")] private static extern IntPtr GetThreadDpiAwarenessContext();
    [DllImport("user32.dll")] private static extern IntPtr GetWindowDpiAwarenessContext(IntPtr window);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool AreDpiAwarenessContextsEqual(IntPtr first, IntPtr second);

    internal static void Verify(MainForm form, string output)
    {
        if (Environment.OSVersion.Version >= new Version(10, 0, 15063))
        {
            var perMonitorV2 = new IntPtr(-4);
            Assert(AreDpiAwarenessContextsEqual(GetThreadDpiAwarenessContext(), perMonitorV2), "The application configuration must enable PerMonitorV2");
            Assert(AreDpiAwarenessContextsEqual(GetWindowDpiAwarenessContext(form.Handle), perMonitorV2), "The main window must render without Windows bitmap scaling");
        }
        form.WindowState = FormWindowState.Normal; form.StartPosition = FormStartPosition.Manual;
        form.Location = new Point(-20000, -20000); form.ShowInTaskbar = false;
        form.ClientSize = LogicalSize(form, 1440, 1000); form.Show(); form.PerformLayout(); Pump(); form.Refresh();
        var sidebar = Find(form, "workspaceSidebar"); var map = Find(form, "MainMap");
        Assert(Math.Abs(sidebar.Width - 360 * form.DeviceDpi / 96.0) <= 1, "Sidebar must scale exactly once at the display DPI");
        Assert(map.Width > 600 * form.DeviceDpi / 96.0 && map.Height > 500 * form.DeviceDpi / 96.0, "Map must occupy the available workspace");
        VerifyVisibleControls(form);
        Capture(form, Path.Combine(output, "ui-current-dpi.png")); Capture(sidebar, Path.Combine(output, "ui-menu-current-dpi.png"));
        form.ClientSize = LogicalSize(form, 1040, 720); form.PerformLayout(); Pump();
        Assert(map.Width > 500 * form.DeviceDpi / 96.0 && map.Height > 400 * form.DeviceDpi / 96.0, "Minimum window size must retain a usable map");
        VerifyVisibleControls(form); Capture(form, Path.Combine(output, "ui-compact-current-dpi.png"));
        form.ClientSize = LogicalSize(form, 1440, 1000); form.PerformLayout();
        VerifyCaptionScaling();
        Console.WriteLine("PASS native workspace at " + form.DeviceDpi + " DPI, normal/compact layouts and original caption scaling");
    }

    internal static void VerifyExportWindow(StaticImage form, string output)
    {
        form.StartPosition = FormStartPosition.Manual; form.Location = new Point(-20000, -20000); form.ShowInTaskbar = false;
        if (Environment.OSVersion.Version >= new Version(10, 0, 15063))
            Assert(AreDpiAwarenessContextsEqual(GetWindowDpiAwarenessContext(form.Handle), new IntPtr(-4)), "The export dialog must use PerMonitorV2");
        form.Show(); form.PerformLayout(); Pump(); form.Refresh();
        Assert(Math.Abs(form.ClientSize.Width - 660 * form.DeviceDpi / 96.0) <= 1, "Export dialog width must scale exactly once");
        VerifyVisibleControls(form); Capture(form, Path.Combine(output, "ui-export-current-dpi.png"));
        Console.WriteLine("PASS native export dialog DPI/layout: " + form.DeviceDpi);
    }

    private static Size LogicalSize(Control control, int width, int height) => new Size(width * control.DeviceDpi / 96, height * control.DeviceDpi / 96);
    internal static Control Find(Control parent, string name)
    {
        var result = parent.Controls.Find(name, true); Assert(result.Length == 1, "Expected one control: " + name); return result[0];
    }
    internal static void VerifyVisibleControls(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            if (!child.Visible) continue;
            if (child is TextBox || child is ComboBox || child is NumericUpDown)
            {
                Assert(child.Width >= 35 * child.DeviceDpi / 96.0, "Input width is usable: " + child.Name);
                Assert(child.Height >= child.Font.Height, "Input text is not vertically clipped: " + child.Name);
                Assert(child.Parent.ClientRectangle.Contains(child.Bounds), "Input remains in its layout row: " + child.Name);
            }
            if (child is Button) Assert(child.Parent.ClientRectangle.Contains(child.Bounds), "Button remains in its layout row: " + child.Name + " " + child.Bounds + " parent=" + child.Parent.ClientRectangle);
            VerifyVisibleControls(child);
        }
    }
    internal static void Capture(Control control, string path)
    {
        using var bitmap = new Bitmap(control.Width, control.Height);
        bitmap.SetResolution(control.DeviceDpi, control.DeviceDpi); control.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
        bitmap.Save(path, ImageFormat.Png);
    }
    internal static void Pump() { for (int i = 0; i < 10; i++) { Application.DoEvents(); System.Threading.Thread.Sleep(10); } }
    private static void VerifyCaptionScaling()
    {
        using var list = new BSE.Windows.Forms.XPanderPanelList(); using var panel = new BSE.Windows.Forms.XPanderPanel(); list.Controls.Add(panel);
        float[] factors = { 1.5f, 4f / 3f, 0.5f }; int[] heights = { 38, 50, 25 }; int[] iconSizes = { 24, 32, 16 };
        for (int i = 0; i < factors.Length; i++)
        {
            list.Scale(new SizeF(factors[i], factors[i])); Assert(list.CaptionHeight == heights[i] && panel.CaptionHeight == heights[i], "Legacy captions scale once");
            Assert(panel.ImageSize == new Size(iconSizes[i], iconSizes[i]), "Legacy caption icons scale once");
        }
    }
    internal sealed class OffscreenExportForm : StaticImage
    {
        internal OffscreenExportForm(MainForm main) : base(main) { }
        protected override bool ShowWithoutActivation => true;
    }
    internal sealed class OffscreenMainForm : MainForm
    {
        internal OffscreenMainForm(GMap.NET.MapProviders.GMapProvider provider, string cache, string networkPath = null) : base(provider, cache, networkPath) { }
        internal AreaPrefetchDialog LastPrefetch;
        internal override AreaPrefetchDialog CreatePrefetchDialog(GMap.NET.RectLatLng area, int first, int last) =>
            LastPrefetch = new OffscreenPrefetchDialog(area, first, last, MainMap.MapProvider, MainMap.Manager.Mode == GMap.NET.AccessMode.CacheOnly);
        protected override bool ShowWithoutActivation => true;
        protected override void OnLoad(EventArgs e) { }
    }
    private sealed class OffscreenPrefetchDialog : AreaPrefetchDialog
    {
        internal OffscreenPrefetchDialog(GMap.NET.RectLatLng area, int first, int last, GMap.NET.MapProviders.GMapProvider provider, bool cacheOnly)
            : base(area, first, last, provider, cacheOnly, 0)
        {
            StartPosition = FormStartPosition.Manual; Location = new Point(-20000, -20000);
        }
        protected override bool ShowWithoutActivation => true;
    }

    private static void Assert(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
