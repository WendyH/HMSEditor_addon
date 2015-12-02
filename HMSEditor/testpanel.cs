using System.Windows.Forms;

namespace HMSEditorNS {
	public static class testpanel {
		public static void AddWatch() {
			var HmsScriptFrame = HMSEditor.ActiveEditor.HmsScriptFrame;
			object ob1 = "mpFilePath";

			HmsScriptFrame.AddWatch(ref ob1);

			MessageBox.Show("Test ok - " + ob1.ToString());
		}

		public static void ChangeScriptName() {
			var HmsScriptFrame = HMSEditor.ActiveEditor.HmsScriptFrame;
			object ob1;
			ob1 = (object)"Нет скрипта";

			HmsScriptFrame.ChangeScriptName(ref ob1);

			MessageBox.Show("Test ok - " + ob1.ToString());
		}

		public static void CompileScript() {
			var Editor = HMSEditor.ActiveEditor;
			var HmsScriptFrame = Editor.HmsScriptFrame;
			object obScriptName = Editor.ScriptLanguage;
			object obScriptText = Editor.Text;
			object obErrorMessage = "";
			int errorLine = 0, errorChar = 0, result = 0;

			HmsScriptFrame.CompileScript(ref obScriptName, ref obScriptText, ref obErrorMessage, ref errorLine, ref errorChar, ref result);

			MessageBox.Show("Test ok.\nobScriptName=" + obScriptName.ToString() + "\nobErrorMessage=" + obErrorMessage.ToString() + "\nerrorLine=" + errorLine.ToString() + "\nerrorChar=" + errorChar.ToString() + "\nresult=" + result.ToString());
		}

		public static void GenerateScriptDescriptions() {
			var HmsScriptFrame = HMSEditor.ActiveEditor.HmsScriptFrame;
			object ob1 = "";
			HmsScriptFrame.GenerateScriptDescriptions(ref ob1);
			string text = ob1.ToString();
			MessageBox.Show("Test ok - " + text.Substring(0, 50) + "...");
		}

		public static void GetCurrentState() {
			var HmsScriptFrame = HMSEditor.ActiveEditor.HmsScriptFrame;
			int running = 0, iLine = 0, iChar = 0;
			HmsScriptFrame.GetCurrentState(ref running, ref iLine, ref iChar);
			bool isrunning = (running < 0);
			MessageBox.Show("Test ok - running=" + isrunning + " iLine=" + iLine + " iChar=" + iLine);
		}

		public static void IsBreakpointLine() {
			var HmsScriptFrame = HMSEditor.ActiveEditor.HmsScriptFrame;
			int result = 0;
			int line = 1;
			HmsScriptFrame.IsBreakpointLine(line, ref result);
			bool bResult = (result < 0);
			MessageBox.Show("Test ok - line=" + line + " result=" + bResult);
		}

		public static void IsExecutableLine() {
			var HmsScriptFrame = HMSEditor.ActiveEditor.HmsScriptFrame;
			int result = 0;
			int line = 1;
			HmsScriptFrame.IsExecutableLine(line, ref result);
			bool bResult = (result < 0);
			MessageBox.Show("Test ok - line=" + line + " result=" + bResult);
		}

		public static void ProcessCommand() {
			var HmsScriptFrame = HMSEditor.ActiveEditor.HmsScriptFrame;
			int command = HmsAddons.Constatns.ecEvaluate;
			HmsScriptFrame.ProcessCommand(command);
			MessageBox.Show("Test ok - (ecEvaluate) command=" + command);
		}

		public static void SolveExpression() {
			var HmsScriptFrame = HMSEditor.ActiveEditor.HmsScriptFrame;
			object expression = "mpFilePath";
			object result = "";
			HmsScriptFrame.SolveExpression(ref expression, ref result);
			MessageBox.Show("Test ok - expression=" + expression + " result=" + result);
		}

		public static void ToggleBreakpoint() {
			var Editor = HMSEditor.ActiveEditor;
			var HmsScriptFrame = Editor.HmsScriptFrame;
			int line = 1;
			//HmsScriptFrame.ToggleBreakpoint(line);
			Editor.ToggleBreakpoint(line);
			MessageBox.Show("Test ok - line=" + line);
		}
	}
}
