﻿using System.Collections.Generic;
using FastColoredTextBoxNS;

namespace HMSEditorNS {
	public class HMSItem: AutocompleteItem {
		public string  Help      = "";
		public string  Type      = "";
		public bool    Global    = false;
		public DefKind Kind      = DefKind.NotDef;
		public string  Value     = "";
		public bool    IsClass   = false;
		public List<string> Params = new List<string>();
		public int PositionReal  = 0;
		public int PositionStart = 0;
		public int PositionEnd   = 0;
		public string  Filter    = "";

		// constructors
		public HMSItem() {
		}

		public HMSItem(string text) {
			Text = text;
		}

		public HMSItem(string text, int imageIndex)
			: this(text) {
			base.ImageIndex = imageIndex;
		}

		public HMSItem(string text, int imageIndex, string menuText)
			: this(text, imageIndex) {
			base.MenuText = menuText;
		}

		public HMSItem(string text, int imageIndex, string menuText, string toolTipTitle, string toolTipText)
			: this(text, imageIndex, menuText) {
			base.ToolTipTitle = toolTipTitle;
			base.ToolTipText  = toolTipText;
		}

		/// <summary>
		/// This method is called after item inserted into text
		/// </summary>
		public override void OnSelected(AutocompleteMenu popupMenu, SelectedEventArgs e) {
			if (e.Item.Text.IndexOf('^') < 0) return;
			e.Tb.BeginUpdate();
			e.Tb.Selection.BeginUpdate();
			//remember places
			var p1 = popupMenu.Fragment.Start;
			var p2 = e.Tb.Selection.Start;
			e.Tb.Selection.Start = p1;
			//move caret position right and find char ^
			while (e.Tb.Selection.CharBeforeStart != '^')
				if (!e.Tb.Selection.GoRightThroughFolded())
					break;
			//remove char ^
			e.Tb.Selection.GoLeft(true);
			e.Tb.InsertText("");
			//
			e.Tb.Selection.EndUpdate();
			e.Tb.EndUpdate();
		}

		public bool IsFuncOrProcedure { get { return ((Kind == DefKind.Function) || (Kind == DefKind.Procedure)); } }

	}

	public class HMSItemComparer: IComparer<HMSItem> {
		private readonly string name;

		public HMSItemComparer(string name) {
			this.name = name.ToLower();
		}

		public int Compare(HMSItem item1, HMSItem item2) {
			return item1.MenuText.ToLower().CompareTo(name);
		}
	}

	public enum DefKind { NotDef, Variable, Function, Procedure, Class, Method, Property, Constant, Event, Other }
}