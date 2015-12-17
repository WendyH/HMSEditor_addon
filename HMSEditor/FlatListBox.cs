using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Security.Permissions;
using FastColoredTextBoxNS;

namespace HMSEditorNS {
    public partial class FlatListBox: UserControl {
        new FlatScrollbar VerticalScroll   = new FlatScrollbar(false);
        new FlatScrollbar HorizontalScroll = new FlatScrollbar(true );

        new Size ClientSize {
            get {
                int w = base.ClientSize.Width  - (VerticalScroll  .Visible ? VerticalScroll  .Width  : 0);
                int h = base.ClientSize.Height - (HorizontalScroll.Visible ? HorizontalScroll.Height : 0);
                return new Size(w, h);
            }
        }

        public HMSItem SelectedItem {
            get {
                if (Items.Count > 0 && focussedItemIndex < Items.Count) {
                    return Items[focussedItemIndex];
                }
                return new HMSItem();
            }
        }

        public event EventHandler FocussedItemIndexChanged;

        public AutocompleteItems Items = new AutocompleteItems();

        int focussedItemIndex = 0;
        
        private int ItemHeight {
            get { return Font.Height + 2; }
        }

        int oldItemCount = 0;

        internal bool AllowTabKey  { get; set; }
        public ImageList ImageList { get; set; }

        public Color SelectedColor { get; set; }
        public Color HoveredColor  { get; set; }
        public int FocussedItemIndex {
            get { return focussedItemIndex; }
            set {
                if (focussedItemIndex != value) {
                    int startI = VerticalScroll.Value / ItemHeight - 1;
                    int finishI = (VerticalScroll.Value + ClientSize.Height) / ItemHeight + 1;
                    startI  = Math.Max(startI, 0);
                    finishI = Math.Min(finishI, Items.Count);
                    if ((value< startI) || (value> finishI))
                        VerticalScroll.Value = value * ItemHeight;
                    focussedItemIndex = value;
                    if (FocussedItemIndexChanged != null)
                        FocussedItemIndexChanged(this, EventArgs.Empty);
                }
            }
        }

        public HMSItem FocussedItem {
            get {
                if (FocussedItemIndex >= 0 && focussedItemIndex < Items.Count)
                    return Items[focussedItemIndex];
                return null;
            }
            set {
                FocussedItemIndex = Items.IndexOf(value);
            }
        }

