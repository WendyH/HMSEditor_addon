/* This code is released under WTFPL Version 2 (http://www.wtfpl.net/) * Created by WendyH. Copyleft. */
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO;
using FastColoredTextBoxNS;
using System.Threading;
using System.Windows.Forms;
using Ionic.Zip;
using System.Security.Permissions;

namespace HMSEditorNS {
	public static class Images {
		public const int Class     = 0;
		public const int Snippet   = 1;
		public const int Enum      = 2;
		public const int Field     = 3;
		public const int Function  = 5;
		public const int Keyword   = 6;
		public const int Method    = 7;
		public const int Procedure = 8;
		public const int Bookmark  = 9;
		public const int Constant  = 10;
		public const int Event     = 11;
	}

	public class HMSItem: AutocompleteItem {
		public string  Help      = "";
		public string  Type      = "";
		public bool    Global    = false;
		public DefKind Kind      = DefKind.NotDef;
		public string  Value     = "";
		public bool    IsClass   = false;
		public List<string> Params = new List<string>();
		public int PositionReal  = 0;
		public int PositionStart = 0;
		public int PositionEnd   = 0;
		public string  Filter    = "";

		// constructors
		public HMSItem() {
		}

		public HMSItem(string text) {
			Text = text;
		}

		public HMSItem(string text, int imageIndex)
			: this(text) {
			base.ImageIndex = imageIndex;
		}

		public HMSItem(string text, int imageIndex, string menuText)
			: this(text, imageIndex) {
			base.MenuText = menuText;
		}

		public HMSItem(string text, int imageIndex, string menuText, string toolTipTitle, string toolTipText)
			: this(text, imageIndex, menuText) {
			base.ToolTipTitle = toolTipTitle;
			base.ToolTipText  = toolTipText;
		}

		/// <summary>
		/// This method is called after item inserted into text
		/// </summary>
		public override void OnSelected(AutocompleteMenu popupMenu, SelectedEventArgs e) {
			if (e.Item.Text.IndexOf('^') < 0) return;
			e.Tb.BeginUpdate();
			e.Tb.Selection.BeginUpdate();
			//remember places
			var p1 = popupMenu.Fragment.Start;
			var p2 = e.Tb.Selection.Start;
			e.Tb.Selection.Start = p1;
			//move caret position right and find char ^
			while (e.Tb.Selection.CharBeforeStart != '^')
				if (!e.Tb.Selection.GoRightThroughFolded())
					break;
			//remove char ^
			e.Tb.Selection.GoLeft(true);
			e.Tb.InsertText("");
			//
			e.Tb.Selection.EndUpdate();
			e.Tb.EndUpdate();
		}

		public bool IsFuncOrProcedure { get { return ((Kind == DefKind.Function) || (Kind == DefKind.Procedure)); } }

	}

	public class SnippetHMSItem: HMSItem {
		public SnippetHMSItem(string snippet) {
			Text = snippet.Replace("\r", "");
			base.ToolTipTitle = "Шаблон:";
			base.ToolTipText  = Text;
		}

		public override string ToString() {
			return MenuText ?? Text.Replace("\n", " ").Replace("^", "");
		}

		public override string GetTextForReplace() {
			return Text;
		}

		public override void OnSelected(AutocompleteMenu popupMenu, SelectedEventArgs e) {
			e.Tb.BeginUpdate();
			e.Tb.Selection.BeginUpdate();
			//remember places
			var p1 = popupMenu.Fragment.Start;
			var p2 = e.Tb.Selection.Start;
			//do auto indent
			if (e.Tb.AutoIndent) {
				for (int iLine = p1.iLine + 1; iLine <= p2.iLine; iLine++) {
					e.Tb.Selection.Start = new Place(0, iLine);
					e.Tb.DoAutoIndent(iLine);
				}
			}
			e.Tb.Selection.Start = p1;
			//move caret position right and find char ^
			while (e.Tb.Selection.CharBeforeStart != '^')
				if (!e.Tb.Selection.GoRightThroughFolded())
					break;
			//remove char ^
			e.Tb.Selection.GoLeft(true);
			e.Tb.InsertText("");
			//
			e.Tb.Selection.EndUpdate();
			e.Tb.EndUpdate();
		}

		/// <summary>
		/// Compares fragment text with this item
		/// </summary>
		public override CompareResult Compare(string fragmentText) {
			if (Text.StartsWith(fragmentText, StringComparison.InvariantCultureIgnoreCase) &&
				   Text != fragmentText)
				return CompareResult.Visible;

			return CompareResult.Hidden;
		}
	}

