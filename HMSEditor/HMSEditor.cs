/* This code is released under WTFPL Version 2 (http://www.wtfpl.net/) * Created by WendyH. Copyleft. */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using System.Diagnostics;
using HmsAddons;

namespace HMSEditorNS {
    public sealed partial class HMSEditor: UserControl {
        #region Static
        public  static string    Title           = "HMS Editor addon v" + AboutDialog.AssemblyVersion;
        public  static string    Description     = "Альтернативный редактор скриптов c поддержкой IntelliSence";
        public  static string    SettingsSection = "AddonMain";
        public  static HMSEditor ActiveEditor    = null;
        public  static bool      NeedRestart     = false;
        public  static string    NeedCopyNewFile = "";
        public  static string    NeedCopyDllFile = "";
        public  static INI       Settings        = new INI(HMS.SettingsFile);
        private static bool      EnableMouseHelp = false;
        private static object    LockObject      = new object();

        #region Regular Expressions Magnetic Field
        private static Regex regexProceduresCPP    = new Regex(@"(?:^|[\r\n])\s*?(?<type>\w+)\s+(\w+)\s*?\("  , RegexOptions.Singleline | RegexOptions.Compiled);
        private static Regex regexProceduresPascal = new Regex(@"\b(?:procedure|function)\s+(\w+)"            , RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex regexProceduresBasic  = new Regex(@"(?:^|[\r\n])\s*?sub\s+(\w+)"                 , RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex regexProceduresYAML   = new Regex(@"(?:^|[\r\n])\s*?(\w+)\s*?:"                  , RegexOptions.Singleline | RegexOptions.Compiled);
        public  static Regex regexExcludeWords     = new Regex(@"\b(for|if|else|return|true|false|while|do)\b", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex regexDetectProcedure  = new Regex(@"\b(void|procedure)" , RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
        private static Regex regexPartOfLine       = new Regex(@"\b(.*?\s.*?\s.*?)\b", RegexOptions.Compiled);

        private static Regex regexSearchConstantsCPP      = new Regex(@"#define\s+(\w+)(.*)"                             , RegexOptions.Compiled);
        private static Regex regexSearchConstantsPascal1  = new Regex(@"\bconst\b(.*?)\b(var|procedure|function|begin)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static Regex regexSearchConstantsPascal2  = new Regex(@"([\w]+)\s*?=\s*?(.*?)[;\r\n]"                    , RegexOptions.Compiled);

        private static Regex regexSearchVarsCPP           = new Regex(@"(?<type>\w+)\s+(?<vars>[^;{}]+)"   , RegexOptions.Compiled);
        private static Regex regexSearchVarsJS            = new Regex(@"(?<type>\w+)\s+(?<vars>[^;{}]+)"   , RegexOptions.Compiled);
        private static Regex regexSearchVarsPascal        = new Regex(@"(?<vars>[\w ,]+):(?<type>[^=;\)]+)", RegexOptions.Compiled);

        private static Regex regexTwoWords                = new Regex(@"(\w+)\s+(\w+)\s*$"    , RegexOptions.Compiled);
        private static Regex regexAssignment              = new Regex(@"=[^,$]+"              , RegexOptions.Compiled);
        private static Regex regexConstantKeys            = new Regex(@"\b(var|const)\b"      , RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex regexNotValidCharsInVars     = new Regex(@"[{}\(\[\]]"           , RegexOptions.Compiled);
        private static Regex regexExractConstantValue     = new Regex(@"(""[^""]+""|'[^']+')" , RegexOptions.Compiled);
        private static Regex regexBrackets                = new Regex(@"(\(|\)|\{|\})"        , RegexOptions.Compiled);

        private static Regex regexSplitFuncParam          = new Regex("[,;]", RegexOptions.Compiled);
        private static Regex regexFoundOurFunction        = new Regex(@"([\w\.]+)\s*?[\(\[](.*)$", RegexOptions.Compiled | RegexOptions.Singleline);

        private static Regex regexIsNum      = new Regex(@"(\d|\$)", RegexOptions.Compiled);
        private static Regex regexIsStr      = new Regex(@"(""|')" , RegexOptions.Compiled);
        private static Regex regexAllSymbols = new Regex("."       , RegexOptions.Compiled);
        private static Regex regexLineBreaks = new Regex(@"[\r\n]" , RegexOptions.Compiled);
        private static Regex regexNotNewLine = new Regex(@"[^\n]"  , RegexOptions.Compiled);

        private static RegexOptions StdOpt = RegexOptions.Singleline | RegexOptions.IgnoreCase; // Стандартные флаги RegexOptions
        #endregion Regular Expressions magnetic filed

        private static string ReturnSpaces(Match m) { return regexAllSymbols.Replace(m.Value, " "); }
        private static MatchEvaluator evaluatorSpaces = new MatchEvaluator(ReturnSpaces);

        private static char CensChar = ' '; // Символ замены строк и комментариев при обработке текста на поиск переменных и проч.

        private string RemoveLinebeaks(string text) { return regexLineBreaks.Replace(text, ""); }

        public static void MouseTimer_Task(object StateObj) {
            if (ActiveEditor != null) {
                //if (ActiveEditor.PopupMenu.Visible || ActiveEditor.Editor.ToolTip4Function.Visible) return;
                if (EnableMouseHelp || (ActiveEditor.EnableEvaluateByMouse && ActiveEditor.DebugMode)) {
                    MouseHelpTimer.Task(ActiveEditor);
                }
            }
        }
        #endregion Static
        // Constructor
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public HMSEditor(IntPtr aScriptFrame, int aScriptMode) {
            ActiveEditor   = this;                // static field - current editor for static tasks
            PtrScriptFrame = aScriptFrame;
            if (PtrScriptFrame != IntPtr.Zero) {
                HmsScriptFrame = (IHmsScriptFrame)System.Runtime.Remoting.Services.EnterpriseServicesHelper.WrapIUnknownWithComObject(PtrScriptFrame);
            }
            HmsScriptMode  = (HmsScriptMode)aScriptMode;
            InitializeComponent();
            labelVersion.Text = HMSEditor.Title;
            splitContainer1.Panel2Collapsed = true;
            Editor.CurrentLineColor = Color.FromArgb(100, 210, 210, 255);
            Editor.ChangedLineColor = Color.FromArgb(255, 152, 251, 152);
            Editor.LostFocus += Editor_LostFocus; // for hiding all tooltipds when lost focus
            helpPanel1.PanelClose += HelpPanel1_PanelClose;
            helpPanel1.Init(imageList1, HmsScriptMode.ToString());
            WorkerCheckSyntax.DoWork += WorkerCheckSyntax_DoWork;
            WorkerCheckSyntax.RunWorkerCompleted += WorkerCheckSyntax_RunWorkerCompleted;
            CreateAutocompleteItemsByScriptDescrition();
            SetAutoCompleteMenu();
        }

        private void HelpPanel1_PanelClose(object sender, EventArgs e) {
            splitContainer1.Panel2Collapsed = true;
            btnSprav.Checked = false;
        }

        private void WorkerCheckSyntax_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            if (needCheckSyntaxAgain) {
                needCheckSyntaxAgain = false;
                WorkerCheckSyntax.RunWorkerAsync();
            }
            if (ErrorMessage.Length > 0) {
                Editor.SetErrorLines(ErrorChar, ErrorLine, ErrorMessage);
            } else {
                Editor.ClearErrorLines();
            }
            Editor.Focus();
        }

        private void WorkerCheckSyntax_DoWork(object sender, DoWorkEventArgs e) {
            ErrorLine = 0;
            ErrorChar = 0;
            ErrorMessage = "";
            if (PtrScriptFrame != IntPtr.Zero) {
                try {
                    object objScriptName = ScriptLanguage;
                    object objScriptText = Editor.Text;
                    object objErrorMessage = "";
                    int nErrorLine = 0;
                    int nErrorChar = 0;
                    int nResult    = -1;
                    IHmsScriptFrame scriptFrame1 = (IHmsScriptFrame)System.Runtime.Remoting.Services.EnterpriseServicesHelper.WrapIUnknownWithComObject(PtrScriptFrame);
                    scriptFrame1.CompileScript(ref objScriptName, ref objScriptText, ref objErrorMessage, ref nErrorLine, ref nErrorChar, ref nResult);
                    if (nResult == 0) {
                        ErrorChar = Math.Max(0, nErrorChar - 1);
                        ErrorLine = Math.Max(0, nErrorLine - 1);
                        ErrorMessage = objErrorMessage.ToString();
                    }
                } catch { ; }
            } else {
                Editor.SetErrorLines(8, 50, "Проверка!");
            }
        }

        bool needCheckSyntaxAgain = false;
        public void AutoCheckSyntaxBackground() {
            if (WorkerCheckSyntax.IsBusy) {
                needCheckSyntaxAgain = true;
                return;
            }
            WorkerCheckSyntax.RunWorkerAsync();
		} 

		// Fields
		public  bool   Locked             = false;
        public  string Filename           = HMS.TemplatesDir; // last opened or saved file 
        public  int    LastPtocedureIndex = -1;
        public  bool   WasCommaOrBracket  = false;
        public  string CurrentValidTypes    = ""; // Sets in CreateAutocomplete() procedure
        public  string CurrentValidTypesReg = ""; // Sets in CreateAutocomplete() procedure
        public  bool   IsFirstActivate    = true;
        private bool   NeedRecalcVars     = false;
        private string ThemeName          = "";
        private uint   OldTextHash        = 0;

        private IntPtr PtrScriptFrame = IntPtr.Zero;
        public IHmsScriptFrame HmsScriptFrame = null;
        private HmsScriptMode  HmsScriptMode  = HmsScriptMode.smUnknown;
        private BackgroundWorker WorkerCheckSyntax = new BackgroundWorker();

        public bool Modified       { get { return Editor.IsChanged     ; } set { Editor.IsChanged      = value; } }
        public int  SelectionStart { get { return Editor.SelectionStart; } set { Editor.SelectionStart = value; } }
        public ImageList  IconList { get { return imageList1; } }
        public string SelectedText { get { return Editor.Selection.Text; } set { Editor.InsertText(value); } }

        public bool NavigateBackwardEnable { get { return btnNavigateBack   .Enabled; } set { btnNavigateBack   .Enabled = value; } }
        public bool NavigateForwardEnable  { get { return btnNavigateForward.Enabled; } set { btnNavigateForward.Enabled = value; } }

        //public ValueHintControl ValueHint = new ValueHintControl();
        public ValueToolTip ValueHint = new ValueToolTip();
        public FormValue    ValueForm = new FormValue();

        public AutocompleteItems LocalVars = new AutocompleteItems();
        public AutocompleteItems Variables = new AutocompleteItems();
        public AutocompleteItems Functions = new AutocompleteItems();

        private System.Threading.Timer MouseTimer = new System.Threading.Timer(MouseTimer_Task, null, Timeout.Infinite, Timeout.Infinite);
        public  Point       MouseLocation         = new Point();
        public  Style       InvisibleCharsStyle   = new InvisibleCharsRenderer(Pens.Gray);
        public  MarkerStyle SameWordsStyle        = new MarkerStyle(new SolidBrush(Color.FromArgb(33, Color.Gray)));
        private DateTime    LastNavigatedDateTime = DateTime.Now;

        private MatchEvaluator evaluatorSameLines = new MatchEvaluator(MatchReturnEmptyLines);
        public  AutocompleteMenu PopupMenu;
        public static int MaxPopupItems = 18;

        private bool TextBoxFindChanged = false;
        private int    ErrorLine = 0;
        private int    ErrorChar = 0;
        private string ErrorMessage = "";

        public string Filter { get { return PopupMenu.Filter; } set { PopupMenu.Filter = value; } } // Фильтр для видимости переменных по заголовку окна
        public int  tsHeight { get { return tsMain.Height; } }

        public bool AutoCompleteBrackets     { get { return Editor.AutoCompleteBrackets    ; } set { Editor.AutoCompleteBrackets    = value; } }
        public bool AutoIdent                { get { return Editor.AutoIndent              ; } set { Editor.AutoIndent              = value; } }
        public bool AutoIndentChars          { get { return Editor.AutoIndentChars         ; } set { Editor.AutoIndentChars         = value; } }
        public bool AutoIndentExistingLines  { get { return Editor.AutoIndentExistingLines ; } set { Editor.AutoIndentExistingLines = value; } }
        public bool ToolStripVisible         { get { return tsMain.Visible                 ; } set { tsMain.Visible                 = value; } }
        public bool DebugMode                { get { return Editor.DebugMode               ; } set { Editor.DebugMode               = value; } }
        public bool EnableFunctionToolTip = true;
        public bool EnableEvaluateByMouse = true;

        public string ScriptLanguage {
            get {
                if (Editor.Language == Language.CPPScript) return "C++Script";
                if (Editor.Language == Language.YAML     ) return "Нет скрипта";
                return Editor.Language.ToString();
            }
            set {
                if (ScriptLanguage == value) return;
                Editor.ClearStylesBuffer();
                Editor.Range.ClearStyle(StyleIndex.All);
                switch (value) {
                    case "C++Script"   : Editor.Language = Language.CPPScript   ; break;
                    case "PascalScript": Editor.Language = Language.PascalScript; break;
                    case "BasicScript" : Editor.Language = Language.BasicScript ; break;
                    case "JScript"     : Editor.Language = Language.JScript     ; break;
                    default            : Editor.Language = Language.YAML        ; break;
                }
                CreateAutocomplete();
                CreateInsertTemplateItems();
                Editor.OnSyntaxHighlight(new TextChangedEventArgs(Editor.Range));
            }
        }

        public override string Text { get { return Editor.Text; } set { Editor.Text = value; } }

        protected override void OnEnter(EventArgs e) {
            ActiveEditor = this;
            base.OnEnter(e);
        }

        const int WM_GETTEXTLENGTH = 0x000E;
        const int WM_GETTEXT       = 0x000D;
        const int WM_GETDLGCODE    = 0x0087;
        const int DLGC_WANTALLKEYS = 0x0004;
        const int DLGC_WANTTAB     = 0x0002;
        const int DLGC_WANTARROWS  = 0x0001;
        const int DLGC_HASSETSEL   = 0x0008;
        protected override void WndProc(ref Message m) {
            base.WndProc(ref m);
            if (m.Msg == WM_GETTEXTLENGTH) {
                m.Result = new IntPtr(Text.Length);
                return;
            }
            if (m.Msg == WM_GETTEXT) {
                int size = Marshal.SizeOf(typeof(char)) * Text.Length;
                int bytesToCopy = Math.Min(m.WParam.ToInt32(), size);
                Marshal.Copy(System.Text.Encoding.UTF8.GetBytes(Text), 0, m.LParam, bytesToCopy);
                m.Result = new IntPtr(bytesToCopy);
                return;
            }
            if (m.Msg == WM_GETDLGCODE) {
                int result = m.Result.ToInt32();
                result = result | DLGC_WANTALLKEYS | DLGC_WANTTAB | DLGC_WANTARROWS | DLGC_HASSETSEL;
                m.Result = (IntPtr)result;
            } else {
            }
        }

        public int TextLength { get { return Text.Length; } }

        #region Fuctions and procedures
        public bool ToLock() {
            int countout = 20; // Maximum - two sec
            while (Locked && (countout > 0)) { Thread.Sleep(100); countout--; } // Waiting if locked
            if (Locked) return false;
            Locked = true;
            return Locked;
        }

        public void OnRunningStateChange(bool running) {
            DebugMode = running;
            RunLineRised = false;
        }

        private void Editor_LostFocus(object sender, EventArgs e) {
            if (!PopupMenu.Focused)
                HideAllToolTipsAndHints();
            //if (ValueHint.IsShowing ) Editor.Focus();
        }

        private void HideAllToolTipsAndHints() {
            HideToolTip4Function(true);
            if (Editor.ToolTip != null) Editor.ToolTip.RemoveAll();
            PopupMenu.ToolTip.RemoveAll();
            PopupMenu.Close();
            ValueForm.Hide();
        }

        private void HideToolTip4Function(bool noCheckLine = false) {
            if (IsDisposed) return;
            if (Editor != null && !Editor.IsDisposed && Editor.ToolTip4Function.Visible) {
                if (noCheckLine || Editor.Selection.Start.iLine != Editor.ToolTip4Function.iLine) {
                    Editor.ToolTip4Function.Hide(Editor);
                }
            }
        }

        public void GetCaretPos(ref int aLine, ref int aChar) {
            aLine = Editor.Selection.Start.iLine + 1;
            aChar = Editor.Selection.Start.iChar + 1;
        }

        public void SetCaretPos(int iLine, int iChar) {
            if (iLine > 0) iLine -= 1;
            if (iChar > 0) iChar -= 1;
            Editor.Selection.Start = new Place(iChar, iLine);
            RunLineRised = false;
            if (DebugMode) CheckDebugState(); // 4 getting debug line

            int iFirstLine = Editor.YtoLineIndex() + 2;
            int iLastLine  = Editor.YtoLineIndex(Editor.VerticalScroll.Value + Editor.Height) + iFirstLine-4;
            if ((iLine < iFirstLine) || (iLine > iLastLine)) Editor.DoCaretVisible();
            Editor.Focus();
        }

        public int GetCurrentLine() {
            return Editor.Selection.Start.iLine + 1;
        }

        public void HighlightInvisibleChars(bool flag) {
            Editor.Range.ClearStyle(InvisibleCharsStyle);
            if (flag) Editor.Range.SetStyle(InvisibleCharsStyle, @".$|.\r\n|\s");
            Editor.Invalidate();
        }

        public void ShowLineNumbers(bool flag) {
            Editor.ShowLineNumbers = flag;
        }

        public void ShowFoldingLines(bool flag) {
            Editor.ShowFoldingLines = flag;
            Editor.NeedRecalc(true);
            Editor.Refresh();
        }

        public void Undo() {
            if (Editor.UndoEnabled) Editor.Undo();
        }

        public void Redo() {
            if (Editor.RedoEnabled) Editor.Redo();
        }

        public bool NavigateBackward() {
            return Editor.NavigateBackward(); ;
        }

        public bool NavigateForward() {
            return Editor.NavigateForward(); ;
        }

        public void FindKeyPressed(KeyPressEventArgs e, string text) {
            if (e.KeyChar == '\r')
                FindText(text);
            else
                TextBoxFindChanged = true;
        }

        private void tbFind_Leave(object sender, EventArgs e) {
            Editor.YellowSelection = false;
        }

        public void FindText(string text, bool forward = true) {
            Editor.YellowSelection    = true;
            Editor.SelectionAfterFind = true;
            TextBoxFindChanged = false;
            Range r = TextBoxFindChanged ? Editor.Range.Clone() : Editor.Selection.Clone();
            if (forward) {
                r.End   = new Place(Editor[Editor.LinesCount - 1].Count, Editor.LinesCount - 1);
            } else {
                r.Start = new Place(0, 0);
                r.End   = new Place(Editor.Selection.End.iChar, Editor.Selection.End.iLine);
            }
            var   pattern = Regex.Escape(text);
            bool  founded = false;
            Range foundRange = null;
            foreach (var found in r.GetRanges(pattern, RegexOptions.IgnoreCase)) {
                founded    = true;
                foundRange = found;
                if (forward) break;
            }
            if (founded) {
                foundRange.Inverse();
                Editor.Selection = foundRange;
                Editor.DoSelectionVisible();
            } else {
                MessageBox.Show("\"" + text + "\"" + " не найдено.", Title, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                Editor.SelectionStart = 0;
            }

        }

        public void OffBreakpointInHms(int iLine = -1) {
            if (HmsScriptFrame != null) {
                if (HmsScriptFrame != null) {
                    int isBreakpoint = 0;
                    HmsScriptFrame.IsBreakpointLine(iLine + 1, ref isBreakpoint);
                    bool lineIsBreakpointed = (isBreakpoint < 0);
                    if (lineIsBreakpointed)
                        HmsScriptFrame.ToggleBreakpoint(iLine + 1);
                }
            }
        }

        public void SetBreakpointInHms(int iLine = -1) {
            if (HmsScriptFrame != null) {
                if (HmsScriptFrame != null) {
                    int isBreakpoint = 0;
                    HmsScriptFrame.IsBreakpointLine(iLine + 1, ref isBreakpoint);
                    bool lineIsBreakpointed = (isBreakpoint < 0);
                    if (!lineIsBreakpointed)
                        HmsScriptFrame.ToggleBreakpoint(iLine + 1);
                }
            }
        }

        public void ToggleBreakpoint(int iLine = -1) {
            if (iLine == -1) iLine = Editor.Selection.Start.iLine;
            int  isBreakpoint = 0;

            string line = Editor.WithoutStringAndComments(Editor.Lines[iLine]).Trim();
            if (line.Length == 0) return;

            bool lineIsBreakpointed = Editor.Breakpoints.Contains(iLine);
            if (HmsScriptFrame != null) {
                HmsScriptFrame.IsBreakpointLine(iLine + 1, ref isBreakpoint);
                lineIsBreakpointed = (isBreakpoint < 0);
            }
            if (lineIsBreakpointed) {
                Editor.UnbreakpointLine(iLine);
            } else {
                string name = regexPartOfLine.Match(Editor.Lines[iLine]).Value;
                Editor.BreakpointLine(iLine, "Точка останова " + (Editor.Breakpoints.counter + 1) + " " + name + "...");
            }
            if (HmsScriptFrame != null)
                HmsScriptFrame.ToggleBreakpoint(iLine+1);
        }

        public void Bookmark(int iLine = -1) {
            if (iLine == -1) iLine = Editor.Selection.Start.iLine;
            if (Editor.Bookmarks.Contains(iLine)) {
                Editor.UnbookmarkLine(iLine);
            } else {
                string name = regexPartOfLine.Match(Editor.Lines[iLine]).Value;
                Editor.BookmarkLine(iLine, "Закладка " + (Editor.Bookmarks.counter+1) + " " + name + "...");
            }
        }

        public void BookmarkClear() {
            Editor.Bookmarks.Clear();
            Editor.Invalidate();
        }

        public void BookmarkPrevious() {
            Editor.GotoPrevBookmark(Editor.Selection.Start.iLine);
        }

        public void BookmarkNext() {
            Editor.GotoNextBookmark(Editor.Selection.Start.iLine);
        }

        public void HotKeysDialog() {
            var form = new HotkeysEditorForm(Editor.HotkeysMapping);
            if (form.ShowDialog() == DialogResult.OK)
                Editor.HotkeysMapping = form.GetHotkeys();
        }

        private string FileDialogFilter() {
            return "All files (*.*)|*.*|" +
                   "PascalScript files (*.pas)|*.pas|" +
                   "C++Script files (*.cpp)|*.cpp|" +
                   "JavaScript files (*.js)|*.js|" +
                   "BasicScript files (*.bas, *.vb)|*.bas;*.vb|" + 
                   "Yaml files (*.yml)|*.yml|" +
                   "Text files (*.txt)|*.txt";
        }

        private int FileDialogIndexFilter() {
            switch (Editor.Language) {
                case Language.PascalScript: return 2;
                case Language.CPPScript   : return 3;
                case Language.JScript     : return 4;
                case Language.BasicScript : return 5;
                case Language.YAML        : return 6;
            }
            return 1;
        }

        public void OpenFile() {
            OpenFileDialog fileFialog = new OpenFileDialog();
            if (Filename.Length > 0) {
                fileFialog.InitialDirectory = Path.GetDirectoryName(Filename);
                fileFialog.FileName         = Path.GetFileName     (Filename);
            }
            fileFialog.Filter           = FileDialogFilter();
            fileFialog.FilterIndex      = FileDialogIndexFilter();
            fileFialog.RestoreDirectory = true;
            fileFialog.Title            = "Выбор файла скрипта";
            if (fileFialog.ShowDialog() == DialogResult.OK) {
                Filename = fileFialog.FileName;
                if (File.Exists(Filename)) Editor.OpenFile(Filename);
            }

        }

        public void SaveFile() {
            SaveFileDialog fileFialog = new SaveFileDialog();
            if (Filename.Length > 0) {
                fileFialog.InitialDirectory = Path.GetDirectoryName(Filename);
                fileFialog.FileName         = Path.GetFileNameWithoutExtension(Filename);
            }
            fileFialog.Filter           = FileDialogFilter();
            fileFialog.FilterIndex      = FileDialogIndexFilter();
            fileFialog.RestoreDirectory = true;
            fileFialog.Title            = "Выбор файла скрипта";
            if (fileFialog.ShowDialog() == DialogResult.OK) {
                Filename = fileFialog.FileName;
                try {
                    if (File.Exists(Filename)) File.Delete(Filename);
                    File.WriteAllText(Filename, Editor.Text, Encoding.UTF8);
                    Modified = false;
                } catch (Exception e) {
                    HMS.LogError(e.ToString());
                }
            }

        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public void Print() {
            var settings = new PrintDialogSettings();
            settings.Title  = Filename;
            settings.Header = "&b&w&b";
            settings.Footer = "&b&p";
            settings.ShowPrintPreviewDialog = true;
            Editor.Print(settings);
        }

        public void ClearUndo() {
            Editor.ClearUndo();
        }

        public void RestorePosition() {
            Match m;
            if (btnStorePositions.Checked) {
                try {
                    string hash   = ((uint)Text.GetHashCode()).ToString();
                    string hashes = Settings.Get("LastHash" + HmsScriptMode.ToString(), SettingsSection, "");
                    hashes = Regex.Match(hashes, hash + ":[^\\|]+").Value;
                    if (hashes == "") return;
                    m = Regex.Match(hashes, hash + ":(\\d+):(\\d+)");
                    if (m.Success) {
                        uint sel = 0, pos = 0;
                        uint.TryParse(m.Groups[1].Value, out sel);
                        if (sel < Editor.Text.Length) Editor.SelectionStart = (int)sel;
                        uint.TryParse(m.Groups[2].Value, out pos);
                        if (pos <= Editor.GetMaximumScrollValue()) Editor.SetVerticalScrollValue((int)pos);
                    }
                    m = Regex.Match(hashes, hash + ":.*?:.*?:(.*?):(.*)");
                    if (m.Success) {
                        string[] sbp = m.Groups[1].Value.Split(',');
                        string[] sbm = m.Groups[2].Value.Split(',');
                        Editor.Breakpoints.Clear();
                        foreach (string s in sbp) {
                            m = Regex.Match(s, "\\d+");
                            if (m.Success) ToggleBreakpoint(int.Parse(m.Value));
                        }
                        Editor.Bookmarks.Clear();
                        foreach (string s in sbm) {
                            m = Regex.Match(s, "(\\d+)\\.(\\d+)\"(.*?)\"");
                            if (!m.Success) continue;
                            Bookmark b = new Bookmark(Editor, "", 0);
                            b.LineIndex = int.Parse(m.Groups[1].Value);
                            b.CharIndex = int.Parse(m.Groups[2].Value);
                            b.Name = m.Groups[3].Value;
                            Editor.Bookmarks.Add(b);
                        }
                    }
                    Editor.Invalidate();
                } catch (Exception e) {
                    HMS.LogError(e.ToString());
                    Console.WriteLine("Error restoring position", e);
                }
            }
        }

        /// <summary>
        /// Apply settings from .ini file to the this objects
        /// </summary>
        public void LoadSettings() {
            try { Settings.Load(); } catch (Exception e) { HMS.LogError(e.ToString()); Console.WriteLine("Error loading config file '" + Settings.File + "'", e); return; }
            OldTextHash = (uint)Text.GetHashCode();

            tsMain.Visible = false;
            string section = SettingsSection, sVal;
            tsMain.Visible                   = Settings.Get("ToolStripVisible"    , section, tsMain.Visible);
            btnHighlightCurrentLine .Checked = Settings.Get("HighlightCurrentLine", section, btnHighlightCurrentLine .Checked);
            btnShowLineNumbers      .Checked = Settings.Get("ShowLineNumbers"     , section, btnShowLineNumbers      .Checked);
            btnShowFoldingLines     .Checked = Settings.Get("ShowFoldingLines"    , section, btnShowFoldingLines     .Checked);
            btnShowFoldingIndicator .Checked = Settings.Get("ShowFoldingIndicator", section, btnShowFoldingIndicator .Checked);
            btnEnableFolding        .Checked = Settings.Get("EnableFoldings"      , section, btnEnableFolding        .Checked);
            btnAutoCheckSyntax      .Checked = Settings.Get("AutoCheckSyntax"     , section, btnAutoCheckSyntax      .Checked);
            btnHighlightSameWords   .Checked = Settings.Get("HighlightSameWords"  , section, btnHighlightSameWords   .Checked);
            btnSetIntelliSense      .Checked = Settings.Get("IntelliSense"        , section, btnSetIntelliSense      .Checked);
            btnHints4CtrlSpace      .Checked = Settings.Get("IntelliOnlyCtrlSpace", section, btnHints4CtrlSpace      .Checked);
            btnIntelliSenseFunctions.Checked = Settings.Get("EnableFunctionHelp"  , section, btnIntelliSenseFunctions.Checked);
            btnEvaluateByMouse      .Checked = Settings.Get("EvaluateByMouse"     , section, btnEvaluateByMouse      .Checked);
            btnAutoCompleteBrackets .Checked = Settings.Get("AutoCompleteBrackets", section, btnAutoCompleteBrackets .Checked);
            btnAutoIdent            .Checked = Settings.Get("AutoIdent"           , section, btnAutoIdent            .Checked);
            btnAutoIdentLines       .Checked = Settings.Get("AutoIdentLines"      , section, btnAutoIdentLines       .Checked);
            btnMarkChangedLines     .Checked = Settings.Get("MarkChangedLines"    , section, btnMarkChangedLines     .Checked);
            btnMouseHelp            .Checked = Settings.Get("MouseHelp"           , section, btnMouseHelp            .Checked);
            btnRedStringsHighlight  .Checked = Settings.Get("StringsHighlight"    , section, btnRedStringsHighlight  .Checked);
            btnToolStripMenuItemFONT.Checked = Settings.Get("AlternateFont"       , section, btnToolStripMenuItemFONT.Checked);
            btnVerticalLineText     .Checked = Settings.Get("VerticalLineText"    , section, btnVerticalLineText     .Checked);
            btnStorePositions       .Checked = Settings.Get("StorePositions"      , section, btnStorePositions       .Checked);
            btnSprav                .Checked = Settings.Get("ShowSprav"           , section, btnSprav                .Checked);
            btnBoldCaret            .Checked = Settings.Get("BoldCaret"           , section, btnBoldCaret            .Checked);
            btnCheckKeywordsRegister.Checked = Settings.Get("CheckKeywordsRegister",section, btnCheckKeywordsRegister.Checked);
            btnCheckNewVersionOnLoad.Checked = Settings.Get("CheckNewVersionOnLoad",section, btnCheckNewVersionOnLoad.Checked);
            btnFormatCodeWhenPaste  .Checked = Settings.Get("FormatCodeWhenPaste" , section, btnFormatCodeWhenPaste  .Checked);

            btnUnderlinePascalKeywords.Checked = Settings.Get("UnderlinePascalKeywords", section, btnUnderlinePascalKeywords.Checked);
            Editor.SyntaxHighlighter.AltPascalKeywordsHighlight = btnUnderlinePascalKeywords.Checked;
            Editor.SyntaxHighlighter.RedStringsHighlight        = btnRedStringsHighlight    .Checked;
             
            Editor.Font = new Font("Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            if ((Editor.Font.Name.ToLower().IndexOf("consolas") < 0) && (HMS.PFC.Families.Length > 1)) {
                btnToolStripMenuItemFONT.Visible = false;
                Editor.Font = new Font(HMS.PFC.Families[1], 10f, FontStyle.Regular, GraphicsUnit.Point);
            } else if (btnToolStripMenuItemFONT.Checked) { 
                btnToolStripMenuItemFONT_Click(null, new EventArgs());
            }

            sVal = Settings.Get("Zoom", section, "100"); 
            Editor.Zoom = Int32.Parse(sVal);

            PopupMenu.OnlyCtrlSpace        = btnHints4CtrlSpace      .Checked;
            PopupMenu.Enabled              = btnSetIntelliSense      .Checked;
            Editor.AutoCompleteBrackets    = btnAutoCompleteBrackets .Checked;
            Editor.AutoIndent              = btnAutoIdent            .Checked;
            Editor.AutoIndentExistingLines = btnAutoIdentLines       .Checked;
            EnableFunctionToolTip          = btnIntelliSenseFunctions.Checked;
            EnableEvaluateByMouse          = btnEvaluateByMouse      .Checked;
            Editor.EnableFoldingIndicator  = btnShowFoldingIndicator .Checked;
            Editor.HighlightCurrentLine    = btnHighlightCurrentLine .Checked;

            Themes.Init();

            ThemeName = Settings.Get("Theme"   , section, ThemeName);
            Filename  = Settings.Get("LastFile", section, Filename );

            HMS.LoadTemplates(); // Сначала загружаем шаблоны, какие есть
            FillThemes();

            ScriptLanguage = Settings.Get("Language", section, "C++Script");

            ShowLineNumbers (btnShowLineNumbers .Checked);
            ShowFoldingLines(btnShowFoldingLines.Checked);
            btnEnableFolding_Click   (null, EventArgs.Empty);
            btnMouseHelp_Click       (null, EventArgs.Empty);
            btnVerticalLineText_Click(null, EventArgs.Empty);
            btnAutoIdent_Click       (null, EventArgs.Empty);
            btnMarkChangedLines_Click(null, EventArgs.Empty);
            btnSprav_Click           (null, EventArgs.Empty);
            btnBoldCaret_Click       (null, EventArgs.Empty);
            btnCheckKeywordsRegister_Click(null, EventArgs.Empty);
            btnCheckNewVersionOnLoad_Click(null, EventArgs.Empty);
            btnFormatCodeWhenPaste_Click  (null, EventArgs.Empty);

            Editor.HotkeysMapping.InitDefault(); 
            string hotkeys = Settings.Get("Map", "AddonHotkeys", "");
            if (hotkeys.Length > 0) {
                HotkeysMapping ourMap = HotkeysMapping.Parse(hotkeys);
                foreach(var pair in ourMap)
                    Editor.HotkeysMapping[pair.Key] = pair.Value;
            }
            Editor.Refresh();
        }

        private void FillThemes() {
            btnThemes.DropDownItems.Clear();
            foreach (var name in Themes.Dict.Keys) {
                ToolStripMenuItem item = (ToolStripMenuItem)btnThemes.DropDownItems.Add(name);
                item.Tag = name;
                item.Click += (o, a) => {
                    ThemeName = (string)item.Tag;
                    Themes.SetTheme(this, ThemeName);
                    foreach (ToolStripMenuItem i in btnThemes.DropDownItems) i.Checked = i.Text == ThemeName;
                };
                if (name == ThemeName) {
                    item.Checked = true;
                    Themes.SetTheme(this, name);
                }
            }
        }

        /// <summary>
        /// Save settings in the external .conf file
        /// </summary>
        public void SaveSettings() {
            try {
                Settings.Load(); // reload settings before saving for update LastHashe values
                string section = SettingsSection;
                Settings.Set("ToolStripVisible"    , tsMain.Visible                  , section);
                Settings.Set("HighlightCurrentLine", btnHighlightCurrentLine .Checked, section);
                Settings.Set("ShowLineNumbers"     , btnShowLineNumbers      .Checked, section);
                Settings.Set("HighlightSameWords"  , btnHighlightSameWords   .Checked, section);
                Settings.Set("IntelliSense"        , btnSetIntelliSense      .Checked, section);
                Settings.Set("ShowFoldingLines"    , btnShowFoldingLines     .Checked, section);
                Settings.Set("ShowFoldingIndicator", btnShowFoldingIndicator .Checked, section);
                Settings.Set("EnableFoldings"      , btnEnableFolding        .Checked, section);
                Settings.Set("AutoCheckSyntax"     , btnAutoCheckSyntax      .Checked, section);
                Settings.Set("IntelliOnlyCtrlSpace", btnHints4CtrlSpace      .Checked, section);
                Settings.Set("EnableFunctionHelp"  , btnIntelliSenseFunctions.Checked, section);
                Settings.Set("EvaluateByMouse"     , btnEvaluateByMouse      .Checked, section);
                Settings.Set("AutoCompleteBrackets", btnAutoCompleteBrackets .Checked, section);
                Settings.Set("AutoIdent"           , btnAutoIdent            .Checked, section);
                Settings.Set("AutoIdentLines"      , btnAutoIdentLines       .Checked, section);
                Settings.Set("MarkChangedLines"    , btnMarkChangedLines     .Checked, section);
                Settings.Set("MouseHelp"           , btnMouseHelp            .Checked, section);
                Settings.Set("StringsHighlight"    , btnRedStringsHighlight  .Checked, section);
                Settings.Set("AlternateFont"       , btnToolStripMenuItemFONT.Checked, section);
                Settings.Set("VerticalLineText"    , btnVerticalLineText     .Checked, section);
                Settings.Set("StorePositions"      , btnStorePositions       .Checked, section);
                Settings.Set("ShowSprav"           , btnSprav                .Checked, section);
                Settings.Set("BoldCaret"           , btnBoldCaret            .Checked, section);
                Settings.Set("CheckKeywordsRegister",btnCheckKeywordsRegister.Checked, section);
                Settings.Set("CheckNewVersionOnLoad",btnCheckNewVersionOnLoad.Checked, section);
                Settings.Set("FormatCodeWhenPaste" , btnFormatCodeWhenPaste  .Checked, section);

                Settings.Set("Theme"               , ThemeName                       , section);
                Settings.Set("LastFile"            , Filename                        , section);
                Settings.Set("Language"            , ScriptLanguage                  , section);
                Settings.Set("Zoom"                , Editor.Zoom                     , section);

                Settings.Set("UnderlinePascalKeywords", btnUnderlinePascalKeywords.Checked, section);

                if (btnStorePositions.Checked) {
                    SaveState();
                }

                string hotkeys = GetHotKeysMapping();
                Settings.Set("Map", hotkeys, "AddonHotkeys");

                Settings.Save();

            } catch (Exception e) { 
                HMS.LogError(e.ToString());
            }
        }

        private void SaveState() {
            string oldHash   = OldTextHash.ToString();
            string hashParam = "LastHash" + HmsScriptMode.ToString();
            string hashes    = Settings.Get(hashParam, SettingsSection, "");
            string firstKey  = "";
            string lastHash  = ((uint)Text.GetHashCode()).ToString();
            string linesBPs  = "";
            string linesBMs  = "";

            Dictionary<string, string> hashesDict = new Dictionary<string, string>();

            foreach (string hashval in hashes.Split('|')) {
                Match m = Regex.Match(hashval, "(.*?):(.*)");
                if (!m.Success) continue;
                if (firstKey == "") firstKey  = m.Groups[1].Value;
                hashesDict[m.Groups[1].Value] = m.Groups[2].Value;
            }

            if (hashesDict.ContainsKey(oldHash)) hashesDict.Remove(oldHash);
            if (hashesDict.Count >= 20) hashesDict.Remove(firstKey);

            // Store our state
            foreach (var b in Editor.Breakpoints) { if (linesBPs != "") linesBPs += ","; linesBPs += b.LineIndex; }
            foreach (var b in Editor.Bookmarks  ) { if (linesBMs != "") linesBMs += ","; linesBMs += b.LineIndex + "." + b.CharIndex + "\"" + b.Name.Replace("|", "") + "\""; };

            hashesDict[lastHash] = ""+Editor.SelectionStart+":"+Editor.GetVerticalScrollValue()+":"+linesBPs+":"+linesBMs;

            hashes = ""; foreach (var keyVal in hashesDict) hashes += "|" + keyVal.Key + ":" + keyVal.Value;
            if (hashes.Length > 0) hashes = hashes.Substring(1);
            Settings.Set(hashParam, hashes, SettingsSection);
        }

        private string GetHotKeysMapping() {
            HotkeysMapping ourMap = new HotkeysMapping();
            HotkeysMapping defMap = new HotkeysMapping();
            ourMap.Clear();
            defMap.InitDefault();
            foreach(var m in Editor.HotkeysMapping) {
                if (defMap.ContainsKey(m.Key)) continue;
                ourMap.Add(m.Key, m.Value);
            }
            return ourMap.ToString();
        }


        public bool LoadFile(string filename) {
            bool success = false;
            if (!String.IsNullOrEmpty(filename) && File.Exists(filename)) {
                Editor.Clear();
                Editor.Text = File.ReadAllText(filename, Encoding.UTF8);
                Editor.ClearUndo();
                Modified = false;
                success = true;
            }
            return success;
        }

        private void RunScript() {
            Editor.HmsDebugLine = -1;
            if (HmsScriptFrame!=null)
                HmsScriptFrame.ProcessCommand(Constatns.ecRunScript);
        }

        bool RunLineRised = false;
        private void RunLine() {
            if (HmsScriptFrame != null) {
                if (RunLineRised) return;
                RunLineRised = true; // resets in SetCaretPos() and OnRunningStateChange()
                string line = Editor.Lines[Editor.Selection.Start.iLine];
                HmsScriptFrame.ProcessCommand(Constatns.ecRunLine);
                if (ValueForm.Visible) {
                    if (line.IndexOf(ValueForm.RealExpression) >= 0) ValueForm.Value = EvalVariableValue(ValueForm.Expression);
                } else if (ValueHint.IsShowing) {
                    if (line.IndexOf(ValueHint.RealExpression) >= 0) ValueHint.Value = EvalVariableValue(ValueHint.Expression);
                }
            }
        }

        private void EvaluateDialog() {
            if (HmsScriptFrame != null)
                HmsScriptFrame.ProcessCommand(Constatns.ecEvaluate);
        }

        private bool IsInRanges(List<Range> ranges, Place place) {
            foreach (var r in ranges) {
                int p1 = Editor.PlaceToPosition(place);
                int ps = Editor.PlaceToPosition(r.Start);
                int pe = Editor.PlaceToPosition(r.End);
                if ((p1 >= ps) && (p1 <= pe)) return true;
            }
            return false;
        }

        private string GetScriptDescriptions() {
            object objXml = "";
            if (HmsScriptFrame!=null)
                HmsScriptFrame.GenerateScriptDescriptions(ref objXml);
            return objXml.ToString();
        }

        private void RenameVariable() {
            if (Editor.LinesCount < 1) return;
            string name = CurrentWord();
            if (name == "") return;

            FormRename form = new FormRename();
            form.OldVarName = name;

            form.TextBox.BackColor = Editor.BackColor;
            form.TextBox.ForeColor = Editor.ForeColor;
            form.TextBox.Language  = Editor.Language;
            form.TextBox.ShowFoldingLines   = false;
            form.TextBox.ShowFoldingMarkers = false;

            Themes.SetTheme(form.TextBox, ThemeName);
            form.TextBox.DrawLineNumberFromInfo = true;

            form.OrigRanges.Clear();
            form.TextBox   .Clear();

            string text = Editor.WithoutStringAndComments(Text, true);

            NeedRecalcVars = true;
            UpdateCurrentVisibleVariables();
            HMSItem itemCurrentFunction = GetCurrentProcedure(Editor.SelectionStart);

            List<Range> ExcludeRanges = new List<Range>();

            // 1. Весь контекст со всеми процедурами
            // 2. Глобальный контекст + только те процедуры, где такое имя не определялось
            // 3. Только в текущей процедуре (определена в текущей процедуре)

            if (itemCurrentFunction!=null && itemCurrentFunction.Type != "MainProcedure" && LocalVars.ContainsName(name)) {
                Place start = Editor.PositionToPlace(itemCurrentFunction.PositionStart);
                Place end   = Editor.PositionToPlace(itemCurrentFunction.PositionEnd  );
                ExcludeRanges.Add(new Range(Editor, Editor.Range.Start, start));
                ExcludeRanges.Add(new Range(Editor, end, Editor.Range.End));
                form.Context = "Контекст: " + (itemCurrentFunction.Kind == DefKind.Function ? "функция" : "процедура") + " " + itemCurrentFunction.MenuText;
                form.LocalFunction = itemCurrentFunction.MenuText;
            } else {
                string firstFuncname = "", exclFuncNames="";
                foreach(var itemFunc in Functions) {
                    if (itemFunc.Type == "MainProcedure") continue;
                    UpdateCurrentVisibleVariables(itemFunc.PositionStart+itemFunc.MenuText.Length+2);
                    if (LocalVars.ContainsName(name)) {
                        Place start = Editor.PositionToPlace(itemFunc.PositionStart);
                        Place end   = Editor.PositionToPlace(itemFunc.PositionEnd  );
                        ExcludeRanges.Add(new Range(Editor, start, end));
                        if (firstFuncname == "")
                            firstFuncname = itemFunc.MenuText;
                        if (exclFuncNames != "") exclFuncNames += ",\n";
                        exclFuncNames += itemFunc.MenuText;
                    }
                }
                UpdateCurrentVisibleVariables();
                if (ExcludeRanges.Count > 0) {
                    if (ExcludeRanges.Count > 1) firstFuncname += " и др.";
                    form.Context = "Контекст: глобальный, кроме локальных переопределений в " + firstFuncname;
                    form.ExcludeFunctions = exclFuncNames;
                } else {
                    form.Context = "Контекст: весь скрипт";
                }
            }

            // Обойти все процедуры и определить, определялось ли там такое имя
            // Собрать ExcludeRanges

            Regex regexSearchVarName = new Regex(@"\b(" + name + @")\b", RegexOptions.IgnoreCase);
            int iLine = 0; string[] lines = text.Split('\n'); int iCharB, iCharE, addedLine = 0; 
            foreach (string lineString in lines) {
                MatchCollection mc = regexSearchVarName.Matches(lineString);
                if (mc.Count > 0) {
                    List<Range> rangeList = new List<Range>();
                    bool wasadded = false;
                    foreach (Match m in mc) {
                        iCharB = m.Index;
                        iCharE = iCharB + name.Length;
                        if (IsInRanges(ExcludeRanges, new Place(iCharB, iLine))) continue;
                        Range r = new Range(Editor, iCharB, iLine, iCharE, iLine);
                        r.StoredLineNo = addedLine;
                        form.OrigRanges.Add(r);
                        wasadded = true;
                    }
                    if (wasadded) {
                        if (addedLine > 0) form.TextLines += "\n";
                        form.TextLines += Editor.Lines[iLine];
                        addedLine++;
                    }
                }
                iLine++;
            }

            DialogResult result = form.ShowDialog();
            if (result == DialogResult.OK) {
                Editor.FastReplaceRanges(form.NewVarName, form.OrigRanges);
                NeedRecalcVars = true;
                Editor_TextChangedDelayed(null, null);
            } else if (result == DialogResult.Retry) {
                Editor.Navigate(form.LineNo4Goto, form.CharNo4Goto);
                Editor.Focus();
            }
        }
#endregion Function and procedures

        #region Control Events

        private void Editor_KeyDown(object sender, KeyEventArgs e) {
            if      (e.KeyCode == Keys.F11   ) tsMain.Visible = !tsMain.Visible;
            else if (e.KeyCode == Keys.F12   ) GotoDefinition();
            else if (e.KeyCode == Keys.F2    ) RenameVariable();
            else if (e.KeyCode == Keys.Escape) {
                if (Editor.findForm    != null) Editor.findForm   .Hide();
                if (Editor.replaceForm != null) Editor.replaceForm.Hide();
                HideAllToolTipsAndHints();
                PopupMenu.TempNotShow = true;
            }
            else if (e.Alt) {
                if      (e.KeyCode == Keys.D1) Editor.SetBookmarkByName(Editor.Selection.Start.iLine, "1");
                else if (e.KeyCode == Keys.D2) Editor.SetBookmarkByName(Editor.Selection.Start.iLine, "2");
                else if (e.KeyCode == Keys.D3) Editor.SetBookmarkByName(Editor.Selection.Start.iLine, "3");
                else if (e.KeyCode == Keys.D4) Editor.SetBookmarkByName(Editor.Selection.Start.iLine, "4");
                else if (e.KeyCode == Keys.D5) Editor.SetBookmarkByName(Editor.Selection.Start.iLine, "5");
                else if (e.KeyCode == Keys.D6) Editor.SetBookmarkByName(Editor.Selection.Start.iLine, "6");
                else if (e.KeyCode == Keys.D7) Editor.SetBookmarkByName(Editor.Selection.Start.iLine, "7");
                else if (e.KeyCode == Keys.D8) Editor.SetBookmarkByName(Editor.Selection.Start.iLine, "8");
                else if (e.KeyCode == Keys.D9) Editor.SetBookmarkByName(Editor.Selection.Start.iLine, "9");
                else if (e.KeyCode == Keys.Space) {

                }
            } else if (e.Control) {
                if      (e.KeyCode == Keys.D1) Editor.GotoBookmarkByName("1");
                else if (e.KeyCode == Keys.D2) Editor.GotoBookmarkByName("2");
                else if (e.KeyCode == Keys.D3) Editor.GotoBookmarkByName("3");
                else if (e.KeyCode == Keys.D4) Editor.GotoBookmarkByName("4");
                else if (e.KeyCode == Keys.D5) Editor.GotoBookmarkByName("5");
                else if (e.KeyCode == Keys.D6) Editor.GotoBookmarkByName("6");
                else if (e.KeyCode == Keys.D7) Editor.GotoBookmarkByName("7");
                else if (e.KeyCode == Keys.D8) Editor.GotoBookmarkByName("8");
                else if (e.KeyCode == Keys.D9) Editor.GotoBookmarkByName("9");
            } else if (e.KeyCode == Keys.Oemcomma || (e.Shift && e.KeyCode == Keys.D9)) {
                if (!Editor.Selection.IsStringOrComment) WasCommaOrBracket = true;
            }

            if      (e.KeyCode == Keys.F5) ToggleBreakpoint();
            else if (e.KeyCode == Keys.F7) EvaluateDialog();
            else if (e.KeyCode == Keys.F8) RunLine();
            else if (e.KeyCode == Keys.F9) RunScript();

        }

        private void Editor_SelectionChanged(object sender, EventArgs e) {
            if (Editor.SelectionAfterFind) {
                Editor.SelectionAfterFind = false;
                return;
            }
            Editor.YellowSelection = false;
            if (EnableFunctionToolTip && WasCommaOrBracket || Editor.ToolTip4Function.Visible)
                if (!CheckPositionIsInParametersSequenceWorker.IsBusy)
                    CheckPositionIsInParametersSequenceWorker.RunWorkerAsync();
        }

        private void Editor_SelectionChangedDelayed(object sender, EventArgs e) {
            if (btnHighlightSameWords.Checked) HighlightSameWords();
            if (btnSetIntelliSense   .Checked) UpdateCurrentVisibleVariables();
        }

        private void Editor_TextChangedDelayed(object sender, TextChangedEventArgs e) {
            Locked = true;       // Say to other processes we is busy - don't tuch us!
            BuildFunctionList(); // Only when text changed - build the list of functions
			if (btnAutoCheckSyntax.Checked) AutoCheckSyntaxBackground();

			Locked = false;
            if (IsFirstActivate) { 
                IsFirstActivate = false;
                Editor.Focus();
            }
        }

        private void Editor_TextChanged(object sender, TextChangedEventArgs e) {
            NeedRecalcVars = true;
        }

        private void btnAutoCheckSyntax_Click(object sender, EventArgs e) {
            if (btnAutoCheckSyntax.Checked) {
				AutoCheckSyntaxBackground();
			} else {
                Editor.ClearErrorLines();
            }
        }

        private void btnOpen_Click(object sender, EventArgs e) {
            OpenFile();
        }

        private void btnSave_Click(object sender, EventArgs e) {
            SaveFile();
        }

        private void btnPrint_Click(object sender, EventArgs e) {
            Print();
        }

        private void btnCut_Click(object sender, EventArgs e) {
            Editor.Cut();
        }

        private void btnCopy_Click(object sender, EventArgs e) {
            Editor.Copy();
        }

        private void btnPaste_Click(object sender, EventArgs e) {
            Editor.Paste();
        }

        private void btnInvisibleChars_Click(object sender, EventArgs e) {
            HighlightInvisibleChars(btnInvisibleChars.Checked);
        }

        private void btnHighlightCurrentLine_Click(object sender, EventArgs e) {
            Editor.HighlightCurrentLine = btnHighlightCurrentLine.Checked;
            Editor.Invalidate();
        }

        private void btnShowLineNumbers_Click(object sender, EventArgs e) {
            ShowLineNumbers(btnShowLineNumbers.Checked);
        }

        private void btnShowFoldingLines_Click(object sender, EventArgs e) {
            ShowFoldingLines(btnShowFoldingLines.Checked);
        }

        private void btnEnableFoldingIndicator_Click(object sender, EventArgs e) {
            Editor.EnableFoldingIndicator = btnShowFoldingIndicator.Checked;
            Editor.Invalidate();
        }

        private void btnUndo_Click(object sender, EventArgs e) {
            Undo();
        }

        private void btnRedo_Click(object sender, EventArgs e) {
            Redo();
        }

        private void btnNavigateBack_Click(object sender, EventArgs e) {
            NavigateBackward();
        }

        private void btnNavigateForward_Click(object sender, EventArgs e) {
            NavigateForward();
        }

        private void tbFind_KeyPress(object sender, KeyPressEventArgs e) {
            FindKeyPressed(e, tbFind.Text);
            tbFind.Focus();
        }

        private void btnFindPrev_Click(object sender, EventArgs e) {
            FindText(tbFind.Text, false);
        }

        private void btnFindNext_Click(object sender, EventArgs e) {
            FindText(tbFind.Text);
        }

        private void btnBookmarkPlus_Click(object sender, EventArgs e) {
            Bookmark();
        }

        private void btnBookmarkMinus_Click(object sender, EventArgs e) {
            if (Editor.Bookmarks.Count < 1) return;

            string txt = "";
            foreach (var b in Editor.Bookmarks) txt += b.Name + "\n";

            if (MessageBox.Show("Удалить все закладки?\n"+ txt, Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question)== DialogResult.Yes)
                BookmarkClear();
        }

        private void btnBookmarkPrevious_Click(object sender, EventArgs e) {
            BookmarkPrevious();
        }

        private void btnBookmarkNext_Click(object sender, EventArgs e) {
            BookmarkNext();
        }

        private void FillGoToItems(ToolStripItemCollection items) {
            items.Clear();
            foreach (var bookmark in Editor.Bookmarks) {
                ToolStripItem item = items.Add(bookmark.Name, imageList1.Images[9]);
                item.Tag = bookmark;
                item.Click += (o, a) => {
                    var b = (Bookmark)(o as ToolStripItem).Tag;
                    b.DoVisible();
                };
            }
            // --------------------------------------------------------
            foreach (HMSItem item in Functions) {
                ToolStripItem tipItem = items.Add(item.MenuText, imageList1.Images[item.ImageIndex]);
                tipItem.Tag = item.PositionStart;
                tipItem.Click += (o, a) => {
                    try {
                        Editor.SelectionStart = (int)(o as ToolStripItem).Tag;
                        Editor.DoRangeVisible(Editor.Selection, true);
                        Editor.Invalidate();
                    } catch { }
                };
            }
        }

        private void btnGoTo_DropDownOpening(object sender, EventArgs e) {
            FillGoToItems(btnGoTo.DropDownItems);
        }

        private void toolStripButtonHotKeys_Click(object sender, EventArgs e) {
            HotKeysDialog();
        }

        private void btnHighlightSameWords_Click(object sender, EventArgs e) {
            if (!btnHighlightSameWords.Checked)
                Editor.Range.ClearStyle(SameWordsStyle);
            else
                HighlightSameWords();
        }

        private void btnSetIntelliSense_Click(object sender, EventArgs e) {
            PopupMenu.Enabled = btnSetIntelliSense.Checked;
        }

        private void btnIntelliSenseFunctions_Click(object sender, EventArgs e) {
            EnableFunctionToolTip = btnIntelliSenseFunctions.Checked;
        }

        private void btnEvaluateByMouse_Click(object sender, EventArgs e) {
            EnableEvaluateByMouse = btnEvaluateByMouse.Checked;
        }

        private void btnAutoCompleteBrackets_Click(object sender, EventArgs e) {
            Editor.AutoCompleteBrackets = btnAutoCompleteBrackets.Checked;
        }

        private void btnHints4CtrlSpace_Click(object sender, EventArgs e) {
            PopupMenu.OnlyCtrlSpace = btnHints4CtrlSpace.Checked;
        }

        private void btnAutoIdent_Click(object sender, EventArgs e) {
            Editor.AutoIndent = btnAutoIdent.Checked;
            btnAutoIdentLines.Enabled = Editor.AutoIndent;
            if (btnAutoIdentLines.Enabled) {
                btnAutoIdentLines.ToolTipText = "";
            } else {
                btnAutoIdentLines.ToolTipText = "Доступно только при включенном автоматическом отступе";
            }
        }

        private void btnAutoIdentChars_Click(object sender, EventArgs e) {
            Editor.AutoIndentExistingLines = btnAutoIdentLines.Checked;
        }

        public void btnMarkChangedLines_Click(object sender, EventArgs e) {
            Editor.HighlightChangedLine = btnMarkChangedLines.Checked;
        }

        private void ToolStripMenuItemCut_Click(object sender, EventArgs e) {
            Editor.Cut();
        }

        private void ToolStripMenuItemCopy_Click(object sender, EventArgs e) {
            Editor.Copy();
        }

        private void ToolStripMenuItemPaste_Click(object sender, EventArgs e) {
            Editor.Paste();
        }

        private void ToolStripMenuItemDelete_Click(object sender, EventArgs e) {
            Editor.Delete();
        }

        private void btnContextMenuCommentBlock_Click(object sender, EventArgs e) {
            Editor.CommentSelected();
        }

        private void ToolStripMenuItemBookmarkClear_Click(object sender, EventArgs e) {
            BookmarkClear();
        }

        private void ToolStripMenuItemClearBreakpoints_Click(object sender, EventArgs e) {
            List<int> lines = new List<int>();
            foreach (Bookmark b in Editor.Breakpoints) lines.Add(b.LineIndex);
            foreach (int iLine in lines) ToggleBreakpoint(iLine);
        }

        private void ToolStripMenuItemSelectAll_Click(object sender, EventArgs e) {
            Editor.SelectAll();
        }

        private void btnContextMenuToggleBookmark_Click(object sender, EventArgs e) {
            Bookmark();
        }

        private void btnContextMenuBack_Click(object sender, EventArgs e) {
            NavigateBackward();
        }

        private void btnContextMenuForward_Click(object sender, EventArgs e) {
            NavigateForward();
        }

        private void btnContextMenuToolBar_Click(object sender, EventArgs e) {
            tsMain.Visible = btnContextMenuToolBar.Checked;
        }

        private void btnContextMenuGotoDef_Click(object sender, EventArgs e) {
            GotoDefinition();
        }

        private void btnHelpPanelContextMenu_Click(object sender, EventArgs e) {
            btnSprav.Checked = btnHelpPanelContextMenu.Checked;
            btnSprav_Click(sender, e);
        }

        private void btnCheckKeywordsRegister_Click(object sender, EventArgs e) {
            Editor.CheckKeywordsRegister = btnCheckKeywordsRegister.Checked;
        }

        private void btnCheckNewVersionOnLoad_Click(object sender, EventArgs e) {
            if (!HMS.NewVersionChecked && btnCheckNewVersionOnLoad.Checked) {
                BackgroundWorker w = new BackgroundWorker();
                w.DoWork += W_DoWork;
                w.RunWorkerCompleted += W_RunWorkerCompleted;
                w.RunWorkerAsync();
            }
        }

        private void W_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            labelNewVersion.Visible = HMS.NewVersionExist;
        }

        private void W_DoWork(object sender, DoWorkEventArgs e) {
            HMS.CheckNewVersion();
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e) {
            ToolStripMenuItemZoom100         .Enabled = (Editor.Zoom != 100);
            ToolStripMenuItemUndo            .Enabled = Editor.UndoEnabled;
            ToolStripMenuItemRedo            .Enabled = Editor.RedoEnabled;
            ToolStripMenuItemBookmarkClear   .Enabled = (Editor.Bookmarks.Count > 0);
            ToolStripMenuItemClearBreakpoints.Enabled = (Editor.Breakpoints.Count > 0);
            btnContextMenuBack   .Enabled = Editor.NavigateBackward(true);
            btnContextMenuForward.Enabled = Editor.NavigateForward (true);
            FillGoToItems(btnGotoContextMenu.DropDownItems);
            btnContextMenuToolBar.Checked = tsMain.Visible;
            btnContextMenuAutoIndent.Enabled = (Editor.SelectionLength > 0);
            btnAdd2Watch.Visible = DebugMode && (Editor.SelectionLength > 0);
            btnHelpPanelContextMenu.Checked = btnSprav.Checked;
        }

        private void btnContextMenuAutoIndent_Click(object sender, EventArgs e) {
            Editor.DoAutoIndent();
        }

        private void ToolStripMenuItemZoom100_Click(object sender, EventArgs e) {
            Editor.Zoom = 100;
        }

        private void ToolStripMenuItemUndo_Click(object sender, EventArgs e) {
            Undo();
        }

        private void ToolStripMenuItemRedo_Click(object sender, EventArgs e) {
            Redo();
        }

        private void ToolStripMenuItemAltPascalScriptHighlight_Click(object sender, EventArgs e) {
            Editor.SyntaxHighlighter.AltPascalKeywordsHighlight = btnUnderlinePascalKeywords.Checked;
            Editor.RefreshTheme();
        }

        private void btnRedStringsHighlight_Click(object sender, EventArgs e) {
            Editor.SyntaxHighlighter.RedStringsHighlight = btnRedStringsHighlight.Checked;
            Editor.RefreshTheme();
        }

        private void Editor_Scroll(object sender, ScrollEventArgs e) {
            HideAllToolTipsAndHints();
        }

        private void btnToolStripMenuItemFONT_Click(object sender, EventArgs e) {
            if (btnToolStripMenuItemFONT.Checked && (HMS.PFC.Families.Length > 1))
                Editor.Font = new Font(HMS.PFC.Families[1], 10f, FontStyle.Regular, GraphicsUnit.Point);
            else
                Editor.Font = new Font("Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
        }

        private void EditorMouseClick(object sender, MouseEventArgs e) {
            if (e.X < (Editor.LeftIndent - 4)) { 
                //System.Windows.Forms.MessageBox.Show("VerticalScroll.Value=" + Editor.VerticalScroll.Value.ToString());
                int iStartLine = Editor.YtoLineIndex();
                int iLine = iStartLine + (e.Y / Editor.CharHeight);
                ToggleBreakpoint(iLine);
            }
        }

        private void Editor_MouseLeave(object sender, EventArgs e) {
            // Do not show help tooltip when mouse is out
            MouseTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Editor_MouseMove(object sender, MouseEventArgs e) {
            if (EnableMouseHelp || (EnableEvaluateByMouse && DebugMode)) {
                if (MouseLocation == e.Location) {
                    // Mouse stopped
                } else {
                    // Mouse mooving
                    MouseLocation = e.Location;
                    ActiveEditor = this;
                    if (ValueHint.Visible) ValueHint.Visible = false;
                    MouseTimer.Change(800, Timeout.Infinite); // Show help tooltip from mouse cursor
                }
            }
        }

        private void btnMouseHelp_Click(object sender, EventArgs e) {
            EnableMouseHelp = btnMouseHelp.Checked;
        }

        private void Editor_MouseClick(object sender, MouseEventArgs e) {
        }

        private void Editor_MouseDoubleClick(object sender, MouseEventArgs e) {
            EditorMouseClick(sender, e);
        }

        private void btnVerticalLineText_Click(object sender, EventArgs e) {
            Editor.PreferredLineWidth = btnVerticalLineText.Checked ? 80 : 0;
            Editor.NeedRecalc();
        }

        private void btnEnableFolding_Click(object sender, EventArgs e) {
            Editor.ShowFoldingMarkers = btnEnableFolding.Checked;
            Editor.NeedRecalc();
        }

        private void btnAbout_Click(object sender, EventArgs e) {
            AboutDialog aboutDialog = new AboutDialog();
            aboutDialog.ShowDialog();
        }

        private void btnBoldCaret_Click(object sender, EventArgs e) {
            Editor.BoldCaret = btnBoldCaret.Checked;
        }

        private void btnStorePositions_Click(object sender, EventArgs e) {

        }

        private void btnAdd2Watch_Click(object sender, EventArgs e) {
            object text = Editor.SelectedText;
            HmsScriptFrame.AddWatch(ref text);
        }

        private void btnSprav_Click(object sender, EventArgs e) {
            splitContainer1.Panel2Collapsed = !btnSprav.Checked;
        }

        private void splitContainer1_DoubleClick(object sender, EventArgs e) {
            btnSprav.Checked = !btnSprav.Checked;
            btnSprav_Click(sender, e);
        }

        private void labelNewVersion_Click(object sender, EventArgs e) {
            if (HMS.UpdateInfo.Length == 0) return;
            frmUpdateInfoDialog form = new frmUpdateInfoDialog();
            string html = HMS.ReadTextFromResource("Markdown.html");
            form.SetText(html.Replace("<MarkdownText>", HMS.UpdateInfo));
            form.ShowDialog();
        }

        private void labelVersion_MouseDown(object sender, MouseEventArgs e) {
            btnAbout_Click(sender, e);
        }

        private void labelVersion_DoubleClick(object sender, EventArgs e) {
            btnAbout_Click(sender, e);
        }

        private void btnFormatCodeWhenPaste_Click(object sender, EventArgs e) {
            Editor.FormatCodeWhenPaste = btnFormatCodeWhenPaste.Checked;
        }
        #endregion Control Events

        #region Smart IDE functions
        private static string MatchReturnEmptyLines(Match m) { return regexNotNewLine.Replace(m.Value, CensChar.ToString()); }
        private static string MatchRemoveLinebreaks(Match m) { return regexLineBreaks.Replace(m.Value, String.Empty); }

        public string EvalVariableValue(string varName) {
            object varname = varName;
            object result  = "";
            if (HmsScriptFrame != null)
                HmsScriptFrame.SolveExpression(ref varname, ref result);
            if (result == null) return "";
            return result.ToString();
        }

        public bool CheckDebugState() {
            int running = 0;
            int currentSourceLine = 0;
            int currentSourceChar = 0;
            if (HmsScriptFrame != null)
                HmsScriptFrame.GetCurrentState(ref running, ref currentSourceLine, ref currentSourceChar);
            Editor.DebugMode    = running < 0;
            Editor.HmsDebugLine = currentSourceLine - 1;
            Editor.HmsDebugChar = currentSourceChar - 1;
            Editor.NeedRecalc();
            return Editor.DebugMode;
        }

        private static Regex forbittenSameText = new Regex(@"\W+", RegexOptions.Compiled);
        private void HighlightSameWords() {
            Editor.Range.ClearStyle(SameWordsStyle);
            if (!Editor.Selection.IsEmpty) return;
            var fragment = Editor.Selection.GetFragment(@"\w");
            string text = fragment.Text;
            if (text.Length > 0 && !forbittenSameText.IsMatch(text)) {
                Editor.Range.SetStyle(SameWordsStyle, "\\b" + text + "\\b", RegexOptions.Multiline);
            }
        }

		private static Regex regexTextOfComment = new Regex(@"^\s*?//.*?(\w.*)", RegexOptions.Compiled);

        private void BuildFunctionList() {
            Functions.Clear();
            MatchCollection mc = null;
            string startBlock = "", endBlock = "";
            string txt = Editor.WithoutStringAndComments(Editor.Text);
            switch (Editor.Language) {
                case Language.CPPScript   :
                case Language.JScript     : mc = regexProceduresCPP   .Matches(txt); startBlock = "{"; endBlock = "}"; break;
                case Language.PascalScript: mc = regexProceduresPascal.Matches(txt); startBlock = @"\b(begin|try)\b"; endBlock = @"\b(end)\b"; break;
                case Language.BasicScript : mc = regexProceduresBasic .Matches(txt); startBlock = @"\b(Sub|Function)\b"; endBlock = @"\bEnd (Sub|Function)\b"; break;
            }

            if (mc != null) {
                foreach (Match m in mc) {
                    string name = m.Groups[1].Value;
                    if (regexExcludeWords.IsMatch(m.Value)) continue;
                    HMSItem item = new HMSItem();
                    item.Type          = m.Groups["type"].Value;
                    item.Text          = name;
                    item.MenuText      = name;
                    item.Kind          = regexDetectProcedure.IsMatch(m.Value) ? DefKind.Procedure : DefKind.Function;
                    item.ImageIndex    = (item.Kind == DefKind.Function) ? ImagesIndex.Function : ImagesIndex.Procedure;
                    item.ToolTipTitle  = name;
                    item.ToolTipText   = ((item.Kind == DefKind.Function) ? "Функция" : "Процедура") + " (объявлена в скрипте)";
                    item.PositionReal  = m.Index;
                    item.PositionStart = m.Groups[1].Index;
                    item.PositionEnd   = item.PositionStart + m.Groups[1].Value.Length;
                    // check comment before procedure
                    int iLine = Editor.PositionToPlace(item.PositionStart).iLine;
                    if (iLine > 0) {
                        Match matchComment = regexTextOfComment.Match(Editor.Lines[iLine-1]);
                        if (matchComment.Success) item.Help = matchComment.Groups[1].Value;
                    }
                    // search end of procedure
                    if (startBlock.Length > 0) {
                        var stack = new Stack<string>();
                        MatchCollection mc2 = Regex.Matches(txt.Substring(item.PositionStart), "(" + startBlock + "|" + endBlock + ")", StdOpt);
                        foreach (Match m2 in mc2) {
                            if (Regex.IsMatch(m2.Value, startBlock, StdOpt)) stack.Push(startBlock);
                            else if (stack.Count > 0) stack.Pop();
                            item.PositionEnd = item.PositionStart + m2.Groups[1].Index;
                            if (stack.Count < 1) break;
                        }
                        item.PositionEnd += endBlock.Length;
                    }
                    string s = new Range(Editor, Editor.PositionToPlace(item.PositionStart), Editor.PositionToPlace(item.PositionEnd)).Text;
                    Match m3 = Regex.Match(s, @"^(.*?)(\bvar\b|" + startBlock + ")", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    if (m3.Success) item.ToolTipTitle = m3.Groups[1].Value.Trim().Replace("\r", "").Replace("\n", "");
                    if (item.Kind == DefKind.Function) {
                        if (Editor.Language == Language.PascalScript) {
                            item.Type = HMS.GetVarTypePascalFormat(item.ToolTipTitle);
                        } else {
                            item.ToolTipTitle = item.Type + " " + item.ToolTipTitle;
                        }
                    }
                    Functions.Add(item);
                }
            }
            // add info about main start procedure

            if (Functions.LastEndPosition < txt.Length) {
                Match matchMainProc = Regex.Match(txt.Substring(Functions.LastEndPosition), startBlock, StdOpt);
                if (matchMainProc.Success) {
                    HMSItem item = new HMSItem();
                    item.Type          = "MainProcedure";
                    item.Text          = "Главная процедура";
                    item.MenuText      = item.Text;
                    item.Kind          = DefKind.Procedure;
                    item.Help          = "Процедура, с которой начинается запуск скрипта";
                    item.ImageIndex    = ImagesIndex.Procedure;
                    item.PositionStart = Functions.LastEndPosition + matchMainProc.Index;
                    item.PositionEnd   = txt.Length - 1;
                    Functions.Add(item);
                }
            }
            PopupMenu.Items.SetVisibleFunctionsItems(Functions);
        }

        private string CurrentWord() {
            Range r = new Range(Editor, Editor.Selection.Start, Editor.Selection.Start);
            r = r.GetFragment(@"[\w]");
            if (r.IsStringOrComment) return "";
            return r.Text;
        }

        private string GetGlobalContext() {
            char[] txt = Editor.WithoutStringAndComments(Editor.Text).ToCharArray();
            foreach (HMSItem item in Functions) {
                for (int i = item.PositionReal; i < item.PositionEnd; i++) {
                    if (item.Type == "MainProcedure") continue;
                    if (i >= txt.Length) break;
                    txt[i] = txt[i] != '\n' ? CensChar : '\n';
                }
            }
            return new string(txt);
        }

        private HMSItem GetCurrentProcedure(int position) {
            foreach (var item in Functions) if ((position > item.PositionStart) && (position < item.PositionEnd)) return item;
            return null;
        }

        private void GotoDefinition() {
            Editor_SelectionChangedDelayed(null, new EventArgs());
            string name = CurrentWord();
            if      (Variables.ContainsName(name)) GotoPosition(Variables[name].PositionStart);
            else if (LocalVars.ContainsName(name)) GotoPosition(LocalVars[name].PositionStart);
            else if (Functions.ContainsName(name)) GotoPosition(Functions[name].PositionStart);
        }

        private void UpdateCurrentVisibleVariables(int position = -1) {
            if (Editor.Language == Language.YAML) return;
            if (position < 0) position = Editor.SelectionStart;
            HMSItem itemFunction = GetCurrentProcedure(position);

            if ((itemFunction != null) && (itemFunction.Type != "MainProcedure")) {
                if ((itemFunction.PositionStart == LastPtocedureIndex) && !NeedRecalcVars) return; // We are in same procedure - skip update
                LastPtocedureIndex = itemFunction.PositionStart;
            } else if ((LastPtocedureIndex == 0) && !NeedRecalcVars) {
                return;
            } else
                LastPtocedureIndex = 0;

            NeedRecalcVars = false;
            LocalVars.Clear();

            if ((itemFunction != null) && (itemFunction.Type != "MainProcedure")) {
                string context = Editor.WithoutStringAndComments(Editor.GetRange(itemFunction.PositionStart, itemFunction.PositionEnd).Text);
                if (context.Length > 0) GetVariables(context, itemFunction.PositionStart, LocalVars, Variables);
                if (itemFunction.Kind == DefKind.Function) {
                    HMSItem hmsItem = new HMSItem("Result");
                    hmsItem.ImageIndex    = ImagesIndex.Field;
                    hmsItem.MenuText      = hmsItem.Text;
                    hmsItem.Type          = itemFunction.Type;
                    hmsItem.PositionStart = itemFunction.PositionStart;
                    hmsItem.PositionEnd   = itemFunction.PositionEnd;
                    hmsItem.ToolTipText   = "Переменная, хранящая значение, которое будет возвращено функцией как результат.";
                    hmsItem.Help          = "Используется в PascalScript, но видна как переменная и в других режимах синтаксиса.\nИмеет такой-же тип, как и функция, в которой она видна.";
                    hmsItem.ToolTipTitle  = "Result: " + hmsItem.Type;
                    LocalVars.Add(hmsItem);
                }
            } else {
                Variables.Clear();
                string contextGlobal = GetGlobalContext();
                if (contextGlobal.Length > 0) GetVariables(contextGlobal, 0, Variables, LocalVars);
            }
            PopupMenu.Items.SetVisibleVariablesItems(Variables);
            PopupMenu.Items.SetLocalssVariablesItems(LocalVars);
        }

        private string GetTypeOfConstant(string part) {
            if (part.Length == 0        ) return "String";
            if (regexIsNum.IsMatch(part)) return "Integer";
            if (regexIsStr.IsMatch(part)) return "String";
            return "Variant";
        }

        private void GetVariables(string txt, int indexContext, AutocompleteItems ITEMS, AutocompleteItems ITEMSVersus) {
            MatchCollection mc = null; bool isGlobalContext = (indexContext == 0);
            // Collect constants
            if (isGlobalContext) {
                switch (Editor.Language) {
                    case Language.CPPScript:
                        mc = regexSearchConstantsCPP.Matches(txt);
                        foreach (Match m in mc) {
                            string name = m.Groups[1].Value;
                            string sval = m.Groups[2].Value.Trim(); // Value
                            if (!ITEMS.ContainsName(name)) {
                                HMSItem item = new HMSItem();
                                item.Global        = isGlobalContext;
                                item.Kind          = DefKind.Constant;
                                item.ImageIndex    = ImagesIndex.Enum;
                                item.Text          = name.Trim();
                                item.MenuText      = RemoveLinebeaks(item.Text);
                                item.ToolTipTitle  = item.Text;
                                item.ToolTipText   = "Объявленная константа";
                                item.Type          = GetTypeOfConstant(sval);
                                item.PositionStart = m.Groups[1].Index + indexContext;
                                item.PositionEnd   = item.PositionStart + name.Length;
                                if (item.Type.Length > 0) item.ToolTipText += "\nТип: " + item.Type;
                                if ((sval.Length == 0) || (sval == ";")) sval = regexExractConstantValue.Match(Editor.Text.Substring(m.Groups[2].Index, 96)).Value;
                                if (sval.Length > 0) item.Help += "\nЗначение: " + sval;
                                ITEMS.Add(item);
                            }
                        }
                        break;
                    case Language.PascalScript:
                        Match c = regexSearchConstantsPascal1.Match(txt);
                        if (c.Success) {
                            mc = regexSearchConstantsPascal2.Matches(c.Groups[1].Value);
                            foreach (Match m in mc) {
                                string name = m.Groups[1].Value;
                                string sval = m.Groups[2].Value.Trim(); // Value
                                if (!ITEMS.ContainsName(name)) {
                                    HMSItem item = new HMSItem();
                                    item.Global        = isGlobalContext;
                                    item.Kind          = DefKind.Constant;
                                    item.PositionStart = c.Groups[1].Index + m.Index + indexContext;
                                    item.PositionEnd   = item.PositionStart + name.Length;
                                    item.ImageIndex    = ImagesIndex.Enum;
                                    item.Text          = name.Trim();
                                    item.MenuText      = RemoveLinebeaks(item.Text);
                                    item.ToolTipTitle  = item.Text;
                                    item.ToolTipText   = "Объявленная константа";
                                    item.Type          = GetTypeOfConstant(sval);
                                    if (item.Type.Length > 0) item.ToolTipText += "\nТип: " + item.Type;
                                    if ((sval.Length == 0) || (sval == ";")) sval = regexExractConstantValue.Match(Editor.Text.Substring(m.Groups[2].Index, 96)).Value;
                                    if (sval.Length > 0) item.Help += "\nЗначение: " + sval;
                                    ITEMS.Add(item);
                                }
                            }

                        }
                        break;
                }
            }

            mc = null;
            switch (Editor.Language) {
                case Language.CPPScript   : mc = regexSearchVarsCPP   .Matches(txt); break;
                case Language.JScript     : mc = regexSearchVarsJS    .Matches(txt); break;
                case Language.PascalScript: mc = regexSearchVarsPascal.Matches(txt); break;
            }
            if (mc != null) {
                foreach (Match m in mc) {
                    int    index = m.Groups["vars"].Index;
                    string names = m.Groups["vars"].Value;
                    string type  = m.Groups["type"].Value.Trim();
                    if (!ValidHmsType(type)) continue;
                    names = HMS.GetTextWithoutBrackets(names); // Убираем скобки и всё что в них
                    names = regexAssignment  .Replace(names, evaluatorSpaces); // Убираем присвоение - знак равно и после
                    names = regexConstantKeys.Replace(names, evaluatorSpaces); // Убираем ключевые слова констант (var, const)
                    string[] aname = names.Split(',');
                    foreach (string namePart in aname) {
                        string name = namePart;
                        if ((namePart.Trim().Length != 0) && !regexExcludeWords.IsMatch(namePart)) {
                            if (Regex.IsMatch(name, @"\b(\w+).*?\b(\w+).*?\b(\w+)")) continue;
                            Match m2 = regexTwoWords.Match(name);
                            if (m2.Success) {
                                bool typeFirst = (index > m.Groups["type"].Index);
                                type   = m2.Groups[typeFirst ? 1 : 2].Value;
                                name   = m2.Groups[typeFirst ? 2 : 1].Value;
                                index += m2.Groups[typeFirst ? 2 : 1].Index;
                            }
                            if (!regexNotValidCharsInVars.IsMatch(name) && !ITEMS.ContainsName(name) && !Functions.ContainsName(name)) {
                                HMSItem item = new HMSItem();
                                item.Global        = isGlobalContext;
                                item.Kind          = DefKind.Variable;
                                item.Text          = name.Trim();
                                item.Type          = type.Trim();
                                item.MenuText      = RemoveLinebeaks(item.Text);
                                item.ToolTipTitle  = item.Text;
                                item.ToolTipText   = item.Global ? "Глобальная переменная" : "Локальная переменная";
                                item.PositionStart = index + (name.Length - name.TrimStart().Length) + indexContext;
                                item.PositionEnd   = item.PositionStart + name.Length;
                                item.ImageIndex    = ImagesIndex.Field;
                                if (item.Type.Length > 0) item.ToolTipText += "\nТип: " + item.Type;
                                ITEMS.Add(item);
                                HMSItem it = ITEMSVersus.GetItemOrNull(item.MenuText);
                                if (it != null) {
                                    ITEMSVersus.Remove(it);
                                }
                            }
                            if (m2.Success) index -= m2.Groups["vars"].Index;
                        }
                        index += namePart.Length + 1;
                    }
                }
            }
        }

        private bool ValidHmsType(string type) {
            string lowertype = type.ToLower();
            if (CurrentValidTypes.IndexOf("|" + lowertype + "|") >= 0) return true;
            if (HMS.ClassesString.IndexOf("|" + lowertype + "|") >= 0) return true;
            return false;
        }

        public HMSItem GetHMSItemByText(string text) {
            string partAfterDot;
            return GetHMSItemByText(text, out partAfterDot);
        }

        public HMSItem GetHMSItemByText(string text, out string partAfterDot, bool returnItemBeforeDot = false) {
            HMSItem      item = null;
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
                    if (item == null) item =         LocalVars.GetItemOrNull(name); // try visible known variables
                    if (item == null) item =         Variables.GetItemOrNull(name); // try visible known variables
                    if (item == null) item =         Functions.GetItemOrNull(name); // try functions in script
                    if (item == null) item = HMS.ItemsVariable.GetItemOrNull(name); // try internal variables
                    if (item == null) item = HMS.ItemsConstant.GetItemOrNull(name); // try internal constants
                    if (item == null) item = HMS.ItemsFunction.GetItemOrNull(name); // try internal functions
                    if (item == null) item = HMS.ItemsClass   .GetItemOrNull(name); // try internal classes
                    if (count < names.Length) {
                        if (item != null) {
                            info = HMS.HmsClasses[item.MenuText];
                            if (info.Name.Length == 0) info = HMS.HmsClasses[item.Type];
                        } else break;
                    }
                }
            }
            return item;
        }

        private string NameCase(string s) {
            if (s.Length > 0) {
                char[] c = s.Trim().ToLower().ToCharArray();
                c[0] = c[0].ToString().ToUpper().ToCharArray()[0];
                s = new String(c);
            }
            return s;
        }

        private void GotoPosition(int position) {
            Editor.SelectionStart = position;
            Editor.DoRangeVisible(Editor.Selection, true);
            Editor.Invalidate();
        }

        private void CheckPositionIsInParametersSequence_DoWork(object sender, DoWorkEventArgs e) {
            if (Editor.IsUpdating) {
                for (int i = 3; i > 0; i--) {
                    Thread.Sleep(100);
                    if (!Editor.IsUpdating) break;
                }
            }
            if (!Editor.IsUpdating) CheckPositionIsInParametersSequence();
        }

        private void CheckPositionIsInParametersSequence() {
            string name, parameters;

            string text = Editor.Selection.GetFunctionLookedLeft();

            HMS.CurrentParamType = "";
            Match m = regexFoundOurFunction.Match(text);
            if (m.Success) {
                name       = m.Groups[1].Value;
                parameters = m.Groups[2].Value;
                Place pp = Editor.PositionToPlace(Editor.SelectionStart - text.Length + m.Index);
                int iLinesCorrect = Editor.Selection.Start.iLine - pp.iLine + 1;
                Point p  = Editor.PositionToPoint(Editor.SelectionStart - text.Length + m.Index);
                p.Offset(0, Editor.CharHeight * iLinesCorrect + 2 );
                Editor.ToolTip4Function.iLine = Editor.Selection.Start.iLine;
                ShowFunctionToolTip(p, name, parameters);
            } else {
                if (Editor.ToolTip4Function.Visible)
                    HideToolTip4Function(true);
            }
            WasCommaOrBracket = false;
            return;
        }

        private void ShowFunctionToolTip(Point p, string name, string parameters="") {
            int paramNum = regexSplitFuncParam.Split(parameters).Length;
            HMS.CurrentParamType = "";
            HMSItem item = GetHMSItemByText(name);
            if (item != null) {
                if (item.IsFuncOrProcedure || (item.Kind == DefKind.Method)) {
                    if ((Editor.SelectionStart >= item.PositionStart) && (Editor.SelectionStart <= item.PositionEnd)) return; // we writing this function
                    Editor.ToolTip4Function.ShowFunctionParams(item, paramNum, Editor, p);
                }
            }
        }

        private void InsertTemplate(string text) {
            int posSta = Editor.SelectionStart;
            int posEnd = posSta + text.Length;
            text = FormatCodeText(text);
            Editor.InsertText(text);
        }

        private string FormatCodeText(string text) {
            // Поиск минимального отступа в коде вставляемого текста
            int codeIndent = 200;
            foreach (string line in text.Split('\n')) {
                int indent = line.Length - line.TrimStart().Length;
                codeIndent = Math.Min(indent, codeIndent);
                if (codeIndent == 0) break;
            }
            // Поиск текущего отступа для кода в редакторе по текущей позиции
            int iLine = Editor.Selection.Start.iLine;
            int needIndent = Editor.GetRealLine(iLine).AutoIndentSpacesNeededCount;
            for (int i = iLine; i >= 0; i--) {
                string line = Editor.Lines[i];
                needIndent = line.Length - line.TrimStart().Length;
                if (line.Trim().Length > 0) break;
            }
            bool firstLine = true;
            Range fragmentLine = new Range(Editor, new Place(0, Editor.Selection.Start.iLine), Editor.Selection.Start);
            bool currentLineWithText = (fragmentLine.Text.Trim().Length > 0);
            StringBuilder sb = new StringBuilder();
            foreach (string line in text.Split('\n')) {
                string newLine = line.Substring(codeIndent).TrimEnd();
                if (firstLine && currentLineWithText) {
                } else {
                    newLine = newLine.PadLeft(newLine.Length + needIndent);
                }
                sb.AppendLine(newLine);
                firstLine = false;
            }
            text = sb.ToString();
            return text;
        }

        #endregion

        private void SetAutoCompleteMenu() {
            PopupMenu = new AutocompleteMenu(Editor, this);
            PopupMenu.ImageList         = imageList1;
            PopupMenu.MinFragmentLength = 1; 
            PopupMenu.InitSize = new Size(210, PopupMenu.Items.ItemHeight * MaxPopupItems);
            PopupMenu.InitDefaultSize();
        }

        public void CreateAutocomplete() {
            if (PopupMenu == null || PopupMenu.IsDisposed) return;
            string hmsTypes = HMS.HmsTypesStringWithHelp;
            string keywords = "", snippets = "";
            string hlp = "", key = "";
            CurrentValidTypes    = HMS.HmsTypesString;
            switch (ScriptLanguage) {
                case "C++Script":
                    CurrentValidTypes    += "int|long|void|bool|float|";
                    CurrentValidTypesReg += "int|long|void|bool|float|";
                    hmsTypes = hmsTypes.Replace("Integer|", "int|long|").Replace("Extended|", "extended|float|").Replace("Boolean|", "bool|").Replace("String", "string") + "|{Тип функции: процедура (отсутствие возвращаемого значения)}void|";
                    keywords = "#include|#define|new|break|continue|exit|delete|return|if|else|switch|default|case|do|while|for|try|finally|except|in|is|nil|null|true|false|";
                    snippets = "if (^) {\n}|if (^) {\n}\nelse {\n}|for (^;;) {\n}|while (^) {\n}|do {\n^}while ();";
                    break;
                case "PascalScript":
                    keywords = "Program|Uses|Const|Var|Not|In|Is|OR|XOR|DIV|MOD|AND|SHL|SHR|Break|Continue|Exit|Begin|End|If|Then|Else|Case|Of|Repeat|Until|While|Do|For|To|DownTo|Try|Finally|Except|With|Function|Procedure|Nil|Null|True|False";
                    snippets = "If ^ Then |If (^) Then Begin\nEnd else Begin\nEnd;";
                    break;
                case "BasicScript":
                    keywords = "EOL|IMPORTS|DIM|AS|NOT|IN|IS|OR|XOR|MOD|AND|ADDRESSOF|BREAK|CONTINUE|EXIT|DELETE|SET|RETURN|IF|THEN|END|ELSEIF|ELSE|SELECT|CASE|DO|LOOP|UNTIL|WHILE|WEND|FOR|TO|STEP|NEXT|TRY|FINALLY|CATCH|WITH|SUB|FUNCTION|BYREF|BYVAL|NIL|NULL|TRUE|FALSE";
                    break;
                case "JScript":
                    hmsTypes = "var";
                    keywords = "import|new|in|is|break|continue|exit|delete|return|if|else|switch|default|case|do|while|for|try|finally|except|function|with|Nil|Null|True|False";
                    break;
            }
            CurrentValidTypesReg = Regex.Replace(hmsTypes, "{.*?}", "");
            HMS.Keywords = keywords;
            HMS.KeywordsString = keywords.ToLower();
            snippets += "|ShowMessage(\"^\");|HmsLogMessage(1, \"^\");";
            lock (LockObject) {
                var items = new AutocompleteItems();

                foreach (var s in keywords.Split('|')) if (s.Length > 0) items.Add(new HMSItem(s, ImagesIndex.Keyword, s, s, "Ключевое слово"));
                foreach (var s in snippets.Split('|')) if (s.Length > 0) items.Add(new SnippetHMSItem(s) { ImageIndex = ImagesIndex.Snippet });

                foreach (var name in hmsTypes.Split('|')) {
                    Match m = Regex.Match(name, "{(.*?)}");
                    if (m.Success) hlp = m.Groups[1].Value;
                    key = Regex.Replace(name, "{.*?}", "");
                    items.Add(new HMSItem(key, ImagesIndex.Keyword, key, key, hlp));
                }

                PopupMenu.Items.SetAutocompleteItems(items);
                PopupMenu.Filter = HmsScriptMode.ToString();
                PopupMenu.Items.AddAutocompleteItems(HMS.ItemsFunction);
                PopupMenu.Items.AddFilteredItems    (HMS.ItemsVariable);
                PopupMenu.Items.AddAutocompleteItems(HMS.ItemsConstant);
                PopupMenu.Items.AddAutocompleteItems(HMS.ItemsClass   );
                PopupMenu.Items.AddAutocompleteItems(ScriptAutocompleteItems);

            }
        }

        AutocompleteItems ScriptAutocompleteItems = new AutocompleteItems();
        private static Regex regexCutFunctions = new Regex("<Functions>(.*?)</Functions>", RegexOptions.Compiled);
        private static Regex regexCutClasses   = new Regex("<Classes>(.*?)</Classes>"    , RegexOptions.Compiled);
        private static Regex regexCutTypes     = new Regex("<Types>(.*?)</Types>"        , RegexOptions.Compiled);
        private static Regex regexCutVariables = new Regex("<Variables>(.*?)</Variables>", RegexOptions.Compiled);
        private static Regex regexCutConstants = new Regex("<Constants>(.*?)</Constants>", RegexOptions.Compiled);
        private static Regex regexCutItem      = new Regex("<item[^>]+/>"                , RegexOptions.Compiled);
        private static Regex regexCutText      = new Regex("text=\"([^\"]+)"             , RegexOptions.Compiled);
        private static Regex regexCutTextF     = new Regex("text=\"\\w+ ([^\"]+)"        , RegexOptions.Compiled);
        private static Regex regexCutDesc      = new Regex("description=\"([^\"]+)"      , RegexOptions.Compiled);
        private static Regex regexExcludeConst = new Regex("^(True|False|nil|Null)"      , RegexOptions.Compiled);

        private void SearchAutocompleteItemsInScriptDescrition(Regex reBlock, ref string xml, int imageIndex, string toolTipText, DefKind kind, AutocompleteItems Items) {
            string xmlBlock = reBlock.Match(xml).Value;
            string text, descr; HMSItem item, foundItem;
            MatchCollection mc = regexCutItem.Matches(xmlBlock);
            foreach (Match matchItem in mc) {
                if (kind == DefKind.Function)
                    text = regexCutTextF.Match(matchItem.Value).Groups[1].Value;
                else
                    text = regexCutText.Match(matchItem.Value).Groups[1].Value;
                descr = regexCutDesc.Match(matchItem.Value).Groups[1].Value;
                if (text.Length==0) continue;
                item = HMS.GetHmsItemFromLine(text);
                item.ImageIndex  = imageIndex;
                item.ToolTipText = toolTipText;
                item.Kind        = kind;
                item.Help        = descr;
                if (kind == DefKind.Function) item.Kind = (item.Type.Length > 0) ? DefKind.Function : DefKind.Procedure;
                if (regexExcludeConst.IsMatch(item.MenuText)) continue;
                foundItem = Items.GetItemOrNull(item.MenuText);
                if (foundItem!=null) {
                    if (foundItem.Help.Length == 0) foundItem.Help = descr;
                } else {
                    ScriptAutocompleteItems.Add(item);
                    //Console.WriteLine(kind.ToString() + " MenuText: " + item.MenuText);
                }
            }
        }

        private void CreateAutocompleteItemsByScriptDescrition() {
            ScriptAutocompleteItems.Clear();
            string xml = "";
#if DEBUG
            var sw = Stopwatch.StartNew();

            if ((HmsScriptFrame == null) && (File.Exists(@"D:\descr.txt")))
                xml = File.ReadAllText(@"D:\descr.txt");
#endif
            if (HmsScriptFrame != null) {
                xml = GetScriptDescriptions();
                //File.WriteAllText(@"D:\descr.txt", xml); 
            }

            if (!string.IsNullOrEmpty(xml)) {
                SearchAutocompleteItemsInScriptDescrition(regexCutFunctions, ref xml, ImagesIndex.Procedure, ""                     , DefKind.Function, HMS.ItemsFunction);
                SearchAutocompleteItemsInScriptDescrition(regexCutVariables, ref xml, ImagesIndex.Field    , "Встроенная переменная", DefKind.Variable, HMS.ItemsVariable);
                SearchAutocompleteItemsInScriptDescrition(regexCutConstants, ref xml, ImagesIndex.Enum     , "Встроенная константа" , DefKind.Constant, HMS.ItemsConstant);
            }

#if DEBUG
            sw.Stop();
            Console.WriteLine("CreateAutocompleteItemsByScriptDescrition ElapsedMilliseconds: " + sw.ElapsedMilliseconds);
            Console.WriteLine("ScriptAutocompleteItems.Count: " + ScriptAutocompleteItems.Count);
#endif
            HMS.AllowPrepareFastDraw = true;
            HMS.PrepareFastDrawInBackground();
        }

        public void CreateInsertTemplateItems() {
            // Set templates for selected script language
            btnInsertTemplate.DropDownItems.Clear();
            AddTemplateItemsRecursive(btnInsertTemplate, HMS.Templates[Editor.Language]);
            btnInsertTemplate.Visible = btnInsertTemplate.DropDownItems.Count > 0;
        }

        private void AddTemplateItemsRecursive(ToolStripMenuItem menuItem, Templates templates) {
            foreach (TemplateItem templateItem in templates) {
                ToolStripItem item = HMS.SetTemplateMenuItem(menuItem, templateItem.Name, templateItem.Text);
                if (templateItem.Submenu) {
                    AddTemplateItemsRecursive((ToolStripMenuItem)item, templateItem.ChildItems);

                } else {
                    item.Click += (o, a) => {
                        InsertTemplate((o as ToolStripItem).AccessibleDescription);
                    };

                } // if
            } // foreach

        } // end AddTemplateItemsRecursive

    }
}
