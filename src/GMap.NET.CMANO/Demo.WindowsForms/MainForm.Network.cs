using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace Demo.WindowsForms;

public partial class MainForm
{
    private MapNetworkSettings networkSettings;
    private string networkSettingsPath;
    private string networkSettingsMessage;
    private Label networkSummary;
    internal MapNetworkSettings NetworkSettings => networkSettings;

    private void InitializeNetworkSettings(string path)
    {
        networkSettingsPath = Path.GetFullPath(path ?? MapNetworkSettings.DefaultPath);
        networkSettings = MapNetworkSettings.Load(networkSettingsPath, out networkSettingsMessage);
        networkSettings.Apply(networkSettings.CreateProxy());
    }

    private void AddNetworkSettings(TableLayoutPanel source)
    {
        networkSummary = ModernTheme.Label(""); networkSummary.Name = "networkSummary";
        ModernTheme.Add(source, networkSummary, 40);
        var button = ModernTheme.Button("代理与连接设置…", "mapNetworkButton");
        button.Click += (s, e) =>
        {
            using var dialog = new MapNetworkDialog(networkSettings, SaveAndApplyNetworkSettings);
            dialog.ShowDialog(this);
        };
        ModernTheme.Add(source, button, 40);
        UpdateNetworkSummary();
    }

    internal string SaveAndApplyNetworkSettings(MapNetworkSettings settings)
    {
        if (!settings.TryValidate(out var error)) return error;
        System.Net.IWebProxy proxy;
        try
        {
            proxy = settings.CreateProxy();
            settings.Save(networkSettingsPath);
        }
        catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is SecurityException || e is CryptographicException)
        {
            return "无法保存连接设置。请检查 EXE 所在目录的写入权限。";
        }
        settings.Apply(proxy);
        networkSettings = settings; networkSettingsMessage = null;
        UpdateNetworkSummary();
        MainMap.ReloadMap();
        return null;
    }

    private void UpdateNetworkSummary()
    {
        if (networkSummary == null) return;
        networkSummary.Text = networkSettingsMessage ?? networkSettings.Summary;
        networkSummary.ForeColor = networkSettingsMessage == null ? ModernTheme.Accent : System.Drawing.Color.Firebrick;
    }
}
