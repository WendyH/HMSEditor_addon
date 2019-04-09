﻿using System;
using System.Windows.Forms;
using System.Drawing;
using System.Security.Permissions;

namespace HMSEditorNS {
    /// <summary>
    /// Класс для подсказки вычисленного выражения переменной или свойства при наведении курсора мыши в режиме отладки скрипта
    /// </summary>
    public sealed class ValueToolTip: ToolStripDropDown {
        private ToolStripControlHost BtnHost;
        private ContextMenuStrip     menustrip = new ContextMenuStrip();
        private RichTextBox ctl = new RichTextBox();
        private Timer  timer    = new Timer();
        private Size   MouseDX  = new Size(20, 20);
        private Button Btn      = new Button();
        private Size   MaxSize  = new Size(500, 400);
        public string  Expression     = "";
        public string  RealExpression = "";
        public bool    IsShowing;
        public string Value { get { return ctl.Text; } set { ctl.Text = value; } }

        protected override CreateParams CreateParams {
            get {
                CreateParams baseParams = base.CreateParams;

                const int WS_EX_NOACTIVATE = 0x08000000;
                const int WS_EX_TOOLWINDOW = 0x00000080;
                baseParams.ExStyle |= (int)(WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW);

                return baseParams;
            }
        }

        public ValueToolTip() {
            Btn.AutoSize = false;
            Btn.Size     = new Size(34, 25);
            Btn.Text     = "";
            Btn.Image    = Properties.Resources.Find_5650;
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
            ctl.BackColor   = Color.FromArgb(255, 231, 232, 236);
            timer.Tick     += tick_CheckMouseNearly;
            timer.Interval  = 500;
            var item = menustrip.Items.Add("Копировать", Properties.Resources.Copy_6524);
            item.Click += Copy_Click;
            item = menustrip.Items.Add("Выделить всё");
            item.Click += SelectAll_Click;
            menustrip.Opening += Menustrip_Opening;
            menustrip.Closing += Menustrip_Closing;
            ctl.ContextMenuStrip = menustrip;
            ctl.KeyDown += Ctl_KeyDown;

            BtnHost = new ToolStripControlHost(Btn) {
                Margin   = Padding.Empty,
                Padding  = Padding.Empty,
                AutoSize = false,
                Visible  = false
            };
            Items.Add(BtnHost);
            //this.BackColor = Color.LightGray;
            Initialize();
            BackColor = ctl.BackColor;
            AutoClose = true;
        }

        private void Btn_Click(object sender, EventArgs e) {
            if (HMSEditor.ActiveEditor != null) {
                HMSEditor.ActiveEditor.ValueForm.Show(HMSEditor.ActiveEditor.TB, Expression, ctl.Text, RealExpression);
                Close();
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
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
                //allow resize on the lower right corner
                if (pt.X >= clientSize.Width - 16 && pt.Y >= clientSize.Height - 16 && clientSize.Height >= 16) {
                    m.Result = (IntPtr)(IsMirrored ? htBottomLeft : htBottomRight);
                    return;
                }
                //allow resize on the lower left corner
                if (pt.X <= 16 && pt.Y >= clientSize.Height - 16 && clientSize.Height >= 16) {
                    m.Result = (IntPtr)(IsMirrored ? htBottomRight : htBottomLeft);
                    return;
                }
                //allow resize on the upper right corner
                if (pt.X <= 16 && pt.Y <= 16 && clientSize.Height >= 16) {
                    m.Result = (IntPtr)(IsMirrored ? htTopRight : htTopLeft);
                    return;
                }
                //allow resize on the upper left corner
                if (pt.X >= clientSize.Width - 16 && pt.Y <= 16 && clientSize.Height >= 16) {
                    m.Result = (IntPtr)(IsMirrored ? htTopLeft : htTopRight);
                    return;
                }
                //allow resize on the top border
                if (pt.Y <= 16 && clientSize.Height >= 16) {
                    m.Result = (IntPtr)(htTop);
                    return;
                }
                //allow resize on the bottom border
                if (pt.Y >= clientSize.Height - 16 && clientSize.Height >= 16) {
                    m.Result = (IntPtr)(htBottom);
                    return;
                }
                //allow resize on the left border
                if (pt.X <= 16 && clientSize.Height >= 16) {
                    m.Result = (IntPtr)(htLeft);
                    return;
                }
                //allow resize on the right border
                if (pt.X >= clientSize.Width - 16 && clientSize.Height >= 16) {
                    m.Result = (IntPtr)(htRight);
                    return;
                }
            }
            base.WndProc(ref m);
        }

