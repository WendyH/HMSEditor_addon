namespace Darwen.Windows.Forms.Controls.TabbedDocuments
{
    partial class TabbedDocumentControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._menuStrip = new System.Windows.Forms.MenuStrip();
            this._closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._activeFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._hiddenControlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._nextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._previousToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._menuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // _menuStrip
            // 
            this._menuStrip.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this._menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._closeToolStripMenuItem,
            this._activeFilesToolStripMenuItem,
            this._hiddenControlToolStripMenuItem});
            this._menuStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this._menuStrip.Location = new System.Drawing.Point(0, 0);
            this._menuStrip.Name = "_menuStrip";
            this._menuStrip.ShowItemToolTips = true;
            this._menuStrip.Size = new System.Drawing.Size(627, 24);
            this._menuStrip.TabIndex = 0;
            this._menuStrip.Text = "menuStrip1";
            this._menuStrip.Visible = false;
            this._menuStrip.Paint += new System.Windows.Forms.PaintEventHandler(this._menuStrip_Paint);
            // 
            // _closeToolStripMenuItem
            // 
            this._closeToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this._closeToolStripMenuItem.Font = new System.Drawing.Font("Marlett", 7.25F);
            this._closeToolStripMenuItem.Name = "_closeToolStripMenuItem";
            this._closeToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0);
            this._closeToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F4)));
            this._closeToolStripMenuItem.ShowShortcutKeys = false;
            this._closeToolStripMenuItem.Size = new System.Drawing.Size(19, 20);
            this._closeToolStripMenuItem.Text = "r";
            this._closeToolStripMenuItem.ToolTipText = "Close";
            this._closeToolStripMenuItem.Click += new System.EventHandler(this._closeToolStripMenuItem_Click);
            // 
            // _activeFilesToolStripMenuItem
            // 
            this._activeFilesToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this._activeFilesToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._activeFilesToolStripMenuItem.Font = new System.Drawing.Font("Marlett", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this._activeFilesToolStripMenuItem.Name = "_activeFilesToolStripMenuItem";
            this._activeFilesToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0);
            this._activeFilesToolStripMenuItem.Size = new System.Drawing.Size(21, 20);
            this._activeFilesToolStripMenuItem.Text = "6";
            this._activeFilesToolStripMenuItem.ToolTipText = "Active Windows";
            this._activeFilesToolStripMenuItem.Click += new System.EventHandler(this._activeFilesToolStripMenuItem_Click);
            // 
            // _hiddenControlToolStripMenuItem
            // 
            this._hiddenControlToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._nextToolStripMenuItem,
            this._previousToolStripMenuItem});
            this._hiddenControlToolStripMenuItem.Name = "_hiddenControlToolStripMenuItem";
            this._hiddenControlToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Tab)));
            this._hiddenControlToolStripMenuItem.Size = new System.Drawing.Size(58, 20);
            this._hiddenControlToolStripMenuItem.Text = "Hidden";
            this._hiddenControlToolStripMenuItem.Visible = false;
            // 
            // _nextToolStripMenuItem
            // 
            this._nextToolStripMenuItem.Name = "_nextToolStripMenuItem";
            this._nextToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Tab)));
            this._nextToolStripMenuItem.ShowShortcutKeys = false;
            this._nextToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this._nextToolStripMenuItem.Text = "Next";
            this._nextToolStripMenuItem.Click += new System.EventHandler(this._nextToolStripMenuItem_Click);
            // 
            // _previousToolStripMenuItem
            // 
            this._previousToolStripMenuItem.Name = "_previousToolStripMenuItem";
            this._previousToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift)
                        | System.Windows.Forms.Keys.Tab)));
            this._previousToolStripMenuItem.ShowShortcutKeys = false;
            this._previousToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this._previousToolStripMenuItem.Text = "Previous";
            this._previousToolStripMenuItem.Click += new System.EventHandler(this._previousToolStripMenuItem_Click);
            // 
            // TabbedDocumentControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.Controls.Add(this._menuStrip);
            this.Name = "TabbedDocumentControl";
            this.Size = new System.Drawing.Size(627, 150);
            this._menuStrip.ResumeLayout(false);
            this._menuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip _menuStrip;
        private System.Windows.Forms.ToolStripMenuItem _closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _activeFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _hiddenControlToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _nextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _previousToolStripMenuItem;
    }
}
