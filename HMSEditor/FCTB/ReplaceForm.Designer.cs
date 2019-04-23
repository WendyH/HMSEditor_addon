namespace FastColoredTextBoxNS
{
    partial class ReplaceForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                timer.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReplaceForm));
            this.btFindNext = new System.Windows.Forms.Button();
            this.cbRegex = new System.Windows.Forms.CheckBox();
            this.cbMatchCase = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbWholeWord = new System.Windows.Forms.CheckBox();
            this.btnReplace = new System.Windows.Forms.Button();
            this.btnReplaceAll = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.tbReplace = new System.Windows.Forms.TextBox();
            this.lblFound = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.cbFind = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // btFindNext
            // 
            this.btFindNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btFindNext.Location = new System.Drawing.Point(109, 92);
            this.btFindNext.Name = "btFindNext";
            this.btFindNext.Size = new System.Drawing.Size(120, 25);
            this.btFindNext.TabIndex = 5;
            this.btFindNext.Text = "Найти следующий";
            this.btFindNext.UseVisualStyleBackColor = true;
            this.btFindNext.Click += new System.EventHandler(this.btFindNext_Click);
            // 
            // cbRegex
            // 
            this.cbRegex.AutoSize = true;
            this.cbRegex.Location = new System.Drawing.Point(364, 41);
            this.cbRegex.Name = "cbRegex";
            this.cbRegex.Size = new System.Drawing.Size(57, 17);
            this.cbRegex.TabIndex = 3;
            this.cbRegex.Text = "Regex";
            this.cbRegex.UseVisualStyleBackColor = true;
            this.cbRegex.CheckedChanged += new System.EventHandler(this.cbMatchCase_CheckedChanged);
            // 
            // cbMatchCase
            // 
            this.cbMatchCase.AutoSize = true;
            this.cbMatchCase.Location = new System.Drawing.Point(87, 41);
            this.cbMatchCase.Name = "cbMatchCase";
            this.cbMatchCase.Size = new System.Drawing.Size(131, 17);
            this.cbMatchCase.TabIndex = 1;
            this.cbMatchCase.Text = "Регистрозависимый";
            this.cbMatchCase.UseVisualStyleBackColor = true;
            this.cbMatchCase.CheckedChanged += new System.EventHandler(this.cbMatchCase_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(37, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Найти: ";
            // 
            // cbWholeWord
            // 
            this.cbWholeWord.AutoSize = true;
            this.cbWholeWord.Location = new System.Drawing.Point(224, 41);
            this.cbWholeWord.Name = "cbWholeWord";
            this.cbWholeWord.Size = new System.Drawing.Size(134, 17);
            this.cbWholeWord.TabIndex = 2;
            this.cbWholeWord.Text = "Как отдельное слово";
            this.cbWholeWord.UseVisualStyleBackColor = true;
            this.cbWholeWord.CheckedChanged += new System.EventHandler(this.cbMatchCase_CheckedChanged);
            // 
            // btnReplace
            // 
            this.btnReplace.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReplace.Location = new System.Drawing.Point(235, 92);
            this.btnReplace.Name = "btnReplace";
            this.btnReplace.Size = new System.Drawing.Size(90, 25);
            this.btnReplace.TabIndex = 6;
            this.btnReplace.Text = "Заменить";
            this.btnReplace.UseVisualStyleBackColor = true;
            this.btnReplace.Click += new System.EventHandler(this.btReplace_Click);
            // 
            // btnReplaceAll
            // 
            this.btnReplaceAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReplaceAll.Location = new System.Drawing.Point(331, 92);
            this.btnReplaceAll.Name = "btnReplaceAll";
            this.btnReplaceAll.Size = new System.Drawing.Size(90, 25);
            this.btnReplaceAll.TabIndex = 7;
            this.btnReplaceAll.Text = "Заменить все";
            this.btnReplaceAll.UseVisualStyleBackColor = true;
            this.btnReplaceAll.Click += new System.EventHandler(this.btReplaceAll_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 67);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Заменить на:";
            // 
            // tbReplace
            // 
            this.tbReplace.Location = new System.Drawing.Point(87, 64);
            this.tbReplace.Name = "tbReplace";
            this.tbReplace.Size = new System.Drawing.Size(334, 20);
            this.tbReplace.TabIndex = 0;
            this.tbReplace.TextChanged += new System.EventHandler(this.cbMatchCase_CheckedChanged);
            this.tbReplace.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cbFind_KeyPress);
            // 
            // lblFound
            // 
            this.lblFound.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFound.BackColor = System.Drawing.Color.Transparent;
            this.lblFound.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblFound.Location = new System.Drawing.Point(387, 17);
            this.lblFound.Name = "lblFound";
            this.lblFound.Size = new System.Drawing.Size(32, 16);
            this.lblFound.TabIndex = 10;
            this.lblFound.Text = "0";
            this.lblFound.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Image = global::HMSEditorNS.Properties.Resources.Close_12x;
            this.btnClose.Location = new System.Drawing.Point(416, 1);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(13, 12);
            this.btnClose.TabIndex = 11;
            this.btnClose.TabStop = false;
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // cbFind
            // 
            this.cbFind.FormattingEnabled = true;
            this.cbFind.Location = new System.Drawing.Point(87, 15);
            this.cbFind.Name = "cbFind";
            this.cbFind.Size = new System.Drawing.Size(294, 21);
            this.cbFind.TabIndex = 12;
            this.cbFind.TextChanged += new System.EventHandler(this.cbMatchCase_CheckedChanged);
            this.cbFind.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cbFind_KeyPress);
            // 
            // ReplaceForm
            // 
            this.AcceptButton = this.btFindNext;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(432, 125);
            this.Controls.Add(this.cbFind);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lblFound);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbReplace);
            this.Controls.Add(this.btnReplaceAll);
            this.Controls.Add(this.btnReplace);
            this.Controls.Add(this.cbWholeWord);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbMatchCase);
            this.Controls.Add(this.cbRegex);
            this.Controls.Add(this.btFindNext);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "ReplaceForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Поиск и замена текста";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ReplaceForm_FormClosing);
            this.Load += new System.EventHandler(this.ReplaceForm_Load);
            this.VisibleChanged += new System.EventHandler(this.ReplaceForm_VisibleChanged);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ReplaceForm_KeyPress);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ReplaceForm_MouseDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btFindNext;
        private System.Windows.Forms.CheckBox cbRegex;
        private System.Windows.Forms.CheckBox cbMatchCase;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbWholeWord;
        private System.Windows.Forms.Button btnReplace;
        private System.Windows.Forms.Button btnReplaceAll;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox tbReplace;
        private System.Windows.Forms.Label lblFound;
        private System.Windows.Forms.Button btnClose;
        public System.Windows.Forms.ComboBox cbFind;
    }
}