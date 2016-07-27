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
using Timer = System.Threading.Timer;
using System.Windows.Forms;
using FastColoredTextBoxNS;
// ReSharper disable once RedundantUsingDirective
using System.Diagnostics;
using HMSEditorNS.Properties;

namespace HMSEditorNS {
    public sealed partial class HMSEditor: UserControl {
        #region Static
        public  static string    Title           = "HMS Editor addon v" + AboutDialog.AssemblyVersion;
        public  static string    Description     = "Альтернативный редактор скриптов c поддержкой IntelliSence";
        public  static string    SettingsSection = "AddonMain";
        public  static bool      NeedRestart     = false;
        public  static string    NeedCopyNewFile = "";
        public  static string    NeedCopyDllFile = "";
        public  static INI       Settings        = new INI(HMS.SettingsFile);
        public  static HMSEditor ActiveEditor;
        private static bool      EnableMouseHelp;

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
            PtrScriptFrame = aScriptFrame;
            if (PtrScriptFrame != IntPtr.Zero) {
                HmsScriptFrame = (IHmsScriptFrame)System.Runtime.Remoting.Services.EnterpriseServicesHelper.WrapIUnknownWithComObject(PtrScriptFrame);
            }
            HmsScriptMode  = (HmsScriptMode)aScriptMode;
            InitializeComponent();
            labelVersion.Text = Title;
            splitContainer1.Panel2Collapsed = true;
            TB.CurrentLineColor = Color.FromArgb(100, 210, 210, 255);
            TB.ChangedLineColor = Color.FromArgb(255, 152, 251, 152);
            PopupMenu = new AutocompleteMenu(TB) { ImageList = imageList1, MinFragmentLength = 1 };
            PopupMenu.InitSize = new Size(210, PopupMenu.Items.ItemHeight * MaxPopupItems);
            PopupMenu.InitDefaultSize();
            helpPanel1.PanelClose += HelpPanel1_PanelClose;
            helpPanel1.Init(imageList1, HmsScriptMode.ToString());
            CodeAnalysis.Init();
        }

        private void HelpPanel1_PanelClose(object sender, EventArgs e) {
            splitContainer1.Panel2Collapsed = true;
            btnSprav.Checked = false;
        }

        private void TimeoutCheckSyntax(object state) {
            t?.Dispose();
            //HMS.LogError("Произошел сбой при автоматической проверке синтаксиса.\nАвтор этого редактора не отказался бы получить любую информацию о произошедшем.\n:(");
            TB.ClearErrorLines();
        }

        Timer t;
        bool CheckSyntaxIsBusy = false;
        bool CheckSyntaxAgain  = false;

        public void AutoCheckSyntaxBackground() {
            if (PtrScriptFrame == IntPtr.Zero || TB.IsDisposed || !TB.IsHandleCreated || !IsHandleCreated) return;
            if (CheckSyntaxIsBusy) { CheckSyntaxAgain = true; return; }
            CheckSyntaxIsBusy = true;
            t = new Timer(TimeoutCheckSyntax, null, 3000, Timeout.Infinite);
            object objScriptName   = new BStrWrapper(ScriptLanguage);
            object objScriptText   = new BStrWrapper(TB.Text);
            object objErrorMessage = new BStrWrapper("");
            var th = new Thread(() =>
            {
                try {
                    int nErrorLine = 0;
                    int nErrorChar = 0;
                    int nResult    = 0;
                    IHmsScriptFrame scriptFrame1 = (IHmsScriptFrame)System.Runtime.Remoting.Services.EnterpriseServicesHelper.WrapIUnknownWithComObject(PtrScriptFrame);
                    scriptFrame1.CompileScript(ref objScriptName, ref objScriptText, ref objErrorMessage, ref nErrorLine, ref nErrorChar, ref nResult);
                    CheckSyntaxArgs args = new CheckSyntaxArgs();
                    if (nResult == 0) {
                        args.ErrorChar    = Math.Max(0, nErrorChar - 1);
                        args.ErrorLine    = Math.Max(0, nErrorLine - 1);
                        args.ErrorMessage = objErrorMessage.ToString();
                    }
                    TB.Invoke((MethodInvoker)delegate {
                        t?.Dispose();
                        if (args.ErrorMessage.Length > 0)
                            TB.SetErrorLines(args.ErrorChar, args.ErrorLine, args.ErrorMessage);
                        else
                            TB.ClearErrorLines();
                        CheckSyntaxIsBusy = false;
                        if (CheckSyntaxAgain) {
                            CheckSyntaxAgain = false;
                            //AutoCheckSyntaxBackground();
                        }
                    });

                } catch (Exception e) {
                    TB.ClearErrorLines();
                    HMS.LogError(e.ToString());
                }
            }, 10);
            th.Start();
        }

        // Fields
        public  bool   Locked;
        public  string Filename             = HMS.TemplatesDir; // last opened or saved file 
        public  int    LastPtocedureIndex   = -1;
        public  string CurrentValidTypes    = ""; // Sets in CreateAutocomplete() procedure
        public  string CurrentValidTypesReg = ""; // Sets in CreateAutocomplete() procedure
        public  bool   IsFirstActivate      = true;
        public  bool   WasCommaOrBracket;
        public  bool   NeedRecalcVars;
        private string ThemeName            = "Стандартная";
        private uint   OldTextHash;

        public  IHmsScriptFrame  HmsScriptFrame;
        private IntPtr           PtrScriptFrame;
        private HmsScriptMode    HmsScriptMode ;

        public ImageList IconList       => imageList1;
        public bool      Modified     { get { return TB.IsChanged     ; } set { TB.IsChanged = value; } }
        public string    SelectedText { get { return TB.Selection.Text; } set { TB.InsertText(value); } }

        public bool NavigateBackwardEnable { get { return btnNavigateBack   .Enabled; } set { btnNavigateBack   .Enabled = value; } }
        public bool NavigateForwardEnable  { get { return btnNavigateForward.Enabled; } set { btnNavigateForward.Enabled = value; } }

        //public ValueHintControl ValueHint = new ValueHintControl();
        public ValueToolTip ValueHint = new ValueToolTip();
        public FormValue    ValueForm = new FormValue();

        public AutocompleteItems LocalVars = new AutocompleteItems();
        public AutocompleteItems Variables = new AutocompleteItems();
        public AutocompleteItems Functions = new AutocompleteItems();

        private Timer       MouseTimer     = new Timer(MouseTimer_Task, null, Timeout.Infinite, Timeout.Infinite);
        public  MarkerStyle SameWordsStyle = new MarkerStyle(new SolidBrush(Color.FromArgb(33, Color.Gray)));
        public  Point       MouseLocation;

        public  AutocompleteMenu PopupMenu;
        public static int MaxPopupItems = 18;

        public bool AutoCompleteBrackets     { get { return TB.AutoCompleteBrackets    ; } set { TB.AutoCompleteBrackets    = value; } }
        public bool AutoIdent                { get { return TB.AutoIndent              ; } set { TB.AutoIndent              = value; } }
        public bool AutoIndentChars          { get { return TB.AutoIndentChars         ; } set { TB.AutoIndentChars         = value; } }
        public bool AutoIndentExistingLines  { get { return TB.AutoIndentExistingLines ; } set { TB.AutoIndentExistingLines = value; } }
        public bool ToolStripVisible         { get { return tsMain.Visible                 ; } set { tsMain.Visible                 = value; } }
        public bool DebugMode                { get { return TB.DebugMode               ; } set { TB.DebugMode               = value; } }
        public bool EnableFunctionToolTip = true;
        public bool EnableEvaluateByMouse = true;
        private bool GetScriptDescrition  = false;

        public string ScriptLanguage {
            get {
                if (TB.Language == Language.CPPScript) return "C++Script";
                if (TB.Language == Language.YAML     ) return "Нет скрипта";
                return TB.Language.ToString();
            }
            set {
                if (ScriptLanguage == value) return;
                TB.ClearStylesBuffer();
                TB.Range.ClearStyle(StyleIndex.All);
                switch (value) {
                    case "C++Script"   : TB.Language = Language.CPPScript   ; break;
                    case "PascalScript": TB.Language = Language.PascalScript; break;
                    case "BasicScript" : TB.Language = Language.BasicScript ; break;
                    case "JScript"     : TB.Language = Language.JScript     ; break;
                    default            : TB.Language = Language.YAML        ; break;
                }
                CreateAutocomplete();
                CreateInsertTemplateItems();
                TB.OnSyntaxHighlight(new TextChangedEventArgs(TB.Range));
            }
        }

