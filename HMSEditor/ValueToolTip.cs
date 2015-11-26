using System;
using System.Windows.Forms;
using System.Drawing;

namespace HMSEditorNS {
	/// <summary>
	/// Класс для подсказки вычисленного выражения переменной или свойства при наведении курсора мыши в режиме отладки скрипта
	/// </summary>
	public class ValueToolTip: ToolStripDropDown {
		private TextBox ctl     = new TextBox();
		private Size    MaxSize = new Size(500, 200);
		private Timer   timer   = new Timer();
		private Size    MouseDX = new Size(20, 15);

		public ValueToolTip() : base() {
			ctl.BorderStyle = BorderStyle.FixedSingle;
			ctl.BackColor   = Color.White;
			ctl.ForeColor   = Color.Black;
			ctl.Multiline   = true;
			ctl.ReadOnly    = true;
			ctl.Font        = new Font("Arial", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
			timer.Tick     += new EventHandler(tick_CheckMouseNearly);
			timer.Interval  = 500;
			Initialize();
		}

		protected override void OnKeyDown(KeyEventArgs e) {
			Close();
			//base.OnKeyDown(e);
		}

		protected override void OnClosing(ToolStripDropDownClosingEventArgs e) {
			timer.Stop();
			base.OnClosing(e);
		}

		private void tick_CheckMouseNearly(object obj, EventArgs e) {
			int x = MousePosition.X - Left;
			int y = MousePosition.Y - Top;
			if ((x < -MouseDX.Width ) || (x - Width  > MouseDX.Width ) || (y < -MouseDX.Height) || (y - Height > MouseDX.Height))
				Hide();
		}

		public void ShowValue(Control control, string value, Point point) {
			Graphics g = ctl.CreateGraphics();
			if (value.Length == 0) value = "  ";
			Size textSize    = TextRenderer.MeasureText(g, value, ctl.Font, MaxSize, TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak);
			textSize.Height += 5;
			textSize.Width  += 16;
			ctl.ScrollBars   = ScrollBars.None;
			if (textSize.Height > MaxSize.Height) { textSize.Height = MaxSize.Height; ctl.ScrollBars = ScrollBars.Vertical; }
			if (textSize.Width  > MaxSize.Width ) { textSize.Width  = MaxSize.Width ; }
			ctl.Text = value;
			ctl.Size = textSize;
			Size     = textSize;
			Show(control, point);
			timer.Start();
		}

		public void Initialize() {
			this.AutoSize = false;
			ToolStripControlHost host = new ToolStripControlHost(this.ctl);
			this.Margin   = Padding.Empty;
			this.Padding  = Padding.Empty;
			host.Margin   = Padding.Empty;
			host.Padding  = Padding.Empty;
			host.AutoSize = false;
			host.Size     = ctl.Size;
			this.Size     = ctl.Size;
			this.Items.Add(host);
		}
	}
}
