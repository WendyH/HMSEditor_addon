using System;
using System.Text.RegularExpressions;
using System.Drawing;
using FastColoredTextBoxNS;
using Newtonsoft.Json;

namespace HMSEditorNS {
    /// <summary>
    /// Класс для работы и отображения всплывающей подсказки при наведении курсора мыши на часть слова редакторе
    /// </summary>
    class MouseHelpTimer {
        const int MaxValueLength = 89000;
        internal static Regex regexRemoveParams = new Regex(@"\(([^\)])*\)|\[([^\]])*\]|(//.*|\/\*[\s\S]*?\*\/)");
        internal static Regex regexNoNewLines   = new Regex(@"[^\n]");

        internal static MatchEvaluator evaluatorSharpLines = ReturnSharpLines;
        internal static string ReturnSharpLines(Match m) { return regexNoNewLines.Replace(m.Value, "#"); }

        /// <summary>
        /// Проверка типа переменной, на возможность более удобного представления при Evaluate
        /// </summary>
        /// <param name="type">Тип переменной</param>
        /// <param name="text">Часть текста (имя меременной, свойства)</param>
        /// <returns></returns>
        private static string CheckTypeForToStringRules(HMSItem item, string text) {
            if ((text.EndsWith("]") || text.EndsWith(")")) && item.ImageIndex != ImagesIndex.Enum) return text;
            string type = item.Type;
            var info = HMS.HmsClasses[type];
            if (info.MemberItems.ContainsName("SaveToString") && type.ToLower()!="tbitmap32")
                return text + ".SaveToString";
            if (info.MemberItems.ContainsName("Text")) return text + ".Text";
            switch (type) {
                case "THmsScriptMediaItem":
                    return "\"" + text + "=\"+Str(" + text + ")+\" (Тип: THmsScriptMediaItem)\"#13#10+\"mpiTitle=\"+" + text + ".Properties[mpiTitle]+#13#10+\"mpiFilePath=\"+" + text + ".Properties[mpiFilePath]+#13#10+\"mpiThumbnail=\"+Str(" + text + ".Properties[mpiThumbnail])+#13#10+\"mpiTimeLength=\"+Str(" + text + ".Properties[mpiTimeLength])+#13#10+\"mpiCreateDate=\"+Str(" + text + ".Properties[mpiCreateDate])";
                case "TRegExpr":
                    return "\"" + text + "=\"+Str(" + text + ")+\" (Тип: TRegExpr)\"#13#10+\"Match(0)=\"+" + text + ".Match(0)+#13#10+\"Match(1)=\"+" + text + ".Match(1)+#13#10+\"Match(2)=\"+" + text + ".Match(2)+#13#10+\"Match(3)=\"+" + text + ".Match(3)";
            }
            return text;
        }

        private static string successMethods4Eval = "|Match|ProgramVersion|HmsCryptCipherList|HmsCryptFormatList|HmsCryptHashList|HmsDataDirectory|HmsGetUserSearchText|HmsSubtitlesDirectory|HmsTempDirectory|ProgramPath|HmsClientID|HmsTranscodingInputParams|HmsTranscodingProbeParams|HmsTranscodingTempDirectory|HmsVlcVersion|".ToLower();
        private static bool OK4Evaluate(HMSItem item) {
            bool success = ((item.Kind == DefKind.Property) || (item.Kind == DefKind.Variable));
            if (!success && (item.Kind == DefKind.Function) || (item.Kind == DefKind.Method)) {
                success = successMethods4Eval.IndexOf("|"+item.MenuText.ToLower()+"|", StringComparison.Ordinal) >= 0;
            }
            return success;
        }

        private static string SafeEval(HMSEditor activeEditor, string expression) {
            string value = "";
            System.Windows.Forms.MethodInvoker action = delegate {
                value = activeEditor.EvalVariableValue(expression);
            };
            if (activeEditor.InvokeRequired)
                activeEditor.Invoke(action);
            else
                action();
            return value;
        }

        private static string ObjectToValues(string expression) {
            var activeEditor = HMSEditor.ActiveEditor;
            string pred = "["; bool strict = false;
            int.TryParse(activeEditor.EvalVariableValue("Length(" + expression + ")"), out int len);
            if (len > 32) { len = 32; strict = true; }
            for (int i = 0; i < len; i++) {
                string elem = activeEditor.EvalVariableValue(expression + "[" + i + "]");
                string typeVal = activeEditor.EvalVariableValue("VarType(" + expression + "[" + i + "])");
                if (i > 0) pred += ",";
                pred += elem;
            }
            if (strict) { pred += ",..."; }
            pred += "]";
            return pred;
        }

        private static bool IsDigitsOnly(string str) {
            foreach (char c in str) if (c < '0' || c > '9') return false;
            return true;
        }