        public override string Text { get { return TB.Text; } set { TB.Text = value; } }

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

        public int TextLength => Text.Length;

        #region Fuctions and procedures
        public void OnRunningStateChange(bool running) {
            DebugMode    = running;
            RunLineRised = false;
            if (!running) {
                TB.HmsDebugChar = -1;
                TB.HmsDebugLine = -1;
            }
        }

        private void HideAllToolTipsAndHints() {
            HideToolTip4Function(true);
            TB.   ToolTip?.RemoveAll();
            PopupMenu.ToolTip?.RemoveAll();
            PopupMenu.Close();
            //ValueForm.Hide();
        }

        private void HideToolTip4Function(bool noCheckLine = false) {
            if (IsDisposed) return;
            if (TB != null && !TB.IsDisposed && TB.ToolTip4Function.Visible) {
                if (noCheckLine || TB.Selection.Start.iLine != TB.ToolTip4Function.iLine) {
                    TB.ToolTip4Function.Hide(TB);
                }
            }
        }

        public void GetCaretPos(out int aLine, out int aChar) {
            aLine = TB.Selection.Start.iLine + 1;
            aChar = TB.Selection.Start.iChar + 1;
        }

        public void SetCaretPos(int iLine, int iChar) {
            if (iLine > 0) iLine -= 1;
            if (iChar > 0) iChar -= 1;
            TB.Selection.Start = new Place(iChar, iLine);
            RunLineRised = false;
            if (DebugMode) CheckDebugState(); // 4 getting debug line

            int iFirstLine = TB.YtoLineIndex() + 2;
            int iLastLine  = TB.YtoLineIndex(TB.VerticalScroll.Value + TB.Height) + iFirstLine-4;
            if ((iLine < iFirstLine) || (iLine > iLastLine)) TB.DoCaretVisible();
            TB.Focus();
        }

        public int GetCurrentLine() {
            return TB.Selection.Start.iLine + 1;
        }

        public void HighlightInvisibleChars(bool flag) {
            TB.Range.ClearStyle(TB.InvisibleCharsStyle);
            if (flag) TB.Range.SetStyle(TB.InvisibleCharsStyle, @".$|.\r\n|\s");
            TB.Invalidate();
        }

        public void ShowLineNumbers(bool flag) {
            TB.ShowLineNumbers = flag;
        }

        public void ShowFoldingLines(bool flag) {
            TB.ShowFoldingLines = flag;
            TB.NeedRecalc(true);
            TB.Refresh();
        }

        public void Undo() {
            if (TB.UndoEnabled) TB.Undo();
        }

        public void Redo() {
            if (TB.RedoEnabled) TB.Redo();
        }

        public bool NavigateBackward() {
            return TB.NavigateBackward();
        }

        public bool NavigateForward() {
            return TB.NavigateForward();
        }

        public void FindKeyPressed(KeyPressEventArgs e, string text) {
            if (e.KeyChar == '\r')
                FindText(text);
        }

        private void tbFind_Leave(object sender, EventArgs e) {
            TB.YellowSelection = false;
        }

