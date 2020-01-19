using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Comparer {
    public partial class formMain : Form {
        public string File1 = "";
        public string File2 = "";
        public bool Semantic;
        public bool IgnoreSpaces = true;
        public bool NoChangesInSpaces;

        public formMain() {
            InitializeComponent();
        }

        public void CopmareFiles() {
            diffControl1.File1 = File1;
            diffControl1.File2 = File2;
            diffControl1.LoadFile1();
            diffControl1.LoadFile2();

            diffControl1.TrimEndWhenDiff = IgnoreSpaces;
            diffControl1.SemanticMerge = Semantic;
            diffControl1.NoSelectEmptyAreas = NoChangesInSpaces;

            diffControl1.Compare();
        }

        private void formMain_Load(object sender, EventArgs e) {
            CopmareFiles();
        }
    }
}
