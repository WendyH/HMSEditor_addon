using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace HMSEditorNS {
    public partial class FlatTreeView: TreeView {

        protected override void OnHandleCreated(EventArgs e) {
            SendMessage(this.Handle, TVM_SETEXTENDEDSTYLE, (IntPtr)TVS_EX_DOUBLEBUFFER, (IntPtr)TVS_EX_DOUBLEBUFFER);
            base.OnHandleCreated(e);
        }
        // Pinvoke:
        private const int TVM_SETEXTENDEDSTYLE = 0x1100 + 44;
        private const int TVM_GETEXTENDEDSTYLE = 0x1100 + 45;
        private const int TVS_EX_DOUBLEBUFFER  = 0x0004;
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        public FlatTreeView() {
            InitializeComponent();
            if (HMS.PFC.Families.Length > 0) { // By WendyH
                base.Font = new Font(HMS.PFC.Families[0], 9.25f, FontStyle.Regular, GraphicsUnit.Point);
            } else {
                base.Font = new Font("Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            }
            DrawMode = TreeViewDrawMode.OwnerDrawText;
            HideSelection = false;
        }

        protected override void OnDrawNode(DrawTreeNodeEventArgs e) {
            if (e.Node == null) return;

            // if treeview's HideSelection property is "True", 
            // this will always returns "False" on unfocused treeview
            var selected  = (e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected;
            var unfocused = !e.Node.TreeView.Focused;

            // we need to do owner drawing only on a selected node
            // and when the treeview is unfocused, else let the OS do it for us
            if (selected && unfocused) {
                var font = e.Node.NodeFont ?? e.Node.TreeView.Font;
                using(Brush brush = new SolidBrush(Color.CornflowerBlue)) {
                    e.Graphics.FillRectangle(brush, new Rectangle(20, e.Bounds.Y, Width, ItemHeight));
                }
                TextRenderer.DrawText(e.Graphics, e.Node.Text, font, e.Bounds, SystemColors.HighlightText, TextFormatFlags.GlyphOverhangPadding);
            } else {
                var font = e.Node.NodeFont ?? e.Node.TreeView.Font;
                TextRenderer.DrawText(e.Graphics, e.Node.Text, font, e.Bounds, ForeColor);
            }
            //e.DrawDefault = true;
            //base.OnDrawNode(e);
        }


        public void AddAutocompleteItems(AutocompleteItems items) {
            foreach (var item in items) {
                var node = Nodes.Add(item.MenuText, item.ToString(), item.ImageIndex, item.ImageIndex);
                node.Tag = item;
            }
        }

        public void SelectNext(int shift) {
            if (Nodes.Count>0) {
                if (SelectedNode == null) {
                    SelectFirst();
                } else {
                    int newIndex = SelectedNode.Index + shift;
                    newIndex = Math.Max(0, Math.Min(Nodes.Count - 1, newIndex));
                    SelectedNode = Nodes[newIndex];
                }
            }
        }

        public void SelectFirst() {
            if (Nodes.Count > 0) {
                SelectedNode = Nodes[0];
            }
        }

        public void SelectLast() {
            if (Nodes.Count>0) {
                SelectedNode = Nodes[Nodes.Count - 1];
            }
        }

    }

}
