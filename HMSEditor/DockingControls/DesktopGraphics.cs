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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
        static private extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("User32.dll")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
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

        #region IDisposable Support
        private bool disposedValue = false; // Для определения избыточных вызовов

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    if (_graphics != null) {
                        _graphics.Dispose();
                        _graphics = null;
                    }
                    if (_hDC != IntPtr.Zero) {
                        ReleaseDC(IntPtr.Zero, _hDC);
                        _hDC = IntPtr.Zero;
                    }
                }
                disposedValue = true;
            }
        }

        ~DesktopGraphics() {
            Dispose(false);
        }

        // Этот код добавлен для правильной реализации шаблона высвобождаемого класса.
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
