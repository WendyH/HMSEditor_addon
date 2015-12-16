namespace Darwen.Windows.Forms.Controls.Docking
{
    partial class DockingManagerControl
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
            this._bottomMenuStrip = new Darwen.Windows.Forms.Controls.MenuStrip();
            this._leftMenuStrip = new Darwen.Windows.Forms.Controls.MenuStrip();
            this._rightMenuStrip = new Darwen.Windows.Forms.Controls.MenuStrip();
            this._topMenuStrip = new Darwen.Windows.Forms.Controls.MenuStrip();
            this.SuspendLayout();
            // 
            // _bottomMenuStrip
            // 
            this._bottomMenuStrip.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._bottomMenuStrip.Location = new System.Drawing.Point(0, 512);
            this._bottomMenuStrip.Name = "_bottomMenuStrip";
            this._bottomMenuStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this._bottomMenuStrip.Size = new System.Drawing.Size(811, 24);
            this._bottomMenuStrip.TabIndex = 9;
            this._bottomMenuStrip.Text = "menuStrip1";
            this._bottomMenuStrip.Visible = false;
            // 
            // _leftMenuStrip
            // 
            this._leftMenuStrip.Dock = System.Windows.Forms.DockStyle.Left;
            this._leftMenuStrip.Location = new System.Drawing.Point(0, 0);
            this._leftMenuStrip.Name = "_leftMenuStrip";
            this._leftMenuStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this._leftMenuStrip.Size = new System.Drawing.Size(98, 536);
            this._leftMenuStrip.TabIndex = 1;
            this._leftMenuStrip.TextDirection = System.Windows.Forms.ToolStripTextDirection.Vertical90;
            this._leftMenuStrip.Visible = false;
            // 
            // _rightMenuStrip
            // 
            this._rightMenuStrip.Dock = System.Windows.Forms.DockStyle.Right;
            this._rightMenuStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
            this._rightMenuStrip.Location = new System.Drawing.Point(713, 0);
            this._rightMenuStrip.Name = "_rightMenuStrip";
            this._rightMenuStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this._rightMenuStrip.Size = new System.Drawing.Size(98, 536);
            this._rightMenuStrip.TabIndex = 5;
            this._rightMenuStrip.Text = "_rightMenuStrip";
            this._rightMenuStrip.TextDirection = System.Windows.Forms.ToolStripTextDirection.Vertical90;
            this._rightMenuStrip.Visible = false;
            // 
            // _topMenuStrip
            // 
            this._topMenuStrip.Location = new System.Drawing.Point(0, 0);
            this._topMenuStrip.Name = "_topMenuStrip";
            this._topMenuStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this._topMenuStrip.Size = new System.Drawing.Size(811, 24);
            this._topMenuStrip.TabIndex = 6;
            this._topMenuStrip.Text = "menuStrip1";
            this._topMenuStrip.Visible = false;
            // 
            // DockingManagerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._leftMenuStrip);
            this.Controls.Add(this._rightMenuStrip);
            this.Controls.Add(this._bottomMenuStrip);
            this.Controls.Add(this._topMenuStrip);
            this.Name = "DockingManagerControl";
            this.Size = new System.Drawing.Size(811, 536);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Darwen.Windows.Forms.Controls.MenuStrip _leftMenuStrip;
        private Darwen.Windows.Forms.Controls.MenuStrip _rightMenuStrip;
        private Darwen.Windows.Forms.Controls.MenuStrip _topMenuStrip;
        private Darwen.Windows.Forms.Controls.MenuStrip _bottomMenuStrip;
    }
}
