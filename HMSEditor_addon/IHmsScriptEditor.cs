using System;
using System.Runtime.InteropServices;

namespace HMS.Addons {

	// Основной интерфейс всех дополнений
	[ComVisible(true), Guid("5C75011A-1A44-47DD-A382-5D829DC24F28"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IHmsCustomAddon {
		IntPtr GetDescription(ref string aDescription);
		IntPtr GetTitle(ref string aTitle);
		IntPtr GetType(ref Int64 aType);
	}

	// Интерфейс редактора
	[ComVisible(true), Guid("B43BB779-379D-4244-A53D-0AAC3863A0FB"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IHmsScriptEditor: IHmsCustomAddon {
		//IntPtr CreateEditor (IntPtr aParent, IHmsScriptFrame aHmsScripter, ref IntPtr aEditor);
		IntPtr DestroyEditor(IntPtr aEditor);

		IntPtr GetCurrentLine(ref int aLine);
		IntPtr GetScriptName(ref string aScriptName);
		IntPtr GetScriptText(ref string aText);
		IntPtr SetScriptName(string aScriptName);
		IntPtr SetScriptText(string aText);

		IntPtr GetModified(ref bool aModified);

		IntPtr InvalidateLine(int aLine);

		IntPtr Repaint();

		IntPtr GetCaretPos(ref int aLine, ref int aChar);
		IntPtr SetCaretPos(int aLine, int aChar);

		IntPtr SetFocus();
		IntPtr SetSelText(string aText);

		IntPtr Setup();
	}

}
