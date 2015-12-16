using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Darwen.Drawing.General
{
    public class GraphicsHdc : IDisposable
    {
        private Graphics _graphics;
        private IntPtr _hDC;

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

        #region IDisposable Members

        public void Dispose()
        {
            if (_hDC != IntPtr.Zero)
            {
                _graphics.ReleaseHdc(_hDC);
                _hDC = IntPtr.Zero;
            }
        }

        #endregion
    }
}
