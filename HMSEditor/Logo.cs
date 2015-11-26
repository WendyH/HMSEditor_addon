using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;

namespace HMSEditorNS {
	public class Logo : Panel {
		private IContainer components ;
		private Timer      effectTimer;

		private Bitmap     bmp;
		private short[,,]  waves;
		private int        activeBuffer  = 0;
		private bool       haveWaves     = false;
		private int        bmpHeight     = 0;
		private int        bmpWidth      = 0;
		private byte[]     bmpBytes      = null;		
		private BitmapData bmpBitmapData = null;

		private void InitializeComponent() {
			components  = new Container();
			effectTimer = new Timer(components);
		}

		public Logo() {
			InitializeComponent();
		}

		[EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
		public void Init() {
			bmp = new Bitmap(BackgroundImage);

			bmpHeight  = bmp.Height;
			bmpWidth   = bmp.Width;

			waves      = new Int16[bmpWidth, bmpHeight, 2];
			bmpBytes   = new Byte [bmpWidth * bmpHeight * 4];
			bmpBitmapData = bmp.LockBits(new Rectangle(0, 0, bmpWidth, bmpHeight), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
			Marshal.Copy(bmpBitmapData.Scan0, bmpBytes, 0, bmpWidth * bmpHeight * 4);
			bmp.UnlockBits(bmpBitmapData);

			effectTimer.Tick += new EventHandler(effectTimer_Tick);
			Paint            += new PaintEventHandler(WaterEffectControl_Paint);
			MouseMove        += new MouseEventHandler(WaterEffectControl_MouseMove);
			effectTimer.Enabled  = true;
			effectTimer.Interval = 50;
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.DoubleBuffer, true);
			haveWaves = false;			
		}

		protected override void Dispose( bool disposing ) {
			if (disposing) {
				if (effectTimer != null)
					effectTimer.Dispose();
				if (components != null)
					components.Dispose();
				if (bmp != null)
					bmp.Dispose();
			}
			base.Dispose(disposing);
		}

		private void effectTimer_Tick(object sender, System.EventArgs e) {	
			if(haveWaves) {
				Invalidate();
				ProcessWaves();
			}
		}

		[EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
		public void WaterEffectControl_Paint(object sender, PaintEventArgs e) {
			using (Bitmap tmp = (Bitmap)bmp.Clone()) {
				int xOffset, yOffset; byte alpha = 255;
				int bmpLenght = bmpWidth * bmpHeight * 4;
				if (haveWaves) {
					BitmapData tmpData  = tmp.LockBits(new Rectangle(0, 0, bmpWidth, bmpHeight), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
					byte[]     tmpBytes = new Byte[bmpLenght];

					Marshal.Copy(tmpData.Scan0, tmpBytes, 0, bmpLenght);
					for(int x=1; x<bmpWidth-1; x++) {
						for(int y=1; y<bmpHeight-1; y++) {
							xOffset = (waves[x,y  ,activeBuffer] - waves[x+1,y  ,activeBuffer]) >> 3;
							yOffset = (waves[x  ,y,activeBuffer] - waves[x  ,y+1,activeBuffer]) >> 3;

							if((xOffset != 0) || (yOffset != 0)) {
								//check bounds
								if(x+xOffset >= bmpWidth -1) xOffset = bmpWidth  - x - 1;
								if(y+yOffset >= bmpHeight-1) yOffset = bmpHeight - y - 1;
								if(x+xOffset < 0) xOffset = -x;
								if(y+yOffset < 0) yOffset = -y;
								
								tmpBytes[4*(x + y*bmpWidth)    ] = bmpBytes[4*(x+xOffset + (y+yOffset)*bmpWidth)];
								tmpBytes[4*(x + y*bmpWidth) + 1] = bmpBytes[4*(x+xOffset + (y+yOffset)*bmpWidth) + 1];
								tmpBytes[4*(x + y*bmpWidth) + 2] = bmpBytes[4*(x+xOffset + (y+yOffset)*bmpWidth) + 2];
								tmpBytes[4*(x + y*bmpWidth) + 3] = alpha;
							}

						}
					}
					Marshal.Copy(tmpBytes, 0, tmpData.Scan0, bmpLenght);
					tmp.UnlockBits(tmpData);
				}

				e.Graphics.DrawImage(tmp, 0, 0, ClientRectangle.Width, ClientRectangle.Height);
				
			}
			
		}

		private void ProcessWaves() {
			int  newBuffer  = (activeBuffer == 0) ? 1 : 0;
			bool wavesFound = false;

			for(int x=1; x< bmpWidth - 1; x++) {
				for(int y=1; y< bmpHeight - 1; y++) {
					waves[x,y,newBuffer] = (short)(
						((waves[x-1, y-1, activeBuffer] +
						  waves[x  , y-1, activeBuffer] +
						  waves[x+1, y-1, activeBuffer] +
						  waves[x-1, y  , activeBuffer] +
						  waves[x+1, y  , activeBuffer] +
						  waves[x-1, y+1, activeBuffer] +
						  waves[x  , y+1, activeBuffer] +
						  waves[x+1, y+1, activeBuffer]) >> 2) - waves[x, y, newBuffer]);
					
					//damping
					if(waves[x, y, newBuffer] != 0) {
						waves[x, y, newBuffer] -= (short)(waves[x, y, newBuffer] >> 4);					
						wavesFound = true;
					}
				}
			}
			haveWaves    = wavesFound;
			activeBuffer = newBuffer;
		}

		private void PutDrop(int x, int y, short height, int radius = 1) {
			haveWaves = true;
			double dist;

			for(int i = -radius; i<=radius; i++) {
				for(int j = -radius; j<=radius; j++) {
					if(((x+i>=0) && (x+i< bmpWidth - 1)) && ((y+j>=0) && (y+j< bmpHeight - 1))) {
						dist = Math.Sqrt(i * i + j * j);
						if(dist<radius)
							waves[x+i, y+j, activeBuffer] = (short)(Math.Cos(dist*Math.PI / radius) * height);
					}
				}
			}
		}

		private void WaterEffectControl_MouseMove(object sender, MouseEventArgs e) {
			int realX = (int)((e.X / (double)this.ClientRectangle.Width) * bmpWidth);
			int realY = (int)((e.Y / (double)this.ClientRectangle.Height) * bmpHeight);
			if (e.Button == MouseButtons.Left)
				PutDrop(realX, realY, 90, 10);
			else
				PutDrop(realX, realY, 40);
		}

	}
}
