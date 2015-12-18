using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Darwen.Windows.Forms.General
{    
    public delegate void AutoHideHandler(AutoHideControlHandler sender);

    public class AutoHideControlHandler : IDisposable
    {
        private Control _control;
        private Timer _timer;
        private int _count;
        private const int HideCount = 3;
        private Control _focusedControl;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        public event AutoHideHandler Hide;

        public AutoHideControlHandler(Control control)
        {
            _control = control;

            _timer = new Timer();
            _timer.Interval = 250;
            _timer.Tick += new EventHandler(_timer_Tick);   
        }

        public void Start()
        {
            if (!_timer.Enabled)
            {
                _count = 0;
                _timer.Enabled = true;
            }
        }

        public void Stop()
        {
            _timer.Enabled = false;
        }

        protected virtual void OnHide()
        {
            _timer.Stop();

            if (Hide != null)
            {
                Hide(this);
            }
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            if (_control.Visible && _control.Parent != null)
            {
                Control focusedControl = ControlHelpers.GetFocusedChildControl(_control);

                if (focusedControl == null)
                {
                    if (_focusedControl != null)
                    {
                        _focusedControl = null;

                        OnHide();
                    }
                    else
                    {
                        Point cursorPosition = _control.Parent.PointToClient(Cursor.Position);

                        if (_control.Bounds.Contains(cursorPosition))
                        {
                            _count = 0;
                        }
                        else
                        {
                            _count += 1;

                            if (_count == HideCount)
                            {
                                OnHide();
                            }
                        }
                    }
                }
                else
                {
                    if (_focusedControl != focusedControl)
                    {
                        _focusedControl = focusedControl;
                    }
                }
            }
            else
            {
                _count = 0;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // Для определения избыточных вызовов

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    _timer.Dispose();
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
