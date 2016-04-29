using System.Windows.Forms;

namespace HMSEditorNS {
    public class FlatButton: Button {

        public FlatButton() {
            FlatAppearance.BorderSize = 0;
        }

        public override void NotifyDefault(bool value) {
            base.NotifyDefault(false);
        }

        protected override bool ShowFocusCues => false;
    }
}