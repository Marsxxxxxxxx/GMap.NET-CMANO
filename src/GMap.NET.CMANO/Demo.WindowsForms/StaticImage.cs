using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;

namespace Demo.WindowsForms;

public partial class StaticImage : Form
{
	private MainForm Main;

	private BackgroundWorker bg = new BackgroundWorker();

	private readonly List<GPoint> tileArea = new List<GPoint>();

	private readonly List<PointLatLng> GpxRoute = new List<PointLatLng>();

	private RectLatLng AreaGpx = RectLatLng.Empty;

	private IContainer components;

	private Button button1;

	private ProgressBar progressBar1;

	private NumericUpDown numericUpDown1;

	private Button button2;

	private Label label1;

	private CheckBox checkBoxWorldFile;

	private CheckBox checkBoxRoutes;

	private CheckBox checkBoxKMZ;

	private TextBox textBoxSizeFile;

	private Label label2;

	private SaveFileDialog saveFileDialog;

	private TextBox textBoxSaveAs;

	private Button btnSave;

	public StaticImage(MainForm main)
	{
		Main = main;
		InitializeComponent();
		numericUpDown1.Maximum = Main.MainMap.MaxZoom;
		numericUpDown1.Minimum = Main.MainMap.MinZoom;
		numericUpDown1.Value = Math.Max(numericUpDown1.Minimum, Math.Min(numericUpDown1.Maximum, (decimal)Math.Round(Main.MainMap.Zoom)));
		UpdateExportPreview();
		bg.WorkerReportsProgress = true;
		bg.WorkerSupportsCancellation = true;
		bg.DoWork += bg_DoWork;
		bg.ProgressChanged += bg_ProgressChanged;
		bg.RunWorkerCompleted += bg_RunWorkerCompleted;
	}

