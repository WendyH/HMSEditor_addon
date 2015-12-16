using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Darwen.Windows.Forms.General
{
    public sealed class ControlHelpers
    {
        private ControlHelpers()
        {
        }

        static public void SetClientSize(Control control, Size size)
        {
            Rectangle bounds = control.Bounds;
            Rectangle clientRectangle = control.ClientRectangle;

            Rectangle newBounds = new Rectangle(bounds.X, bounds.Y,
                size.Width + (bounds.Width - clientRectangle.Width),
                size.Height + (bounds.Height - clientRectangle.Height));

            control.Bounds = newBounds;
        }

        static public Control GetFocusedChildControl(Control control)
        {
            if (control.Focused)
            {
                return control;
            }
            else
            {
                foreach (Control child in control.Controls)
                {
                    Control focused = GetFocusedChildControl(child);

                    if (focused != null)
                    {
                        return focused;
                    }
                }

                return null;
            }
        }

        static public TYPE FindParentControl<TYPE>(Control control) where TYPE : Control
        {
            while (control != null)
            {
                TYPE test = control as TYPE;

                if (test != null)
                {
                    return test;
                }

                control = control.Parent;
            }

            return null;
        }
    }
}
