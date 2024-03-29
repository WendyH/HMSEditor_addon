﻿using System.Drawing;
using System.Windows.Forms;

namespace HMSEditorNS {
    public partial class ValueHintControl: UserControl {
        public string Expression { get { return label1.Text; } set { label1.Text = value; } }
        public string Value      { get { return richTextBox1.Text; } set { richTextBox1.Text = value; } }

        public ValueHintControl() {
            InitializeComponent();
            Visible = false;
        }

        public void ShowValue(Control ctl, string expression, string value, Point point) {
            Expression = expression;
            Value = value;

            Point p = PointToClient(point);
            Left = p.X;
            Top = p.Y;
            Visible = true;
        }

    }
}
