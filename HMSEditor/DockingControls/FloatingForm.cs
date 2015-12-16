using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Darwen.Windows.Forms.General;

namespace Darwen.Windows.Forms.Controls.Docking
{
    internal partial class FloatingForm : Form
    {
        private DockingControl _inner;
        private CancelledChangedHandler _cancelledChangedHandler;
        private ControlDragSizer _dragSizer;
        private DockingManagerControl _manager;
        private bool _loaded;

        public const int PaddingSize = 5;

        public FloatingForm(DockingControl inner, DockingManagerControl manager)
        {
            InitializeComponent();

            _manager = manager;
            _inner = inner;
            _cancelledChangedHandler = new CancelledChangedHandler(Inner_CancelledChanged);
            _inner.CancelledChanged += _cancelledChangedHandler;

            this.SuspendLayout();
            this.Controls.Add(inner);
            inner.Dock = DockStyle.Fill;
            this.Padding = new Padding(PaddingSize, PaddingSize, PaddingSize, PaddingSize);            
            this.ResumeLayout(true);

            _dragSizer = new ControlDragSizer(this);
        }

        public DockingControl InnerControl
        {
            get
            {
                return _inner;
            }

            set
            {
                _inner = value;
            }
        }

        public bool Loaded
        {
            get
            {
                return _loaded;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.MinimumSize = _inner.MinimumSize;
            _loaded = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            ControlPaint.DrawBorder3D(e.Graphics, this.ClientRectangle, Border3DStyle.Raised);
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_inner != null)
            {
                _inner.CancelledChanged -= _cancelledChangedHandler;
            }

            base.OnClosed(e);
        }

        private void Inner_CancelledChanged(DockingControl control)
        {
            if (control == _inner)
            {
                if (_inner.Cancelled)
                {
                    this.Visible = false;                    
                }
                else if (Loaded)
                {
                    this.Visible = true;                    
                }
                else
                {
                    this.Show(_manager.ParentForm);
                }
            }
        }
    }
}