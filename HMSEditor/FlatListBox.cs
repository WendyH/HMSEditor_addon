using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Security.Permissions;
using FastColoredTextBoxNS;

namespace HMSEditorNS {
    public partial class FlatListBox: UserControl {
        new FlatScrollbar VerticalScroll   = new FlatScrollbar();
        new FlatScrollbar HorizontalScroll = new FlatScrollbar(true);

        public Color ArrowColor      = Color.DarkGray;
        public Color ArrowHoverColor = Color.CornflowerBlue;

        public string Filter = "";

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

        public new Font Font { get { return base.Font; } set { base.Font = value; VerticalScroll.SmallChange = ItemHeight; } }

        public event EventHandler FocussedItemIndexChanged;

        public AutocompleteItems Items = new AutocompleteItems();

        int focussedItemIndex;
        
        public int ItemHeight => Font.Height + 2;

        int oldItemCount;

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
                    FocussedItemIndexChanged?.Invoke(this, EventArgs.Empty);
                    Recalc();
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

            Controls.Add(VerticalScroll);
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.Selectable, true);
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (HMS.PFC.Families.Length > 0) { // By WendyH
                base.Font = new Font(HMS.PFC.Families[0], 9.25f, FontStyle.Regular, GraphicsUnit.Point);
            } else {
                base.Font = new Font("Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            }
            SelectedColor = Color.CornflowerBlue;
            HoveredColor  = Color.Red;
            BorderStyle   = BorderStyle.None;
            VScroll = false;
            HScroll = false;
            Controls.Add(HorizontalScroll);
            Controls.Add(VerticalScroll  );
            VerticalScroll.SmallChange     = ItemHeight;
            VerticalScroll.LargeChange     = Height;
            VerticalScroll.AlignByLines    = true;
            HorizontalScroll.AlignByLines  = true;
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

        public void Recalc(bool force = false) {
            if (!force && oldItemCount == Items.Count) return;

            int needHeight = ItemHeight * Items.Count;
            //Height = Math.Min(needHeight, MaximumSize.Height);

            VerticalScroll.Maximum = needHeight - ClientSize.Height;

            RecalcWidth();

            Invalidate();
            oldItemCount = Items.Count;
        }

        protected override void OnResize(EventArgs e) {
            Recalc(true);
            if (!VerticalScroll  .Visible) VerticalScroll  .Height = ClientSize.Height;
            if (!HorizontalScroll.Visible) HorizontalScroll.Width  = ClientSize.Width;
            base.OnResize(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e) {
            int newVal = VerticalScroll.Value - e.Delta;
            newVal = (int)(Math.Ceiling(1d * newVal / ItemHeight) * ItemHeight);
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

        protected bool isUpdated;

        public void BeginUpdate() {
            isUpdated = true;
        }

        public void EndUpdate() {
            isUpdated = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e) {
            if (isUpdated) return;
            Recalc();
            Graphics g = e.Graphics;
            var itemHeight = ItemHeight;

            //int scrollValue = (int)(Math.Ceiling(1d * VerticalScroll.Value / itemHeight) * itemHeight);
            int scrollValue = VerticalScroll.Value;
            int startI  = VerticalScroll.Value / itemHeight;
            int finishI = (VerticalScroll.Value + ClientSize.Height) / itemHeight;
            startI  = Math.Max(startI, 0);
            finishI = Math.Min(finishI, Items.Count);
            int x = -HorizontalScroll.Value;
            int leftPadding = (ImageList!=null ? 18 : 0);
            for (int i = startI; i < finishI; i++) {
                var y = i * itemHeight - scrollValue;

                var item = Items[i];

                var leftShift = item.Level * 16;

                // draw item background
                if (item.BackColor != Color.Transparent) {
                    using (var brush = new SolidBrush(item.BackColor)) {
                        g.FillRectangle(brush, leftShift, y, ClientSize.Width, itemHeight);
                    }
                }
                // draw item image
                if (ImageList != null && item.ImageIndex >= 0 && item.ImageIndex < ImageList.Images.Count) {
                    g.DrawImage(ImageList.Images[item.ImageIndex], x + 1 + leftShift, y);
                }

                if (item.IsClass) {
                    //draw down arrow
                    int x2 = x + leftPadding + leftShift;
                    Point[] points2;
                    if (item.Expanded) {
                        points2 = new[] {
                            new Point(x2+10, y+3),
                            new Point(x2+10, y+14),
                            new Point(x2, y+14)
                        };
                        using (Brush brush = new SolidBrush(ArrowHoverColor)) {
                            g.FillPolygon(brush, points2);
                        }
                    } else {
                        points2 = new[] {
                            new Point(x2+5, y+13),
                            new Point(x2+5, y+3),
                            new Point(x2+4, y+2),
                            new Point(x2+4, y+14),
                            new Point(x2+10, y+8),
                            new Point(x2+4, y+2)
                        };
                        using (Pen pen = new Pen(ArrowHoverColor)) {
                            g.DrawPolygon(pen, points2);
                        }
                    }
                    leftShift += 12;
                }


                // draw selected item
                if (i == FocussedItemIndex) {
                    using (var selectedBrush = new SolidBrush(SelectedColor)) {
                        g.FillRectangle(selectedBrush, x+leftPadding+ leftShift, y, ClientSize.Width - 1 - leftPadding-x, itemHeight+1);
                    }
                }
                // draw item text
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                Color foreColor = item.ForeColor != Color.Transparent ? item.ForeColor : (i == FocussedItemIndex) ? Color.White : ForeColor;
                TextRenderer.DrawText(g, item.ToString(), Font, new Point(x+leftPadding + 1+ leftShift, y), foreColor);
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

            if (e.Button == MouseButtons.Left) {
                FocussedItemIndex = PointToItemIndex(e.Location);
                DoSelectedVisible();
                Invalidate();
            }
        }

        internal virtual void OnSelecting() {
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            ProcessKey(keyData, Keys.None);

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public void DoDoubleClick() {
            OnDoubleClick(EventArgs.Empty);
        }

        protected override void OnDoubleClick(EventArgs e) {
            var indx = FocussedItemIndex;
            var item = SelectedItem;

            if (item.IsClass) {
                BeginUpdate();

                if (!item.Expanded) {
                    item.Expanded = true;
                    AutocompleteItems insertedItems = new AutocompleteItems();
                    insertedItems.AddRange(item.ClassInfo.StaticItems);
                    insertedItems.AddRange(item.ClassInfo.MemberItems);
                    Items.InsertRange(indx + 1, insertedItems);

                } else {

                    item.Expanded = false;
                    if (indx < Count - 1) {
                        int count = 0;
                        for (int i = indx + 1; i < Count; i++) {
                            if (Items[i].Level < 1) break;
                            count++;
                        }
                        Items.RemoveRange(indx + 1, count);
                    }
                }
                EndUpdate();
            }
            base.OnDoubleClick(e);
        }

        private void ProcessKey(Keys keyData, Keys keyModifiers) {
            int count = ClientSize.Height / ItemHeight;
            if (keyModifiers == Keys.None)
                switch (keyData) {
                    case Keys.Right:
                        if (SelectedItem.IsClass && !SelectedItem.Expanded) OnDoubleClick(EventArgs.Empty);
                        return;
                    case Keys.Left:
                        if (SelectedItem.IsClass && SelectedItem.Expanded) OnDoubleClick(EventArgs.Empty);
                        return;
                    case Keys.Down:
                        SelectNext(+1);
                        return;
                    case Keys.PageDown:
                        SelectNext(+count);
                        return;
                    case Keys.Up:
                        SelectNext(-1);
                        return;
                    case Keys.PageUp:
                        SelectNext(-count);
                        return;
                    case Keys.Home:
                        SelectFirst();
                        return;
                    case Keys.End:
                        SelectLast();
                        return;
                    case Keys.Enter:
                        OnSelecting();
                        return;
                }
        }

        public void SelectFirst() {
            if (Items.Count > 0) {
                FocussedItemIndex = 0;
                DoSelectedVisible();
                Invalidate();
            }
        }

        public void SelectLast() {
            if (Items.Count > 0) {
                FocussedItemIndex = Items.Count - 1;
                DoSelectedVisible();
                Invalidate();
            }
        }

        public void SelectNext(int shift) {
            if ((shift < 0) && (FocussedItemIndex == 0)) return;
            if ((shift > 0) && (FocussedItemIndex >= (Items.Count - 1))) return;
            FocussedItemIndex = Math.Max(0, Math.Min(FocussedItemIndex + shift, Items.Count - 1));
            DoSelectedVisible();
            Invalidate();
        }

        public void DoSelectedVisible() {
            if (FocussedItemIndex >= 0) {
                var y = FocussedItemIndex * ItemHeight - VerticalScroll.Value;
                if (y < 0)
                    VerticalScroll.Value = FocussedItemIndex * ItemHeight;
                if (y > ClientSize.Height - ItemHeight)
                    VerticalScroll.Value = Math.Min(VerticalScroll.Maximum, FocussedItemIndex * ItemHeight - ClientSize.Height + ItemHeight);

            }
        }

        public int Count => Items.Count;

        public void SetAutocompleteItems(ICollection<string> items) {
            AutocompleteItems list = new AutocompleteItems();
            foreach (var item in items)
                list.Add(new HMSItem(item));
            SetAutocompleteItems(list);
        }

        public void SetAutocompleteItems(AutocompleteItems items) {
            Items = items;
            RecalcWidth();
        }

        public void AddFilteredItems(AutocompleteItems items, string filter) {
            AutocompleteItems list = new AutocompleteItems();
            foreach (var item in items) {
                if ((filter.Length > 0) && (item.Filter.Length > 0) && (filter.IndexOf(item.Filter, StringComparison.Ordinal) < 0)) continue;
                if (list.ContainsName(item.MenuText)) continue;
                list.Add(item);
            }
            AddAutocompleteItems(list);
        }

        public void AddAutocompleteItems(AutocompleteItems items) {
            Items.AddRange(items);
            RecalcWidth();
        }
    }

    public class MyRichTextBox : RichTextBox {
        private const int WM_SETREDRAW    = 0x000B;
        private const int WM_USER         = 0x0400;
        private const int EM_GETEVENTMASK = (WM_USER + 59);
        private const int EM_SETEVENTMASK = (WM_USER + 69);

        private IntPtr eventMask;

        public void BeginUpdate() {
            // Stop redrawing:
            NativeMethods.SendMessage(Handle, WM_SETREDRAW, (IntPtr)0, IntPtr.Zero);
            // Stop sending of events:
            eventMask = NativeMethods.SendMessage(Handle, EM_GETEVENTMASK, (IntPtr)0, IntPtr.Zero);
        }

        public void EndUpdate() {
            // turn on events
            NativeMethods.SendMessage(Handle, EM_SETEVENTMASK, (IntPtr)0, eventMask);
            // turn on redrawing
            NativeMethods.SendMessage(Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
            // this forces a repaint, which for some reason is necessary in some cases.
            Invalidate();
        }

        public void AppendText(string text, Color color, Font font) {
            SelectionStart  = TextLength;
            SelectionLength = 0;
            SelectionFont   = font;
            SelectionColor  = color;
            AppendText(text);
        }
    }


}
