using System.Collections.Generic;

namespace FastColoredTextBoxNS {

    public class MultilineCommentsInfo {
        public int  Index => (iLine << 16) + iChar;
        public int  iLine;
        public int  iChar;
        public bool IsEnd;
        public MultilineCommentsInfo(int iline, int ichar, bool end) {
            iLine = iline;
            iChar = ichar;
            IsEnd = end;
        }
    }

    public class MultilineComments: List<MultilineCommentsInfo> {

        public int ToIndex(int iLine, int iChar) {
            return (iLine << 16) + iChar;
        }

        public bool IsComment(int iLine, int iChar) {
            if (Count < 1) return false;
            for (int i = Count - 1; i >= 0; i--) {
                var item = this[i];
                if (iLine > item.iLine || (iLine == item.iLine && iChar >= item.iChar))
                    return !item.IsEnd;
            }
            return false;
        }

        public void ClearRange(Range range) {
            if (Count < 1) return;
            for (int i = Count - 1; i >= 0; i--) {
                var item = this[i];
                if ((item.iLine >= range.Start.iLine && item.iLine <= range.End.iLine))
                    RemoveAt(i);
            }
        }

        public void AddStart(Range range) {
            Add(new MultilineCommentsInfo(range.Start.iLine, range.Start.iChar, false));
        }

        public void AddEnd(Range range) {
            Add(new MultilineCommentsInfo(range.End.iLine, range.End.iChar, true));
        }

        public void LinesInserted(int iLine, int count) {
            if (Count < 1) return;
            iLine--;
            for (int i = Count - 1; i >= 0; i--) {
                var item = this[i];
                if (item.iLine > iLine) {
                    item.iLine += count;
                    if (item.iLine < 0)
                        RemoveAt(i);
                }
            }
        }

        public void LinesRemoved(int iLine, int count) {
            LinesInserted(iLine, -count);
        }

        public void SortIndex() {
            Sort((x, y) => x.Index.CompareTo(y.Index) * 2 - x.IsEnd.CompareTo(y.IsEnd));
        }
    }
}
