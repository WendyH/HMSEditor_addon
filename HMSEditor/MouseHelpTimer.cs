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
        private static string CheckTypeForToStringRules(string type, string text) {
            if (text.EndsWith("]") || text.EndsWith(")")) return text;
            var info = HMS.HmsClasses[type];
            if (info.MemberItems.ContainsName("SaveToString") && type.ToLower()!="tbitmap32")
                return text + ".SaveToString";
            if (info.MemberItems.ContainsName("Text")) return text + ".Text";
            switch (type) {
                case "THmsScriptMediaItem":
                    return "\"" + text + "=\"+Str(" + text + ")+\" (Тип: THmsScriptMediaItem)\"#13+\"mpiTitle=\"+" + text + ".Properties[mpiTitle]+#13+\"mpiFilePath=\"+" + text + ".Properties[mpiFilePath]+#13+\"mpiThumbnail=\"+Str(" + text + ".Properties[mpiThumbnail])+#13+\"mpiTimeLength=\"+Str(" + text + ".Properties[mpiTimeLength])+#13+\"mpiCreateDate=\"+Str(" + text + ".Properties[mpiCreateDate])";
                case "TRegExpr":
                    return "\"" + text + "=\"+Str(" + text + ")+\" (Тип: TRegExpr)\"#13+\"Match(0)=\"+" + text + ".Match(0)+#13+\"Match(1)=\"+" + text + ".Match(1)+#13+\"Match(2)=\"+" + text + ".Match(2)+#13+\"Match(3)=\"+" + text + ".Match(3)";
                case "TJsonArray":
                    return "\"" + text + "=\"+Str(" + text + ")+\" (Тип: TJsonArray)\"#13+\"Length=\"+Str(" + text + ".Length)";
            }
            if (info.MemberItems.Count > 0) {
                string cmd = "\"" + text + "=\"+Str(" + text + ")+\" (Тип: " + type + ")\"";
                foreach (var item in info.MemberItems) {
                    if ((item.Kind != DefKind.Property) || (Regex.IsMatch(item.Text, "\\W"))) continue;
                    cmd += "+#13+\""+ item.MenuText + "=\"+Str(" + text + "." + item.MenuText + ")";
                }
                return cmd;
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

        private static bool IsDigitsOnly(string str) {
            foreach (char c in str) if (c < '0' || c > '9') return false;
            return true;
        }

        private static void ShowValue(FastColoredTextBox tb, Point point, string expression, string realExpression, bool isItem=false, string type="") {
            System.Windows.Forms.MethodInvoker action = delegate {
                var activeEditor = HMSEditor.ActiveEditor;
                if (activeEditor != null) {
                    if (activeEditor.HintsInWindow)
                        point = tb.PointToClient(point);
                    point.Offset(0, tb.CharHeight - 4);
                    tb.ReshowCaret = true;
                    string value = activeEditor.EvalVariableValue(expression);
                    if (value == realExpression) return; // Не показываем просто числовые значения

                    if (value.Length < 16) {
                        if (IsDigitsOnly(value) && NativeMethods.KeyState(NativeMethods.VirtualKeyStates.VK_CONTROL))
                            value = string.Format("0x{0:x}", ulong.Parse(value));
                    }

                    if (value.Length > MaxValueLength) {
                        activeEditor.ValueForm.Formatting = false;
                        activeEditor.ValueForm.Show(tb, expression, value, realExpression, activeEditor.HintsInWindow);
                    } else {
                        if (type == "TJsonObject") {
                            try {
                                var ob = JsonConvert.DeserializeObject(value);
                                value = JsonConvert.SerializeObject(ob);
                                Jsbeautifier.BeautifierOptions beautifierOptions = new Jsbeautifier.BeautifierOptions();
                                beautifierOptions.BraceStyle = Jsbeautifier.BraceStyle.EndExpand;
                                Jsbeautifier.Beautifier beautifier = new Jsbeautifier.Beautifier(beautifierOptions);
                                value = beautifier.Beautify(value);
                            } catch {; }
                        }
                        activeEditor.ValueHint.ShowValue(tb, expression, value, point, realExpression, activeEditor.HintsInWindow);
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
            Point  point = TB.lastMouseCoord;
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
                        if (TB.Language == Language.JScript)
                            expression = TryGetTypeOfVariabe(ActiveHMSEditor, item, expression);
                        else if (item.ImageIndex != ImagesIndex.Enum)
                            expression = CheckTypeForToStringRules(item.Type, expression);
                        ShowValue(TB, point, expression, realExpression, true, item.Type);
                        return;
                    }
                    ShowToolTip(TB, point, item.ToolTipTitle, item.ToolTipText, "", item.Help);
                } else if (debugMode && !HMS.WordIsKeyword(expression)) {
                    if (TB.Language == Language.JScript)
                        expression = TryGetTypeOfVariabe(ActiveHMSEditor, item, expression);
                    ShowValue(TB, point, expression, expression);
                }

            } catch (Exception e) {
                HMS.LogError(e.ToString());
            }

        }
        public static string TryGetTypeOfVariabe(HMSEditor ActiveHMSEditor, HMSItem item, string expression) {
            System.Windows.Forms.MethodInvoker action = delegate {
                string classname = ActiveHMSEditor.EvalVariableValue(expression + ".ClassName");
                if (classname.Length > 0)
                    expression = CheckTypeForToStringRules(classname, expression);
            };
            if (ActiveHMSEditor.InvokeRequired) ActiveHMSEditor.Invoke(action);
            else action();
            return expression;
        }
    }

    public sealed class HexStringJsonConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return typeof(int).Equals(objectType) || typeof(uint).Equals(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            writer.WriteValue($"0x{value:x}");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var str = reader.ReadAsString();
            if (str == null || !str.StartsWith("0x"))
                throw new JsonSerializationException();
            return Convert.ToUInt32(str);
        }
    }
}
