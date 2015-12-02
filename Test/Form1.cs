using System;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using HmsAddons;

namespace test {

	public partial class Form1: Form {
		[DllImport("User32")]
		public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

		Guid clsid = new Guid();
		Guid iidAddonList    = new Guid("A8F688A7-441E-4701-9EA0-9C591D0B997A"); // guid by IHmsAddonList
		Guid iidScriptEditor = new Guid("B43BB779-379D-4244-A53D-0AAC3863A0FB"); // guid by IHmsScriptEditor
		IHmsAddonList    AddonList;
		IHmsScriptEditor ScriptEditor;
		IHmsScriptFrame  ScriptFrame  = new HmsScriptFrame();
		IntPtr           EditorHandle = IntPtr.Zero;

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
				assembly = Assembly.LoadFrom(@"D:\Projects\HMSEditor_addon\HMSEditor\bin\Debug\HMSEditor.dll");
				//assembly = Assembly.LoadFrom(@"D:\Projects\Test\HmsAddons.dll");
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
					object objAddonList;
					object objScriptEditor;
					Type type = assembly.GetType("HmsAddons.HmsAddonList");
					objAddonList = Activator.CreateInstance(type);
					if (objAddonList != null) {

						// Return a pointer to the objects IUnknown interface.
						IntPtr pIUnk = Marshal.GetIUnknownForObject(objAddonList);
						IntPtr pInterface;
						Int32 result = Marshal.QueryInterface(pIUnk, ref iidAddonList, out pInterface);

						//IHmsAddonList itf = (IHmsAddonList)pInterface;


						MethodInfo method = type.GetMethod("GetClassObject");
						object[] parameters = new object[] { clsid, iidScriptEditor, null };
						method.Invoke(objAddonList, parameters);
						objScriptEditor = parameters[2];
						if (objScriptEditor != null) {
							Type type2 = assembly.GetType("HmsAddons.HmsScriptEditor");
							MethodInfo method2 = type2.GetMethod("CreateEditor");
							object[] parameters2 = new object[] { Handle, null, 0, IntPtr.Zero };
							method2.Invoke(objScriptEditor, parameters2);
							EditorHandle = (IntPtr)parameters2[3];

							//MethodInfo method3 = type2.GetMethod("DestroyEditor");
							//object[] parameters3 = new object[] { EditorHandle };
							//method3.Invoke(objScriptEditor, parameters3);


						}

					}

				} catch (Exception ex) {
					sb.AppendLine(ex.ToString());

				}

			}
			richTextBox1.Text = "Проверка:\r\n" + sb.ToString();
			
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) {
			if (ScriptEditor!=null) {
				object scriptName = (object)comboBox1.Text;
                ScriptEditor.SetScriptName(ref scriptName);
			}
		}

		private void Form1_Resize(object sender, EventArgs e) {
			if (EditorHandle != IntPtr.Zero) {
				int left = 0;
				int top  = 0;
				int w = panel1.ClientSize.Width;
				int h = panel1.ClientSize.Height;
				SetWindowPos(EditorHandle, 0, left, top, w, h, 0);
			}
		}

		private void btnSetup_Click(object sender, EventArgs e) {

			if (ScriptEditor!=null)
				ScriptEditor.Setup();
		}
	}
}
