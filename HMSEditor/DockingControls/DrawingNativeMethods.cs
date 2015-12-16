using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;

namespace Darwen.Drawing.General
{
    public sealed class DrawingNativeMethods
    {
        private DrawingNativeMethods()
        {
        }

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(
            IntPtr hdcDest,
            int nXDest,
            int nYDest,
            int nWidth, 
            int nHeight, 
            IntPtr hdcSrc,  
            int nXSrc, 
            int nYSrc, 
            System.Int32 dwRop);

        private static readonly int SRCCOPY = 0x00CC0020;

        static public void CopyBitmapFromGraphics(Graphics source, Point sourcePoint, Bitmap destination)
        {
            using (GraphicsHdc hdcSource = new GraphicsHdc(source))
            {
                using (Graphics desintationGraphics = Graphics.FromImage(destination))
                {
                    using (GraphicsHdc hdcDestination = new GraphicsHdc(desintationGraphics))
                    {
                        BitBlt(hdcDestination, 0, 0, destination.Width, destination.Height,
                            hdcSource, sourcePoint.X, sourcePoint.Y, SRCCOPY);
                    }
                }
            }
        }
    }
}
