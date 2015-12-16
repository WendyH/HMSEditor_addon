using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Darwen.Windows.Forms.General
{
    public class DesktopGraphics : IDisposable
    {
        [DllImport("User32.dll")]
        static private extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("User32.dll")]
        static private extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

        private Graphics _graphics;
        private IntPtr _hDC;

        public DesktopGraphics()
        {
            _hDC = GetWindowDC(IntPtr.Zero);

            if (_hDC != IntPtr.Zero)
            {
                _graphics = Graphics.FromHdc(_hDC);
            }
        }

        public Graphics Graphics
        {
            get
            {
                return _graphics;
            }
        }

        static public implicit operator Graphics(DesktopGraphics graphics)
        {
            return graphics.Graphics;
        }

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            if (_graphics != null)
            {
                _graphics.Dispose();
                _graphics = null;
            }

            if (_hDC != IntPtr.Zero)
            {
                ReleaseDC(IntPtr.Zero, _hDC);
                _hDC = IntPtr.Zero;
            }
        }

        #endregion
    }
}
