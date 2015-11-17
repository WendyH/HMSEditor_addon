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
			aTitle = "Это заголовок";
			return IntPtr.Zero;
		}

		public IntPtr GetType(ref long aType) {
			aType = 1;
			return IntPtr.Zero;
		}

		public IntPtr InvalidateLine(int aLine) {
			return IntPtr.Zero;
		}

		public IntPtr Repaint() {
			return IntPtr.Zero;
		}

		public IntPtr SetCaretPos(int aLine, int aChar) {
			return IntPtr.Zero;
		}

		public IntPtr SetFocus() {
			return IntPtr.Zero;
		}

		public IntPtr SetScriptName(string aScriptName) {
			return IntPtr.Zero;
		}

		public IntPtr SetScriptText(string aText) {
			return IntPtr.Zero;
		}

		public IntPtr SetSelText(string aText) {
			return IntPtr.Zero;
		}

		public IntPtr Setup() {
			System.Windows.Forms.MessageBox.Show("Это Setup!");
			return IntPtr.Zero;
		}
	}

	static class Exports {
		[DllExport]
		public static void HmsGetClassObject([MarshalAs(UnmanagedType.Interface)]out IHmsScriptEditor instance) {
			instance = new EditorSample();
		}
	}
}
