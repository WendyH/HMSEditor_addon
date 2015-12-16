using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using Darwen.General;
using Darwen.Windows.Forms.General;

namespace Darwen.Windows.Forms.Controls.TabbedDocuments
{
    public partial class TabbedDocumentControl : UserControl
    {
        private DoubleDictionary<Control, ToolStripButton> _controlButtonMap;
        private Control _activeControl;
        private int _initialMenuStripItemCount;
        private TabbedDocumentControlRenderer _renderer;
        private TabbedDocumentCollection _documents;
        private TabbedDocumentControlActiveFilesContextMenuHandler _activeFilesContextMenuHandler;
        private TabbedDocumentTitlesCollection _titles;

        public delegate void DocumentHandler(TabbedDocumentControl documentControl, Control control);   

        public event EventHandler SelectedControlChanged;
        public event DocumentHandler DocumentAdded;
        public event DocumentHandler DocumentRemoved;

        public TabbedDocumentControl()
        {
            InitializeComponent();

            _renderer = new TabbedDocumentControlRenderer(this, _menuStrip.Renderer);
            _menuStrip.Renderer = _renderer;

            _controlButtonMap = new DoubleDictionary<Control, ToolStripButton>();
            _initialMenuStripItemCount = _menuStrip.Items.Count;
            _menuStrip.Padding = new Padding(8, 2, 8, 2);
            _documents = new TabbedDocumentCollection(this);
            _activeFilesContextMenuHandler = new TabbedDocumentControlActiveFilesContextMenuHandler(this);
            this.TabStop = true;
            _titles = new TabbedDocumentTitlesCollection(this);

            SetStyle(ControlStyles.Selectable, true);
        }

        public TabbedDocumentCollection Items
        {
            get
            {
                return _documents;
            }
        }

        public Control SelectedControl
        {
            get
            {
                return _activeControl;
            }

            set
            {
                SelectControl(value, true);
            }
        }

        public void CloseControl(Control control)
        {
            _documents.Remove(control);
            control.Dispose();
        }

        public TabbedDocumentTitlesCollection ItemTitles
        {
            get
            {
                return _titles;
            }

            set
            {
                _titles = value;
            }
        }

        internal void InsertControl(int index, Control control)
        {            
            ToolStripButton button = new ToolStripButton();
            button.Padding = new Padding(8, 0, 8, 0);

            button.Text = control.Name;
            button.DisplayStyle = ToolStripItemDisplayStyle.Text;
            button.Click += new EventHandler(button_Click);

            _menuStrip.Items.Insert(index + _initialMenuStripItemCount, button);

            _controlButtonMap.Add(control, button);

            control.Dock = DockStyle.Fill;
            control.Parent = this;
            control.BringToFront();

            if (_controlButtonMap.Count == 1)
            {
                SelectControl(control, true);
            }
            else
            {
                control.Visible = false;
            }

            _menuStrip.Visible = true;

            OnDocumentAdded(control);            
        }

        internal int GetFirstButtonControlIndex()
        {
            if (_menuStrip.Items.Count == _initialMenuStripItemCount)
            {
                return 0;
            }
            else
            {
                Control control = _controlButtonMap[_menuStrip.Items[_initialMenuStripItemCount] as ToolStripButton];
                return _documents.IndexOf(control);
            }
        }

        internal void SelectControl(Control control, bool ensureVisible)
        {
            using (RedrawStopper stopRedraws = new RedrawStopper(this, true))
            {
                if (ensureVisible && control != null)
                {
                    MakeVisible(control);
                }

                if (_activeControl != null)
                {
                    _activeControl.Visible = false;
                    _controlButtonMap[_activeControl].Checked = false;
                }

                if (control != null)
                {
                    control.Visible = true;
                    control.Focus();
                    control.Select();
                    _controlButtonMap[control].Checked = true;
                }

                if (_activeControl != control)
                {
                    _activeControl = control;
                    OnSelectedControlChanged();
                }
            }
        }

        internal void Clear()
        {

        }

        internal void RemoveControl(Control control)
        {
            if (control == _activeControl)
            {
                Control nextControl = GetNextControlForClose(control);
                SelectControl(nextControl, false);
            }

            ToolStripButton button = _controlButtonMap[control];
            _menuStrip.Items.Remove(button);

            control.Parent = null;
            _controlButtonMap.Remove(control);

            if (_menuStrip.Items.Count == _initialMenuStripItemCount)
            {
                _menuStrip.Visible = false;

                Form parentForm = ParentForm;

                if (parentForm != null)
                {
                    parentForm.Focus();
                    parentForm.Select();
                }
            }

            OnDocumentRemoved(control);
        }

        public ToolStripRenderer Renderer
        {
            get
            {
                return _renderer.Renderer;
            }

            set
            {
                _renderer.Renderer = value;
            }
        }

        internal ToolStripButton SelectedButton
        {
            get
            {
                if (_activeControl == null)
                {
                    return null;
                }
                else
                {
                    ToolStripButton button = _controlButtonMap[_activeControl];
                    return button;
                }
            }
        }

        internal bool IsDocumentButton(ToolStripButton button)
        {
            return _menuStrip.Items.IndexOf(button) >= _initialMenuStripItemCount;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (_documents.Count == 0)
            {
                base.OnPaintBackground(e);
            }
        }

        protected virtual void OnSelectedControlChanged()
        {
            if (SelectedControlChanged != null)
            {
                SelectedControlChanged(this, null);
            }
        }

