using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Demo.WindowsForms;
using GMap.NET;

internal static class WorkspaceChecks
{
    internal static void Verify(MainForm form, string output)
    {
        Assert(Math.Abs(double.Parse(Text(form, "textBoxLat").Text, CultureInfo.InvariantCulture) - form.MainMap.Position.Lat) < 0.000001, "Rendering DMS controls must not overwrite decimal coordinates");
        Assert(!Button(form, "button13").Enabled, "Export waits for a selected area");
        Assert(MainForm.TryCreateSelection("37.50", "-122.30", "37.49", "-122.29", out var valid, out _), "Decimal bounds accept southern/western coordinates");
        foreach (string[] invalid in new[] { new[] { "NaN", "0", "1", "2" }, new[] { "91", "0", "1", "2" }, new[] { "1", "0", "2", "3" }, new[] { "2", "3", "1", "0" }, new[] { "Infinity", "0", "1", "2" } })
            Assert(!MainForm.TryCreateSelection(invalid[0], invalid[1], invalid[2], invalid[3], out _, out _), "Reject invalid or reversed bounds");
        Text(form, "areaNorth").Text = "37.50"; Text(form, "areaWest").Text = "-122.30"; Text(form, "areaSouth").Text = "37.49"; Text(form, "areaEast").Text = "-122.29";
        Button(form, "applyAreaButton").PerformClick();
        Assert(form.MainMap.SelectedArea == valid && Button(form, "button13").Enabled, "Applying coordinates enables actual map export");
        Text(form, "areaNorth").Text = "invalid"; Button(form, "applyAreaButton").PerformClick();
        Assert(form.MainMap.SelectedArea == valid, "Invalid input preserves the existing selection");
        Text(form, "areaNorth").Text = "37.50"; Button(form, "applyAreaButton").PerformClick();
        Button(form, "selectionModeButton").PerformClick(); Assert(form.MainMap.DisableAltForSelection && form.MainMap.DragButton == MouseButtons.Right, "Selection mode supports a left-button drag");
        var mouseFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var mapType = typeof(GMap.NET.WindowsForms.GMapControl);
        mapType.GetMethod("OnMouseDown", mouseFlags).Invoke(form.MainMap, new object[] { new MouseEventArgs(MouseButtons.Left, 1, 100, 100, 0) });
        mapType.GetMethod("OnMouseMove", mouseFlags).Invoke(form.MainMap, new object[] { new MouseEventArgs(MouseButtons.Left, 0, 240, 220, 0) });
        mapType.GetMethod("OnMouseUp", mouseFlags).Invoke(form.MainMap, new object[] { new MouseEventArgs(MouseButtons.Left, 1, 240, 220, 0) });
        Assert(!form.MainMap.SelectedArea.IsEmpty && Text(form, "areaNorth").Text == form.MainMap.SelectedArea.Top.ToString("F6", CultureInfo.InvariantCulture), "Mouse selection updates the bounds inputs");
        Text(form, "areaNorth").Text = "37.50"; Text(form, "areaWest").Text = "-122.30"; Text(form, "areaSouth").Text = "37.49"; Text(form, "areaEast").Text = "-122.29"; Button(form, "applyAreaButton").PerformClick();
        Button(form, "fitAreaButton").PerformClick(); Button(form, "copyMapCenter").PerformClick(); DpiChecks.Pump();
        DpiChecks.Capture(form, Path.Combine(output, "ui-selection-current-dpi.png"));
        Button(form, "panModeButton").PerformClick(); Assert(!form.MainMap.DisableAltForSelection && form.MainMap.DragButton == MouseButtons.Left, "Pan mode restores conventional left-button dragging");
        Button(form, "navigationMarkers").PerformClick(); DpiChecks.Pump();
        Assert(DpiChecks.Find(form, "markerPage").Visible && !DpiChecks.Find(form, "areaPage").Visible, "Marker navigation selects its own page");
        DpiChecks.VerifyVisibleControls(form); DpiChecks.Capture(DpiChecks.Find(form, "workspaceSidebar"), Path.Combine(output, "ui-markers-current-dpi.png"));
        Button(form, "navigationTools").PerformClick(); DpiChecks.Pump(); Assert(DpiChecks.Find(form, "toolsPage").Visible, "Cache and legacy tools remain accessible");
        DpiChecks.VerifyVisibleControls(form); DpiChecks.Capture(DpiChecks.Find(form, "workspaceSidebar"), Path.Combine(output, "ui-tools-current-dpi.png"));
        Button(form, "navigationArea").PerformClick(); Button(form, "useViewportButton").PerformClick();
        Assert(!form.MainMap.SelectedArea.IsEmpty && form.MainMap.SelectedArea.Contains(form.MainMap.Position), "Current viewport becomes an actual selected map area");
        Button(form, "clearAreaButton").PerformClick(); Assert(form.MainMap.SelectedArea.IsEmpty && !Button(form, "button13").Enabled, "Clear resets the selected area and export action");
        Console.WriteLine("PASS workspace navigation, coordinate validation, pan/selection modes, viewport selection and clear");
    }

