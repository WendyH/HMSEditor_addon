using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Drawing;
using FastColoredTextBoxNS;

namespace HMSEditorNS {
	/// <summary>
	/// Класс для работы и отображения всплывающей подсказки при наведении курсора мыши на часть слова редакторе
	/// </summary>
	class MouseHelpTimer {
		internal static Regex regexRemoveParams = new Regex(@"\(([^\)])*\)|\[([^\]])*\]|(//.*|\/\*[\s\S]*?\*\/)", RegexOptions.Compiled);
		internal static Regex regexNoNewLines   = new Regex(@"[^\n]", RegexOptions.Compiled);

		internal static MatchEvaluator evaluatorSharpLines = new MatchEvaluator(ReturnSharpLines);
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
				return "Str("+text+ ")+\" (THmsScriptMediaItem)\"#13+\"mpiTitle=\"+" + text + "[mpiTitle]+#13+\"mpiFilePath=\"+" + text + "[mpiFilePath]+#13+\"mpiThumbnail=\"+" + text + "[mpiThumbnail]+#13+\"mpiTimeLength=\"+" + text + "[mpiTimeLength]+#13+\"mpiCreateDate=\"+" + text + "[mpiCreateDate]";
			return text;
		}

		private static string successMethods4Eval = "|Match|ProgramVersion|HmsCryptCipherList|HmsCryptFormatList|HmsCryptHashList|HmsDataDirectory|HmsGetUserSearchText|HmsSubtitlesDirectory|HmsTempDirectory|ProgramPath|HmsClientID|HmsTranscodingInputParams|HmsTranscodingProbeParams|HmsTranscodingTempDirectory|HmsVlcVersion|".ToLower();
		private static bool OK4Evaluate(HMSItem item) {
			bool success = ((item.Kind == DefKind.Property) || (item.Kind == DefKind.Variable));
			if (!success && (item.Kind == DefKind.Function) || (item.Kind == DefKind.Method)) {
				success = successMethods4Eval.IndexOf("|"+item.MenuText.ToLower()+"|") >= 0;
			}
			return success;
		}

		/// <summary>
		/// Статическая процедура, вызываемая из MouseTimer_Task по срабатыванию MouseTimer для отображения подсказки при наведении мышкой на часть текста
		/// </summary>
		/// <param name="ActiveHMSEditor">Активный (вызвавший) элемент HMSEditor</param>
		public static void Task(HMSEditor ActiveHMSEditor) {
			if (ActiveHMSEditor.Locked) return;
			try {
				var    Editor = ActiveHMSEditor.Editor;
				Point  point  = ActiveHMSEditor.MouseLocation;
				int iStartLine = ActiveHMSEditor.Editor.YtoLineIndex(0);
				int CharHeight = ActiveHMSEditor.Editor.CharHeight;
				int i = point.Y / CharHeight;
				int iLine = iStartLine + i;
				if ((iLine + 1) > ActiveHMSEditor.Editor.LinesCount) return;
				Place place  = ActiveHMSEditor.PointToPlace(point);
				string line   = Editor.Lines[iLine];
				string value  = "";
				bool evalSelection = false;
				if (Editor.DebugMode && (Editor.SelectedText.Length > 2)) {
					Place selStart = Editor.Selection.Start;
					Place selEnd   = Editor.Selection.End;
					// Если указатель мыши в области виделения, то будем вычислять выдиление
					evalSelection  = (selStart.iLine == iLine) && (selStart.iChar >= place.iChar) && (selEnd.iChar <= iLine);
				}

				Range r = new Range(Editor, place, place);
				Range fragment = r.GetFragmentLookedLeft();
				string text = fragment.Text.Replace("#", "");
				if (text.Length == 0) return;
				
				HMSItem item = ActiveHMSEditor.GetHMSItemByText(text); // Поиск известного элемента HMSItem по части текста
				if (item != null && !string.IsNullOrEmpty(item.Text)) {
					point.Offset(0, Editor.CharHeight-4);
					// Если идёт отладка - проверяем, мы навели на переменную или свойство объекта?
					if (Editor.DebugMode && (evalSelection || OK4Evaluate(item)) ) {
						if (evalSelection) {
							text = Editor.SelectedText;
						} else {
							// проверяем, если это index свойство - то нудно вычислять значение с переданным индексом, поэтому дополняем значением [...]
							if (item.ImageIndex == Images.Enum) {
								Match m = Regex.Match(line, text + @"\[.*?\]");
								if (m.Success) text = m.Value;
							} else if (item.ImageIndex == Images.Function) {
								Match m = Regex.Match(line, text + @"\(.*?\)");
								if (m.Success) text = m.Value;
							}
							// Проверяем тип объекта класса, может быть удобней представить в виде текста? (TStrings или TJsonObject)
							text = CheckTypeForToStringRules(item.Type, text);
						}
						// Вычсиление выражения
						value = ActiveHMSEditor.EvalVariableValue(text);
						// Внедряемся в поток - показываем вплывающее окно со значением
						Editor.Invoke((System.Windows.Forms.MethodInvoker)delegate {
							ActiveHMSEditor.ValueHint.ShowValue(Editor, value, point);
						});
						return;
					} 
					// Показываем инофрмацию о функции или переменной через ToolTip
					Editor.Invoke((System.Windows.Forms.MethodInvoker)delegate {
						var tip = Editor.ToolTip;
						tip.ToolTipTitle = item.ToolTipTitle;
						tip.Help         = item.Help;
						tip.Value        = value;
						tip.Show(item.ToolTipText + " ", Editor, point, 10000);
					});
				}
			} catch (Exception e) {
				HMS.LogError(e.ToString());
			}
		}


	}
}
