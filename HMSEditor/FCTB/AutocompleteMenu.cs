using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using System.Security.Permissions;
using HMSEditorNS;

namespace FastColoredTextBoxNS
{
    /// <summary>
    /// Popup menu for autocomplete
    /// </summary>
    [Browsable(false)]
    public class AutocompleteMenu : ToolStripDropDown
    {
		// < By WendyH -------------------------------
		public  string[]  lastwords = new string[20];
		public  bool      OnlyCtrlSpace = false;
		public  bool      AfterComplete = false;
		public  string    Filter    = "";
		// > By WendyH -------------------------------

		private AutocompleteListView listView;
        public  ToolStripControlHost host;
        public  Range Fragment { get; internal set; }

        /// <summary>
        /// Regex pattern for serach fragment around caret
        /// </summary>
        public string SearchPattern { get; set; }
        /// <summary>
        /// Minimum fragment length for popup
        /// </summary>
        public int MinFragmentLength { get; set; }
        /// <summary>
        /// User selects item
        /// </summary>
        public event EventHandler<SelectingEventArgs> Selecting;
        /// <summary>
        /// It fires after item inserting
        /// </summary>
        public event EventHandler<SelectedEventArgs> Selected;
        /// <summary>
        /// Occurs when popup menu is opening
        /// </summary>
        public new event EventHandler<CancelEventArgs> Opening;
        /// <summary>
        /// Allow TAB for select menu item
        /// </summary>
        public bool AllowTabKey { get { return listView.AllowTabKey; } set { listView.AllowTabKey = value; } }
        /// <summary>
        /// Interval of menu appear (ms)
        /// </summary>
        public int AppearInterval { get { return listView.AppearInterval; } set { listView.AppearInterval = value; } }

        /// <summary>
        /// Back color of selected item
        /// </summary>
        [DefaultValue(typeof(Color), "Orange")]
        public Color SelectedColor
        {
            get { return listView.SelectedColor; }
            set { listView.SelectedColor = value; }
        }

        /// <summary>
        /// Border color of hovered item
        /// </summary>
        [DefaultValue(typeof(Color), "Red")]
        public Color HoveredColor
        {
            get { return listView.HoveredColor; }
            set { listView.HoveredColor = value; }
        }

        public AutocompleteMenu(FastColoredTextBox tb, HMSEditor ed)
        {
			// create a new popup and add the list view to it 
            AutoClose = false;
            AutoSize  = false;
            Margin    = Padding.Empty;
            Padding   = Padding.Empty;
            BackColor = Color.White;
            listView  = new AutocompleteListView(tb);
            host      = new ToolStripControlHost(listView);
            host.Margin      = new Padding(2, 2, 2, 2);
            host.Padding     = Padding.Empty;
            host.AutoSize    = false;
            host.AutoToolTip = false;
            CalcSize();
            base.Items.Add(host);
            listView.Parent   = this;
            SearchPattern     = @"[\{\#\w\.]";
            MinFragmentLength = 2;
        }

        public new Font Font
        {
            get { return listView.Font ; }
            set { listView.Font = value; }
        }

        new internal void OnOpening(CancelEventArgs args)
        {
			if (Opening != null)
                Opening(this, args);
        }

        public new void Close()
        {
            listView.toolTip.Hide(listView);
            base.Close();
        }

        internal void CalcSize()
        {
            host.Size = listView.Size;
            Size = new Size(listView.Size.Width + 4, listView.Size.Height + 4);
        }

        public virtual void OnSelecting()
        {
            listView.OnSelecting();
        }

        public void SelectNext(int shift)
        {
            listView.SelectNext(shift);
        }

        internal void OnSelecting(SelectingEventArgs args)
        {
            if (Selecting != null)
				Selecting(this, args);
		}

