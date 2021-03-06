﻿using System;
using System.Windows.Forms;
using System.Drawing;
using System.Security.Permissions;

namespace HMSEditorNS {
    /// <summary>
    /// Класс для подсказки вычисленного выражения переменной или свойства при наведении курсора мыши в режиме отладки скрипта
    /// </summary>
    public sealed class ValueToolTip : ToolStripDropDown {
        private ToolStripControlHost btnHost;
        private ToolStripControlHost ctlHost;
        private ContextMenuStrip menustrip = new ContextMenuStrip();
        private RichTextBox ctl = new RichTextBox();
        private Timer  timer    = new Timer();
        private Size   MouseDX  = new Size(20, 20);
        private Button btn      = new Button();
        private Size   MaxSize  = new Size(600, 400);
        public  string Expression     = "";
        public  string RealExpression = "";
        public  bool   IsShowing;
        public string Value { get { return ctl.Text; } set { ctl.Text = value; RecalcSize(); } }

        public ValueToolTip() {
            btn.AutoSize      = false;
            btn.Size          = new Size(34, 25);
            btn.Text          = "";
            btn.Image         = Properties.Resources.Find_5650;
            btn.Click        += Btn_Click;
            ctl.BorderStyle   = BorderStyle.None;
            ctl.BackColor     = Color.White;
            ctl.ForeColor     = Color.Black;
            ctl.Multiline     = true;
            ctl.ReadOnly      = true;
            ctl.Font          = new Font("Arial", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            ctl.HideSelection = false;
            ctl.WordWrap      = true;
            ctl.DetectUrls    = false;
            ctl.BackColor     = Color.FromArgb(255, 231, 232, 236);
            timer.Tick       += tick_CheckMouseNearly;
            timer.Interval    = 500;
            var item = menustrip.Items.Add("Копировать", Properties.Resources.Copy_6524);
            item.Click += Copy_Click;
            item = menustrip.Items.Add("Выделить всё");
            item.Click += SelectAll_Click;
            menustrip.Opening += Menustrip_Opening;
            menustrip.Closing += Menustrip_Closing;
            ctl.ContextMenuStrip = menustrip;
            ctl.KeyDown += Ctl_KeyDown;
            ctl.LostFocus += Ctl_LostFocus;

            btnHost = new ToolStripControlHost(btn) {
                Margin   = Padding.Empty,
                Padding  = Padding.Empty,
                AutoSize = false,
                Visible  = false
            };
            Items.Add(btnHost);

            ctlHost = new ToolStripControlHost(ctl) {
                Margin   = Padding.Empty,
                Padding  = Padding.Empty,
                AutoSize = false,
            };
            Items.Add(ctlHost);

            AutoSize  = false;
            BackColor = ctl.BackColor;
            Margin    = Padding.Empty;
            Padding   = new Padding(3, 3, 0, 3);
        }

        private void Ctl_LostFocus(object sender, EventArgs e) {
            Close();
        }

        private void Btn_Click(object sender, EventArgs e) {
            if (HMSEditor.ActiveEditor != null) {
                HMSEditor.ActiveEditor.ValueForm.Show(HMSEditor.ActiveEditor.TB, Expression, ctl.Text, RealExpression, HMSEditor.ActiveEditor.HintsInWindow);
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

        private void Ctl_KeyDown(object sender, KeyEventArgs e) {
            OnKeyDown(e);
        }

        protected override void OnClosed(ToolStripDropDownClosedEventArgs e) {
            IsShowing = false;
            base.OnClosed(e);
        }

        private void Menustrip_Opening(object sender, System.ComponentModel.CancelEventArgs e) {
            //AutoClose = false;
            menustrip.Items[0].Enabled = ctl.SelectionLength > 0;
        }

        private void Menustrip_Closing(object sender, ToolStripDropDownClosingEventArgs e) {
            //AutoClose = true;
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
            if (e.KeyCode == Keys.Escape) {
                e.Handled = true;
                Close();
                return;
            }
            //e.Handled = false;
            //NativeMethods.SendNotifyKey(HMSEditor.ActiveEditor.TB.Handle, (int)e.KeyCode);

            if (Array.IndexOf(SkipKeys, e.KeyCode) >= 0) {
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
            //AutoClose = true;
            IsShowing = false;
            timer.Stop();
            base.OnClosing(e);
        }

        private void tick_CheckMouseNearly(object obj, EventArgs e) {
            if (!CheckMouseNearly()) {
                Hide();
                IsShowing = false;
            }
        }

        public bool CheckMouseNearly() {
            int x = MousePosition.X - Left;
            int y = MousePosition.Y - Top;
            if ((x < -MouseDX.Width) || (x - Width > MouseDX.Width) || (y < -MouseDX.Height) || (y - Height > MouseDX.Height)) {
                return false;
            }
            return true;
        }

        protected override void OnResize(EventArgs e) {
            //base.OnResize(e);
            int h = SystemInformation.HorizontalResizeBorderThickness, w = SystemInformation.VerticalResizeBorderThickness;
            if (btnHost.Visible) {
                h += btnHost.Height;
            }
            ctl.Size = new Size(Size.Width - w, Size.Height - h);
        }

        public void RecalcSize() {
            SizeF textSize = new Size(MaxSize.Width, MaxSize.Height);
            int linesFitted = 0;
            var g = ctl.CreateGraphics();
            StringFormat stringFormat = StringFormat.GenericTypographic;
            textSize = g.MeasureString(ctl.Text, ctl.Font, textSize, stringFormat, out int charsFitted, out linesFitted);
            textSize.Height += linesFitted + 1;
            //textSize.Height += 6;
            textSize.Width  += 8;
            ctl.ScrollBars = textSize.Height+2 >= MaxSize.Height ? RichTextBoxScrollBars.Vertical : RichTextBoxScrollBars.None;

            int h = SystemInformation.HorizontalResizeBorderThickness, w = SystemInformation.VerticalResizeBorderThickness;
            if (ctl.Text.Length > 200) {
                btnHost.Visible = true;
                h += btnHost.Height;
            } else {
                btnHost.Visible = false;
            }

            Size = new Size((int)Math.Round(textSize.Width) + w, (int)Math.Round(textSize.Height) + h);
        }

        public void ShowValue(Control control, string expr, string value, Point point, string realExpression, bool inWindow) {
            if (inWindow && control!=null) {
                NativeMethods.SetParent(Handle, control.Handle);
                if (control.Height < MaxSize.Height) MaxSize.Height = control.Height - btnHost.Height;
                if (control.Width  < MaxSize.Width ) MaxSize.Width  = control.Width;
            }
            if (value.Length == 0) value = "  ";
            ctl.Text       = value;
            Expression     = expr;
            RealExpression = realExpression;
            AutoClose      = false;
            RecalcSize();
            if (inWindow && control != null) {
                if (point.Y + Height > control.Height - control.PointToScreen(new Point(0, 0)).Y) {
                    point.Y -= Height + control.Font.Height;
                }
            }
            Show(control, point);
            OnResize(EventArgs.Empty);
            IsShowing = true;
            timer.Start();
            control?.Focus();
        }
    }


}