	public class HMSClassInfo {
		public string Name = "";
		public string Type = "";
		public string Help = "";
		public AutocompleteItems MemberItems = new AutocompleteItems();
		public AutocompleteItems StaticItems = new AutocompleteItems();
	}

	public class HMSClasses: List<HMSClassInfo> {
		public bool ContainsName(string name) {
			name = name.Trim().ToLower();
			foreach (HMSClassInfo o in this) if (o.Name.ToLower() == name) return true;
			return false;
		}

		public HMSClassInfo this[string name] {
			get {
				name = name.Trim().ToLower();
				foreach (HMSClassInfo o in this) if (o.Name.ToLower() == name) return o;
				return new HMSClassInfo();
			}
		}

	}

	public class HMSItemComparer: IComparer<HMSItem> {
		private readonly string name;

		public HMSItemComparer(string name) {
			this.name = name.ToLower();
		}

		public int Compare(HMSItem item1, HMSItem item2) {
			return item1.MenuText.ToLower().CompareTo(name);
		}
	}

	public class AutocompleteItems: List<HMSItem> {
		public int LastEndPosition { get { if (Count > 0) return this[Count - 1].PositionEnd; return 0; } }

		public void SortByMenuText() {
			this.Sort( delegate (HMSItem a, HMSItem b) { return a.MenuText.CompareTo(b.MenuText); });
		}

		public HMSItem GetItemOrNull(string name) {
			name = name.Trim().ToLower();
			foreach (HMSItem o in this) if (o.MenuText.ToLower() == name) return o;
			return null;
		}

		public bool ContainsName(string name) {
			name = name.ToLower();
			foreach (var o in this) if (o.MenuText.ToLower() == name) return true;
			return false;
		}

		public HMSItem this[string name] {
			get {
				name = name.Trim().ToLower();
				foreach (HMSItem o in this) if (o.MenuText.ToLower() == name) return o;
				return new HMSItem();
			}
		}

		public AutocompleteItems GetFilteredList(string type) {
			AutocompleteItems list = new AutocompleteItems();
			type = type.ToLower();
			foreach (var item in this) if (item.Type.ToLower()==type) list.Add(item);
			return list;
		}

	}
	
	public enum DefKind { NotDef, Variable, Function, Procedure, Class, Method, Property, Constant, Event, Other }

	public class InvisibleCharsRenderer: Style {
		Pen pen;

		public InvisibleCharsRenderer(Pen pen) {
			this.pen = pen;
		}

		public override void Draw(Graphics gr, Point position, Range range) {
			var tb = range.tb;
			using (Brush brush = new SolidBrush(pen.Color))
				foreach (var place in range) {
					switch (tb[place].c) {
						case ' ':
							var point = tb.PlaceToPoint(place);
							point.Offset(tb.CharWidth / 2, tb.CharHeight / 2);
							gr.DrawLine(pen, point.X, point.Y, point.X + 1, point.Y);
							break;
					}

					if (tb[place.iLine].Count - 1 == place.iChar) {
						var point = tb.PlaceToPoint(place);
						point.Offset(tb.CharWidth, 0);
						gr.DrawString("¶", tb.Font, brush, point);
					}
				}
		}
	}

	public static class HMS {
		public static string GitHubHMSEditor  = "WendyH/HMSEditor_addon";
		public static string GitHubTemplates  = "WendyH/HMSEditor-Templates";
		public static int    MaxLogSize       = 1024 * 1024 * 2; // 2 MB
		public static string HmsTypesStringWithHelp = "|{Тип данных: целочисленное}Byte|Word|Integer|Longint|Cardinal|TColor|TColor32|{Тип данных: логический}Boolean|{Тип данных:  расширенный (с плавающей запятой)}Real|Single|Double|Extended|Currency|TDate|TTime|TDateTime|{Тип данных: символьный}Char|{Тип данных: строковый}String|{Тип данных: Variant (вариантный тип)}Pointer|Variant|{Тип данных: массив}Array|{}Nil|Null|True|False|";
		public static string HmsTypesString   = "";
		public static string KeywordsString   = "|#include|#define|new|break|continue|exit|delete|return|if|else|switch|default|case|do|while|for|try|finally|except|in|is|";
		public static string ClassesString    = "";
		public static string NotFoundedType   = "|TFloat|TSizeConstraints|THelpType|TMargins|TBasicAction|TBiDiMode|TDragKind|TDragMode|HDC|TFixed|TAutoComplete|TBevelEdges|TBevelKind|TBorderStyle|TImeMode|TScrollBarStyle|TPixelAccessMode|TArrayOfArrayOfFixedPoint|TArrayOfFixedPoint|TArrayOfArrayOfFloatPoint|TArrayOfFloatPoint|TFormBorderStyle|TDefaultMonitor|TIcon|TPadding|TPopupMode|TPrintScale|TEllipsisPosition|THotTrackStyles|TListItems|TMenuAutoFlag|TMenuItemAutoFlag|TMenuBreak|TOpenOptionsEx|TVerticalAlignment|TPopupAlignment|TMenuAnimation|TTrackButton|TArrayOfArrayOfArrayOfFixedPoint|TTBDrawingStyle|TEdgeBorders|TEdgeStyle|TGradientDirection|TTBGradientDrawingOptions|TPositionToolTip|TMultiSelectStyle|";
		public static string CurrentParamType = "";

