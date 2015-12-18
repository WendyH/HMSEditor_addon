using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Darwen.Windows.Forms.General
{
    public class ControlResizeAnimator: IDisposable
    {
        private Control _control;
        private Timer _timer;
        private int _targetSize;
        private double _currentSize;
        private double _sizeStep;
        private int _currentStep;
        private Direction _direction = Direction.None;
        private const int TimePeriod = 40;
        private const int Steps = 1;

        private enum State
        {
            Showing,
            Hiding
        }

        private State _state;

        public ControlResizeAnimator(Control control, int targetSize)
        {
            _control = control;
            _targetSize = targetSize;
        }

        public int TargetSize
        {
            get
            {
                return _targetSize;
            }

            set
            {
                _targetSize = value;
            }
        }

        public Direction Direction
        {
            get
            {
                return _direction;
            }

            set
            {
                _direction = value;
            }
        }
	

        public bool Showing
        {
            get
            {
                return _timer != null && _timer.Enabled;
            }
        }

        public void Show()
        {
            if (StartTimer())
            {
                _currentSize = 0.0;
                _currentStep = 0;
                _sizeStep = (double)TargetSize / (double)Steps;
                _state = State.Showing;
            }
        }

        public void Hide()
        {
            if (StartTimer())
            {
                _currentSize = (double)TargetSize;
                _currentStep = 0;
                _sizeStep = -(double)TargetSize / (double)Steps;
                _state = State.Hiding;
            }
        }

        private bool StartTimer()
        {
            if (_timer == null)
            {
                _timer = new Timer();
                _timer.Interval = TimePeriod / Steps;
                _timer.Tick += new EventHandler(_timer_Tick);
                _timer.Enabled = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SetControlSize()
        {
            Rectangle oldBounds = _control.Bounds;
            
            using (RedrawStopper redrawStopper = new RedrawStopper(_control))
            {
                Rectangle bounds = Rectangle.Empty;

                switch (_direction)
                {
                    case Direction.None:
                        break;
                    case Direction.Left:
                        _control.Width = (int)_currentSize;
                        break;
                    case Direction.Right:
                        bounds = _control.Bounds;
                        bounds.X = bounds.Right - (int)_currentSize;
                        bounds.Width = (int)_currentSize;
                        _control.Bounds = bounds;
                        break;
                    case Direction.Up:
                        _control.Height = (int)_currentSize;
                        break;
                    case Direction.Down:
                        bounds = _control.Bounds;
                        bounds.Y = bounds.Bottom - (int)_currentSize;
                        bounds.Height = (int)_currentSize;
                        _control.Bounds = bounds;
                        break;
                    default:
                        break;
                }
            }

            Rectangle invalidateBounds = Rectangle.Union(oldBounds, _control.Bounds);
            _control.Parent.Invalidate(invalidateBounds, true);            
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            _currentSize += _sizeStep;

            SetControlSize();

            if (_state == State.Showing && _currentStep == 0)
            {
                _control.Visible = true;
            }

            _currentStep += 1;

            if (_currentStep == Steps)
            {
                _timer.Enabled = false;
                _timer.Dispose();
                _timer = null;

                if (_state == State.Hiding)
                {
                    _control.Visible = false;
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // Для определения избыточных вызовов

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    if (_timer!=null) {
                        _timer.Dispose();
                        _timer = null;
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
