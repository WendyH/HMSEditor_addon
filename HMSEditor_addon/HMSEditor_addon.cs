using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using HMS.Addons;           // Интерфейсы HMS

namespace HMSEditor_addon {

	[Guid("9A871DC9-D8ED-4790-A5AB-E1A825639F2B")]
	class HmsAddonList: IHmsAddonList {
		private int AddonsCount = 1;
		private HmsAddonInfo[] AddonInfoList = new HmsAddonInfo[] {
						new HmsAddonInfo() {
							ClassID          = Constatns.Guid(typeof(EditorSample)),
							InterfaceID      = Constatns.Guid(typeof(IHmsScriptEditor)),
							Title            = "HmsTestEditor",
							Description      = "HmsTestEditor description",
							RequiredVersion  = "2.03",
							CheckedOnVersion = "2.03"
						}
					};

		public uint GetCount(ref int aCount) {
			aCount = AddonsCount;
			return HRESULT.S_OK;
		}

		public uint GetAddonInfo(int aIndex, ref Guid aClassID, ref Guid aInterfaceID, ref string aTitle, ref string aDescription, ref string aRequiredVersion, ref string aCheckedOnVersion) {
			uint result = HRESULT.E_UNEXPECTED;
			if ((aIndex >= 0) && (aIndex < AddonsCount)) {
				HmsAddonInfo info = AddonInfoList[aIndex];
				aClassID          = info.ClassID;
				aInterfaceID      = info.InterfaceID;
				aTitle            = info.Title;
				aDescription      = info.Description;
				aRequiredVersion  = info.RequiredVersion;
				aCheckedOnVersion = info.CheckedOnVersion;
				result = HRESULT.S_OK;
			}
			return result;
		}

		public uint GetClassObject(ref Guid clsid, ref Guid iid, out object instance) {
			uint result = HRESULT.E_NOINTERFACE;
			instance = null;
			if (Constatns.IsEqualGUID(iid, typeof(IHmsScriptEditor))) {
				instance = new EditorSample();
				result = HRESULT.S_OK;

			}
			return result;
		}

	}

	[Guid("5384995C-F7A9-426A-9F2A-A03ADEFAD336")]
	class EditorSample: IHmsScriptEditor, IDisposable {
		private HmsEditBox      EditBox     = null;
		private string          ScriptName  = "";
		private HmsScriptMode   ScriptMode  = HmsScriptMode.smUnknown;
		private IHmsScriptFrame ScriptFrame = null;

		#region IDisposable Support
		private bool disposedValue = false; // Для определения избыточных вызовов

		protected virtual void Dispose(bool disposing) {
			if (!disposedValue) {
				if (disposing) {
					if (EditBox!=null) EditBox.Dispose();
				}
				ScriptFrame   = null;
				EditBox       = null;
				disposedValue = true;
			}
		}

		~EditorSample() {
			Dispose(false);
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion

		public uint AddMessage(object aMessage) {
			return HRESULT.E_NOTIMPL;
		}

		public uint CreateEditor(IntPtr aParent, IHmsScriptFrame aHmsScripter, int aScriptMode, ref IntPtr aEditor) {
			try {
				ScriptMode  = (HmsScriptMode)aScriptMode;
				ScriptFrame = aHmsScripter;
				EditBox     = new HmsEditBox(aParent);
				aEditor     = EditBox.Handle;
                                ScriptFrame.ChangeScriptName("Test");
			} catch {
				Dispose();
				return HRESULT.E_UNEXPECTED;
			}
			return HRESULT.S_OK;
		}

		public uint DestroyEditor(IntPtr aEditor) {
			Dispose();
			return HRESULT.S_OK;
		}

		public uint GetCapabilities(ref int aCapabilities) {
			try {
				aCapabilities = Constatns.ecEditor;

			} catch { return HRESULT.E_UNEXPECTED; }
			return HRESULT.S_OK;
		}

		public uint GetCaretPos(ref int aLine, ref int aChar) {
			if (EditBox != null) {
				EditBox.GetCaretPos(ref aLine, ref aChar);
				return HRESULT.S_OK;
			}
			return HRESULT.E_UNEXPECTED;
		}

		public uint GetCurrentLine(ref int aLine) {
			if (EditBox != null) {
				aLine = EditBox.GetCurrentLine();
				return HRESULT.S_OK;
			}
			return HRESULT.E_UNEXPECTED;
		}

		public uint GetDescription(ref string aDescription) {
			aDescription = "Hello!\r\nЭто описание на русском!";
			return HRESULT.S_OK;
		}

		public uint GetModified(ref bool aModified) {
			return HRESULT.E_NOTIMPL;
		}

		public uint GetScriptName(ref string aScriptName) {
			aScriptName = ScriptName;
			return HRESULT.S_OK;
		}

		public uint GetScriptText(ref string aText) {
			if (EditBox != null) {
				aText = EditBox.Text;
				return HRESULT.S_OK;
			}
			return HRESULT.E_UNEXPECTED;
		}

		public uint GetTitle(ref string aTitle) {
			return HRESULT.E_NOTIMPL;
		}

		public uint GetType(ref int aType) {
			return HRESULT.E_NOTIMPL;
		}

		public uint InvalidateLine(int aLine) {
			if (EditBox != null) {
				EditBox.Invalidate();
				return HRESULT.S_OK;
			}
			return HRESULT.E_UNEXPECTED;
		}

		public uint Repaint() {
			if (EditBox != null) {
				EditBox.Refresh();
				return HRESULT.S_OK;
			}
			return HRESULT.E_UNEXPECTED;
		}

		public uint SetCaretPos(int aLine, int aChar) {
			uint result = HRESULT.E_UNEXPECTED;
			if (EditBox != null) {
				try {
					EditBox.SetCaretPos(aLine, aChar);
					return HRESULT.S_OK;
				} catch { }
			}
			return result;
		}

		public uint SetFocus() {
			if (EditBox!=null) {
				EditBox.Focus();
				return HRESULT.S_OK;
			}
			return HRESULT.E_UNEXPECTED;
		}

		public uint SetScriptName(string aScriptName) {
			ScriptName = aScriptName;
			return HRESULT.S_OK;
		}

		public uint SetScriptText(string aText) {
			if (EditBox != null) {
				EditBox.Text = aText;
				return HRESULT.S_OK;
			}
			return HRESULT.E_UNEXPECTED;
		}

		public uint SetSelText(string aText) {
			if (EditBox != null) {
				EditBox.SelectedText = aText;
				return HRESULT.S_OK;
			}
			return HRESULT.E_UNEXPECTED;
		}

		public uint Setup() {
			MessageBox.Show("Это Setup!\nFScriptName = " + ScriptName);
			return HRESULT.S_OK;
		}

	}

	static class Exports {

		public static uint HmsGetClassObject(Guid clsid, Guid iid, [MarshalAs(UnmanagedType.Interface)]out object instance) {
			uint result = HRESULT.E_NOINTERFACE;
			instance = null;
			if (Constatns.IsEqualGUID(iid, typeof(IHmsAddonList))) {
				instance = new HmsAddonList();
				result = HRESULT.S_OK;

			} else if (Constatns.IsEqualGUID(iid, typeof(IHmsScriptEditor))) {
				instance = new EditorSample();
				result = HRESULT.S_OK;

			}
			return result;
		}

	}
}
