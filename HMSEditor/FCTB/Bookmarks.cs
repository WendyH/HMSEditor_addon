﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

// ReSharper disable once CheckNamespace
namespace FastColoredTextBoxNS
{
    /// <summary>
    /// Base class for bookmark collection
    /// </summary>
    public abstract class BaseBookmarks : ICollection<Bookmark>, IDisposable
    {
        #region ICollection
        public abstract void Add(Bookmark item);
        public abstract void Clear();
        public abstract bool Contains(Bookmark item);
        public abstract void CopyTo(Bookmark[] array, int arrayIndex);
        public abstract int  Count { get; }
        public abstract bool IsReadOnly { get; }
        public abstract bool Remove(Bookmark item);
        public abstract IEnumerator<Bookmark> GetEnumerator();
        public int counter;

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IDisposable
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected abstract void Dispose(Boolean disposing);
        #endregion

        #region Additional properties

        public abstract void Add(int lineIndex, string bookmarkName);
        public abstract void Add(int lineIndex);
        public abstract bool Contains(int lineIndex);
        public abstract bool Remove(int lineIndex);
        public abstract Bookmark GetBookmark(int i);

        #endregion
    }

    /// <summary>
    /// Collection of bookmarks
    /// </summary>
    public class Bookmarks : BaseBookmarks
    {
        protected FastColoredTextBox tb;
        protected List<Bookmark> items = new List<Bookmark>();

        public Bookmarks(FastColoredTextBox tb, bool isBreakPoints)
        {
            this.tb = tb;
            if (isBreakPoints) {
                tb.LineInserted += tb_LineInserted4Breakpoints;
                tb.LineRemoved  += tb_LineRemoved4Breakpoints;
            } else {
                tb.LineInserted += tb_LineInserted;
                tb.LineRemoved  += tb_LineRemoved;
            }
            
        }

        protected virtual void tb_LineRemoved(object sender, LineRemovedEventArgs e)
        {
            bool needUpdateMarkers = false;
            for (int i=0; i<Count; i++) {
                if (items[i].LineIndex >= e.Index) {
                    needUpdateMarkers = true;
                    if (items[i].LineIndex >= e.Index + e.Count) {
                        items[i].LineIndex = items[i].LineIndex - e.Count;
                        continue;
                    }

                    var was = e.Index <= 0;
                    foreach (var b in items)
                        if (b.LineIndex == e.Index - 1)
                            was = true;

                    if (was) {
                        items.RemoveAt(i);
                        i--;
                    } else {
                        items[i].LineIndex = e.Index - 1;
                    }
                }
            }
            if (needUpdateMarkers) tb.UpdateScrollMarkers();
        }

        protected virtual void tb_LineInserted(object sender, LineInsertedEventArgs e)
        {
            bool needUpdateMarkers = false;
            for (int i = 0; i < Count; i++) {
                if (items[i].LineIndex >= e.Index) {
                    needUpdateMarkers = true;
                    items[i].LineIndex = items[i].LineIndex + e.Count;

                } else if (items[i].LineIndex == e.Index - 1 && e.Count == 1) {
                    needUpdateMarkers = true;
                    if (tb[e.Index - 1].StartSpacesCount == tb[e.Index - 1].Count)
                        items[i].LineIndex = items[i].LineIndex + e.Count;
                }
            }
            if (needUpdateMarkers) tb.UpdateScrollMarkers();
        }

        protected virtual void tb_LineRemoved4Breakpoints(object sender, LineRemovedEventArgs e) {
            var ActiveEditor = HMSEditorNS.HMSEditor.ActiveEditor;
            for (int i = 0; i < Count; i++) {
                int oldIndex = items[i].LineIndex;
                bool needMoveBreakpointInHms = false;
                bool was = false;
                if (items[i].LineIndex >= e.Index) {
                    if (items[i].LineIndex >= e.Index + e.Count) {
                        items[i].LineIndex = items[i].LineIndex - e.Count;
                        needMoveBreakpointInHms = true;

                    } else {

                        was = e.Index <= 0;
                        foreach (var b in items) {
                            if (b.LineIndex == e.Index - 1) { was = true; break; }
                        }
                        if (was) {
                            items.RemoveAt(i);
                            i--;
                        } else {
                            items[i].LineIndex = e.Index - 1;
                            needMoveBreakpointInHms = true;
                        }

                    }

                }

                if (needMoveBreakpointInHms && ActiveEditor != null) {
                    ActiveEditor.OffBreakpointInHms(oldIndex);
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (!was) ActiveEditor.SetBreakpointInHms(items[i].LineIndex);
                    tb.UpdateScrollMarkers();
                }

            }
        }

        protected virtual void tb_LineInserted4Breakpoints(object sender, LineInsertedEventArgs e) {
            var ActiveEditor = HMSEditorNS.HMSEditor.ActiveEditor;
            for (int i = 0; i < Count; i++) {
                int oldIndex = items[i].LineIndex;
                bool needMoveBreakpointInHms = false;

                if (items[i].LineIndex >= e.Index) {
                    items[i].LineIndex = items[i].LineIndex + e.Count;
                    needMoveBreakpointInHms = true;

                } else if (items[i].LineIndex == e.Index - 1 && e.Count == 1) {
                    if (tb[e.Index - 1].StartSpacesCount == tb[e.Index - 1].Count) {
                        items[i].LineIndex = items[i].LineIndex + e.Count;
                        needMoveBreakpointInHms = true;
                    }
                }
                if (needMoveBreakpointInHms && ActiveEditor != null) {
                    ActiveEditor.OffBreakpointInHms(oldIndex);
                    ActiveEditor.SetBreakpointInHms(items[i].LineIndex);
                    tb.UpdateScrollMarkers();
                }
            }
        }

