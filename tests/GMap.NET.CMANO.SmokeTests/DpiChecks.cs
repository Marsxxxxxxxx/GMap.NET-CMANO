using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Demo.WindowsForms;

internal static class DpiChecks
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetThreadDpiAwarenessContext();

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowDpiAwarenessContext(IntPtr window);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AreDpiAwarenessContextsEqual(IntPtr first, IntPtr second);

    internal static void Verify(MainForm form, string output)
    {
        if (Environment.OSVersion.Version < new Version(10, 0, 15063))
        {
            Console.WriteLine("SKIP PerMonitorV2 check: Windows 10 1703 or later is required");
            return;
        }

        var perMonitorV2 = new IntPtr(-4);
        Assert(AreDpiAwarenessContextsEqual(GetThreadDpiAwarenessContext(), perMonitorV2),
            "The shared application configuration must enable PerMonitorV2");
        Assert(AreDpiAwarenessContextsEqual(GetWindowDpiAwarenessContext(form.Handle), perMonitorV2),
            "The main window must render without Windows bitmap scaling");
        Console.WriteLine("PASS PerMonitorV2 window; display DPI: " + form.DeviceDpi);
        form.WindowState = FormWindowState.Normal;
        form.StartPosition = FormStartPosition.Manual;
        form.Location = new Point(-20000, -20000);
        form.ShowInTaskbar = false;
        form.ClientSize = new Size(form.DeviceDpi * 1000 / 96, form.DeviceDpi * 1000 / 96);
        form.Show();
        form.PerformLayout();
        form.Refresh();

        Console.WriteLine("Main form client size: " + form.ClientSize);
        foreach (string name in new[] { "panelMenu", "xPanderPanelMain", "xPanderPanelCache" })
        {
            var panel = (BSE.Windows.Forms.BasePanel)form.Controls.Find(name, true)[0];
            int logicalCaption = name == "panelMenu" ? 27 : 25;
            Assert(Math.Abs(panel.CaptionHeight - logicalCaption * form.DeviceDpi / 96.0) <= 1,
                name + " caption must follow the display DPI");
            Console.WriteLine(name + ": " + panel.Bounds + "; caption=" + panel.CaptionHeight + "; DPI=" + panel.DeviceDpi);
        }
        VerifyCaptionScaling();
        using (var bitmap = new Bitmap(form.Width, form.Height))
        {
            bitmap.SetResolution(form.DeviceDpi, form.DeviceDpi);
            form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
            bitmap.Save(Path.Combine(output, "ui-current-dpi.png"), ImageFormat.Png);
        }
        var menu = form.Controls.Find("panelMenu", true)[0];
        using (var bitmap = new Bitmap(menu.Width, menu.Height))
        {
            bitmap.SetResolution(form.DeviceDpi, form.DeviceDpi);
            menu.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
            bitmap.Save(Path.Combine(output, "ui-menu-current-dpi.png"), ImageFormat.Png);
        }
    }

    internal static void VerifyExportWindow(StaticImage form, string output)
    {
        form.StartPosition = FormStartPosition.Manual;
        form.Location = new Point(-20000, -20000);
        if (Environment.OSVersion.Version >= new Version(10, 0, 15063))
        {
            Assert(AreDpiAwarenessContextsEqual(GetWindowDpiAwarenessContext(form.Handle), new IntPtr(-4)),
                "The export dialog must also use PerMonitorV2");
        }
        form.PerformLayout();
        form.ShowInTaskbar = false;
        form.Show();
        form.Refresh();
        Assert(Math.Abs(form.ClientSize.Width - 457 * form.DeviceDpi / 96.0) <= 1,
            "Export dialog width must scale with DPI");
        foreach (Control control in form.Controls)
        {
            Assert(form.ClientRectangle.Contains(control.Bounds),
                "Export control must remain inside the dialog: " + control.Name);
        }
        using var bitmap = new Bitmap(form.Width, form.Height);
        bitmap.SetResolution(form.DeviceDpi, form.DeviceDpi);
        form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
        bitmap.Save(Path.Combine(output, "ui-export-current-dpi.png"), ImageFormat.Png);
        Console.WriteLine("PASS export dialog DPI/layout: " + form.DeviceDpi);
    }

    private static void VerifyCaptionScaling()
    {
        using var list = new BSE.Windows.Forms.XPanderPanelList();
        using var panel = new BSE.Windows.Forms.XPanderPanel();
        list.Controls.Add(panel);
        float[] factors = { 1.5f, 4f / 3f, 0.5f };
        int[] heights = { 38, 50, 25 };
        int[] iconSizes = { 24, 32, 16 };
        for (int step = 0; step < factors.Length; step++)
        {
            list.Scale(new SizeF(factors[step], factors[step]));
            Assert(list.CaptionHeight == heights[step] && panel.CaptionHeight == heights[step],
                "Container and child captions must scale exactly once");
            Assert(panel.ImageSize == new Size(iconSizes[step], iconSizes[step]),
                "Caption icon bounds must follow the scale factor");
        }
        Assert(panel.CaptionHeight == 25, "Caption height must return to 100% without rounding drift");
        Console.WriteLine("PASS caption scaling: 100% -> 150% -> 200% -> 100%");
    }

    internal sealed class OffscreenExportForm : StaticImage
    {
        internal OffscreenExportForm(MainForm main) : base(main) { }
        protected override bool ShowWithoutActivation => true;
    }

    internal sealed class OffscreenMainForm : MainForm
    {
        internal OffscreenMainForm(GMap.NET.MapProviders.GMapProvider provider, string cache)
            : base(provider, cache) { }
        protected override bool ShowWithoutActivation => true;
        // Avoid the legacy Load handler activating the user's desktop during tests.
        protected override void OnLoad(EventArgs e) { }
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
