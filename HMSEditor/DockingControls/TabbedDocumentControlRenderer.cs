using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;
using Darwen.Windows.Forms.General;

namespace Darwen.Windows.Forms.Controls.TabbedDocuments
{
    internal class TabbedDocumentControlRenderer : ToolStripRendererDecorator
    {
        private TabbedDocumentControl _control;
        private const int ButtonPadding = -2;

        public TabbedDocumentControlRenderer(TabbedDocumentControl control, ToolStripRenderer renderer)
            : base(renderer)
        {
            _control = control;
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {            
        }

        protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
        {
            ToolStripButton button = e.Item as ToolStripButton;

            if (button == null || !_control.IsDocumentButton(button))
            {
                base.OnRenderButtonBackground(e);
            }   
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(e.ToolStrip.ClientRectangle,
                        Color.FromKnownColor(KnownColor.ButtonHighlight),
                        Color.FromKnownColor(KnownColor.InactiveCaption), LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, e.ToolStrip.ClientRectangle);
            }

            foreach (ToolStripItem item in e.ToolStrip.Items)
            {
                ToolStripButton button = item as ToolStripButton;

                if (button != null && _control.IsDocumentButton(button))
                {
                    if (button != _control.SelectedButton)
                    {
                        DrawButton(button, e, false);
                    }
                }
            }

            if (_control.SelectedButton != null)
            {
                DrawButton(_control.SelectedButton, e, true);
            }
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            ToolStripButton button = e.Item as ToolStripButton;

            if (button == null || !_control.IsDocumentButton(button))
            {
                base.OnRenderItemText(e);
            }
            else
            {
                Point mousePoint = e.ToolStrip.PointToClient(Control.MousePosition);

                Color textColour = (button != _control.SelectedButton && button.Bounds.Contains(mousePoint)) ? Color.White : Color.Black;

                using (SolidBrush textBrush = new SolidBrush(textColour))
                {
                    e.Graphics.DrawString(e.Text, e.TextFont, textBrush, e.TextRectangle);
                }
            }
        }

        private void DrawButton(ToolStripButton button, ToolStripRenderEventArgs e, bool fill)
        {
            Rectangle drawRectangle = button.Bounds;
            drawRectangle.Height = e.ToolStrip.ClientRectangle.Bottom - drawRectangle.Y;
            drawRectangle.X -= ButtonPadding;
            drawRectangle.Width += 2 * ButtonPadding;

            if (fill)
            {
                using (SolidBrush brush = new SolidBrush(Color.FromKnownColor(KnownColor.ButtonHighlight)))
                {
                    e.Graphics.FillRectangle(brush, drawRectangle);
                }
            }

            using (Pen pen = new Pen(Color.FromKnownColor(KnownColor.ControlDark)))
            {
                e.Graphics.DrawRectangle(pen, drawRectangle);                
            }            
        }
    }
}
