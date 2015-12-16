using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Collections.ObjectModel;

namespace Darwen.Windows.Forms.Controls.TabbedDocuments
{
    public class TabbedDocumentCollection : Collection<Control>
    {
        private TabbedDocumentControl _control;

        public TabbedDocumentCollection(TabbedDocumentControl control)
        {
            _control = control;
        }

        public void Add(string title, Control control)
        {
            Add(control);
            _control.ItemTitles[control] = title;
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            _control.Clear();
        }

        protected override void InsertItem(int index, Control item)
        {
            base.InsertItem(index, item);
            _control.InsertControl(GetControlIndex(index), item);            
        }

        protected override void RemoveItem(int index)
        {
            Control control = base[index];
            base.RemoveItem(index);            
            _control.RemoveControl(control);            
        }

        protected override void SetItem(int index, Control item)
        {
            Control control = base[index];
            _control.RemoveControl(control);
            _control.InsertControl(index, item);

            base.SetItem(index, item);            
        }

        /// <summary>
        /// This is based on the first visible button in the menu strip
        /// </summary>
        private int GetControlIndex(int index)
        {
            int buttonIndex = _control.GetFirstButtonControlIndex();

            if (index < buttonIndex)
            {
                return ((index + this.Count) - buttonIndex);
            }
            else
            {
                return index - buttonIndex;
            }
        }
    }
}
