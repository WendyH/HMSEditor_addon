using System;
using System.Collections.Generic;
using System.Text;

namespace HMSEditorNS {
	public class AutocompleteItems: List<HMSItem> {
		public int LastEndPosition { get { if (Count > 0) return this[Count - 1].PositionEnd; return 0; } }

		public void SortByMenuText() {
			this.Sort(delegate (HMSItem a, HMSItem b) { return a.MenuText.CompareTo(b.MenuText); });
		}

		public HMSItem GetItemOrNull(string name) {
			name = name.Trim().ToLower();
			foreach (HMSItem o in this) if (o.MenuText.ToLower() == name) return o;
			return null;
		}

		public bool ContainsName(string name) {
			name = name.ToLower();
			foreach (var o in this) if (o.MenuText.ToLower() == name) return true;
			return false;
		}

		public HMSItem this[string name] {
			get {
				name = name.Trim().ToLower();
				foreach (HMSItem o in this) if (o.MenuText.ToLower() == name) return o;
				return new HMSItem();
			}
		}

		public AutocompleteItems GetFilteredList(string type) {
			AutocompleteItems list = new AutocompleteItems();
			type = type.ToLower();
			foreach (var item in this) if (item.Type.ToLower() == type) list.Add(item);
			return list;
		}

	}
}
