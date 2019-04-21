//#define debug
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using FastColoredTextBoxNS;

namespace HMSEditorNS {
    public static class CodeAnalysis {
        #region Regular Expressions Magnetic Field
        public static Regex RegexExcludeWords = new Regex(@"\b(for|if|else|return|true|false|while|do)\b", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        public static Regex RegexPartOfLine   = new Regex(@"\b(.*?\s.*?\s.*?)\b");

        private static Regex regexProceduresCPP    = new Regex(@"(?:^|[\n])\s*?(?<type>\w+)\s+(?<name>\w+)\s*?\(", RegexOptions.Singleline);
        private static Regex regexProceduresPascal = new Regex(@"\b(?:procedure|function)\s+(?<name>\w+)"        , RegexOptions.Singleline | RegexOptions.IgnoreCase);
        private static Regex regexProceduresBasic  = new Regex(@"(?:^|[\n])\s*?sub\s+(?<name>\w+)"               , RegexOptions.Singleline | RegexOptions.IgnoreCase);
        private static Regex regexDetectProcedure  = new Regex(@"\b(void|procedure)"                             , RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static Regex regexSearchConstantsCPP     = new Regex(@"#define\s+(\w+)(.*)");
        private static Regex regexSearchConstantsPascal1 = new Regex(@"\bconst\b(.*?)\b(var|procedure|function|begin)\b", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static Regex regexSearchConstantsPascal2 = new Regex(@"([\w]+)\s*?=\s*?(.*?)[;\r\n]");

        private static Regex regexSearchVarsCPP       = new Regex(@"(?<type>\w+)\s+&?(?<vars>[^;{}]+)");
        private static Regex regexSearchVarsJS        = new Regex(@"(?<vars>\w+)\s*?=\s*?$?(?<type>[\w""']+)");
        private static Regex regexSearchVarsPascal    = new Regex(@"(?<vars>[\w\s,\r\n]+):(?<type>[^=;\)]+)");
        private static Regex regexSearchVarsBasic     = new Regex(@"\bDIM\s+(?<vars>[\w ,]+)", RegexOptions.IgnoreCase);

        private static Regex regexTwoWords            = new Regex(@"(\w+)\s+&?(\w+)\s*$");
        private static Regex regexAssignment          = new Regex(@"=[^,$]+");
        private static Regex regexConstantKeys        = new Regex(@"\b(var|const)\b", RegexOptions.IgnoreCase);
        private static Regex regexNotValidCharsInVars = new Regex(@"[{}\(\[\]]");
        private static Regex regexExractConstantValue = new Regex(@"(""[^""]*""|'[^']*')");
        private static Regex regexExractCommentCPP    = new Regex(@"^[^\r\n]*(//(?<com1>[^\r\n]+)|\/\*(?<com2>[^\r\n]+)\*\/)");
        private static Regex regexExractCommentPAS    = new Regex(@"^[^\r\n]*(//(?<com1>[^\r\n]+)|\{(?<com2>[^\r\n]+)\})");
        private static Regex regexExractCommentBAS    = new Regex(@"^[^\r\n]*'(?<com1>[^\r\n]+)");

        public  static Regex regexFoundOurFunction    = new Regex(@"([\w\.]+)\s*?[\(\[](.*)$"    , RegexOptions.Singleline);
        private static Regex regexTextOfComment       = new Regex(@"^\s*?//.*?(\w.*?)[\s-=/\*]*$", RegexOptions.Multiline );

        private static Regex regexIsNum      = new Regex(@"(\d|\$)");
        private static Regex regexIsStr      = new Regex(@"(""|')");
        private static Regex regexAllSymbols = new Regex(".");
        private static Regex regexLineBreaks = new Regex(@"[\r\n]");

        private static readonly RegexOptions StdOpt = RegexOptions.Singleline | RegexOptions.IgnoreCase; // Стандартные флаги RegexOptions
        #endregion Regular Expressions magnetic filed

        private static int    distanseBackward4Comments = 90;
        private static string RemoveLinebeaks(string text) { return regexLineBreaks.Replace(text   , "" ); }
        private static string ReturnSpaces(Match m)        { return regexAllSymbols.Replace(m.Value, " "); }
        private static readonly MatchEvaluator evaluatorSpaces = ReturnSpaces;
        private static char CensChar = ' '; // Символ замены строк и комментариев при обработке текста на поиск переменных и проч.

        private static BackgroundWorker WorkerVariables;
        private static BackgroundWorker WorkerFunctions;
        private static object locker = new object();

        public static void Init() {
            lock (locker) {
                if (WorkerVariables == null) {
                    WorkerVariables = new BackgroundWorker();
                    WorkerVariables.DoWork             += WorkerVariables_DoWork;
                    WorkerVariables.RunWorkerCompleted += WorkerVariables_RunWorkerCompleted; ;
                    WorkerVariables.WorkerSupportsCancellation = true;
                }
                if (WorkerFunctions == null) {
                    WorkerFunctions = new BackgroundWorker();
                    WorkerFunctions.DoWork             += WorkerFunctions_DoWork;
                    WorkerFunctions.RunWorkerCompleted += WorkerFunctions_RunWorkerCompleted;
                    WorkerFunctions.WorkerSupportsCancellation = true;
                }
            }
        }

        public static void Stop() {
            lock (locker) {
                if (WorkerVariables != null) {
                    WorkerVariables.CancelAsync();
                }
                if (WorkerFunctions != null) {
                    WorkerFunctions.CancelAsync();
                }
            }
        }

        public static bool isBusy {
            get {
                if (WorkerVariables != null && WorkerVariables.IsBusy) return true;
                if (WorkerFunctions != null && WorkerFunctions.IsBusy) return true;
                return false;
            }
        }

        public static void BuildFunctionListAsync(HMSEditor HmsEditor) {
            if (WorkerFunctions == null) return;
            CodeAnalysesArgs args = new CodeAnalysesArgs();
            HmsEditor.TB.Range.GetText(out args.Text, out args.IndexToPlace);
            args.Editor                 = HmsEditor;
            args.Language               = HmsEditor.TB.Language;
            args.RegexStringAndComments = HmsEditor.TB.RegexStringAndComments;
            if (!WorkerFunctions.IsBusy)
                WorkerFunctions.RunWorkerAsync(args);
        }

        private static void WorkerFunctions_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            if (e.Error != null) { HMS.LogError(e.Error.ToString()); return; }
            if (e.Cancelled) return;
            CodeAnalysesArgs result = e.Result as CodeAnalysesArgs;
            if (result.Editor != null) {
                result.Editor.Functions = result.Functions;
                result.Editor.PopupMenu.Items.SetVisibleFunctionsItems(result.Functions);
                if (result.Functions.Count > 0)
                    result.Editor.TB.SetBeginOfFunction(result.Editor.TB.PositionToPlace(result.Functions[result.Functions.Count-1].PositionStart).iLine);
            }
        }


        private static void WorkerFunctions_DoWork(object sender, DoWorkEventArgs e) {
            CodeAnalysesArgs result = e.Argument as CodeAnalysesArgs;
            if (result == null) return;
#if debug
            var sw = System.Diagnostics.Stopwatch.StartNew();
#endif
            AutocompleteItems Functions = new AutocompleteItems();

            MatchCollection mc = null; int lastIndexExclude;
            string startBlock = "", endBlock = "";
            string txt = result.Text;

            ExcludeRanges excludes = new ExcludeRanges(result.Editor.TB.MultilineComments);
            foreach (Match excludeMatch in result.RegexStringAndComments.Matches(txt)) {
                SimpleRange range = new SimpleRange();
                range.Start = excludeMatch.Index;
                range.End   = excludeMatch.Index + excludeMatch.Length - 1;
                excludes.Add(range);
            }

            switch (result.Language) {
                case Language.CPPScript   :
                case Language.JScript     : mc = regexProceduresCPP   .Matches(txt); startBlock = "{"; endBlock = "}"  ; break;
                case Language.PascalScript: mc = regexProceduresPascal.Matches(txt); startBlock = @"\b(begin|try)\b"   ; endBlock = @"\b(end)\b"             ; break;
                case Language.BasicScript : mc = regexProceduresBasic .Matches(txt); startBlock = @"\b(Sub|Function)\b"; endBlock = @"\bEnd (Sub|Function)\b"; break;
            }
            int lastEndPosition = 0;
            if (mc != null) {
                lastIndexExclude = 0;
                foreach (Match m in mc) {
                    if (WorkerFunctions.CancellationPending) { e.Cancel = true; return; }
                    Group groupName = m.Groups["name"];
                    if (excludes.InScope(groupName.Index, ref lastIndexExclude)) continue;
                    string name = groupName.Value;
                    if (RegexExcludeWords.IsMatch(m.Value)) continue;
                    HMSItem item = new HMSItem {
                        Type     = m.Groups["type"].Value,
                        Text     = name,
                        MenuText = name,
                        Kind     = regexDetectProcedure.IsMatch(m.Value) ? DefKind.Procedure : DefKind.Function
                    };
                    item.ImageIndex    = (item.Kind == DefKind.Function) ? ImagesIndex.Function : ImagesIndex.Procedure;
                    item.ToolTipTitle  = name;
                    item.ToolTipText   = ((item.Kind == DefKind.Function) ? "Функция" : "Процедура") + " (объявлена в скрипте)";
                    item.PositionReal  = m.Index + 1;
                    item.PositionStart = groupName.Index;
                    item.PositionEnd   = item.PositionStart + groupName.Length;
                    // check comment before procedure
                    if (m.Index > distanseBackward4Comments)
                        item.Help = regexTextOfComment.Match(txt.Substring(m.Index - distanseBackward4Comments, distanseBackward4Comments)).Groups[1].Value;
                    // search end of procedure
                    if (startBlock.Length > 0) {
                        var stack = new Stack<string>();
                        MatchCollection mc2 = Regex.Matches(txt.Substring(item.PositionStart), "(" + startBlock + "|" + endBlock + ")", StdOpt);
                        foreach (Match m2 in mc2) {
                            if (WorkerFunctions.CancellationPending) { e.Cancel = true; return; }
                            if (Regex.IsMatch(m2.Value, startBlock, StdOpt)) stack.Push(startBlock);
                            else if (stack.Count > 0) stack.Pop();
                            item.PositionEnd = item.PositionStart + m2.Groups[1].Index;
                            if (excludes.InScope(item.PositionEnd, ref lastIndexExclude)) continue;
                            if (stack.Count < 1) break;
                        }
                        item.PositionEnd += endBlock.Length;
                    }
                    string s = txt.Substring(item.PositionStart, item.PositionEnd - item.PositionStart);
                    Match m3 = Regex.Match(s, @"^(.*?)(\bvar\b|" + startBlock + ")", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    if (m3.Success) item.ToolTipTitle = m3.Groups[1].Value.Trim().Replace("\r", "").Replace("\n", "");
                    if (item.Kind == DefKind.Function) {
                        if (result.Language == Language.PascalScript) {
                            item.Type = HMS.GetVarTypePascalFormat(item.ToolTipTitle);
                        } else {
                            item.ToolTipTitle = item.Type + " " + item.ToolTipTitle;
                        }
                    }
                    if (Functions.Count > 0 && Functions[Functions.Count - 1].PositionEnd > item.PositionReal) {
                        Functions[Functions.Count - 1].PositionEnd  = item.PositionReal - 1;
                        Functions[Functions.Count - 1].PositionReal = item.PositionReal - 1;
                    }
                    Functions.Add(item);
                    lastEndPosition = item.PositionEnd;
                }
            }
            // add info about main start procedure
            if (lastEndPosition < txt.Length) {
                MatchCollection matchesMain = Regex.Matches(txt.Substring(lastEndPosition), startBlock, StdOpt);
                lastIndexExclude = 0;
                foreach (Match matchMainProc in matchesMain) {
                    if (WorkerFunctions.CancellationPending) { e.Cancel = true; return; }
                    if (excludes.InScope(lastEndPosition + matchMainProc.Index, ref lastIndexExclude))
                        continue;
                    HMSItem item = new HMSItem {
                        Type = "MainProcedure",
                        Text = "Главная процедура"
                    };
                    item.MenuText = item.Text;
                    item.Kind = DefKind.Procedure;
                    item.Help = "Процедура, с которой начинается запуск скрипта";
                    item.ImageIndex    = ImagesIndex.Procedure;
                    item.PositionStart = lastEndPosition + matchMainProc.Index;
                    item.PositionEnd   = txt.Length - 1;
                    item.PositionReal  = lastEndPosition + matchMainProc.Index + 1;
                    Functions.Add(item);
                    break;
                }
            }
            result.Functions = Functions;
            e.Result = result;
#if debug
            sw.Stop();
            Console.WriteLine("BuildFunctionList: " + sw.ElapsedMilliseconds);
#endif
        }

        private static string GetGlobalContext(string text, AutocompleteItems Functions) {
            char[] txt = text.ToCharArray();
            foreach (HMSItem item in Functions) {
                for (int i = item.PositionReal; i < item.PositionEnd; i++) {
                    //if (item.Type == "MainProcedure") continue;
                    if (i >= txt.Length) break;
                    txt[i] = txt[i] != '\n' ? CensChar : '\n';
                }
            }
            return new string(txt);
        }

        public static void UpdateCurrentVisibleVariablesAndWait(HMSEditor HmsEditor, int position = -1) {
            try {
                UpdateCurrentVisibleVariables(HmsEditor, position, true);
            } catch (Exception e) {
                HMS.LogError(e.ToString());
            }
        }

        public static void UpdateCurrentVisibleVariables(HMSEditor HmsEditor, int position = -1, bool wait = false) {
            if (WorkerVariables == null) return;
            CodeAnalysesArgs args = new CodeAnalysesArgs();
            HmsEditor.TB.Range.GetText(out args.Text, out args.IndexToPlace);
            args.Editor    = HmsEditor;
            args.Language  = HmsEditor.TB.Language;
            args.Position  = position;
            args.Functions = new AutocompleteItems();
            args.LocalVars = new AutocompleteItems();
            args.Variables = new AutocompleteItems();
            args.NeedRecalcVars = HmsEditor.NeedRecalcVars;
            args.LastPtocedureIndex = HmsEditor.LastPtocedureIndex;
            args.Functions.AddRange(HmsEditor.Functions);
            args.LocalVars.AddRange(HmsEditor.LocalVars);
            args.Variables.AddRange(HmsEditor.Variables);
            if (wait) {
                WorkerVariables_DoWork(null, new DoWorkEventArgs(args));
                WorkerVariables_RunWorkerCompleted(null, new RunWorkerCompletedEventArgs(args, null, false));
            } else {
                if (!WorkerVariables.IsBusy)
                    WorkerVariables.RunWorkerAsync(args);
            }
        }

        private static void WorkerVariables_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            if (e.Error != null) { HMS.LogError(e.Error.ToString()); return; }
            if (e.Cancelled) return;
            CodeAnalysesArgs result = e.Result as CodeAnalysesArgs;
            if (result != null) {
                result.Editor.NeedRecalcVars     = result.NeedRecalcVars;
                result.Editor.LastPtocedureIndex = result.LastPtocedureIndex;
                result.Editor.Variables          = result.Variables;
                result.Editor.LocalVars          = result.LocalVars;

                var Items = result.Editor.PopupMenu.Items;
                Items.SetVisibleVariablesItems(result.Variables);
                Items.SetLocalssVariablesItems(result.LocalVars);
            }
        }

        public static void WorkerVariables_DoWork(object sender, DoWorkEventArgs e) {
            e.Result = e.Argument;
            CodeAnalysesArgs result = e.Argument as CodeAnalysesArgs;
            if (result == null) return;
            if (result.Language == Language.YAML) return;
#if debug
            var sw = System.Diagnostics.Stopwatch.StartNew();
#endif
            int position = result.Position;
            if (position < 0) position = result.Editor.TB.SelectionStart;

            HMSItem itemFunction = GetCurrentProcedure(result.Functions, position);

            if (itemFunction != null) {
                if ((itemFunction.PositionStart == result.LastPtocedureIndex) && !result.NeedRecalcVars) return; // We are in same procedure - skip update
                result.LastPtocedureIndex = itemFunction.PositionStart;
            } else if ((result.LastPtocedureIndex == 0) && !result.NeedRecalcVars) {
                return;
            } else
                result.LastPtocedureIndex = 0;

            result.NeedRecalcVars = false;
            result.LocalVars.Clear();
            if (itemFunction != null) {
                if (result.Variables.Count==0) {
                    string contextGlobal = GetGlobalContext(result.Editor.TB.WithoutStringAndComments(result.Editor.Text), result.Functions);
                    if (contextGlobal.Length > 0) GetVariables(e, result, contextGlobal, 0, result.Variables, result.LocalVars);
                }
                int len = itemFunction.PositionEnd - itemFunction.PositionStart;
                if (len > 0) {
                    string context = result.Text.Substring(itemFunction.PositionStart, len);
                    context = result.Editor.TB.WithoutStringAndComments(context);
                    if (WorkerVariables.CancellationPending) { e.Cancel = true; return; }
                    if (context.Length > 0) GetVariables(e, result, context, itemFunction.PositionStart, result.LocalVars, result.Variables);
                    if (itemFunction.Kind == DefKind.Function) {
                        HMSItem hmsItem = new HMSItem("Result") { ImageIndex = ImagesIndex.Field };
                        hmsItem.MenuText = hmsItem.Text;
                        hmsItem.Type = itemFunction.Type;
                        hmsItem.PositionStart = itemFunction.PositionStart;
                        hmsItem.PositionEnd = itemFunction.PositionEnd;
                        hmsItem.ToolTipText = "Переменная, хранящая значение, которое будет возвращено функцией как результат.";
                        hmsItem.Help = "Используется в PascalScript, но видна как переменная и в других режимах синтаксиса.\nИмеет такой-же тип, как и функция, в которой она видна.";
                        hmsItem.ToolTipTitle = "Result: " + hmsItem.Type;
                        result.LocalVars.Add(hmsItem);
                    }
                } else {
                    int wtf = 1;
                    wtf++;
                }
            } else {
                result.Variables.Clear();
                string contextGlobal = GetGlobalContext(result.Editor.TB.WithoutStringAndComments(result.Editor.Text), result.Functions);
                if (WorkerVariables.CancellationPending) { e.Cancel = true; return; }
                if (contextGlobal.Length > 0) GetVariables(e, result, contextGlobal, 0, result.Variables, result.LocalVars);
            }
            e.Result = result;
#if debug
            sw.Stop();
            Console.WriteLine("UpdateCurrentVisibleVariables: " + sw.ElapsedMilliseconds);
#endif
        }
        private static string MatchReturnEmptyLines(Match m) { return Regex.Replace(m.Value, @"[^\n]", " "); }
        private static Regex StringsRegex = new Regex(@"""(?>(?:\\[^\r\n]|[^""\r\n])*)""?|'(?>(?:\\[^\r\n]|[^'\r\n])*)'");
        public static string WithoutString(string txt) {
            return StringsRegex.Replace(txt, MatchReturnEmptyLines);
        }

        const int maxLineLength = 200;
        private static void GetVariables(DoWorkEventArgs e, CodeAnalysesArgs result, string txt,
                                int indexContext, AutocompleteItems ITEMS, AutocompleteItems ITEMSVersus) {
            MatchCollection mc; bool isGlobalContext = (indexContext == 0);
            string allText = result.Editor.Text;
            // Collect constants
            if (isGlobalContext) {
                HMS.ItemsConstantUser.Clear();
                switch (result.Language) {
                    case Language.CPPScript:
                        mc = regexSearchConstantsCPP.Matches(txt);
                        foreach (Match m in mc) {
                            if (WorkerVariables.CancellationPending) { e.Cancel = true; return; }
                            string name = m.Groups[1].Value;
                            string sval = m.Groups[2].Value.Trim(); // Value
                            if (!ITEMS.ContainsName(name)) {
                                var item = new HMSItem {
                                    Global     = true,
                                    Kind       = DefKind.Constant,
                                    ImageIndex = ImagesIndex.Enum,
                                    Text       = name.Trim()
                                };
                                item.MenuText      = RemoveLinebeaks(item.Text);
                                item.ToolTipTitle  = item.Text;
                                item.ToolTipText   = "Объявленная константа";
                                item.Type          = GetTypeOfConstant(sval);
                                item.PositionStart = m.Groups[1].Index + indexContext;
                                item.PositionEnd   = item.PositionStart + name.Length;
                                int linelen        = Math.Min(allText.Length - m.Groups[2].Index, maxLineLength);
                                if (linelen < 1) break;
                                string textline    = allText.Substring(m.Groups[2].Index, linelen);
                                if (item.Type.Length > 0) item.ToolTipText += "\nТип: " + item.Type;
                                if ((sval.Length == 0) || (sval == ";")) sval = "<s>" + regexExractConstantValue.Match(textline).Value + "</s>";
                                if (sval.Length > 0) item.ToolTipTitle += " = " + sval;
                                var cm = regexExractCommentCPP.Match(WithoutString(textline));
                                var comment = "";
                                if (cm.Groups["com1"].Success) comment = cm.Groups["com1"].Value;
                                if (cm.Groups["com2"].Success) comment = cm.Groups["com2"].Value;
                                if (comment.Length > 0) item.Help = comment;
                                ITEMS.Add(item);
                                HMS.ItemsConstantUser.Add(item);
                            }
                        }
                        txt = regexSearchConstantsCPP.Replace(txt, MatchReturnEmptyLines);
                        break;
                    case Language.PascalScript:
                        Match c = regexSearchConstantsPascal1.Match(txt);
                        if (c.Success) {
                            mc = regexSearchConstantsPascal2.Matches(c.Groups[1].Value);
                            foreach (Match m in mc) {
                                if (WorkerVariables.CancellationPending) { e.Cancel = true; return; }
                                string name = m.Groups[1].Value;
                                string sval = m.Groups[2].Value.Trim(); // Value
                                if (!ITEMS.ContainsName(name)) {
                                    var item = new HMSItem {
                                        Global = true,
                                        Kind   = DefKind.Constant,
                                        PositionStart = c.Groups[1].Index + m.Index + indexContext
                                    };
                                    item.PositionEnd  = item.PositionStart + name.Length;
                                    item.ImageIndex   = ImagesIndex.Enum;
                                    item.Text         = name.Trim();
                                    item.MenuText     = RemoveLinebeaks(item.Text);
                                    item.ToolTipTitle = item.Text;
                                    item.ToolTipText  = "Объявленная константа";
                                    item.Type         = GetTypeOfConstant(sval);
                                    int linelen       = Math.Min(allText.Length - m.Groups[2].Index, maxLineLength);
                                    if (linelen < 1) break;
                                    string textline   = allText.Substring(m.Groups[2].Index, linelen);
                                    if (item.Type.Length > 0) item.ToolTipText += "\nТип: " + item.Type;
                                    if ((sval.Length == 0) || (sval == ";")) sval = "<s>" + regexExractConstantValue.Match(textline).Value + "</s>";
                                    if (sval.Length > 0) item.ToolTipTitle += " = " + sval;
                                    var cm = regexExractCommentPAS.Match(WithoutString(textline));
                                    var comment = "";
                                    if (cm.Groups["com1"].Success) comment = cm.Groups["com1"].Value;
                                    if (cm.Groups["com2"].Success) comment = cm.Groups["com2"].Value;
                                    if (comment.Length > 0) item.Help = comment;
                                    ITEMS.Add(item);
                                    HMS.ItemsConstantUser.Add(item);
                                }
                            }
                            txt = regexSearchConstantsPascal1.Replace(txt, MatchReturnEmptyLines);
                        }
                        break;
                }
                if (HMS.ItemsConstantUser.Count > 0) HMS.BuildConstantSyntaxRegexes();
            }

            mc = null;

            switch (result.Language) {
                case Language.CPPScript   : mc = regexSearchVarsCPP   .Matches(txt); break;
                case Language.JScript     : mc = regexSearchVarsJS    .Matches(txt); break;
                case Language.PascalScript: mc = regexSearchVarsPascal.Matches(txt); break;
                case Language.BasicScript : mc = regexSearchVarsBasic .Matches(txt); break;
            }
            if (mc != null) {
                foreach (Match m in mc) {
                    if (WorkerVariables.CancellationPending) { e.Cancel = true; return; }
                    int    index = m.Groups["vars"].Index;
                    string names = m.Groups["vars"].Value;
                    string type  = m.Groups["type"].Value.Trim();
                    if (!ValidHmsType(result.Editor.CurrentValidTypes, type)) {
                        if (result.Language == Language.JScript) {
                            if (!DetectTypeOfHmsItem(result.Editor, ref type))
                                continue;
                        } else
                            continue;
                    }
                    if (type.ToLower()=="variant") {
                        // Попытка определения реального типа переменной после присвоения
                        var mt = Regex.Match(names, "=([^;$]+)");
                        if (mt.Success) {
                            string detectedType = mt.Groups[1].Value.Trim();
                            if (DetectTypeOfHmsItem(result.Editor, ref detectedType))
                                type = detectedType;
                        }
                    }
                    names = HMS.GetTextWithoutBrackets(names); // Убираем скобки и всё что в них
                    names = regexAssignment.Replace(names, evaluatorSpaces); // Убираем присвоение - знак равно и после
                    names = regexConstantKeys.Replace(names, evaluatorSpaces); // Убираем ключевые слова констант (var, const)
                    string[] aname = names.Split(',');
                    foreach (string namePart in aname) {
                        if (WorkerVariables.CancellationPending) { e.Cancel = true; return; }
                        string name = namePart;
                        if ((namePart.Trim().Length != 0) && !RegexExcludeWords.IsMatch(namePart)) {
                            if (Regex.IsMatch(name, @"\b(\w+).*?\b(\w+).*?\b(\w+)")) continue;
                            Match m2 = regexTwoWords.Match(name);
                            if (m2.Success) {
                                bool typeFirst = (index > m.Groups["type"].Index);
                                type   = m2.Groups[typeFirst ? 1 : 2].Value;
                                name   = m2.Groups[typeFirst ? 2 : 1].Value;
                                index += m2.Groups[typeFirst ? 2 : 1].Index;
                            }
                            if (!regexNotValidCharsInVars.IsMatch(name) && !ITEMS.ContainsName(name) && !result.Functions.ContainsName(name)) {
                                HMSItem item = new HMSItem {
                                    Global = isGlobalContext,
                                    Kind = DefKind.Variable,
                                    Text = name.Trim(),
                                    Type = type.Trim()
                                };
                                item.MenuText      = RemoveLinebeaks(item.Text);
                                item.ToolTipTitle  = item.Text;
                                item.ToolTipText   = item.Global ? "Глобальная переменная" : "Локальная переменная";
                                item.PositionStart = index + (name.Length - name.TrimStart().Length) + indexContext;
                                item.PositionEnd   = item.PositionStart + name.Length;
                                item.ImageIndex    = ImagesIndex.Field;
                                if (item.Type.Length > 0) item.ToolTipText += "\nТип: " + item.Type;
                                int linelen        = Math.Min(allText.Length - m.Groups[2].Index, maxLineLength);
                                if (linelen < 1) break;
                                string textline = WithoutString(allText.Substring(index + indexContext, linelen));
                                Match commentMatch = null;
                                switch (result.Language) {
                                    case Language.CPPScript   :
                                    case Language.JScript     : commentMatch = regexExractCommentCPP.Match(textline); break;
                                    case Language.PascalScript: commentMatch = regexExractCommentPAS.Match(textline); break;
                                    case Language.BasicScript : commentMatch = regexExractCommentBAS.Match(textline); break;
                                }
                                if (commentMatch != null) {
                                    var comment = "";
                                    if (commentMatch.Groups["com1"].Success) comment = commentMatch.Groups["com1"].Value;
                                    if (commentMatch.Groups["com2"].Success) comment = commentMatch.Groups["com2"].Value;
                                    if (comment.Length > 0) item.Help = comment;
                                }
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
            if (result.Language == Language.BasicScript) {
                // search types of vars
                mc = regexSearchVarsJS.Matches(txt);
                foreach (Match m in mc) {
                    if (WorkerVariables.CancellationPending) { e.Cancel = true; return; }
                    string name = m.Groups["vars"].Value;
                    string type = m.Groups["type"].Value.Trim();
                    if (string.IsNullOrEmpty(ITEMS[name].Text)) continue;
                    if (!ValidHmsType(result.Editor.CurrentValidTypes, type)) {
                        if (!DetectTypeOfHmsItem(result.Editor, ref type))
                            continue;
                    }
                    ITEMS[name].Type = type;
                    ITEMS[name].ToolTipText = (ITEMS[name].Global ? "Глобальная переменная" : "Локальная переменная") + "\nТип: " + type;
                }
            }

        }

        public static bool DetectTypeOfHmsItem(HMSEditor Editor, ref string type) {
            float val;
            HMSItem it = Editor.GetHMSItemByText(type);
            if (it != null)
                type = it.Type;
            else if (float.TryParse(type, out val))
                type = "Integer";
            else if (type.Length > 0 && (type[0] == '\'' || type[0] == '"'))
                type = "String";
            else if (type.StartsWith("["))
                type = "Array";
            else
                return false;
            return true;
        }

        public static string CurrentVariableName(FastColoredTextBox ts) {
            Range r = new Range(ts, ts.Selection.Start, ts.Selection.Start);
            r = r.GetFragment(@"[\w]");
            if (r.IsStringOrComment) return "";
            return r.Text;
        }

        public static HMSItem GetCurrentProcedure(AutocompleteItems Functions, int position) {
            foreach (var item in Functions) if ((position > item.PositionStart) && (position < item.PositionEnd)) return item;
            return null;
        }

        private static string GetTypeOfConstant(string part) {
            if (part.Length == 0        ) return "String" ;
            if (regexIsNum.IsMatch(part)) return "Integer";
            if (regexIsStr.IsMatch(part)) return "String" ;
            return "Variant";
        }

        private static bool ValidHmsType(string currentValidTypes, string type) {
            var lowertype = type.ToLower();
            if (currentValidTypes.IndexOf("|" + lowertype + "|", StringComparison.Ordinal) >= 0) return true;
            if (HMS.ClassesString.IndexOf("|" + lowertype + "|", StringComparison.Ordinal) >= 0) return true;
            return false;
        }

    }

    public class CodeAnalysesArgs {
        public HMSEditor Editor;
        public string Text;
        public List<Place> IndexToPlace;
        public Language Language;
        public AutocompleteItems Functions;
        public AutocompleteItems Variables;
        public AutocompleteItems LocalVars;
        public int LastPtocedureIndex;
        public bool NeedRecalcVars;
        public int Position;
        public Regex RegexStringAndComments;
    }

    public class SimpleRange {
        public int Start;
        public int End;
    }

    public class ExcludeRanges: List<SimpleRange> {

        MultilineComments MC;

        public ExcludeRanges(MultilineComments mc) {
            MC = mc;
        }

        public bool InScope(int place, ref int indexFrom) {
            for (; indexFrom < Count; indexFrom++) {
                var range = this[indexFrom];
                if (place >= range.Start && place <= range.End)
                    return true;
                if (range.Start > place) {
                    if (indexFrom > 0) indexFrom--;
                    break;
                }
            }
            return MC.IsComment(place);
        }
    }

    public class CheckSyntaxArgs {
        public int    ErrorLine    = 0;
        public int    ErrorChar    = 0;
        public string ErrorMessage = "";
    }

}
