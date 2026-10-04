using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Demo.WindowsForms;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.Projections;
using GMap.NET.WindowsForms;

internal static class Program
{
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            string output = Path.GetFullPath(args.Length == 0 ? "artifacts/smoke" : args[0]);
            Directory.CreateDirectory(output);
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);

            AssertVersion(typeof(GMaps).Assembly, "GMap.NET.Core");
            AssertVersion(typeof(GMapControl).Assembly, "GMap.NET.WindowsForms");
            using (var db = new SQLiteConnection("Data Source=:memory:"))
            {
                db.Open();
                using var command = db.CreateCommand();
                command.CommandText = "select sqlite_version()";
                Console.WriteLine("PASS native SQLite: " + command.ExecuteScalar());
            }

            GMaps.Instance.Mode = AccessMode.ServerOnly;
            var provider = new SyntheticProvider();
            CacheChecks.VerifyDefault(provider, output);
            using var form = new DpiChecks.OffscreenMainForm(provider, Path.Combine(output, "cache override"));
            Assert(form.MainMap.MapProvider == provider, "Injected offline map provider");
            CacheChecks.VerifyPaths(form, Path.Combine(output, "cache override"));
            Console.WriteLine("PASS explicit cache override retained");
            form.CreateControl();
            Console.WriteLine("PASS main form and resources constructed");
            DpiChecks.Verify(form, output);

            const int zoom = 14;
            var projection = provider.Projection;
            var origin = projection.FromLatLngToPixel(37.5, -122.3, zoom);
            var topLeft = projection.FromPixelToLatLng(origin.X, origin.Y, zoom);
            var bottomRight = projection.FromPixelToLatLng(origin.X + 513, origin.Y + 387, zoom);
            var area = RectLatLng.FromLTRB(topLeft.Lng, topLeft.Lat, bottomRight.Lng, bottomRight.Lat);
            form.MainMap.SelectedArea = area;
            form.MainMap.Zoom = zoom;
            form.MainMap.Position = topLeft;

            using (var export = new DpiChecks.OffscreenExportForm(form))
            {
                Assert(GetField<CheckBox>(export, "checkBoxWorldFile").Checked,
                    "CMANO world file remains enabled by default");
                DpiChecks.VerifyExportWindow(export, output);
                GetField<TextBox>(export, "textBoxSaveAs").Text = Path.Combine(output, "layer.png");
                GetField<TextBox>(export, "textBoxSizeFile").Text = "99";
                GetField<List<GPoint>>(export, "tileArea").AddRange(projection.GetAreaTileList(area, zoom, 1));
                var work = new DoWorkEventArgs(new MapInfo(area, zoom, provider, true, false));
                typeof(StaticImage).GetMethod("bg_DoWork", PrivateInstance).Invoke(export, new object[] { null, work });
                Assert(!work.Cancel, "Export was not cancelled");
            }

            using (var bitmap = new Bitmap(Path.Combine(output, "layer.png")))
            {
                Assert(bitmap.Width == 513 && bitmap.Height == 387, "Full source pixel dimensions retained");
                Assert(bitmap.GetPixel(100, 100).A == 255, "Export contains opaque map pixels");
                var worldFile = File.ReadAllLines(Path.Combine(output, "layer.pgw"));
                Assert(worldFile.Length == 6, "Six-line PGW output");
                var coefficients = Array.ConvertAll(worldFile, s => double.Parse(s, CultureInfo.InvariantCulture));
                Assert(coefficients[0] > 0 && coefficients[3] < 0, "PGW axis directions");
                Assert(Math.Abs(coefficients[0] * bitmap.Width - area.WidthLng) < 1e-8, "Unscaled longitude extent");
                Assert(Math.Abs(coefficients[3] * bitmap.Height + area.HeightLat) < 1e-8, "Unscaled latitude extent");
                Console.WriteLine("PASS actual exporter: PNG 513x387 and matching PGW");
            }
            Assert(provider.RequestCount > 0, "Actual tile retrieval ran");
            Console.WriteLine("PASS synthetic tile requests: " + provider.RequestCount);
            Console.WriteLine("No public map service was contacted. In-game alignment is a separate manual check.");
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            return 1;
        }
    }

    private static T GetField<T>(StaticImage value, string name) =>
        (T)typeof(StaticImage).GetField(name, PrivateInstance).GetValue(value);

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void AssertVersion(Assembly assembly, string name)
    {
        var version = assembly.GetName().Version;
        Assert(version.Major == 2 && version.Minor == 1 && version.Build == 7, name + " version mismatch: " + version);
        Console.WriteLine("PASS " + name + " " + version);
    }

    private sealed class SyntheticProvider : GMapProvider
    {
        public int RequestCount;
        public override Guid Id => new Guid("f61138ba-9432-4fd4-bb1a-a570c62bf405");
        public override string Name => "Local smoke-test tiles";
        public override PureProjection Projection => MercatorProjection.Instance;
        public override GMapProvider[] Overlays => new GMapProvider[] { this };

        public override PureImage GetTileImage(GPoint position, int zoom)
        {
            Interlocked.Increment(ref RequestCount);
            var bitmap = new Bitmap(256, 256, PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.FromArgb(255, (int)(position.X % 128) + 80, (int)(position.Y % 128) + 80, 120));
                graphics.DrawLine(Pens.White, 0, 0, 255, 255);
                graphics.DrawLine(Pens.Black, 0, 255, 255, 0);
            }
            var data = new MemoryStream();
            bitmap.Save(data, ImageFormat.Png);
            data.Position = 0;
            return new GMapImage { Img = bitmap, Data = data };
        }
    }
}
