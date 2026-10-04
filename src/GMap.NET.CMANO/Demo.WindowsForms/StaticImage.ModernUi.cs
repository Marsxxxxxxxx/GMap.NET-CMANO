using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.MapProviders;

namespace Demo.WindowsForms;

public partial class StaticImage
{
    private Label exportResolutionPreview;
    private Label exportStatus;
    private Panel exportSettings;
    private bool closeAfterExport;

    private void InitializeModernExport()
    {
        button1 = RebuildExportButton(button1, button1_Click);
        button2 = RebuildExportButton(button2, button2_Click);
        btnSave = RebuildExportButton(btnSave, btnSave_Click);
        Controls.Clear();
        Font = ModernTheme.Body; ForeColor = ModernTheme.Ink; BackColor = ModernTheme.Background;
        Padding = Padding.Empty; ClientSize = new Size(660, 650); MinimumSize = new Size(620, 620);
        FormBorderStyle = FormBorderStyle.Sizable; MaximizeBox = false; MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent; ShowInTaskbar = false; Text = "导出地图叠加层 — GMap.NET CMANO";
        var root = new TableLayoutPanel { Name = "exportRoot", Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Margin = Padding.Empty };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 80)); root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 108));
        Controls.Add(root);
        var header = ModernTheme.Stack(); header.Padding = new Padding(24, 12, 24, 8);
        var title = ModernTheme.Label("导出地图叠加层", true); title.Font = ModernTheme.Heading;
        ModernTheme.Add(header, title, 31); ModernTheme.Add(header, ModernTheme.Label("设置原始像素、文件大小与输出位置"), 25); root.Controls.Add(header, 0, 0);
        exportSettings = new Panel { Name = "exportSettings", AutoScroll = true, Dock = DockStyle.Fill, Margin = Padding.Empty, Padding = new Padding(24, 8, 24, 12) };
        var stack = ModernTheme.Stack(); stack.BackColor = ModernTheme.Background; exportSettings.Controls.Add(stack); root.Controls.Add(exportSettings, 0, 1);
        var quality = ModernTheme.Card(stack, "输出画质");
        ModernTheme.Add(quality, ModernTheme.Columns(ModernTheme.Label("地图缩放级别"), ModernTheme.Label("缩小版大小上限（MB）")), 24);
        numericUpDown1.AccessibleName = "导出缩放级别"; textBoxSizeFile.AccessibleName = "文件大小上限（MB）"; textBoxSaveAs.AccessibleName = "PNG 输出路径";
        numericUpDown1.Font = ModernTheme.Body; numericUpDown1.Margin = new Padding(0, 2, 0, 4); numericUpDown1.TextAlign = HorizontalAlignment.Left;
        ModernTheme.Input(textBoxSizeFile); textBoxSizeFile.MaxLength = 9; textBoxSizeFile.TextAlign = HorizontalAlignment.Left;
        ModernTheme.Add(quality, ModernTheme.Columns(numericUpDown1, textBoxSizeFile), 34);
        exportResolutionPreview = ModernTheme.Label("", true, ModernTheme.Accent); exportResolutionPreview.Name = "exportResolutionPreview";
        ModernTheme.Add(quality, exportResolutionPreview, 52);
        ModernTheme.Add(quality, ModernTheme.Label("缩放级别越高，原始像素越多。超过大小上限时会缩图，同时保留带时间戳的原始 PNG。"), 42);
        var output = ModernTheme.Card(stack, "输出文件");
        ModernTheme.Add(output, ModernTheme.Label("PNG 保存位置"), 23);
        ModernTheme.Input(textBoxSaveAs); textBoxSaveAs.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "layer.png");
        ModernTheme.Style(btnSave, "浏览…");
        var pathRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
        pathRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        pathRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); pathRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88));
        textBoxSaveAs.Dock = btnSave.Dock = DockStyle.Fill; btnSave.Margin = new Padding(8, 0, 0, 3);
        pathRow.Controls.Add(textBoxSaveAs, 0, 0); pathRow.Controls.Add(btnSave, 1, 0); ModernTheme.Add(output, pathRow, 34);
        ModernTheme.Check(checkBoxWorldFile, "生成 PGW 定位文件（CMANO 叠加层）"); ModernTheme.Add(output, checkBoxWorldFile, 30);
        ModernTheme.Check(checkBoxKMZ, "同时生成 KMZ 地图包"); ModernTheme.Add(output, checkBoxKMZ, 28);
        ModernTheme.Check(checkBoxRoutes, "使用地图中全部路线的范围"); ModernTheme.Add(output, checkBoxRoutes, 28);
        var footer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = Color.White,
            Margin = Padding.Empty, Padding = new Padding(24, 8, 24, 12) };
        footer.RowStyles.Add(new RowStyle(SizeType.Absolute, 24)); footer.RowStyles.Add(new RowStyle(SizeType.Absolute, 8)); footer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        exportStatus = ModernTheme.Label("准备导出"); exportStatus.Name = "exportStatus"; footer.Controls.Add(exportStatus, 0, 0);
        progressBar1.Dock = DockStyle.Fill; progressBar1.Margin = Padding.Empty; footer.Controls.Add(progressBar1, 0, 1);
        ModernTheme.Style(button1, "开始导出", true); ModernTheme.Style(button2, "关闭");
        var actions = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Margin = new Padding(0, 8, 0, 0) };
        actions.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); actions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96)); actions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 144));
        button2.Dock = button1.Dock = DockStyle.Fill; button2.Margin = new Padding(0, 0, 8, 0); button1.Margin = Padding.Empty;
        actions.Controls.Add(button2, 1, 0); actions.Controls.Add(button1, 2, 0); footer.Controls.Add(actions, 0, 2); root.Controls.Add(footer, 0, 2);
        AcceptButton = button1; CancelButton = button2;
        saveFileDialog.Filter = "PNG 地图 (*.png)|*.png"; saveFileDialog.DefaultExt = "png"; saveFileDialog.AddExtension = true; saveFileDialog.FileName = "layer.png";
        numericUpDown1.ValueChanged += (s, e) => UpdateExportPreview();
        checkBoxRoutes.CheckedChanged += (s, e) => UpdateExportPreview(); checkBoxWorldFile.CheckedChanged += (s, e) => UpdateExportPreview(); checkBoxKMZ.CheckedChanged += (s, e) => UpdateExportPreview();
        textBoxSizeFile.TextChanged += (s, e) => UpdateExportPreview(); textBoxSaveAs.TextChanged += (s, e) => UpdateExportPreview();
        FormClosing += (s, e) => { if (bg.IsBusy) { e.Cancel = true; closeAfterExport = true; bg.CancelAsync(); exportStatus.Text = "正在取消导出，请稍候…"; } };
        Main.MainMap.OnSelectionChange += ExportSelectionChanged;
        Main.MainMap.OnMapTypeChanged += ExportProviderChanged;
        UpdateExportPreview();
    }

    private static Button RebuildExportButton(Button original, EventHandler handler)
    {
        var button = ModernTheme.Button(original.Text, original.Name); button.TabIndex = original.TabIndex; button.Click += handler;
        original.Dispose(); return button;
    }
    private RectLatLng ExportArea => checkBoxRoutes.Checked ? Main.MainMap.GetRectOfAllRoutes(null) ?? RectLatLng.Empty : Main.MainMap.SelectedArea;
    private void ExportSelectionChanged(RectLatLng area, bool zoomToFit) => UpdateExportPreview();
    private void ExportProviderChanged(GMapProvider provider) => UpdateExportPreview();
    protected override void OnActivated(EventArgs e) { base.OnActivated(e); UpdateExportPreview(); }
    private void UpdateExportPreview()
    {
        if (exportResolutionPreview == null || IsDisposed || bg.IsBusy) return;
        var area = ExportArea;
        bool validArea = TryEstimateExport(area, (int)numericUpDown1.Value, Main.MainMap.MapProvider, !checkBoxWorldFile.Checked && !checkBoxKMZ.Checked, out long width, out long height);
        if (validArea)
        {
            double resolution = Main.MainMap.MapProvider.Projection.GetGroundResolution((int)numericUpDown1.Value, area.Top - area.HeightLat / 2);
            exportResolutionPreview.Text = string.Format(CultureInfo.InvariantCulture, "原始尺寸  {0:N0} × {1:N0} px\n地面分辨率约 {2:0.##} 米 / 像素", width, height, resolution);
        }
        else exportResolutionPreview.Text = area.IsEmpty ? "请先在主窗口选择导出区域" : "当前缩放级别无法生成此区域，请调整范围或级别";
        bool validSize = long.TryParse(textBoxSizeFile.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out long limit) && limit > 0;
        bool validPath = TryOutputPath(textBoxSaveAs.Text, out _);
        button1.Enabled = validArea && validSize && validPath;
        exportStatus.Text = !validArea ? "等待有效选区" : !validSize ? "大小上限需要是大于 0 的整数（MB）" : !validPath ? "请输入完整的 PNG 路径，并确认文件夹存在" : "准备导出  ·  PNG 与定位文件保存到同一目录";
    }
    internal static bool TryEstimateExport(RectLatLng area, int zoom, GMapProvider provider, bool withFrame, out long width, out long height)
    {
        width = height = 0;
        if (area.IsEmpty) return false;
        var first = provider.Projection.FromLatLngToPixel(area.LocationTopLeft, zoom);
        var last = provider.Projection.FromLatLngToPixel(area.Bottom, area.Right, zoom);
        width = last.X - first.X + (withFrame ? 44 : 0); height = last.Y - first.Y + (withFrame ? 44 : 0);
        return width > 0 && height > 0 && width <= int.MaxValue && height <= int.MaxValue && width * (double)height <= int.MaxValue / 4;
    }
    private static bool TryOutputPath(string value, out string path)
    {
        path = null;
        try { if (!Path.IsPathRooted(value) || !string.Equals(Path.GetExtension(value), ".png", StringComparison.OrdinalIgnoreCase)) return false;
            path = Path.GetFullPath(value); return Directory.Exists(Path.GetDirectoryName(path)); }
        catch (ArgumentException) { return false; } catch (NotSupportedException) { return false; } catch (PathTooLongException) { return false; }
    }
    private void SetExportBusy(bool busy)
    {
        exportSettings.Enabled = !busy; button1.Enabled = !busy; numericUpDown1.Enabled = !busy;
        button2.Text = busy ? "取消导出" : "关闭";
        if (busy) exportStatus.Text = "正在下载地图瓦片…";
    }
    private void DisposeModernExport()
    {
        if (Main != null) { Main.MainMap.OnSelectionChange -= ExportSelectionChanged; Main.MainMap.OnMapTypeChanged -= ExportProviderChanged; }
        label1?.Dispose(); label2?.Dispose();
    }
}
