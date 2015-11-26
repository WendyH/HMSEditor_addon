using System;
using System.Runtime.InteropServices;
using HmsInterfaces; // Интерфейсы HMS (hmsInterfaces.dll)

namespace HMSEditorNS {

	[Guid("9A871DC9-D8ED-4790-A5AB-E1A825639F2B")]
	public class HmsAddonList: IHmsAddonList {
		private int AddonsCount = 1;
		private HmsAddonInfo[] AddonInfoList = new HmsAddonInfo[] {
						new HmsAddonInfo() {
							ClassID          = Constatns.Guid(typeof(HmsScriptEditor )),
							InterfaceID      = Constatns.Guid(typeof(IHmsScriptEditor)),
							Title            = HMSEditor.Title,
							Description      = HMSEditor.Description,
							RequiredVersion  = "2.03",
							CheckedOnVersion = "2.03"
						}
					};

		public HmsAddonList() {
			HMS.Init();
		}

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
				instance = new HmsScriptEditor();
				result = HRESULT.S_OK;

			}
			return result;
		}

	}

	[Guid("5384995C-F7A9-426A-9F2A-A03ADEFAD336")]
	public class HmsScriptEditor: IHmsScriptEditor, IDisposable {
		private HMSEditor EditBox = null;

		// Constructor
		public HmsScriptEditor() {
		}

		#region IDisposable Support
		private bool disposedValue = false; // Для определения избыточных вызовов

		protected virtual void Dispose(bool disposing) {
			if (!disposedValue) {
				if (disposing) {
					if (EditBox!=null) EditBox.Dispose();
				}
				EditBox       = null;
				disposedValue = true;
			}
		}

		~HmsScriptEditor() {
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
				EditBox     = new HMSEditor(aParent, aHmsScripter, (HmsScriptMode)aScriptMode);
				aEditor     = EditBox.Handle;

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
			aDescription = HMSEditor.Description;
			return HRESULT.S_OK;
		}

		public uint GetModified(ref bool aModified) {
			aModified = EditBox.Modified;
			return HRESULT.S_OK;
		}

		public uint GetScriptName(ref string aScriptName) {
			aScriptName = EditBox.ScriptLanguage;
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
			aTitle = HMSEditor.Title;
			return HRESULT.S_OK;
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
					EditBox.CheckDebugState();
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
			EditBox.ScriptLanguage = aScriptName;
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
			EditBox.HotKeysDialog();
			return HRESULT.S_OK;
		}

	}

}
