using System;
using System.Runtime.InteropServices;
using HMS.Addons;
using RGiesecke.DllExport;

namespace HMSEditor_addon {

	public class EditorSample: IHmsScriptEditor {

		public IntPtr DestroyEditor(IntPtr aEditor) {
			return IntPtr.Zero;
        }

		public IntPtr GetCaretPos(ref int aLine, ref int aChar) {
			aLine = 14;
			aChar = 4;
            return IntPtr.Zero;
		}

		public IntPtr GetCurrentLine(ref int aLine) {
			aLine = 10;
			return IntPtr.Zero;
		}

		public IntPtr GetDescription(ref string aDescription) {
			aDescription = "Hello! Это описание на русском!";
			return IntPtr.Zero;
		}

		public IntPtr GetModified(ref bool aModified) {
			aModified = true;
			return IntPtr.Zero;
		}

		public IntPtr GetScriptName(ref string aScriptName) {
			aScriptName = "TestEditorrr.cpp";
			return IntPtr.Zero;
		}

		public IntPtr GetScriptText(ref string aText) {
			aText = "{ ShowMessage('Привет'); }";
			return IntPtr.Zero;
		}

		public IntPtr GetTitle(ref string aTitle) {
			throw new NotImplementedException();
		}

		public IntPtr GetType(ref long aType) {
			throw new NotImplementedException();
		}

		public IntPtr InvalidateLine(int aLine) {
			throw new NotImplementedException();
		}

		public IntPtr Repaint() {
			throw new NotImplementedException();
		}

		public IntPtr SetCaretPos(int aLine, int aChar) {
			throw new NotImplementedException();
		}

		public IntPtr SetFocus() {
			throw new NotImplementedException();
		}

		public IntPtr SetScriptName(string aScriptName) {
			throw new NotImplementedException();
		}

		public IntPtr SetScriptText(string aText) {
			throw new NotImplementedException();
		}

		public IntPtr SetSelText(string aText) {
			throw new NotImplementedException();
		}

		public IntPtr Setup() {
			throw new NotImplementedException();
		}
	}

	static class Exports {
		[DllExport]
		public static void HmsGetClassObject([MarshalAs(UnmanagedType.Interface)]out IHmsScriptEditor instance) {
			instance = new Sample { Name = "Test" };
		}
	}
}