        public void FindText(string text, bool forward = true) {
            TB.YellowSelection    = true;
            TB.SelectionAfterFind = true;
            Range r = TB.Selection.Clone();
            if (forward) {
                r.End   = new Place(TB[TB.LinesCount - 1].Count, TB.LinesCount - 1);
            } else {
                r.Start = new Place(0, 0);
                r.End   = new Place(TB.Selection.End.iChar, TB.Selection.End.iLine);
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
                TB.Selection = foundRange;
                TB.DoSelectionVisible();
            } else {
                MessageBox.Show(@"""" + text + @"""" + @" не найдено.", Title, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                TB.SelectionStart = 0;
            }

        }

        public void OffBreakpointInHms(int iLine = -1) {
            if (HmsScriptFrame != null) {
                try {
                    int isBreakpoint = 0;
                    HmsScriptFrame.IsBreakpointLine(iLine + 1, ref isBreakpoint);
                    bool lineIsBreakpointed = (isBreakpoint < 0);
                    if (lineIsBreakpointed)
                        HmsScriptFrame.ToggleBreakpoint(iLine + 1);
                } catch (Exception e) {
                    HMS.LogError(e.ToString());
                }
            }
        }

        public void SetBreakpointInHms(int iLine = -1) {
            if (HmsScriptFrame != null) {
                try {
                    int isBreakpoint = 0;
                    HmsScriptFrame.IsBreakpointLine(iLine + 1, ref isBreakpoint);
                    bool lineIsBreakpointed = (isBreakpoint < 0);
                    if (!lineIsBreakpointed)
                        HmsScriptFrame.ToggleBreakpoint(iLine + 1);
                } catch (Exception e) {
                    HMS.LogError(e.ToString());
                }
            }
        }

        public void ToggleBreakpoint(int iLine = -1) {
            if (iLine == -1) iLine = TB.Selection.Start.iLine;
            int isBreakpoint = 0;
            bool lineIsBreakpointed = TB.Breakpoints.Contains(iLine);
            try {
                if (!lineIsBreakpointed) {
                    string line = TB.WithoutStringAndComments(TB.Lines[iLine]).Trim();
                    if (line.Length == 0) return;
                }

                if (HmsScriptFrame != null) {
                    HmsScriptFrame.IsBreakpointLine(iLine + 1, ref isBreakpoint);
                    lineIsBreakpointed = (isBreakpoint < 0);
                }
                if (lineIsBreakpointed) {
                    TB.UnbreakpointLine(iLine);
                } else {
                    string name = CodeAnalysis.RegexPartOfLine.Match(TB.Lines[iLine]).Value;
                    TB.BreakpointLine(iLine, "Точка останова " + (TB.Breakpoints.counter + 1) + " " + name + "...");
                }
                HmsScriptFrame?.ToggleBreakpoint(iLine + 1);
            } catch (Exception e) {
                HMS.LogError(e.ToString());
            }
        }

        public void Bookmark(int iLine = -1) {
            if (iLine == -1) iLine = TB.Selection.Start.iLine;
            if (TB.Bookmarks.Contains(iLine)) {
                TB.UnbookmarkLine(iLine);
            } else {
                string name = CodeAnalysis.RegexPartOfLine.Match(TB.Lines[iLine]).Value;
                TB.BookmarkLine(iLine, "Закладка " + (TB.Bookmarks.counter+1) + " " + name + "...");
            }
        }

        public void BookmarkClear() {
            TB.Bookmarks.Clear();
            TB.Invalidate();
        }

        public void BookmarkPrevious() {
            TB.GotoPrevBookmark(TB.Selection.Start.iLine);
        }

        public void BookmarkNext() {
            TB.GotoNextBookmark(TB.Selection.Start.iLine);
        }

        public void HotKeysDialog() {
            var form = new HotkeysEditorForm(TB.HotkeysMapping);
            if (form.ShowDialog() == DialogResult.OK)
                TB.HotkeysMapping = form.GetHotkeys();
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
            switch (TB.Language) {
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
            fileFialog.Title            = @"Выбор файла скрипта";
            if (fileFialog.ShowDialog() == DialogResult.OK) {
                Filename = fileFialog.FileName;
                if (File.Exists(Filename)) TB.OpenFile(Filename);
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
            fileFialog.Title            = @"Выбор файла скрипта";
            if (fileFialog.ShowDialog() == DialogResult.OK) {
                Filename = fileFialog.FileName;
                try {
                    if (File.Exists(Filename)) File.Delete(Filename);
                    File.WriteAllText(Filename, TB.Text, Encoding.UTF8);
                    Modified = false;
                } catch (Exception e) {
                    HMS.LogError(e.ToString());
                }
            }

        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public void Print() {
            var settings = new PrintDialogSettings {
                Title  = Filename,
                Header = "&b&w&b",
                Footer = "&b&p",
                ShowPrintPreviewDialog = true
            };
            TB.Print(settings);
        }

        public void ClearUndo() {
            TB.ClearUndo();
        }

        public void RestorePosition() {
            if (btnStorePositions.Checked) {
                try {
                    string hash   = ((uint)Text.GetHashCode()).ToString();
                    string hashes = Settings.Get("LastHash" + HmsScriptMode.ToString(), SettingsSection, "");
                    hashes = Regex.Match(hashes, hash + ":[^\\|]+").Value;
                    if (hashes == "") return;
                    var m = Regex.Match(hashes, hash + ":(\\d+):(\\d+)");
                    if (m.Success) {
                        uint sel, pos;
                        uint.TryParse(m.Groups[1].Value, out sel);
                        if (sel < TB.Text.Length) TB.SelectionStart = (int)sel;
                        uint.TryParse(m.Groups[2].Value, out pos);
                        if (pos <= TB.GetMaximumScrollValue()) TB.SetVerticalScrollValue((int)pos);
                    }
                    m = Regex.Match(hashes, hash + ":.*?:.*?:(.*?):(.*)");
                    if (m.Success) {
                        string[] sbp = m.Groups[1].Value.Split(',');
                        string[] sbm = m.Groups[2].Value.Split(',');
                        TB.Breakpoints.Clear();
                        foreach (string s in sbp) {
                            m = Regex.Match(s, "\\d+");
                            if (m.Success) ToggleBreakpoint(int.Parse(m.Value));
                        }
                        TB.Bookmarks.Clear();
                        foreach (string s in sbm) {
                            m = Regex.Match(s, "(\\d+)\\.(\\d+)\"(.*?)\"");
                            if (!m.Success) continue;
                            Bookmark b = new Bookmark(TB, "", 0) {
                                LineIndex = int.Parse(m.Groups[1].Value),
                                CharIndex = int.Parse(m.Groups[2].Value),
                                Name      = m.Groups[3].Value
                            };
                            TB.Bookmarks.Add(b);
                        }
                    }
                    TB.Invalidate();
                } catch (Exception e) {
                    HMS.LogError(e.ToString());
                    //Console.WriteLine("Error restoring position");
                }
            }
        }

        /// <summary>
        /// Apply settings from .ini file to the this objects
        /// </summary>
        public void LoadSettings() {
            try { Settings.Load(); } catch (Exception e) { HMS.LogError(e.ToString()); Console.WriteLine(@"Error loading config file '" + Settings.File + @"'", e); return; }
            OldTextHash = (uint)Text.GetHashCode();

            tsMain.Visible = false;
            string section = SettingsSection;
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
            btnUnderlinePascalKeywrd.Checked = Settings.Get("UnderlinePascalKeywords", section, btnUnderlinePascalKeywrd.Checked);
            btnKeywordsToLowcase    .Checked = Settings.Get("KeywordsToLowcase"      , section, btnKeywordsToLowcase    .Checked);
            btnGetScriptDescriptions.Checked = Settings.Get("GetScriptDescriptions"  , section, btnGetScriptDescriptions.Checked);
            btnInvisiblesInSelection.Checked = Settings.Get("InvisiblesInSelection"  , section, btnInvisiblesInSelection.Checked);
            btnSelectionBorder      .Checked = Settings.Get("SelectionWithBorders"   , section, btnSelectionBorder      .Checked);
            btnShowBeginOfFunctions .Checked = Settings.Get("ShowBeginOfFunctions"   , section, btnShowBeginOfFunctions .Checked);
            // Set to false deprecated settings
            btnCheckKeywordsRegister.Checked = false;

            TB.ShowBeginOfFunctions                         = btnShowBeginOfFunctions .Checked;
            TB.SyntaxHighlighter.AltPascalKeywordsHighlight = btnUnderlinePascalKeywrd.Checked;
            TB.SyntaxHighlighter.RedStringsHighlight        = btnRedStringsHighlight  .Checked;
             
            TB.Font = new Font("Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            if ((TB.Font.Name.ToLower().IndexOf("consolas", StringComparison.Ordinal) < 0) && (HMS.PFC.Families.Length > 1)) {
                btnToolStripMenuItemFONT.Visible = false;
                TB.Font = new Font(HMS.PFC.Families[1], 10f, FontStyle.Regular, GraphicsUnit.Point);
            } else if (btnToolStripMenuItemFONT.Checked) { 
                btnToolStripMenuItemFONT_Click(null, new EventArgs());
            }

            int val;
            if (int.TryParse(Settings.Get("RoundedCornersRadius", section, FastColoredTextBox.RoundedCornersRadius.ToString()), out val))
                FastColoredTextBox.RoundedCornersRadius = val;
            if (int.TryParse(Settings.Get("CacheMinLines", section, FastColoredTextBox.minCacheLines.ToString()), out val))
                FastColoredTextBox.minCacheLines = val;
            if (int.TryParse(Settings.Get("CacheMaxFiles", section, FastColoredTextBox.maxCacheFiles.ToString()), out val))
                FastColoredTextBox.maxCacheFiles = val;
            if (int.TryParse(Settings.Get("CacheMaxSize" , section, FastColoredTextBox.maxCacheSize .ToString()), out val))
                FastColoredTextBox.maxCacheSize  = val;

            var sVal = Settings.Get("Zoom", section, "100"); 
            TB.Zoom = Int32.Parse(sVal);

            PopupMenu.OnlyCtrlSpace    = btnHints4CtrlSpace      .Checked;
            PopupMenu.Enabled          = btnSetIntelliSense      .Checked;
            TB.AutoCompleteBrackets    = btnAutoCompleteBrackets .Checked;
            TB.AutoIndent              = btnAutoIdent            .Checked;
            TB.AutoIndentExistingLines = btnAutoIdentLines       .Checked;
            EnableFunctionToolTip      = btnIntelliSenseFunctions.Checked;
            EnableEvaluateByMouse      = btnEvaluateByMouse      .Checked;
            TB.EnableFoldingIndicator  = btnShowFoldingIndicator .Checked;
            TB.HighlightCurrentLine    = btnHighlightCurrentLine .Checked;

            Filename  = Settings.Get("LastFile", section, Filename );
            ThemeName = Settings.Get("Theme"   , section, ThemeName);
            int countOfBuildinThemes = Themes.Init();
            FillThemes(countOfBuildinThemes);
            Themes.SetTheme(this, ThemeName, btnThemes.DropDownItems);
            HMS.LoadTemplates(); // Сначала загружаем шаблоны, какие есть

            // Need before the set ScriptLanguage!
            btnGetScriptDescriptions_Click(null, EventArgs.Empty);
            if (GetScriptDescrition) {
                CreateAutocompleteItemsByScriptDescrition();
            }

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
            btnKeywordsToLowcase_Click    (null, EventArgs.Empty);
            btnInvisiblesInSelection_Click(null, EventArgs.Empty);
            btnSelectionBorder_Click      (null, EventArgs.Empty);

            TB.HotkeysMapping.InitDefault(); 
            string hotkeys = Settings.Get("Map", "AddonHotkeys", "");
            if (hotkeys.Length > 0) {
                HotkeysMapping ourMap = HotkeysMapping.Parse(hotkeys);
                foreach(var pair in ourMap)
                    TB.HotkeysMapping[pair.Key] = pair.Value;
            }
            TB.Refresh();
        }

        private void FillThemes(int buildinThemes) {
            btnThemes.DropDownItems.Clear();
            foreach (var name in Themes.Dict.Keys) {
                if (btnThemes.DropDownItems.Count == buildinThemes) {
                    btnThemes.DropDownItems.Add(new ToolStripSeparator());
                }
                ToolStripMenuItem item = (ToolStripMenuItem)btnThemes.DropDownItems.Add(name);
                item.Tag = name;
                item.Click += (o, a) => {
                    ThemeName = (string)item.Tag;
                    Themes.SetTheme(this, ThemeName, btnThemes.DropDownItems);
                    TB.ClearCache();
                };
                if (name == ThemeName) {
                    item.Checked = true;
                    Themes.SetTheme(this, name, btnThemes.DropDownItems);
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
                Settings.Set("FormatCodeWhenPaste"  ,btnFormatCodeWhenPaste  .Checked, section);
                Settings.Set("KeywordsToLowcase"    ,btnKeywordsToLowcase    .Checked, section);
                Settings.Set("GetScriptDescriptions",btnGetScriptDescriptions.Checked, section);
                Settings.Set("InvisiblesInSelection",btnInvisiblesInSelection.Checked, section);
                Settings.Set("SelectionWithBorders" , btnSelectionBorder     .Checked, section);
                Settings.Set("ShowBeginOfFunctions" , btnShowBeginOfFunctions.Checked, section);

                Settings.Set("Theme"               , ThemeName                       , section);
                Settings.Set("LastFile"            , Filename                        , section);
                Settings.Set("Language"            , ScriptLanguage                  , section);
                Settings.Set("Zoom"                , TB.Zoom                     , section);

                Settings.Set("UnderlinePascalKeywords", btnUnderlinePascalKeywrd.Checked, section);

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
            foreach (var b in TB.Breakpoints) { if (linesBPs != "") linesBPs += ","; linesBPs += b.LineIndex; }
            foreach (var b in TB.Bookmarks  ) { if (linesBMs != "") linesBMs += ","; linesBMs += b.LineIndex + "." + b.CharIndex + "\"" + b.Name.Replace("|", "") + "\""; }

            hashesDict[lastHash] = ""+TB.SelectionStart+":"+TB.GetVerticalScrollValue()+":"+linesBPs+":"+linesBMs;

            hashes = ""; foreach (var keyVal in hashesDict) hashes += "|" + keyVal.Key + ":" + keyVal.Value;
            if (hashes.Length > 0) hashes = hashes.Substring(1);
            Settings.Set(hashParam, hashes, SettingsSection);
        }

        private string GetHotKeysMapping() {
            HotkeysMapping ourMap = new HotkeysMapping();
            HotkeysMapping defMap = new HotkeysMapping();
            ourMap.Clear();
            defMap.InitDefault();
            foreach(var m in TB.HotkeysMapping) {
                if (defMap.ContainsKey(m.Key)) continue;
                ourMap.Add(m.Key, m.Value);
            }
            return ourMap.ToString();
        }


        public bool LoadFile(string filename) {
            bool success = false;
            if (!String.IsNullOrEmpty(filename) && File.Exists(filename)) {
                TB.Clear();
                TB.Text = File.ReadAllText(filename, Encoding.UTF8);
                TB.ClearUndo();
                Modified = false;
                success = true;
            }
            return success;
        }

        private void RunScript() {
            try {
                TB.HmsDebugLine = -1;
                HmsScriptFrame?.ProcessCommand(Constatns.ecRunScript);
            } catch (Exception e) {
                HMS.LogError(e.ToString());
            }
        }

        bool RunLineRised;
        private void RunLine() {
            if (HmsScriptFrame != null) {
                if (RunLineRised) return;
                try {
                    RunLineRised = true; // resets in SetCaretPos() and OnRunningStateChange()
                    string line = TB.Lines[TB.Selection.Start.iLine];
                    HmsScriptFrame.ProcessCommand(Constatns.ecRunLine);
                    if (ValueForm.Visible) {
                        if (line.IndexOf(ValueForm.RealExpression, StringComparison.Ordinal) >= 0) ValueForm.Value = EvalVariableValue(ValueForm.Expression);
                    } else if (ValueHint.IsShowing) {
                        if (line.IndexOf(ValueHint.RealExpression, StringComparison.Ordinal) >= 0) ValueHint.Value = EvalVariableValue(ValueHint.Expression);
                    }
                } catch (Exception e) {
                    HMS.LogError(e.ToString());
                }
            }
        }

        private void EvaluateDialog() {
            try {
                HmsScriptFrame?.ProcessCommand(Constatns.ecEvaluate);
            } catch (Exception e) {
                HMS.LogError(e.ToString());
            }
        }

        private bool IsInRanges(List<Range> ranges, Place place) {
            foreach (var r in ranges) {
                int p1 = TB.PlaceToPosition(place);
                int ps = TB.PlaceToPosition(r.Start);
                int pe = TB.PlaceToPosition(r.End);
                if ((p1 >= ps) && (p1 <= pe)) return true;
            }
            return false;
        }

        private string GetScriptDescriptions() {
            object objXml = "";
            try {
                HmsScriptFrame?.GenerateScriptDescriptions(ref objXml);
            } catch (Exception e) {
                HMS.LogError(e.ToString());
            }
            return objXml.ToString();
        }

        private void RenameVariable() {
            if (TB.LinesCount < 1) return;
            string name = CodeAnalysis.CurrentVariableName(TB);
            if (name == "") return;

            FormRename form = new FormRename {OldVarName = name};

            form.TextBox.BackColor = TB.BackColor;
            form.TextBox.ForeColor = TB.ForeColor;
            form.TextBox.Language  = TB.Language;
            form.TextBox.Font      = TB.Font;
            form.TextBox.ShowFoldingLines   = false;
            form.TextBox.ShowFoldingMarkers = false;
            form.TextBox.SelectionWithBorders = TB.SelectionWithBorders;

            Themes.SetTheme(form.TextBox, ThemeName);
            form.TextBox.DrawLineNumberFromInfo = true;

            form.OrigRanges.Clear();
            form.TextBox   .Clear();

            string text = TB.WithoutStringAndComments(Text, true);

            NeedRecalcVars = true;
            CodeAnalysis.UpdateCurrentVisibleVariablesAndWait(this);
            HMSItem itemCurrentFunction = CodeAnalysis.GetCurrentProcedure(Functions, TB.SelectionStart);

            List<Range> ExcludeRanges = new List<Range>();

            // 1. Весь контекст со всеми процедурами
            // 2. Глобальный контекст + только те процедуры, где такое имя не определялось
            // 3. Только в текущей процедуре (определена в текущей процедуре)

            if (itemCurrentFunction!=null && itemCurrentFunction.Type != "MainProcedure" && LocalVars.ContainsName(name)) {
                Place start = TB.PositionToPlace(itemCurrentFunction.PositionStart);
                Place end   = TB.PositionToPlace(itemCurrentFunction.PositionEnd  );
                ExcludeRanges.Add(new Range(TB, TB.Range.Start, start));
                ExcludeRanges.Add(new Range(TB, end, TB.Range.End));
                form.Context = "Контекст: " + (itemCurrentFunction.Kind == DefKind.Function ? "функция" : "процедура") + " " + itemCurrentFunction.MenuText;
                form.LocalFunction = itemCurrentFunction.MenuText;
            } else {
                string firstFuncname = "", exclFuncNames="";
                foreach(var itemFunc in Functions) {
                    if (itemFunc.Type == "MainProcedure") continue;
                    CodeAnalysis.UpdateCurrentVisibleVariablesAndWait(this, itemFunc.PositionStart + itemFunc.MenuText.Length + 2);
                    if (LocalVars.ContainsName(name)) {
                        Place start = TB.PositionToPlace(itemFunc.PositionStart);
                        Place end   = TB.PositionToPlace(itemFunc.PositionEnd  );
                        ExcludeRanges.Add(new Range(TB, start, end));
                        if (firstFuncname == "")
                            firstFuncname = itemFunc.MenuText;
                        if (exclFuncNames != "") exclFuncNames += ",\n";
                        exclFuncNames += itemFunc.MenuText;
                    }
                }
                CodeAnalysis.UpdateCurrentVisibleVariablesAndWait(this);
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
            int iLine = 0; string[] lines = text.Split('\n');
            int addedLine = 0;
            foreach (string lineString in lines) {
                MatchCollection mc = regexSearchVarName.Matches(lineString);
                if (mc.Count > 0) {
                    bool wasadded = false;
                    foreach (Match m in mc) {
                        var iCharB = m.Index;
                        var iCharE = iCharB + name.Length;
                        if (IsInRanges(ExcludeRanges, new Place(iCharB, iLine))) continue;
                        var r = new Range(TB, iCharB, iLine, iCharE, iLine) { StoredLineNo = addedLine };
                        form.OrigRanges.Add(r);
                        wasadded = true;
                    }
                    if (wasadded) {
                        if (addedLine > 0) form.TextLines += "\n";
                        form.TextLines += TB.Lines[iLine];
                        addedLine++;
                    }
                }
                iLine++;
            }

            DialogResult result = form.ShowDialog();
            if (result == DialogResult.OK) {
                NeedRecalcVars = true;
                TB.FastReplaceRanges(form.NewVarName, form.OrigRanges);
                //Editor_TextChangedDelayed(null, null);
            } else if (result == DialogResult.Retry) {
                TB.Navigate(form.LineNo4Goto, form.CharNo4Goto);
                TB.Focus();
            }
        }
        #endregion Function and procedures

        #region Control Events

        private void Editor_KeyDown(object sender, KeyEventArgs e) {
            if      (e.KeyCode == Keys.F11   ) tsMain.Visible = !tsMain.Visible;
            else if (e.KeyCode == Keys.F12   ) GotoDefinition();
            else if (e.KeyCode == Keys.F2    ) RenameVariable();
            else if (e.KeyCode == Keys.Escape) {
                TB.findForm?.Hide();
                TB.replaceForm?.Hide();
                HideAllToolTipsAndHints();
                PopupMenu.TempNotShow = true;
            }
            else if (e.Alt) {
                if      (e.KeyCode == Keys.D1) TB.SetBookmarkByName(TB.Selection.Start.iLine, "1");
                else if (e.KeyCode == Keys.D2) TB.SetBookmarkByName(TB.Selection.Start.iLine, "2");
                else if (e.KeyCode == Keys.D3) TB.SetBookmarkByName(TB.Selection.Start.iLine, "3");
                else if (e.KeyCode == Keys.D4) TB.SetBookmarkByName(TB.Selection.Start.iLine, "4");
                else if (e.KeyCode == Keys.D5) TB.SetBookmarkByName(TB.Selection.Start.iLine, "5");
                else if (e.KeyCode == Keys.D6) TB.SetBookmarkByName(TB.Selection.Start.iLine, "6");
                else if (e.KeyCode == Keys.D7) TB.SetBookmarkByName(TB.Selection.Start.iLine, "7");
                else if (e.KeyCode == Keys.D8) TB.SetBookmarkByName(TB.Selection.Start.iLine, "8");
                else if (e.KeyCode == Keys.D9) TB.SetBookmarkByName(TB.Selection.Start.iLine, "9");
                else if (e.KeyCode == Keys.Space) {

                }
            } else if (e.Control) {
                if      (e.KeyCode == Keys.D1) TB.GotoBookmarkByName("1");
                else if (e.KeyCode == Keys.D2) TB.GotoBookmarkByName("2");
                else if (e.KeyCode == Keys.D3) TB.GotoBookmarkByName("3");
                else if (e.KeyCode == Keys.D4) TB.GotoBookmarkByName("4");
                else if (e.KeyCode == Keys.D5) TB.GotoBookmarkByName("5");
                else if (e.KeyCode == Keys.D6) TB.GotoBookmarkByName("6");
                else if (e.KeyCode == Keys.D7) TB.GotoBookmarkByName("7");
                else if (e.KeyCode == Keys.D8) TB.GotoBookmarkByName("8");
                else if (e.KeyCode == Keys.D9) TB.GotoBookmarkByName("9");
            } else if (e.KeyCode == Keys.Oemcomma || (e.Shift && e.KeyCode == Keys.D9)) {
                if (!TB.Selection.IsStringOrComment) WasCommaOrBracket = true;
            }

            if      (e.KeyCode == Keys.F5) ToggleBreakpoint();
            else if (e.KeyCode == Keys.F7) EvaluateDialog();
            else if (e.KeyCode == Keys.F8) RunLine();
            else if (e.KeyCode == Keys.F9) RunScript();

        }

        private void Editor_SelectionChanged(object sender, EventArgs e) {
            if (prevWord != "") {
                prevWord = "";
                TB.Range.ClearStyle(SameWordsStyle);
            }
            if (TB.SelectionAfterFind) {
                TB.SelectionAfterFind = false;
                return;
            }
            TB.YellowSelection = false;
            if (EnableFunctionToolTip && WasCommaOrBracket || TB.ToolTip4Function.Visible)
                if (!CheckPositionIsInParametersSequenceWorker.IsBusy)
                    CheckPositionIsInParametersSequenceWorker.RunWorkerAsync();
        }

        private void Editor_SelectionChangedDelayed(object sender, EventArgs e) {
            if (btnHighlightSameWords.Checked) HighlightSameWords();
            if (btnSetIntelliSense   .Checked) CodeAnalysis.UpdateCurrentVisibleVariables(this);
        }

        private void Editor_TextChangedDelayed(object sender, TextChangedEventArgs e) {
            CodeAnalysis.BuildFunctionListAsync(this); // Only when text changed - build the list of functions
            if (btnAutoCheckSyntax.Checked) AutoCheckSyntaxBackground();

            if (IsFirstActivate) {
                IsFirstActivate = false;
                TB.Focus();
            }
        }

        private void Editor_TextChanged(object sender, TextChangedEventArgs e) {
            NeedRecalcVars = true;
        }

        private void btnAutoCheckSyntax_Click(object sender, EventArgs e) {
            if (btnAutoCheckSyntax.Checked) {
				AutoCheckSyntaxBackground();
			} else {
                TB.ClearErrorLines();
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
            TB.Cut();
        }

        private void btnCopy_Click(object sender, EventArgs e) {
            TB.Copy();
        }

        private void btnPaste_Click(object sender, EventArgs e) {
            TB.Paste();
        }

        private void btnInvisibleChars_Click(object sender, EventArgs e) {
            HighlightInvisibleChars(btnInvisibleChars.Checked);
        }

        private void btnHighlightCurrentLine_Click(object sender, EventArgs e) {
            TB.HighlightCurrentLine = btnHighlightCurrentLine.Checked;
            TB.Invalidate();
        }

        private void btnShowLineNumbers_Click(object sender, EventArgs e) {
            ShowLineNumbers(btnShowLineNumbers.Checked);
        }

        private void btnShowFoldingLines_Click(object sender, EventArgs e) {
            ShowFoldingLines(btnShowFoldingLines.Checked);
        }

        private void btnEnableFoldingIndicator_Click(object sender, EventArgs e) {
            TB.EnableFoldingIndicator = btnShowFoldingIndicator.Checked;
            TB.Invalidate();
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
            //ValueForm.Show(Editor, "", Editor.Text, "");
            Bookmark();
        }

        private void btnBookmarkMinus_Click(object sender, EventArgs e) {
            if (TB.Bookmarks.Count < 1) return;

            string txt = "";
            foreach (var b in TB.Bookmarks) txt += b.Name + "\n";

            if (MessageBox.Show(Resources.HMSEditor_btnBookmarkMinus_Click_+ txt, Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question)== DialogResult.Yes)
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
            foreach (var bookmark in TB.Bookmarks) {
                ToolStripItem item = items.Add(bookmark.Name, imageList1.Images[9]);
                item.Tag = bookmark;
                item.Click += (o, a) => {
                    var toolStripItem = o as ToolStripItem;
                    if (toolStripItem == null) return;
                    var b = (Bookmark)toolStripItem.Tag;
                    b.DoVisible();
                };
            }
            // --------------------------------------------------------
            foreach (HMSItem item in Functions) {
                ToolStripItem tipItem = items.Add(item.MenuText, imageList1.Images[item.ImageIndex]);
                tipItem.Tag = item.PositionStart;
                tipItem.Click += (o, a) => {
                    try {
                        var toolStripItem = o as ToolStripItem;
                        if (toolStripItem != null) TB.SelectionStart = (int)toolStripItem.Tag;
                        TB.DoRangeVisible(TB.Selection, true);
                        TB.Invalidate();
                    }
                    catch {
                        // ignored
                    }
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
                TB.Range.ClearStyle(SameWordsStyle);
            else {
                prevWord = "";
                HighlightSameWords();
            }
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
            TB.AutoCompleteBrackets = btnAutoCompleteBrackets.Checked;
        }

        private void btnHints4CtrlSpace_Click(object sender, EventArgs e) {
            PopupMenu.OnlyCtrlSpace = btnHints4CtrlSpace.Checked;
        }

        private void btnAutoIdent_Click(object sender, EventArgs e) {
            TB.AutoIndent = btnAutoIdent.Checked;
            btnAutoIdentLines.Enabled = TB.AutoIndent;
            btnAutoIdentLines.ToolTipText = btnAutoIdentLines.Enabled ? "" : Resources.HMSEditor_btnAutoIdent_Click_Доступно_только_при_включенном_автоматическом_отступе;
        }

        private void btnAutoIdentChars_Click(object sender, EventArgs e) {
            TB.AutoIndentExistingLines = btnAutoIdentLines.Checked;
        }

        public void btnMarkChangedLines_Click(object sender, EventArgs e) {
            TB.HighlightChangedLine = btnMarkChangedLines.Checked;
        }

        private void ToolStripMenuItemCut_Click(object sender, EventArgs e) {
            TB.Cut();
        }

        private void ToolStripMenuItemCopy_Click(object sender, EventArgs e) {
            TB.Copy();
        }

        private void ToolStripMenuItemPaste_Click(object sender, EventArgs e) {
            TB.Paste();
        }

        private void ToolStripMenuItemDelete_Click(object sender, EventArgs e) {
            TB.Delete();
        }

        private void btnContextMenuCommentBlock_Click(object sender, EventArgs e) {
            TB.CommentSelected();
        }

        private void ToolStripMenuItemBookmarkClear_Click(object sender, EventArgs e) {
            BookmarkClear();
        }

        private void ToolStripMenuItemClearBreakpoints_Click(object sender, EventArgs e) {
            List<int> lines = new List<int>();
            foreach (Bookmark b in TB.Breakpoints) lines.Add(b.LineIndex);
            foreach (int iLine in lines) ToggleBreakpoint(iLine);
        }

        private void ToolStripMenuItemSelectAll_Click(object sender, EventArgs e) {
            TB.SelectAll();
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
            TB.CheckKeywordsRegister = btnCheckKeywordsRegister.Checked;
        }

        private void btnCheckNewVersionOnLoad_Click(object sender, EventArgs e) {
            if (!HMS.NewVersionChecked && btnCheckNewVersionOnLoad.Checked) {
                BackgroundWorker w = new BackgroundWorker();
                w.DoWork             += W_DoWork;
                w.RunWorkerCompleted += W_RunWorkerCompleted;
                if (!w.IsBusy)
                    w.RunWorkerAsync();
            }
        }

        private void W_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            if (e.Error != null) { HMS.LogError(e.Error.ToString()); return; }
            if (e.Cancelled) return;
            labelNewVersion.Visible = HMS.NewVersionExist;
        }

        private void W_DoWork(object sender, DoWorkEventArgs e) {
            HMS.CheckNewVersion();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e) {
            ToolStripMenuItemZoom100         .Enabled = (TB.Zoom != 100);
            ToolStripMenuItemUndo            .Enabled = TB.UndoEnabled;
            ToolStripMenuItemRedo            .Enabled = TB.RedoEnabled;
            ToolStripMenuItemBookmarkClear   .Enabled = (TB.Bookmarks.Count > 0);
            ToolStripMenuItemClearBreakpoints.Enabled = (TB.Breakpoints.Count > 0);
            btnContextMenuBack   .Enabled = TB.NavigateBackward(true);
            btnContextMenuForward.Enabled = TB.NavigateForward (true);
            FillGoToItems(btnGotoContextMenu.DropDownItems);
            btnContextMenuToolBar.Checked = tsMain.Visible;
            btnContextMenuAutoIndent.Enabled = (TB.SelectionLength > 0);
            btnAdd2Watch.Visible = DebugMode && (TB.SelectionLength > 0);
            btnHelpPanelContextMenu.Checked = btnSprav.Checked;
        }

        private void btnContextMenuAutoIndent_Click(object sender, EventArgs e) {
            TB.DoAutoIndent();
        }

        private void ToolStripMenuItemZoom100_Click(object sender, EventArgs e) {
            TB.Zoom = 100;
        }

        private void ToolStripMenuItemUndo_Click(object sender, EventArgs e) {
            Undo();
        }

        private void ToolStripMenuItemRedo_Click(object sender, EventArgs e) {
            Redo();
        }

        private void ToolStripMenuItemAltPascalScriptHighlight_Click(object sender, EventArgs e) {
            TB.SyntaxHighlighter.AltPascalKeywordsHighlight = btnUnderlinePascalKeywrd.Checked;
            TB.RefreshTheme();
        }

        private void btnRedStringsHighlight_Click(object sender, EventArgs e) {
            TB.SyntaxHighlighter.RedStringsHighlight = btnRedStringsHighlight.Checked;
            TB.RefreshTheme();
        }

        private void Editor_Scroll(object sender, ScrollEventArgs e) {
            HideAllToolTipsAndHints();
        }

        private void btnToolStripMenuItemFONT_Click(object sender, EventArgs e) {
            if (btnToolStripMenuItemFONT.Checked && (HMS.PFC.Families.Length > 1))
                TB.Font = new Font(HMS.PFC.Families[1], 10f, FontStyle.Regular, GraphicsUnit.Point);
            else
                TB.Font = new Font("Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
        }

        private void EditorMouseClick(MouseEventArgs e) {
            if (e.X < (TB.LeftIndent - 4)) { 
                //System.Windows.Forms.MessageBox.Show("VerticalScroll.Value=" + Editor.VerticalScroll.Value.ToString());
                int iStartLine = TB.YtoLineIndex();
                int iLine = iStartLine + (e.Y / TB.CharHeight);
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
            EditorMouseClick(e);
        }

        private void btnVerticalLineText_Click(object sender, EventArgs e) {
            TB.PreferredLineWidth = btnVerticalLineText.Checked ? 80 : 0;
            TB.NeedRecalc();
        }

        private void btnEnableFolding_Click(object sender, EventArgs e) {
            TB.ShowFoldingMarkers = btnEnableFolding.Checked;
            TB.NeedRecalc();
        }

        private void btnAbout_Click(object sender, EventArgs e) {
            AboutDialog aboutDialog = new AboutDialog();
            aboutDialog.ShowDialog();
        }

        private void btnBoldCaret_Click(object sender, EventArgs e) {
            TB.BoldCaret = btnBoldCaret.Checked;
        }

        private void btnStorePositions_Click(object sender, EventArgs e) {

        }

        private void btnAdd2Watch_Click(object sender, EventArgs e) {
            try {
                object text = TB.SelectedText;
                HmsScriptFrame?.AddWatch(ref text);
            } catch (Exception ex) {
                HMS.LogError(ex.ToString());
            }
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
            TB.FormatCodeWhenPaste = btnFormatCodeWhenPaste.Checked;
        }

        private void btnKeywordsToLowcase_Click(object sender, EventArgs e) {
            TB.KeywordsToLowcase = btnKeywordsToLowcase.Checked;
        }

        private void btnGetScriptDescriptions_Click(object sender, EventArgs e) {
            GetScriptDescrition = btnGetScriptDescriptions.Checked;
        }

        private void btnInvisiblesInSelection_Click(object sender, EventArgs e) {
            TB.ShowInvisibleCharsInSelection =  btnInvisiblesInSelection.Checked;
            TB.Invalidate();
        }

        private void btnSelectionBorder_Click(object sender, EventArgs e) {
            TB.SelectionWithBorders = btnSelectionBorder.Checked;
            TB.Invalidate();
        }

        private void btnShowBeginOfFunctions_Click(object sender, EventArgs e) {
            TB.ShowBeginOfFunctions = btnShowBeginOfFunctions.Checked;
            TB.Invalidate();
        }
        #endregion Control Events

        #region Smart IDE functions
        public string EvalVariableValue(string varName) {
            object varname = varName;
            object result  = "";
            try {
                HmsScriptFrame?.SolveExpression(ref varname, ref result);
            } catch (Exception e) {
                HMS.LogError(e.ToString());
            }
            if (result == null) return "";
            return result.ToString();
        }

        public bool CheckDebugState() {
            int running = 0;
            int currentSourceLine = 0;
            int currentSourceChar = 0;
            try {
                HmsScriptFrame?.GetCurrentState(ref running, ref currentSourceLine, ref currentSourceChar);
            } catch (Exception e) {
                HMS.LogError(e.ToString());
            }
            TB.DebugMode    = running < 0;
            TB.HmsDebugLine = currentSourceLine - 1;
            TB.HmsDebugChar = currentSourceChar - 1;
            TB.NeedRecalc();
            return TB.DebugMode;
        }

        private static Regex forbittenSameText = new Regex(@"\W+", RegexOptions.Compiled);
        private string prevWord = "";
        private void HighlightSameWords() {
            if (TB.YellowSelection) return;
            if (TB.Selection.IsEmpty) {
                TB.Range.ClearStyle(SameWordsStyle);
                prevWord = "";
                return;
            }
            var fragment = TB.Selection.GetFragment(@"\w");
            if (fragment.Start != TB.Selection.End || fragment.End != TB.Selection.Start)
                return;
            string text = fragment.Text;
            if (prevWord != text && !forbittenSameText.IsMatch(text)) {
                TB.Range.ClearStyle(SameWordsStyle);
                TB.Range.SetStyleExcludeSection(SameWordsStyle, "\\b" + text + "\\b", RegexOptions.Multiline);
                prevWord = text;
            }
        }

        private void GotoDefinition() {
            string name = CodeAnalysis.CurrentVariableName(TB);
            if      (Variables.ContainsName(name)) GotoPosition(Variables[name].PositionStart);
            else if (LocalVars.ContainsName(name)) GotoPosition(LocalVars[name].PositionStart);
            else if (Functions.ContainsName(name)) GotoPosition(Functions[name].PositionStart);
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
                    item = info.MemberItems.GetItemOrNull(name) ??
                           info.StaticItems.GetItemOrNull(name);
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
                    if (item == null) item = ScriptAutocompleteItems.GetItemOrNull(name); // try additional
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

        private void GotoPosition(int position) {
            TB.SelectionStart = position;
            TB.DoRangeVisible(TB.Selection, true);
            TB.Invalidate();
        }

        private void CheckPositionIsInParametersSequence_DoWork(object sender, DoWorkEventArgs e) {
            //if (Editor.IsUpdating) {
            //    for (int i = 3; i > 0; i--) {
            //        Thread.Sleep(100);
            //        if (!Editor.IsUpdating) break;
            //    }
            //}
            if (!TB.IsUpdating) CheckPositionIsInParametersSequence();
        }

        private void CheckPositionIsInParametersSequence() {
            string text = TB.Selection.GetFunctionLookedLeft();

            HMS.CurrentParamType = "";
            var m = CodeAnalysis.regexFoundOurFunction.Match(text);
            if (m.Success) {
                var name = m.Groups[1].Value;
                var parameters = m.Groups[2].Value;
                Place pp = TB.PositionToPlace(TB.SelectionStart - text.Length + m.Index);
                int iLinesCorrect = TB.Selection.Start.iLine - pp.iLine + 1;
                Point p  = TB.PositionToPoint(TB.SelectionStart - text.Length + m.Index);
                p.Offset(0, TB.CharHeight * iLinesCorrect + 2 );
                TB.ToolTip4Function.iLine = TB.Selection.Start.iLine;
                ShowFunctionToolTip(p, name, parameters);
            } else {
                if (TB.ToolTip4Function.Visible)
                    HideToolTip4Function(true);
            }
            WasCommaOrBracket = false;
        }

        private void ShowFunctionToolTip(Point p, string name, string parameters="") {
            int paramNum = Regex.Split(parameters, "[,;]").Length;
            HMS.CurrentParamType = "";
            HMSItem item = GetHMSItemByText(name);
            if (item != null) {
                if (item.IsFuncOrProcedure || (item.Kind == DefKind.Method)) {
                    if ((TB.SelectionStart >= item.PositionStart) && (TB.SelectionStart <= item.PositionEnd)) return; // we writing this function
                    TB.ToolTip4Function.ShowFunctionParams(item, paramNum, TB, p);
                }
            }
        }

        private void InsertTemplate(string text) {
            text = FormatCodeText(text);
            TB.InsertText(text);
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
            int iLine = TB.Selection.Start.iLine;
            int needIndent = TB.GetRealLine(iLine).AutoIndentSpacesNeededCount;
            for (int i = iLine; i >= 0; i--) {
                string line = TB.Lines[i];
                needIndent = line.Length - line.TrimStart().Length;
                if (line.Trim().Length > 0) break;
            }
            bool firstLine = true;
            Range fragmentLine = new Range(TB, new Place(0, TB.Selection.Start.iLine), TB.Selection.Start);
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

        public void CreateAutocomplete() {
            if (PopupMenu == null || PopupMenu.IsDisposed) return;
            lock (PopupMenuLockObject) {
                string hmsTypes = HMS.HmsTypesStringWithHelp;
                string keywords = "", snippets = "";
                string hlp = "";
                CurrentValidTypes = HMS.HmsTypesString;
                switch (ScriptLanguage) {
                    case "C++Script":
                        HMS.InitItemsBoolean(true);
                        CurrentValidTypes += "int|long|void|bool|float|";
                        hmsTypes = hmsTypes.Replace("Integer|", "int|long|").Replace("Extended|", "extended|float|").Replace("Boolean|", "bool|").Replace("Boolean|", "bool|").Replace("String", "string") + "|{Тип функции: процедура (отсутствие возвращаемого значения)}void|";
                        keywords = "#include|#define|new|break|continue|exit|delete|return|if|else|switch|default|case|do|while|for|try|finally|except|in|is|nil|null|true|false|";
                        snippets = "for (i=0; i < ^; i++) {\n}|while (^)";
                        break;
                    case "PascalScript":
                        HMS.InitItemsBoolean(false);
                        keywords = "Program|Uses|Const|Var|Not|In|Is|OR|XOR|DIV|MOD|AND|SHL|SHR|Break|Continue|Exit|Begin|End|If|Then|Else|Case|Of|Repeat|Until|While|Do|For|To|DownTo|Try|Finally|Except|With|Function|Procedure|Nil|Null|True|False";
                        snippets = "If ^ Then |If (^) Then Begin\nEnd else Begin\nEnd;";
                        break;
                    case "BasicScript":
                        HMS.InitItemsBoolean(false);
                        keywords = "EOL|IMPORTS|DIM|AS|NOT|IN|IS|OR|XOR|MOD|AND|ADDRESSOF|BREAK|CONTINUE|EXIT|DELETE|SET|RETURN|IF|THEN|END|ELSEIF|ELSE|SELECT|CASE|DO|LOOP|UNTIL|WHILE|WEND|FOR|TO|STEP|NEXT|TRY|FINALLY|CATCH|WITH|SUB|FUNCTION|BYREF|BYVAL|NIL|NULL|TRUE|FALSE";
                        break;
                    case "JScript":
                        HMS.InitItemsBoolean(true);
                        hmsTypes = "var";
                        keywords = "import|new|in|is|break|continue|exit|delete|return|if|else|switch|default|case|do|while|for|try|finally|except|function|with|Nil|Null|True|False";
                        break;
                }
                CurrentValidTypesReg = Regex.Replace(hmsTypes, "{.*?}", "");
                HMS.Keywords = keywords;
                HMS.KeywordsString = keywords.ToLower();
                snippets += "|ShowMessage(\"^\");|HmsLogMessage(1, \"^\");";
                var items = new AutocompleteItems();

                foreach (var s in keywords.Split('|')) if (s.Length > 0) items.Add(new HMSItem(s, ImagesIndex.Keyword, s, s, "Ключевое слово"));
                foreach (var s in snippets.Split('|')) if (s.Length > 0) items.Add(new SnippetHMSItem(s) { ImageIndex = ImagesIndex.Snippet });

                foreach (var name in hmsTypes.Split('|')) {
                    Match m = Regex.Match(name, "{(.*?)}");
                    if (m.Success) hlp = m.Groups[1].Value;
                    var key = Regex.Replace(name, "{.*?}", "");
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
        private object PopupMenuLockObject = new object();
        AutocompleteItems ScriptAutocompleteItems = new AutocompleteItems();
        private static Regex regexCutFunctions = new Regex("<Functions>(.*?)</Functions>");
        private static Regex regexCutVariables = new Regex("<Variables>(.*?)</Variables>");
        private static Regex regexCutConstants = new Regex("<Constants>(.*?)</Constants>");
        private static Regex regexCutItem      = new Regex("<item[^>]+/>"                );
        private static Regex regexCutText      = new Regex("text=\"([^\"]+)"             );
        private static Regex regexCutTextF     = new Regex("text=\"\\w+ ([^\"]+)"        );
        private static Regex regexCutDesc      = new Regex("description=\"([^\"]+)"      );
        private static Regex regexExcludeConst = new Regex("^(True|False|nil|Null)"      );

        private void SearchClassesItemsInScriptDescrition(ref string xml) {
            lock (HMS.HmsClasses) {
                string xmlBlock = Regex.Match(xml, "<Classes>(.*?)</Classes>").Value;
                MatchCollection mc = Regex.Matches(xmlBlock, @"<item(.*?)</item>");
                foreach (Match matchItem in mc) {
                    var text  = regexCutText.Match(matchItem.Value).Groups[1].Value;
                    var descr = regexCutDesc.Match(matchItem.Value).Groups[1].Value;
                    if (text.Length == 0) continue;
                    var item = HMS.GetHmsItemFromLine(text);
                    item.ImageIndex  = ImagesIndex.Class;
                    item.ToolTipText = "Класс";
                    item.Kind = DefKind.Class;
                    item.Help = descr;
                    var foundItem = HMS.ItemsClass.GetItemOrNull(item.MenuText);
                    if (foundItem != null) {
                        if (foundItem.Help.Length == 0) foundItem.Help = descr;
                    } else {
                        var hmsclass = new HMSClassInfo {
                            Name = item.Text,
                            Type = item.Type,
                            Help = item.Help
                        };
                        HMS.HmsClasses.Add(hmsclass);
                        item.Kind         = DefKind.Class;
                        item.ImageIndex   = ImagesIndex.Class;
                        item.ToolTipTitle = "Класс " + item.Text;
                        item.IsClass      = true;
                        item.ClassInfo    = hmsclass;
                        HMS.ItemsClass.Add(item);
                        HMS.ClassesString += hmsclass.Name.ToLower() + "|";
                        //ScriptAutocompleteItems.Add(item);
                        MatchCollection mcChild = Regex.Matches(matchItem.Groups[1].Value, "<item text=\"(.*?)\"");
                        foreach (Match m in mcChild) {
                            var childItem = HMS.GetHmsItemFromLine(m.Groups[1].Value);
                            var cmd = childItem.ToolTipTitle;
                            if      (cmd.StartsWith("function" )) { childItem.ImageIndex = ImagesIndex.Method; childItem.Kind = DefKind.Method   ; }
                            else if (cmd.StartsWith("procedure")) { childItem.ImageIndex = ImagesIndex.Method; childItem.Kind = DefKind.Procedure; }
                            else if (cmd.StartsWith("property" )) { childItem.ImageIndex = ImagesIndex.Field ; childItem.Kind = DefKind.Property ; }
                            else if (cmd.StartsWith("index"    )) { childItem.ImageIndex = ImagesIndex.Enum  ; childItem.Kind = DefKind.Property ; }
                            else if (cmd.StartsWith("event"    )) { childItem.ImageIndex = ImagesIndex.Event ; childItem.Kind = DefKind.Event    ; }
                            var name = Regex.Replace(cmd, @"^(function|procedure|property|index property|event)\s+", "");
                            name = Regex.Match(name, @"\w+").Value.Trim();
                            if (name.Length < 1) name += " ";
                            childItem.Text         = name;
                            childItem.MenuText     = name;
                            childItem.Level        = 1;
                            if      (childItem.ImageIndex == ImagesIndex.Enum  ) childItem.Text = name + "[^]";
                            else if (childItem.ImageIndex == ImagesIndex.Method) {
                                if (cmd.IndexOf('(')>0) childItem.Text = name + "(^)";
                            }
                            if (name.ToLower() == "create") hmsclass?.StaticItems.Add(childItem);
                            else                            hmsclass?.MemberItems.Add(childItem);
                        }
                        //Console.WriteLine(kind.ToString() + " MenuText: " + item.MenuText);
                    }
                }
            }
        }

        private void SearchAutocompleteItemsInScriptDescrition(Regex reBlock, ref string xml, int imageIndex, string toolTipText, DefKind kind, AutocompleteItems Items) {
            string xmlBlock = reBlock.Match(xml).Value;
            MatchCollection mc = regexCutItem.Matches(xmlBlock);
            foreach (Match matchItem in mc) {
                var text = kind == DefKind.Function ? regexCutTextF.Match(matchItem.Value).Groups[1].Value : regexCutText.Match(matchItem.Value).Groups[1].Value;
                var descr = regexCutDesc.Match(matchItem.Value).Groups[1].Value;
                if (text.Length==0) continue;
                var item = HMS.GetHmsItemFromLine(text);
                item.ImageIndex  = imageIndex;
                item.ToolTipText = toolTipText;
                item.Kind        = kind;
                item.Help        = descr;
                if (kind == DefKind.Function) item.Kind = (item.Type.Length > 0) ? DefKind.Function : DefKind.Procedure;
                if (regexExcludeConst.IsMatch(item.MenuText)) continue;
                var foundItem = Items.GetItemOrNull(item.MenuText);
                if (foundItem!=null) {
                    if (foundItem.Help.Length == 0) foundItem.Help = descr;
                } else {
                    ScriptAutocompleteItems.Add(item);
                    //Console.WriteLine(kind.ToString() + " MenuText: " + item.MenuText);
                }
            }
        }

        private void CreateAutocompleteItemsByScriptDescrition() {
            lock (ScriptAutocompleteItems) {
                ScriptAutocompleteItems.Clear();
                string xml = "";
#if DEBUG
                var sw = Stopwatch.StartNew();

                //if ((HmsScriptFrame == null) && (File.Exists(@"D:\descr.txt")))
                //    xml = File.ReadAllText(@"D:\descr.txt");
#endif
                if (HmsScriptFrame != null) {
                    xml = GetScriptDescriptions();
                    //File.WriteAllText(@"D:\descr.txt", xml);
                }

                if (!string.IsNullOrEmpty(xml)) {
                    SearchClassesItemsInScriptDescrition(ref xml);
                    SearchAutocompleteItemsInScriptDescrition(regexCutFunctions, ref xml, ImagesIndex.Procedure, ""                     , DefKind.Function, HMS.ItemsFunction);
                    SearchAutocompleteItemsInScriptDescrition(regexCutVariables, ref xml, ImagesIndex.Field    , "Встроенная переменная", DefKind.Variable, HMS.ItemsVariable);
                    SearchAutocompleteItemsInScriptDescrition(regexCutConstants, ref xml, ImagesIndex.Enum     , "Встроенная константа" , DefKind.Constant, HMS.ItemsConstant);
                }
#if DEBUG
                sw.Stop();
                Console.WriteLine(@"CreateAutocompleteItemsByScriptDescrition ElapsedMilliseconds: " + sw.ElapsedMilliseconds);
                Console.WriteLine(@"ScriptAutocompleteItems.Count: " + ScriptAutocompleteItems.Count);
#endif
                //HMS.AllowPrepareFastDraw = true;
                //HMS.PrepareFastDrawInBackground();
            }
        }

        public void CreateInsertTemplateItems() {
            // Set templates for selected script language
            btnInsertTemplate.DropDownItems.Clear();
            AddTemplateItemsRecursive(btnInsertTemplate, HMS.Templates[TB.Language]);
            btnInsertTemplate.Visible = btnInsertTemplate.DropDownItems.Count > 0;
        }

        private void AddTemplateItemsRecursive(ToolStripMenuItem menuItem, Templates templates) {
            foreach (TemplateItem templateItem in templates) {
                ToolStripItem item = HMS.SetTemplateMenuItem(menuItem, templateItem.Name, templateItem.Text);
                if (templateItem.Submenu) {
                    AddTemplateItemsRecursive((ToolStripMenuItem)item, templateItem.ChildItems);

                } else {
                    item.Click += (o, a) => {
                        var toolStripItem = o as ToolStripItem;
                        if (toolStripItem != null) InsertTemplate(toolStripItem.AccessibleDescription);
                    };

                } // if
            } // foreach

        } // end AddTemplateItemsRecursive

    }
}
