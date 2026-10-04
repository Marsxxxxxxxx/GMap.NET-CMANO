using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.MapProviders;

namespace Demo.WindowsForms;

// One task spans the entire zoom range; closing waits for the current request to finish.
internal class AreaPrefetchDialog : Form
{
    private readonly RectLatLng area;
    private readonly int firstZoom, lastZoom, requestDelay;
    private readonly GMapProvider provider;
    private readonly bool cacheOnly;
    private readonly PureImageCache cache;
    private readonly BackgroundWorker worker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
    private readonly Label status;
    private readonly ProgressBar progress;
    private readonly Button cancel;
    internal readonly List<int> CompletedZooms = new List<int>();
    internal long Downloaded { get; private set; }
    internal long AlreadyCached { get; private set; }
    internal long Missing { get; private set; }
    internal bool WasCancelled { get; private set; }
    internal string Summary { get; private set; }
    internal bool IsBusy => worker.IsBusy;

    internal AreaPrefetchDialog(RectLatLng selectedArea, int startZoom, int endZoom, GMapProvider mapProvider, bool useCacheOnly, int delay = 100)
    {
        area = selectedArea; firstZoom = startZoom; lastZoom = endZoom; provider = mapProvider;
        cacheOnly = useCacheOnly; requestDelay = delay;
        cache = GMaps.Instance.PrimaryCache ?? throw new InvalidOperationException("未启用磁盘瓦片缓存。");
        SuspendLayout();
        AutoScaleDimensions = new SizeF(96, 96); AutoScaleMode = AutoScaleMode.Dpi;
        Font = ModernTheme.Body; ForeColor = ModernTheme.Ink; BackColor = ModernTheme.Background;
        Text = "预下载选区地图 — GMap.NET CMANO"; ClientSize = new Size(620, 260); MinimumSize = new Size(520, 260);
        StartPosition = FormStartPosition.CenterParent; ShowInTaskbar = false; MinimizeBox = MaximizeBox = false;
        var root = ModernTheme.Stack(); root.Dock = DockStyle.Top; root.Padding = new Padding(24, 16, 24, 16); root.BackColor = ModernTheme.Background;
        Controls.Add(root);
        var title = ModernTheme.Label("预下载选区地图", true); title.Font = ModernTheme.Heading; ModernTheme.Add(root, title, 36);
        ModernTheme.Add(root, ModernTheme.Label(provider.Name + "  ·  Z" + firstZoom + " → Z" + lastZoom), 28);
        ModernTheme.Add(root, ModernTheme.Label(cacheOnly ? "仅本地缓存模式：检查已有瓦片，不连接服务器。" : "各等级连续下载，已有缓存自动跳过；下载后直接写入磁盘缓存。"), 38);
        status = ModernTheme.Label("准备开始…", color: ModernTheme.Accent); status.Name = "prefetchStatus";
        ModernTheme.Add(root, status, 42);
        progress = new ProgressBar { Name = "prefetchProgress", Margin = new Padding(0, 4, 0, 12) }; ModernTheme.Add(root, progress, 28);
        var actions = new TableLayoutPanel { ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); actions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 136));
        actions.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        cancel = ModernTheme.Button("取消下载", "prefetchCancel"); cancel.Dock = DockStyle.Fill; cancel.Margin = Padding.Empty;
        cancel.Click += (s, e) => RequestCancellation(); actions.Controls.Add(cancel, 1, 0); ModernTheme.Add(root, actions, 40);
        CancelButton = cancel; cancel.DialogResult = DialogResult.None;
        worker.DoWork += DownloadTiles;
        worker.ProgressChanged += (s, e) =>
        {
            if (!worker.IsBusy || IsDisposed) return;
            if (!worker.CancellationPending) { status.Text = (string)e.UserState; progress.Value = e.ProgressPercentage; }
        };
        worker.RunWorkerCompleted += (s, e) =>
        {
            WasCancelled = e.Cancelled;
            Summary = e.Error != null ? "预下载失败：" + e.Error.Message :
                (WasCancelled ? "预下载已取消" : "预下载完成") + " · 新下载 " + Downloaded + "，已有缓存 " + AlreadyCached + "，未获取 " + Missing;
            Close();
        };
        FormClosing += (s, e) => { if (worker.IsBusy) { e.Cancel = true; RequestCancellation(); } };
        FormClosed += (s, e) => worker.Dispose();
        Shown += (s, e) => worker.RunWorkerAsync();
        ResumeLayout(true);
    }

    private void RequestCancellation()
    {
        if (!worker.IsBusy) { Close(); return; }
        worker.CancelAsync(); cancel.Enabled = false; status.Text = "正在停止，等待当前请求结束…";
    }

    private void DownloadTiles(object sender, DoWorkEventArgs e)
    {
        for (int zoom = firstZoom; zoom <= lastZoom; zoom++)
        {
            if (worker.CancellationPending) { e.Cancel = true; return; }
            var tiles = provider.Projection.GetAreaTileList(area, zoom, 0);
            for (int i = 0; i < tiles.Count; i++)
            {
                if (worker.CancellationPending) { e.Cancel = true; return; }
                worker.ReportProgress((int)(i * 100L / tiles.Count),
                    "Z" + zoom + "（" + (zoom - firstZoom + 1) + "/" + (lastZoom - firstZoom + 1) + "） · 瓦片 " + (i + 1) + "/" + tiles.Count);
                foreach (var overlay in provider.Overlays)
                {
                    if (worker.CancellationPending) { e.Cancel = true; return; }
                    var position = tiles[i];
                    if (overlay.InvertedAxisY)
                        position = new GPoint(position.X, overlay.Projection.GetTileMatrixMaxXY(zoom).Height - position.Y);
                    CacheTile(overlay, position, zoom);
                }
                worker.ReportProgress((int)((i + 1) * 100L / tiles.Count),
                    "Z" + zoom + "（" + (zoom - firstZoom + 1) + "/" + (lastZoom - firstZoom + 1) + "） · 瓦片 " + (i + 1) + "/" + tiles.Count);
            }
            if (worker.CancellationPending) { e.Cancel = true; return; }
            CompletedZooms.Add(zoom);
        }
    }

    private void CacheTile(GMapProvider source, GPoint position, int zoom)
    {
        using (var existing = cache.GetImageFromCache(source.DbId, position, zoom))
        {
            if (existing != null) { AlreadyCached++; return; }
        }
        if (cacheOnly) { Missing++; return; }
        for (int attempt = 0; attempt < 2; attempt++)
        {
            if (worker.CancellationPending) return;
            PureImage tile = null;
            try { tile = source.GetTileImage(position, zoom); }
            catch (Exception e) when (e is System.Net.WebException || e is IOException) { }
            using (tile)
            {
                if (tile != null)
                {
                    if (tile.Data == null || !cache.PutImageToCache(tile.Data.ToArray(), source.DbId, position, zoom))
                        throw new IOException("无法写入地图缓存，请检查 cache 目录和磁盘空间。");
                    Downloaded++;
                    if (requestDelay > 0 && !worker.CancellationPending) Thread.Sleep(requestDelay);
                    return;
                }
            }
            if (attempt == 0 && !worker.CancellationPending) Thread.Sleep(250);
        }
        if (!worker.CancellationPending) Missing++;
    }
}
