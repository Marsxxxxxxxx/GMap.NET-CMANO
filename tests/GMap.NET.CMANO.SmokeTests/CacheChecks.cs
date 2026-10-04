using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using GMap.NET;
using GMap.NET.CacheProviders;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using Demo.WindowsForms;

internal static class CacheChecks
{
    internal static void VerifyDefault(GMapProvider provider, string output)
    {
        string originalDirectory = Environment.CurrentDirectory;
        string otherDirectory = Path.Combine(output, "different working directory");
        Directory.CreateDirectory(otherDirectory);
        try
        {
            Environment.CurrentDirectory = otherDirectory;
            using var form = new MainForm(provider);
            string expected = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache");
            VerifyPaths(form, expected);
            Assert(!Directory.Exists(Path.Combine(otherDirectory, "cache")),
                "The shortcut working directory must not determine the cache location");

            const int testType = 987654321;
            var position = new GPoint(2, 3);
            const int zoom = 4;
            byte[] tile;
            using (var bitmap = new Bitmap(16, 16))
            using (var stream = new MemoryStream())
            {
                using (var graphics = Graphics.FromImage(bitmap)) graphics.Clear(Color.CornflowerBlue);
                bitmap.Save(stream, ImageFormat.Png);
                tile = stream.ToArray();
            }
            var cache = GMaps.Instance.PrimaryCache;
            Assert(cache.PutImageToCache(tile, testType, position, zoom),
                "The portable SQLite cache must accept a tile");
            using (var restored = cache.GetImageFromCache(testType, position, zoom))
            {
                var image = restored as GMapImage;
                Assert(image != null && image.Img.Size == new Size(16, 16),
                    "The tile must be readable from the portable cache");
            }
            var sqlite = (SQLitePureImageCache)cache;
            Assert(File.Exists(Path.Combine(sqlite.GtileCache, GMapProvider.LanguageStr, "Data.gmdb")),
                "The SQLite tile database must exist under the executable cache folder");
            Console.WriteLine("PASS executable-relative default cache and actual SQLite tile round trip: " + expected);
        }
        finally
        {
            Environment.CurrentDirectory = originalDirectory;
        }
    }

    internal static void VerifyPaths(MainForm form, string expected)
    {
        Assert(SameDirectory(form.MainMap.CacheLocation, expected), "Map cache directory");
        Assert(Directory.Exists(expected), "Cache folder created automatically");
        Assert(form.MainMap.CacheLocation.EndsWith(Path.DirectorySeparatorChar.ToString()),
            "Legacy cache tools require a trailing directory separator");
        var sqlite = GMaps.Instance.PrimaryCache as SQLitePureImageCache;
        Assert(sqlite != null && SameDirectory(sqlite.CacheLocation, expected), "SQLite cache directory");
        var ipCache = (SQLiteIpCache)typeof(MainForm).GetField("IpCache", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
        Assert(SameDirectory(ipCache.CacheLocation, expected), "IP cache directory");
        Assert(File.Exists(Path.Combine(expected, "IpGeoCacheDB", "Data.ipdb")), "IP database created in the selected cache");
    }

    private static bool SameDirectory(string first, string second) =>
        string.Equals(Path.GetFullPath(first).TrimEnd(Path.DirectorySeparatorChar),
            Path.GetFullPath(second).TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