        public void OnSelected(SelectedEventArgs args)
        {
			if (lastwords[0] != args.Item.MenuText) {
				Array.Copy(lastwords, 0, lastwords, 1, lastwords.Length - 1); // By WendyH
				lastwords[0] = args.Item.MenuText;
			}
			if (Selected != null)
				Selected(this, args);
        }

        public new AutocompleteListView Items
        {
            get { return listView; }
        }

        /// <summary>
        /// Shows popup menu immediately
        /// </summary>
        /// <param name="forced">If True - MinFragmentLength will be ignored</param>
        public void Show(bool forced)
        {
            Items.DoAutocomplete(forced);
        }

        /// <summary>
        /// Minimal size of menu
        /// </summary>
        public new Size MinimumSize
        {
            get { return Items.MinimumSize; }
            set { Items.MinimumSize = value; }
        }

        /// <summary>
        /// Image list of menu
        /// </summary>
        public new ImageList ImageList
        {
            get { return Items.ImageList; }
            set { Items.ImageList = value; }
        }

        /// <summary>
        /// Tooltip duration (ms)
        /// </summary>
        public int ToolTipDuration
        {
            get { return Items.ToolTipDuration; }
            set { Items.ToolTipDuration = value; }
        }

        /// <summary>
        /// Tooltip
        /// </summary>
        public HmsToolTip ToolTip
        {
            get { return Items.toolTip; }
            set { Items.toolTip = value; }
        }

		private void InitializeComponent() {
			this.SuspendLayout();
			this.ResumeLayout(false);

		}
    }

	[System.ComponentModel.ToolboxItem(false)]
	public class AutocompleteListView: UserControl {
		string LastSelected = "";
		public event EventHandler FocussedItemIndexChanged;

		internal AutocompleteItems visibleItems;
		AutocompleteItems sourceItems = new AutocompleteItems();
		public AutocompleteItems VisibleVariables = new AutocompleteItems();
		public AutocompleteItems VisibleLocalVars = new AutocompleteItems();
		public AutocompleteItems VisibleFunctions = new AutocompleteItems();

		int focussedItemIndex = 0;
		int hoveredItemIndex = -1;

		private int ItemHeight {
			get { return Font.Height + 2; }
		}

		AutocompleteMenu Menu { get { return Parent as AutocompleteMenu; } }
		int oldItemCount = 0;
		FastColoredTextBox tb;
		internal HmsToolTip toolTip = new HmsToolTip();

		System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

		internal bool AllowTabKey { get; set; }
		public ImageList ImageList { get; set; }
		internal int AppearInterval { get { return timer.Interval; } set { timer.Interval = value; } }
		internal int ToolTipDuration { get; set; }

		public Color SelectedColor { get; set; }
		public Color HoveredColor { get; set; }
		public int FocussedItemIndex {
			get { return focussedItemIndex; }
			set {
				if (focussedItemIndex != value) {
					focussedItemIndex = value;
					if (FocussedItemIndexChanged != null)
						FocussedItemIndexChanged(this, EventArgs.Empty);
				}
			}
		}

		public HMSItem FocussedItem {
			get {
				if (FocussedItemIndex >= 0 && focussedItemIndex < visibleItems.Count)
					return visibleItems[focussedItemIndex];
				return null;
			}
			set {
				FocussedItemIndex = visibleItems.IndexOf(value);
			}
		}

