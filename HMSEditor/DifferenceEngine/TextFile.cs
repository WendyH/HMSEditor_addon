using System;
using System.IO;
using System.Collections;

namespace DifferenceEngine {
    public class TextLine: IComparable {
        public string Line;
        public int _hash;

        public TextLine(string str) {
            Line = str.Replace("\t", "    ");
            _hash = str.GetHashCode();
        }
        #region IComparable Members

        public int CompareTo(object obj) {
            return _hash.CompareTo(((TextLine)obj)._hash);
        }

        #endregion
    }


    public class DiffList_TextFile: IDiffList {
        private const int MaxLineLength = 1024;
        private ArrayList _lines;

        public DiffList_TextFile() {

        }

        public DiffList_TextFile(string text, bool byChars) {
            _lines = new ArrayList();
            if (string.IsNullOrEmpty(text)) return;
            if (byChars) {
                foreach (char ch in text.ToCharArray())
                    _lines.Add(new TextLine(ch.ToString()));
            } else {
                foreach (string line in System.Text.RegularExpressions.Regex.Split(text, "\r\n|\r|\n")) {
                    if (line.Length > MaxLineLength) {
                        throw new InvalidOperationException(
                            string.Format("Файл содержит строки длиной более {0} симоволов. Такие файлы не поддерживаются.",
                                MaxLineLength.ToString()));
                    }
                    _lines.Add(new TextLine(line));
                }
            }
        }

        #region IDiffList Members

        public int Count() {
            return _lines.Count;
        }

        public IComparable GetByIndex(int index) {
            return (TextLine)_lines[index];
        }

        #endregion

    }
}