using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.MapProviders;

namespace Demo.WindowsForms;

public partial class MainForm
{
    private TableLayoutPanel workspaceBody;
    private Panel workspacePages;
    private Panel[] workspacePage;
    private WorkspaceButton[] navigation;
    private WorkspaceButton panButton;
    private WorkspaceButton selectionButton;
    private TextBox areaNorth, areaWest, areaSouth, areaEast;
    private Label selectionSummary;
    private Label viewportSummary;
    private Label zoomCaption;
    private ToolStripStatusLabel providerStatus, positionStatus, zoomStatus, tileStatus;
    private bool selectingArea;
    private bool temporarySelection;

    // Keep the recovered designer as a control/event factory. Layout and styling live here.
    private void InitializeModernWorkspace()
    {
        RebuildWorkspaceButtons();
        Controls.Clear();
        Font = ModernTheme.Body;
        ForeColor = ModernTheme.Ink;
        BackColor = ModernTheme.Background;
        MinimumSize = new Size(1040, 720);
        WindowState = FormWindowState.Normal;
        ClientSize = new Size(1440, 1000);
        Text = "GMap.NET CMANO — 地图叠加层工作台 · Preview 6";

        var root = new TableLayoutPanel { Name = "workspaceRoot", Dock = DockStyle.Fill, Margin = Padding.Empty, ColumnCount = 1, RowCount = 3, BackColor = ModernTheme.Background };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        Controls.Add(root);
        root.Controls.Add(BuildWorkspaceHeader(), 0, 0);

        workspaceBody = new TableLayoutPanel { Name = "workspaceBody", Dock = DockStyle.Fill, Margin = Padding.Empty, ColumnCount = 2, RowCount = 1 };
        workspaceBody.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360));
        workspaceBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.Controls.Add(workspaceBody, 0, 1);
        workspaceBody.Controls.Add(BuildWorkspaceSidebar(), 0, 0);
        workspaceBody.Controls.Add(BuildMapWorkspace(), 1, 0);

        var status = new StatusStrip { Name = "workspaceStatus", Dock = DockStyle.Fill, SizingGrip = false, BackColor = Color.White,
            Font = ModernTheme.Body, ForeColor = ModernTheme.Muted, Padding = new Padding(12, 0, 12, 0), Margin = Padding.Empty };
        providerStatus = new ToolStripStatusLabel();
        positionStatus = new ToolStripStatusLabel { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
        zoomStatus = new ToolStripStatusLabel();
        tileStatus = new ToolStripStatusLabel("就绪");
        status.Items.AddRange(new ToolStripItem[] { providerStatus, positionStatus, zoomStatus, tileStatus });
        root.Controls.Add(status, 0, 2);

        MainMap.DragButton = MouseButtons.Left;
        MainMap.SelectedAreaFillColor = Color.FromArgb(48, ModernTheme.Accent);
        MainMap.EmptyTileColor = ModernTheme.Background;
        MainMap.OnSelectionChange += WorkspaceSelectionChanged;
        MainMap.MouseUp += (s, e) => UpdateSelection(true);
        MainMap.OnPositionChanged += p => UpdateWorkspaceStatus();
        MainMap.OnMapZoomChanged += UpdateWorkspaceStatus;
        MainMap.OnMapTypeChanged += p => UpdateWorkspaceStatus();
        panelMenu.TextChanged += (s, e) => tileStatus.Text = panelMenu.Text.Contains("loading") ? "地图加载中…" : "加载完成 · " + MainMap.ElapsedMilliseconds + " ms";
        routes.Routes.CollectionChanged += (s, e) => UpdateWorkspaceStatus();
        KeyDown += WorkspaceKeyDown;
        KeyUp += WorkspaceKeyUp;
        Deactivate += (s, e) => { temporarySelection = false; ApplyMapInteraction(); };
        SwitchWorkspacePage(0);
    }

    private Control BuildWorkspaceHeader()
    {
        var header = new TableLayoutPanel { Name = "workspaceHeader", Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1,
            BackColor = Color.White, Padding = new Padding(22, 12, 22, 12), Margin = Padding.Empty };
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        foreach (float width in new[] { 54f, 0f, 136f, 174f }) header.ColumnStyles.Add(new ColumnStyle(width == 0 ? SizeType.Percent : SizeType.Absolute, width == 0 ? 100 : width));
        header.Controls.Add(new MapBrandMark { Dock = DockStyle.Fill, Margin = new Padding(0, 5, 10, 5) }, 0, 0);
        var brand = ModernTheme.Stack();
        var name = ModernTheme.Label("GMap.NET", true); name.Font = ModernTheme.Brand;
        ModernTheme.Add(brand, name, 33);
        ModernTheme.Add(brand, ModernTheme.Label("CMANO 地图叠加层  /  Preview 6"), 21);
        header.Controls.Add(brand, 1, 0);
        ModernTheme.Style(button1, "刷新地图");
        ModernTheme.Style(button13, "导出叠加层", true);
        button1.Dock = button13.Dock = DockStyle.Fill;
        button1.Margin = new Padding(12, 8, 4, 8); button13.Margin = new Padding(8, 8, 0, 8);
        header.Controls.Add(button1, 2, 0); header.Controls.Add(button13, 3, 0);
        return header;
    }

    private Control BuildWorkspaceSidebar()
    {
        var sidebar = new TableLayoutPanel { Name = "workspaceSidebar", Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3,
            Margin = Padding.Empty, BackColor = ModernTheme.Background };
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var title = ModernTheme.Label("制作地图叠加层", true); title.Font = ModernTheme.Heading; title.Margin = new Padding(16, 8, 16, 0);
        sidebar.Controls.Add(title, 0, 0);
        navigation = new[] { ModernTheme.Button("区域", "navigationArea"), ModernTheme.Button("标记与路线", "navigationMarkers"), ModernTheme.Button("工具", "navigationTools") };
        var nav = ModernTheme.Columns(navigation); nav.Margin = new Padding(16, 0, 16, 8);
        for (int i = 0; i < navigation.Length; i++) { int index = i; navigation[i].Click += (s, e) => SwitchWorkspacePage(index); }
        sidebar.Controls.Add(nav, 0, 1);
        workspacePages = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
        sidebar.Controls.Add(workspacePages, 0, 2);
        workspacePage = new[] { BuildAreaPage(), BuildMarkerPage(), BuildToolsPage() };
        foreach (var page in workspacePage) workspacePages.Controls.Add(page);
        return sidebar;
    }

    private Panel NewWorkspacePage(string name, out TableLayoutPanel content)
    {
        var page = new Panel { Name = name, AutoScroll = true, Dock = DockStyle.Fill, Padding = new Padding(16, 4, 16, 16), BackColor = ModernTheme.Background };
        content = ModernTheme.Stack(); content.BackColor = ModernTheme.Background;
        page.Controls.Add(content);
        return page;
    }

    private Panel BuildAreaPage()
    {
        var page = NewWorkspacePage("areaPage", out var stack);
        var source = ModernTheme.Card(stack, "01   选择地图");
        ModernTheme.Add(source, ModernTheme.Label("地图源"), 22);
        ModernTheme.Input(comboBoxMapType); comboBoxMapType.AccessibleName = "地图源"; ModernTheme.Add(source, comboBoxMapType, 34);
        ModernTheme.Add(source, ModernTheme.Label("访问方式"), 22);
        ModernTheme.Input(comboBoxMode); comboBoxMode.FormattingEnabled = true;
        comboBoxMode.Format += (s, e) => { if (e.ListItem is AccessMode mode) e.Value = mode == AccessMode.CacheOnly ? "仅本地缓存" : mode == AccessMode.ServerOnly ? "仅在线地图" : "在线地图 + 本地缓存"; };
        ModernTheme.Add(source, comboBoxMode, 34);
        AddNetworkSettings(source);

        var location = ModernTheme.Card(stack, "02   定位地图");
        ModernTheme.Add(location, ModernTheme.Columns(ModernTheme.Label("纬度  /  Latitude"), ModernTheme.Label("经度  /  Longitude")), 24);
        ModernTheme.Input(textBoxLat); ModernTheme.Input(textBoxLng); textBoxLat.AccessibleName = "定位纬度"; textBoxLng.AccessibleName = "定位经度";
        ModernTheme.Add(location, ModernTheme.Columns(textBoxLat, textBoxLng), 34);
        ModernTheme.Style(button8, "定位到坐标");
        var current = ModernTheme.Button("填入地图中心", "copyMapCenter");
        current.Click += (s, e) => { textBoxLat.Text = MainMap.Position.Lat.ToString("F6", CultureInfo.InvariantCulture); textBoxLng.Text = MainMap.Position.Lng.ToString("F6", CultureInfo.InvariantCulture); SetDmsInputs(MainMap.Position); };
        ModernTheme.Add(location, ModernTheme.Columns(button8, current), 40);
        ModernTheme.Add(location, ModernTheme.Label("地点搜索  ·  输入名称后按 Enter"), 26);
        ModernTheme.Input(textBoxGeo); textBoxGeo.Text = string.Empty;
        ModernTheme.Add(location, textBoxGeo, 34);

        var area = ModernTheme.Card(stack, "03   确定导出范围", "在地图上切换到「框选」，拖动鼠标选择区域；也可填写十进制度边界。");
        areaNorth = NewCoordinateInput("areaNorth"); areaWest = NewCoordinateInput("areaWest");
        areaSouth = NewCoordinateInput("areaSouth"); areaEast = NewCoordinateInput("areaEast");
        ModernTheme.Add(area, ModernTheme.Columns(ModernTheme.Label("北纬度  /  North"), ModernTheme.Label("西经度  /  West")), 24);
        ModernTheme.Add(area, ModernTheme.Columns(areaNorth, areaWest), 34);
        ModernTheme.Add(area, ModernTheme.Columns(ModernTheme.Label("南纬度  /  South"), ModernTheme.Label("东经度  /  East")), 24);
        ModernTheme.Add(area, ModernTheme.Columns(areaSouth, areaEast), 34);
        var apply = ModernTheme.Button("应用边界坐标", "applyAreaButton", true); apply.Click += ApplyAreaCoordinates;
        ModernTheme.Add(area, apply, 42);
        var view = ModernTheme.Button("使用当前视野", "useViewportButton"); view.Click += (s, e) => SetWorkspaceSelection(MainMap.ViewArea);
        var clear = ModernTheme.Button("清除选区", "clearAreaButton"); clear.Click += (s, e) => SetWorkspaceSelection(RectLatLng.Empty);
        ModernTheme.Add(area, ModernTheme.Columns(view, clear), 40);
        selectionSummary = ModernTheme.Label("尚未选择区域", color: ModernTheme.Accent); selectionSummary.Name = "selectionSummary";
        ModernTheme.Add(area, selectionSummary, 40);
        return page;
    }

    private Panel BuildMarkerPage()
    {
        var page = NewWorkspacePage("markerPage", out var stack);
        var markers = ModernTheme.Card(stack, "标记与多边形", "开启当前标记后，在地图上单击放置，再添加固定标记。");
        ModernTheme.Check(checkBoxCurrentMarker, "显示当前标记"); ModernTheme.Add(markers, checkBoxCurrentMarker, 28);
        ModernTheme.Check(checkBoxPlacemarkInfo, "查询标记地址信息"); ModernTheme.Add(markers, checkBoxPlacemarkInfo, 28);
        AddButtons(markers, button4, "添加标记", button5, "清除标记");
        ModernTheme.Style(button7, "缩放至全部标记"); ModernTheme.Add(markers, button7, 40);
        AddReadout(markers, "标记数", textBoxMarkerCount);
        var route = ModernTheme.Card(stack, "路线", "先在地图上设置当前标记，再分别取作起点与终点。");
        AddButtons(route, buttonSetStart, "设置起点", buttonSetEnd, "设置终点");
        ModernTheme.Style(button3, "生成路线"); ModernTheme.Add(route, button3, 40);
        AddButtons(route, button16, "导入 GPX", buttonExportToGpx, "导出 GPX");
        AddButtons(route, button6, "清除路线", button15, "清除多边形");
        AddReadout(route, "路线数", textBoxrouteCount);
        var gps = ModernTheme.Card(stack, "移动 GPS 日志");
        ModernTheme.Add(gps, ModernTheme.Label("开始时间"), 24); ModernTheme.Add(gps, MobileLogFrom, 32);
        ModernTheme.Add(gps, ModernTheme.Label("结束时间"), 24); ModernTheme.Add(gps, MobileLogTo, 32);
        ModernTheme.Style(button14, "打开 GPS 日志"); ModernTheme.Add(gps, button14, 40);
        return page;
    }

    private Panel BuildToolsPage()
    {
        var page = NewWorkspacePage("toolsPage", out var stack);
        var cache = ModernTheme.Card(stack, "本地缓存", "缓存在 EXE 同目录的 cache 文件夹中，可随程序一起保留。");
        ModernTheme.Style(button17, "打开缓存文件夹"); ModernTheme.Add(cache, button17, 40);
        AddReadout(cache, "磁盘缓存", textBoxCacheSize); AddReadout(cache, "状态", textBoxCacheStatus); AddReadout(cache, "内存", textBoxMemory);
        ModernTheme.Check(checkBoxUseRouteCache, "缓存路线、地理编码与地址查询"); ModernTheme.Add(cache, checkBoxUseRouteCache, 30);
        AddButtons(cache, button10, "导入缓存", button9, "导出缓存");
        ModernTheme.Style(button11, "预下载选区地图"); ModernTheme.Add(cache, button11, 40);
        ModernTheme.Style(button2, "清理磁盘瓦片缓存"); ModernTheme.Add(cache, button2, 40);
        var map = ModernTheme.Card(stack, "地图工具");
        ModernTheme.Check(checkBoxCanDrag, "允许平移地图"); ModernTheme.Add(map, checkBoxCanDrag, 28);
        ModernTheme.Check(checkBoxDebug, "显示瓦片网格"); ModernTheme.Add(map, checkBoxDebug, 28);
        ModernTheme.Style(button12, "保存当前视图 PNG"); ModernTheme.Add(map, button12, 40);
        trackBar1.Orientation = Orientation.Horizontal; trackBar1.AutoSize = false; trackBar1.TickStyle = TickStyle.None;
        ModernTheme.Add(map, ModernTheme.Label("缩放级别"), 22); ModernTheme.Add(map, trackBar1, 34);
        ModernTheme.Check(checkBoxTileHost, "启动 Leaflet 地图演示"); ModernTheme.Add(map, checkBoxTileHost, 28);
        var dms = ModernTheme.Card(stack, "度 / 分 / 秒坐标", "度栏写 N31 / E121，分与秒单独填写。主页面使用十进制度。");
        AddDmsRow(dms, "定位纬度", textBoxLatDegree, textBoxLatMinute, textBoxLatSecond);
        AddDmsRow(dms, "定位经度", textBoxLngDegree, textBoxLngMinute, textBoxLngSecond);
        var go = ModernTheme.Button("定位到度分秒坐标"); go.Click += LocateDmsCoordinates; ModernTheme.Add(dms, go, 40);
        AddDmsRow(dms, "西北纬度", textBoxTLLatDegree, textBoxTLLatMinute, textBoxTLLatSecond);
        AddDmsRow(dms, "西北经度", textBoxTLLngDegree, textBoxTLLngMinute, textBoxTLLngSecond);
        AddDmsRow(dms, "东南纬度", textBoxBRLatDegree, textBoxBRLatMinute, textBoxBRLatSecond);
        AddDmsRow(dms, "东南经度", textBoxBRLngDegree, textBoxBRLngMinute, textBoxBRLngSecond);
        ModernTheme.Style(buttonDraw, "应用度分秒选区"); ModernTheme.Add(dms, buttonDraw, 40);
        var live = ModernTheme.Card(stack, "旧版演示工具", "用于诊断与演示，地图导出通常无需开启。");
        var choices = ModernTheme.Stack();
        RadioButton[] modes = { radioButtonNone, radioButtonPerf, radioButtonFlight, radioButtonVehicle, radioButtonTcpIp };
        string[] names = { "关闭演示", "性能测试", "航班演示", "车辆演示", "TCP/IP 连接演示" };
        for (int i = 0; i < modes.Length; i++) { modes[i].Text = names[i]; modes[i].Font = ModernTheme.Body; modes[i].AutoSize = false; ModernTheme.Add(choices, modes[i], 28); }
        ModernTheme.Add(live, choices);
        GridConnections.BackgroundColor = Color.White; GridConnections.BorderStyle = BorderStyle.FixedSingle; GridConnections.Font = ModernTheme.Body;
        ModernTheme.Add(live, GridConnections, 150);
        ModernTheme.Check(checkBoxTcpIpSnap, "新连接出现时跟随地图"); ModernTheme.Add(live, checkBoxTcpIpSnap, 28);
        ModernTheme.Check(checkBoxTraceRoute, "跟踪每个连接的路由"); ModernTheme.Add(live, checkBoxTraceRoute, 28);
        return page;
    }

    private Control BuildMapWorkspace()
    {
        var workspace = new TableLayoutPanel { Name = "mapWorkspace", Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4,
            Margin = new Padding(16, 10, 16, 16), BackColor = Color.White, Padding = new Padding(1) };
        workspace.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        workspace.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        workspace.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        workspace.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        var heading = ModernTheme.Stack(); heading.Padding = new Padding(16, 5, 16, 4);
        var title = ModernTheme.Label("地图工作区", true); title.Font = ModernTheme.Heading; ModernTheme.Add(heading, title, 28);
        viewportSummary = ModernTheme.Label("定位地图后，框选需要导出的区域"); viewportSummary.Name = "viewportSummary";
        ModernTheme.Add(heading, viewportSummary, 20); workspace.Controls.Add(heading, 0, 0);
        var toolbar = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 7, RowCount = 1, Margin = new Padding(12, 0, 12, 6) };
        toolbar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        float[] widths = { 34, 34, 74, 94, 94, 0, 108 };
        foreach (float width in widths) toolbar.ColumnStyles.Add(new ColumnStyle(width == 0 ? SizeType.Percent : SizeType.Absolute, width == 0 ? 100 : width));
        ModernTheme.Style(buttonZoomUp, "+"); ModernTheme.Style(buttonZoomDown, "−");
                panButton = ModernTheme.Button("平移", "panModeButton"); panButton.Selected = true; panButton.Click += (s, e) => SetSelectionMode(false);
        selectionButton = ModernTheme.Button("框选区域", "selectionModeButton"); selectionButton.Click += (s, e) => SetSelectionMode(true);
        var fit = ModernTheme.Button("适配选区", "fitAreaButton"); fit.Click += (s, e) => { if (!MainMap.SelectedArea.IsEmpty) MainMap.SetZoomToFitRect(MainMap.SelectedArea); };
        zoomCaption = ModernTheme.Label("Z 9", true); zoomCaption.TextAlign = ContentAlignment.MiddleCenter;
        Control[] controls = { buttonZoomUp, buttonZoomDown, zoomCaption, panButton, selectionButton, new Panel(), fit };
        for (int i = 0; i < controls.Length; i++) { controls[i].Dock = DockStyle.Fill; controls[i].Margin = new Padding(2, 2, 2, 2); toolbar.Controls.Add(controls[i], i, 0); }
        workspace.Controls.Add(toolbar, 0, 1);
        MainMap.Dock = DockStyle.Fill; MainMap.Margin = Padding.Empty; workspace.Controls.Add(MainMap, 0, 2);
        var hint = ModernTheme.Label("左键平移  ·  框选模式下左键拖动选区  ·  Alt + 左键也可框选"); hint.Name = "mapInteractionHint"; hint.Margin = new Padding(12, 0, 12, 0);
        workspace.Controls.Add(hint, 0, 3);
        return workspace;
    }

    private static Button RebuildButton(Button original, EventHandler handler)
    {
        var button = ModernTheme.Button(original.Text, original.Name);
        button.TabIndex = original.TabIndex; button.Click += handler;
        return button;
    }
    private void RebuildWorkspaceButtons()
    {
        button1 = RebuildButton(button1, button1_Click); button2 = RebuildButton(button2, button2_Click);
        button3 = RebuildButton(button3, button3_Click); button4 = RebuildButton(button4, button4_Click);
        button5 = RebuildButton(button5, button5_Click); button6 = RebuildButton(button6, button6_Click);
        button7 = RebuildButton(button7, button7_Click); button8 = RebuildButton(button8, button8_Click);
        button9 = RebuildButton(button9, button9_Click); button10 = RebuildButton(button10, button10_Click);
        button11 = RebuildButton(button11, button11_Click); button12 = RebuildButton(button12, button12_Click);
        button13 = RebuildButton(button13, button13_Click); button14 = RebuildButton(button14, button14_Click);
        button15 = RebuildButton(button15, button15_Click); button16 = RebuildButton(button16, button16_Click);
        button17 = RebuildButton(button17, button17_Click); buttonDraw = RebuildButton(buttonDraw, buttonDraw_Click);
        buttonSetStart = RebuildButton(buttonSetStart, buttonSetStart_Click); buttonSetEnd = RebuildButton(buttonSetEnd, buttonSetEnd_Click);
        buttonExportToGpx = RebuildButton(buttonExportToGpx, buttonExportToGpx_Click);
        buttonZoomUp = RebuildButton(buttonZoomUp, buttonZoomUp_Click); buttonZoomDown = RebuildButton(buttonZoomDown, buttonZoomDown_Click);
    }
    private static TextBox NewCoordinateInput(string name) { var input = new TextBox { Name = name, AccessibleName = name == "areaNorth" ? "北纬度" : name == "areaSouth" ? "南纬度" : name == "areaWest" ? "西经度" : "东经度" }; ModernTheme.Input(input); return input; }
    private static void AddButtons(TableLayoutPanel card, Button first, string firstText, Button second, string secondText)
    {
        ModernTheme.Style(first, firstText); ModernTheme.Style(second, secondText); ModernTheme.Add(card, ModernTheme.Columns(first, second), 40);
    }
    private static void AddReadout(TableLayoutPanel card, string label, TextBox value)
    {
        value.ReadOnly = true; value.BorderStyle = BorderStyle.None; value.Font = ModernTheme.Body; value.BackColor = Color.White; value.ForeColor = ModernTheme.Ink;
        ModernTheme.Add(card, ModernTheme.Label(label), 21); ModernTheme.Add(card, value, 25);
    }
    private static void AddDmsRow(TableLayoutPanel card, string label, TextBox degrees, TextBox minutes, TextBox seconds)
    {
        ModernTheme.Add(card, ModernTheme.Label(label + "  ·  度 / 分 / 秒"), 23);
        ModernTheme.Input(degrees); ModernTheme.Input(minutes); ModernTheme.Input(seconds);
        ModernTheme.Add(card, ModernTheme.Columns(degrees, minutes, seconds), 34);
    }
    private void SwitchWorkspacePage(int index)
    {
        for (int i = 0; i < workspacePage.Length; i++) { workspacePage[i].Visible = i == index; navigation[i].Selected = i == index; navigation[i].Invalidate(); }
        workspacePage[index].BringToFront();
    }
    private void SetDmsInputs(PointLatLng position)
    {
        SetDmsValue(position.Lat, true, textBoxLatDegree, textBoxLatMinute, textBoxLatSecond);
        SetDmsValue(position.Lng, false, textBoxLngDegree, textBoxLngMinute, textBoxLngSecond);
    }
    private static void SetDmsValue(double value, bool latitude, TextBox degree, TextBox minute, TextBox second)
    {
        double total = Math.Round(Math.Abs(value) * 3600, 3);
        int d = (int)(total / 3600); int m = (int)((total - d * 3600) / 60);
        string direction = latitude ? (value < 0 ? "S" : "N") : (value < 0 ? "W" : "E");
        degree.Text = direction + d.ToString(CultureInfo.InvariantCulture);
        minute.Text = m.ToString(CultureInfo.InvariantCulture); second.Text = (total - d * 3600 - m * 60).ToString("0.###", CultureInfo.InvariantCulture);
    }
    private void LocateDmsCoordinates(object sender, EventArgs e)
    {
        double lat = Stuff.ConvertDegreeAngleToDouble(textBoxLatDegree, textBoxLatMinute, textBoxLatSecond, true);
        double lng = Stuff.ConvertDegreeAngleToDouble(textBoxLngDegree, textBoxLngMinute, textBoxLngSecond, false);
        textBoxLat.Text = lat.ToString("F6", CultureInfo.InvariantCulture); textBoxLng.Text = lng.ToString("F6", CultureInfo.InvariantCulture);
        button8_Click(sender, e);
    }
    private void SetSelectionMode(bool select) { selectingArea = select; temporarySelection = false; ApplyMapInteraction(); MainMap.Focus(); }
    private void ApplyMapInteraction()
    {
        MainMap.DragButton = selectingArea || temporarySelection ? MouseButtons.Right : MouseButtons.Left;
        MainMap.DisableAltForSelection = selectingArea || temporarySelection;
        panButton.Selected = !selectingArea; selectionButton.Selected = selectingArea;
        panButton.Invalidate(); selectionButton.Invalidate();
    }
    private void WorkspaceKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.E && button13.Enabled) { button13.PerformClick(); e.SuppressKeyPress = true; }
        if (e.KeyCode == Keys.Enter && (textBoxLat.Focused || textBoxLng.Focused)) { button8_Click(sender, e); e.SuppressKeyPress = true; }
        if (e.Alt && MainMap.Focused && !selectingArea) { temporarySelection = true; ApplyMapInteraction(); }
    }
    private void WorkspaceKeyUp(object sender, KeyEventArgs e)
    {
        if (temporarySelection && !e.Alt) { temporarySelection = false; ApplyMapInteraction(); }
    }
    private void WorkspaceSelectionChanged(RectLatLng area, bool zoomToFit) => UpdateSelection(true);
    private void SetWorkspaceSelection(RectLatLng area) { MainMap.SelectedArea = area; MainMap.Refresh(); UpdateSelection(true); UpdateWorkspaceStatus(); }
    private void ApplyAreaCoordinates(object sender, EventArgs e)
    {
        if (!TryCreateSelection(areaNorth.Text, areaWest.Text, areaSouth.Text, areaEast.Text, out var area, out string error))
        { selectionSummary.Text = error; selectionSummary.ForeColor = Color.FromArgb(168, 61, 47); return; }
        SetWorkspaceSelection(area);
    }
    internal static bool TryCreateSelection(string north, string west, string south, string east, out RectLatLng area, out string error)
    {
        area = RectLatLng.Empty; error = "请输入有效的十进制度坐标。";
        if (!TryCoordinate(north, out double n) || !TryCoordinate(west, out double w) || !TryCoordinate(south, out double s) || !TryCoordinate(east, out double e)) return false;
        if (n > 90 || s < -90 || n < -90 || s > 90 || w < -180 || e > 180 || w > 180 || e < -180) { error = "纬度范围 −90～90，经度范围 −180～180。"; return false; }
        if (n <= s) { error = "北纬度需要大于南纬度。"; return false; }
        if (e <= w) { error = "东经度需要大于西经度；跨 180° 请分区。"; return false; }
        area = RectLatLng.FromLTRB(w, n, e, s); error = string.Empty; return true;
    }
    private static bool TryCoordinate(string text, out double value)
    {
        bool parsed = double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) || double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value);
        return parsed && !double.IsNaN(value) && !double.IsInfinity(value);
    }
    private void UpdateSelection(bool copyCoordinates)
    {
        if (selectionSummary == null || IsDisposed) return;
        var area = MainMap.SelectedArea;
        selectionSummary.ForeColor = ModernTheme.Accent;
        selectionSummary.Text = area.IsEmpty ? "尚未选择区域 · 框选后可从右上角导出" : string.Format(CultureInfo.InvariantCulture, "已选择区域  ·  {0:F4}° × {1:F4}°\n可从右上角导出 PNG + PGW", area.WidthLng, area.HeightLat);
        if (copyCoordinates)
        {
            areaNorth.Text = area.IsEmpty ? "" : area.Top.ToString("F6", CultureInfo.InvariantCulture); areaWest.Text = area.IsEmpty ? "" : area.Left.ToString("F6", CultureInfo.InvariantCulture);
            areaSouth.Text = area.IsEmpty ? "" : area.Bottom.ToString("F6", CultureInfo.InvariantCulture); areaEast.Text = area.IsEmpty ? "" : area.Right.ToString("F6", CultureInfo.InvariantCulture);
        }
        button13.Enabled = !area.IsEmpty || routes.Routes.Count > 0;
    }
    private void UpdateWorkspaceStatus()
    {
        if (providerStatus == null || IsDisposed) return;
        if (InvokeRequired) { try { BeginInvoke(new MethodInvoker(UpdateWorkspaceStatus)); } catch (InvalidOperationException) { } return; }
        textBoxMarkerCount.Text = objects.Markers.Count.ToString(CultureInfo.InvariantCulture); textBoxrouteCount.Text = routes.Routes.Count.ToString(CultureInfo.InvariantCulture);
        providerStatus.Text = "地图：" + MainMap.MapProvider.Name + "    ";
        positionStatus.Text = string.Format(CultureInfo.InvariantCulture, "中心 {0:F5}, {1:F5}", MainMap.Position.Lat, MainMap.Position.Lng);
        zoomStatus.Text = "缩放 " + MainMap.Zoom.ToString("0.##", CultureInfo.InvariantCulture) + "    ";
        zoomCaption.Text = "Z " + MainMap.Zoom.ToString("0.##", CultureInfo.InvariantCulture);
        viewportSummary.Text = MainMap.SelectedArea.IsEmpty ? "定位地图后，框选需要导出的区域" : "选区已就绪  ·  点击「导出叠加层」设置输出画质";
        UpdateSelection(false);
    }
}
