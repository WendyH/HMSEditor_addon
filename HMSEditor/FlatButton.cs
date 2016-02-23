using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace HMSEditorNS {
    public class FlatButton: Button {

        public FlatButton() {
            FlatAppearance.BorderSize = 0;
        }

        public override void NotifyDefault(bool value) {
            base.NotifyDefault(false);
        }

        protected override bool ShowFocusCues { get { return false; } }
    }
}