using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using GMap.NET.MapProviders;

namespace Demo.WindowsForms;

internal enum MapProxyMode { Direct, System, ManualHttp }

internal sealed class MapNetworkSettings
{
    internal MapProxyMode Mode { get; set; }
    internal string Address { get; set; } = "";
    internal string UserName { get; set; } = "";
    internal string Password { get; set; } = "";
    internal int TimeoutSeconds { get; set; } = 15;
    internal static string DefaultPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings", "network.xml");

    internal bool TryValidate(out string error)
    {
        error = null;
        if (!Enum.IsDefined(typeof(MapProxyMode), Mode)) error = "请选择有效的连接方式。";
        else if (TimeoutSeconds < 5 || TimeoutSeconds > 120) error = "连接超时应为 5～120 秒。";
        else if (Mode == MapProxyMode.ManualHttp)
        {
            if (!TryProxyAddress(Address, out _, out error)) return false;
            if (Password.Length > 0 && string.IsNullOrWhiteSpace(UserName)) error = "填写代理密码时，也需要填写用户名。";
        }
        return error == null;
    }

    internal static bool TryProxyAddress(string text, out Uri address, out string error)
    {
        address = null; error = null;
        string candidate = (text ?? "").Trim();
        if (candidate.Length == 0) { error = "请输入代理地址，例如 http://127.0.0.1:7890。"; return false; }
        if (!candidate.Contains("://")) candidate = "http://" + candidate;
        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttp ||
            string.IsNullOrEmpty(uri.Host) || uri.Port < 1 || uri.Port > 65535 || uri.AbsolutePath != "/" ||
            uri.Query.Length > 0 || uri.Fragment.Length > 0 || uri.UserInfo.Length > 0 ||
            !uri.IsWellFormedOriginalString())
        {
            error = "请填写 HTTP 代理地址（主机:端口或 http://主机:端口）。认证请填写下方用户名和密码；不要使用 SOCKS 地址。";
            return false;
        }
        address = uri;
        return true;
    }

    internal IWebProxy CreateProxy()
    {
        if (!TryValidate(out var error)) throw new ArgumentException(error);
        if (Mode == MapProxyMode.Direct) return EmptyWebProxy.Instance;
        if (Mode == MapProxyMode.System)
        {
            var systemProxy = WebRequest.GetSystemWebProxy();
            systemProxy.Credentials = CredentialCache.DefaultNetworkCredentials;
            return systemProxy;
        }
        TryProxyAddress(Address, out var address, out _);
        return new WebProxy(address, false)
        {
            Credentials = string.IsNullOrWhiteSpace(UserName) ? null : new NetworkCredential(UserName, Password)
        };
    }

    internal void Apply(IWebProxy proxy)
    {
        // GMap.NET uses these globals for tiles, geocoding, routes and export downloads.
        GMapProvider.WebProxy = proxy;
        GMapProvider.IsSocksProxy = false;
        GMapProvider.TimeoutMs = checked(TimeoutSeconds * 1000);
    }

    internal string Summary
    {
        get
        {
            string connection = Mode == MapProxyMode.Direct ? "直连" : Mode == MapProxyMode.System ? "Windows 系统代理" : "HTTP 代理";
            return connection + "  ·  超时 " + TimeoutSeconds + " 秒";
        }
    }

    internal void Save(string path)
    {
        if (!TryValidate(out var error)) throw new ArgumentException(error);
        string savedAddress = "";
        if (TryProxyAddress(Address, out var address, out _)) savedAddress = address.GetLeftPart(UriPartial.Authority);
        string protectedPassword = Password.Length == 0 ? "" : Convert.ToBase64String(
            ProtectedData.Protect(Encoding.UTF8.GetBytes(Password), null, DataProtectionScope.CurrentUser));
        var document = new XDocument(new XElement("MapNetwork",
            new XElement("Mode", Mode.ToString()), new XElement("Address", savedAddress),
            new XElement("UserName", UserName), new XElement("ProtectedPassword", protectedPassword),
            new XElement("TimeoutSeconds", TimeoutSeconds.ToString(CultureInfo.InvariantCulture))));
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
        string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            document.Save(temporary);
            if (File.Exists(path)) File.Replace(temporary, path, null);
            else File.Move(temporary, path);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    internal static MapNetworkSettings Load(string path, out string message)
    {
        message = null;
        if (!File.Exists(path)) return new MapNetworkSettings();
        try
        {
            using var reader = XmlReader.Create(path, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit });
            var root = XDocument.Load(reader).Root;
            if (root?.Name != "MapNetwork" || !Enum.TryParse((string)root.Element("Mode"), out MapProxyMode mode) ||
                !int.TryParse((string)root.Element("TimeoutSeconds"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int timeout))
                throw new FormatException("Invalid network settings.");
            var settings = new MapNetworkSettings
            {
                Mode = mode, Address = (string)root.Element("Address") ?? "",
                UserName = (string)root.Element("UserName") ?? "", TimeoutSeconds = timeout
            };
            string password = (string)root.Element("ProtectedPassword") ?? "";
            if (password.Length > 0)
            {
                try { settings.Password = Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(password), null, DataProtectionScope.CurrentUser)); }
                catch (Exception e) when (e is CryptographicException || e is FormatException)
                { message = "已加载代理地址；保存的密码无法读取，请在连接设置中重新填写。"; }
            }
            if (!settings.TryValidate(out _)) throw new FormatException("Invalid network settings.");
            return settings;
        }
        catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is XmlException || e is FormatException || e is System.Security.SecurityException)
        {
            message = "连接设置读取失败，已暂用直连；请打开「代理与连接设置」重新保存。";
            return new MapNetworkSettings();
        }
    }
}
