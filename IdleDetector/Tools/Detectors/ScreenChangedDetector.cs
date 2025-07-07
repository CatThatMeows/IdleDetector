using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace IdleDetector.Tools.Detectors
{
    internal class ScreenChangedDetector
    {
        public EventHandler<bool> OnScreenStatusChanged;

        private System.Timers.Timer ScreenChangedDetectorTimer;
        private int IdleScreenCount { get; set; }
        private byte[] LastImageBytes { get; set; }
        private bool IsScreenIdle { get; set; } = false;

        private void CheckScreenDifference(object sender, EventArgs e)
        {
            byte[] imageBytes = CaptureScreen();
            if(ByteArrayCompare(imageBytes, LastImageBytes))
            {
                IdleScreenCount++;
            }
            else
            {
                IdleScreenCount = 0;
                LastImageBytes = imageBytes;
            }

            if((IdleScreenCount >= 2) && (IsScreenIdle == false))
            {
                IsScreenIdle = true;
                OnStatusChanged();
            }
            else if((IdleScreenCount < 2) && (IsScreenIdle == true))
            {
                IsScreenIdle = false;
                OnStatusChanged();
            }
        }

        private void OnStatusChanged()
        {
            EventHandler<bool> handleMouseChanged = OnScreenStatusChanged;
            if (handleMouseChanged != null)
            {
                handleMouseChanged(this, IsScreenIdle);
            }
        }

        private byte[] CaptureScreen()
        {
            byte[] result;
            Rectangle bounds = Screen.GetBounds(Point.Empty);
            Point Source = new Point(0, 100);
            using (Bitmap bitmap = new Bitmap(bounds.Width, (bounds.Height - 100)))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(Source, Source, bounds.Size);
                }
                result = ImageToByte(bitmap);
            }

            return result;
        }
        private byte[] ImageToByte(Image img)
        {
            byte[] result;
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                result = stream.ToArray();
            }

            return result;
        }

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int memcmp(byte[] b1, byte[] b2, long count);
        private bool ByteArrayCompare(byte[] b1, byte[] b2)
        {
            return b1.Length == b2.Length && memcmp(b1, b2, b1.Length) == 0;
        }

        public ScreenChangedDetector()
        {
            LastImageBytes = CaptureScreen();
            ScreenChangedDetectorTimer = new System.Timers.Timer();
            ScreenChangedDetectorTimer.Interval = 30000;
            ScreenChangedDetectorTimer.Elapsed += CheckScreenDifference;
            ScreenChangedDetectorTimer.AutoReset = true;
            ScreenChangedDetectorTimer.Enabled = true;
        }
    }
}
