namespace Darwen.Windows.Forms.Controls.Docking
{
    partial class AutoResizeControl
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
            if (disposing && (components != null)) {
                components.Dispose();
                if (_dragHandler!=null) {
                    _dragHandler.Dispose();
                    _dragHandler = null;
                }
                if (_setDragCursor != null) {
                    _setDragCursor.Dispose();
                    _setDragCursor = null;
                }
                if (_animator != null) {
                    _animator.Dispose();
                    _animator = null;
                }
                if (_autoHideHandler != null) {
                    _autoHideHandler.Dispose();
                    _autoHideHandler = null;
                }
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
            components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        }

        #endregion
    }
}
