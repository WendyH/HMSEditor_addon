using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;

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

        public Bookmarks(FastColoredTextBox tb)
        {
            this.tb = tb;
            tb.LineInserted += tb_LineInserted;
            tb.LineRemoved  += tb_LineRemoved;
        }

        protected virtual void tb_LineRemoved(object sender, LineRemovedEventArgs e)
        {
            for(int i=0; i<Count; i++)
            if (items[i].LineIndex >= e.Index)
            {
                if (items[i].LineIndex >= e.Index + e.Count)
                {
                    items[i].LineIndex = items[i].LineIndex - e.Count;
                    continue;
                }

                var was = e.Index <= 0;
                foreach (var b in items)
                    if (b.LineIndex == e.Index - 1)
                        was = true;

                if(was)
                {
                    items.RemoveAt(i);
                    i--;
                }else
                    items[i].LineIndex = e.Index - 1;

                //if (items[i].LineIndex == e.Index + e.Count - 1)
                //{
                //    items[i].LineIndex = items[i].LineIndex - e.Count;
                //    continue;
                //}
                //
                //items.RemoveAt(i);
                //i--;
            }
        }

        protected virtual void tb_LineInserted(object sender, LineInsertedEventArgs e)
        {
            for (int i = 0; i < Count; i++)
                if (items[i].LineIndex >= e.Index)
                {
                    items[i].LineIndex = items[i].LineIndex + e.Count;
                }else
                if (items[i].LineIndex == e.Index - 1 && e.Count == 1)
                {
                    if(tb[e.Index - 1].StartSpacesCount == tb[e.Index - 1].Count)
                        items[i].LineIndex = items[i].LineIndex + e.Count;
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

        public override int Count
        {
            get { return items.Count; }
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

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
        public FastColoredTextBox TB { get; private set; }
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
            this.TB = tb;
            this.Name = name;
            this.LineIndex = lineIndex;
			this.CharIndex = tb.Selection.Start.iChar; // By WendyH
			Color = tb.BookmarkColor;
		}

		private static Font TextFont  = new Font("Arial", 6);
		public virtual void Paint(Graphics gr, Rectangle lineRect, bool itsreakpoint = false) {
			var size = TB.CharHeight - 1;
			// By WendyH < ---------------------------------------
			if (itsreakpoint && (TB.BreakpointIcon != null)) {
				gr.DrawImage(TB.BreakpointIcon, 0, lineRect.Top, TB.BreakpointIcon.Width, TB.BreakpointIcon.Height);
			} else if (TB.BookmarkIcon != null) {
				gr.DrawImage(TB.BookmarkIcon, 0, lineRect.Top, TB.BookmarkIcon.Width, TB.BookmarkIcon.Height);
			} else {
			// By WendyH > ---------------------------------------
				using (var brush = new LinearGradientBrush(new Rectangle(0, lineRect.Top, size, size), Color.White, Color, 45))
					gr.FillEllipse(brush, 0, lineRect.Top, size, size);
				using (var pen = new Pen(Color))
					gr.DrawEllipse(pen, 0, lineRect.Top, size, size);
			}
			if (Name.Length == 1) gr.DrawString(Name, TextFont, Brushes.DarkSlateGray, new Point(4, lineRect.Top+3));
		}
	}
}
