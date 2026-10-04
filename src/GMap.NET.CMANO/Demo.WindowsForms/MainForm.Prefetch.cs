using System;
using System.Windows.Forms;
using GMap.NET;

namespace Demo.WindowsForms;

public partial class MainForm
{
    private NumericUpDown prefetchLastZoom;
    private Label prefetchRangeSummary;
    private bool updatingPrefetchRange, prefetchRangeEdited;

    private void AddPrefetchControls(TableLayoutPanel card)
    {
        ModernTheme.Add(card, ModernTheme.Label("预下载到缩放等级"), 24);
        prefetchLastZoom = new NumericUpDown { Name = "prefetchLastZoom", AccessibleName = "预下载最大缩放等级", Minimum = 0, Maximum = 24, Value = 24 };
        ModernTheme.Input(prefetchLastZoom); ModernTheme.Add(card, prefetchLastZoom, 34);
        prefetchRangeSummary = ModernTheme.Label(""); prefetchRangeSummary.Name = "prefetchRangeSummary";
        ModernTheme.Add(card, prefetchRangeSummary, 40);
        prefetchLastZoom.ValueChanged += (s, e) => { if (!updatingPrefetchRange) prefetchRangeEdited = true; UpdatePrefetchRange(); };
    }

    private void UpdatePrefetchRange()
    {
        if (prefetchLastZoom == null || updatingPrefetchRange) return;
        updatingPrefetchRange = true;
        try
        {
            int first = Math.Max(0, (int)MainMap.Zoom);
            int last = Math.Max(first, MainMap.MaxZoom);
            prefetchLastZoom.Maximum = last; prefetchLastZoom.Minimum = first;
            if (!prefetchRangeEdited) prefetchLastZoom.Value = last;
            prefetchRangeSummary.Text = "从当前 Z" + first + " 到 Z" + prefetchLastZoom.Value + " 连续下载，无需逐级确认。";
        }
        finally { updatingPrefetchRange = false; }
    }

    internal virtual AreaPrefetchDialog CreatePrefetchDialog(RectLatLng area, int first, int last) =>
        new AreaPrefetchDialog(area, first, last, MainMap.MapProvider, MainMap.Manager.Mode == AccessMode.CacheOnly);

    private void StartAreaPrefetch()
    {
        if (MainMap.SelectedArea.IsEmpty) { textBoxCacheStatus.Text = "请先在地图上框选预下载区域。"; return; }
        UpdatePrefetchRange();
        using var dialog = CreatePrefetchDialog(MainMap.SelectedArea, (int)MainMap.Zoom, (int)prefetchLastZoom.Value);
        dialog.ShowDialog(this);
        textBoxCacheStatus.Text = dialog.Summary;
    }
}
