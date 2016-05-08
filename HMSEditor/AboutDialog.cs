using System;
using System.Reflection;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Security.Permissions;
using HMSEditorNS.Properties;

namespace HMSEditorNS {
    partial class AboutDialog: Form {
        private static AboutDialog ThisDialog;
        private static ProgressBar progress;
        private static string tmpFileRelease  = "";
        private static string tmpFileTemplate = "";
        private System.Threading.Timer UpdateTimer = new System.Threading.Timer(UpdateTimer_Task, null, Timeout.Infinite, Timeout.Infinite);
        private string TemplatesInfo = "";
        private string TemplatesDate = "";
        private int ProgressProcent;
        private static string ExecutableDir = Path.GetDirectoryName(Application.ExecutablePath);
        private static bool   DeniedClose;

        public AboutDialog() {
            ThisDialog = this;
            InitializeComponent();
            
            tmpFileRelease  = HMS.DownloadDir + "HMSEditor_addon.zip";
            tmpFileTemplate = HMS.DownloadDir + "HMSEditorTemplates.zip";

            Text = $"О программе {AssemblyTitle}";
            labelProductName.Text = AssemblyProduct;
            labelVersion.Text = $"Версия {AssemblyVersion}";
            labelCopyright.Text = AssemblyCopyright;
            labelCompanyName.Text = AssemblyCompany;
            textBoxDescription.Text = AssemblyDescription;
            DeleteGarbage();
            progress = progressBar1;
            logo.Init();
        }

        public sealed override string Text {
            get { return base.Text; }
            set { base.Text = value; }
        }

        #region Методы доступа к атрибутам сборки

