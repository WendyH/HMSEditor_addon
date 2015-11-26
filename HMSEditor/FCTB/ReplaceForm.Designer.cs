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
			this.btClose = new System.Windows.Forms.Button();
			this.btFindNext = new System.Windows.Forms.Button();
			this.tbFind = new System.Windows.Forms.TextBox();
			this.cbRegex = new System.Windows.Forms.CheckBox();
			this.cbMatchCase = new System.Windows.Forms.CheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this.cbWholeWord = new System.Windows.Forms.CheckBox();
			this.btReplace = new System.Windows.Forms.Button();
			this.btReplaceAll = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.tbReplace = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// btClose
			// 
			this.btClose.Location = new System.Drawing.Point(351, 123);
			this.btClose.Name = "btClose";
			this.btClose.Size = new System.Drawing.Size(90, 24);
			this.btClose.TabIndex = 8;
			this.btClose.Text = "Закрыть";
			this.btClose.UseVisualStyleBackColor = true;
			this.btClose.Click += new System.EventHandler(this.btClose_Click);
			// 
			// btFindNext
			// 
			this.btFindNext.Location = new System.Drawing.Point(129, 90);
			this.btFindNext.Name = "btFindNext";
			this.btFindNext.Size = new System.Drawing.Size(120, 27);
			this.btFindNext.TabIndex = 5;
			this.btFindNext.Text = "Найти следующий";
			this.btFindNext.UseVisualStyleBackColor = true;
			this.btFindNext.Click += new System.EventHandler(this.btFindNext_Click);
			// 
			// tbFind
			// 
			this.tbFind.Location = new System.Drawing.Point(87, 12);
			this.tbFind.Name = "tbFind";
			this.tbFind.Size = new System.Drawing.Size(354, 20);
			this.tbFind.TabIndex = 0;
			this.tbFind.TextChanged += new System.EventHandler(this.cbMatchCase_CheckedChanged);
			this.tbFind.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbFind_KeyPress);
			// 
			// cbRegex
			// 
			this.cbRegex.AutoSize = true;
			this.cbRegex.Location = new System.Drawing.Point(371, 38);
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
			this.cbMatchCase.Location = new System.Drawing.Point(87, 38);
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
			this.label1.Location = new System.Drawing.Point(37, 15);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(44, 13);
			this.label1.TabIndex = 5;
			this.label1.Text = "Найти: ";
			// 
			// cbWholeWord
			// 
			this.cbWholeWord.AutoSize = true;
			this.cbWholeWord.Location = new System.Drawing.Point(224, 38);
			this.cbWholeWord.Name = "cbWholeWord";
			this.cbWholeWord.Size = new System.Drawing.Size(141, 17);
			this.cbWholeWord.TabIndex = 2;
			this.cbWholeWord.Text = "По отдельным словам";
			this.cbWholeWord.UseVisualStyleBackColor = true;
			this.cbWholeWord.CheckedChanged += new System.EventHandler(this.cbMatchCase_CheckedChanged);
			// 
			// btReplace
			// 
			this.btReplace.Location = new System.Drawing.Point(255, 90);
			this.btReplace.Name = "btReplace";
			this.btReplace.Size = new System.Drawing.Size(90, 27);
			this.btReplace.TabIndex = 6;
			this.btReplace.Text = "Заменить";
			this.btReplace.UseVisualStyleBackColor = true;
			this.btReplace.Click += new System.EventHandler(this.btReplace_Click);
			// 
			// btReplaceAll
			// 
			this.btReplaceAll.Location = new System.Drawing.Point(351, 90);
			this.btReplaceAll.Name = "btReplaceAll";
			this.btReplaceAll.Size = new System.Drawing.Size(90, 27);
			this.btReplaceAll.TabIndex = 7;
			this.btReplaceAll.Text = "Заменить все";
			this.btReplaceAll.UseVisualStyleBackColor = true;
			this.btReplaceAll.Click += new System.EventHandler(this.btReplaceAll_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 64);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(75, 13);
			this.label2.TabIndex = 9;
			this.label2.Text = "Заменить на:";
			// 
			// tbReplace
			// 
			this.tbReplace.Location = new System.Drawing.Point(87, 61);
			this.tbReplace.Name = "tbReplace";
			this.tbReplace.Size = new System.Drawing.Size(354, 20);
			this.tbReplace.TabIndex = 0;
			this.tbReplace.TextChanged += new System.EventHandler(this.cbMatchCase_CheckedChanged);
			this.tbReplace.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbFind_KeyPress);
			// 
			// ReplaceForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(449, 157);
			this.Controls.Add(this.tbFind);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.tbReplace);
			this.Controls.Add(this.btReplaceAll);
			this.Controls.Add(this.btReplace);
			this.Controls.Add(this.cbWholeWord);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.cbMatchCase);
			this.Controls.Add(this.cbRegex);
			this.Controls.Add(this.btFindNext);
			this.Controls.Add(this.btClose);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "ReplaceForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Поиск и замена текста";
			this.TopMost = true;
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ReplaceForm_FormClosing);
			this.Load += new System.EventHandler(this.ReplaceForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btClose;
        private System.Windows.Forms.Button btFindNext;
        private System.Windows.Forms.CheckBox cbRegex;
        private System.Windows.Forms.CheckBox cbMatchCase;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbWholeWord;
        private System.Windows.Forms.Button btReplace;
        private System.Windows.Forms.Button btReplaceAll;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox tbFind;
        public System.Windows.Forms.TextBox tbReplace;
    }
}