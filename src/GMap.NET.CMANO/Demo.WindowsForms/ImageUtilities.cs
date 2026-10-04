using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Demo.WindowsForms;

public static class ImageUtilities
{
	private static Dictionary<string, ImageCodecInfo> encoders = null;

	private static object encodersLock = new object();

	public static Dictionary<string, ImageCodecInfo> Encoders
	{
		get
		{
			if (encoders == null)
			{
				lock (encodersLock)
				{
					if (encoders == null)
					{
						encoders = new Dictionary<string, ImageCodecInfo>();
						ImageCodecInfo[] imageEncoders = ImageCodecInfo.GetImageEncoders();
						foreach (ImageCodecInfo imageCodecInfo in imageEncoders)
						{
							encoders.Add(imageCodecInfo.MimeType.ToLower(), imageCodecInfo);
						}
					}
				}
			}
			return encoders;
		}
	}

	public static Bitmap ResizeImage(Image image, int width, int height)
	{
		Bitmap bitmap = new Bitmap(width, height);
		bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);
		using Graphics graphics = Graphics.FromImage(bitmap);
		graphics.CompositingQuality = CompositingQuality.HighQuality;
		graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
		graphics.SmoothingMode = SmoothingMode.HighQuality;
		graphics.DrawImage(image, 0, 0, bitmap.Width, bitmap.Height);
		return bitmap;
	}

	public static void SavePng(string path, Image image)
	{
		image.Save(path, ImageFormat.Png);
	}

	public static ImageCodecInfo GetEncoderInfo(string mimeType)
	{
		string key = mimeType.ToLower();
		ImageCodecInfo result = null;
		if (Encoders.ContainsKey(key))
		{
			result = Encoders[key];
		}
		return result;
	}
}
