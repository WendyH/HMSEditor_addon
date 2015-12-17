using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;
using FastColoredTextBoxNS;

namespace HMSEditorNS {
    public class MyListBox: UserControl {
        public event EventHandler FocussedItemIndexChanged;

        public AutocompleteItems Items = new AutocompleteItems();

        int focussedItemIndex = 0;

        public FlatScrollbar VerticalScrollBar = new FlatScrollbar();

        private int ItemHeight
        {
            get { return Font.Height + 2; }
        }

        int oldItemCount = 0;

        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (VerticalScrollBar != null)
                    VerticalScrollBar.Dispose();
            }
            FocussedItemIndexChanged = null;
            Items = null;
            VerticalScrollBar = null;
            base.Dispose(disposing);
        }
        // > By WendyH -----------------------------------

        internal bool AllowTabKey { get; set; }
        public ImageList ImageList { get; set; }

        public Color SelectedColor { get; set; }
        public Color HoveredColor { get; set; }
        public int FocussedItemIndex
        {
            get { return focussedItemIndex; }
            set
            {
                if (focussedItemIndex != value) {
                    focussedItemIndex = value;
                    if (FocussedItemIndexChanged != null)
                        FocussedItemIndexChanged(this, EventArgs.Empty);
                }
            }
        }

        public HMSItem FocussedItem
        {
            get
            {
                if (FocussedItemIndex >= 0 && focussedItemIndex < Items.Count)
                    return Items[focussedItemIndex];
                return null;
            }
            set
            {
                FocussedItemIndex = Items.IndexOf(value);
            }
        }

        internal MyListBox() {
            this.Controls.Add(VerticalScrollBar);
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            if (HMS.PFC.Families.Length > 0) { // By WendyH
                base.Font = new Font(HMS.PFC.Families[0], 9.25f, FontStyle.Regular, GraphicsUnit.Point);
            } else {
                base.Font = new Font("Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            }
            VerticalScrollBar.SmallChange = ItemHeight;
            VerticalScrollBar.LargeChange = Height;
            SelectedColor = Color.CornflowerBlue;
            HoveredColor  = Color.Red;
            BorderStyle   = BorderStyle.None;
            this.VScroll  = false;
            this.HScroll  = false;
            VerticalScrollBar.ValueChanged += VerticalScrollBar_Scroll;
        }

        protected override void OnMouseWheel(MouseEventArgs e) {
            int newVal = VerticalScrollBar.Value - e.Delta;
            newVal = Math.Max(VerticalScrollBar.Minimum, newVal);
            newVal = Math.Min(VerticalScrollBar.Maximum, newVal);
            VerticalScrollBar.Value = newVal;
            Invalidate();
        }

        private void VerticalScrollBar_Scroll(object sender, EventArgs e) {
            Invalidate();
        }

        void AdjustScroll() {
            if (oldItemCount == Items.Count)
                return;

            int needHeight = ItemHeight * Items.Count + 1;
            Height = Math.Min(needHeight, MaximumSize.Height);

            VerticalScrollBar.Maximum = needHeight - Height;
            //AutoScrollMinSize = new Size(0, needHeight);
            oldItemCount = Items.Count;
        }

        protected override void OnPaint(PaintEventArgs e) {
            AdjustScroll();
            Graphics g = e.Graphics;

            var itemHeight = ItemHeight;
            int startI = VerticalScrollBar.Value / itemHeight - 1;
            int finishI = (VerticalScrollBar.Value + ClientSize.Height) / itemHeight + 1;
            startI = Math.Max(startI, 0);
            finishI = Math.Min(finishI, Items.Count);
            int y = 0;
            int leftPadding = 18;
            for (int i = startI; i < finishI; i++) {
                y = i * itemHeight - VerticalScrollBar.Value;

                var item = Items[i];
                // draw item background
                if (item.BackColor != Color.Transparent) {
                    using (var brush = new SolidBrush(item.BackColor)) {
                        g.FillRectangle(brush, 1, y, ClientSize.Width - 1 - 1, itemHeight - 1);
                    }
                }
                // draw item image
                if (ImageList != null && item.ImageIndex >= 0 && item.ImageIndex < ImageList.Images.Count) {
                    g.DrawImage(ImageList.Images[item.ImageIndex], 1, y);
                }
                // draw selected item
                if (i == FocussedItemIndex) {
                    using (var selectedBrush = new SolidBrush(SelectedColor)) {
                        using (var pen = new Pen(SelectedColor)) {
                            g.FillRectangle(selectedBrush, leftPadding, y, ClientSize.Width - 1 - leftPadding, itemHeight);
                        }
                    }
                }
                // draw item text
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                Color foreColor = item.ForeColor != Color.Transparent ? item.ForeColor : (i == FocussedItemIndex) ? Color.White : ForeColor;
                TextRenderer.DrawText(g, item.ToString(), Font, new Point(leftPadding + 1, y), foreColor);
                //using (var brush = new SolidBrush(foreColor)) {
                //g.DrawString(item.ToString(), Font, brush, leftPadding+2, y, StringFormat.GenericTypographic);
                //}
            }
        }

        int PointToItemIndex(Point p) {
            return (p.Y + VerticalScrollBar.Value) / ItemHeight;
        }

        protected override void OnScroll(ScrollEventArgs se) {
            base.OnScroll(se);
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e) {
            base.OnMouseClick(e);

            if (e.Button == System.Windows.Forms.MouseButtons.Left) {
                FocussedItemIndex = PointToItemIndex(e.Location);
                DoSelectedVisible();
                Invalidate();
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e) {
            base.OnMouseDoubleClick(e);
            FocussedItemIndex = PointToItemIndex(e.Location);
            Invalidate();
            OnSelecting();
        }

        internal virtual void OnSelecting() {
            if (FocussedItemIndex < 0 || FocussedItemIndex >= Items.Count)
                return;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            ProcessKey(keyData, Keys.None);

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private bool ProcessKey(Keys keyData, Keys keyModifiers) {
            if (keyModifiers == Keys.None)
                switch (keyData) {
                    case Keys.Down:
                        SelectNext(+1);
                        return true;
                    case Keys.PageDown:
                        SelectNext(+10);
                        return true;
                    case Keys.Up:
                        SelectNext(-1);
                        return true;
                    case Keys.PageUp:
                        SelectNext(-10);
                        return true;
                    case Keys.Enter:
                        OnSelecting();
                        return true;
                }

            return false;
        }

        public void SelectNext(int shift) {
            if ((shift < 0) && (FocussedItemIndex == 0)) return;
            if ((shift > 0) && (FocussedItemIndex >= (Items.Count - 1))) return;
            FocussedItemIndex = Math.Max(0, Math.Min(FocussedItemIndex + shift, Items.Count - 1));
            DoSelectedVisible();
            //
            Invalidate();
        }

        private void DoSelectedVisible() {
            if (FocussedItemIndex >= 0) {
                var y = FocussedItemIndex * ItemHeight - VerticalScrollBar.Value;
                if (y < 0)
                    VerticalScrollBar.Value = FocussedItemIndex * ItemHeight;
                if (y > ClientSize.Height - ItemHeight)
                    VerticalScrollBar.Value = Math.Min(VerticalScrollBar.Maximum, FocussedItemIndex * ItemHeight - ClientSize.Height + ItemHeight);

            }
            //some magic for update scrolls
            //AutoScrollMinSize -= new Size(1, 0);
            //AutoScrollMinSize += new Size(1, 0);
        }

        public int Count { get { return Items.Count; } }

        public void SetAutocompleteItems(ICollection<string> items) {
            AutocompleteItems list = new AutocompleteItems();
            foreach (var item in items)
                list.Add(new HMSItem(item));
            SetAutocompleteItems(list);
        }

        public void SetAutocompleteItems(AutocompleteItems items) {
            Items = items;
        }

        public void AddFilteredItems(AutocompleteItems items, string filter) {
            AutocompleteItems list = new AutocompleteItems();
            foreach (var item in items) {
                if ((filter.Length > 0) && (item.Filter.Length > 0) && (filter.IndexOf(item.Filter) < 0)) continue;
                if (list.ContainsName(item.MenuText)) continue;
                list.Add(item);
            }
            AddAutocompleteItems(list);
        }

        public void AddAutocompleteItems(AutocompleteItems items) {
            Items.AddRange(items);
        }

    }
}
