namespace HMSEditorNS {
	partial class AboutDialog {
		/// <summary>
		/// Обязательная переменная конструктора.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Освободить все используемые ресурсы.
		/// </summary>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
				UpdateTimer.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Код, автоматически созданный конструктором форм Windows

		/// <summary>
		/// Требуемый метод для поддержки конструктора — не изменяйте 
		/// содержимое этого метода с помощью редактора кода.
		/// </summary>
		private void InitializeComponent() {
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.labelProductName = new System.Windows.Forms.Label();
			this.labelVersion = new System.Windows.Forms.Label();
			this.labelCopyright = new System.Windows.Forms.Label();
			this.labelCompanyName = new System.Windows.Forms.Label();
			this.textBoxDescription = new System.Windows.Forms.TextBox();
			this.linkLabel1 = new System.Windows.Forms.LinkLabel();
			this.okButton = new System.Windows.Forms.Button();
			this.btnUpdateProgram = new System.Windows.Forms.Button();
			this.labelNewVersion = new System.Windows.Forms.LinkLabel();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.linkLabel2 = new System.Windows.Forms.LinkLabel();
			this.linkLabel3 = new System.Windows.Forms.LinkLabel();
			this.labelNewTemplates = new System.Windows.Forms.LinkLabel();
			this.btnUpdateTemplates = new System.Windows.Forms.Button();
			this.logo = new HMSEditorNS.Logo();
			this.btnDelete = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// progressBar1
			// 
			this.progressBar1.Location = new System.Drawing.Point(188, 151);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(302, 23);
			this.progressBar1.TabIndex = 1;
			this.progressBar1.Visible = false;
			// 
			// labelProductName
			// 
			this.labelProductName.AutoSize = true;
			this.labelProductName.Location = new System.Drawing.Point(185, 14);
			this.labelProductName.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
			this.labelProductName.MaximumSize = new System.Drawing.Size(0, 17);
			this.labelProductName.Name = "labelProductName";
			this.labelProductName.Size = new System.Drawing.Size(106, 13);
			this.labelProductName.TabIndex = 30;
			this.labelProductName.Text = "Название продукта";
			this.labelProductName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelVersion
			// 
			this.labelVersion.AutoSize = true;
			this.labelVersion.Location = new System.Drawing.Point(185, 36);
			this.labelVersion.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
			this.labelVersion.MaximumSize = new System.Drawing.Size(0, 17);
			this.labelVersion.Name = "labelVersion";
			this.labelVersion.Size = new System.Drawing.Size(44, 13);
			this.labelVersion.TabIndex = 28;
			this.labelVersion.Text = "Версия";
			this.labelVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelCopyright
			// 
			this.labelCopyright.AutoSize = true;
			this.labelCopyright.Location = new System.Drawing.Point(185, 80);
			this.labelCopyright.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
			this.labelCopyright.MaximumSize = new System.Drawing.Size(0, 17);
			this.labelCopyright.Name = "labelCopyright";
			this.labelCopyright.Size = new System.Drawing.Size(94, 13);
			this.labelCopyright.TabIndex = 31;
			this.labelCopyright.Text = "Авторские права";
			this.labelCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelCompanyName
			// 
			this.labelCompanyName.AutoSize = true;
			this.labelCompanyName.Location = new System.Drawing.Point(185, 58);
			this.labelCompanyName.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
			this.labelCompanyName.MaximumSize = new System.Drawing.Size(0, 17);
			this.labelCompanyName.Name = "labelCompanyName";
			this.labelCompanyName.Size = new System.Drawing.Size(125, 13);
			this.labelCompanyName.TabIndex = 32;
			this.labelCompanyName.Text = "Название организации";
			this.labelCompanyName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxDescription
			// 
			this.textBoxDescription.Location = new System.Drawing.Point(188, 102);
			this.textBoxDescription.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.textBoxDescription.Multiline = true;
			this.textBoxDescription.Name = "textBoxDescription";
			this.textBoxDescription.ReadOnly = true;
			this.textBoxDescription.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textBoxDescription.Size = new System.Drawing.Size(302, 72);
			this.textBoxDescription.TabIndex = 33;
			this.textBoxDescription.TabStop = false;
			// 
			// linkLabel1
			// 
			this.linkLabel1.AutoSize = true;
			this.linkLabel1.Location = new System.Drawing.Point(185, 183);
			this.linkLabel1.Name = "linkLabel1";
			this.linkLabel1.Size = new System.Drawing.Size(198, 13);
			this.linkLabel1.TabIndex = 35;
			this.linkLabel1.TabStop = true;
			this.linkLabel1.Text = "https://github.com/WendyH/HMSEditor";
			this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
			// 
			// okButton
			// 
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.okButton.Location = new System.Drawing.Point(406, 290);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(84, 23);
			this.okButton.TabIndex = 34;
			this.okButton.Text = "&ОК";
			// 
			// btnUpdateProgram
			// 
			this.btnUpdateProgram.Location = new System.Drawing.Point(188, 255);
			this.btnUpdateProgram.Name = "btnUpdateProgram";
			this.btnUpdateProgram.Size = new System.Drawing.Size(131, 23);
			this.btnUpdateProgram.TabIndex = 36;
			this.btnUpdateProgram.Text = "Обновить программу";
			this.btnUpdateProgram.UseVisualStyleBackColor = true;
			this.btnUpdateProgram.Visible = false;
			this.btnUpdateProgram.Click += new System.EventHandler(this.btnUpdate_Click);
			// 
			// labelNewVersion
			// 
			this.labelNewVersion.Location = new System.Drawing.Point(9, 249);
			this.labelNewVersion.Name = "labelNewVersion";
			this.labelNewVersion.Size = new System.Drawing.Size(169, 35);
			this.labelNewVersion.TabIndex = 39;
			this.labelNewVersion.TabStop = true;
			this.labelNewVersion.Text = "Информация о версиях";
			this.labelNewVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.labelNewVersion.Visible = false;
			this.labelNewVersion.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.labelNewVersion_LinkClicked);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(58, 183);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(120, 13);
			this.label1.TabIndex = 40;
			this.label1.Text = "Страница программы:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(33, 204);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(145, 13);
			this.label2.TabIndex = 41;
			this.label2.Text = "Официальный форум HMS:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(21, 225);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(157, 13);
			this.label3.TabIndex = 42;
			this.label3.Text = "Неофициальный форум HMS:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// linkLabel2
			// 
			this.linkLabel2.AutoSize = true;
			this.linkLabel2.Location = new System.Drawing.Point(185, 204);
			this.linkLabel2.Name = "linkLabel2";
			this.linkLabel2.Size = new System.Drawing.Size(174, 13);
			this.linkLabel2.TabIndex = 43;
			this.linkLabel2.TabStop = true;
			this.linkLabel2.Text = "https://homemediaserver.ru/forum/";
			this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
			// 
			// linkLabel3
			// 
			this.linkLabel3.AutoSize = true;
			this.linkLabel3.Location = new System.Drawing.Point(185, 225);
			this.linkLabel3.Name = "linkLabel3";
			this.linkLabel3.Size = new System.Drawing.Size(114, 13);
			this.linkLabel3.TabIndex = 44;
			this.linkLabel3.TabStop = true;
			this.linkLabel3.Text = "https://hms.lostcut.net";
			this.linkLabel3.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel3_LinkClicked);
			// 
			// labelNewTemplates
			// 
			this.labelNewTemplates.Location = new System.Drawing.Point(9, 284);
			this.labelNewTemplates.Name = "labelNewTemplates";
			this.labelNewTemplates.Size = new System.Drawing.Size(169, 35);
			this.labelNewTemplates.TabIndex = 46;
			this.labelNewTemplates.TabStop = true;
			this.labelNewTemplates.Text = "История обновлений шаблонов";
			this.labelNewTemplates.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.labelNewTemplates.Visible = false;
			this.labelNewTemplates.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.labelNewTemplates_LinkClicked);
			// 
			// btnUpdateTemplates
			// 
			this.btnUpdateTemplates.Location = new System.Drawing.Point(188, 290);
			this.btnUpdateTemplates.Name = "btnUpdateTemplates";
			this.btnUpdateTemplates.Size = new System.Drawing.Size(131, 23);
			this.btnUpdateTemplates.TabIndex = 45;
			this.btnUpdateTemplates.Text = "Обновить шаблоны";
			this.btnUpdateTemplates.UseVisualStyleBackColor = true;
			this.btnUpdateTemplates.Visible = false;
			this.btnUpdateTemplates.Click += new System.EventHandler(this.btnUpdateTemplates_Click);
			// 
			// logo
			// 
			this.logo.BackColor = System.Drawing.Color.White;
			this.logo.BackgroundImage = global::HMSEditorNS.Properties.Resources.logo;
			this.logo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.logo.Location = new System.Drawing.Point(13, 14);
			this.logo.Name = "logo";
			this.logo.Size = new System.Drawing.Size(160, 160);
			this.logo.TabIndex = 38;
			// 
			// btnDelete
			// 
			this.btnDelete.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnDelete.Location = new System.Drawing.Point(406, 14);
			this.btnDelete.Name = "btnDelete";
			this.btnDelete.Size = new System.Drawing.Size(84, 23);
			this.btnDelete.TabIndex = 47;
			this.btnDelete.Text = "Удаление";
			this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
			// 
			// AboutDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(502, 325);
			this.Controls.Add(this.btnDelete);
			this.Controls.Add(this.labelNewTemplates);
			this.Controls.Add(this.btnUpdateTemplates);
			this.Controls.Add(this.linkLabel3);
			this.Controls.Add(this.linkLabel2);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.labelNewVersion);
			this.Controls.Add(this.progressBar1);
			this.Controls.Add(this.labelProductName);
			this.Controls.Add(this.labelVersion);
			this.Controls.Add(this.labelCopyright);
			this.Controls.Add(this.labelCompanyName);
			this.Controls.Add(this.textBoxDescription);
			this.Controls.Add(this.linkLabel1);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.btnUpdateProgram);
			this.Controls.Add(this.logo);
			this.DoubleBuffered = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AboutDialog";
			this.Padding = new System.Windows.Forms.Padding(9);
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "О программе";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AboutDialog_FormClosing);
			this.Load += new System.EventHandler(this.AboutDialog_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.Label labelProductName;
		private System.Windows.Forms.Label labelVersion;
		private System.Windows.Forms.Label labelCopyright;
		private System.Windows.Forms.Label labelCompanyName;
		private System.Windows.Forms.TextBox textBoxDescription;
		private System.Windows.Forms.LinkLabel linkLabel1;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button btnUpdateProgram;
		private Logo logo;
		private System.Windows.Forms.LinkLabel labelNewVersion;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.LinkLabel linkLabel2;
		private System.Windows.Forms.LinkLabel linkLabel3;
		private System.Windows.Forms.LinkLabel labelNewTemplates;
		private System.Windows.Forms.Button btnUpdateTemplates;
		private System.Windows.Forms.Button btnDelete;
	}
}
