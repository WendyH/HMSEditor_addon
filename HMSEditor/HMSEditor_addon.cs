using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;
using HMSEditorNS;
using NativeMethods = HMSEditorNS.NativeMethods;

// ReSharper disable once CheckNamespace
namespace HmsAddons {

    [Guid("9A871DC9-D8ED-4790-A5AB-E1A825639F2B")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public class HmsAddonList: IHmsAddonList, IHmsAddonTools {
        private int AddonsCount = 1;
        private HmsAddonInfo[] AddonInfoList = new HmsAddonInfo[] {
                        new HmsAddonInfo() {
                            ClassID          = Constatns.Guid(typeof(HmsScriptEditor )),
                            InterfaceID      = Constatns.Guid(typeof(IHmsScriptEditor)),
                            Title            = HMSEditor.Title,
                            Description      = HMSEditor.Description,
                            RequiredVersion  = "2.03",
                            CheckedOnVersion = "3.00"
                        }
                    };

        public uint GetCount(out int aCount) {
            aCount = AddonsCount;
            return HRESULT.S_OK;
        }

        public uint GetAddonInfo(int aIndex, ref Guid aClassID, ref Guid aInterfaceID, ref object aTitle, ref object aDescription, ref object aRequiredVersion, ref object aCheckedOnVersion) {
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
                result   = HRESULT.S_OK;
            }
            return result;
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public uint CanUnloadNow() {
            if ((HMSEditor.NeedCopyDllFile != "") && (System.IO.File.Exists(HMSEditor.NeedCopyDllFile))) {
                MessageBox.Show("Будет установлена новая версия дополнения. Просканируйте список дополнений через несколько секунд.", HMSEditor.Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                AboutDialog.CopyNewFile();
            }
            return HRESULT.S_OK;
        }

        // from IHmsAddonTools
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public uint Setup(IntPtr aParent, ref int aReload) {
            AboutDialog about = new AboutDialog();
            about.ShowDialog();
            return HRESULT.S_OK;
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public uint Update(ref int aFlags, ref object aResult) {
            //MessageBox.Show("Update! aFlags="+ aFlags.ToString()+ " aResult=" + aResult.ToString());
            return HRESULT.E_NOTIMPL;
        }
    }

    [Guid("5384995C-F7A9-426A-9F2A-A03ADEFAD336")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public class HmsScriptEditor: IHmsScriptEditor, IDisposable {
        private HMSEditor EditBox;
        private bool      FirstSetPos;
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (EditBox != null && !EditBox.IsDisposed) EditBox.Dispose();
            }
            EditBox = null;
        }

        ~HmsScriptEditor() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
        }

        public void LoadSettings() {
            EditBox?.LoadSettings();
        }

        public void SaveSettings() {
            EditBox?.SaveSettings();
        }

        public uint AddMessage(ref object aMessage) {
            return HRESULT.E_NOTIMPL;
        }

        public uint CreateEditor(IntPtr aParent, IntPtr aScriptFrame, int aScriptMode, ref IntPtr aEditor) {
            try {
                EditBox = new HMSEditor(aScriptFrame, aScriptMode, aParent);
                LoadSettings();
                aEditor = EditBox.Handle;

            } catch (Exception e) {
                MessageBox.Show("Ошибка создания окна редактора.\n\n" + e, HMSEditor.Title);
                Dispose();
                return HRESULT.E_UNEXPECTED;
            }
            return HRESULT.S_OK;
        }

        public uint DestroyEditor(IntPtr aEditor) {
            EditBox.SaveSettings();
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
                EditBox.GetCaretPos(out aLine, out aChar);
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

        public uint GetModified(ref int aModified) {
            if (EditBox != null) {
                aModified = EditBox.Modified ? -1 : 0;
                return HRESULT.S_OK;
            }
            return HRESULT.E_UNEXPECTED;
        }

        public uint GetScriptName(ref object aScriptName) {
            if (EditBox != null) {
                aScriptName = EditBox.ScriptLanguage;
            }
            return HRESULT.S_OK;
        }

        public uint GetScriptText(ref object aText) {
            if (EditBox != null) {
                aText = EditBox.Text;
                return HRESULT.S_OK;
            }
            return HRESULT.E_UNEXPECTED;
        }

        public uint InvalidateLine(int aLine) {
            if (EditBox != null) {
                bool f = EditBox.TB.Focused;
                EditBox.CheckDebugState();
                EditBox.Invalidate();
                if (f) EditBox.TB.Select();
                return HRESULT.S_OK;
            }
            return HRESULT.E_UNEXPECTED;
        }

        public uint Repaint() {
            if (EditBox != null) {
                EditBox.CheckDebugState();
                EditBox.Refresh();
                return HRESULT.S_OK;
            }
            return HRESULT.E_UNEXPECTED;
        }

        public uint SetCaretPos(int aLine, int aChar) {
            uint result = HRESULT.E_UNEXPECTED;
            if (EditBox != null) {
                if (FirstSetPos) {
                    FirstSetPos = false;
                    EditBox.RestorePosition();
                } else {
                    EditBox.SetCaretPos(aLine, aChar);
                    if (EditBox.DebugMode) {
                        if (EditBox.ValueHint.IsShowing) {
                            EditBox.ValueHint.Value = EditBox.EvalVariableValue(EditBox.ValueHint.Expression);
                        }
                    }
                }
                return HRESULT.S_OK;
            }
            return result;
        }

        public uint SetFocus() {
            if (EditBox != null) {
                EditBox.CheckDebugState();
                EditBox.TB.Focus();
                EditBox.TB.Select();
                return HRESULT.S_OK;
            }
            return HRESULT.E_UNEXPECTED;
        }

        public uint SetRunning(int aValue) {
            if (EditBox != null) {
                EditBox.OnRunningStateChange(aValue < 0);
                EditBox.TB.Select();
            }
            return HRESULT.S_OK;
        }

        public uint SetScriptName(ref object aScriptName) {
            if (EditBox != null)
                EditBox.ScriptLanguage = (string)aScriptName;
            return HRESULT.S_OK;
        }

        public uint SetScriptText(ref object aText) {
            if (EditBox != null) {
                EditBox.SetText((string)aText);
                FirstSetPos = true;
                return HRESULT.S_OK;
            }
            return HRESULT.E_UNEXPECTED;
        }

        public uint SetSelText(ref object aText) {
            if (EditBox != null) {
                EditBox.SelectedText = (string)aText;
                return HRESULT.S_OK;
            }
            return HRESULT.E_UNEXPECTED;
        }

        public uint Setup() {
            EditBox?.HotKeysDialog();
            return HRESULT.S_OK;
        }

    }
}