		private static string ResourcePath = "HMSEditorNS.Resources.";
		private static string workingdir = "";
		internal static string WorkingDir {
			get {
				if (workingdir.Length == 0)
					workingdir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + DS + "HMSEditor";
				return workingdir;
			}
		}

		private static string downloaddir = "";
		internal static string DownloadDir {
			get { if (downloaddir.Length == 0) downloaddir = Path.GetTempPath(); return downloaddir; }
		}

		internal static char DS { get { return Path.DirectorySeparatorChar; } }

		public static string TemplatesDir { get { return WorkingDir + DS + "Templates" ; } }
		public static string ThemesDir    { get { return WorkingDir + DS + "Themes"    ; } }
		public static string ErrorLogFile { get { return WorkingDir + DS + "errors.log"; } }

		public static HMSClasses HmsClasses = new HMSClasses();
		public static HMSClasses HmsTypes   = new HMSClasses();
		public static PrivateFontCollection PFC = new PrivateFontCollection();

		public static AutocompleteItems ItemsClass    = new AutocompleteItems();
		public static AutocompleteItems ItemsConstant = new AutocompleteItems();
		public static AutocompleteItems ItemsFunction = new AutocompleteItems();
		public static AutocompleteItems ItemsVariable = new AutocompleteItems();
		public static AutocompleteItems ItemsBoolean  = new AutocompleteItems();

		private static Regex regexGetFromLineCmd  = new Regex(@"\s*?(\w.*?)(\||$)", RegexOptions.Compiled);
		private static Regex regexGetFromLineHelp = new Regex(@"\|(.*)"           , RegexOptions.Compiled);
		private static Regex regexGetFromLineType = new Regex(@"[^\|]*:\s*?(\w+)" , RegexOptions.Compiled);
		private static Regex regexGetFromLineName = new Regex(@"^\s*?(\w+)"       , RegexOptions.Compiled);

		private static Regex regexTypePascal  = new Regex(@":\s*?(\w+)\s*;?$"            , RegexOptions.Compiled);
		private static Regex regexTypeCPP     = new Regex(@"^(\w+)\s+\w+\s*?(\(|;|=|\.)" , RegexOptions.Compiled);
		private static Regex regexType        = new Regex(@":\s*?(\w+)"                  , RegexOptions.Compiled);

		public  static Templates Templates    = new Templates();
		private static System.Threading.Timer DownloadTimer = new System.Threading.Timer(DownloadTemplateUpdates_Task, null, Timeout.Infinite, Timeout.Infinite);
		private static bool initialized = false;

