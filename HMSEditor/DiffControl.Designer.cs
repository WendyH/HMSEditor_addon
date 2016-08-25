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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DiffControl));
            FastColoredTextBoxNS.ServiceColors serviceColors1 = new FastColoredTextBoxNS.ServiceColors();
            FastColoredTextBoxNS.ServiceColors serviceColors2 = new FastColoredTextBoxNS.ServiceColors();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ToolStripMenuItemSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItemCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolStripMenuItemFind = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItemGoTo = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolStripMenuItemZoom100 = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItemLoad1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemInsert1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemPrevDiff1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemNextDiff1 = new System.Windows.Forms.ToolStripMenuItem();
            this.label2 = new System.Windows.Forms.Label();
            this.menuStrip2 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItemLoad2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemInsert2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemPrevDiff2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemNextDiff2 = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel4 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tb1 = new FastColoredTextBoxNS.FastColoredTextBox();
            this.tb2 = new FastColoredTextBoxNS.FastColoredTextBox();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.menuStrip2.SuspendLayout();
            this.statusStrip1.SuspendLayout();
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
            this.splitContainer1.Size = new System.Drawing.Size(850, 452);
            this.splitContainer1.SplitterDistance = 411;
            this.splitContainer1.SplitterWidth = 6;
            this.splitContainer1.TabIndex = 1;
            this.splitContainer1.TabStop = false;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItemSelectAll,
            this.ToolStripMenuItemCopy,
            this.toolStripSeparator1,
            this.ToolStripMenuItemFind,
            this.ToolStripMenuItemGoTo,
            this.toolStripSeparator10,
            this.ToolStripMenuItemZoom100});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(196, 126);
            // 
            // ToolStripMenuItemSelectAll
            // 
            this.ToolStripMenuItemSelectAll.Name = "ToolStripMenuItemSelectAll";
            this.ToolStripMenuItemSelectAll.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.ToolStripMenuItemSelectAll.Size = new System.Drawing.Size(195, 22);
            this.ToolStripMenuItemSelectAll.Text = "Выделить всё";
            this.ToolStripMenuItemSelectAll.Click += new System.EventHandler(this.ToolStripMenuItemSelectAll_Click);
            // 
            // ToolStripMenuItemCopy
            // 
            this.ToolStripMenuItemCopy.Image = global::HMSEditorNS.Properties.Resources.Copy_6524;
            this.ToolStripMenuItemCopy.Name = "ToolStripMenuItemCopy";
            this.ToolStripMenuItemCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.ToolStripMenuItemCopy.Size = new System.Drawing.Size(195, 22);
            this.ToolStripMenuItemCopy.Text = "Копировать";
            this.ToolStripMenuItemCopy.Click += new System.EventHandler(this.ToolStripMenuItemCopy_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(192, 6);
            // 
            // ToolStripMenuItemFind
            // 
            this.ToolStripMenuItemFind.Image = global::HMSEditorNS.Properties.Resources.Find_5650;
            this.ToolStripMenuItemFind.Name = "ToolStripMenuItemFind";
            this.ToolStripMenuItemFind.Size = new System.Drawing.Size(195, 22);
            this.ToolStripMenuItemFind.Text = "Найти";
            this.ToolStripMenuItemFind.Click += new System.EventHandler(this.ToolStripMenuItemFind_Click);
            // 
            // ToolStripMenuItemGoTo
            // 
            this.ToolStripMenuItemGoTo.Image = global::HMSEditorNS.Properties.Resources.HistoricalInstructionPointer_16x;
            this.ToolStripMenuItemGoTo.Name = "ToolStripMenuItemGoTo";
            this.ToolStripMenuItemGoTo.Size = new System.Drawing.Size(195, 22);
            this.ToolStripMenuItemGoTo.Text = "Перейти к строке №...";
            this.ToolStripMenuItemGoTo.Click += new System.EventHandler(this.ToolStripMenuItemGoTo_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(192, 6);
            // 
            // ToolStripMenuItemZoom100
            // 
            this.ToolStripMenuItemZoom100.Name = "ToolStripMenuItemZoom100";
            this.ToolStripMenuItemZoom100.Size = new System.Drawing.Size(195, 22);
            this.ToolStripMenuItemZoom100.Text = "Zoom 100%";
            this.ToolStripMenuItemZoom100.Click += new System.EventHandler(this.ToolStripMenuItemZoom100_Click);
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.SystemColors.Control;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.label1.Location = new System.Drawing.Point(0, 24);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.label1.Size = new System.Drawing.Size(411, 19);
            this.label1.TabIndex = 3;
            this.label1.Text = "Исходный текст:";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemLoad1,
            this.toolStripMenuItemInsert1,
            this.toolStripMenuItemPrevDiff1,
            this.toolStripMenuItemNextDiff1});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(411, 24);
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
            // toolStripMenuItemPrevDiff1
            // 
            this.toolStripMenuItemPrevDiff1.AutoToolTip = true;
            this.toolStripMenuItemPrevDiff1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripMenuItemPrevDiff1.Image = global::HMSEditorNS.Properties.Resources.FindPrevious_13244;
            this.toolStripMenuItemPrevDiff1.Name = "toolStripMenuItemPrevDiff1";
            this.toolStripMenuItemPrevDiff1.Size = new System.Drawing.Size(28, 20);
            this.toolStripMenuItemPrevDiff1.ToolTipText = "Перейти к предыдущему отличию";
            this.toolStripMenuItemPrevDiff1.Click += new System.EventHandler(this.toolStripMenuItemPrevDiff1_Click);
            // 
            // toolStripMenuItemNextDiff1
            // 
            this.toolStripMenuItemNextDiff1.AutoToolTip = true;
            this.toolStripMenuItemNextDiff1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripMenuItemNextDiff1.Image = global::HMSEditorNS.Properties.Resources.FindNext_13243;
            this.toolStripMenuItemNextDiff1.Name = "toolStripMenuItemNextDiff1";
            this.toolStripMenuItemNextDiff1.Size = new System.Drawing.Size(28, 20);
            this.toolStripMenuItemNextDiff1.ToolTipText = "Перейти к следующему отличию";
            this.toolStripMenuItemNextDiff1.Click += new System.EventHandler(this.toolStripMenuItemNextDiff1_Click);
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.SystemColors.Control;
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.label2.Location = new System.Drawing.Point(0, 24);
            this.label2.Name = "label2";
            this.label2.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.label2.Size = new System.Drawing.Size(433, 19);
            this.label2.TabIndex = 4;
            this.label2.Text = "Изменённый текст:";
            // 
            // menuStrip2
            // 
            this.menuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemLoad2,
            this.toolStripMenuItemInsert2,
            this.toolStripMenuItemPrevDiff2,
            this.toolStripMenuItemNextDiff2});
            this.menuStrip2.Location = new System.Drawing.Point(0, 0);
            this.menuStrip2.Name = "menuStrip2";
            this.menuStrip2.Size = new System.Drawing.Size(433, 24);
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
            // toolStripMenuItemPrevDiff2
            // 
            this.toolStripMenuItemPrevDiff2.AutoToolTip = true;
            this.toolStripMenuItemPrevDiff2.Image = global::HMSEditorNS.Properties.Resources.FindPrevious_13244;
            this.toolStripMenuItemPrevDiff2.Name = "toolStripMenuItemPrevDiff2";
            this.toolStripMenuItemPrevDiff2.Size = new System.Drawing.Size(28, 20);
            this.toolStripMenuItemPrevDiff2.ToolTipText = "Перейти к предыдущему отличию";
            this.toolStripMenuItemPrevDiff2.Click += new System.EventHandler(this.toolStripMenuItemPrevDiff2_Click);
            // 
            // toolStripMenuItemNextDiff2
            // 
            this.toolStripMenuItemNextDiff2.AutoToolTip = true;
            this.toolStripMenuItemNextDiff2.Image = global::HMSEditorNS.Properties.Resources.FindNext_13243;
            this.toolStripMenuItemNextDiff2.Name = "toolStripMenuItemNextDiff2";
            this.toolStripMenuItemNextDiff2.Size = new System.Drawing.Size(28, 20);
            this.toolStripMenuItemNextDiff2.ToolTipText = "Перейти к следующему отличию";
            this.toolStripMenuItemNextDiff2.Click += new System.EventHandler(this.toolStripMenuItemNextDiff2_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel3,
            this.toolStripStatusLabel4,
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2});
            this.statusStrip1.Location = new System.Drawing.Point(0, 453);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(913, 23);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.BackColor = System.Drawing.Color.Green;
            this.toolStripStatusLabel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(13, 15);
            this.toolStripStatusLabel1.Text = "  ";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(68, 18);
            this.toolStripStatusLabel2.Text = "Добавлено";
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.toolStripStatusLabel3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(13, 15);
            this.toolStripStatusLabel3.Text = "  ";
            // 
            // toolStripStatusLabel4
            // 
            this.toolStripStatusLabel4.Name = "toolStripStatusLabel4";
            this.toolStripStatusLabel4.Size = new System.Drawing.Size(59, 18);
            this.toolStripStatusLabel4.Text = "Удалено  ";
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
            this.tb1.ContextMenuStrip = this.contextMenuStrip1;
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
            this.tb1.RoundedCornersRadius = 3;
            serviceColors1.CollapseMarkerBackColor = System.Drawing.Color.White;
            serviceColors1.CollapseMarkerBorderColor = System.Drawing.Color.Silver;
            serviceColors1.CollapseMarkerForeColor = System.Drawing.Color.Silver;
            serviceColors1.ExpandMarkerBackColor = System.Drawing.Color.White;
            serviceColors1.ExpandMarkerBorderColor = System.Drawing.Color.Silver;
            serviceColors1.ExpandMarkerForeColor = System.Drawing.Color.Red;
            this.tb1.ServiceColors = serviceColors1;
            this.tb1.ShowChangedLinesOnScrollbar = false;
            this.tb1.ShowScrollBars = false;
            this.tb1.Size = new System.Drawing.Size(411, 409);
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
            this.tb2.ContextMenuStrip = this.contextMenuStrip1;
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
            this.tb2.RoundedCornersRadius = 3;
            serviceColors2.CollapseMarkerBackColor = System.Drawing.Color.White;
            serviceColors2.CollapseMarkerBorderColor = System.Drawing.Color.Silver;
            serviceColors2.CollapseMarkerForeColor = System.Drawing.Color.Silver;
            serviceColors2.ExpandMarkerBackColor = System.Drawing.Color.White;
            serviceColors2.ExpandMarkerBorderColor = System.Drawing.Color.Silver;
            serviceColors2.ExpandMarkerForeColor = System.Drawing.Color.Red;
            this.tb2.ServiceColors = serviceColors2;
            this.tb2.ShowChangedLinesOnScrollbar = false;
            this.tb2.ShowScrollBars = false;
            this.tb2.Size = new System.Drawing.Size(433, 409);
            this.tb2.TabIndex = 2;
            this.tb2.VerticalScrollVisible = true;
            this.tb2.Zoom = 100;
            // 
            // DiffControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Name = "DiffControl";
            this.Size = new System.Drawing.Size(913, 476);
            this.Load += new System.EventHandler(this.DiffControl_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.menuStrip2.ResumeLayout(false);
            this.menuStrip2.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tb1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tb2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemSelectAll;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemCopy;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemZoom100;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemFind;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemGoTo;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemPrevDiff1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemNextDiff1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemPrevDiff2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemNextDiff2;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel4;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
    }
}