    internal static void VerifyExport(StaticImage form, string output)
    {
        var zoom = (NumericUpDown)DpiChecks.Find(form, "numericUpDown1");
        Assert(zoom.Value == 14, "Export starts at the current map zoom");
        Assert(DpiChecks.Find(form, "exportResolutionPreview").Text.Contains("513") && DpiChecks.Find(form, "exportResolutionPreview").Text.Contains("387"), "Pixel preview matches the actual native export dimensions");
        var path = Text(form, "textBoxSaveAs"); var size = Text(form, "textBoxSizeFile"); var start = Button(form, "button1");
        path.Text = "relative-file.png"; Assert(!start.Enabled, "Invalid output paths cannot start a worker");
        path.Text = Path.Combine(output, "layer.png"); size.Text = "0"; Assert(!start.Enabled, "Zero size limit is rejected");
        size.Text = "99"; Assert(start.Enabled, "Valid settings enable export");
        DpiChecks.Capture(form, Path.Combine(output, "ui-export-current-dpi.png"));
        Console.WriteLine("PASS export preview matches 513x387 pixels and validates path/size before starting");
    }
    internal static void VerifyAsyncExport(StaticImage export, MainForm main, string output)
    {
        string path = Path.Combine(output, "interactive-layer.png");
        Text(export, "textBoxSaveAs").Text = path;
        bool started = false, completed = false;
        DateTime deadline = DateTime.UtcNow.AddSeconds(10);
        using var context = new ApplicationContext();
        using var timer = new Timer { Interval = 25 };
        timer.Tick += (s, e) =>
        {
            if (!started)
            {
                // Run the real button inside a real WinForms message loop, so worker callbacks return to the UI thread.
                Button(export, "button1").PerformClick();
                Assert(!DpiChecks.Find(export, "exportSettings").Enabled && main.MainMap.HoldInvalidation, "Running export locks its settings");
                Text(export, "textBoxSaveAs").Text = Path.Combine(output, "changed-after-start.png");
                started = true;
            }
            else if (DpiChecks.Find(export, "exportSettings").Enabled && DpiChecks.Find(export, "exportStatus").Text.StartsWith("导出完成"))
            { completed = true; context.ExitThread(); }
            if (DateTime.UtcNow > deadline) context.ExitThread();
        };
        timer.Start(); Application.Run(context); timer.Stop();
        Assert(completed && !main.MainMap.HoldInvalidation, "Completed export releases settings and map refresh");
        Assert(File.Exists(path) && File.Exists(Path.ChangeExtension(path, ".pgw")) && !File.Exists(Path.Combine(output, "changed-after-start.png")), "Worker uses the captured settings and produces PNG/PGW");
        using var bitmap = new Bitmap(path); Assert(bitmap.Size == new Size(513, 387), "Actual button-driven export retains source pixel dimensions");
        Console.WriteLine("PASS actual export button, UI-thread progress, captured settings and completion state");
    }
    internal static void VerifyCloseCancellation(MainForm main, string output)
    {
        using var export = new DpiChecks.OffscreenExportForm(main);
        export.StartPosition = FormStartPosition.Manual; export.Location = new Point(-20000, -20000); export.Show();
        Text(export, "textBoxSaveAs").Text = Path.Combine(output, "cancelled-layer.png");
        int phase = 0; bool closed = false;
        export.FormClosed += (s, e) => closed = true;
        DateTime deadline = DateTime.UtcNow.AddSeconds(10);
        using var context = new ApplicationContext(); using var timer = new Timer { Interval = 25 };
        timer.Tick += (s, e) =>
        {
            if (phase == 0) { Button(export, "button1").PerformClick(); phase = 1; }
            else if (phase == 1)
            {
                Assert(!DpiChecks.Find(export, "exportSettings").Enabled, "Delayed tile source keeps the export active");
                export.Close(); Assert(!closed && !export.IsDisposed, "Close waits for cancellation before disposing the worker UI"); phase = 2;
            }
            else if (closed) context.ExitThread();
            if (DateTime.UtcNow > deadline) context.ExitThread();
        };
        timer.Start(); Application.Run(context); timer.Stop();
        Assert(closed && !main.MainMap.HoldInvalidation, "Cancelled export closes cleanly and releases map refresh");
        Console.WriteLine("PASS close during export cancels before disposing the dialog");
    }
    private static TextBox Text(Control parent, string name) => (TextBox)DpiChecks.Find(parent, name);
    private static Button Button(Control parent, string name) => (Button)DpiChecks.Find(parent, name);
    private static void Assert(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
