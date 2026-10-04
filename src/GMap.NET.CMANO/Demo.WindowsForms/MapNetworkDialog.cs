using System;
using System.Drawing;
using System.Windows.Forms;

namespace Demo.WindowsForms;

internal class MapNetworkDialog : Form
{
    private readonly MapNetworkSettings original;
    private readonly Func<MapNetworkSettings, string> saveAndApply;
    private readonly ComboBox mode;
    private readonly TextBox address, userName, password;
    private readonly NumericUpDown timeout;
    private readonly Label message;

    internal MapNetworkDialog(MapNetworkSettings settings, Func<MapNetworkSettings, string> apply)
    {
        original = settings; saveAndApply = apply;
        SuspendLayout();
        AutoScaleDimensions = new SizeF(96, 96); AutoScaleMode = AutoScaleMode.Dpi;
        Font = ModernTheme.Body; ForeColor = ModernTheme.Ink; BackColor = ModernTheme.Background;
        Text = "代理与连接设置 — GMap.NET CMANO";
        ClientSize = new Size(620, 630); MinimumSize = new Size(560, 610);
        StartPosition = FormStartPosition.CenterParent; ShowInTaskbar = false;
        MinimizeBox = false; MaximizeBox = false;

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Margin = Padding.Empty };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
        Controls.Add(root);
        var header = ModernTheme.Stack(); header.Padding = new Padding(24, 12, 24, 8); header.BackColor = ModernTheme.Background;
        var title = ModernTheme.Label("代理与连接设置", true); title.Font = ModernTheme.Heading;
        ModernTheme.Add(header, title, 31);
        ModernTheme.Add(header, ModernTheme.Label("应用后刷新地图；地图浏览与导出共用此设置。"), 25); root.Controls.Add(header, 0, 0);

        var panel = new Panel { AutoScroll = true, Dock = DockStyle.Fill, Margin = Padding.Empty, Padding = new Padding(24, 4, 24, 12) };
        var stack = ModernTheme.Stack(); stack.BackColor = ModernTheme.Background; panel.Controls.Add(stack); root.Controls.Add(panel, 0, 1);
        var connection = ModernTheme.Card(stack, "地图连接");
        ModernTheme.Add(connection, ModernTheme.Label("连接方式"), 24);
        mode = new ComboBox { Name = "proxyMode", AccessibleName = "连接方式" }; ModernTheme.Input(mode);
        mode.Items.AddRange(new object[] { "直连（不使用代理）", "使用 Windows 系统代理", "手动 HTTP 代理" });
        ModernTheme.Add(connection, mode, 34);
        ModernTheme.Add(connection, ModernTheme.Label("HTTP 代理服务器"), 24);
        address = Input("proxyAddress", "HTTP 代理服务器"); ModernTheme.Add(connection, address, 34);
        ModernTheme.Add(connection, ModernTheme.Label("例如 http://127.0.0.1:7890。请使用代理软件的 HTTP 或 Mixed 端口；HTTP 代理也可用于 HTTPS 地图。"), 44);
        ModernTheme.Add(connection, ModernTheme.Columns(ModernTheme.Label("用户名（可选）"), ModernTheme.Label("密码（可选）")), 24);
        userName = Input("proxyUserName", "代理用户名"); password = Input("proxyPassword", "代理密码"); password.UseSystemPasswordChar = true;
        ModernTheme.Add(connection, ModernTheme.Columns(userName, password), 34);
        ModernTheme.Add(connection, ModernTheme.Label("密码由 Windows 加密保存，仅当前 Windows 用户可读取。"), 27);
        ModernTheme.Add(connection, ModernTheme.Label("连接超时（秒）"), 24);
        timeout = new NumericUpDown { Name = "proxyTimeout", AccessibleName = "连接超时（秒）", Minimum = 5, Maximum = 120 };
        ModernTheme.Input(timeout); ModernTheme.Add(connection, timeout, 34);
        ModernTheme.Add(connection, ModernTheme.Label("仅本地缓存模式不会联网。保存设置不会切换访问方式。"), 32);

        var footer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2,
            Margin = Padding.Empty, Padding = new Padding(24, 8, 24, 12), BackColor = Color.White };
        footer.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); footer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        message = ModernTheme.Label("若仍有部分地图无法加载，可切换地图源；代理无法恢复已停止服务的图源。"); message.Name = "proxyMessage";
        footer.Controls.Add(message, 0, 0);
        var actions = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Margin = Padding.Empty };
        actions.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96)); actions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        var cancel = ModernTheme.Button("取消", "proxyCancel"); cancel.DialogResult = DialogResult.Cancel; cancel.Margin = new Padding(0, 0, 8, 0);
        var save = ModernTheme.Button("保存并应用", "proxySave", true); save.Margin = Padding.Empty;
        cancel.Dock = save.Dock = DockStyle.Fill; actions.Controls.Add(cancel, 1, 0); actions.Controls.Add(save, 2, 0);
        footer.Controls.Add(actions, 0, 1); root.Controls.Add(footer, 0, 2);
        AcceptButton = save; CancelButton = cancel;
        save.Click += SaveSettings;
        mode.SelectedIndexChanged += (s, e) => UpdateMode();
        mode.SelectedIndex = (int)settings.Mode; address.Text = settings.Address;
        userName.Text = settings.UserName; password.Text = settings.Password; timeout.Value = settings.TimeoutSeconds;
        UpdateMode();
        ResumeLayout(true);
    }

    private static TextBox Input(string name, string accessibleName)
    {
        var input = new TextBox { Name = name, AccessibleName = accessibleName };
        ModernTheme.Input(input); return input;
    }

    private void UpdateMode()
    {
        bool manual = mode.SelectedIndex == (int)MapProxyMode.ManualHttp;
        address.Enabled = userName.Enabled = password.Enabled = manual;
    }

    private void SaveSettings(object sender, EventArgs e)
    {
        bool manual = mode.SelectedIndex == (int)MapProxyMode.ManualHttp;
        var settings = new MapNetworkSettings
        {
            Mode = (MapProxyMode)mode.SelectedIndex, TimeoutSeconds = (int)timeout.Value,
            Address = manual ? address.Text.Trim() : original.Address,
            UserName = manual ? userName.Text.Trim() : original.UserName,
            Password = manual ? password.Text : original.Password
        };
        if (!settings.TryValidate(out var error) || (error = saveAndApply(settings)) != null)
        {
            message.ForeColor = Color.Firebrick; message.Text = error; return;
        }
        DialogResult = DialogResult.OK; Close();
    }
}
