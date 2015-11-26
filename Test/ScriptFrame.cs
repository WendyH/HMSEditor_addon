using HmsInterfaces; // Интерфейсы HMS (hmsInterfaces.dll)

namespace test {
	class HmsScriptFrame: IHmsScriptFrame {
		public System.Windows.Forms.RichTextBox LogTextBox;
		public void Log(string msg) {
			if (LogTextBox!=null) {
				LogTextBox.SelectionStart = LogTextBox.TextLength;
				LogTextBox.SelectedText   = msg + "\n";
				LogTextBox.ScrollToCaret();
			}
		}

		public uint AddWatch(string aExpression) {
			Log("AddWatch: aExpression=" + aExpression);
            return 0;
		}

		public uint ChangeScriptName(string aScriptName) {
			Log("ChangeScriptName: aScriptName=" + aScriptName);
			return 0;
		}

		public uint CompileScript(string aScriptName, string aScriptText, ref string aErrorMessage, ref int aErrorLine, ref int aErrorChar, ref bool aResult) {
			Log("CompileScript:");
			return 0;
		}

		public uint GenerateScriptDescriptions(ref string aXMLDescriptions) {
			Log("GenerateScriptDescriptions:");
			return 0;
		}

		public uint GetCurrentState(ref bool aRunning, ref bool aStepByStep, ref bool aStopped, ref int aCurrentSourceLine, ref int aCurrentSourceChar) {
			Log("GetCurrentState:");
			return 0;
		}

		public uint IsBreakpointLine(int aLine, ref bool aResult) {
			Log("IsBreakpointLine: aLine=" + aLine.ToString());
			return 0;
		}

		public uint IsExecutableLine(int aLine, ref bool aResult) {
			Log("IsExecutableLine: aLine=" + aLine.ToString());
			return 0;
		}

		public uint ProcessCommand(int aCommand) {
			Log("ProcessCommand: aCommand=" + aCommand.ToString());
			return 0;
		}

		public uint SolveExpression(string aExpression, ref string aResult) {
			Log("SolveExpression: aExpression=" + aExpression);
			return 0;
		}

		public uint ToggleBreakpoint(int aLine) {
			Log("ToggleBreakpoint: aLine=" + aLine.ToString());
			return 0;
		}
	}
}