	private void bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		if (!e.Cancelled)
		{
			if (e.Error != null)
			{
				exportStatus.Text = "导出失败：" + e.Error.Message;
			}
			else
			{
				_ = e.Result;
			}
		}
		Text = "导出地图叠加层 — GMap.NET CMANO";
		progressBar1.Value = 0;
		button1.Enabled = true;
		SetExportBusy(false);
		UpdateExportPreview();
		exportStatus.Text = e.Cancelled ? "导出已取消" : e.Error != null ? "导出失败：" + e.Error.Message : "导出完成 · 文件已保存到指定目录";
		progressBar1.Value = e.Cancelled || e.Error != null ? 0 : 100;
		Main.MainMap.HoldInvalidation = false;
		Main.MainMap.Refresh();
		if (closeAfterExport) Close();
	}

	private void bg_ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		if (!bg.IsBusy) return;
		progressBar1.Value = e.ProgressPercentage;
		GPoint gPoint = (GPoint)e.UserState;
		exportStatus.Text = "正在下载地图瓦片  ·  " + e.ProgressPercentage + "%";
	}

	private void bg_DoWork(object sender, DoWorkEventArgs e)
	{
		MapInfo mapInfo = (MapInfo)e.Argument;
		string outputPath = mapInfo.OutputFileName ?? textBoxSaveAs.Text;
		string sizeLimitText = mapInfo.SizeLimitMegabytes?.ToString(CultureInfo.InvariantCulture) ?? textBoxSizeFile.Text;
		if (mapInfo.Area.IsEmpty)
		{
			return;
		}
		string text = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), Path.DirectorySeparatorChar.ToString(), "GMap at zoom ", mapInfo.Zoom, " - ", mapInfo.Type, "-", DateTime.Now.Ticks, ".png");
		if (!string.IsNullOrEmpty(outputPath))
		{
			text = Path.Combine(Path.GetDirectoryName(outputPath), Path.GetFileName(text));
		}
		string text2 = Path.Combine(Path.GetDirectoryName(text), "layer.png");
		if (!string.IsNullOrEmpty(outputPath))
		{
			text2 = outputPath;
		}
		e.Result = text;
		GPoint gPoint = mapInfo.Type.Projection.FromLatLngToPixel(mapInfo.Area.LocationTopLeft, mapInfo.Zoom);
		GPoint gPoint2 = mapInfo.Type.Projection.FromLatLngToPixel(mapInfo.Area.Bottom, mapInfo.Area.Right, mapInfo.Zoom);
		checked
		{
			GPoint gPoint3 = new GPoint(gPoint2.X - gPoint.X, gPoint2.Y - gPoint.Y);
			GSize tileMatrixMaxXY = mapInfo.Type.Projection.GetTileMatrixMaxXY(mapInfo.Zoom);
			int num = 0;
			int num2 = ((!mapInfo.MakeWorldFile && !mapInfo.MakeKmz) ? 22 : 0);
			using (Bitmap bitmap = new Bitmap((int)(gPoint3.X + num2 * 2), (int)(gPoint3.Y + num2 * 2)))
			{
				using (Graphics graphics = Graphics.FromImage(bitmap))
				{
					graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
					graphics.SmoothingMode = SmoothingMode.HighQuality;
					int num3 = 0;
					lock (tileArea)
					{
						foreach (GPoint item in tileArea)
						{
							if (bg.CancellationPending)
							{
								e.Cancel = true;
								return;
							}
							int percentProgress = (int)((double)(++num3) / (double)tileArea.Count * 100.0);
							bg.ReportProgress(percentProgress, item);
							GMapProvider[] overlays = mapInfo.Type.Overlays;
							foreach (GMapProvider gMapProvider in overlays)
							{
								GMapImage gMapImage = ((!gMapProvider.InvertedAxisY) ? (GMaps.Instance.GetImageFrom(gMapProvider, item, mapInfo.Zoom, out var result) as GMapImage) : (GMaps.Instance.GetImageFrom(gMapProvider, new GPoint(item.X, tileMatrixMaxXY.Height - item.Y), mapInfo.Zoom, out result) as GMapImage));
								if (gMapImage != null)
								{
									using (gMapImage)
									{
										long num4 = item.X * mapInfo.Type.Projection.TileSize.Width - gPoint.X + num2;
										long num5 = item.Y * mapInfo.Type.Projection.TileSize.Width - gPoint.Y + num2;
										graphics.DrawImage(gMapImage.Img, num4, num5, mapInfo.Type.Projection.TileSize.Width, mapInfo.Type.Projection.TileSize.Height);
									}
								}
							}
						}
					}
					foreach (GMapRoute route in Main.routes.Routes)
					{
						if (!route.IsVisible)
						{
							continue;
						}
						using GraphicsPath graphicsPath = new GraphicsPath();
						for (int j = 0; j < route.Points.Count; j++)
						{
							PointLatLng pointLatLng = route.Points[j];
							GPoint gPoint4 = mapInfo.Type.Projection.FromLatLngToPixel(pointLatLng.Lat, pointLatLng.Lng, mapInfo.Zoom);
							gPoint4.Offset(num2, num2);
							gPoint4.Offset(-gPoint.X, -gPoint.Y);
							GPoint gPoint5 = gPoint4;
							if (j == 0)
							{
								graphicsPath.AddLine(gPoint5.X, gPoint5.Y, gPoint5.X, gPoint5.Y);
								continue;
							}
							PointF lastPoint = graphicsPath.GetLastPoint();
							graphicsPath.AddLine(lastPoint.X, lastPoint.Y, gPoint5.X, gPoint5.Y);
						}
						if (graphicsPath.PointCount > 0)
						{
							graphics.DrawPath(route.Stroke, graphicsPath);
						}
					}
					foreach (GMapPolygon polygon in Main.polygons.Polygons)
					{
						if (!polygon.IsVisible)
						{
							continue;
						}
						using GraphicsPath graphicsPath2 = new GraphicsPath();
						for (int k = 0; k < polygon.Points.Count; k++)
						{
							PointLatLng pointLatLng2 = polygon.Points[k];
							GPoint gPoint6 = mapInfo.Type.Projection.FromLatLngToPixel(pointLatLng2.Lat, pointLatLng2.Lng, mapInfo.Zoom);
							gPoint6.Offset(num2, num2);
							gPoint6.Offset(-gPoint.X, -gPoint.Y);
							GPoint gPoint7 = gPoint6;
							if (k == 0)
							{
								graphicsPath2.AddLine(gPoint7.X, gPoint7.Y, gPoint7.X, gPoint7.Y);
								continue;
							}
							PointF lastPoint2 = graphicsPath2.GetLastPoint();
							graphicsPath2.AddLine(lastPoint2.X, lastPoint2.Y, gPoint7.X, gPoint7.Y);
						}
						if (graphicsPath2.PointCount > 0)
						{
							graphicsPath2.CloseFigure();
							graphics.FillPath(polygon.Fill, graphicsPath2);
							graphics.DrawPath(polygon.Stroke, graphicsPath2);
						}
					}
					foreach (GMapMarker marker in Main.objects.Markers)
					{
						if (marker.IsVisible)
						{
							PointLatLng position = marker.Position;
							GPoint gPoint8 = mapInfo.Type.Projection.FromLatLngToPixel(position.Lat, position.Lng, mapInfo.Zoom);
							gPoint8.Offset(num2, num2);
							gPoint8.Offset(-gPoint.X, -gPoint.Y);
							gPoint8.Offset(marker.Offset.X, marker.Offset.Y);
							graphics.ResetTransform();
							graphics.TranslateTransform(-marker.LocalPosition.X, -marker.LocalPosition.Y);
							graphics.TranslateTransform((int)gPoint8.X, (int)gPoint8.Y);
							marker.OnRender(graphics);
						}
					}
					foreach (GMapMarker marker2 in Main.objects.Markers)
					{
						if (marker2.IsVisible && marker2.ToolTip != null && marker2.IsVisible && !string.IsNullOrEmpty(marker2.ToolTipText))
						{
							PointLatLng position2 = marker2.Position;
							GPoint gPoint9 = mapInfo.Type.Projection.FromLatLngToPixel(position2.Lat, position2.Lng, mapInfo.Zoom);
							gPoint9.Offset(num2, num2);
							gPoint9.Offset(-gPoint.X, -gPoint.Y);
							gPoint9.Offset(marker2.Offset.X, marker2.Offset.Y);
							graphics.ResetTransform();
							graphics.TranslateTransform(-marker2.LocalPosition.X, -marker2.LocalPosition.Y);
							graphics.TranslateTransform((int)gPoint9.X, (int)gPoint9.Y);
							marker2.ToolTip.OnRender(graphics);
						}
					}
					graphics.ResetTransform();
					if (!mapInfo.MakeWorldFile)
					{
						Rectangle rect = new Rectangle
						{
							Location = new Point(num2, num2),
							Size = new Size((int)gPoint3.X, (int)gPoint3.Y)
						};
						using Font font = new Font(FontFamily.GenericSansSerif, 9f, FontStyle.Bold);
						using (Pen pen = new Pen(Brushes.DimGray, 3f))
						{
							pen.DashStyle = DashStyle.DashDot;
							graphics.DrawRectangle(pen, rect);
							string s = mapInfo.Area.LocationTopLeft.ToString();
							SizeF sizeF = graphics.MeasureString(s, font);
							graphics.DrawString(s, font, pen.Brush, (float)rect.X + sizeF.Height / 2f, (float)rect.Y + sizeF.Height / 2f);
							string s2 = new PointLatLng(mapInfo.Area.Bottom, mapInfo.Area.Right).ToString();
							SizeF sizeF2 = graphics.MeasureString(s2, font);
							graphics.DrawString(s2, font, pen.Brush, (float)rect.Right - sizeF2.Width - sizeF2.Height / 2f, (float)rect.Bottom - sizeF2.Height - sizeF2.Height / 2f);
						}
						using Pen pen2 = new Pen(Brushes.Blue, 1f);
						double groundResolution = mapInfo.Type.Projection.GetGroundResolution(mapInfo.Zoom, mapInfo.Area.Bottom);
						int num6 = (int)(100.0 / groundResolution);
						int num7 = (int)(1000.0 / groundResolution);
						graphics.DrawRectangle(pen2, rect.X + 10, rect.Bottom - 20, num7, 10);
						graphics.DrawRectangle(pen2, rect.X + 10, rect.Bottom - 20, num6, 10);
						string s3 = "scale: 100m | 1Km";
						SizeF sizeF3 = graphics.MeasureString(s3, font);
						graphics.DrawString(s3, font, pen2.Brush, rect.X + 10, (float)rect.Bottom - sizeF3.Height - 20f);
					}
				}
				ImageUtilities.SavePng(text, bitmap);
				Image image = null;
				unchecked
				{
					try
					{
						image = bitmap;
						if (long.TryParse(sizeLimitText, out var result2))
						{
							long length = new FileInfo(text).Length;
							if (length > 0 && length / 1024 / 1024 > result2)
							{
								while (num <= 100)
								{
									num = checked(num + 1);
									int num8 = checked(image.Width * 80) / 100;
									int num9 = checked(image.Height * 80) / 100;
									image = Stuff.FixedSize(image, num8, num9);
									ImageUtilities.SavePng(text2, image);
									length = new FileInfo(text2).Length;
									if (length > 0 && length / 1024 / 1024 < result2)
									{
										break;
									}
								}
							}
							else
							{
								ImageUtilities.SavePng(text2, image);
							}
						}
						else
						{
							throw new InvalidOperationException("文件大小上限无效。");
						}
					}
					catch (Exception ex)
					{
						throw new IOException("无法写入输出文件：" + ex.Message, ex);
					}
					finally
					{
						image.Dispose();
					}
				}
			}
			if (mapInfo.MakeWorldFile)
			{
				using (StreamWriter streamWriter = File.CreateText(Path.ChangeExtension(text, "pgw")))
				{
					streamWriter.WriteLine("{0:0.000000000000}", mapInfo.Area.WidthLng / (double)gPoint3.X);
					streamWriter.WriteLine("0.0000000");
					streamWriter.WriteLine("0.0000000");
					streamWriter.WriteLine("{0:0.000000000000}", (0.0 - mapInfo.Area.HeightLat) / (double)gPoint3.Y);
					streamWriter.WriteLine("{0:0.000000000000}", mapInfo.Area.Left);
					streamWriter.WriteLine("{0:0.000000000000}", mapInfo.Area.Top);
					streamWriter.Close();
				}
				using StreamWriter streamWriter2 = File.CreateText(Path.ChangeExtension(text2, "pgw"));
				streamWriter2.WriteLine("{0:0.000000000000}", mapInfo.Area.WidthLng / (double)gPoint3.X * ((num > 0) ? Math.Pow(1.25, num) : 1.0));
				streamWriter2.WriteLine("0.0000000");
				streamWriter2.WriteLine("0.0000000");
				streamWriter2.WriteLine("{0:0.000000000000}", (0.0 - mapInfo.Area.HeightLat) / (double)gPoint3.Y * ((num > 0) ? Math.Pow(1.25, num) : 1.0));
				streamWriter2.WriteLine("{0:0.000000000000}", mapInfo.Area.Left);
				streamWriter2.WriteLine("{0:0.000000000000}", mapInfo.Area.Top);
				streamWriter2.Close();
			}
			if (!mapInfo.MakeKmz)
			{
				return;
			}
			string text3 = (string)(e.Result = Path.GetDirectoryName(text) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(text) + ".kmz");
			using ZipStorer zipStorer = ZipStorer.Create(text3, "GMap.NET");
			zipStorer.AddFile(ZipStorer.Compression.Store, text, "files/map.jpg", "map");
			using MemoryStream source = new MemoryStream(Encoding.UTF8.GetBytes(string.Format(CultureInfo.InvariantCulture, "<?xml version=\"1.0\" encoding=\"UTF-8\"?> \r\n<kml xmlns=\"http://www.opengis.net/kml/2.2\" xmlns:gx=\"http://www.google.com/kml/ext/2.2\" xmlns:kml=\"http://www.opengis.net/kml/2.2\" xmlns:atom=\"http://www.w3.org/2005/Atom\">\r\n<GroundOverlay>\r\n\t<name>{8}</name>\r\n\t<LookAt>\r\n\t\t<longitude>{6}</longitude>\r\n\t\t<latitude>{7}</latitude>\r\n\t\t<altitude>0</altitude>\r\n\t\t<heading>0</heading>\r\n\t\t<tilt>0</tilt>\r\n\t\t<range>69327.55500845652</range>\r\n\t</LookAt>\r\n\t<color>91ffffff</color>\r\n\t<Icon>\r\n\t\t<href>files/map.jpg</href>\r\n\t</Icon>\r\n\t<gx:LatLonQuad>\r\n\t\t<coordinates>\r\n\t\t\t{0},{1},0 {2},{3},0 {4},{5},0 {6},{7},0 \r\n\t\t</coordinates>\r\n\t</gx:LatLonQuad>\r\n</GroundOverlay>\r\n</kml>", mapInfo.Area.Left, mapInfo.Area.Bottom, mapInfo.Area.Right, mapInfo.Area.Bottom, mapInfo.Area.Right, mapInfo.Area.Top, mapInfo.Area.Left, mapInfo.Area.Top, text3)));
			zipStorer.AddStream(ZipStorer.Compression.Store, "doc.kml", source, DateTime.Now, "kml");
			zipStorer.Close();
		}
	}

	private void button1_Click(object sender, EventArgs e)
	{
		UpdateExportPreview();
		if (!button1.Enabled) return;
		RectLatLng? rectLatLng = null;
		if (checkBoxRoutes.Checked)
		{
			rectLatLng = Main.MainMap.GetRectOfAllRoutes(null);
			if (!rectLatLng.HasValue)
			{
				MessageBox.Show("No routes in map", "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}
		}
		else
		{
			rectLatLng = Main.MainMap.SelectedArea;
			if (rectLatLng.Value.IsEmpty)
			{
				MessageBox.Show("Select map area holding ALT", "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}
		}
		if (!bg.IsBusy)
		{
			lock (tileArea)
			{
				tileArea.Clear();
				tileArea.AddRange(Main.MainMap.MapProvider.Projection.GetAreaTileList(rectLatLng.Value, (int)numericUpDown1.Value, 1));
				tileArea.TrimExcess();
			}
			numericUpDown1.Enabled = false;
			progressBar1.Value = 0;
			SetExportBusy(true);
			Main.MainMap.HoldInvalidation = true;
			bg.RunWorkerAsync(new MapInfo(rectLatLng.Value, (int)numericUpDown1.Value, Main.MainMap.MapProvider, checkBoxWorldFile.Checked, checkBoxKMZ.Checked, textBoxSaveAs.Text, long.Parse(textBoxSizeFile.Text, CultureInfo.InvariantCulture)));
		}
	}

	private void button2_Click(object sender, EventArgs e)
	{
		if (bg.IsBusy)
		{
			bg.CancelAsync();
			exportStatus.Text = "正在取消导出，请稍候…";
		}
		else Close();
	}

	private void btnSave_Click(object sender, EventArgs e)
	{
		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			textBoxSaveAs.Text = saveFileDialog.FileName;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		if (disposing) DisposeModernExport();
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.button1 = new System.Windows.Forms.Button();
		this.progressBar1 = new System.Windows.Forms.ProgressBar();
		this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
		this.button2 = new System.Windows.Forms.Button();
		this.label1 = new System.Windows.Forms.Label();
		this.checkBoxWorldFile = new System.Windows.Forms.CheckBox();
		this.checkBoxRoutes = new System.Windows.Forms.CheckBox();
		this.checkBoxKMZ = new System.Windows.Forms.CheckBox();
		this.textBoxSizeFile = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
		this.textBoxSaveAs = new System.Windows.Forms.TextBox();
		this.btnSave = new System.Windows.Forms.Button();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown1).BeginInit();
		base.SuspendLayout();
		this.button1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.button1.Location = new System.Drawing.Point(366, 29);
		this.button1.Margin = new System.Windows.Forms.Padding(2);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(84, 32);
		this.button1.TabIndex = 1;
		this.button1.Text = "Generate";
		this.button1.UseVisualStyleBackColor = true;
		this.button1.Click += new System.EventHandler(this.button1_Click);
		this.progressBar1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.progressBar1.Location = new System.Drawing.Point(6, 6);
		this.progressBar1.Margin = new System.Windows.Forms.Padding(2);
		this.progressBar1.Name = "progressBar1";
		this.progressBar1.Size = new System.Drawing.Size(353, 93);
		this.progressBar1.TabIndex = 2;
		this.numericUpDown1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.numericUpDown1.Location = new System.Drawing.Point(404, 6);
		this.numericUpDown1.Margin = new System.Windows.Forms.Padding(2);
		this.numericUpDown1.Name = "numericUpDown1";
		this.numericUpDown1.Size = new System.Drawing.Size(47, 20);
		this.numericUpDown1.TabIndex = 3;
		this.numericUpDown1.Value = new decimal(new int[4] { 17, 0, 0, 0 });
		this.button2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.button2.Location = new System.Drawing.Point(366, 67);
		this.button2.Margin = new System.Windows.Forms.Padding(2);
		this.button2.Name = "button2";
		this.button2.Size = new System.Drawing.Size(84, 32);
		this.button2.TabIndex = 4;
		this.button2.Text = "Cancel";
		this.button2.UseVisualStyleBackColor = true;
		this.button2.Click += new System.EventHandler(this.button2_Click);
		this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(363, 8);
		this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(37, 13);
		this.label1.TabIndex = 5;
		this.label1.Text = "Zoom:";
		this.checkBoxWorldFile.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.checkBoxWorldFile.AutoSize = true;
		this.checkBoxWorldFile.Checked = true;
		this.checkBoxWorldFile.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkBoxWorldFile.Location = new System.Drawing.Point(189, 115);
		this.checkBoxWorldFile.Name = "checkBoxWorldFile";
		this.checkBoxWorldFile.Size = new System.Drawing.Size(67, 17);
		this.checkBoxWorldFile.TabIndex = 6;
		this.checkBoxWorldFile.Text = "Worldfile";
		this.checkBoxWorldFile.UseVisualStyleBackColor = true;
		this.checkBoxRoutes.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.checkBoxRoutes.AutoSize = true;
		this.checkBoxRoutes.Location = new System.Drawing.Point(7, 115);
		this.checkBoxRoutes.Name = "checkBoxRoutes";
		this.checkBoxRoutes.Size = new System.Drawing.Size(147, 17);
		this.checkBoxRoutes.TabIndex = 7;
		this.checkBoxRoutes.Text = "Use area of routes in map";
		this.checkBoxRoutes.UseVisualStyleBackColor = true;
		this.checkBoxKMZ.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.checkBoxKMZ.AutoSize = true;
		this.checkBoxKMZ.Location = new System.Drawing.Point(291, 115);
		this.checkBoxKMZ.Name = "checkBoxKMZ";
		this.checkBoxKMZ.Size = new System.Drawing.Size(49, 17);
		this.checkBoxKMZ.TabIndex = 8;
		this.checkBoxKMZ.Text = "KMZ";
		this.checkBoxKMZ.UseVisualStyleBackColor = true;
		this.textBoxSizeFile.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.textBoxSizeFile.Location = new System.Drawing.Point(422, 113);
		this.textBoxSizeFile.MaxLength = 2;
		this.textBoxSizeFile.Name = "textBoxSizeFile";
		this.textBoxSizeFile.Size = new System.Drawing.Size(28, 20);
		this.textBoxSizeFile.TabIndex = 9;
		this.textBoxSizeFile.Text = "10";
		this.textBoxSizeFile.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.label2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(363, 116);
		this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(54, 13);
		this.label2.TabIndex = 10;
		this.label2.Text = "Size (Mo):";
		this.saveFileDialog.DefaultExt = "png";
		this.saveFileDialog.Filter = "png|*.png";
		this.saveFileDialog.RestoreDirectory = true;
		this.saveFileDialog.Title = "Save as";
		this.textBoxSaveAs.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.textBoxSaveAs.Location = new System.Drawing.Point(7, 139);
		this.textBoxSaveAs.Name = "textBoxSaveAs";
		this.textBoxSaveAs.ReadOnly = true;
		this.textBoxSaveAs.Size = new System.Drawing.Size(352, 20);
		this.textBoxSaveAs.TabIndex = 11;
		this.btnSave.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.btnSave.Location = new System.Drawing.Point(366, 138);
		this.btnSave.Name = "btnSave";
		this.btnSave.Size = new System.Drawing.Size(84, 23);
		this.btnSave.TabIndex = 12;
		this.btnSave.Text = "Set file";
		this.btnSave.UseVisualStyleBackColor = true;
		this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
		base.ClientSize = new System.Drawing.Size(457, 173);
		base.Controls.Add(this.btnSave);
		base.Controls.Add(this.textBoxSaveAs);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.textBoxSizeFile);
		base.Controls.Add(this.checkBoxKMZ);
		base.Controls.Add(this.checkBoxRoutes);
		base.Controls.Add(this.checkBoxWorldFile);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.button2);
		base.Controls.Add(this.numericUpDown1);
		base.Controls.Add(this.progressBar1);
		base.Controls.Add(this.button1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
		base.Margin = new System.Windows.Forms.Padding(2);
		this.MinimumSize = new System.Drawing.Size(16, 164);
		base.Name = "StaticImage";
		base.Padding = new System.Windows.Forms.Padding(4);
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "导出地图叠加层 — GMap.NET CMANO";
		((System.ComponentModel.ISupportInitialize)this.numericUpDown1).EndInit();
		InitializeModernExport();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