		internal AutocompleteListView(FastColoredTextBox tb) {
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
			if (HMS.PFC.Families.Length > 0) { // By WendyH
				base.Font = new Font(HMS.PFC.Families[0], 10f, FontStyle.Regular, GraphicsUnit.Point);
			} else {
				base.Font = new Font("Segoe UI", 9, FontStyle.Regular, GraphicsUnit.Point);
			}
			visibleItems    = new AutocompleteItems();
			VerticalScroll.SmallChange = ItemHeight;
			MaximumSize     = new Size(Size.Width, 180);
			toolTip.ShowAlways = false;
			AppearInterval  = 250;
			timer.Tick     += new EventHandler(timer_Tick);
			SelectedColor   = Color.Orange;
			HoveredColor    = Color.Red;
			ToolTipDuration = 10000;
			this.tb = tb;

			tb.KeyDown          += new KeyEventHandler(tb_KeyDown);
			tb.SelectionChanged += new EventHandler(tb_SelectionChanged);
			tb.KeyPressed       += new KeyPressEventHandler(tb_KeyPressed);

			Form form = tb.FindForm();
			if (form != null) {
				form.LocationChanged += (o, e) => Menu.Close();
				form.ResizeBegin     += (o, e) => Menu.Close();
				form.FormClosing     += (o, e) => Menu.Close();
				form.LostFocus       += (o, e) => Menu.Close();
			}

			tb.LostFocus += (o, e) => {
				if (!Menu.Focused) Menu.Close();
			};

			tb.Scroll += (o, e) => Menu.Close();

			this.VisibleChanged += (o, e) => {
				if (this.Visible)
					DoSelectedVisible();
			};
		}

		void tb_KeyPressed(object sender, KeyPressEventArgs e) {
			if (Menu.OnlyCtrlSpace) return; // By WendyH
			bool backspaceORdel = e.KeyChar == '\b' || e.KeyChar == 0xff;

			/*
            if (backspaceORdel)
                prevSelection = tb.Selection.Start;*/

			if (Menu.Visible && !backspaceORdel)
				DoAutocomplete(false);
			else
				ResetTimer(timer);
		}

		void timer_Tick(object sender, EventArgs e) {
			timer.Stop();
			DoAutocomplete(false);
		}

		void ResetTimer(System.Windows.Forms.Timer timer) {
			timer.Stop();
			timer.Start();
		}

		internal void DoAutocomplete() {
			DoAutocomplete(false);
		}

		// < By WendyH ------------------------------------------ 
		public HMSItem GetHMSItemByText(string text, out string partAfterDot, bool returnItemBeforeDot = false) {
			HMSItem item = null;
			HMSClassInfo info = new HMSClassInfo();

			string[] names = text.ToLower().Split('.');
			int count = 0; partAfterDot = "";
			foreach (string word in names) {
				string name = HMS.GetTextWithoutBrackets(word);
                count++; partAfterDot = name;
				if (returnItemBeforeDot && (count >= names.Length)) break; // return last item before the dot
				if (info.Name.Length > 0) {
					// search in class members
					item = info.MemberItems.GetItemOrNull(name);
					if (item == null) item = info.StaticItems.GetItemOrNull(name);
					if (item != null) info = HMS.HmsClasses[item.Type];
				} else {
					// try get variabe
					if (item == null) item = Menu.Items.VisibleLocalVars.GetItemOrNull(name); // try visible known variables
					if (item == null) item = Menu.Items.VisibleVariables.GetItemOrNull(name); // try visible known variables
					if (item == null) item = Menu.Items.VisibleFunctions.GetItemOrNull(name); // try functions in script
					if (item == null) item = HMS.ItemsVariable.GetItemOrNull(name); // try internal variables
					if (item == null) item = HMS.ItemsConstant.GetItemOrNull(name); // try internal constants
					if (item == null) item = HMS.ItemsFunction.GetItemOrNull(name); // try internal functions
					if (item == null) item = HMS.ItemsClass.GetItemOrNull(name); // try internal classes
					if (count < names.Length) {
						if (item != null) info = HMS.HmsClasses[item.Type];
						else break;
					}
				}
			}
			return item;
		}