        private static void ShowValue(FastColoredTextBox tb, Point point, string expression, string realExpression, bool isItem=false, string type="") {
            System.Windows.Forms.MethodInvoker action = delegate {
                point.Offset(0, tb.CharHeight - 4);
                tb.ReshowCaret = true;
                var activeEditor = HMSEditor.ActiveEditor;
                if (activeEditor != null) {
                    string typeVal = ""; string value="";
                    if (isItem)
                        typeVal = activeEditor.EvalVariableValue("VarType(" + expression + ")");
                    if (typeVal == "8204") // Это Array
                        value = ObjectToValues(expression);
                    else
                        value = activeEditor.EvalVariableValue(expression);
                    if (value == realExpression) return; // Не показываем просто числовые значения

                    if (type == "TJsonObject") {
                        try {
                            var ob = JsonConvert.DeserializeObject(value);
                            value = JsonConvert.SerializeObject(ob);
                            Jsbeautifier.BeautifierOptions beautifierOptions = new Jsbeautifier.BeautifierOptions();
                            beautifierOptions.BraceStyle = Jsbeautifier.BraceStyle.EndExpand;
                            Jsbeautifier.Beautifier beautifier = new Jsbeautifier.Beautifier(beautifierOptions);
                            value = beautifier.Beautify(value);
                        }
                        catch { ; }
                    }

                    if (value.Length < 32) {
                        if (IsDigitsOnly(value) && NativeMethods.KeyState(NativeMethods.VirtualKeyStates.VK_CONTROL))
                            value = string.Format("0x{0:X}", int.Parse(value));
                    }

                    if (value.Length > MaxValueLength || activeEditor.ValueForm.Visible) {
                        value = value.Substring(0, MaxValueLength) + "...";
                        activeEditor.ValueForm.Show(tb, expression, value, realExpression);
                    } else {
                        activeEditor.ValueHint.ShowValue(tb, expression, value, point, realExpression);
                    }
                }
            };
            if (tb.InvokeRequired) tb.Invoke(action);
            else action();
        }

        private static void ShowToolTip(FastColoredTextBox tb, Point point, string title, string text, string value, string help) {
            // Показываем инофрмацию о функции или переменной через ToolTip
            System.Windows.Forms.MethodInvoker action = delegate {
                var tip = tb.ToolTip;
                tip.ToolTipTitle = title;
                tip.Help = help;
                tip.Value = value;
                tip.Show(text + " ", tb, point, 10000);
            };
            if (tb.InvokeRequired) tb.Invoke(action);
            else action();
        }

        /// <summary>
        /// Статическая процедура, вызываемая из MouseTimer_Task по срабатыванию MouseTimer для отображения подсказки при наведении мышкой на часть текста
        /// </summary>
        /// <param name="ActiveHMSEditor">Активный (вызвавший) элемент HMSEditor</param>
        public static void Task(HMSEditor ActiveHMSEditor) {
            var    TB    = ActiveHMSEditor.TB;
            Point  point = ActiveHMSEditor.MouseLocation;
            bool   debugMode  = TB.DebugMode;
            bool   selectMode = false;
            string expression = "";
            try {
                Place place = TB.PointToPlace(point);
                Range r = new Range(TB, place, place);
                if (r.IsStringOrCommentBefore()) return;
                if (r.IsErrorPlace) {
                    // Показываем инофрмацию об ошибке через ToolTip
                    ShowToolTip(TB, point, "Ошибка синтаксиса", "", "", TB.ErrorStyle.Message);
                    return;
                }
                if (debugMode && (TB.Selection.Start != TB.Selection.End) && TB.Selection.InRange(place)) {
                    expression = TB.SelectedText.Trim();
                    selectMode = true;
                }
                if (expression.Length == 0) {
                    expression = r.GetFragmentAroundThePlace(place).Text.Replace("#", "").Trim();
                }
                if (expression.Length == 0)
                    return;
                var item = ActiveHMSEditor.GetHMSItemByText(expression);
                if (item != null) {
                    // Если идёт отладка - проверяем, мы навели на переменную или свойство объекта?
                    if (debugMode && (OK4Evaluate(item) || selectMode)) {
                        // Проверяем тип объекта класса, может быть удобней представить в виде текста? (TStrings или TJsonObject)
                        string realExpression = expression;
                        expression = CheckTypeForToStringRules(item, expression);
                        ShowValue(TB, point, expression, realExpression, true, item.Type);
                        return;
                    }
                    ShowToolTip(TB, point, item.ToolTipTitle, item.ToolTipText, "", item.Help);
                } else if (debugMode && !HMS.WordIsKeyword(expression)) {
                    ShowValue(TB, point, expression, expression);
                }

            } catch (Exception e) {
                HMS.LogError(e.ToString());
            }

        }

    }
}
