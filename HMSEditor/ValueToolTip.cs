using System;
using System.Windows.Forms;
using System.Drawing;

namespace HMSEditorNS {
    /// <summary>
    /// Класс для подсказки вычисленного выражения переменной или свойства при наведении курсора мыши в режиме отладки скрипта
    /// </summary>
    public class ValueToolTip: ToolStripDropDown {
        private ContextMenuStrip menustrip = new ContextMenuStrip();
        private RichTextBox ctl     = new RichTextBox();
        private Size    MaxSize = new Size(500, 400);
        private Timer   timer   = new Timer();
        private Size    MouseDX = new Size(30, 30);

        public ValueToolTip() : base() {
            ctl.BorderStyle = BorderStyle.None;
            ctl.BackColor   = Color.White;
            ctl.ForeColor   = Color.Black;
            ctl.Multiline   = true;
            ctl.ReadOnly    = true;
            ctl.Font        = new Font("Arial", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            ctl.EnableAutoDragDrop = false;
            ctl.DetectUrls = false;
            ctl.HideSelection = false;
            MaximumSize     = new Size(MaxSize.Width+20, MaxSize.Height+20);
            timer.Tick     += new EventHandler(tick_CheckMouseNearly);
            timer.Interval  = 500;
            ToolStripItem item;
            item = menustrip.Items.Add("Копировать", Properties.Resources.Copy_6524);
            item.Click += Copy_Click;
            item = menustrip.Items.Add("Выделить всё");
            item.Click += SelectAll_Click;
            menustrip.Opening += Menustrip_Opening;
            menustrip.Closing += Menustrip_Closing;
            ctl.ContextMenuStrip = menustrip;
            Initialize();
        }

        protected override void OnClosed(ToolStripDropDownClosedEventArgs e) {
            base.OnClosed(e);
            if (HMSEditor.ActiveEditor != null) HMSEditor.ActiveEditor.Focus();
        }

        private void Menustrip_Opening(object sender, System.ComponentModel.CancelEventArgs e) {
            AutoClose = false;
            menustrip.Items[0].Enabled = ctl.SelectionLength > 0;
        }

        private void Menustrip_Closing(object sender, ToolStripDropDownClosingEventArgs e) {
            AutoClose = true;
        }

        private void SelectAll_Click(object sender, EventArgs e) {
            ctl.SelectAll();
            ctl.Focus();
        }

        private void Copy_Click(object sender, EventArgs e) {
            ctl.Copy();
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            //Close(); 
            base.OnKeyDown(e);
        }

        protected override void OnClosing(ToolStripDropDownClosingEventArgs e) {
            timer.Stop();
            base.OnClosing(e);
        }

        private void tick_CheckMouseNearly(object obj, EventArgs e) {
            int x = MousePosition.X - Left;
            int y = MousePosition.Y - Top;
            if ((x < -MouseDX.Width ) || (x - Width  > MouseDX.Width ) || (y < -MouseDX.Height) || (y - Height > MouseDX.Height)) {
                Hide();
            }
        }
         
        public void ShowValue(Control control, string value, Point point) {
            if (value.Length == 0) value = "  ";
            Size textSize = new Size(MaxSize.Width, MaxSize.Height);
            if (value.Length < 500)
                textSize = TextRenderer.MeasureText(value, ctl.Font, MaxSize);
            textSize.Height += 4;  
            textSize.Width  += 4;
            ctl.ScrollBars = RichTextBoxScrollBars.None;
            if (textSize.Height > MaxSize.Height) { textSize.Height = MaxSize.Height; ctl.ScrollBars = RichTextBoxScrollBars.Vertical; }
            if (textSize.Width  > MaxSize.Width ) { textSize.Width  = MaxSize.Width; }
            if (textSize.Width > MaxSize.Width - 54) { textSize.Height += 14; }
            ctl.Size = textSize;
            ctl.Text = value;
            int n = 0; 
            if (ctl.ScrollBars == RichTextBoxScrollBars.Vertical) n = 12;
            Size = new Size(textSize.Width+6, textSize.Height +6+ n);
            Show(control, point);
            timer.Start();
            ctl.Focus();
        } 

        public void Initialize() {
            this.AutoSize = false;
            ToolStripControlHost host = new ToolStripControlHost(this.ctl);
            this.Margin   = Padding.Empty;
            this.Padding  = new Padding(3, 3, 0, 3);
            host.Margin   = Padding.Empty;
            host.Padding  = Padding.Empty;
            host.AutoSize = false;
            host.Size     = ctl.Size;
            this.Size     = ctl.Size;
            this.Items.Add(host);
        }
    }
}