        protected override void OnMouseDown(MouseEventArgs mea) {
            base.OnMouseDown(mea);
            //ctrl-leftclick anywhere on the control to drag the form to a new location 
            if (mea.Button == MouseButtons.Left && ModifierKeys == Keys.Control) {
                NativeMethods.ReleaseCapture();
                NativeMethods.SendMessage(Handle, NativeMethods.WM_NCLBUTTONDOWN, (IntPtr)NativeMethods.HT_CAPTION, (IntPtr)0);
            }
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            ControlPaint.DrawBorder(e.Graphics, ClientRectangle, HMS.BordersColor, ButtonBorderStyle.Solid);
        }

        protected override void OnResize(EventArgs e) {
            //base.OnResize(e);
            int h = SystemInformation.HorizontalResizeBorderThickness, w = SystemInformation.VerticalResizeBorderThickness;
            if (BtnHost.Visible) {
                h += BtnHost.Height + 0; 
            }
            ctl.Size = new Size(Size.Width - w , Size.Height - h);
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
                NativeMethods.SendNotifyKey(HMSEditor.ActiveEditor.TB.Handle, (int)e.KeyCode);
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
                IsShowing = false;
            }
        }

        public void ShowValue(Control control, string expr, string value, Point point, string realExpression) {
            if (value.Length == 0) value = "  ";
            ctl.Text       = value;
            Expression     = expr;
            RealExpression = realExpression;
            SizeF textSize = new Size(MaxSize.Width, MaxSize.Height);
            if (value.Length < 500) {
                ctl.
                textSize = HMSEditor.ActiveEditor.GetSizeText(ctl.Text, ctl.Font, new Size(MaxSize.Width, MaxSize.Height));
                //textSize = ctl.GetPreferredSize(new Size(MaxSize.Width, MaxSize.Height));
                /*
                var g = ctl.CreateGraphics();
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                StringFormat stringFormat = StringFormat.GenericTypographic;
                stringFormat.Trimming = StringTrimming.Word;
                stringFormat.LineAlignment = StringAlignment.Near;
                textSize = g.MeasureString(value, ctl.Font, textSize, stringFormat, out int chars_fitted, out int lines_fitted);
                string linef = value.Substring(0, chars_fitted);
                */
            }
            textSize.Height += 10;
            textSize.Width  += 10;
            ctl.ScrollBars = RichTextBoxScrollBars.None;
            if (textSize.Height > MaxSize.Height) { ctl.ScrollBars = RichTextBoxScrollBars.Vertical; }
            /*
            if (value.Length > 200) {
                BtnHost.Visible = true;
                textSize.Height += BtnHost.Height;
            }
            */
            Size = new Size((int)textSize.Width, (int)textSize.Height);
            Show(control, point);
            OnResize(EventArgs.Empty);
            timer.Start();
            IsShowing = true;
            control.Focus();
        }

        private void Initialize() {
            AutoSize = false;
            ToolStripControlHost host = new ToolStripControlHost(ctl);
            Margin = Padding.Empty;
            Padding = new Padding(3, 3, 0, 3);
            host.Margin   = Padding.Empty;
            host.Padding  = Padding.Empty;
            host.AutoSize = false;
            host.Size     = ctl.Size;
            Size = ctl.Size;
            Items.Add(host);
        }
    }


}
