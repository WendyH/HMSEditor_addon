namespace Comparer {
    partial class formMain {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formMain));
            this.diffControl1 = new HMSEditorNS.DiffControl();
            this.SuspendLayout();
            // 
            // diffControl1
            // 
            this.diffControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.diffControl1.HideLineBreakInvisibleChar = true;
            this.diffControl1.Language = FastColoredTextBoxNS.Language.Custom;
            this.diffControl1.Location = new System.Drawing.Point(0, 0);
            this.diffControl1.Name = "diffControl1";
            this.diffControl1.NoSelectEmptyAreas = false;
            this.diffControl1.SelectionHighlightingForLineBreaksEnabled = true;
            this.diffControl1.SemanticMerge = false;
            this.diffControl1.ShowInvisibleCharsInSelection = true;
            this.diffControl1.Size = new System.Drawing.Size(800, 450);
            this.diffControl1.TabIndex = 0;
            this.diffControl1.TrimEndWhenDiff = true;
            // 
            // formMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.diffControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "formMain";
            this.Text = "Сравнение исходных текстов";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.formMain_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private HMSEditorNS.DiffControl diffControl1;
    }
}

