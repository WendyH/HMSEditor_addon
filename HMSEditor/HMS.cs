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
using System.ComponentModel;
using System.Globalization;

namespace HMSEditorNS {
    
    public static class HMS {
        static HMS() {
            try {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
            }
            catch {
                // ignored
            }

            try {
                // Для начала вставляем обработку события при неудачных зависимостях, а там загрузим внедрённые dll
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                Regex.CacheSize = 30;

                // Загружаем встроенные шрифты
                AddFontFromResource("RobotoMono-Regular.ttf");
                AddFontFromResource("Roboto-Regular.ttf");

                // Заполняем базу знаний функций, классов, встроенных констант и переменных...
                InitAndLoadHMSKnowledgeDatabase();

                HMS.AllowPrepareFastDraw = true;
                PrepareFastDrawInBackground(); 

            } catch (Exception e) {
                LogError(e.ToString());

            }
        }

        public static string GitHubHMSEditor  = "WendyH/HMSEditor_addon";
        public static string GitHubTemplates  = "WendyH/HMSEditor-Templates";
        public static int    MaxLogSize       = 1024 * 1024 * 2; // 2 MB
        public static bool   NewVersionChecked;
        public static bool   NewVersionExist;
        public static string HmsTypesStringWithHelp = "|{Тип данных: целочисленное}Byte|Word|Integer|Longint|Cardinal|TColor|TColor32|{Тип данных: логический}Boolean|{Тип данных:  расширенный (с плавающей запятой)}Real|Single|Double|Extended|Currency|TDate|TTime|TDateTime|{Тип данных: символьный}Char|{Тип данных: строковый}String|{Тип данных: Variant (вариантный тип)}Pointer|Variant|{Тип данных: массив}Array|";
        public static string HmsTypesString   = "";
        public static string KeywordsString   = "|#include|#define|new|break|continue|exit|delete|return|if|else|switch|default|case|do|while|for|try|finally|except|in|is|";
        public static string Keywords         = "";
        public static string ClassesString    = "";
        public static string NotFoundedType   = "|TFloat|TSizeConstraints|THelpType|TMargins|TBasicAction|TBiDiMode|TDragKind|TDragMode|HDC|TFixed|TAutoComplete|TBevelEdges|TBevelKind|TBorderStyle|TImeMode|TScrollBarStyle|TPixelAccessMode|TArrayOfArrayOfFixedPoint|TArrayOfFixedPoint|TArrayOfArrayOfFloatPoint|TArrayOfFloatPoint|TFormBorderStyle|TDefaultMonitor|TIcon|TPadding|TPopupMode|TPrintScale|TEllipsisPosition|THotTrackStyles|TListItems|TMenuAutoFlag|TMenuItemAutoFlag|TMenuBreak|TOpenOptionsEx|TVerticalAlignment|TPopupAlignment|TMenuAnimation|TTrackButton|TArrayOfArrayOfArrayOfFixedPoint|TTBDrawingStyle|TEdgeBorders|TEdgeStyle|TGradientDirection|TTBGradientDrawingOptions|TPositionToolTip|TMultiSelectStyle|";
        public static string CurrentParamType = "";
        public static string UpdateInfo       = "";
        public static Color  BordersColor     = Color.FromArgb(255, 70, 119, 207);

        private static string ResourcePath = "HMSEditorNS.Resources.";
        private static string _workingdir  = "";
        internal static string WorkingDir {
            get {
                if (_workingdir.Length == 0) {
                    _workingdir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\HMSEditor\";
                }
                return _workingdir;
            }
        }

        private static string _downloaddir = "";
        internal static string DownloadDir {
            get { if (_downloaddir.Length == 0) _downloaddir = Path.GetTempPath(); return _downloaddir; }
        }

        public static bool AllowPrepareFastDraw = false;
        public static void PrepareFastDrawInBackground() {
            if (AllowPrepareFastDraw && !Worker.IsBusy) {
                Worker.DoWork             += Worker_DoWork;
                Worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
                Worker.RunWorkerAsync(BackgraundTask.PrepareFastDraw);
            }

        }

        public static string AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public static string TemplatesDir => WorkingDir + "Templates";
        public static string ThemesDir    => WorkingDir + "Themes";
        public static string SettingsFile => WorkingDir + "HMSEditor.ini";
        public static string ErrorLogFile => WorkingDir + "errors.log";
        public static string DockingsFile => WorkingDir + "layout.xml";