		internal void SetVisibleMethods(string text) {
			string  part = ""; HMSClassInfo info;
            HMSItem item = GetHMSItemByText(text, out part, true);
			if (item != null) { 
				if (item.IsClass) {
					info = HMS.HmsClasses[item.MenuText];
					if (!String.IsNullOrEmpty(info.Name))
						foreach (HMSItem childItem in info.StaticItems) if (childItem.MenuText.ToLower().StartsWith(part)) { childItem.Parent = Menu; visibleItems.Add(childItem); }
				} else {
					info = HMS.HmsClasses[item.Type];  // variable founded - search type in classes
					if (!String.IsNullOrEmpty(info.Name))
						foreach (HMSItem childItem in info.MemberItems) if (childItem.MenuText.ToLower().StartsWith(part)) { childItem.Parent = Menu; visibleItems.Add(childItem); }
				}
			}
		}

		internal void SetVisibleTypes(string text) {
			string part = "";
			HMSItem item = GetHMSItemByText(text, out part);
			if (item != null) {
				if (item.Type.ToLower().StartsWith("bool")) {
					visibleItems.AddRange(HMS.ItemsBoolean);
					return;
				}
				HMSClassInfo info = HMS.HmsTypes[item.Type];
				if (!string.IsNullOrEmpty(info.Name))
					visibleItems.AddRange(info.MemberItems);
			}
		}

		internal string GetLastSelectedWord(string text) {
			string lastword = "";
			foreach (var word in Menu.lastwords) {
				if (String.IsNullOrEmpty(word)) continue;
				if (word.IndexOf(text, StringComparison.InvariantCultureIgnoreCase)>=0 && word != text) {
					lastword = word; break;
				}
			}
			return lastword;
		}

		internal bool AddInVisibleItems(string text, string lastword, AutocompleteItems ITEMS, bool foundSelected) {
			// set active lastword in visible items
			foreach (var item in ITEMS) {
				item.Parent = Menu;
				CompareResult res = item.Compare(text);
				if (res != CompareResult.Hidden) visibleItems.Add(item);
				if (lastword.Length > 0) {
					if (item.MenuText == lastword) {
						foundSelected = true;
						FocussedItemIndex = visibleItems.Count - 1;
					}
				} else if (res == CompareResult.VisibleAndSelected && !foundSelected) {
					foundSelected = true;
					FocussedItemIndex = visibleItems.Count - 1;
				}
			}
			return foundSelected;
		}

		internal bool GetActiveLastwordInVisibleItems(string text, string lastword) {
			bool foundSelected = false;
			foundSelected = AddInVisibleItems(text, lastword, VisibleVariables, foundSelected);
			foundSelected = AddInVisibleItems(text, lastword, VisibleLocalVars, foundSelected);
			foundSelected = AddInVisibleItems(text, lastword, VisibleFunctions, foundSelected);
			return foundSelected;
        }
		// > By WendyH ------------------------------------------ 