        public string AssemblyTitle {
            get {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0) {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "") {
                        return titleAttribute.Title;
                    }
                }
                return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public static string AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public static string AssemblyDescription {
            get {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0) {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public static string AssemblyProduct {
            get {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0) {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public static string AssemblyCopyright {
            get {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0) {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public static string AssemblyCompany {
            get {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0) {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
        #endregion

        private void CheckUpdate(string lastVersion, string templateVersion) {
            TemplatesDate = templateVersion;
            string tmpltsLastUpdateStored = HMSEditor.Settings.Get("TemplateLastUpdate", "Common", "");
            if (tmpltsLastUpdateStored != templateVersion) {
                string datetime = templateVersion.Replace("T", " ").Replace("Z", "").Replace("-", ".");
                labelNewTemplates.Text = Resources.AboutDialog_CheckUpdate_Есть_новая_версия_шаблонов_от_ + datetime;
                labelNewTemplates .Visible = true;
                btnUpdateTemplates.Visible = true;
            }
            int resultCompares = GitHub.CompareVersions(lastVersion, AssemblyVersion);
            if (lastVersion.Length == 0) {
                labelNewVersion.Text = Resources.AboutDialog_CheckUpdate_Не_удалось_проверить_версию_на_GitHub;
            } else if (resultCompares == 0) {
                labelNewVersion.Text = Resources.AboutDialog_CheckUpdate_У_вас_последняя_версия;
            } else if (resultCompares > 0) {
                labelNewVersion.Text = Resources.AboutDialog_CheckUpdate_Есть_новая_версия_ + lastVersion;
                HMS.NewVersionExist      = true;
                btnUpdateProgram.Visible = true;
            }
            HMS.NewVersionChecked   = true;
            labelNewVersion.Visible = true;
            if (TemplatesInfo.Length > 0) labelNewTemplates.Visible = true;
        }

        // Проверка новой версии в фоновом режиме
        private static void UpdateTimer_Task(object state) {
            string updatesInfo, templatesInfo;
            string lastVersion     = GitHub.GetLatestReleaseVersion(HMS.GitHubHMSEditor, out updatesInfo);
            string templateVersion = GitHub.GetRepoUpdatedDate(HMS.GitHubTemplates, out templatesInfo);
            if (ThisDialog.Visible) {
                DeniedClose = true;
                try {
                    ThisDialog.Invoke((MethodInvoker)delegate {
                        HMS.UpdateInfo           = updatesInfo;
                        ThisDialog.TemplatesInfo = templatesInfo;
                        ThisDialog.CheckUpdate(lastVersion, templateVersion);
                    });
                } finally {
                    DeniedClose = false;
                }
            }
        }

        private void AboutDialog_Load(object sender, EventArgs e) {
        if (HMSEditor.NeedRestart) SetNeedRestart();
            else UpdateTimer.Change(1, Timeout.Infinite);
        }

        private void AboutDialog_FormClosing(object sender, FormClosingEventArgs e) {
            if (DeniedClose) e.Cancel = true;
            else DeleteGarbage();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Process.Start(linkLabel1.Text);
        }

        protected override void OnPaintBackground(PaintEventArgs e) {
            base.OnPaintBackground(e);
            DrawProgress();
        }

        public void DrawProgress() {
            int penWidth = 4;
            System.Drawing.Pen      pen = new System.Drawing.Pen(System.Drawing.Color.Green, penWidth);
            System.Drawing.Graphics g   = CreateGraphics();
            int w = ClientSize.Width  - (penWidth / 2);
            int h = ClientSize.Height - (penWidth / 2); 
            int P = w * 2 + h * 2;
            int L = P * ProgressProcent / 100 ;
            int DL1 = Math.Min(L, w / 2); L -= DL1;
            int DL2 = Math.Min(L, h);     L -= DL2;
            int DL3 = Math.Min(L, w);     L -= DL3;
            int DL4 = Math.Min(L, h);     L -= DL4;
            int DL5 = Math.Min(L, w / 2);
            int x = w / 2 + 1, y = h + 1;
            if (DL1 > 0) g.DrawLine(pen, x, y, x - DL1 - 1, y); x -= DL1;
            if (DL2 > 0) g.DrawLine(pen, x, y, x, y - DL2 - 1); y -= DL2;
            if (DL3 > 0) g.DrawLine(pen, x, y, x + DL3 + 1, y); x += DL3;
            if (DL4 > 0) g.DrawLine(pen, x, y, x, y + DL4 + 1); y += DL4;
            if (DL5 > 0) g.DrawLine(pen, x, y, x - DL5 - 1, y);
            pen.Dispose();
            g.Dispose();
        }

        private void btnUpdate_Click(object sender, EventArgs e) {

            Process.Start(GitHub.ReleaseUrl);

            ////progress.Show();
            //Refresh();
            //btnUpdateProgram.Text = Resources.AboutDialog_btnUpdate_Click_Идёт_загрузка___;
            //btnUpdateProgram  .Enabled = false;
            //btnUpdateTemplates.Enabled = false;
            //GitHub.DownloadFileCompleted   += DownloadReleaseCallback;
            //GitHub.DownloadProgressChanged += DownloadProgressCallback;

            GitHub.DownloadLatestReleaseAsync(tmpFileRelease);
        }

        private static bool DirIsWriteable(string dir) {
            DirectoryInfo dirInfo = new DirectoryInfo(dir);
            AuthorizationRuleCollection rules;
            WindowsIdentity identity;
            try {
                rules = dirInfo.GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier));
                identity = WindowsIdentity.GetCurrent();
            } catch (UnauthorizedAccessException) {
                return false;
            }

            bool   isAllow = false;
            if (identity.User != null) {
                string userSID = identity.User.Value;
                foreach (FileSystemAccessRule rule in rules) {
                    if (identity.Groups != null && (rule.IdentityReference.ToString() == userSID || identity.Groups.Contains(rule.IdentityReference))) {
                        var i = (int)(rule.FileSystemRights & FileSystemRights.Write);
                        i |= (int)(rule.FileSystemRights & FileSystemRights.WriteAttributes);
                        i |= (int)(rule.FileSystemRights & FileSystemRights.WriteData);
                        i |= (int)(rule.FileSystemRights & FileSystemRights.CreateDirectories);
                        i |= (int)(rule.FileSystemRights & FileSystemRights.CreateFiles);
                        if ((i > 0) && (rule.AccessControlType == AccessControlType.Deny))
                            return false;
                        else if ((i > 0) && (rule.AccessControlType == AccessControlType.Allow))
                            isAllow = true;
                    }
                }
            }
            return isAllow;
        }

        private static void TryDeleteFile(string file) {
            try {
                if (File.Exists(file))
                    File.Delete(file);
            }
            catch {
                // ignored
            }
        }

        public void SetNeedRestart() {
            labelNewVersion .Text    = Resources.AboutDialog_SetNeedRestart_Требуется_перезапуск_дополнения;
            labelNewVersion .Visible = true;
            btnUpdateProgram.Text    = Resources.AboutDialog_SetNeedRestart_Перезапустить;
            btnUpdateProgram.Visible = false;
            btnUpdateProgram  .Enabled = true;
            btnUpdateTemplates.Enabled = true;
            ToolTip tip = new ToolTip();
            //string msg = "Необходимо зайти в список дополнений и удалить существующее дополнение. После чего будет автоматически запущено обновление на новую версию.";
            tip.SetToolTip(labelNewVersion, "Для перезапуска необходимо зайти в список дополнений программы\nи удалить его из списка дополнений.\nПосле чего заново просканировать на наличие обновлённой версии.");
        }

        private void InstallNewFile() {
            HMSEditor.NeedRestart     = true;
            HMSEditor.NeedCopyNewFile = tmpFileRelease;
            SetNeedRestart();
        }

        private void DownloadReleaseCallback(object sender, EventArgs e) {
            GitHub.DownloadFileCompleted   -= DownloadReleaseCallback;
            GitHub.DownloadProgressChanged -= DownloadProgressCallback;

            if (ThisDialog != null && ThisDialog.Visible) {
                DeniedClose = true;
                try {
                    ThisDialog.Invoke((MethodInvoker)delegate {
                        ProgressProcent = 100;
                        DrawProgress();
                        progress.Hide();
                        string tmpFile = HMS.DownloadDir + "HMSEditor.dll";
                        HMS.ExtractZipTo(tmpFileRelease, HMS.DownloadDir, "HMSEditor.dll");

                        if (!AuthenticodeTools.IsTrusted(tmpFile)) {
                            string msg = "У полученного файла не верная цифровая подпись. Обновление прервано.\n\n" +
                                         "Это может означать, что произошла подмена файла или автор забыл подписать файл. " +
                                         "Может быть временные проблемы с интернетом. В любом случае, можно попробовать " +
                                         "посетить пару мест, где знают о существовании данной программы и спросить там:\n" +
                                         "https://homemediaserver.ru/forum\nhttps://hms.lostcut.net\nhttps://github.com/WendyH/HMSEditor_addon/issues";
                            MessageBox.Show(msg, HMSEditor.Title, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                            return;
                        }
                        InstallNewFile();
                        HMSEditor.NeedCopyDllFile = tmpFile;
                    });
                } finally {
                    DeniedClose = false;
                }
            }
        }

        private void DownloadProgressCallback(object sender, EventArgs e) {
            GitHub.RequestState state = sender as GitHub.RequestState;
            if (ThisDialog != null && ThisDialog.Visible) {
                if (state != null) {
                    DeniedClose = true;
                    try {
                        ThisDialog.Invoke((MethodInvoker)delegate {
                            if (state.TotalBytes > 0) {
                                progress.Maximum = (int)state.TotalBytes;
                                progress.Value   = (int)state.BytesRead;
                                ProgressProcent = (int)(state.BytesRead / (state.TotalBytes / 100));
                                DrawProgress();
                            }
                        });
                    } finally {
                        DeniedClose = false;
                    }
                }
            } else {
                state?.Close();
            }
        }

        private void labelNewVersion_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            if (HMS.UpdateInfo.Length == 0) return;
            frmUpdateInfoDialog form = new frmUpdateInfoDialog();
            string html = HMS.ReadTextFromResource("Markdown.html");
            form.SetText(html.Replace("<MarkdownText>", HMS.UpdateInfo));
            form.ShowDialog();
        }

        private void labelNewTemplates_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            if (TemplatesInfo.Length == 0) return;
            frmUpdateInfoDialog form = new frmUpdateInfoDialog();
            string html = HMS.ReadTextFromResource("Markdown.html");
            form.SetText(html.Replace("<MarkdownText>", TemplatesInfo));
            form.Text = Resources.AboutDialog_labelNewTemplates_LinkClicked_Информация_о_новых_шаблонах_HMS_Editor;
            form.ShowDialog();
        }

        private void DeleteGarbage() {
            TryDeleteFile(tmpFileTemplate);
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Process.Start(linkLabel3.Text);
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Process.Start(linkLabel2.Text);
        }

        private void DownloadTemplateCallback(object sender, EventArgs e) {
            GitHub.DownloadFileCompleted   -= DownloadTemplateCallback;
            GitHub.DownloadProgressChanged -= DownloadProgressCallback;

            if (ThisDialog != null && ThisDialog.Visible) {
                DeniedClose = true;
                try {
                    ThisDialog.Invoke((MethodInvoker)delegate {
                        progress.Hide();
                        btnUpdateProgram  .Enabled = true;
                        btnUpdateTemplates.Enabled = true;
                        btnUpdateTemplates.Visible = false;
                        if (HMS.ExtractZip(tmpFileTemplate, true)) {
                            HMSEditor.Settings.Set("TemplateLastUpdate", TemplatesDate, "Common");
                            HMSEditor.Settings.Save();
                            HMS.LoadTemplates();
                            labelNewTemplates.Text = Resources.AboutDialog_DownloadTemplateCallback_Обновлено;
                            if (HMSEditor.ActiveEditor != null) HMSEditor.ActiveEditor.CreateInsertTemplateItems();
                        }
                    });
                } finally {
                    DeniedClose = false;
                }
            }
        }

        private void btnUpdateTemplates_Click(object sender, EventArgs e) {
            //progress.Show();
            Refresh();
            btnUpdateTemplates.Text = Resources.AboutDialog_btnUpdate_Click_Идёт_загрузка___;
            btnUpdateProgram  .Enabled = false;
            btnUpdateTemplates.Enabled = false;
            GitHub.DownloadFileCompleted   += DownloadTemplateCallback;
            GitHub.DownloadProgressChanged += DownloadProgressCallback;

            GitHub.DownloadFileAsync("https://codeload.github.com/" + HMS.GitHubTemplates + "/legacy.zip/master", tmpFileTemplate);
        }

        private void btnDelete_Click(object sender, EventArgs e) {
            var msg = "ВНИМАНИЕ!\n" +
                      "Загруженные шаблоны, настройки и установленные темы будут УДАЛЕНЫ!\n" +
                      "Вы уверены, что хотите удалить папку и всё её содержимое: "+HMS.WorkingDir+"?";
            DialogResult answ = MessageBox.Show(msg, HMSEditor.Title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
            if (answ == DialogResult.Yes) {
                try {
                    Directory.Delete(HMS.WorkingDir, true);
                }
                catch {
                    // ignored
                }
                TryDeleteFile(HMS.ErrorLogFile);
                DeleteGarbage();
            }
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static void CopyNewFile() {
            if (HMSEditor.NeedCopyDllFile == "") return;
            if (File.Exists(HMSEditor.NeedCopyDllFile)) {
                string msg;
                string addonDir  = ExecutableDir + "\\Addons\\";
                string addonfile = addonDir + "HMSEditor.dll";
                // waiting 3 sek, copy new file to our path and start our executable
                string rargs = "/C ping 127.0.0.1 -n 3 && Copy /Y \"" + HMSEditor.NeedCopyDllFile + "\" \"" + addonfile + "\" &&  Del \"" + HMSEditor.NeedCopyDllFile + "\"";
                ProcessStartInfo Info = new ProcessStartInfo {
                    Arguments      = rargs,
                    WindowStyle    = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    FileName       = "cmd.exe"
                };
                if (!DirIsWriteable(addonDir)) {
                    msg = "Дополнение находится в каталоге, где нужны привилегии для записи файлов.\n" +
                          "Будет сделан запрос на ввод имени и пароля пользователя,\n" +
                          "который данными привилегиями обладает.";
                    MessageBox.Show(msg, HMSEditor.Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Info.Verb = "runas";
                }
                try {
                    Process.Start(Info);
                } catch (Exception ex) {
                    msg = "Ошибка обновления дополнения.\n" +
                          "Возможно, из-за нарушения прав доступа или по какой-то другой причине.\n" +
                          "Автоматическое обновление не произошло.";
                    MessageBox.Show(msg, HMSEditor.Title, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    HMS.LogError(ex.ToString());
                    return;
                }
                TryDeleteFile(HMS.ErrorLogFile);
            }
        }

    }
}
