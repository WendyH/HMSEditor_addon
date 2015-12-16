using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Darwen.Windows.Forms.General
{
    public sealed class PaddingDrawHandler
    {        
        public PaddingDrawHandler(Control control)
        {
            control.Paint += new PaintEventHandler(control_Paint);
        }

        private void control_Paint(object sender, PaintEventArgs e)
        {
            DrawSplitHighlights(sender as Control, e.Graphics);
        }

        static private void DrawSplitHighlights(Control control, Graphics graphics)
        {
            Rectangle clientRectangle = control.ClientRectangle;

            using (Pen penLight = new Pen(Color.FromKnownColor(KnownColor.ButtonHighlight)))
            {
                using (Pen penDark = new Pen(Color.FromKnownColor(KnownColor.ButtonShadow)))
                {
                    if (control.Padding.Left > 0)
                    {
                        graphics.DrawLine(penLight, clientRectangle.Left, clientRectangle.Top,
                            clientRectangle.Left, clientRectangle.Bottom);

                        graphics.DrawLine(penDark, clientRectangle.Left + control.Padding.Left - 1, clientRectangle.Top,
                            clientRectangle.Left + control.Padding.Left - 1, clientRectangle.Bottom);
                    }

                    if (control.Padding.Right > 0)
                    {
                        graphics.DrawLine(penDark, clientRectangle.Right - control.Padding.Right, clientRectangle.Top,
                            clientRectangle.Right - control.Padding.Right, clientRectangle.Bottom);

                        graphics.DrawLine(penLight, clientRectangle.Right - 1, clientRectangle.Top,
                            clientRectangle.Right - 1, clientRectangle.Bottom);
                    }

                    if (control.Padding.Top > 0)
                    {
                        graphics.DrawLine(penLight, clientRectangle.Left, clientRectangle.Top,
                            clientRectangle.Right, clientRectangle.Top);

                        graphics.DrawLine(penDark, clientRectangle.Left, clientRectangle.Top + (control.Padding.Top - 1),
                            clientRectangle.Right, clientRectangle.Top + (control.Padding.Top - 1));
                    }

                    if (control.Padding.Bottom > 0)
                    {
                        graphics.DrawLine(penDark, clientRectangle.Left, clientRectangle.Bottom - control.Padding.Bottom,
                            clientRectangle.Right, clientRectangle.Bottom - control.Padding.Bottom);

                        graphics.DrawLine(penLight, clientRectangle.Left, clientRectangle.Bottom - 1,
                            clientRectangle.Right, clientRectangle.Bottom - 1);
                    }
                }
            }
        }
    }
}
