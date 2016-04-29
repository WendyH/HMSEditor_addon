using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Security.Permissions;
using HMSEditorNS;

// ReSharper disable once CheckNamespace
namespace FastColoredTextBoxNS
{
    /// <summary>
    /// Popup menu for autocomplete
    /// </summary>
    [Browsable(false)]
    public class AutocompleteMenu : ToolStripDropDown
    {
        // < By WendyH -------------------------------
        public Dictionary<string, long> LastWords = new Dictionary<string, long>();
        public  bool      OnlyCtrlSpace = false;
        public  bool      AfterComplete;
        public  string    Filter        = "";
        public  bool      TempNotShow;
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

        protected override void OnPaint(PaintEventArgs e) {
            Graphics g = e.Graphics;
            using (SolidBrush brush = new SolidBrush(Color.White)) {
                g.FillRectangle(brush, new Rectangle(0, 0, Width, Height));
            }
            g.DrawRectangle(new Pen(HMS.BordersColor, 1), new Rectangle(0, 0, Width-1, Height-1));
            //base.OnPaint(e);
        }
        private const int grab = 12;

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m) {
            base.WndProc(ref m);
            if (m.Msg == 0x84) {  // Trap WM_NCHITTEST
                var pos = PointToClient(new Point(m.LParam.ToInt32() & 0xffff, m.LParam.ToInt32() >> 16));
                if (pos.X >= ClientSize.Width - grab && pos.Y >= ClientSize.Height - grab)
                    m.Result = new IntPtr(17);  // HT_BOTTOMRIGHT
            }
        }

        //const int WS_BORDER  = 0x800000;
        //const int WS_SIZEBOX = 0x040000;
        //protected override CreateParams CreateParams {
        //    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        //    get { var cp = base.CreateParams; cp.Style |= WS_BORDER; return cp; }
        //}

        public AutocompleteMenu(FastColoredTextBox tb)
        {
            // create a new popup and add the list view to it 
            AutoClose = false;
            AutoSize  = false;
            Margin    = Padding.Empty;
            Padding   = Padding.Empty;
            BackColor = Color.White;
            listView  = new AutocompleteListView(tb);
            host      = new ToolStripControlHost(listView) {
                Margin      = new Padding(2, 2, 2, 2),
                Padding     = Padding.Empty,
                AutoSize    = false,
                AutoToolTip = false
            };
            CalcSize();
            base.Items.Add(host);
            listView.Parent   = this;
            SearchPattern     = @"[\{\#\w\.]";
            MinFragmentLength = 2;
            InitSize = new Size(240, listView.ItemHeight * 15);
            ResizeRedraw = false;
            //listView.VerticalScrollBar.ThumbHoverColor = Color.FromArgb(255, Color.SkyBlue);
        }

        public sealed override bool AutoSize {
            get { return base.AutoSize; }
            set { base.AutoSize = value; }
        }

        public Size InitSize;
        protected override void OnResize(EventArgs e) {
            //base.OnResize(e);
            if (listView!=null) {
                Size newSize = new Size(Size.Width - 4, Size.Height - 4);
                listView.Size        = newSize;
                listView.MaximumSize = newSize;
            }
        }

        public new Font Font
        {
            get { return listView.Font ; }
            set { listView.Font = value; }
        }

        internal new void OnOpening(CancelEventArgs args) {
            Opening?.Invoke(this, args);
        }

        public void InitDefaultSize() {
            Items.MaximumSize = new Size(InitSize.Width, InitSize.Height);
            Items.Width = InitSize.Width;
        }