        public FlatListBox() {
            InitializeComponent();

            this.Controls.Add(VerticalScroll);
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.Selectable, true);
            if (HMS.PFC.Families.Length > 0) { // By WendyH
                base.Font = new Font(HMS.PFC.Families[0], 9.25f, FontStyle.Regular, GraphicsUnit.Point);
            } else {
                base.Font = new Font("Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            }
            VerticalScroll.SmallChange = ItemHeight;
            VerticalScroll.LargeChange = Height;
            SelectedColor = Color.CornflowerBlue;
            HoveredColor = Color.Red;
            BorderStyle = BorderStyle.None;
            this.VScroll = false;
            this.HScroll = false;
            Controls.Add(HorizontalScroll);
            Controls.Add(VerticalScroll  );
            VerticalScroll  .AlignByLines = true;
            HorizontalScroll.AlignByLines = true;
            VerticalScroll  .ValueChanged += VerticalScroll_Scroll;
            HorizontalScroll.ValueChanged += HorizontalScroll_Scroll;
            HorizontalScroll.Maximum = 0;
        }

        private void HorizontalScroll_Scroll(object sender, EventArgs e) {
            Invalidate();
        }

        private void VerticalScroll_Scroll(object sender, EventArgs e) {
            Invalidate();
        }

        protected override void OnResize(EventArgs e) {
            if (!VerticalScroll  .Visible) VerticalScroll  .Height = Height;
            if (!HorizontalScroll.Visible) HorizontalScroll.Width  = Width;
            RecalcWidth();
            Invalidate();
            base.OnResize(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e) {
            int newVal = VerticalScroll.Value - e.Delta;
            newVal = Math.Max(VerticalScroll.Minimum, newVal);
            newVal = Math.Min(VerticalScroll.Maximum, newVal);
            VerticalScroll.Value = newVal;
            Invalidate();
        }

        private void RecalcWidth() {
            Graphics g = Graphics.FromHwnd(Handle);
            int leftPadding = (ImageList != null ? 18 : 0);
            int needWidth = 0;
            foreach(var item in Items) {
                Size size = TextRenderer.MeasureText(g, item.ToString(), Font);
                needWidth = Math.Max(needWidth, size.Width + leftPadding + 16);
            }
            HorizontalScroll.Maximum = Math.Max(0, needWidth - Width);
        }

        private void AdjustScroll() {
            if (oldItemCount == Items.Count)
                return;

            int needHeight = ItemHeight * Items.Count + 1;
            //Height = Math.Min(needHeight, MaximumSize.Height);

            VerticalScroll.Maximum = needHeight - ClientSize.Height;
            oldItemCount = Items.Count;
        }

        protected override void OnPaint(PaintEventArgs e) {
            AdjustScroll();
            Graphics g = e.Graphics;

            var itemHeight = ItemHeight;
            int startI  = VerticalScroll.Value / itemHeight - 1;
            int finishI = (VerticalScroll.Value + ClientSize.Height) / itemHeight + 1;
            startI  = Math.Max(startI, 0);
            finishI = Math.Min(finishI, Items.Count);
            int y = 0;
            int x = -HorizontalScroll.Value;
            int leftPadding = (ImageList!=null ? 18 : 0);
            for (int i = startI; i < finishI; i++) {
                y = i * itemHeight - VerticalScroll.Value;

                var item = Items[i];
                // draw item background
                if (item.BackColor != Color.Transparent) {
                    using (var brush = new SolidBrush(item.BackColor)) {
                        g.FillRectangle(brush, 0, y, ClientSize.Width, itemHeight);
                    }
                }
                // draw item image
                if (ImageList != null && item.ImageIndex >= 0 && item.ImageIndex < ImageList.Images.Count) {
                    g.DrawImage(ImageList.Images[item.ImageIndex], x+1, y);
                }
                // draw selected item
                if (i == FocussedItemIndex) {
                    using (var selectedBrush = new SolidBrush(SelectedColor)) {
                        using (var pen = new Pen(SelectedColor)) {
                            g.FillRectangle(selectedBrush, x+leftPadding, y-1, ClientSize.Width - 1 - leftPadding-x, itemHeight);
                        }
                    }
                }
                // draw item text
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                Color foreColor = item.ForeColor != Color.Transparent ? item.ForeColor : (i == FocussedItemIndex) ? Color.White : ForeColor;
                TextRenderer.DrawText(g, item.ToString(), Font, new Point(x+leftPadding + 1, y), foreColor);
                //using (var brush = new SolidBrush(foreColor)) {
                //g.DrawString(item.ToString(), Font, brush, leftPadding+2, y, StringFormat.GenericTypographic);
                //}
            }
        }

        int PointToItemIndex(Point p) {
            return (p.Y + VerticalScroll.Value) / ItemHeight;
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
            Invalidate();
        }

        private void DoSelectedVisible() {
            if (FocussedItemIndex >= 0) {
                var y = FocussedItemIndex * ItemHeight - VerticalScroll.Value;
                if (y < 0)
                    VerticalScroll.Value = FocussedItemIndex * ItemHeight;
                if (y > ClientSize.Height - ItemHeight)
                    VerticalScroll.Value = Math.Min(VerticalScroll.Maximum, FocussedItemIndex * ItemHeight - ClientSize.Height + ItemHeight);

            }
        }

        public int Count { get { return Items.Count; } }

        public void SetAutocompleteItems(ICollection<string> items) {
            AutocompleteItems list = new AutocompleteItems();
            foreach (var item in items)
                list.Add(new HMSItem(item));
            SetAutocompleteItems(list);
            RecalcWidth();
        }

        public void SetAutocompleteItems(AutocompleteItems items) {
            Items = items;
            RecalcWidth();
        }

        public void AddFilteredItems(AutocompleteItems items, string filter) {
            AutocompleteItems list = new AutocompleteItems();
            foreach (var item in items) {
                if ((filter.Length > 0) && (item.Filter.Length > 0) && (filter.IndexOf(item.Filter) < 0)) continue;
                if (list.ContainsName(item.MenuText)) continue;
                list.Add(item);
            }
            AddAutocompleteItems(list);
            RecalcWidth();
        }

        public void AddAutocompleteItems(AutocompleteItems items) {
            Items.AddRange(items);
            RecalcWidth();
        }
    }
}
