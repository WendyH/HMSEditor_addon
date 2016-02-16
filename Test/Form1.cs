using System;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using HmsAddons;
using System.IO;

namespace test {

    public partial class Form1: Form {

        Guid clsid = new Guid();
        Guid iidAddonList    = new Guid("A8F688A7-441E-4701-9EA0-9C591D0B997A"); // guid by IHmsAddonList
        Guid iidScriptEditor = new Guid("B43BB779-379D-4244-A53D-0AAC3863A0FB"); // guid by IHmsScriptEditor
        IHmsScriptFrame  ScriptFrame  = new HmsScriptFrame();
        IntPtr           EditorHandle = IntPtr.Zero;
        object objAddonList     = null;
        object objScriptEditor  = null;
        Type   typeScriptEditor = null;
		
        public Form1() {
            InitializeComponent();
            comboBox1.Items.Add("C++Script"   );
            comboBox1.Items.Add("PascalScript");
            comboBox1.Items.Add("BasicScript" );
            comboBox1.Items.Add("JScript"     );
            comboBox1.Items.Add("Нет скрипта" );
            comboBox1.Text = "C++Script";
            //ScriptFrame.LogTextBox = LogTextBox;
        }

        private void Form1_Load(object sender, EventArgs e) {
            StringBuilder sb = new StringBuilder();
            Assembly assembly = null;
            try {
#if (DEBUG)
                string filedll = @"D:\Projects\HMSEditor_addon\HMSEditor\bin\Debug\HMSEditor.dll";
                if (!File.Exists(filedll)) filedll = @"D:\Projects\GitHub\HMSEditor_addon\HMSEditor\bin\Debug\HMSEditor.dll";
                assembly = Assembly.LoadFrom(filedll);
#else
                assembly = Assembly.LoadFrom("HMSEditor.dll");
#endif
            } catch (Exception ex) {
                sb.AppendLine(ex.ToString());

            }
            if (assembly != null) {
                
                // Цикл сбора информации о существующих классах и методах
                Type[] types = assembly.GetTypes();
                foreach (Type t in types) {
                    if (t.Namespace != "HmsAddons") continue;
                    sb.AppendLine(t.ToString());
                    MethodInfo[] methods = t.GetMethods(BindingFlags.Public);
                    foreach (MethodInfo info in methods) {
                        sb.AppendLine("   " + info.ToString());
                    }
                }
                
                try {
                    

                    // Рефлексия
                    Type type = assembly.GetType("HmsAddons.HmsAddonList");
                    objAddonList = Activator.CreateInstance(type);
                    if (objAddonList != null) {

                        // Return a pointer to the objects IUnknown interface.
                        IntPtr pIUnk = Marshal.GetIUnknownForObject(objAddonList);
                        IntPtr pInterface = IntPtr.Zero;
                        Int32 result = Marshal.QueryInterface(pIUnk, ref iidAddonList, out pInterface);

                        //object obj = Marshal.GetTypedObjectForIUnknown(pInterface, typeof(IHmsAddonList));
                        //IHmsAddonList itf = (IHmsAddonList)obj;


                        MethodInfo method = type.GetMethod("GetClassObject");
                        object[] parameters = new object[] { clsid, iidScriptEditor, null };
                        method.Invoke(objAddonList, parameters);
                        objScriptEditor = parameters[2];
                        if (objScriptEditor != null) {
                            typeScriptEditor = assembly.GetType("HmsAddons.HmsScriptEditor");
                            MethodInfo method2 = typeScriptEditor.GetMethod("CreateEditor");
                            object[] parameters2 = new object[] { panel1.Handle, null, 0, IntPtr.Zero };
                            method2.Invoke(objScriptEditor, parameters2);
                            EditorHandle = (IntPtr)parameters2[3];

                            Form1_Resize(null, EventArgs.Empty);

                            string file = @"D:\tesst2.cpp";
                            if (System.IO.File.Exists(file)) {
                                string text = System.IO.File.ReadAllText(file);
                                MethodInfo method3 = typeScriptEditor.GetMethod("SetScriptText");
                                method3.Invoke(objScriptEditor, new object[] { text });
                            }
                        }

                    }

                } catch (Exception ex) {
                    sb.AppendLine(ex.ToString());

                }

            }
            richTextBox1.Text = "Проверка:\r\n" + sb.ToString();
            
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) {
            if (typeScriptEditor!=null) {
                string text = comboBox1.Text;
                MethodInfo method = typeScriptEditor.GetMethod("SetScriptName");
                method.Invoke(objScriptEditor, new object[] { text });
            }
        }

        private void Form1_Resize(object sender, EventArgs e) {
            if (EditorHandle != IntPtr.Zero) {
                int left = 0;
                int top  = 0;
                int w = panel1.ClientSize.Width;
                int h = panel1.ClientSize.Height;
                NativeMethods.SetWindowPos(EditorHandle, 0, left, top, w, h, 0);
            }
        }

        private void btnSetup_Click(object sender, EventArgs e) {
            if (typeScriptEditor != null) {
                MethodInfo method = typeScriptEditor.GetMethod("Setup");
                method.Invoke(objScriptEditor, new object[] {});
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e) {
            try {
                MethodInfo methodSaveSettings = typeScriptEditor.GetMethod("SaveSettings");
                methodSaveSettings.Invoke(objScriptEditor, new object[] { });
            }
            catch { }
        }
    }
}
