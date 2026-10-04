using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Demo.WindowsForms;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.Projections;

internal static class NetworkChecks
{
    internal static void VerifySettings(string output)
    {
        var previousProxy = GMapProvider.WebProxy;
        bool previousSocks = GMapProvider.IsSocksProxy;
        int previousTimeout = GMapProvider.TimeoutMs;
        try
        {
            foreach (string invalid in new[] { "", "socks5://127.0.0.1:1080", "https://127.0.0.1:7890", "http://127.0.0.1:0",
                "http://127.0.0.1:99999", "http://user:password@127.0.0.1:7890", "http://127.0.0.1:7890/path", "http://127.0.0.1:7890?x=1" })
                Assert(!MapNetworkSettings.TryProxyAddress(invalid, out _, out _), "Reject invalid or unsupported proxy address");
            Assert(MapNetworkSettings.TryProxyAddress("127.0.0.1:7890", out var normalized, out _) &&
                normalized == new Uri("http://127.0.0.1:7890"), "Host and port accept an omitted HTTP scheme");
            Assert(MapNetworkSettings.TryProxyAddress("http://[::1]:7890", out _, out _), "IPv6 proxy address");
            Assert(!new MapNetworkSettings { TimeoutSeconds = 0 }.TryValidate(out _), "Validate timeout bounds");
            Assert(!new MapNetworkSettings { Mode = MapProxyMode.ManualHttp, Address = "127.0.0.1:7890", Password = "secret" }.TryValidate(out _), "Password requires a username");

            string path = Path.Combine(output, "network settings", "network.xml");
            var settings = new MapNetworkSettings { Mode = MapProxyMode.ManualHttp, Address = "127.0.0.1:7890",
                UserName = "smoke-user", Password = "synthetic-proxy-secret", TimeoutSeconds = 30 };
            settings.Save(path);
            Assert(!File.ReadAllText(path).Contains(settings.Password), "Password must not be stored as plaintext");
            var loaded = MapNetworkSettings.Load(path, out string message);
            Assert(message == null && loaded.Mode == settings.Mode && loaded.Address == "http://127.0.0.1:7890" &&
                loaded.Password == settings.Password && loaded.UserName == settings.UserName && loaded.TimeoutSeconds == 30,
                "Settings and encrypted credentials survive a reload");
            settings.TimeoutSeconds = 45; settings.Save(path);
            Assert(MapNetworkSettings.Load(path, out _).TimeoutSeconds == 45, "Existing settings update atomically");
            var manualProxy = loaded.CreateProxy();
            var credential = manualProxy.Credentials.GetCredential(new Uri(loaded.Address), "Basic");
            Assert(credential.UserName == loaded.UserName && credential.Password == loaded.Password, "Credentials belong to the proxy");
            loaded.Apply(manualProxy);
            Assert(GMapProvider.TimeoutMs == 30000 && !GMapProvider.IsSocksProxy, "Timeout and HTTP mode reach GMap.NET");

            var direct = new MapNetworkSettings();
            direct.Apply(direct.CreateProxy());
            Assert(GMapProvider.WebProxy.IsBypassed(new Uri("https://gmap-smoke.invalid/")), "Direct mode bypasses a system proxy");
            var system = new MapNetworkSettings { Mode = MapProxyMode.System };
            system.Apply(system.CreateProxy());
            Assert(GMapProvider.WebProxy != null && GMapProvider.WebProxy != EmptyWebProxy.Instance, "System mode uses Windows proxy settings");

            File.WriteAllText(path, "<MapNetwork><Mode>invalid</Mode></MapNetwork>");
            var fallback = MapNetworkSettings.Load(path, out message);
            Assert(fallback.Mode == MapProxyMode.Direct && message != null, "Malformed settings fall back with a visible message");
            string wrongPassword = Path.Combine(output, "unreadable-password.xml");
            File.WriteAllText(wrongPassword, "<MapNetwork><Mode>ManualHttp</Mode><Address>http://127.0.0.1:7890</Address><UserName>smoke-user</UserName><ProtectedPassword>not-base64</ProtectedPassword><TimeoutSeconds>15</TimeoutSeconds></MapNetwork>");
            var recovered = MapNetworkSettings.Load(wrongPassword, out message);
            Assert(recovered.Mode == MapProxyMode.ManualHttp && recovered.Password.Length == 0 && message != null, "Unreadable password preserves the proxy address");
            string expectedDefault = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings", "network.xml");
            Assert(MapNetworkSettings.DefaultPath == expectedDefault, "Network settings are executable-relative");
            VerifyHttpThroughProxy();
            Console.WriteLine("PASS proxy modes, validation, timeout, encrypted settings and actual GMap.NET HTTP tile request through a loopback proxy");
        }
        finally
        {
            GMapProvider.WebProxy = previousProxy;
            GMapProvider.IsSocksProxy = previousSocks;
            GMapProvider.TimeoutMs = previousTimeout;
        }
    }

