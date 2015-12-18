using System.Windows.Forms;
using Darwen.Windows.Forms.Controls.Docking;

namespace HMSEditorNS {
    public partial class DockingPanel: Darwen.Windows.Forms.Controls.Docking.DockingManagerControl {
        public HelpPanel      HelpPanel = new HelpPanel();
        public DockingControl HelpControl;

        public DockingPanel() {
            InitializeComponent();

            IDockingPanel rightPanel = this.Panels[DockingType.Right].InsertPanel(0);
            rightPanel.Dimension = 400;
            HelpControl = (DockingControl)rightPanel.DockedControls.Add("Справочник", HelpPanel);

        }
    }
}
