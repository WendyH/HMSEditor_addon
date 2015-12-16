using System;
using System.Collections.Generic;
using System.Text;

namespace Darwen.Windows.Forms.Controls.TabbedDocuments
{
    internal static class TabbedDocumentControlHelper
    {
        static public void SelectNextDocument(TabbedDocumentControl documentControl)
        {
            if (documentControl.SelectedControl == null)
            {
                return;
            }
            else
            {
                int index = documentControl.Items.IndexOf(documentControl.SelectedControl);
                index = (index + 1 == documentControl.Items.Count) ? 0 : index + 1;

                documentControl.SelectedControl = documentControl.Items[index];
            }
        }

        static public void SelectPreviousDocument(TabbedDocumentControl documentControl)
        {
            if (documentControl.SelectedControl == null)
            {
                return;
            }
            else
            {
                int index = documentControl.Items.IndexOf(documentControl.SelectedControl);
                index = (index == 0) ? documentControl.Items.Count - 1 : index - 1;

                documentControl.SelectedControl = documentControl.Items[index];
            }
        }
    }
}
