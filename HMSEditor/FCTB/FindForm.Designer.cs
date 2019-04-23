namespace FastColoredTextBoxNS
{
    partial class FindForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FindForm));
            this.cbRegex = new System.Windows.Forms.CheckBox();
            this.cbMatchCase = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbWholeWord = new System.Windows.Forms.CheckBox();
            this.lblFound = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.cbFind = new System.Windows.Forms.ComboBox();
            this.btnForw = new HMSEditorNS.FlatButton();
            this.btnBack = new HMSEditorNS.FlatButton();
            this.SuspendLayout();
            // 
            // cbRegex
            // 
            this.cbRegex.AutoSize = true;
            this.cbRegex.Location = new System.Drawing.Point(286, 44);
            this.cbRegex.Name = "cbRegex";
            this.cbRegex.Size = new System.Drawing.Size(57, 17);
            this.cbRegex.TabIndex = 3;
            this.cbRegex.Text = "Regex";
            this.cbRegex.CheckedChanged += new System.EventHandler(this.cbMatchCase_CheckedChanged);
            this.cbRegex.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cbRegex_KeyPress);
            // 
            // cbMatchCase
            // 
            this.cbMatchCase.AutoSize = true;
            this.cbMatchCase.Location = new System.Drawing.Point(9, 44);
            this.cbMatchCase.Name = "cbMatchCase";
            this.cbMatchCase.Size = new System.Drawing.Size(131, 17);
            this.cbMatchCase.TabIndex = 1;
            this.cbMatchCase.Text = "Регистрозависимый";
            this.cbMatchCase.CheckedChanged += new System.EventHandler(this.cbMatchCase_CheckedChanged);
            this.cbMatchCase.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cbMatchCase_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Поиск: ";
            // 
            // cbWholeWord
            // 
            this.cbWholeWord.AutoSize = true;
            this.cbWholeWord.Location = new System.Drawing.Point(146, 44);
            this.cbWholeWord.Name = "cbWholeWord";
            this.cbWholeWord.Size = new System.Drawing.Size(134, 17);
            this.cbWholeWord.TabIndex = 2;
            this.cbWholeWord.Text = "Как отдельное слово";
            this.cbWholeWord.CheckedChanged += new System.EventHandler(this.cbMatchCase_CheckedChanged);
            this.cbWholeWord.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cbWholeWord_KeyPress);
            // 
            // lblFound
            // 
            this.lblFound.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFound.BackColor = System.Drawing.Color.Transparent;
            this.lblFound.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblFound.Location = new System.Drawing.Point(309, 20);
            this.lblFound.Name = "lblFound";
            this.lblFound.Size = new System.Drawing.Size(32, 16);
            this.lblFound.TabIndex = 6;
            this.lblFound.Text = "0";
            this.lblFound.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Image = global::HMSEditorNS.Properties.Resources.Close_12x;
            this.btnClose.Location = new System.Drawing.Point(338, 2);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(13, 12);
            this.btnClose.TabIndex = 9;
            this.btnClose.TabStop = false;
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.btnClose.Leave += new System.EventHandler(this.BtnClose_Leave);
            // 
            // cbFind
            // 
            this.cbFind.FormattingEnabled = true;
            this.cbFind.Location = new System.Drawing.Point(48, 18);
            this.cbFind.Name = "cbFind";
            this.cbFind.Size = new System.Drawing.Size(221, 21);
            this.cbFind.TabIndex = 10;
            this.cbFind.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cbFind_KeyPress);
            // 
            // btnForw
            // 
            this.btnForw.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnForw.Image = global::HMSEditorNS.Properties.Resources.Down;
            this.btnForw.Location = new System.Drawing.Point(294, 18);
            this.btnForw.Name = "btnForw";
            this.btnForw.Size = new System.Drawing.Size(21, 19);
            this.btnForw.TabIndex = 8;
            this.btnForw.TabStop = false;
            this.btnForw.UseVisualStyleBackColor = true;
            this.btnForw.Click += new System.EventHandler(this.btnForw_Click);
            // 
            // btnBack
            // 
            this.btnBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBack.Image = global::HMSEditorNS.Properties.Resources.Up;
            this.btnBack.Location = new System.Drawing.Point(272, 18);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(21, 19);
            this.btnBack.TabIndex = 7;
            this.btnBack.TabStop = false;
            this.btnBack.UseVisualStyleBackColor = true;
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // FindForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(354, 71);
            this.Controls.Add(this.cbFind);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnForw);
            this.Controls.Add(this.btnBack);
            this.Controls.Add(this.lblFound);
            this.Controls.Add(this.cbWholeWord);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbMatchCase);
            this.Controls.Add(this.cbRegex);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "FindForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Поиск";
            this.Activated += new System.EventHandler(this.FindForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FindForm_FormClosing);
            this.VisibleChanged += new System.EventHandler(this.FindForm_VisibleChanged);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FindForm_KeyPress);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FindForm_MouseDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.CheckBox cbRegex;
        private System.Windows.Forms.CheckBox cbMatchCase;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbWholeWord;
        private System.Windows.Forms.Label lblFound;
        private HMSEditorNS.FlatButton btnBack;
        private HMSEditorNS.FlatButton btnForw;
        private System.Windows.Forms.Button btnClose;
        public System.Windows.Forms.ComboBox cbFind;
    }
}