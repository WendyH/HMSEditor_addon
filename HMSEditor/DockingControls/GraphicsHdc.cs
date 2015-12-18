using System;
using System.Drawing;
using System.Security.Permissions;

namespace Darwen.Drawing.General
{
    public class GraphicsHdc : IDisposable
    {
        private Graphics _graphics;
        private IntPtr _hDC;

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public GraphicsHdc(Graphics graphics)
        {
            _graphics = graphics;
            _hDC = _graphics.GetHdc();
        }

        public IntPtr HDC
        {
            get
            {
                return _hDC;
            }
        }

        static public implicit operator IntPtr(GraphicsHdc hdc)
        {
            return hdc.HDC;
        }

        #region IDisposable Support
        private bool disposedValue = false; // Для определения избыточных вызовов

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    if (_hDC != IntPtr.Zero) {
                        _graphics.ReleaseHdc(_hDC);
                        _hDC = IntPtr.Zero;
                    }
                }
                disposedValue = true;
            }
        }
        public void Dispose() {
            Dispose(true);
        }
        #endregion

    }
}
