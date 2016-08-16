namespace HMSEditorNS {
    partial class DiffControl {
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

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DiffControl));
            FastColoredTextBoxNS.ServiceColors serviceColors1 = new FastColoredTextBoxNS.ServiceColors();
            FastColoredTextBoxNS.ServiceColors serviceColors2 = new FastColoredTextBoxNS.ServiceColors();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.label1 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItemLoad1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemInsert1 = new System.Windows.Forms.ToolStripMenuItem();
            this.label2 = new System.Windows.Forms.Label();
            this.menuStrip2 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItemLoad2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemInsert2 = new System.Windows.Forms.ToolStripMenuItem();
            this.tb1 = new FastColoredTextBoxNS.FastColoredTextBox();
            this.tb2 = new FastColoredTextBoxNS.FastColoredTextBox();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.menuStrip2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tb1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tb2)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tb1);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.menuStrip1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tb2);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Panel2.Controls.Add(this.menuStrip2);
            this.splitContainer1.Size = new System.Drawing.Size(641, 380);
            this.splitContainer1.SplitterDistance = 311;
            this.splitContainer1.SplitterWidth = 6;
            this.splitContainer1.TabIndex = 1;
            this.splitContainer1.TabStop = false;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.SystemColors.Control;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.label1.Location = new System.Drawing.Point(0, 24);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.label1.Size = new System.Drawing.Size(311, 19);
            this.label1.TabIndex = 3;
            this.label1.Text = "Исходный текст:";
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemLoad1,
            this.toolStripMenuItemInsert1});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(311, 24);
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
            // label2
            // 
            this.label2.BackColor = System.Drawing.SystemColors.Control;
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.label2.Location = new System.Drawing.Point(0, 24);
            this.label2.Name = "label2";
            this.label2.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.label2.Size = new System.Drawing.Size(324, 19);
            this.label2.TabIndex = 4;
            this.label2.Text = "Изменённый текст:";
            // 
            // menuStrip2
            // 
            this.menuStrip2.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.menuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemLoad2,
            this.toolStripMenuItemInsert2});
            this.menuStrip2.Location = new System.Drawing.Point(0, 0);
            this.menuStrip2.Name = "menuStrip2";
            this.menuStrip2.Size = new System.Drawing.Size(324, 24);
            this.menuStrip2.TabIndex = 0;
            this.menuStrip2.Text = "menuStrip2";
            // 
            // toolStripMenuItemLoad2
            // 
            this.toolStripMenuItemLoad2.Image = global::HMSEditorNS.Properties.Resources.Open_6529;
            this.toolStripMenuItemLoad2.Name = "toolStripMenuItemLoad2";
            this.toolStripMenuItemLoad2.Size = new System.Drawing.Size(28, 20);
            this.toolStripMenuItemLoad2.ToolTipText = "Загрузить из файла";
            this.toolStripMenuItemLoad2.Click += new System.EventHandler(this.toolStripMenuItemLoad2_Click);
            // 
            // toolStripMenuItemInsert2
            // 
            this.toolStripMenuItemInsert2.Image = global::HMSEditorNS.Properties.Resources.Paste_6520;
            this.toolStripMenuItemInsert2.Name = "toolStripMenuItemInsert2";
            this.toolStripMenuItemInsert2.Size = new System.Drawing.Size(187, 20);
            this.toolStripMenuItemInsert2.Text = "Вставить из буфера обмена";
            this.toolStripMenuItemInsert2.Click += new System.EventHandler(this.toolStripMenuItemInsert2_Click);
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
            this.tb1.HorizontalScrollVisible = true;
            this.tb1.Hotkeys = resources.GetString("tb1.Hotkeys");
            this.tb1.IsReplaceMode = false;
            this.tb1.Location = new System.Drawing.Point(0, 43);
            this.tb1.Name = "tb1";
            this.tb1.Paddings = new System.Windows.Forms.Padding(0);
            this.tb1.ReadOnly = true;
            serviceColors1.CollapseMarkerBackColor = System.Drawing.Color.White;
            serviceColors1.CollapseMarkerBorderColor = System.Drawing.Color.Silver;
            serviceColors1.CollapseMarkerForeColor = System.Drawing.Color.Silver;
            serviceColors1.ExpandMarkerBackColor = System.Drawing.Color.White;
            serviceColors1.ExpandMarkerBorderColor = System.Drawing.Color.Silver;
            serviceColors1.ExpandMarkerForeColor = System.Drawing.Color.Red;
            this.tb1.ServiceColors = serviceColors1;
            this.tb1.ShowChangedLinesOnScrollbar = false;
            this.tb1.ShowScrollBars = false;
            this.tb1.Size = new System.Drawing.Size(311, 337);
            this.tb1.TabIndex = 1;
            this.tb1.VerticalScrollVisible = true;
            this.tb1.Zoom = 100;
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
            this.tb2.HorizontalScrollVisible = true;
            this.tb2.Hotkeys = resources.GetString("tb2.Hotkeys");
            this.tb2.IsReplaceMode = false;
            this.tb2.Location = new System.Drawing.Point(0, 43);
            this.tb2.Name = "tb2";
            this.tb2.Paddings = new System.Windows.Forms.Padding(0);
            this.tb2.ReadOnly = true;
            serviceColors2.CollapseMarkerBackColor = System.Drawing.Color.White;
            serviceColors2.CollapseMarkerBorderColor = System.Drawing.Color.Silver;
            serviceColors2.CollapseMarkerForeColor = System.Drawing.Color.Silver;
            serviceColors2.ExpandMarkerBackColor = System.Drawing.Color.White;
            serviceColors2.ExpandMarkerBorderColor = System.Drawing.Color.Silver;
            serviceColors2.ExpandMarkerForeColor = System.Drawing.Color.Red;
            this.tb2.ServiceColors = serviceColors2;
            this.tb2.ShowChangedLinesOnScrollbar = false;
            this.tb2.ShowScrollBars = false;
            this.tb2.Size = new System.Drawing.Size(324, 337);
            this.tb2.TabIndex = 2;
            this.tb2.VerticalScrollVisible = true;
            this.tb2.Zoom = 100;
            // 
            // DiffControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "DiffControl";
            this.Size = new System.Drawing.Size(704, 380);
            this.Load += new System.EventHandler(this.DiffControl_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.menuStrip2.ResumeLayout(false);
            this.menuStrip2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tb1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tb2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private FastColoredTextBoxNS.FastColoredTextBox tb1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemLoad1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemInsert1;
        private FastColoredTextBoxNS.FastColoredTextBox tb2;
        private System.Windows.Forms.MenuStrip menuStrip2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemLoad2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemInsert2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}
