using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Darwen.Windows.Forms.General
{
    public class SetCursor : IDisposable
    {
        private Cursor _cursor;
        private Cursor _oldCursor;
        private Cursor _oldControlCursor;
        private Control _control;

        public SetCursor(Cursor cursor)
        {
            _cursor = cursor;
            _oldCursor = Cursor.Current;
            Cursor.Current = cursor;
        }

        public SetCursor(Control control, Cursor cursor)
            : this(cursor)
        {
            _control = control;
            _oldControlCursor = control.Cursor;
            _control.Cursor = cursor;
        }

        public Cursor Cursor
        {
            get
            {
                return _cursor;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // ��� ����������� ���������� �������

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                }
                disposedValue = true;
            }
            if (_control != null) {
                _control.Cursor = _oldControlCursor;
            }
            Cursor.Current = _oldCursor;
        }

        public void Dispose() {
            Dispose(true);
        }
        #endregion

    }
}
