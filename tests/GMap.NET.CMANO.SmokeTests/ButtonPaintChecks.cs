using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Demo.WindowsForms;

internal static class ButtonPaintChecks
{
    internal static void Verify(MainForm form, string output)
    {
        foreach (string name in new[] { "navigationMarkers", "navigationTools", "button1", "applyAreaButton", "button13" })
        {
            var button = (WorkspaceButton)DpiChecks.Find(form, name);
            bool originalEnabled = button.Enabled;
            bool originalSelected = button.Selected;
            try
            {
                button.Enabled = true;
                VerifyPaint(button, output, "normal");
                button.Selected = true;
                VerifyPaint(button, output, "selected");
                button.Selected = originalSelected;
                button.Enabled = false;
                VerifyPaint(button, output, "disabled");
            }
            finally { button.Enabled = originalEnabled; button.Selected = originalSelected; }
        }
        Console.WriteLine("PASS rounded button corners match the parent background at " + form.DeviceDpi + " DPI (normal, selected and disabled)");
    }

    private static void VerifyPaint(WorkspaceButton button, string output, string state)
    {
        using var bitmap = new Bitmap(button.Width, button.Height);
        bitmap.SetResolution(button.DeviceDpi, button.DeviceDpi);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            // DrawToBitmap also erases the background and can hide unpainted corners.
            // Seed a dirty paint buffer and exercise the actual foreground paint path.
            graphics.Clear(Color.Black);
            typeof(WorkspaceButton).GetMethod("OnPaint", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(button, new object[] { new PaintEventArgs(graphics, button.ClientRectangle) });
        }
        if (button.Name == "navigationTools" && state == "normal")
            bitmap.Save(Path.Combine(output, "ui-button-corners.png"), ImageFormat.Png);
        foreach (var corner in new[] { Point.Empty, new Point(bitmap.Width - 1, 0), new Point(0, bitmap.Height - 1), new Point(bitmap.Width - 1, bitmap.Height - 1) })
        {
            if (bitmap.GetPixel(corner.X, corner.Y).ToArgb() != button.Parent.BackColor.ToArgb())
                throw new InvalidOperationException("Unpainted rounded corner: " + button.Name + " / " + state + " / " + corner);
        }
    }
}
