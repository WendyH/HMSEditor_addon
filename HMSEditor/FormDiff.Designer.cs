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
            FastColoredTextBoxNS.ServiceColors serviceColors1 = new FastColoredTextBoxNS.ServiceColors();
            FastColoredTextBoxNS.ServiceColors serviceColors2 = new FastColoredTextBoxNS.ServiceColors();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tb1 = new FastColoredTextBoxNS.FastColoredTextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItemLoad1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemInsert1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemCompare1 = new System.Windows.Forms.ToolStripMenuItem();
            this.tb2 = new FastColoredTextBoxNS.FastColoredTextBox();
            this.menuStrip2 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemInsert2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemCompare2 = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tb1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tb2)).BeginInit();
            this.menuStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(0, 4);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tb1);
            this.splitContainer1.Panel1.Controls.Add(this.menuStrip1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tb2);
            this.splitContainer1.Panel2.Controls.Add(this.menuStrip2);
            this.splitContainer1.Size = new System.Drawing.Size(797, 483);
            this.splitContainer1.SplitterDistance = 381;
            this.splitContainer1.TabIndex = 0;
            // 
            // tb1
            // 
            this.tb1.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
            this.tb1.AutoIndentExistingLines = false;
            this.tb1.AutoScrollMinSize = new System.Drawing.Size(24, 15);
            this.tb1.BackBrush = null;
            this.tb1.BoldCaret = false;
            this.tb1.BookmarkIcon = null;
            this.tb1.BreakpointIcon = null;
            this.tb1.BreakpointLineColor = System.Drawing.Color.Empty;
            this.tb1.CharHeight = 15;
            this.tb1.CharWidth = 7;
            this.tb1.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.tb1.DebugCurrentLineIcon = null;
            this.tb1.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.tb1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tb1.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.tb1.Hotkeys = resources.GetString("tb1.Hotkeys");
            this.tb1.IsReplaceMode = false;
            this.tb1.Location = new System.Drawing.Point(0, 24);
            this.tb1.Name = "tb1";
            this.tb1.Paddings = new System.Windows.Forms.Padding(0);
            serviceColors1.CollapseMarkerBackColor = System.Drawing.Color.White;
            serviceColors1.CollapseMarkerBorderColor = System.Drawing.Color.Silver;
            serviceColors1.CollapseMarkerForeColor = System.Drawing.Color.Silver;
            serviceColors1.ExpandMarkerBackColor = System.Drawing.Color.White;
            serviceColors1.ExpandMarkerBorderColor = System.Drawing.Color.Silver;
            serviceColors1.ExpandMarkerForeColor = System.Drawing.Color.Red;
            this.tb1.ServiceColors = serviceColors1;
            this.tb1.ShowChangedLinesOnScrollbar = false;
            this.tb1.ShowScrollBars = false;
            this.tb1.Size = new System.Drawing.Size(381, 459);
            this.tb1.TabIndex = 1;
            this.tb1.Zoom = 100;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemLoad1,
            this.toolStripMenuItemInsert1,
            this.toolStripMenuItemCompare1});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(381, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItemLoad1
            // 
            this.toolStripMenuItemLoad1.Image = global::HMSEditorNS.Properties.Resources.Open_6529;
            this.toolStripMenuItemLoad1.Name = "toolStripMenuItemLoad1";
            this.toolStripMenuItemLoad1.Size = new System.Drawing.Size(28, 20);
            this.toolStripMenuItemLoad1.ToolTipText = "Загрузить из файла";
            this.toolStripMenuItemLoad1.Click += new System.EventHandler(this.toolStripMenuItemLoad1_Click);
            // 
            // toolStripMenuItemInsert1
            // 
            this.toolStripMenuItemInsert1.Image = global::HMSEditorNS.Properties.Resources.Paste_6520;
            this.toolStripMenuItemInsert1.Name = "toolStripMenuItemInsert1";
            this.toolStripMenuItemInsert1.Size = new System.Drawing.Size(187, 20);
            this.toolStripMenuItemInsert1.Text = "Вставить из буфера обмена";
            this.toolStripMenuItemInsert1.Click += new System.EventHandler(this.toolStripMenuItemInsert1_Click);
            // 
            // toolStripMenuItemCompare1
            // 
            this.toolStripMenuItemCompare1.Image = global::HMSEditorNS.Properties.Resources.Compare_13153;
            this.toolStripMenuItemCompare1.Name = "toolStripMenuItemCompare1";
            this.toolStripMenuItemCompare1.Size = new System.Drawing.Size(87, 20);
            this.toolStripMenuItemCompare1.Text = "Сравнить";
            this.toolStripMenuItemCompare1.ToolTipText = "Сравнить";
            this.toolStripMenuItemCompare1.Click += new System.EventHandler(this.toolStripMenuItemCompare1_Click);
            // 
            // tb2
            // 
            this.tb2.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
            this.tb2.AutoIndentExistingLines = false;
            this.tb2.AutoScrollMinSize = new System.Drawing.Size(24, 15);
            this.tb2.BackBrush = null;
            this.tb2.BoldCaret = false;
            this.tb2.BookmarkIcon = null;
            this.tb2.BreakpointIcon = null;
            this.tb2.BreakpointLineColor = System.Drawing.Color.Empty;
            this.tb2.CharHeight = 15;
            this.tb2.CharWidth = 7;
            this.tb2.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.tb2.DebugCurrentLineIcon = null;
            this.tb2.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.tb2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tb2.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.tb2.Hotkeys = resources.GetString("tb2.Hotkeys");
            this.tb2.IsReplaceMode = false;
            this.tb2.Location = new System.Drawing.Point(0, 24);
            this.tb2.Name = "tb2";
            this.tb2.Paddings = new System.Windows.Forms.Padding(0);
            serviceColors2.CollapseMarkerBackColor = System.Drawing.Color.White;
            serviceColors2.CollapseMarkerBorderColor = System.Drawing.Color.Silver;
            serviceColors2.CollapseMarkerForeColor = System.Drawing.Color.Silver;
            serviceColors2.ExpandMarkerBackColor = System.Drawing.Color.White;
            serviceColors2.ExpandMarkerBorderColor = System.Drawing.Color.Silver;
            serviceColors2.ExpandMarkerForeColor = System.Drawing.Color.Red;
            this.tb2.ServiceColors = serviceColors2;
            this.tb2.ShowChangedLinesOnScrollbar = false;
            this.tb2.ShowScrollBars = false;
            this.tb2.Size = new System.Drawing.Size(412, 459);
            this.tb2.TabIndex = 2;
            this.tb2.Zoom = 100;
            // 
            // menuStrip2
            // 
            this.menuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripMenuItemInsert2,
            this.toolStripMenuItemCompare2});
            this.menuStrip2.Location = new System.Drawing.Point(0, 0);
            this.menuStrip2.Name = "menuStrip2";
            this.menuStrip2.Size = new System.Drawing.Size(412, 24);
            this.menuStrip2.TabIndex = 0;
            this.menuStrip2.Text = "menuStrip2";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Image = global::HMSEditorNS.Properties.Resources.Open_6529;
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(28, 20);
            this.toolStripMenuItem1.ToolTipText = "Загрузить из файла";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItemLoad2_Click);
            // 
            // toolStripMenuItemInsert2
            // 
            this.toolStripMenuItemInsert2.Image = global::HMSEditorNS.Properties.Resources.Paste_6520;
            this.toolStripMenuItemInsert2.Name = "toolStripMenuItemInsert2";
            this.toolStripMenuItemInsert2.Size = new System.Drawing.Size(187, 20);
            this.toolStripMenuItemInsert2.Text = "Вставить из буфера обмена";
            this.toolStripMenuItemInsert2.Click += new System.EventHandler(this.toolStripMenuItemInsert2_Click);
            // 
            // toolStripMenuItemCompare2
            // 
            this.toolStripMenuItemCompare2.Image = global::HMSEditorNS.Properties.Resources.Compare_13153;
            this.toolStripMenuItemCompare2.Name = "toolStripMenuItemCompare2";
            this.toolStripMenuItemCompare2.Size = new System.Drawing.Size(87, 20);
            this.toolStripMenuItemCompare2.Text = "Сравнить";
            this.toolStripMenuItemCompare2.ToolTipText = "Сравнить";
            this.toolStripMenuItemCompare2.Click += new System.EventHandler(this.toolStripMenuItemCompare2_Click);
            // 
            // FormDiff
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(872, 487);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FormDiff";
            this.Text = "Сравнение исходного кода";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tb1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tb2)).EndInit();
            this.menuStrip2.ResumeLayout(false);
            this.menuStrip2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private FastColoredTextBoxNS.FastColoredTextBox tb1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemLoad1;
        private FastColoredTextBoxNS.FastColoredTextBox tb2;
        private System.Windows.Forms.MenuStrip menuStrip2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCompare2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemInsert1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemInsert2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCompare1;
    }
}