		internal void DoAutocomplete(bool forced) {
			if (tb.IsDisposed) return;
			if (!Menu.Enabled || !this.Enabled) { Menu.Close(); return; }
			if (tb.CheckInTheStringOrComment()) return;
			if (!forced && Menu.AfterComplete) { Menu.AfterComplete = false; return; }
			visibleItems.Clear();
			FocussedItemIndex = 0;
			VerticalScroll.Value = 0;
			//some magic for update scrolls
			AutoScrollMinSize -= new Size(1, 0);
			AutoScrollMinSize += new Size(1, 0);
			//get fragment around caret

			//Range fragment = tb.Selection.GetFragment(Menu.SearchPattern);
			Range fragment = tb.Selection.GetFragmentLookedLeft();
			string text = fragment.Text;
			// < By WendyH ------------------------
			bool doNotGetFromSourceItems = false; bool showTypes = false;
			if (text.Length == 0) {
				if (tb.ToolTip4Function.Visible && (HMS.CurrentParamType.Length > 0)) {
					if (HMS.CurrentParamType == "boolean") { visibleItems.AddRange(HMS.ItemsBoolean); doNotGetFromSourceItems = true; Menu.Fragment = fragment; }
					else if (HMS.HmsTypes.ContainsName(HMS.CurrentParamType)) {
						HMSClassInfo info = HMS.HmsTypes[HMS.CurrentParamType];
						visibleItems.AddRange(info.MemberItems);
						doNotGetFromSourceItems = true;
						Menu.Fragment = fragment;
					}
				} else {
					text = tb.Selection.GetVariableForEqual(Menu.SearchPattern);
					if (text.Length > 0) {
						doNotGetFromSourceItems = true;
						forced = true;
						showTypes = true;
					}
				}
			}
			// > By WendyH ------------------------
			//calc screen point for popup menu
			Point point = tb.PlaceToPoint(fragment.End);
			point.Offset(2, tb.CharHeight);
			// By WendyH
			if (tb.ToolTip4Function.Visible) {
				Rectangle b = tb.ToolTip4Function.Bounds;
				point.Y += b.Height + 4;
			}
			//
			if (forced || (text.Length >= Menu.MinFragmentLength 
                && tb.Selection.IsEmpty /*pops up only if selected range is empty*/
                && (tb.Selection.Start > fragment.Start || text.Length == 0/*pops up only if caret is after first letter*/)))
            {
                Menu.Fragment = fragment;
                bool foundSelected = false;
				//build popup menu
				// < By WendyH -------------------------------------
				string lastword = GetLastSelectedWord(text);
				if (showTypes) SetVisibleTypes(text);
				else {
					int indexDot = text.IndexOf(".");
					if (indexDot > 0) {
						SetVisibleMethods(text);
						doNotGetFromSourceItems = true;
						Menu.Fragment.ShiftStart(text.LastIndexOf('.') + 1);
					} else {
						foundSelected = GetActiveLastwordInVisibleItems(text, lastword);
					}
				}
				// > By WendyH -------------------------------------
				if (!doNotGetFromSourceItems) {
					AutocompleteItems notExacctly = new AutocompleteItems();
					foreach (var item in sourceItems) {
						item.Parent = Menu;
						CompareResult res = item.Compare(text);
						if (res != CompareResult.Hidden)
							visibleItems.Add(item);
						else if (item.NotExactlyCompare(text) == CompareResult.Visible)
							notExacctly.Add(item);
						if (lastword.Length > 0) {
							if (item.MenuText == lastword) {
								foundSelected = true;
								FocussedItemIndex = visibleItems.Count - 1;
							}
						} else if (res == CompareResult.VisibleAndSelected && !foundSelected) {
							foundSelected = true;
							FocussedItemIndex = visibleItems.Count - 1;
						}
						if (visibleItems.Count > 150) break;
					}
					visibleItems.AddRange(notExacctly);
				}

				if (foundSelected)
                {
                    AdjustScroll();
                    //DoSelectedVisible();
                }
            }
			if ((visibleItems.Count == 1) && (visibleItems[0].MenuText!=null) && (visibleItems[0].MenuText.ToLower() == text.ToLower())) return;
			//show popup menu
			if (Count > 0)
            {
                if (!Menu.Visible)
                {
                    CancelEventArgs args = new CancelEventArgs();
					//// By WendyH
					//Point ps = tb.PointToScreen(point);
					//if (ps.Y + Menu.Height + (tb.CharHeight * 3) > SystemInformation.VirtualScreen.Bottom) {
					//	int size = Math.Min(MaximumSize.Height, ItemHeight * visibleItems.Count) + 5;
					//	point.Y -= (size + tb.CharHeight);
					//	Rectangle b = tb.ToolTip4Function.Bounds;
					//	if (tb.ToolTip4Function.Active) point.Y -= (b.Height + 4);
					//}
					Menu.OnOpening(args);
                    if(!args.Cancel)
                        Menu.Show(tb, point);
				}
                else {
					Invalidate();
					DoSelectedVisible();
				}
			}
            else
                Menu.Close();
        }

