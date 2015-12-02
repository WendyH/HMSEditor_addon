using System.Collections.Generic;

namespace HMSEditorNS {
	public class HMSClassInfo {
		public string Name = "";
		public string Type = "";
		public string Help = "";
		public AutocompleteItems MemberItems = new AutocompleteItems();
		public AutocompleteItems StaticItems = new AutocompleteItems();
	}

	public class HMSClasses: List<HMSClassInfo> {
		public bool ContainsName(string name) {
			name = name.Trim().ToLower();
			foreach (HMSClassInfo o in this) if (o.Name.ToLower() == name) return true;
			return false;
		}

		public HMSClassInfo this[string name] {
			get {
				name = name.Trim().ToLower();
				foreach (HMSClassInfo o in this) if (o.Name.ToLower() == name) return o;
				return new HMSClassInfo();
			}
		}

	}
}