		[EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
		public static void Init() {
			if (initialized) return;
			initialized = true;

			// Заголовок для всех MessageBox
			HMSEditor.Title += " v" + Application.ProductVersion;

			// Всё норм, запускаемся. Для начала вставляем обработку события при неудачных зависимостях, а там загрузим внедрённые dll
			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

			try {
				Application.EnableVisualStyles();

				// Загружаем встроенные шрифты
				AddFontFromResource("RobotoMono-Regular.ttf");
				AddFontFromResource("Roboto-Regular.ttf");

				// Заполняем базу знаний функций, классов, встроенных констант и переменных...
				InitAndLoadHMSKnowledgeDatabase();
            } catch (Exception e) {
				LogError(e.ToString());

			}
		}

		public static void DownloadTemplates(string lastUpdateDate) {
			string tmpFile = DownloadDir + "HMSEditorTemplates.zip";
			GitHub.DownloadLegacyArchive(GitHubTemplates, tmpFile);
			if (ExtractZip(tmpFile, true)) {
				HMSEditor.Settings.Set("TemplateLastUpdate", lastUpdateDate, "Common");
				HMSEditor.Settings.Save();
				LoadTemplates();
			}
		}

		private static void DownloadTemplateUpdates_Task(object state) {
			string info = "";
			try {
				string lastUpdateStored = HMSEditor.Settings.Get("TemplateLastUpdate", "Common", "");
				string lastUpdateDate   = GitHub.GetRepoUpdatedDate(GitHubTemplates, out info);
				if (lastUpdateStored != lastUpdateDate) {
					DownloadTemplates(lastUpdateDate);
                }
			} catch (Exception e) {
				HMS.LogError(e.ToString());
			}
		}

		public static void LogError(string msg) {
			if (File.Exists(ErrorLogFile)) {
				FileInfo fileInfo = new FileInfo(ErrorLogFile);
				// Если он такой большой, значит забытый и не нужный - удаляем, чтобы начать всё заного
				if (fileInfo.Length > MaxLogSize)
					File.Delete(ErrorLogFile);
			}
			try {
				File.AppendAllText(ErrorLogFile, DateTime.Now.ToString() + " " + msg + "\n");
			} catch { }
        }

		internal static bool ExtractZip(string zipfile, bool excludeTopDir = false, bool deleteAfter = true) {
			bool success = false;
			try {
				if (File.Exists(zipfile)) {
					char[] separators = new char[] { '/', '\\' };
					using (ZipFile zip = ZipFile.Read(zipfile)) {
						ZipEntry[] entries = new ZipEntry[zip.Entries.Count];
						zip.Entries.CopyTo(entries, 0);
						for (int i = 0; i < entries.Length; i++) {
							var e = entries[i];
							if (excludeTopDir) {
								string filename = e.FileName;
								int index  = filename.IndexOfAny(separators) + 1;
								if (index >= filename.Length) continue;
								e.FileName = filename.Substring(index);
							}
							e.Extract(TemplatesDir, ExtractExistingFileAction.OverwriteSilently);
						}
					}
					success = true;
					if (deleteAfter) File.Delete(zipfile);
				}
			} catch (Exception e) {
				HMS.LogError(e.ToString());
			}
			return success;
		}

		public static string GetType(string pascalVarOrFunction) {
			Match m = regexType.Match(pascalVarOrFunction);
			if (m.Success) return m.Groups[1].Value;
			return "";
		}

		public static string GetVarTypePascalFormat(string cmd) {
			Match m = regexTypePascal.Match(cmd);
			if (m.Success) return m.Groups[1].Value;
			return "";
		}

		public static string GetVarTypeCPPFormat(string cmd) {
			cmd = HMSEditor.regexExcludeWords.Replace(cmd, "");
			Match m = regexTypeCPP.Match(cmd);
			if (m.Success) return m.Groups[1].Value;
			return "";
		}

		public static void AddFontFromResource(string fontName) {
			Assembly assembly = Assembly.GetExecutingAssembly();
			Stream fontStream = assembly.GetManifestResourceStream("HMSEditorNS.Resources."+ fontName);
			if (fontStream == null) { Console.WriteLine("No font with name "+fontName+" in the resource"); return; }

			byte[] fontdata = new byte[fontStream.Length];
			fontStream.Read(fontdata, 0, (int)fontStream.Length);
			fontStream.Close();
			unsafe {
				fixed (byte* pFontData = fontdata) {
					PFC.AddMemoryFont((System.IntPtr)pFontData, fontdata.Length);
				}
			}
		}

		internal static void CreateIfNotExistDirectory(string dir, bool resetWorkingDirIfError = false) {
			try {
				if (!Directory.Exists(WorkingDir))
					Directory.CreateDirectory(WorkingDir);
				if (!Directory.Exists(dir))
					Directory.CreateDirectory(dir);
			} catch (Exception e) {
				if (resetWorkingDirIfError) workingdir = "";
				LogError(e.ToString());
			}
		}

		public static void InitAndLoadHMSKnowledgeDatabase() {
			Themes.Init();

			CreateIfNotExistDirectory(WorkingDir, true);
			CreateIfNotExistDirectory(TemplatesDir);
			CreateIfNotExistDirectory(ThemesDir);

			// Загружаем базу данных знаний о HMS (классы, типы, функции и т.п.) из ресурсов
			HmsTypesString    = Regex.Replace(HmsTypesStringWithHelp, "{.*?}", "").ToLower();
			Assembly assembly = Assembly.GetExecutingAssembly();
			HMSItem  item     = null;
			bool     isStatic = false;

			ItemsBoolean.Add(new HMSItem() { Text = "True" , ImageIndex = Images.Constant, MenuText = "True" , Type = "Boolean" });
			ItemsBoolean.Add(new HMSItem() { Text = "False", ImageIndex = Images.Constant, MenuText = "False", Type = "Boolean" });
			
			Stream stream = null;
			try {
				// Load classes items
				stream = assembly.GetManifestResourceStream(ResourcePath + "hms_classes.txt");
				using (StreamReader reader = new StreamReader(stream)) {
					stream = null;
					ClassesString = "|";
					string line, name, cmd; HMSClassInfo hmsclass = null;
					line = reader.ReadLine();
					while (line != null) {
						if ((line.Trim().Length < 1) || (line.StartsWith("*"))) { line = reader.ReadLine(); continue; }
						int indent = line.Length - line.TrimStart().Length;

						item = GetHmsItemFromLine(line);
						if (indent == 0) {
							// it's Class
							if (!HmsClasses.ContainsName(item.Text)) {
								hmsclass = new HMSClassInfo();
								hmsclass.Name = item.Text;
								hmsclass.Type = item.Type;
								hmsclass.Help = item.Help;
								HmsClasses.Add(hmsclass);
								item.Kind         = DefKind.Class;
								item.ImageIndex   = Images.Class;
								item.ToolTipTitle = "Класс " + item.Text;
								item.IsClass      = true;
								ItemsClass.Add(item);
								ClassesString += hmsclass.Name.ToLower() + "|";
							}

						} else if (indent == 2) {
							// it's method or property of the class
							cmd = item.ToolTipTitle;
							if      (cmd.StartsWith("function" )) { item.ImageIndex = Images.Method; item.Kind = DefKind.Method   ; }
							else if (cmd.StartsWith("procedure")) { item.ImageIndex = Images.Method; item.Kind = DefKind.Procedure; }
							else if (cmd.StartsWith("property" )) { item.ImageIndex = Images.Field ; item.Kind = DefKind.Property ; }
							else if (cmd.StartsWith("index"    )) { item.ImageIndex = Images.Enum  ; item.Kind = DefKind.Property ; }
							else if (cmd.StartsWith("event"    )) { item.ImageIndex = Images.Event ; item.Kind = DefKind.Event    ; }
							name = Regex.Replace(cmd, @"^(function|procedure|property|index property|event)\s+", "");
							name = Regex.Match(name, @"\w+").Value.Trim();
							if (name.Length < 1) name += " ";
							item.Text         = name;
							item.MenuText     = name;
							if (item.ImageIndex == Images.Enum) item.Text = name + "[^]";
							else if (item.ImageIndex == Images.Method) {
								if (cmd.IndexOf('(')>0) item.Text = name + "(^)";
								//else                    item.Text = name + "()";
							}
							if (name.ToLower() == "create") {
								// hmm... only one static method
								isStatic = true;
								hmsclass.StaticItems.Add(item);
							} else {
								isStatic = false;
								hmsclass.MemberItems.Add(item);
							}
						} else if ((indent == 4) || (line[0] == '\t')) {
							// it's help for parameters of last method
							if (isStatic) {
								if (hmsclass.StaticItems.Count > 0) {
									item = hmsclass.StaticItems[hmsclass.StaticItems.Count - 1];
									item.Params.Add(StylishHelp(line));
								}
							} else {
								if (hmsclass.MemberItems.Count > 0) {
									item = hmsclass.MemberItems[hmsclass.MemberItems.Count - 1];
									item.Params.Add(StylishHelp(line));
								}
							}
						}
						line = reader.ReadLine();
					}
				}
				// For each Class look the derived class and add his methods (info1) and methods of derived class of derived class (info2)
				foreach (var classItem in HmsClasses) {
					if (classItem.Type.Length == 0) continue;        // if no type - skip
					HMSClassInfo info1 = HmsClasses[classItem.Type]; // get derived class
					if (info1.Name.Length == 0) continue;            // if no found - skip
					HMSClassInfo info2 = HmsClasses[info1.Type];     // get derived class of the derived class
					if (info2.Name.Length > 0) {
						foreach (var i2 in info2.MemberItems) if (!classItem.MemberItems.ContainsName(i2.MenuText)) classItem.MemberItems.Add(i2);
						foreach (var i2 in info2.StaticItems) if (!classItem.StaticItems.ContainsName(i2.MenuText)) classItem.StaticItems.Add(i2);
					}
					foreach (var i1 in info1.MemberItems) if (!classItem.MemberItems.ContainsName(i1.MenuText)) classItem.MemberItems.Add(i1);
					foreach (var i1 in info1.StaticItems) if (!classItem.StaticItems.ContainsName(i1.MenuText)) classItem.StaticItems.Add(i1);
					classItem.MemberItems.SortByMenuText();
					classItem.StaticItems.SortByMenuText();
				}

				// Load a built-in Types (Enumerates)
				stream = assembly.GetManifestResourceStream(ResourcePath + "hms_types.txt");
				using (StreamReader reader = new StreamReader(stream)) {
					stream = null; string line; HMSClassInfo hmsType = null;
					while ((line = reader.ReadLine()) != null) {
						if (line.StartsWith("*") || (line.Trim().Length == 0)) continue; // Skip comments and blank lines
						item = GetHmsItemFromLine(line);
						if (!HmsTypes.ContainsName(item.Text)) {
							hmsType = new HMSClassInfo();
							hmsType.Name = item.Text;
							hmsType.Type = item.Text;
							hmsType.Help = item.Help;
							string names = Regex.Match(line, @"\((.*?)\)").Groups[1].Value;
							foreach(string name in names.Split(',')) {
								item = new HMSItem();
								item.ImageIndex   = Images.Enum;
								item.Text         = name;
								item.MenuText     = name;
								item.ToolTipTitle = name;
								item.ToolTipText  = "Перечисление типа " + hmsType.Name;
								hmsType.MemberItems.Add(item);
							}
							HmsTypes.Add(hmsType);
							ClassesString += hmsType.Name.ToLower() + "|";
						}
					}
				}

			} catch (Exception e) {
				HMS.LogError(e.ToString());

			} finally {
				if (stream != null)
					stream.Dispose();
			}

			// Load a built-in Functions and Procedures items
			BuildAutocompleteItemsFromResourse(ResourcePath + "hms_func.txt", Images.Procedure, "", ItemsFunction, DefKind.Function);
			foreach(var itemFunc in ItemsFunction) { if (itemFunc.Type.Length > 0) itemFunc.ImageIndex = Images.Function; }

			// Load a built-in Variables
			BuildAutocompleteItemsFromResourse(ResourcePath + "hms_vars.txt"     , Images.Field, "Встроенная переменная", ItemsVariable, DefKind.Variable);
				
			// Load a built-in Constants
			BuildAutocompleteItemsFromResourse(ResourcePath + "hms_constants.txt", Images.Enum , "Встроенная константа" , ItemsConstant, DefKind.Constant);

			foreach(var info in HmsTypes) {
				foreach(var typeitem in info.MemberItems) {
					ItemsConstant.Add(typeitem);
				}
			}

			// Check the self
			NotFoundedType  = "|";
			HmsTypesString += "int|long|void|bool|float|";
			foreach (var q in HmsClasses)  { KnownType(q.Type); foreach (var a in q.MemberItems) KnownType(a.Type); }
			foreach (var q in ItemsFunction) KnownType(q.Type);
			foreach (var q in ItemsVariable) KnownType(q.Type);
			foreach (var q in ItemsConstant) KnownType(q.Type);

			string funcList = "";
			foreach (var q in ItemsFunction) funcList += "|"+q.MenuText;
			funcList = funcList.Substring(1).Replace("|Int|", "|Int\\(|");
            RegexHmsFunctions = new Regex(@"\b(" + funcList + @")\b", RegexOptions.IgnoreCase);

			ClassesString += NotFoundedType.ToLower();
			HmsTypesString += "";
		}
		public static Regex RegexHmsFunctions = null;


		private static bool KnownType(string type) {
			if (type.Length < 1) return true;
			string s = type.ToLower();
			bool   k = ((ClassesString.IndexOf("|" + s + "|") >= 0) || (HmsTypesString.IndexOf("|" + s + "|") >= 0) || (NotFoundedType.ToLower().IndexOf("|" + s + "|") >= 0));
			if (!k) NotFoundedType += type + "|";
			return k;
		}

		private static void BuildAutocompleteItemsFromResourse(string file, int imageIndex, string toolTipText, AutocompleteItems itemsList, DefKind kind) {
			string   section  = "";
			string   filter   = "";
			Assembly assembly = Assembly.GetExecutingAssembly();
			Stream   stream   = assembly.GetManifestResourceStream(file);
			try {
				using (StreamReader reader = new StreamReader(stream)) {
					stream = null; string line; HMSItem item = null; Match m;
					while ((line = reader.ReadLine()) != null) {
						m = Regex.Match(line, @"^\*\s*?\[(.*)\]"    ); if (m.Success) { section = m.Groups[1].Value.Trim(); continue; }
						m = Regex.Match(line, @"^\*sm\w+\s*?<(.*?)>"); if (m.Success) { filter  = m.Groups[1].Value.Trim(); continue; }
						if (filter == "-") continue;
                        if (line.StartsWith("*") || (line.Trim().Length == 0)) continue; // Skip comments and blank lines
						int indent = line.Length - line.TrimStart().Length;
						if (indent == 0) {
							item = GetHmsItemFromLine(line);
							item.ImageIndex  = imageIndex;
							item.ToolTipText = toolTipText + ((section.Length > 0) ? (" ("+ section + ")") : "");
							item.Kind        = kind;
							item.Filter      = filter;
                            if (kind == DefKind.Function) item.Kind = (item.Type.Length > 0) ? DefKind.Function : DefKind.Procedure;
                            itemsList.Add(item);
						} else if ((indent == 2) || (line[0] == '\t')) {
							// it's help for parameters of last method
							if (itemsList.Count > 0) {
								item = itemsList[itemsList.Count - 1];
								item.Params.Add(StylishHelp(line));
							}
						}
					}
				}

			} catch (Exception e) {
				HMS.LogError(e.ToString());

			} finally {
				if (stream != null)
					stream.Dispose();
			}
		}

		private static HMSItem GetHmsItemFromLine(string line) {
			HMSItem item = new HMSItem();
			item.Text         = regexGetFromLineName.Match(line).Groups[1].Value.Trim();
			item.Type         = regexGetFromLineType.Match(line).Groups[1].Value.Trim(); // All ok, if not success - value is empty string
			item.ToolTipTitle = regexGetFromLineCmd .Match(line).Groups[1].Value.Trim();
			item.ToolTipText  = "";
			item.MenuText     = item.Text;
			item.Help         = StylishHelp(regexGetFromLineHelp.Match(line).Groups[1].Value);
			if (item.ToolTipTitle.Length == 0) {
				item.ToolTipTitle = item.Text;
				if (item.Type.Length > 0 ) item.ToolTipTitle += ": " + item.Type;
			}
			return item;
		}

		private static string StylishHelp(string help) {
			help = help.Replace(@"\n", "\n").Trim();
			help = regexStringAndComments.Replace(help, "<s>$0</s>");
			return help;
		}

		private static Regex regexBracketsR = new Regex(@"(\(|\))", RegexOptions.Compiled);
		private static Regex regexBracketsQ = new Regex(@"(\[|\])", RegexOptions.Compiled);
		private static Regex regexStringAndComments = new Regex(@"""(\\""|[^""])*""|'(\\'|[^'])*'|(//.*|\/\*[\s\S]*?\*\/)", RegexOptions.Compiled | RegexOptions.Singleline);

		public static string GetTextWithoutBrackets(string text) {
			text = GetTextWithoutBlock(text, regexBracketsR, "(");
			return GetTextWithoutBlock(text, regexBracketsQ, "[");
		}

		public static string ReadTextFromResource(string file) {
			string text = "";
			Assembly assembly = Assembly.GetExecutingAssembly();
			Stream stream = assembly.GetManifestResourceStream(ResourcePath + file);
			try {
				using (StreamReader reader = new StreamReader(stream, Encoding.UTF8)) {
					stream = null;
					text = reader.ReadToEnd();
				}
			} finally {
				if (stream != null)
					stream.Dispose();
			}
			return text;
		}

		private static string GetTextWithoutBlock(string text, Regex regex, string startBlock) {
			var sb = new StringBuilder(text);
			var stack = new Stack<int>();
			int indexStart = 0, indexEnd = 0;

			foreach (Match m in regex.Matches(text)) {
				if (m.Value == startBlock) {
					stack.Push(m.Index);
				} else {
					indexStart = (stack.Count > 0) ? stack.Pop() : m.Index;
					indexEnd   = m.Index + 1;
				}
				if (stack.Count < 1) {
					if (indexEnd > indexStart) {
						int len = indexEnd - indexStart;
						sb.Remove(indexStart, len);
						sb.Insert(indexStart, " ".PadRight(len));
					}
				}

			}
			return sb.ToString();
		}

		public static bool TypeIsClass(string type) { return (ClassesString.IndexOf("|" + type.Trim().ToLower() + "|") >= 0); }

		#region Работа с шаблонами
		public static bool TemplatesIsLoading = false;
		public static void LoadTemplates() {
			// Если уже кто-то загружает шаблоны (другой поток) - ждём максимум 4 секунды
			int i = 10; while (TemplatesIsLoading) { if (i-- < 0) return; Thread.Sleep(400); }
				
			// load templates from resource
			INI ini = new INI();
			Assembly assembly = Assembly.GetExecutingAssembly();
			Stream stream = null;
			try {
				stream = assembly.GetManifestResourceStream("HMSEditorNS.Resources.hms_templates.txt");
				using (StreamReader reader = new StreamReader(stream)) {
					stream = null;
					ini.Text = reader.ReadToEnd();
				}

			} catch (Exception e) {
				HMS.LogError(e.ToString());

			} finally {
				if (stream != null)
					stream.Dispose();
			}
			
			TemplatesIsLoading = true; // Говорим какбе другим потокам, которые могут обновлять в фоне: "Стапэ - идёт загрузка и установка шаблонов".
			foreach (var lang in new Language[] { Language.CPPScript, Language.PascalScript, Language.BasicScript, Language.JScript }) {
				AddTemplatesFromIni(Templates, lang, ini);
				LoadTemplatesFromDirectory(Templates, lang, TemplatesDir + "/" + lang);
			}
			TemplatesIsLoading = false;
		}

		private static void LoadTemplatesFromDirectory(Templates parentItem, Language lang, string targetDirectory) {
			if (!Directory.Exists(targetDirectory)) return;
			string[] fileEntries = Directory.GetFiles(targetDirectory);
			foreach (string fileName in fileEntries) {
				string name = Path.GetFileNameWithoutExtension(fileName);
				string text = File.ReadAllText(fileName);
				parentItem.Set(lang, name, text);
			}

			string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
			foreach (string subdirectory in subdirectoryEntries) {
				string name = Path.GetFileName(subdirectory);
				LoadTemplatesFromDirectory(parentItem.Set(lang, name).ChildItems, lang, subdirectory);
			}
		}

		public static ToolStripItem SetTemplateMenuItem(ToolStripMenuItem menuItem, string name, string text) {
			ToolStripItem item;
			if (menuItem.DropDownItems.ContainsKey(name))
				item = menuItem.DropDownItems[name];
			else
				item = menuItem.DropDownItems.Add(name, Properties.Resources.Template_Application_16xLG);
			item.Name = name;
			item.AccessibleDescription = text;
			string hint = ""; int maxLines = 15;
			foreach (string line in item.AccessibleDescription.Split('\n')) {
				hint += line + '\n';
				if (maxLines-- < 0) { hint += "..."; break; }
			}
			item.ToolTipText = hint;
			return item;
		}

		private static void AddTemplatesFromIni(Templates parentItem, Language lang, INI ini) {
			string langString = lang.ToString();
			foreach (string section in ini.Dict.Keys) {
				if (!section.StartsWith(langString)) continue;
				TemplateItem item = new TemplateItem(lang);
				string name = section.Substring(langString.Length + 1).Trim();
				string text = ini.GetSectionText(section);
				parentItem.Set(lang, name, text);
			}
		}
		#endregion Работа с шаблонами

		public static string getHomePath() {
			// Not in .NET 2.0 System.Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			if (Environment.OSVersion.Platform == PlatformID.Unix)
				return Environment.GetEnvironmentVariable("HOME");

			return Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
		}

		internal static string getDownloadFolderPath() {
			if (System.Environment.OSVersion.Platform == System.PlatformID.Unix) {
				string pathDownload = System.IO.Path.Combine(getHomePath(), "Downloads");
				return pathDownload;
			}

			return System.Convert.ToString(Microsoft.Win32.Registry.GetValue(
					 @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders"
					, "{374DE290-123F-4565-9164-39C4925E467B}"
					, String.Empty
				)
			);
		}

		/// <summary>
		/// Функция, вызываемая при событии, в случае неудачного определения зависимостей (а оно произойдёт, поверьте). Тут мы это пытаемся исправить.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
			// Загрузка внедрённых библиотек (dll) из ресурсов в память
			byte[] buffer;
			string resource = "HMSEditorNS.Resources.Ionic.Zip.Reduced.dll"; // Мини-библиотека для работы с zip (ибо в .NET 2.0 нет поддержки zip файлов)
			Assembly assembly = Assembly.GetExecutingAssembly();
			using (System.IO.Stream stm = assembly.GetManifestResourceStream(resource)) {
				buffer = new byte[(int)stm.Length];
				stm.Read(buffer, 0, (int)stm.Length);
				return Assembly.Load(buffer);
			}
		}

	}
}