        protected override void Dispose(bool disposible)
        {
            if (!tb.IsDisposed) {
                tb.LineInserted -= tb_LineInserted;
                tb.LineRemoved  -= tb_LineRemoved;
            }
            GC.SuppressFinalize(this);
        }

        public override IEnumerator<Bookmark> GetEnumerator()
        {
            foreach (var item in items)
                yield return item;
        }

        public override void Add(int lineIndex, string bookmarkName)
        {
            Add(new Bookmark(tb, bookmarkName ?? "Bookmark " + counter, lineIndex));
        }

        public override void Add(int lineIndex)
        {
            Add(new Bookmark(tb, "Bookmark " + counter, lineIndex));
        }

        public void Set(int lineIndex, string name) {
            bool exist = false; string namel = name.ToLower();
            foreach (var b in this) if (b.Name.ToLower() == namel) { b.LineIndex = lineIndex; exist = true; break; }
            if (!exist)
                Add(new Bookmark(tb, name, lineIndex));
            tb.NeedRecalc(true); // By WendyH
            tb.Invalidate();
        }

        public Bookmark GetByName(string name) {
            Bookmark bookmark = null; string namel = name.ToLower();
            foreach (var b in this) if (b.Name.ToLower() == namel) { bookmark = b; break; }
            return bookmark;
        }

        public override void Clear()
        {
            items.Clear();
            tb.NeedRecalc(true); // By WendyH
            counter = 0;
        }

        public override void Add(Bookmark bookmark)
        {
            foreach (var bm in items)
                if (bm.LineIndex == bookmark.LineIndex)
                    return;

            items.Add(bookmark);
            counter++;
            tb.NeedRecalc(true); // By WendyH
            tb.Invalidate();
        }

        public override bool Contains(Bookmark item)
        {
            return items.Contains(item);
        }

        public override bool Contains(int lineIndex)
        {
            foreach (var item in items)
                if (item.LineIndex == lineIndex)
                    return true;
            return false;
        }

        public override void CopyTo(Bookmark[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        public override int Count => items.Count;

        public override bool IsReadOnly => false;

        public override bool Remove(Bookmark item)
        {
            // By WendyH < -----
            bool removed = items.Remove(item);
            tb.NeedRecalc(true);
            tb.Invalidate();
            return removed;
            // By WendyH > -----
        }

        /// <summary>
        /// Removes bookmark by line index
        /// </summary>
        public override bool Remove(int lineIndex)
        {
            bool was = false;
            for (int i = 0; i < Count; i++)
            if (items[i].LineIndex == lineIndex)
            {
                items.RemoveAt(i);
                i--;
                was = true;
            }
            tb.NeedRecalc(true); // By WendyH
            tb.Invalidate();

            return was;
        }

        /// <summary>
        /// Returns Bookmark by index.
        /// </summary>
        public override Bookmark GetBookmark(int i)
        {
            return items[i];
        }

    }

    /// <summary>
    /// Bookmark of FastColoredTextbox
    /// </summary>
    public class Bookmark
    {
        public FastColoredTextBox TB { get; }
        /// <summary>
        /// Name of bookmark
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Line index
        /// </summary>
        public int LineIndex {get; set; }
        /// <summary>
        /// Color of bookmark sign
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Index of char
        /// </summary>
        public int CharIndex { get; set; } // By WendyH

        /// <summary>
        /// Scroll textbox to the bookmark
        /// </summary>
        public virtual void DoVisible()
        {
            // By WendyH < -----------------
            if (CharIndex >= TB.Lines[0].Length) CharIndex = 0;
            // By WendyH > -----------------
            TB.Selection.Start = new Place(0, LineIndex);
            TB.DoRangeVisible(TB.Selection, true);
            TB.Invalidate();
        }

        public Bookmark(FastColoredTextBox tb, string name, int lineIndex)
        {
            TB = tb;
            Name = name;
            LineIndex = lineIndex;
            CharIndex = tb.Selection.Start.iChar; // By WendyH
            Color = tb.BookmarkColor;
        }

        public static Font TextFont  = new Font("Arial", 6);
        public virtual void Paint(Graphics gr, Rectangle lineRect, bool itsreakpoint = false) {
            var size = TB.CharHeight - 1;
            // By WendyH < ---------------------------------------
            if (itsreakpoint && (TB.BreakpointIcon != null)) {
                gr.DrawImage(TB.BreakpointIcon, 0, lineRect.Top, lineRect.Height, lineRect.Height);
            } else if (TB.BookmarkIcon != null) {
                gr.DrawImage(TB.BookmarkIcon, 0, lineRect.Top, lineRect.Height, lineRect.Height);
            } else {
            // By WendyH > ---------------------------------------
                using (var brush = new LinearGradientBrush(new Rectangle(0, lineRect.Top, size, size), Color.White, Color, 45))
                    gr.FillEllipse(brush, 0, lineRect.Top, size, size);
                using (var pen = new Pen(Color))
                    gr.DrawEllipse(pen, 0, lineRect.Top, size, size);
            }
            if (Name.Length == 1) gr.DrawString(Name, TextFont, Brushes.DarkSlateGray, new Point(3, lineRect.Top+(int)((TB.CharHeight - TextFont.Size) /4)));
        }
    }
}