    private static void VerifyHttpThroughProxy()
    {
        byte[] image;
        using (var bitmap = new Bitmap(256, 256))
        using (var graphics = Graphics.FromImage(bitmap))
        using (var stream = new MemoryStream())
        {
            graphics.Clear(Color.Teal); bitmap.Save(stream, ImageFormat.Png); image = stream.ToArray();
        }
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var response = Task.Run(() =>
        {
            using var client = listener.AcceptTcpClient();
            client.ReceiveTimeout = 5000; client.SendTimeout = 5000;
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.ASCII, false, 1024, true);
            string requestLine = reader.ReadLine();
            string line;
            do { line = reader.ReadLine(); } while (!string.IsNullOrEmpty(line));
            byte[] headers = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nContent-Type: image/png\r\nContent-Length: " + image.Length + "\r\nConnection: close\r\n\r\n");
            stream.Write(headers, 0, headers.Length); stream.Write(image, 0, image.Length); stream.Flush();
            return requestLine;
        });
        try
        {
            var settings = new MapNetworkSettings { Mode = MapProxyMode.ManualHttp, Address = "http://127.0.0.1:" + port, TimeoutSeconds = 5 };
            settings.Apply(settings.CreateProxy());
            using var tile = new HttpProbeProvider().GetTileImage(new GPoint(1, 2), 3);
            Assert(tile != null && tile.Data.Length > 0, "Actual GMap.NET HTTP request decoded the proxied tile");
            Assert(response.Wait(5000), "Local proxy response completed");
            Assert(response.Result.StartsWith("GET http://gmap-proxy-smoke.invalid/tiles/3/1/2.png "), "Proxy receives the absolute target URL");
        }
        finally
        {
            listener.Stop();
            try { response.Wait(1000); } catch (AggregateException) { }
        }
    }

    internal static void VerifyDialog(MainForm form, string output, string settingsPath)
    {
        Assert(DpiChecks.Find(form, "mapNetworkButton").Visible, "Proxy entry is visible in the map source card");
        using var dialog = new OffscreenNetworkDialog(form.NetworkSettings, form.SaveAndApplyNetworkSettings);
        dialog.StartPosition = FormStartPosition.Manual; dialog.Location = new Point(-20000, -20000);
        dialog.Show(); dialog.PerformLayout(); DpiChecks.Pump();
        Assert(Math.Abs(dialog.ClientSize.Width - 620 * dialog.DeviceDpi / 96.0) <= 1, "Proxy dialog scales once");
        var mode = (ComboBox)DpiChecks.Find(dialog, "proxyMode");
        var address = (TextBox)DpiChecks.Find(dialog, "proxyAddress");
        var password = (TextBox)DpiChecks.Find(dialog, "proxyPassword");
        var timeout = (NumericUpDown)DpiChecks.Find(dialog, "proxyTimeout");
        var save = (Button)DpiChecks.Find(dialog, "proxySave");
        Assert(!address.Enabled && password.UseSystemPasswordChar, "Direct mode disables proxy fields and masks passwords");
        mode.SelectedIndex = (int)MapProxyMode.ManualHttp;
        Assert(address.Enabled && password.Enabled, "Manual mode enables proxy fields");
        address.Text = "127.0.0.1:7890"; timeout.Value = 30;
        DpiChecks.Capture(dialog, Path.Combine(output, "ui-network-current-dpi.png"));
        address.Text = "socks5://127.0.0.1:1080"; save.PerformClick();
        Assert(dialog.Visible && DpiChecks.Find(dialog, "proxyMessage").Text.Contains("HTTP") &&
            form.NetworkSettings.Mode == MapProxyMode.Direct && !File.Exists(settingsPath), "Invalid address leaves current settings unchanged");
        address.Text = "127.0.0.1:7890"; timeout.Value = 30;
        DpiChecks.VerifyVisibleControls(dialog);
        dialog.ClientSize = new Size(560 * dialog.DeviceDpi / 96, 610 * dialog.DeviceDpi / 96);
        dialog.PerformLayout(); DpiChecks.Pump(); DpiChecks.VerifyVisibleControls(dialog);
        var selectedArea = form.MainMap.SelectedArea; var accessMode = form.MainMap.Manager.Mode;
        save.PerformClick();
        Assert(form.NetworkSettings.Mode == MapProxyMode.ManualHttp && File.Exists(settingsPath) &&
            GMapProvider.WebProxy.GetProxy(new Uri("https://gmap-smoke.invalid/")).Port == 7890 &&
            GMapProvider.TimeoutMs == 30000, "Save button persists settings and applies the proxy immediately");
        Assert(form.MainMap.SelectedArea == selectedArea && form.MainMap.Manager.Mode == accessMode, "Applying proxy preserves selection and access mode");
        var restored = MapNetworkSettings.Load(settingsPath, out _);
        Assert(restored.Mode == MapProxyMode.ManualHttp && restored.TimeoutSeconds == 30, "Dialog settings restore after reopening");
        Assert(DpiChecks.Find(form, "networkSummary").Text.Contains("HTTP"), "Main window reports the active connection mode");
        Assert(form.SaveAndApplyNetworkSettings(new MapNetworkSettings()) == null, "Reset synthetic tests to direct mode");
        Console.WriteLine("PASS native proxy dialog at " + form.DeviceDpi + " DPI, invalid-input rejection, save/apply and unchanged access mode");
    }

    private sealed class OffscreenNetworkDialog : MapNetworkDialog
    {
        internal OffscreenNetworkDialog(MapNetworkSettings settings, Func<MapNetworkSettings, string> apply) : base(settings, apply) { }
        protected override bool ShowWithoutActivation => true;
    }

    private sealed class HttpProbeProvider : GMapProvider
    {
        public override Guid Id => new Guid("1242a94b-3401-4ccc-8b25-904df2a5c6d4");
        public override string Name => "Loopback proxy HTTP probe";
        public override PureProjection Projection => MercatorProjection.Instance;
        public override GMapProvider[] Overlays => new GMapProvider[] { this };
        public override PureImage GetTileImage(GPoint position, int zoom) =>
            GetTileImageUsingHttp("http://gmap-proxy-smoke.invalid/tiles/" + zoom + "/" + position.X + "/" + position.Y + ".png");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