        public static HMSClasses HmsClasses = new HMSClasses();
        public static HMSClasses HmsTypes   = new HMSClasses();
        public static PrivateFontCollection PFC = new PrivateFontCollection();

        public static AutocompleteItems ItemsClass    = new AutocompleteItems();
        public static AutocompleteItems ItemsConstant = new AutocompleteItems();
        public static AutocompleteItems ItemsFunction = new AutocompleteItems();
        public static AutocompleteItems ItemsVariable = new AutocompleteItems();
        public static AutocompleteItems ItemsBoolean  = new AutocompleteItems();

        private static Regex regexGetFromLineCmd  = new Regex(@"\s*?(\w.*?)(\||$)");
        private static Regex regexGetFromLineHelp = new Regex(@"\|(.*)"           );
        private static Regex regexGetFromLineType = new Regex(@"[^\|]*:\s*?(\w+)" );
        private static Regex regexGetFromLineName = new Regex(@"^\s*?([\w-]+)"    );

        private static Regex regexTypePascal  = new Regex(@":\s*?(\w+)\s*;?$"            );
        private static Regex regexTypeCPP     = new Regex(@"^(\w+)\s+\w+\s*?(\(|;|=|\.)" );
        private static Regex regexType        = new Regex(@":\s*?(\w+)"                  );

        public  static Templates        Templates = new Templates();
        private static BackgroundWorker Worker    = new BackgroundWorker();

        public static bool BaseInitialized;

        enum BackgraundTask { None = 0, PrepareFastDraw }