        void tb_SelectionChanged(object sender, EventArgs e)
        {
            /*
            FastColoredTextBox tb = sender as FastColoredTextBox;
            
            if (Math.Abs(prevSelection.iChar - tb.Selection.Start.iChar) > 1 ||
                        prevSelection.iLine != tb.Selection.Start.iLine)
                Menu.Close();
            prevSelection = tb.Selection.Start;*/
            if (Menu.Visible)
            {
                bool needClose = false;

                if (!tb.Selection.IsEmpty)
                    needClose = true;
                else
                    if (!Menu.Fragment.Contains(tb.Selection.Start))
                    {
                        if (tb.Selection.Start.iLine == Menu.Fragment.End.iLine && tb.Selection.Start.iChar == Menu.Fragment.End.iChar + 1)
                        {
                            //user press key at end of fragment
                            char c = tb.Selection.CharBeforeStart;
                            if (!Regex.IsMatch(c.ToString(), Menu.SearchPattern))//check char
                                needClose = true;
                        }
                        else
                            needClose = true;
                    }

                if (needClose)
                    Menu.Close();
            }
            
        }

        void tb_KeyDown(object sender, KeyEventArgs e)
        {
            var tb = sender as FastColoredTextBox;

			if (Menu.Visible)
                if (ProcessKey(e.KeyCode, e.Modifiers))
                    e.Handled = true;

            if (!Menu.Visible)
            {
                if (tb.HotkeysMapping.ContainsKey(e.KeyData) && tb.HotkeysMapping[e.KeyData] == FCTBAction.AutocompleteMenu)
                {
                    DoAutocomplete();
                    e.Handled = true;
                }
                else
                {
					if (Menu.OnlyCtrlSpace && timer.Enabled) timer.Stop(); // By WendyH
                    if (e.KeyCode == Keys.Escape && timer.Enabled)
                        timer.Stop();
                }
            }
        }

        void AdjustScroll()
        {
            if (oldItemCount == visibleItems.Count)
                return;

            int needHeight = ItemHeight * visibleItems.Count + 1;
            Height = Math.Min(needHeight, MaximumSize.Height);
            Menu.CalcSize();

            AutoScrollMinSize = new Size(0, needHeight);
            oldItemCount = visibleItems.Count;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            AdjustScroll();

            var itemHeight = ItemHeight;
            int startI = VerticalScroll.Value / itemHeight - 1;
            int finishI = (VerticalScroll.Value + ClientSize.Height) / itemHeight + 1;
            startI = Math.Max(startI, 0);
            finishI = Math.Min(finishI, visibleItems.Count);
            int y = 0;
            int leftPadding = 18;
            for (int i = startI; i < finishI; i++)
            {
                y = i * itemHeight - VerticalScroll.Value;

                var item = visibleItems[i];
				e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                if (item.BackColor != Color.Transparent)
                using (var brush = new SolidBrush(item.BackColor))
                    e.Graphics.FillRectangle(brush, 1, y, ClientSize.Width - 1 - 1, itemHeight - 1);

                if (ImageList != null && item.ImageIndex >= 0 && item.ImageIndex < ImageList.Images.Count)
                    e.Graphics.DrawImage(ImageList.Images[item.ImageIndex], 1, y);

                if (i == FocussedItemIndex)
                using (var selectedBrush = new LinearGradientBrush(new Point(0, y - 3), new Point(0, y + itemHeight), Color.Transparent, SelectedColor))
                using (var pen = new Pen(SelectedColor))
                {
                    e.Graphics.FillRectangle(selectedBrush, leftPadding, y, ClientSize.Width - 1 - leftPadding, itemHeight - 1);
                    e.Graphics.DrawRectangle(pen, leftPadding, y, ClientSize.Width - 1 - leftPadding, itemHeight - 1);
                }

                if (i == hoveredItemIndex)
                using(var pen = new Pen(HoveredColor))
                    e.Graphics.DrawRectangle(pen, leftPadding, y, ClientSize.Width - 1 - leftPadding, itemHeight - 1);

                using (var brush = new SolidBrush(item.ForeColor != Color.Transparent ? item.ForeColor : ForeColor))
                    e.Graphics.DrawString(item.ToString(), Font, brush, leftPadding, y + 1);
            }
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                FocussedItemIndex = PointToItemIndex(e.Location);
                DoSelectedVisible();
                Invalidate();
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
			base.OnMouseDoubleClick(e);
            FocussedItemIndex = PointToItemIndex(e.Location);
            Invalidate();
            OnSelecting();
		}

