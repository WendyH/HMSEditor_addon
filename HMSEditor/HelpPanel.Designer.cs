namespace HMSEditorNS {
	partial class HelpPanel {
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
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.flatListBox1 = new HMSEditorNS.FlatListBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.HelpTextBox = new MyRichTextBox();
            this.TopPanel = new System.Windows.Forms.Panel();
            this.valueToolTip1 = new HMSEditorNS.ValueToolTip();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.TopPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboBox1
            // 
            this.comboBox1.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.comboBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.Simple;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(0, 0);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(352, 21);
            this.comboBox1.TabIndex = 1;
            this.comboBox1.TabStop = false;
            this.comboBox1.TextChanged += new System.EventHandler(this.comboBox1_TextChanged);
            this.comboBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.comboBox1_KeyDown);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 21);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panel1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panel2);
            this.splitContainer1.Size = new System.Drawing.Size(352, 573);
            this.splitContainer1.SplitterDistance = 261;
            this.splitContainer1.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.flatListBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.panel1.Size = new System.Drawing.Size(352, 261);
            this.panel1.TabIndex = 1;
            // 
            // flatListBox1
            // 
            this.flatListBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flatListBox1.FocussedItem = null;
            this.flatListBox1.FocussedItemIndex = 0;
            this.flatListBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
            this.flatListBox1.HoveredColor = System.Drawing.Color.Red;
            this.flatListBox1.ImageList = null;
            this.flatListBox1.Location = new System.Drawing.Point(0, 4);
            this.flatListBox1.Name = "flatListBox1";
            this.flatListBox1.SelectedColor = System.Drawing.Color.CornflowerBlue;
            this.flatListBox1.Size = new System.Drawing.Size(352, 257);
            this.flatListBox1.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.HelpTextBox);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(352, 308);
            this.panel2.TabIndex = 1;
            // 
            // HelpTextBox
            // 
            this.HelpTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.HelpTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HelpTextBox.Location = new System.Drawing.Point(0, 0);
            this.HelpTextBox.Name = "HelpTextBox";
            this.HelpTextBox.ReadOnly = true;
            this.HelpTextBox.Size = new System.Drawing.Size(352, 308);
            this.HelpTextBox.TabIndex = 0;
            this.HelpTextBox.Text = "";
            // 
            // TopPanel
            // 
            this.TopPanel.Controls.Add(this.comboBox1);
            this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.TopPanel.Location = new System.Drawing.Point(0, 0);
            this.TopPanel.Name = "TopPanel";
            this.TopPanel.Size = new System.Drawing.Size(352, 21);
            this.TopPanel.TabIndex = 2;
            // 
            // valueToolTip1
            // 
            this.valueToolTip1.AutoSize = false;
            this.valueToolTip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.valueToolTip1.Name = "valueToolTip1";
            this.valueToolTip1.Padding = new System.Windows.Forms.Padding(0);
            this.valueToolTip1.Size = new System.Drawing.Size(100, 20);
            // 
            // HelpPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.TopPanel);
            this.Name = "HelpPanel";
            this.Size = new System.Drawing.Size(352, 594);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.TopPanel.ResumeLayout(false);
            this.ResumeLayout(false);

		}

        #endregion

        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private MyRichTextBox HelpTextBox;
        private System.Windows.Forms.Panel TopPanel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private ValueToolTip valueToolTip1;
        private FlatListBox flatListBox1;
    }
}
