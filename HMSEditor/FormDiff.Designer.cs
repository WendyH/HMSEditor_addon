namespace HMSEditorNS {
    partial class FormDiff {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDiff));
            this.diffControl1 = new HMSEditorNS.DiffControl();
            this.SuspendLayout();
            // 
            // diffControl1
            // 
            this.diffControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.diffControl1.Language = FastColoredTextBoxNS.Language.Custom;
            this.diffControl1.Location = new System.Drawing.Point(0, 0);
            this.diffControl1.Name = "diffControl1";
            this.diffControl1.Size = new System.Drawing.Size(872, 487);
            this.diffControl1.TabIndex = 0;
            // 
            // FormDiff
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(872, 487);
            this.Controls.Add(this.diffControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "FormDiff";
            this.Text = "Сравнение исходного кода";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormDiff_FormClosing);
            this.Load += new System.EventHandler(this.FormDiff_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormDiff_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private DiffControl diffControl1;
    }
}