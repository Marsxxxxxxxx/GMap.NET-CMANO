using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.MapProviders;

internal static class PrefetchChecks
{
    internal static void Verify(GMapProvider provider, string output, Action<int> delay)
    {
        using var main = new DpiChecks.OffscreenMainForm(provider, Path.Combine(output, "prefetch cache"), Path.Combine(output, "prefetch settings", "network.xml"));
        main.StartPosition = FormStartPosition.Manual; main.Location = new System.Drawing.Point(-20000, -20000);
        main.ShowInTaskbar = false;
        using var context = new ApplicationContext(main);
        using var timer = new Timer { Interval = 25 };
        Exception failure = null;
        timer.Tick += (s, e) =>
        {
            timer.Stop();
            try { VerifyInsideMessageLoop(main, output, delay); }
            catch (Exception error) { failure = error; }
            finally { delay(0); context.ExitThread(); }
        };
        timer.Start(); Application.Run(context);
        if (failure != null) throw failure;
        Console.WriteLine("PASS actual pre-download button: Z5-Z7 without confirmations, SQLite writes, cached-tile reuse, cancellation and offline mode");
    }

    private static void VerifyInsideMessageLoop(DpiChecks.OffscreenMainForm main, string output, Action<int> delay)
    {
        var originalMode = main.MainMap.Manager.Mode;
        Assert(!Button(main, "button11").Enabled, "Pre-download requires an actual selected area");
        main.MainMap.MaxZoom = 7; main.MainMap.Zoom = 5;
        var area = RectLatLng.FromLTRB(-122.30, 37.50, -122.298, 37.498);
        SelectArea(main, area);
        ((NumericUpDown)DpiChecks.Find(main, "prefetchLastZoom")).Value = 7;
        long expected = Enumerable.Range(5, 3).Sum(z => (long)main.MainMap.MapProvider.Projection.GetAreaTileList(area, z, 0).Count);
        var first = RunButton(main);
        Assert(first.CompletedZooms.SequenceEqual(new[] { 5, 6, 7 }) && first.Downloaded == expected &&
            first.Missing == 0 && !first.WasCancelled, "Actual button automatically finishes all three zoom levels");
        foreach (int zoom in first.CompletedZooms)
        foreach (var tile in main.MainMap.MapProvider.Projection.GetAreaTileList(area, zoom, 0))
        {
            using var cached = GMaps.Instance.PrimaryCache.GetImageFromCache(main.MainMap.MapProvider.DbId, tile, zoom);
            Assert(cached != null, "Every completed level is actually stored in the SQLite cache");
        }
        var repeated = RunButton(main);
        Assert(repeated.Downloaded == 0 && repeated.AlreadyCached == expected && repeated.CompletedZooms.Count == 3,
            "Repeating pre-download skips existing cached tiles");
        Assert(main.MainMap.SelectedArea == area && main.MainMap.Manager.Mode == originalMode,
            "Pre-download preserves map selection and access mode");

        SelectArea(main, RectLatLng.FromLTRB(-70, 40, -69.998, 39.998));
        delay(200);
        bool cancelledInFlight = false;
        var cancelled = RunButton(main, dialog =>
        {
            if (cancelledInFlight) return;
            cancelledInFlight = true;
            DpiChecks.VerifyVisibleControls(dialog);
            DpiChecks.Capture(dialog, Path.Combine(output, "ui-prefetch-current-dpi.png"));
            Button(dialog, "prefetchCancel").PerformClick();
            Assert(dialog.IsBusy && !dialog.IsDisposed, "Cancel waits for the active worker before closing its UI");
        });
        delay(0);
        Assert(cancelledInFlight && cancelled.WasCancelled && cancelled.CompletedZooms.Count == 0 &&
            DpiChecks.Find(main, "textBoxCacheStatus").Text.Contains("取消"), "Cancel stops the whole range before another zoom starts");

        SelectArea(main, RectLatLng.FromLTRB(100, 10, 100.002, 9.998));
        main.MainMap.Manager.Mode = AccessMode.CacheOnly;
        var offline = RunButton(main);
        Assert(offline.Downloaded == 0 && offline.Missing > 0 && offline.CompletedZooms.Count == 3 &&
            main.MainMap.Manager.Mode == AccessMode.CacheOnly, "Cache-only mode does not download missing tiles");
        main.MainMap.Manager.Mode = originalMode;
    }

    private static void SelectArea(DpiChecks.OffscreenMainForm main, RectLatLng area)
    {
        Button(main, "navigationArea").PerformClick();
        DpiChecks.Find(main, "areaNorth").Text = area.Top.ToString("F6", CultureInfo.InvariantCulture);
        DpiChecks.Find(main, "areaWest").Text = area.Left.ToString("F6", CultureInfo.InvariantCulture);
        DpiChecks.Find(main, "areaSouth").Text = area.Bottom.ToString("F6", CultureInfo.InvariantCulture);
        DpiChecks.Find(main, "areaEast").Text = area.Right.ToString("F6", CultureInfo.InvariantCulture);
        Button(main, "applyAreaButton").PerformClick();
        Button(main, "navigationTools").PerformClick();
        Assert(Button(main, "button11").Enabled, "Selecting bounds enables the real pre-download button");
    }

    private static Demo.WindowsForms.AreaPrefetchDialog RunButton(DpiChecks.OffscreenMainForm main, Action<Demo.WindowsForms.AreaPrefetchDialog> running = null)
    {
        Exception failure = null;
        DateTime deadline = DateTime.UtcNow.AddSeconds(8);
        using var timer = new Timer { Interval = 25 };
        timer.Tick += (s, e) =>
        {
            try
            {
                if (main.LastPrefetch?.IsBusy == true) running?.Invoke(main.LastPrefetch);
                if (DateTime.UtcNow > deadline) throw new TimeoutException("Pre-download must proceed without confirmation dialogs.");
            }
            catch (Exception error) { failure = error; timer.Stop(); main.LastPrefetch?.Close(); }
        };
        timer.Start(); Button(main, "button11").PerformClick(); timer.Stop();
        if (failure != null) throw failure;
        Assert(main.LastPrefetch != null && !main.LastPrefetch.IsBusy, "The real pre-download task completes before returning");
        return main.LastPrefetch;
    }

    private static Button Button(Control parent, string name) => (Button)DpiChecks.Find(parent, name);
    private static void Assert(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