        public new void Close()
        {
            listView.ToolTip.Hide(listView);
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

        internal void OnSelecting(SelectingEventArgs args) {
            Selecting?.Invoke(this, args);
        }

        public void OnSelected(SelectedEventArgs args)
        {
            if (args.Item.MenuText != null) {
                string word = args.Item.ToString();
                LastWords[word] = System.Diagnostics.Stopwatch.GetTimestamp();
                /*
                if (LastWords.ContainsKey(word))
                    LastWords[word]++;
                else
                    LastWords[word] = 1;
                */
            }

            Selected?.Invoke(this, args);
        }

        public new AutocompleteListView Items => listView;

        /// <summary>
        /// Shows popup menu immediately
        /// </summary>
        /// <param name="forced">If True - MinFragmentLength will be ignored</param>
        public void Show(bool forced)
        {
            InitDefaultSize();
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
            get { return Items.ToolTip; }
            set { Items.ToolTip = value; }
        }

        // < By WendyH -----------------------------------
        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (listView != null && !listView.IsDisposed)
                    listView.Dispose();
                if (host != null && !host.IsDisposed)
                    host.Dispose();
            }
            listView = null;
            host = null;
            Fragment = null;
            base.Dispose(disposing);
        }
        // > By WendyH -----------------------------------

    }

    [ToolboxItem(false)]
    public sealed class AutocompleteListView: UserControl {
        public event EventHandler FocussedItemIndexChanged;

        internal AutocompleteItems visibleItems;
        AutocompleteItems sourceItems = new AutocompleteItems();
        public AutocompleteItems VisibleVariables = new AutocompleteItems();
        public AutocompleteItems VisibleLocalVars = new AutocompleteItems();
        public AutocompleteItems VisibleFunctions = new AutocompleteItems();

        int focussedItemIndex;

        public FlatScrollbar VerticalScrollBar = new FlatScrollbar();

        public int ItemHeight => Font.Height + 2;

        AutocompleteMenu Menu => Parent as AutocompleteMenu;
        int oldItemCount;
        FastColoredTextBox tb;
        public HmsToolTip ToolTip = new HmsToolTip();

        Timer timer = new Timer();



        // < By WendyH -----------------------------------
        protected override void Dispose(bool disposing) {
            if (disposing) {
                ToolTip?.Dispose();
                timer  ?.Dispose();
                VerticalScrollBar?.Dispose();
            }
            FocussedItemIndexChanged = null;
            visibleItems = null;
            sourceItems  = null;
            VisibleVariables = null;
            VisibleLocalVars = null;
            VisibleFunctions = null;
            tb      = null;
            ToolTip = null;
            timer   = null;
            VerticalScrollBar = null;
            base.Dispose(disposing);
            ResizeRedraw = true;
        }
        // > By WendyH -----------------------------------

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
                    FocussedItemIndexChanged?.Invoke(this, EventArgs.Empty);
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
            Controls.Add(VerticalScrollBar);
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (HMS.PFC.Families.Length > 0) { // By WendyH
                Font = new Font(HMS.PFC.Families[0], 9.25f, FontStyle.Regular, GraphicsUnit.Point);
            } else {
                Font = new Font("Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            }
            visibleItems = new AutocompleteItems();
            VerticalScrollBar.SmallChange = ItemHeight;
            VerticalScrollBar.LargeChange = Height;
            MaximumSize     = new Size(Size.Width, ItemHeight * 10);
            ToolTip.ShowAlways = false;
            AppearInterval  = 250;
            timer.Tick     += timer_Tick;
            SelectedColor   = Color.CornflowerBlue;
            HoveredColor    = Color.Red;
            ToolTipDuration = 300000;
            this.tb = tb;
            BorderStyle = BorderStyle.None;
            tb.KeyDown          += tb_KeyDown;
            tb.SelectionChanged += tb_SelectionChanged;
            tb.KeyPressed       += tb_KeyPressed;
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

            VisibleChanged += (o, e) => {
                if (Visible)
                    DoSelectedVisible();
            };
            VScroll = false;
            HScroll = false;
            VerticalScrollBar.ValueChanged += VerticalScrollBar_Scroll;
            DoubleBuffered = true;

        }

        void tb_KeyPressed(object sender, KeyPressEventArgs e) {

            Range r = tb.Selection.Clone();
            r.GoLeft();
            if (r.IsStringOrComment)
                return;

            if (Menu.OnlyCtrlSpace) return; // By WendyH
            bool backspaceORdel = e.KeyChar == '\b' || e.KeyChar == 0xff;

            
            if (backspaceORdel && !Menu.Visible) {
                timer.Stop();
                return;
            }

            if (Menu.Visible && !backspaceORdel)
                DoAutocomplete(false);
            else
                ResetTimer();
        }

        void timer_Tick(object sender, EventArgs e) {
            timer.Stop();
            if (!Menu.TempNotShow) DoAutocomplete(false);
        }

        void ResetTimer() {
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
                    item = info.MemberItems.GetItemOrNull(name) ?? info.StaticItems.GetItemOrNull(name);
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
            string  part;
            HMSItem item = GetHMSItemByText(text, out part, true);
            if (item != null) {
                HMSClassInfo info;
                if (item.IsClass) {
                    info = HMS.HmsClasses[item.MenuText];
                    if (!string.IsNullOrEmpty(info.Name))
                        foreach (HMSItem childItem in info.StaticItems) if (childItem.MenuText.ToLower().StartsWith(part)) { childItem.Parent = Menu; visibleItems.Add(childItem); }
                } else {
                    info = HMS.HmsClasses[item.Type];  // variable founded - search type in classes
                    if (!string.IsNullOrEmpty(info.Name))
                        foreach (HMSItem childItem in info.MemberItems) if (childItem.MenuText.ToLower().StartsWith(part)) { childItem.Parent = Menu; visibleItems.Add(childItem); }
                }
            }
        }

        internal void SetVisibleTypes(string text) {
            string part;
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
            List<KeyValuePair<string, long>> sortedWords = new List<KeyValuePair<string, long>>();
            foreach (var pair in Menu.LastWords) sortedWords.Add(pair);
            sortedWords.Sort((firstPair, nextPair) => nextPair.Value.CompareTo(firstPair.Value));
            foreach (var pair in sortedWords) {
                string word = pair.Key;
                if (word.IndexOf(text, StringComparison.InvariantCultureIgnoreCase)>=0 && word != text) {
                    lastword = word; break;
                }
            }
            return lastword;
        }

        internal bool AddInVisibleItems(string text, string lastword, AutocompleteItems ITEMS, bool foundSelected) {
            if (foundSelected) return true;
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
            var foundSelected = AddInVisibleItems(text, lastword, VisibleVariables, false);
                foundSelected = AddInVisibleItems(text, lastword, VisibleLocalVars, foundSelected);
                foundSelected = AddInVisibleItems(text, lastword, VisibleFunctions, foundSelected);
            return foundSelected;
        }

        private static Regex regexIsPascalFunctionName = new Regex(@"\b(Procedure|Function)\b", RegexOptions.IgnoreCase);
        private static Regex regexSpaces = new Regex(@"[\s]");

        public static bool isType(string word) {
            return (HMS.HmsTypesString).IndexOf("|" + word.ToLower() + "|", StringComparison.Ordinal) >= 0;
        }
        private static Regex regexBeginWithDigit = new Regex("^\\d");
        // > By WendyH ------------------------------------------ 

        internal void DoAutocomplete(bool forced) {
            if (tb.IsDisposed) return;
            if (!Menu.Enabled || !Enabled) { Menu.Close(); return; }
            if (!forced && Menu.AfterComplete) { Menu.AfterComplete = false; return; }
            visibleItems.Clear();
            FocussedItemIndex = 0;
            VerticalScrollBar.Value = 0;
            //some magic for update scrolls
            //AutoScrollMinSize -= new Size(1, 0);
            //AutoScrollMinSize += new Size(1, 0);
            //get fragment around caret

            //Range fragment = tb.Selection.GetFragment(Menu.SearchPattern);
            Range fragment = tb.Selection.GetFragmentLookedLeft();
            string text = fragment.Text;

            if (!forced && regexBeginWithDigit.IsMatch(text)) return;

            Range fragmentBefore = fragment.Clone();
            int iLine = fragmentBefore.Start.iLine;
            int iChar = fragmentBefore.Start.iChar;
            fragmentBefore.Start = new Place(0    , iLine);
            fragmentBefore.End   = new Place(iChar, iLine);
            string wordBefore = Regex.Match(fragmentBefore.Text, @"((\w+)[^\(\S,]+)[^\(;:]*?$", RegexOptions.RightToLeft).Groups[2].Value;
            if (tb.Language == Language.PascalScript && regexIsPascalFunctionName.IsMatch(wordBefore)) {
                Menu.TempNotShow = true;
                return;
            } else if (tb.Language == Language.CPPScript && isType(wordBefore)) {
                Menu.TempNotShow = true;
                return;
            }
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
                    if (tb.Selection.GetVariableForEqual(Menu.SearchPattern, out text)) {
                        doNotGetFromSourceItems = true;
                        forced = true;
                        showTypes = true;
                    } else {
                        return;
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
                    int indexDot = text.IndexOf(".", StringComparison.Ordinal);
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
                    bool notExacctlyfoundSelected     = false;
                    int  notExacctlyFocussedItemIndex = 0;

                    foreach (var item in sourceItems) {
                        item.Parent = Menu;
                        CompareResult resultCompare = item.Compare(text);
                        if (resultCompare == CompareResult.VisibleAndSelected) {
                            visibleItems.Add(item);

                            if (!foundSelected && (lastword.Length > 0) && (item.MenuText == lastword)) {
                                foundSelected = true;
                                FocussedItemIndex = visibleItems.Count - 1;
                            }

                        } else if (item.NotExactlyCompare(text) == CompareResult.Visible) {
                            notExacctly.Add(item);

                            if (!notExacctlyfoundSelected && !foundSelected && visibleItems.Count == 0) {
                                if ((lastword.Length > 0) && (item.MenuText == lastword)) {
                                    notExacctlyfoundSelected = true;
                                    notExacctlyFocussedItemIndex = notExacctly.Count - 1;
                                }
                            }

                        } else continue;
                        if (visibleItems.Count + notExacctly.Count > 150) break;
                    }

                    if (!foundSelected && notExacctlyfoundSelected) FocussedItemIndex = visibleItems.Count + notExacctlyFocussedItemIndex;
                    visibleItems.AddRange(notExacctly);
                }
                if (visibleItems.Count > 0 && FocussedItemIndex < 0)
                    FocussedItemIndex = 0;
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
                // < By WendyH -------------------------------
                // Recalc position
                Menu.InitDefaultSize();
                int h = MaximumSize.Height;
                int ih = ItemHeight;
                if (visibleItems.Count < HMSEditor.MaxPopupItems) {
                    h = visibleItems.Count * ih + 4;
                }
                Point ps = tb.PointToScreen(point);
                if (ps.Y + h > Screen.PrimaryScreen.WorkingArea.Size.Height) {
                    point.Y -= (h + tb.CharHeight + 4);
                }
                Size = new Size(Size.Width, h);
                Menu.CalcSize();
                if (Menu.Visible) {
                    Menu.Top = tb.PointToScreen(point).Y;
                }
                // > By WendyH -------------------------------
                if (!Menu.Visible)
                {
                    CancelEventArgs args = new CancelEventArgs();
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

        private static Keys[] keysBreaksTempNotShow = new Keys[] { Keys.Space, Keys.Tab, Keys.Enter, Keys.End, Keys.Home, Keys.Oemcomma, Keys.OemCloseBrackets, Keys.OemOpenBrackets, Keys.OemSemicolon, Keys.OemPeriod, Keys.Decimal };
        void tb_KeyDown(object sender, KeyEventArgs e)
        {
            var box = sender as FastColoredTextBox;
            if (Menu.Visible)
                if (ProcessKey(e.KeyCode, e.Modifiers))
                    e.Handled = true;
            if (box == null) return;

            if (Menu.TempNotShow) {
                char ch = box.Selection.CharBeforeStart;
                if (box.Selection.Start.iChar == 0) {
                    Menu.TempNotShow = false;
                } else if (ch == ' ' || ch == '\n' || ch == '(') {
                    Menu.TempNotShow = false;
                } else if (Array.IndexOf(keysBreaksTempNotShow, e.KeyCode)>=0) {
                    Menu.TempNotShow = false;
                }
            }

            if (!Menu.Visible)
            {
                if (box.HotkeysMapping.ContainsKey(e.KeyData) && box.HotkeysMapping[e.KeyData] == FCTBAction.AutocompleteMenu)
                {
                    DoAutocomplete(true);
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

        protected override void OnMouseWheel(MouseEventArgs e) {
            int newVal = VerticalScrollBar.Value - e.Delta;
            newVal = Math.Max(VerticalScrollBar.Minimum, newVal);
            newVal = Math.Min(VerticalScrollBar.Maximum, newVal);
            VerticalScrollBar.Value = newVal;
            Invalidate();
        }

        private void VerticalScrollBar_Scroll(object sender, EventArgs e) {
            Invalidate();
        }

        void AdjustScroll()
        {
            if (oldItemCount == visibleItems.Count)
                return;

            int needHeight = ItemHeight * visibleItems.Count + 1;
            Height = Math.Min(needHeight, MaximumSize.Height);
            Menu.CalcSize();
            if (!VerticalScrollBar.Visible) VerticalScrollBar.Height = Height;
            VerticalScrollBar.Maximum = needHeight - Height;
            //AutoScrollMinSize = new Size(0, needHeight);
            oldItemCount = visibleItems.Count;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            AdjustScroll();
            Graphics g = e.Graphics;

            var itemHeight = ItemHeight;
            int startI     = VerticalScrollBar.Value / itemHeight - 1;
            int finishI    = (VerticalScrollBar.Value + ClientSize.Height) / itemHeight + 1;
            startI  = Math.Max(startI , 0);
            finishI = Math.Min(finishI, visibleItems.Count);
            int leftPadding = 18;
            for (int i = startI; i < finishI; i++)
            {
                var y = i * itemHeight - VerticalScrollBar.Value;

                var item = visibleItems[i];
                // draw item background
                if (item.BackColor != Color.Transparent) {
                    using (var brush = new SolidBrush(item.BackColor)) {
                        g.FillRectangle(brush, 1, y, ClientSize.Width - 1 - 1, itemHeight - 1);
                    }
                }
                // draw item image
                if (ImageList != null && item.ImageIndex >= 0 && item.ImageIndex < ImageList.Images.Count) {
                    g.DrawImage(ImageList.Images[item.ImageIndex], 1, y);
                }
                // draw selected item
                if (i == FocussedItemIndex) {
                    using (var selectedBrush = new SolidBrush(SelectedColor)) {
                        g.FillRectangle(selectedBrush, leftPadding, y, ClientSize.Width, itemHeight);
                    }
                }
                // draw item text
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                Color foreColor = item.ForeColor != Color.Transparent ? item.ForeColor : (i == FocussedItemIndex) ? Color.White : ForeColor;
                TextRenderer.DrawText(g, item.ToString(), Font, new Point(leftPadding + 1, y), foreColor);
                //using (var brush = new SolidBrush(foreColor)) {
                     //g.DrawString(item.ToString(), Font, brush, leftPadding+2, y, StringFormat.GenericTypographic);
                //}
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

            if (e.Button == MouseButtons.Left)
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

        internal void OnSelecting()
        {
            if (FocussedItemIndex < 0 || FocussedItemIndex >= visibleItems.Count)
                return;
            tb.TextSource.Manager.BeginAutoUndoCommands();
            try
            {
                AutocompleteItem item = FocussedItem;
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
            var box = fragment.tb;
            // < By WendyH ---------------------------
            if (box.ToolTip4Function.Visible) Menu.AfterComplete = true;
            if (fragment.CharBeforeStart == '=') newText = " " + newText;
            int iLine = fragment.Start.iLine;
            HMSItem hmsItem = item as HMSItem;
            if (hmsItem != null) {
                if (((hmsItem.Kind == DefKind.Property) || (hmsItem.Kind == DefKind.Variable)) && !HMS.TypeIsClass(hmsItem.Type)) {
                    Range fwords = fragment.GetFragmentLookedLeft();
                    var f = new Range(box, new Place(0, iLine), new Place(fwords.Start.iChar, iLine));
                    string line = f.Text.Trim();
                    if ((line.Length == 0) && (box.Lines[iLine].IndexOf('=') < 0)) newText += (box.Language == Language.PascalScript) ? " := " : " = ";
                }
            }

            // > By WendyH ---------------------------

            box.BeginAutoUndo(); 
            box.TextSource.Manager.ExecuteCommand(new SelectCommand(box.TextSource));
            if (box.Selection.ColumnSelectionMode)
            {
                var start = box.Selection.Start; 
                var end   = box.Selection.End;
                start.iChar = fragment.Start.iChar;
                end  .iChar = fragment.End  .iChar;
                box.Selection.Start = start;
                box.Selection.End   = end;
            }
            else
            {
                box.Selection.Start = fragment.Start;
                box.Selection.End = fragment.End;
            }
            box.InsertText(newText);
            box.TextSource.Manager.ExecuteCommand(new SelectCommand(box.TextSource));
            box.EndAutoUndo();
            if ((hmsItem != null) && (hmsItem.Params.Count > 0)) {
                if (HMSEditor.ActiveEditor != null) HMSEditor.ActiveEditor.WasCommaOrBracket = true;
            }
            box.Focus();
        }

        int PointToItemIndex(Point p)
        {
            return (p.Y + VerticalScrollBar.Value) / ItemHeight;
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
                    ToolTip.Hide(this);
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
                var y = FocussedItemIndex * ItemHeight - VerticalScrollBar.Value;
                if (y < 0)
                    VerticalScrollBar.Value = FocussedItemIndex * ItemHeight;
                if (y > ClientSize.Height - ItemHeight)
                    VerticalScrollBar.Value = Math.Min(VerticalScrollBar.Maximum, FocussedItemIndex * ItemHeight - ClientSize.Height + ItemHeight);

            }
            //some magic for update scrolls
            //AutoScrollMinSize -= new Size(1, 0);
            //AutoScrollMinSize += new Size(1, 0);
        }

        private void SetToolTip(HMSItem autocompleteItem)
        {
            if (IsDisposed) return;
            if (ToolTip.Visible && ToolTip.HmsItem == autocompleteItem) return;
            IWin32Window window = Parent;
            ToolTip.Hide(window);
            // By WendyH
            if (string.IsNullOrEmpty(autocompleteItem.ToolTipTitle)) {
                ToolTip.Hide(window);
                return;        
            }
            ToolTip.ParentRect   = new Rectangle(Location.X, Location.Y, Width, Height);
            Point location = new Point((window == this ? Width : Right) + 3, 0);
            int y = FocussedItemIndex * ItemHeight - VerticalScrollBar.Value; // By WendyH
            if (y < 0) y = 0;
            if (y > ClientSize.Height - ItemHeight) y = ClientSize.Height - ItemHeight;
            location.Y = y;
            ToolTip.Show(autocompleteItem, window, location, ToolTipDuration);
        }

        public int Count => visibleItems.Count;
        private object obj4LockItems = new object();

        public void SetAutocompleteItems(ICollection<string> items)
        {
            lock (obj4LockItems) {
                AutocompleteItems list = new AutocompleteItems();
                foreach (var item in items)
                    list.Add(new HMSItem(item));
                SetAutocompleteItems(list);
            }
        }

        public void SetAutocompleteItems(AutocompleteItems items)
        {
            lock (obj4LockItems)
                sourceItems = items;
        }

        public void AddFilteredItems(AutocompleteItems items) {
            lock (obj4LockItems) {
                AutocompleteItems list = new AutocompleteItems();
                string filter = Menu.Filter;
                foreach (var item in items) {
                    if ((filter.Length > 0) && (item.Filter.Length>0) && (filter.IndexOf(item.Filter, StringComparison.Ordinal)<0)) continue;
                    if (list.ContainsName(item.MenuText)) continue;
                    list.Add(item);
                }
                AddAutocompleteItems(list);
            }
        }

        public void AddAutocompleteItems(AutocompleteItems items) {
            lock (obj4LockItems)
                sourceItems.AddRange(items);
        }

        public void SetVisibleVariablesItems(AutocompleteItems items) {
            lock (obj4LockItems)
                VisibleVariables = items;
        }

        public void SetLocalssVariablesItems(AutocompleteItems items) {
            lock (obj4LockItems)
                VisibleLocalVars = items;
        }

        public void SetVisibleFunctionsItems(AutocompleteItems items) {
            lock (obj4LockItems)
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
