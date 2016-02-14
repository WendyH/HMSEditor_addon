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
        private Button Btn = new Button();
        private ToolStripControlHost BtnHost;
        private string Expression = "";
        public bool IsShowing = false;

        public ValueToolTip() : base() {
            Btn.AutoSize = true;
            Btn.Size     = new Size(34, 25);
            Btn.Text     = "";
            Btn.Image    = global::HMSEditorNS.Properties.Resources.Find_5650;
            Btn.Click   += Btn_Click;
            ctl.BorderStyle = BorderStyle.None;
            ctl.BackColor   = Color.White;
            ctl.ForeColor   = Color.Black;
            ctl.Multiline   = true;
            ctl.ReadOnly    = true;
            ctl.Font        = new Font("Arial", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            ctl.HideSelection = false;
            ctl.WordWrap    = true;
            ctl.DetectUrls  = false;
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
            ctl.KeyDown += Ctl_KeyDown;

            BtnHost = new ToolStripControlHost(this.Btn);
            BtnHost.Margin   = Padding.Empty;
            BtnHost.Padding  = Padding.Empty;
            BtnHost.AutoSize = false;
            BtnHost.Visible  = false;
            this.Items.Add(BtnHost);
            //this.BackColor = Color.LightGray;
            Initialize();
            
            AutoClose = true;
        }

        private void Btn_Click(object sender, EventArgs e) {
            if (HMSEditor.ActiveEditor != null) {
                HMSEditor.ActiveEditor.ValueForm.Show(this, Expression, ctl.Text);
                Close();
            }
        }

        protected override void WndProc(ref Message m) {
            const int wmNcHitTest = 0x84;
            const int htLeft        = 10;
            const int htRight       = 11;
            const int htTop         = 12;
            const int htTopLeft     = 13;
            const int htTopRight    = 14;
            const int htBottom      = 15;
            const int htBottomLeft  = 16;
            const int htBottomRight = 17;

            if (m.Msg == wmNcHitTest) {
                int x = (int)(m.LParam.ToInt64() & 0xFFFF);
                int y = (int)((m.LParam.ToInt64() & 0xFFFF0000) >> 16);
                Point pt = PointToClient(new Point(x, y));
                Size clientSize = ClientSize;
                ///allow resize on the lower right corner
                if (pt.X >= clientSize.Width - 16 && pt.Y >= clientSize.Height - 16 && clientSize.Height >= 16) {
                    m.Result = (IntPtr)(IsMirrored ? htBottomLeft : htBottomRight);
                    return;
                }
                ///allow resize on the lower left corner
                if (pt.X <= 16 && pt.Y >= clientSize.Height - 16 && clientSize.Height >= 16) {
                    m.Result = (IntPtr)(IsMirrored ? htBottomRight : htBottomLeft);
                    return;
                }
                ///allow resize on the upper right corner
                if (pt.X <= 16 && pt.Y <= 16 && clientSize.Height >= 16) {
                    m.Result = (IntPtr)(IsMirrored ? htTopRight : htTopLeft);
                    return;
                }
                ///allow resize on the upper left corner
                if (pt.X >= clientSize.Width - 16 && pt.Y <= 16 && clientSize.Height >= 16) {
                    m.Result = (IntPtr)(IsMirrored ? htTopLeft : htTopRight);
                    return;
                }
                ///allow resize on the top border
                if (pt.Y <= 16 && clientSize.Height >= 16) {
                    m.Result = (IntPtr)(htTop);
                    return;
                }
                ///allow resize on the bottom border
                if (pt.Y >= clientSize.Height - 16 && clientSize.Height >= 16) {
                    m.Result = (IntPtr)(htBottom);
                    return;
                }
                ///allow resize on the left border
                if (pt.X <= 16 && clientSize.Height >= 16) {
                    m.Result = (IntPtr)(htLeft);
                    return;
                }
                ///allow resize on the right border
                if (pt.X >= clientSize.Width - 16 && clientSize.Height >= 16) {
                    m.Result = (IntPtr)(htRight);
                    return;
                }
            }
            base.WndProc(ref m);
        }
        //This gives us the ability to drag the borderless form to a new location
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        private void YOURCONTROL_MouseDown(object sender, MouseEventArgs e) {
            //ctrl-leftclick anywhere on the control to drag the form to a new location 
            if (e.Button == MouseButtons.Left && Control.ModifierKeys == Keys.Control) {
                NativeMethods.ReleaseCapture();
                NativeMethods.SendMessage(Handle, WM_NCLBUTTONDOWN, (IntPtr)HT_CAPTION, (IntPtr)0);
            }
        }

        protected override CreateParams CreateParams {
            get { var cp = base.CreateParams; cp.Style |= 0x040000; return cp; } // Turn on WS_BORDER + WS_THICKFRAME 
        }

        protected override void OnResize(EventArgs e) {
            //base.OnResize(e);
            int h = Size.Height - 18;
            if (BtnHost.Visible) {
                h -= BtnHost.Height;
            }
            ctl.Size = new Size(Size.Width - 18 , h);
        }

        private void Ctl_KeyDown(object sender, KeyEventArgs e) {
            OnKeyDown(e);
        }

        protected override void OnClosed(ToolStripDropDownClosedEventArgs e) {
            IsShowing = false;
            base.OnClosed(e);
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

        Keys[] SkipKeys = new Keys[] { Keys.PageDown, Keys.PageUp, Keys.Left, Keys.Right, Keys.Down, Keys.Up, Keys.Home, Keys.End };
        protected override void OnKeyDown(KeyEventArgs e) {
            if (e.Control && e.KeyCode==Keys.A) {
                e.Handled = true;
                ctl.SelectAll();
                return;
            }
            if (e.Control && e.KeyCode == Keys.C) {
                e.Handled = true;
                ctl.Copy();
                return;
            }
            if (Array.IndexOf(SkipKeys, e.KeyCode)>=0) {
                base.OnKeyDown(e); 
                return; 
            }

            if ((e.KeyValue >= (int)Keys.F1 && e.KeyValue <= (int)Keys.F12) && HMSEditor.ActiveEditor != null) {
                e.Handled = true;
                NativeMethods.SendNotifyKey(HMSEditor.ActiveEditor.Editor.Handle, (int)e.KeyCode);
                return;
            }
            base.OnKeyDown(e);
        }

        protected override void OnClosing(ToolStripDropDownClosingEventArgs e) {
            if (Focused) {
                e.Cancel = true;
                return;
            }
            IsShowing = false;
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

        public void ShowValue(Control control, string expr, string value, Point point) {
            Expression = expr;
            if (value.Length == 0) value = "  ";
            Size textSize = new Size(MaxSize.Width, MaxSize.Height);
            if (value.Length < 500)
                textSize = TextRenderer.MeasureText(value, ctl.Font, MaxSize, TextFormatFlags.WordBreak | TextFormatFlags.ExternalLeading);
            textSize.Height += 16;
            textSize.Width += 16;
            ctl.ScrollBars = RichTextBoxScrollBars.None;
            if (textSize.Height > MaxSize.Height) { textSize.Height = MaxSize.Height; ctl.ScrollBars = RichTextBoxScrollBars.Vertical; }
            if (textSize.Width > MaxSize.Width) { textSize.Width = MaxSize.Width; }
            if (textSize.Width > MaxSize.Width - 54) { textSize.Height += 14; }
            ctl.Size = textSize;
            ctl.Text = value;
            int n = 0;
            if (ctl.ScrollBars == RichTextBoxScrollBars.Vertical) n = 18;

            bool btnEnable  = (value.Length > 200);
            BtnHost.Visible = btnEnable;

            if (btnEnable) { n += BtnHost.Height; }

            Size = new Size(textSize.Width+18, textSize.Height +6+ n);
            Show(control, point);
            timer.Start();
            IsShowing = true;
            control.Focus();
        }

        private void Initialize() {
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