		internal virtual void OnSelecting()
        {
            if (FocussedItemIndex < 0 || FocussedItemIndex >= visibleItems.Count)
                return;
            tb.TextSource.Manager.BeginAutoUndoCommands();
            try
            {
                AutocompleteItem item = FocussedItem;
				LastSelected = item.MenuText; // By WendyH
				SelectingEventArgs args = new SelectingEventArgs()
                {
                    Item = item,
                    SelectedIndex = FocussedItemIndex
                };

                Menu.OnSelecting(args);

                if (args.Cancel)
                {
                    FocussedItemIndex = args.SelectedIndex;
                    Invalidate();
                    return;
                }

                if (!args.Handled)
                {
                    var fragment = Menu.Fragment;
                    DoAutocomplete(item, fragment);
                }

                Menu.Close();
                //
                SelectedEventArgs args2 = new SelectedEventArgs()
                {
                    Item = item,
                    Tb = Menu.Fragment.tb
                };
                item.OnSelected(Menu, args2);
                Menu.OnSelected(args2);
            }
            finally
            {
                tb.TextSource.Manager.EndAutoUndoCommands();
            }
        }

        private void DoAutocomplete(AutocompleteItem item, Range fragment)
        {
            string newText = item.GetTextForReplace();

			//replace text of fragment
			var tb = fragment.tb;
			// < By WendyH ---------------------------
			if (tb.ToolTip4Function.Visible) Menu.AfterComplete = true;
            if (fragment.CharBeforeStart == '=') newText = " " + newText;
			int iLine = fragment.Start.iLine;
			int iChar = fragment.Start.iChar;
			HMSItem hmsItem = item as HMSItem;
			if ((hmsItem != null) && ((hmsItem.Kind == DefKind.Property) || (hmsItem.Kind == DefKind.Variable)) && !HMS.TypeIsClass(hmsItem.Type)) {
				Range fwords = fragment.GetFragmentLookedLeft();
				var f = new Range(tb, new Place(0, iLine), new Place(fwords.Start.iChar, iLine));
				string line = f.Text.Trim();
				if (line.Length == 0) newText += (tb.Language == Language.PascalScript) ? " := " : " = ";
			}

			// > By WendyH ---------------------------

			tb.BeginAutoUndo();
            tb.TextSource.Manager.ExecuteCommand(new SelectCommand(tb.TextSource));
            if (tb.Selection.ColumnSelectionMode)
            {
                var start = tb.Selection.Start;
                var end = tb.Selection.End;
                start.iChar = fragment.Start.iChar;
                end.iChar = fragment.End.iChar;
                tb.Selection.Start = start;
                tb.Selection.End = end;
            }
            else
            {
                tb.Selection.Start = fragment.Start;
                tb.Selection.End = fragment.End;
            }
            tb.InsertText(newText);
            tb.TextSource.Manager.ExecuteCommand(new SelectCommand(tb.TextSource));
            tb.EndAutoUndo();
            tb.Focus();
        }

        int PointToItemIndex(Point p)
        {
            return (p.Y + VerticalScroll.Value) / ItemHeight;
        }

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            ProcessKey(keyData, Keys.None);
            
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private bool ProcessKey(Keys keyData, Keys keyModifiers)
        {
            if (keyModifiers == Keys.None)
            switch (keyData)
            {
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
                case Keys.Tab:
                    if (!AllowTabKey)
                        break;
                    OnSelecting();
                    return true;
                case Keys.Escape:
                    Menu.Close();
                    return true;
            }

            return false;
        }