        public static void Msg(string msg) {
            MessageBox.Show(msg, HMSEditor.Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void Err(string msg) {
            MessageBox.Show(msg, HMSEditor.Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private static void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            if (e.Error != null) { HMS.LogError(e.Error.ToString()); }
            if (e.Cancelled) return;
            BackgraundTask taskType = BackgraundTask.None;
            if (e.Result != null) taskType = (BackgraundTask)e.Result;
            if (taskType == BackgraundTask.PrepareFastDraw) {
                BaseInitialized = true;
            }
        }

        private static void PrepareFastDraw() {
            IntPtr desktopPtr = NativeMethods.GetDC(IntPtr.Zero);
            Graphics g = Graphics.FromHdc(desktopPtr);
            try {
                foreach (var item in ItemsFunction) HmsToolTip.PrepareFastDraw(item, g);
                foreach (var item in ItemsVariable) HmsToolTip.PrepareFastDraw(item, g);
                foreach (var item in ItemsConstant) HmsToolTip.PrepareFastDraw(item, g);
                lock (ItemsClass) {
                    foreach(var item in ItemsClass) HmsToolTip.PrepareFastDraw(item, g);
                }
            } finally {
                g.Dispose();
                NativeMethods.ReleaseDC(IntPtr.Zero, desktopPtr);
            }
        }


        private static void Worker_DoWork(object sender, DoWorkEventArgs e) {
            BackgraundTask taskType = (BackgraundTask)e.Argument;
            e.Result = taskType;
            if (taskType == BackgraundTask.PrepareFastDraw) {
                PrepareFastDraw();
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

        public static void LogError(string msg) {
            if (File.Exists(ErrorLogFile)) {
                FileInfo fileInfo = new FileInfo(ErrorLogFile);
                // Если он такой большой, значит забытый и не нужный - удаляем, чтобы начать всё заного
                if (fileInfo.Length > MaxLogSize)
                    File.Delete(ErrorLogFile);
            }
            try {
                File.AppendAllText(ErrorLogFile, $"{DateTime.Now.ToString(CultureInfo.CurrentCulture)} {msg}\n");
            }
            catch {
                // ignored
            }
        }

        public static void LogError(Exception e) {
            string msg = e.ToString();
            MessageBox.Show(msg);
            if (File.Exists(ErrorLogFile)) {
                FileInfo fileInfo = new FileInfo(ErrorLogFile);
                // Если он такой большой, значит забытый и не нужный - удаляем, чтобы начать всё заного
                if (fileInfo.Length > MaxLogSize)
                    File.Delete(ErrorLogFile);
            }
            try {
                File.AppendAllText(ErrorLogFile, $"{DateTime.Now.ToString(CultureInfo.CurrentCulture)} {msg}\n");
            } catch {
                // ignored
            }
        }
        /// <summary>
        /// Exctract file(s) from zip to specific path
        /// </summary>
        /// <param name="zipfile">Path existing to zip file</param>
        /// <param name="path">Path exctact to</param>
        /// <param name="criteria">Example: name = *.xml  and  mtime > 2009-01-15</param>
        /// <returns></returns>
        internal static bool ExtractZipTo(string zipfile, string path, string criteria = "*") {
            bool success = false;
            try {
                if (File.Exists(zipfile)) {
                    using (ZipFile zip = ZipFile.Read(zipfile)) {
                        zip.ExtractSelectedEntries(criteria, null, path, ExtractExistingFileAction.OverwriteSilently);
                    }
                    success = true;
                }
            } catch (Exception e) {
                LogError(e.ToString());
            }
            return success;
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
                LogError(e.ToString());
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
            cmd = CodeAnalysis.RegexExcludeWords.Replace(cmd, "");
            Match m = regexTypeCPP.Match(cmd);
            if (m.Success) return m.Groups[1].Value;
            return "";
        }

        public static void AddFontFromResource(string fontName) {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var fontStream = assembly.GetManifestResourceStream("HMSEditorNS.Resources."+ fontName);
            if (fontStream == null) { Console.WriteLine($"No font with name {fontName} in the resource"); return; }

            byte[] fontdata = new byte[fontStream.Length];
            fontStream.Read(fontdata, 0, (int)fontStream.Length);
            fontStream.Close();
            unsafe {
                fixed (byte* pFontData = fontdata) {
                    PFC.AddMemoryFont((IntPtr)pFontData, fontdata.Length);
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
                if (resetWorkingDirIfError) _workingdir = "";
                LogError(e.ToString());
            }
        }

        public static void CheckNewVersion() {
            if (NewVersionChecked) return;
            string updatesInfo;
            string lastVersion = GitHub.GetLatestReleaseVersion(GitHubHMSEditor, out updatesInfo);
            GitHub.CompareVersions(lastVersion, AssemblyVersion);
            if (lastVersion.Length > 0) {
                NewVersionExist = true;
                UpdateInfo = updatesInfo;
            }
            NewVersionChecked = true;
        }

        public static void InitItemsBoolean(bool lowerCase = false) {
            ItemsBoolean.Clear();
            ItemsBoolean.Add(new HMSItem() { Text = lowerCase ? "true"  : "True" , ImageIndex = ImagesIndex.Constant, MenuText = lowerCase ? "true"  : "True" , Type = "Boolean" });
            ItemsBoolean.Add(new HMSItem() { Text = lowerCase ? "false" : "False", ImageIndex = ImagesIndex.Constant, MenuText = lowerCase ? "false" : "False", Type = "Boolean" });
        }

        public static void InitAndLoadHMSKnowledgeDatabase() {

            CreateIfNotExistDirectory(WorkingDir, true);
            CreateIfNotExistDirectory(TemplatesDir);
            CreateIfNotExistDirectory(ThemesDir);
            InitItemsBoolean();

            // Загружаем базу данных знаний о HMS (классы, типы, функции и т.п.) из ресурсов
            HmsTypesString = Regex.Replace(HmsTypesStringWithHelp, "{.*?}", "").ToLower();
            Assembly assembly = Assembly.GetExecutingAssembly();
            bool     isStatic = false;
            lock (HmsClasses) {
                Stream stream = null;
                try {
                    // Load classes items
                    HMSItem item;
                    stream = assembly.GetManifestResourceStream(ResourcePath + "hms_classes.txt");
                    if (stream != null) {
                        using (var reader = new StreamReader(stream)) {
                            stream = null;
                            ClassesString = "|";
                            HMSClassInfo hmsclass = null;
                            var line = reader.ReadLine();
                            while (line != null) {
                                if ((line.Trim().Length < 1) || (line.StartsWith("*"))) { line = reader.ReadLine(); continue; }
                                int indent = line.Length - line.TrimStart().Length;

                                item = GetHmsItemFromLine(line);
                                if (indent == 0) {
                                    // it's Class
                                    if (!HmsClasses.ContainsName(item.Text)) {
                                        hmsclass = new HMSClassInfo {
                                            Name = item.Text,
                                            Type = item.Type,
                                            Help = item.Help
                                        };
                                        HmsClasses.Add(hmsclass);
                                        item.Kind         = DefKind.Class;
                                        item.ImageIndex   = ImagesIndex.Class;
                                        item.ToolTipTitle = "Класс " + item.Text;
                                        item.IsClass      = true;
                                        item.ClassInfo    = hmsclass;
                                        ItemsClass.Add(item);
                                        ClassesString += hmsclass.Name.ToLower() + "|";
                                    }

                                } else if (indent == 2) {
                                    // it's method or property of the class
                                    var cmd = item.ToolTipTitle;
                                    if      (cmd.StartsWith("function" )) { item.ImageIndex = ImagesIndex.Method; item.Kind = DefKind.Method   ; }
                                    else if (cmd.StartsWith("procedure")) { item.ImageIndex = ImagesIndex.Method; item.Kind = DefKind.Procedure; }
                                    else if (cmd.StartsWith("property" )) { item.ImageIndex = ImagesIndex.Field ; item.Kind = DefKind.Property ; }
                                    else if (cmd.StartsWith("index"    )) { item.ImageIndex = ImagesIndex.Enum  ; item.Kind = DefKind.Property ; }
                                    else if (cmd.StartsWith("event"    )) { item.ImageIndex = ImagesIndex.Event ; item.Kind = DefKind.Event    ; }
                                    var name = Regex.Replace(cmd, @"^(function|procedure|property|index property|event)\s+", "");
                                    name = Regex.Match(name, @"\w+").Value.Trim();
                                    if (name.Length < 1) name += " ";
                                    item.Text         = name;
                                    item.MenuText     = name;
                                    item.Level        = 1;
                                    if (item.ImageIndex == ImagesIndex.Enum) item.Text = name + "[^]";
                                    else if (item.ImageIndex == ImagesIndex.Method) {
                                        if (cmd.IndexOf('(')>0) item.Text = name + "(^)";
                                      //else                    item.Text = name + "()";
                                    }
                                    if (name.ToLower() == "create") {
                                        // hmm... only one static method
                                        isStatic = true;
                                        hmsclass?.StaticItems.Add(item);
                                    }
                                    else {
                                        isStatic = false;
                                        hmsclass?.MemberItems.Add(item);
                                    }
                                } else if ((indent == 4) || (line[0] == '\t')) {
                                    // it's help for parameters of last method
                                    if (isStatic) {
                                        if (hmsclass?.StaticItems.Count > 0) {
                                            item = hmsclass.StaticItems[hmsclass.StaticItems.Count - 1];
                                            item.Params.Add(StylishHelp(line));
                                        }
                                    } else {
                                        if (hmsclass?.MemberItems.Count > 0) {
                                            item = hmsclass.MemberItems[hmsclass.MemberItems.Count - 1];
                                            item.Params.Add(StylishHelp(line));
                                        }
                                    }
                                }
                                line = reader.ReadLine();
                            }
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
                    if (stream != null) {
                        using (var reader = new StreamReader(stream)) {
                            stream = null; string line;
                            while ((line = reader.ReadLine()) != null) {
                                if (line.StartsWith("*") || (line.Trim().Length == 0)) continue; // Skip comments and blank lines
                                item = GetHmsItemFromLine(line);
                                if (!HmsTypes.ContainsName(item.Text)) {
                                    var hmsType = new HMSClassInfo {
                                        Name = item.Text,
                                        Type = item.Text,
                                        Help = item.Help
                                    };
                                    string names = Regex.Match(line, @"\((.*?)\)").Groups[1].Value;
                                    foreach(string name in names.Split(',')) {
                                        item = new HMSItem {
                                            ImageIndex   = ImagesIndex.Enum,
                                            Text         = name,
                                            MenuText     = name,
                                            ToolTipTitle = name,
                                            ToolTipText  = "Перечисление типа " + hmsType.Name
                                        };
                                        hmsType.MemberItems.Add(item);
                                    }
                                    HmsTypes.Add(hmsType);
                                    ClassesString += hmsType.Name.ToLower() + "|";
                                }
                            }
                        }
                    }

                } catch (Exception e) {
                    LogError(e.ToString());

                } finally {
                    // ReSharper disable once UseNullPropagation
                    if (stream != null) stream.Dispose();
                }
            }

            // Load a built-in Functions and Procedures items
            BuildAutocompleteItemsFromResourse(ResourcePath + "hms_func.txt", ImagesIndex.Procedure, "", ItemsFunction, DefKind.Function);

            // Load a built-in Variables
            BuildAutocompleteItemsFromResourse(ResourcePath + "hms_vars.txt"     , ImagesIndex.Field, "Встроенная переменная", ItemsVariable, DefKind.Variable);
                
            // Load a built-in Constants
            BuildAutocompleteItemsFromResourse(ResourcePath + "hms_constants.txt", ImagesIndex.Enum , "Встроенная константа" , ItemsConstant, DefKind.Constant);

            foreach(var info in HmsTypes) {
                foreach(var typeitem in info.MemberItems) {
                    ItemsConstant.Add(typeitem);
                }
            }

            // Check the self
            NotFoundedType  = "|";
            HmsTypesString += "int|long|void|bool|float|";
            foreach (var q in HmsClasses)  { CheckKnownType(q.Type); foreach (var a in q.MemberItems) CheckKnownType(a.Type); }
            foreach (var q in ItemsFunction) CheckKnownType(q.Type);
            foreach (var q in ItemsVariable) CheckKnownType(q.Type);
            foreach (var q in ItemsConstant) CheckKnownType(q.Type);

            string funcList = "";
            foreach (var q in ItemsFunction) funcList += "|"+q.MenuText;
            funcList = funcList.Substring(1).Replace("|Int|", "|Int\\(|");
            RegexHmsFunctions = new Regex(@"\b(" + funcList + @")\b", RegexOptions.IgnoreCase);

            string varsList = "";
            foreach (var q in ItemsVariable) varsList += "|" + q.MenuText;
            varsList = varsList.Substring(1);
            RegexHmsVariables = new Regex(@"\b(" + varsList + @")\b", RegexOptions.IgnoreCase);

            varsList = "|true|false|nil|null";
            foreach (var q in ItemsConstant) varsList += "|" + q.MenuText;
            varsList = varsList.Substring(1);
            RegexHmsConstants = new Regex(@"\b(" + varsList + @")\b", RegexOptions.IgnoreCase);

            ClassesString += NotFoundedType.ToLower();
            HmsTypesString += "";
        }
        public static Regex RegexHmsFunctions;
        public static Regex RegexHmsVariables;
        public static Regex RegexHmsConstants;
        

        private static void CheckKnownType(string type) {
            if (type.Length < 1) return;
            var s = type.ToLower();
            var k = (ClassesString.IndexOf("|" + s + "|", StringComparison.Ordinal) >= 0) || (HmsTypesString.IndexOf("|" + s + "|", StringComparison.Ordinal) >= 0) || (NotFoundedType.ToLower().IndexOf("|" + s + "|", StringComparison.Ordinal) >= 0);
            if (!k) NotFoundedType += type + "|";
        }

        private static void BuildAutocompleteItemsFromResourse(string file, int imageIndex, string toolTipText, AutocompleteItems itemsList, DefKind kind) {
            string   section  = "";
            string   filter   = "";
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream   stream   = assembly.GetManifestResourceStream(file);
            try {
                if (stream != null) {
                    using (var reader = new StreamReader(stream)) {
                        stream = null; string line;
                        while ((line = reader.ReadLine()) != null) {
                            var m = Regex.Match(line, @"^\*\s*?\[(.*)\]"); if (m.Success) { section = m.Groups[1].Value.Trim(); continue; }
                                m = Regex.Match(line, @"^\*(sm\w+)"     ); if (m.Success) { filter  = m.Groups[1].Value.Trim(); continue; }
                            if (filter == "smAll") filter = "";
                            if (line.StartsWith("*") || (line.Trim().Length == 0)) continue; // Skip comments and blank lines
                            int indent = line.Length - line.TrimStart().Length;
                            HMSItem item;
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
                }

            } catch (Exception e) {
                LogError(e.ToString());

            } finally {
                stream?.Dispose();
            }
        }

        public static HMSItem GetHmsItemFromLine(string line) {
            HMSItem item = new HMSItem {
                Text = regexGetFromLineName.Match(line).Groups[1].Value.Trim(),
                Type = regexGetFromLineType.Match(line).Groups[1].Value.Trim(),
                ToolTipTitle = regexGetFromLineCmd.Match(line).Groups[1].Value.Trim(),
                ToolTipText  = ""
            };
            // All ok, if not success - value is empty string
            item.MenuText     = item.Text;
            item.Help         = StylishHelp(regexGetFromLineHelp.Match(line).Groups[1].Value);
            if (item.ToolTipTitle.Length == 0) {
                item.ToolTipTitle = item.Text;
                if (item.Type.Length > 0 ) item.ToolTipTitle += ": " + item.Type;
            }
            return item;
        }

        public static string StylishHelp(string help) {
            help = help.Replace(@"\n", "\n").Trim();
            help = regexStringAndComments.Replace(help, "<s>$0</s>");
            return help;
        }

        private static Regex regexBracketsR = new Regex(@"(\(|\))");
        private static Regex regexBracketsQ = new Regex(@"(\[|\])");
        private static Regex regexStringAndComments = new Regex(@"""(\\""|[^""])*""|'(\\'|[^'])*'|(//.*|\/\*[\s\S]*?\*\/)", RegexOptions.Singleline);

        public static string GetTextWithoutBrackets(string text) {
            text = GetTextWithoutBlock(text, regexBracketsR, "(");
            return GetTextWithoutBlock(text, regexBracketsQ, "[");
        }

        public static string ReadTextFromResource(string file) {
            string text = "";
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(ResourcePath + file);
            try {
                if (stream != null) {
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8)) {
                        stream = null;
                        text = reader.ReadToEnd();
                    }
                }
            } finally {
                stream?.Dispose();
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

        public static bool TypeIsClass  (string type) { return ClassesString .IndexOf("|" + type.Trim().ToLower() + "|", StringComparison.Ordinal) >= 0; }

        public static bool WordIsKeyword(string word) { return KeywordsString.IndexOf("|" + word.Trim().ToLower() + "|", StringComparison.Ordinal) >= 0; }

        public static bool CheckKeywordRegister(ref string word) {
            if (word.Length < 2) return false;
            if (KeywordsString.Length != Keywords.Length) return false;
            string wordlower = word.ToLower();
            int i = KeywordsString.IndexOf("|" + wordlower + "|", StringComparison.Ordinal);
            if (i >= 0) {
                string keyw = Keywords.Substring(i + 1, wordlower.Length);
                if (keyw != word) {
                    word = keyw;
                    return true;
                }
            }
            if (HMSEditor.ActiveEditor!=null) {
                string types = HMSEditor.ActiveEditor.CurrentValidTypesReg;
                i = types.ToLower().IndexOf("|" + wordlower + "|", StringComparison.Ordinal);
                if (i >= 0) {
                    string keyw = types.Substring(i + 1, wordlower.Length);
                    if (keyw != word) {
                        word = keyw;
                        return true;
                    }
                }
            }
            return false;
        }

        #region Работа с шаблонами
        public static bool TemplatesIsLoading;
        public static void LoadTemplates() {
            // Если уже кто-то загружает шаблоны (другой поток) - ждём максимум 4 секунды
            int i = 10; while (TemplatesIsLoading) { if (i-- < 0) return; Thread.Sleep(400); }
                
            // load templates from resource
            INI ini = new INI();
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = null;
            try {
                stream = assembly.GetManifestResourceStream("HMSEditorNS.Resources.hms_templates.txt");
                if (stream != null) {
                    using (StreamReader reader = new StreamReader(stream)) {
                        stream = null;
                        ini.Text = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception e) {
                LogError(e.ToString());

            } finally {
                stream?.Dispose();
            }

            TemplatesIsLoading = true; // Говорим какбе другим потокам, которые могут обновлять в фоне: "Стапэ - идёт загрузка и установка шаблонов".
            foreach (var lang in new[] { Language.CPPScript, Language.PascalScript, Language.BasicScript, Language.JScript }) {
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
            var item = menuItem.DropDownItems.ContainsKey(name) ? menuItem.DropDownItems[name] : menuItem.DropDownItems.Add(name, Properties.Resources.Template_Application_16xLG);
            item.Name = name;
            item.AccessibleDescription = text;
            string hint = ""; int maxLines = 10; int maxChars = 100;
            foreach (string line in item.AccessibleDescription.Split('\n')) {
                string cut = line.Substring(0, Math.Min(line.Length, maxChars));
                hint += cut + '\n';
                if (maxLines-- < 0) { hint += "..."; break; }
            }
            item.ToolTipText = hint;
            
            return item;
        }

        private static void AddTemplatesFromIni(Templates parentItem, Language lang, INI ini) {
            string langString = lang.ToString();
            foreach (string section in ini.Dict.Keys) {
                if (!section.StartsWith(langString)) continue;
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
            if (Environment.OSVersion.Platform == PlatformID.Unix) {
                string pathDownload = Path.Combine(getHomePath(), "Downloads");
                return pathDownload;
            }

            return Convert.ToString(Microsoft.Win32.Registry.GetValue(
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
            if (args.Name.IndexOf("Ionic.Zip.Reduced", StringComparison.Ordinal) >= 0) {
                Assembly assembly = Assembly.GetExecutingAssembly();
                const string resourceZip = "HMSEditorNS.Resources.Ionic.Zip.Reduced.dll"; // Мини-библиотека для работы с zip (ибо в .NET 2.0 нет поддержки zip файлов)
                using (Stream stm = assembly.GetManifestResourceStream(resourceZip)) {
                    if (stm != null) {
                        var buffer = new byte[(int)stm.Length];
                        stm.Read(buffer, 0, (int)stm.Length);
                        return Assembly.Load(buffer);
                    }
                }
            }
            return null;
        }

        // Function to detect the encoding for UTF-7, UTF-8/16/32 (bom, no bom, little
        // & big endian), and local default codepage, and potentially other codepages.
        // 'taster' = number of bytes to check of the file (to save processing). Higher
        // value is slower, but more reliable (especially UTF-8 with special characters
        // later on may appear to be ASCII initially). If taster = 0, then taster
        // becomes the length of the file (for maximum reliability). 'text' is simply
        // the string with the discovered encoding applied to the file.
        public static Encoding LoadAndDetectEncoding(string filename, out string text, int taster = 1000) {
            byte[] b = File.ReadAllBytes(filename);

            //////////////// First check the low hanging fruit by checking if a
            //////////////// BOM/signature exists (sourced from http://www.unicode.org/faq/utf_bom.html#bom4)
            if (b.Length >= 4 && b[0] == 0x00 && b[1] == 0x00 && b[2] == 0xFE && b[3] == 0xFF) { text = Encoding.GetEncoding("utf-32BE").GetString(b, 4, b.Length - 4); return Encoding.GetEncoding("utf-32BE"); }  // UTF-32, big-endian 
            else if (b.Length >= 4 && b[0] == 0xFF && b[1] == 0xFE && b[2] == 0x00 && b[3] == 0x00) { text = Encoding.UTF32.GetString(b, 4, b.Length - 4); return Encoding.UTF32; }    // UTF-32, little-endian
            else if (b.Length >= 2 && b[0] == 0xFE && b[1] == 0xFF) { text = Encoding.BigEndianUnicode.GetString(b, 2, b.Length - 2); return Encoding.BigEndianUnicode; }     // UTF-16, big-endian
            else if (b.Length >= 2 && b[0] == 0xFF && b[1] == 0xFE) { text = Encoding.Unicode.GetString(b, 2, b.Length - 2); return Encoding.Unicode; }              // UTF-16, little-endian
            else if (b.Length >= 3 && b[0] == 0xEF && b[1] == 0xBB && b[2] == 0xBF) { text = Encoding.UTF8.GetString(b, 3, b.Length - 3); return Encoding.UTF8; } // UTF-8
            else if (b.Length >= 3 && b[0] == 0x2b && b[1] == 0x2f && b[2] == 0x76) { text = Encoding.UTF7.GetString(b, 3, b.Length - 3); return Encoding.UTF7; } // UTF-7


            //////////// If the code reaches here, no BOM/signature was found, so now
            //////////// we need to 'taste' the file to see if can manually discover
            //////////// the encoding. A high taster value is desired for UTF-8
            if (taster == 0 || taster > b.Length) taster = b.Length;    // Taster size can't be bigger than the filesize obviously.


            // Some text files are encoded in UTF8, but have no BOM/signature. Hence
            // the below manually checks for a UTF8 pattern. This code is based off
            // the top answer at: http://stackoverflow.com/questions/6555015/check-for-invalid-utf8
            // For our purposes, an unnecessarily strict (and terser/slower)
            // implementation is shown at: http://stackoverflow.com/questions/1031645/how-to-detect-utf-8-in-plain-c
            // For the below, false positives should be exceedingly rare (and would
            // be either slightly malformed UTF-8 (which would suit our purposes
            // anyway) or 8-bit extended ASCII/UTF-16/32 at a vanishingly long shot).
            int i = 0;
            bool utf8 = false;
            while (i < taster - 4) {
                if (b[i] <= 0x7F) { i += 1; continue; }     // If all characters are below 0x80, then it is valid UTF8, but UTF8 is not 'required' (and therefore the text is more desirable to be treated as the default codepage of the computer). Hence, there's no "utf8 = true;" code unlike the next three checks.
                if (b[i] >= 0xC2 && b[i] <= 0xDF && b[i + 1] >= 0x80 && b[i + 1] < 0xC0) { i += 2; utf8 = true; continue; }
                if (b[i] >= 0xE0 && b[i] <= 0xF0 && b[i + 1] >= 0x80 && b[i + 1] < 0xC0 && b[i + 2] >= 0x80 && b[i + 2] < 0xC0) { i += 3; utf8 = true; continue; }
                if (b[i] >= 0xF0 && b[i] <= 0xF4 && b[i + 1] >= 0x80 && b[i + 1] < 0xC0 && b[i + 2] >= 0x80 && b[i + 2] < 0xC0 && b[i + 3] >= 0x80 && b[i + 3] < 0xC0) { i += 4; utf8 = true; continue; }
                utf8 = false; break;
            }
            if (utf8 == true) {
                text = Encoding.UTF8.GetString(b);
                return Encoding.UTF8;
            }


            // The next check is a heuristic attempt to detect UTF-16 without a BOM.
            // We simply look for zeroes in odd or even byte places, and if a certain
            // threshold is reached, the code is 'probably' UF-16.          
            double threshold = 0.1; // proportion of chars step 2 which must be zeroed to be diagnosed as utf-16. 0.1 = 10%
            int count = 0;
            for (int n = 0; n < taster; n += 2) if (b[n] == 0) count++;
            if (((double)count) / taster > threshold) { text = Encoding.BigEndianUnicode.GetString(b); return Encoding.BigEndianUnicode; }
            count = 0;
            for (int n = 1; n < taster; n += 2) if (b[n] == 0) count++;
            if (((double)count) / taster > threshold) { text = Encoding.Unicode.GetString(b); return Encoding.Unicode; } // (little-endian)


            // Finally, a long shot - let's see if we can find "charset=xyz" or
            // "encoding=xyz" to identify the encoding:
            for (int n = 0; n < taster - 9; n++) {
                if (
                    ((b[n + 0] == 'c' || b[n + 0] == 'C') && (b[n + 1] == 'h' || b[n + 1] == 'H') && (b[n + 2] == 'a' || b[n + 2] == 'A') && (b[n + 3] == 'r' || b[n + 3] == 'R') && (b[n + 4] == 's' || b[n + 4] == 'S') && (b[n + 5] == 'e' || b[n + 5] == 'E') && (b[n + 6] == 't' || b[n + 6] == 'T') && (b[n + 7] == '=')) ||
                    ((b[n + 0] == 'e' || b[n + 0] == 'E') && (b[n + 1] == 'n' || b[n + 1] == 'N') && (b[n + 2] == 'c' || b[n + 2] == 'C') && (b[n + 3] == 'o' || b[n + 3] == 'O') && (b[n + 4] == 'd' || b[n + 4] == 'D') && (b[n + 5] == 'i' || b[n + 5] == 'I') && (b[n + 6] == 'n' || b[n + 6] == 'N') && (b[n + 7] == 'g' || b[n + 7] == 'G') && (b[n + 8] == '='))
                    ) {
                    if (b[n + 0] == 'c' || b[n + 0] == 'C') n += 8; else n += 9;
                    if (b[n] == '"' || b[n] == '\'') n++;
                    int oldn = n;
                    while (n < taster && (b[n] == '_' || b[n] == '-' || (b[n] >= '0' && b[n] <= '9') || (b[n] >= 'a' && b[n] <= 'z') || (b[n] >= 'A' && b[n] <= 'Z'))) { n++; }
                    byte[] nb = new byte[n - oldn];
                    Array.Copy(b, oldn, nb, 0, n - oldn);
                    try {
                        string internalEnc = Encoding.ASCII.GetString(nb);
                        text = Encoding.GetEncoding(internalEnc).GetString(b);
                        return Encoding.GetEncoding(internalEnc);
                    } catch { break; }    // If C# doesn't recognize the name of the encoding, break.
                }
            }


            // If all else fails, the encoding is probably (though certainly not
            // definitely) the user's local codepage! One might present to the user a
            // list of alternative encodings as shown here: http://stackoverflow.com/questions/8509339/what-is-the-most-common-encoding-of-each-language
            // A full list can be found using Encoding.GetEncodings();
            text = Encoding.Default.GetString(b);
            return Encoding.Default;
        }

        public static bool ExistsRussianUtf8(string text) {
            return Regex.IsMatch(text, "[А-Яа-я]");
        }


    }
}

