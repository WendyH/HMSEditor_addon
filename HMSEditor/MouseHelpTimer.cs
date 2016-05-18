﻿using System;
using System.Text.RegularExpressions;
using System.Drawing;
using FastColoredTextBoxNS;

namespace HMSEditorNS {
    /// <summary>
    /// Класс для работы и отображения всплывающей подсказки при наведении курсора мыши на часть слова редакторе
    /// </summary>
    class MouseHelpTimer {
        const int MaxValueLength = 119000;
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
            var info = HMS.HmsClasses[type];
            if (info.MemberItems.ContainsName("SaveToString") && type.ToLower()!="tbitmap32") return text + ".SaveToString";
            if (info.MemberItems.ContainsName("Text")) return text + ".Text";
            if (type == "THmsScriptMediaItem")
                return "Str("+text+ ")+\" (Тип: THmsScriptMediaItem)\"#13#10+\"mpiTitle=\"+" + text + "[mpiTitle]+#13#10+\"mpiFilePath=\"+" + text + "[mpiFilePath]+#13#10+\"mpiThumbnail=\"+Str(" + text + "[mpiThumbnail])+#13#10+\"mpiTimeLength=\"+Str(" + text + "[mpiTimeLength])+#13#10+\"mpiCreateDate=\"+Str(" + text + "[mpiCreateDate])";
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

        /// <summary>
        /// Статическая процедура, вызываемая из MouseTimer_Task по срабатыванию MouseTimer для отображения подсказки при наведении мышкой на часть текста
        /// </summary>
        /// <param name="ActiveHMSEditor">Активный (вызвавший) элемент HMSEditor</param>
        public static void Task(HMSEditor ActiveHMSEditor) {
            if (CodeAnalysis.isBusy) return;
            try {
                var    TB      = ActiveHMSEditor.TB;
                Point  point   = ActiveHMSEditor.MouseLocation;
                int iStartLine = ActiveHMSEditor.TB.YtoLineIndex();
                int CharHeight = ActiveHMSEditor.TB.CharHeight;
                int i = point.Y / CharHeight;
                int iLine = iStartLine + i;
                if (iLine >= TB.Lines.Count) return;
                Place place   = TB.PointToPlace(point);
                string line;
                try {  line   = TB.Lines[iLine]; } catch { return; }
                if (line.Length <= place.iChar) return;
                var value  = "";
                var evalSelection = false;
                string text;
                if (TB.DebugMode && (TB.SelectedText.Length > 2)) {
                    int posStart = TB.PlaceToPosition(TB.Selection.Start);
                    int posEnd   = TB.PlaceToPosition(TB.Selection.End  );
                    int posCur   = TB.PlaceToPosition(place);
                    // Если указатель мыши в области виделения, то будем вычислять выдиление
                    if (posStart < posEnd) {
                        evalSelection = (posCur >= posStart) && (posCur <= posEnd  );
                    } else {
                        evalSelection = (posCur >= posEnd  ) && (posCur <= posStart);
                    }
                }
                if (evalSelection) {
                    text = TB.SelectedText.Trim();
                    if (text.Length == 0) return;
                    // Внедряемся в поток - показываем вплывающее окно со значением
                    TB.Invoke((System.Windows.Forms.MethodInvoker)delegate {
                        TB.ReshowCaret = true;
                        value = ActiveHMSEditor.EvalVariableValue(text); // Вычсиление выражения
                        if (value.Length > MaxValueLength || HMSEditor.ActiveEditor.ValueForm.Visible) {
                            //value = value.Substring(0, MaxValueLength) + "...";
                            HMSEditor.ActiveEditor.ValueForm.Show(TB, text, value, text);
                        } else {
                            ActiveHMSEditor.ValueHint.ShowValue(TB, text, value, point, text);
                        }
                    });
                    return;

                }
                Range r = new Range(TB, place, place);
                if (r.IsErrorPlace) {
                    // Показываем инофрмацию об ошибке через ToolTip
                    TB.Invoke((System.Windows.Forms.MethodInvoker)delegate {
                        var tip = TB.ToolTip;
                        tip.ToolTipTitle = "Ошибка синтаксиса"; 
                        tip.Help = TB.ErrorStyle.Message;
                        tip.Show(" ", TB, point, 10000);
                    });
                    return;
                }
                if (r.IsStringOrComment) return;
                Range fragment = r.GetFragmentLookedLeft();
                text = fragment.Text.Replace("#", "").Trim();

                if (text.Length == 0) return;
                var item = ActiveHMSEditor.GetHMSItemByText(text);
                if (!string.IsNullOrEmpty(item?.Text)) {
                    point.Offset(0, TB.CharHeight-4);
                    // Если идёт отладка - проверяем, мы навели на переменную или свойство объекта?
                    if (TB.DebugMode && OK4Evaluate(item)) {
                        // проверяем, если это index свойство - то нудно вычислять значение с переданным индексом, поэтому дополняем значением [...]
                        if (item.ImageIndex == ImagesIndex.Enum) {
                            Match m = Regex.Match(line, text + @"\[.*?\]");
                            if (m.Success) text = m.Value;
                        } else if (item.ImageIndex == ImagesIndex.Function) {
                            Match m = Regex.Match(line, text + @"\(.*?\)");
                            if (m.Success) text = m.Value;
                        }
                        // Проверяем тип объекта класса, может быть удобней представить в виде текста? (TStrings или TJsonObject)
                        string realExpression = text;
                        text = CheckTypeForToStringRules(item.Type, text);
                        // Внедряемся в поток - показываем вплывающее окно со значением
                        TB.Invoke((System.Windows.Forms.MethodInvoker)delegate {
                            TB.ReshowCaret = true;
                            value = ActiveHMSEditor.EvalVariableValue(text); // Вычсиление выражения
                            if (HMSEditor.ActiveEditor.ValueForm.Visible) {
                                HMSEditor.ActiveEditor.ValueForm.Show(TB, text, value, realExpression);
                            } else {
                                if (value.Length > MaxValueLength) value = value.Substring(0, MaxValueLength) + "...";
                                ActiveHMSEditor.ValueHint.ShowValue(TB, text, value, point, realExpression);
                            }
                        });
                        return;
                    }
                    // Показываем инофрмацию о функции или переменной через ToolTip
                    TB.Invoke((System.Windows.Forms.MethodInvoker)delegate {
                        var tip = TB.ToolTip;
                        tip.ToolTipTitle = item.ToolTipTitle;
                        tip.Help         = item.Help;
                        tip.Value        = value;
                        tip.Show(item.ToolTipText + " ", TB, point, 10000);
                        //ActiveHMSEditor.ValueForm.Show(Editor, item.ToolTipTitle, item.Help + " ");
                        //ActiveHMSEditor.ValueHint.ShowValue(Editor, item.ToolTipTitle, item.Help, point, "test");
                    });
                }
            } catch (Exception e) {
                HMS.LogError(e.ToString());
            }

        }

    }
}
