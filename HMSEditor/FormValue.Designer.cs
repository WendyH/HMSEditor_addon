namespace HMSEditorNS {
    partial class FormValue {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormValue));
            FastColoredTextBoxNS.ServiceColors serviceColors1 = new FastColoredTextBoxNS.ServiceColors();
            this.panel1 = new System.Windows.Forms.Panel();
            this.chkOnTop = new System.Windows.Forms.CheckBox();
            this.chkWordWrap = new System.Windows.Forms.CheckBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.panelTop = new System.Windows.Forms.Panel();
            this.groupRegex = new System.Windows.Forms.GroupBox();
            this.tbResult = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cbResultIndex = new System.Windows.Forms.ComboBox();
            this.chkHTML2Text = new System.Windows.Forms.CheckBox();
            this.cbPattern = new System.Windows.Forms.ComboBox();
            this.tbRegexCount = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkPCRE_CASELESS = new System.Windows.Forms.CheckBox();
            this.chkPCRE_MULTILINE = new System.Windows.Forms.CheckBox();
            this.chkPCRE_SINGLELINE = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.chkUseRegex = new System.Windows.Forms.CheckBox();
            this.tbExpression = new System.Windows.Forms.TextBox();
            this.lblExpression = new System.Windows.Forms.Label();
            this.fastColoredTB = new FastColoredTextBoxNS.FastColoredTextBox();
            this.panel1.SuspendLayout();
            this.panelTop.SuspendLayout();
            this.groupRegex.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fastColoredTB)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.panel1.Controls.Add(this.chkOnTop);
            this.panel1.Controls.Add(this.chkWordWrap);
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 366);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(692, 42);
            this.panel1.TabIndex = 1;
            // 
            // chkOnTop
            // 
            this.chkOnTop.AutoSize = true;
            this.chkOnTop.Checked = true;
            this.chkOnTop.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOnTop.Location = new System.Drawing.Point(161, 15);
            this.chkOnTop.Name = "chkOnTop";
            this.chkOnTop.Size = new System.Drawing.Size(120, 17);
            this.chkOnTop.TabIndex = 2;
            this.chkOnTop.Text = "Поверх всех форм";
            this.chkOnTop.UseVisualStyleBackColor = true;
            this.chkOnTop.CheckedChanged += new System.EventHandler(this.chkOnTop_CheckedChanged);
            // 
            // chkWordWrap
            // 
            this.chkWordWrap.AutoSize = true;
            this.chkWordWrap.Checked = true;
            this.chkWordWrap.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkWordWrap.Location = new System.Drawing.Point(12, 15);
            this.chkWordWrap.Name = "chkWordWrap";
            this.chkWordWrap.Size = new System.Drawing.Size(143, 17);
            this.chkWordWrap.TabIndex = 1;
            this.chkWordWrap.Text = "Переносить по словам";
            this.chkWordWrap.UseVisualStyleBackColor = true;
            this.chkWordWrap.CheckedChanged += new System.EventHandler(this.chkWordWrap_CheckedChanged);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(597, 9);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(87, 27);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Закрыть";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.groupRegex);
            this.panelTop.Controls.Add(this.chkUseRegex);
            this.panelTop.Controls.Add(this.tbExpression);
            this.panelTop.Controls.Add(this.lblExpression);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(692, 151);
            this.panelTop.TabIndex = 2;
            // 
            // groupRegex
            // 
            this.groupRegex.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupRegex.Controls.Add(this.tbResult);
            this.groupRegex.Controls.Add(this.label3);
            this.groupRegex.Controls.Add(this.cbResultIndex);
            this.groupRegex.Controls.Add(this.chkHTML2Text);
            this.groupRegex.Controls.Add(this.cbPattern);
            this.groupRegex.Controls.Add(this.tbRegexCount);
            this.groupRegex.Controls.Add(this.label2);
            this.groupRegex.Controls.Add(this.chkPCRE_CASELESS);
            this.groupRegex.Controls.Add(this.chkPCRE_MULTILINE);
            this.groupRegex.Controls.Add(this.chkPCRE_SINGLELINE);
            this.groupRegex.Controls.Add(this.label1);
            this.groupRegex.Location = new System.Drawing.Point(7, 53);
            this.groupRegex.MinimumSize = new System.Drawing.Size(682, 92);
            this.groupRegex.Name = "groupRegex";
            this.groupRegex.Size = new System.Drawing.Size(682, 92);
            this.groupRegex.TabIndex = 9;
            this.groupRegex.TabStop = false;
            // 
            // tbResult
            // 
            this.tbResult.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbResult.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tbResult.Location = new System.Drawing.Point(443, 61);
            this.tbResult.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.tbResult.Name = "tbResult";
            this.tbResult.ReadOnly = true;
            this.tbResult.Size = new System.Drawing.Size(232, 23);
            this.tbResult.TabIndex = 21;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(372, 66);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 13);
            this.label3.TabIndex = 20;
            this.label3.Text = "Результат:";
            // 
            // cbResultIndex
            // 
            this.cbResultIndex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbResultIndex.FormattingEnabled = true;
            this.cbResultIndex.Location = new System.Drawing.Point(227, 63);
            this.cbResultIndex.Name = "cbResultIndex";
            this.cbResultIndex.Size = new System.Drawing.Size(137, 21);
            this.cbResultIndex.TabIndex = 19;
            // 
            // chkHTML2Text
            // 
            this.chkHTML2Text.AutoSize = true;
            this.chkHTML2Text.Location = new System.Drawing.Point(12, 65);
            this.chkHTML2Text.Name = "chkHTML2Text";
            this.chkHTML2Text.Size = new System.Drawing.Size(209, 17);
            this.chkHTML2Text.TabIndex = 18;
            this.chkHTML2Text.Text = "Применить HtmlToText к результату";
            this.chkHTML2Text.UseVisualStyleBackColor = true;
            this.chkHTML2Text.CheckedChanged += new System.EventHandler(this.chkHTML2Text_CheckedChanged);
            // 
            // cbPattern
            // 
            this.cbPattern.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cbPattern.FormattingEnabled = true;
            this.cbPattern.Location = new System.Drawing.Point(12, 32);
            this.cbPattern.Name = "cbPattern";
            this.cbPattern.Size = new System.Drawing.Size(249, 24);
            this.cbPattern.TabIndex = 17;
            this.cbPattern.SelectedIndexChanged += new System.EventHandler(this.cbPattern_SelectedIndexChanged);
            this.cbPattern.SelectedValueChanged += new System.EventHandler(this.cbPattern_SelectedValueChanged);
            this.cbPattern.TextChanged += new System.EventHandler(this.cbPattern_TextChanged);
            this.cbPattern.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cbPattern_KeyDown);
            // 
            // tbRegexCount
            // 
            this.tbRegexCount.Font = new System.Drawing.Font("Arial Unicode MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tbRegexCount.Location = new System.Drawing.Point(623, 15);
            this.tbRegexCount.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.tbRegexCount.Name = "tbRegexCount";
            this.tbRegexCount.ReadOnly = true;
            this.tbRegexCount.Size = new System.Drawing.Size(51, 24);
            this.tbRegexCount.TabIndex = 16;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(551, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "Совпадений:";
            // 
            // chkPCRE_CASELESS
            // 
            this.chkPCRE_CASELESS.AutoSize = true;
            this.chkPCRE_CASELESS.Location = new System.Drawing.Point(281, 36);
            this.chkPCRE_CASELESS.Name = "chkPCRE_CASELESS";
            this.chkPCRE_CASELESS.Size = new System.Drawing.Size(116, 17);
            this.chkPCRE_CASELESS.TabIndex = 14;
            this.chkPCRE_CASELESS.Text = "PCRE_CASELESS";
            this.chkPCRE_CASELESS.UseVisualStyleBackColor = true;
            this.chkPCRE_CASELESS.CheckedChanged += new System.EventHandler(this.chkPCRE_CASELESS_CheckedChanged);
            // 
            // chkPCRE_MULTILINE
            // 
            this.chkPCRE_MULTILINE.AutoSize = true;
            this.chkPCRE_MULTILINE.Location = new System.Drawing.Point(411, 16);
            this.chkPCRE_MULTILINE.Name = "chkPCRE_MULTILINE";
            this.chkPCRE_MULTILINE.Size = new System.Drawing.Size(118, 17);
            this.chkPCRE_MULTILINE.TabIndex = 13;
            this.chkPCRE_MULTILINE.Text = "PCRE_MULTILINE";
            this.chkPCRE_MULTILINE.UseVisualStyleBackColor = true;
            this.chkPCRE_MULTILINE.CheckedChanged += new System.EventHandler(this.chkPCRE_MULTILINE_CheckedChanged);
            // 
            // chkPCRE_SINGLELINE
            // 
            this.chkPCRE_SINGLELINE.AutoSize = true;
            this.chkPCRE_SINGLELINE.Location = new System.Drawing.Point(281, 16);
            this.chkPCRE_SINGLELINE.Name = "chkPCRE_SINGLELINE";
            this.chkPCRE_SINGLELINE.Size = new System.Drawing.Size(124, 17);
            this.chkPCRE_SINGLELINE.TabIndex = 11;
            this.chkPCRE_SINGLELINE.Text = "PCRE_SINGLELINE";
            this.chkPCRE_SINGLELINE.UseVisualStyleBackColor = true;
            this.chkPCRE_SINGLELINE.CheckedChanged += new System.EventHandler(this.chkPCRE_SINGLELINE_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(130, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Регулярное выражение:";
            // 
            // chkUseRegex
            // 
            this.chkUseRegex.AutoSize = true;
            this.chkUseRegex.Location = new System.Drawing.Point(12, 36);
            this.chkUseRegex.Name = "chkUseRegex";
            this.chkUseRegex.Size = new System.Drawing.Size(132, 17);
            this.chkUseRegex.TabIndex = 2;
            this.chkUseRegex.Text = "Инструменты Regex:";
            this.chkUseRegex.UseVisualStyleBackColor = true;
            this.chkUseRegex.CheckedChanged += new System.EventHandler(this.chkUseRegex_CheckedChanged);
            // 
            // tbExpression
            // 
            this.tbExpression.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbExpression.Font = new System.Drawing.Font("Arial Unicode MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tbExpression.Location = new System.Drawing.Point(77, 6);
            this.tbExpression.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.tbExpression.Name = "tbExpression";
            this.tbExpression.ReadOnly = true;
            this.tbExpression.Size = new System.Drawing.Size(607, 24);
            this.tbExpression.TabIndex = 1;
            // 
            // lblExpression
            // 
            this.lblExpression.AutoSize = true;
            this.lblExpression.Location = new System.Drawing.Point(5, 10);
            this.lblExpression.Name = "lblExpression";
            this.lblExpression.Size = new System.Drawing.Size(69, 13);
            this.lblExpression.TabIndex = 0;
            this.lblExpression.Text = "Выражение:";
            // 
            // fastColoredTB
            // 
            this.fastColoredTB.AcceptsReturn = false;
            this.fastColoredTB.AcceptsTab = false;
            this.fastColoredTB.AllowDrop = false;
            this.fastColoredTB.AllowMacroRecording = false;
            this.fastColoredTB.AutoCompleteBracketsList = new char[0];
            this.fastColoredTB.AutoIndent = false;
            this.fastColoredTB.AutoIndentChars = false;
            this.fastColoredTB.AutoIndentCharsPatterns = "";
            this.fastColoredTB.AutoIndentExistingLines = false;
            this.fastColoredTB.AutoScrollMinSize = new System.Drawing.Size(0, 15);
            this.fastColoredTB.BackBrush = null;
            this.fastColoredTB.BoldCaret = false;
            this.fastColoredTB.BookmarkIcon = null;
            this.fastColoredTB.BreakpointIcon = null;
            this.fastColoredTB.BreakpointLineColor = System.Drawing.Color.Empty;
            this.fastColoredTB.CharHeight = 15;
            this.fastColoredTB.CharWidth = 7;
            this.fastColoredTB.CommentPrefix = null;
            this.fastColoredTB.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.fastColoredTB.DebugCurrentLineIcon = null;
            this.fastColoredTB.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.fastColoredTB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fastColoredTB.FindEndOfFoldingBlockStrategy = FastColoredTextBoxNS.FindEndOfFoldingBlockStrategy.StrategyIndent;
            this.fastColoredTB.FoldingIndicatorColor = System.Drawing.Color.Transparent;
            this.fastColoredTB.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.fastColoredTB.HighlightFoldingIndicator = false;
            this.fastColoredTB.Hotkeys = resources.GetString("fastColoredTB.Hotkeys");
            this.fastColoredTB.IsReplaceMode = false;
            this.fastColoredTB.Language = FastColoredTextBoxNS.Language.HTML;
            this.fastColoredTB.LeftBracket = '<';
            this.fastColoredTB.LeftBracket2 = '(';
            this.fastColoredTB.Location = new System.Drawing.Point(0, 151);
            this.fastColoredTB.Name = "fastColoredTB";
            this.fastColoredTB.Paddings = new System.Windows.Forms.Padding(0);
            this.fastColoredTB.ReadOnly = true;
            this.fastColoredTB.RightBracket = '>';
            this.fastColoredTB.RightBracket2 = ')';
            this.fastColoredTB.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.fastColoredTB.SelectionHighlightingForLineBreaksEnabled = false;
            serviceColors1.CollapseMarkerBackColor = System.Drawing.Color.White;
            serviceColors1.CollapseMarkerBorderColor = System.Drawing.Color.Silver;
            serviceColors1.CollapseMarkerForeColor = System.Drawing.Color.Silver;
            serviceColors1.ExpandMarkerBackColor = System.Drawing.Color.White;
            serviceColors1.ExpandMarkerBorderColor = System.Drawing.Color.Silver;
            serviceColors1.ExpandMarkerForeColor = System.Drawing.Color.Red;
            this.fastColoredTB.ServiceColors = serviceColors1;
            this.fastColoredTB.ShowLineNumbers = false;
            this.fastColoredTB.ShowScrollBars = false;
            this.fastColoredTB.Size = new System.Drawing.Size(692, 215);
            this.fastColoredTB.SourceTextBox = this.fastColoredTB;
            this.fastColoredTB.TabIndex = 0;
            this.fastColoredTB.Text = "<HTML>";
            this.fastColoredTB.WordWrap = true;
            this.fastColoredTB.WordWrapAutoIndent = false;
            this.fastColoredTB.Zoom = 100;
            // 
            // FormValue
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(692, 408);
            this.Controls.Add(this.fastColoredTB);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.panel1);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(392, 151);
            this.Name = "FormValue";
            this.ShowIcon = false;
            this.Text = "Просмотр значения";
            this.TopMost = true;
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormValue_KeyDown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.groupRegex.ResumeLayout(false);
            this.groupRegex.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fastColoredTB)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox chkWordWrap;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.TextBox tbExpression;
        private System.Windows.Forms.Label lblExpression;
        private FastColoredTextBoxNS.FastColoredTextBox fastColoredTB;
        private System.Windows.Forms.GroupBox groupRegex;
        private System.Windows.Forms.ComboBox cbPattern;
        private System.Windows.Forms.TextBox tbRegexCount;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkPCRE_CASELESS;
        private System.Windows.Forms.CheckBox chkPCRE_MULTILINE;
        private System.Windows.Forms.CheckBox chkPCRE_SINGLELINE;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkUseRegex;
        private System.Windows.Forms.ComboBox cbResultIndex;
        private System.Windows.Forms.CheckBox chkHTML2Text;
        private System.Windows.Forms.TextBox tbResult;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkOnTop;
    }
}