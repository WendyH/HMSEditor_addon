namespace Darwen.Windows.Forms.Controls.Docking
{
    partial class DockingControl
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
            this.components = new System.ComponentModel.Container();
            this.BottomToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.TopToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.RightToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.LeftToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.ContentPanel = new System.Windows.Forms.ToolStripContentPanel();
            this._transparentUserControl = new Darwen.Windows.Forms.General.GradientFillUserControl();
            this._toolStripCaption = new Darwen.Windows.Forms.Controls.Docking.CaptionToolStrip(this.components);
            this._toolStripLabel = new System.Windows.Forms.ToolStripLabel();
            this._toolStripButtonClose = new System.Windows.Forms.ToolStripButton();
            this._autoHidetoolStripButton = new System.Windows.Forms.ToolStripButton();
            this._tabsToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._toolStripCaption.SuspendLayout();
            this.SuspendLayout();
            // 
            // BottomToolStripPanel
            // 
            this.BottomToolStripPanel.Enabled = false;
            this.BottomToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.BottomToolStripPanel.Name = "BottomToolStripPanel";
            this.BottomToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.BottomToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.BottomToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // TopToolStripPanel
            // 
            this.TopToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.TopToolStripPanel.Name = "TopToolStripPanel";
            this.TopToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.TopToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.TopToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // RightToolStripPanel
            // 
            this.RightToolStripPanel.Enabled = false;
            this.RightToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.RightToolStripPanel.Name = "RightToolStripPanel";
            this.RightToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.RightToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.RightToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // LeftToolStripPanel
            // 
            this.LeftToolStripPanel.Enabled = false;
            this.LeftToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.LeftToolStripPanel.Name = "LeftToolStripPanel";
            this.LeftToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.LeftToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.LeftToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // ContentPanel
            // 
            this.ContentPanel.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.ContentPanel.Size = new System.Drawing.Size(348, 334);
            // 
            // _transparentUserControl
            // 
            this._transparentUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._transparentUserControl.Location = new System.Drawing.Point(0, 25);
            this._transparentUserControl.Name = "_transparentUserControl";
            this._transparentUserControl.Padding = new System.Windows.Forms.Padding(5);
            this._transparentUserControl.Size = new System.Drawing.Size(348, 309);
            this._transparentUserControl.TabIndex = 2;
            // 
            // _toolStripCaption
            // 
            this._toolStripCaption.AutoSize = false;
            this._toolStripCaption.CanOverflow = false;
            this._toolStripCaption.DraggingEnabled = true;
            this._toolStripCaption.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._toolStripCaption.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._toolStripLabel,
            this._toolStripButtonClose,
            this._autoHidetoolStripButton,
            this._tabsToolStripButton});
            this._toolStripCaption.Location = new System.Drawing.Point(0, 0);
            this._toolStripCaption.Name = "_toolStripCaption";
            this._toolStripCaption.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this._toolStripCaption.Size = new System.Drawing.Size(348, 25);
            this._toolStripCaption.Stretch = true;
            this._toolStripCaption.TabIndex = 1;
            this._toolStripCaption.CaptionDrag += new System.Windows.Forms.MouseEventHandler(this._toolStripCaption_CaptionDrag);
            this._toolStripCaption.StartCaptionDrag += new System.Windows.Forms.MouseEventHandler(this._toolStripCaption_StartCaptionDrag);
            this._toolStripCaption.CancelCaptionDrag += new System.EventHandler(this._toolStripCaption_CanelCaptionDrag);
            this._toolStripCaption.EndCaptionDrag += new System.Windows.Forms.MouseEventHandler(this._toolStripCaption_EndCaptionDrag);
            // 
            // _toolStripLabel
            // 
            this._toolStripLabel.Enabled = false;
            this._toolStripLabel.Name = "_toolStripLabel";
            this._toolStripLabel.Size = new System.Drawing.Size(54, 22);
            this._toolStripLabel.Text = "Window 1";
            // 
            // _toolStripButtonClose
            // 
            this._toolStripButtonClose.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this._toolStripButtonClose.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._toolStripButtonClose.Font = new System.Drawing.Font("Marlett", 7.25F);
            this._toolStripButtonClose.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolStripButtonClose.Name = "_toolStripButtonClose";
            this._toolStripButtonClose.Size = new System.Drawing.Size(23, 22);
            this._toolStripButtonClose.Text = "r";
            this._toolStripButtonClose.ToolTipText = "Close";
            this._toolStripButtonClose.Click += new System.EventHandler(this._toolStripButtonClose_Click);
            // 
            // _autoHidetoolStripButton
            // 
            this._autoHidetoolStripButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this._autoHidetoolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._autoHidetoolStripButton.Image = global::HMSEditorNS.Properties.Resources.AutoHideOff;
            this._autoHidetoolStripButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this._autoHidetoolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._autoHidetoolStripButton.Name = "_autoHidetoolStripButton";
            this._autoHidetoolStripButton.Size = new System.Drawing.Size(23, 22);
            this._autoHidetoolStripButton.ToolTipText = "Auto Hide";
            this._autoHidetoolStripButton.Click += new System.EventHandler(this._autoHidetoolStripButton_Click);
            // 
            // _tabsToolStripButton
            // 
            this._tabsToolStripButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this._tabsToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._tabsToolStripButton.Image = global::HMSEditorNS.Properties.Resources.ToggleTabs;
            this._tabsToolStripButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this._tabsToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._tabsToolStripButton.Name = "_tabsToolStripButton";
            this._tabsToolStripButton.Size = new System.Drawing.Size(23, 22);
            this._tabsToolStripButton.Text = "Toggle tabs";
            this._tabsToolStripButton.ToolTipText = "Toggle Tabs";
            this._tabsToolStripButton.Click += new System.EventHandler(this._tabsToolStripButton_Click);
            // 
            // DockingControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this._transparentUserControl);
            this.Controls.Add(this._toolStripCaption);
            this.MinimumSize = new System.Drawing.Size(25, 40);
            this.Name = "DockingControl";
            this.Size = new System.Drawing.Size(348, 334);
            this._toolStripCaption.ResumeLayout(false);
            this._toolStripCaption.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripPanel BottomToolStripPanel;
        private System.Windows.Forms.ToolStripPanel TopToolStripPanel;
        private System.Windows.Forms.ToolStripPanel RightToolStripPanel;
        private System.Windows.Forms.ToolStripPanel LeftToolStripPanel;
        private System.Windows.Forms.ToolStripContentPanel ContentPanel;
        private Darwen.Windows.Forms.General.GradientFillUserControl _transparentUserControl;
        private CaptionToolStrip _toolStripCaption;
        private System.Windows.Forms.ToolStripLabel _toolStripLabel;
        private System.Windows.Forms.ToolStripButton _toolStripButtonClose;
        private System.Windows.Forms.ToolStripButton _autoHidetoolStripButton;
        private System.Windows.Forms.ToolStripButton _tabsToolStripButton;


    }
}
