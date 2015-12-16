using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Darwen.Windows.Forms.General
{
    public class RedrawStopper : IDisposable
    {
        private Control _control;
        private bool _invalidate;
        private bool _dispose;

        static Dictionary<Control, int> _mapControlToCount = new Dictionary<Control, int>();

        public RedrawStopper(Control control)
            : this(control, false)
        {
        }

        public RedrawStopper(Control control, bool invalidate)
        {
            _control = control;
            _invalidate = invalidate;

            if (_control != null)
            {
                if (_mapControlToCount.ContainsKey(control))
                {
                    _mapControlToCount[control] += 1;
                }
                else
                {
                    if (_control.IsHandleCreated && _control.Visible)
                    {
                        NativeMethods.SendMessage(_control, NativeMethods.Constants.WM_SETREDRAW, 0, 0);
                        _dispose = true;
                    }

                    _mapControlToCount.Add(control, 1);
                }
            }            
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_control != null)
            {
                int count = _mapControlToCount[_control] - 1;

                if (count == 0)
                {
                    if (_dispose)
                    {
                        NativeMethods.SendMessage(_control, NativeMethods.Constants.WM_SETREDRAW, -1, 0);
                    }

                    if (_invalidate)
                    {
                        _control.Invalidate(true);
                    }            

                    _mapControlToCount.Remove(_control);
                }
                else
                {
                    _mapControlToCount[_control] = count;
                }
            }            
        }

        #endregion
    }
}