        protected virtual void OnDocumentRemoved(Control control)
        {
            if (DocumentRemoved != null)
            {
                DocumentRemoved(this, control);
            }
        }

        protected virtual void OnDocumentAdded(Control control)
        {
            if (DocumentAdded != null)
            {
                DocumentAdded(this, control);
            }
        }

        private void _menuStrip_Paint(object sender, PaintEventArgs e)
        {
            using (Pen pen = new Pen(Color.FromKnownColor(KnownColor.ControlDark)))
            {
                Rectangle clientRectangle = this.ClientRectangle;
                clientRectangle.Width -= 1;
                e.Graphics.DrawRectangle(pen, clientRectangle);
            }
        }

        private void button_Click(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;
            SelectControl(_controlButtonMap[button], false);
        }

        private void _closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_activeControl != null)
            {
                Control control = _activeControl;
                _documents.Remove(control);
                control.Dispose();
            }
        }

        private void _activeFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Rectangle screenRectOfButton = _menuStrip.RectangleToScreen(_activeFilesToolStripMenuItem.Bounds);
            _activeFilesContextMenuHandler.Show(screenRectOfButton);
        }

        private Control GetNextControlForClose(Control control)
        {
            if (_documents.Count == 0)
            {
                return null;
            }
            else
            {
                ToolStripButton button = _controlButtonMap[_activeControl];
                int index = _menuStrip.Items.IndexOf(button);
                index += 1;

                if (index >= _menuStrip.Items.Count)
                {
                    index -= 2;
                }

                if (index >= _initialMenuStripItemCount)
                {
                    return _controlButtonMap[_menuStrip.Items[index] as ToolStripButton];
                }
                else
                {
                    return null;
                }
            }
        }

        private void MakeVisible(Control control)
        {
            ToolStripButton button = _controlButtonMap[control];
            
            int controlIndex = _documents.IndexOf(control);
            int selectedIndex = _documents.IndexOf(_activeControl);

            if (button.Bounds.Right >= _activeFilesToolStripMenuItem.Bounds.Left)
            {
                if (controlIndex > selectedIndex)
                {
                    Rectangle clientRectangle = _menuStrip.ClientRectangle;

                    while (button.Bounds.Right >= _activeFilesToolStripMenuItem.Bounds.Left)
                    {
                        ToolStripButton buttonLeft = _menuStrip.Items[_initialMenuStripItemCount] as ToolStripButton;

                        if (buttonLeft == button)
                        {
                            return;
                        }

                        _menuStrip.Items.Remove(buttonLeft);
                        _menuStrip.Items.Add(buttonLeft);
                        _menuStrip.PerformLayout();
                    }
                }
                else if (controlIndex < selectedIndex)
                {
                    int itemCount = _menuStrip.Items.Count;
                    int controlButtonIndex = _menuStrip.Items.IndexOf(button);

                    for (int itemIndex = controlButtonIndex; itemIndex < itemCount; itemIndex += 1)
                    {
                        ToolStripButton item = _menuStrip.Items[_menuStrip.Items.Count - 1] as ToolStripButton;
                        _menuStrip.Items.RemoveAt(_menuStrip.Items.Count - 1);
                        _menuStrip.Items.Insert(_initialMenuStripItemCount, item);
                    }

                    _menuStrip.PerformLayout();
                }
            }
        }

        public class TabbedDocumentTitlesCollection : IEnumerable<string>
        {
            private TabbedDocumentControl _control;

            internal TabbedDocumentTitlesCollection(TabbedDocumentControl control)
            {
                _control = control;
            }

            public string this[Control control]
            {
                get
                {
                    if (_control._controlButtonMap.ContainsKey(control))
                    {
                        return _control._controlButtonMap[control].Text;
                    }
                    else
                    {
                        throw new ArgumentException("Control isn't a part of the document control");
                    }
                }

                set
                {
                    if (_control._controlButtonMap.ContainsKey(control))
                    {
                        _control._controlButtonMap[control].Text = value;
                    }
                    else
                    {
                        throw new ArgumentException("Control isn't a part of the document control");
                    }
                }
            }

            #region IEnumerable<string> Members

            public IEnumerator<string> GetEnumerator()
            {
                return new TitlesEnumerator(_control);
            }

            #endregion

            #region IEnumerable Members

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return new TitlesEnumerator(_control);
            }

            #endregion

            private class TitlesEnumerator : IEnumerator<string>
            {
                private IEnumerator<ToolStripButton> _enumerator;

                public TitlesEnumerator(TabbedDocumentControl control)
                {
                    _enumerator = control._controlButtonMap.Values.GetEnumerator();
                }

                #region IEnumerator<string> Members

                public string Current
                {
                    get
                    {
                        return _enumerator.Current.Text;
                    }
                }

                #endregion

                #region IDisposable Members

                public void Dispose()
                {                    
                }

                #endregion

                #region IEnumerator Members

                object System.Collections.IEnumerator.Current
                {
                    get
                    {
                        return _enumerator.Current.Text;
                    }
                }

                public bool MoveNext()
                {
                    return _enumerator.MoveNext();
                }

                public void Reset()
                {
                    _enumerator.Reset();
                }

                #endregion
            }
        }

        private void _nextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabbedDocumentControlHelper.SelectNextDocument(this);
        }

        private void _previousToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabbedDocumentControlHelper.SelectPreviousDocument(this);
        }
    }
}