        public void SelectNext(int shift)
        {
			if ((shift < 0) && (FocussedItemIndex == 0)) return;
			if ((shift > 0) && (FocussedItemIndex >= (visibleItems.Count-1))) return;
			FocussedItemIndex = Math.Max(0, Math.Min(FocussedItemIndex + shift, visibleItems.Count - 1));
            DoSelectedVisible();
            //
            Invalidate();
        }

        private void DoSelectedVisible()
        {
            if (FocussedItem != null)
                SetToolTip(FocussedItem);

			if (FocussedItemIndex >= 0) {
				var y = FocussedItemIndex * ItemHeight - VerticalScroll.Value;
				if (y < 0)
					VerticalScroll.Value = FocussedItemIndex * ItemHeight;
				if (y > ClientSize.Height - ItemHeight)
					VerticalScroll.Value = Math.Min(VerticalScroll.Maximum, FocussedItemIndex * ItemHeight - ClientSize.Height + ItemHeight);

			}
			//some magic for update scrolls
			AutoScrollMinSize -= new Size(1, 0);
            AutoScrollMinSize += new Size(1, 0);
        }

        private void SetToolTip(HMSItem autocompleteItem)
        {
			if (IsDisposed) return;
			IWin32Window window = this.Parent ?? this;
            toolTip.Hide(window);
			if (string.IsNullOrEmpty(autocompleteItem.ToolTipTitle)) return;        // By WendyH
			toolTip.Help         = autocompleteItem.Help;
			toolTip.ToolTipTitle = autocompleteItem.ToolTipTitle;
            string text          = autocompleteItem.ToolTipText;

			Point location = new Point((window == this ? Width : Right) + 3, 0);

			int y = FocussedItemIndex * ItemHeight - VerticalScroll.Value; // By WendyH
			if (y < 0) y = 0;
			if (y > ClientSize.Height - ItemHeight) y = ClientSize.Height - ItemHeight;
			location.Y = y;

			if (text.Length > 0)
				toolTip.Show(text, window, location.X, location.Y, ToolTipDuration);
			else
				toolTip.Show(" ", window, location.X, location.Y, ToolTipDuration);

		}

		public int Count
        {
            get { return visibleItems.Count; }
        }

        public void SetAutocompleteItems(ICollection<string> items)
        {
            AutocompleteItems list = new AutocompleteItems();
            foreach (var item in items)
				list.Add(new HMSItem(item));
			SetAutocompleteItems(list);
        }

        public void SetAutocompleteItems(AutocompleteItems items)
        {
            sourceItems = items;
        }

		public void AddFilteredItems(AutocompleteItems items) {
			AutocompleteItems list = new AutocompleteItems();
			string filter = Menu.Filter;
			foreach (var item in items) {
				if ((filter.Length > 0) && (item.Filter.Length>0) && (filter.IndexOf(item.Filter)<0)) continue;
				if (list.ContainsName(item.MenuText)) continue;
                list.Add(item);
			}
			AddAutocompleteItems(list);
        }

		public void AddAutocompleteItems(AutocompleteItems items) {
			sourceItems.AddRange(items);
		}

		public void SetVisibleVariablesItems(AutocompleteItems items) {
			VisibleVariables = items;
		}

		public void SetLocalssVariablesItems(AutocompleteItems items) {
			VisibleLocalVars = items;
		}

		public void SetVisibleFunctionsItems(AutocompleteItems items) {
			VisibleFunctions = items;
		}

	}

	public class SelectingEventArgs : EventArgs
    {
        public AutocompleteItem Item { get; internal set; }
        public bool Cancel        { get; set; }
        public int  SelectedIndex { get; set; }
        public bool Handled       { get; set; }
    }

    public class SelectedEventArgs : EventArgs
    {
        public AutocompleteItem Item { get; internal set; }
        public FastColoredTextBox Tb { get; set; }
    }
}
