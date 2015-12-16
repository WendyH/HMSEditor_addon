using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;

namespace Darwen.Windows.Forms.General
{
    public sealed class NativeMethods
    {
        private NativeMethods()
        {
        }

        public sealed class Constants
        {
            public const int DC_ACTIVE = 0x0001;
            public const int DC_SMALLCAP = 0x0002;
            public const int DC_ICON = 0x0004;
            public const int DC_TEXT = 0x0008;
            public const int DC_INBUTTON = 0x0010;
            public const int DC_GRADIENT = 0x0020;
            public const int DC_BUTTONS = 0x1000;

            public const int WM_MOUSEMOVE = 0x0200;
            public const int WM_LBUTTONDOWN = 0x0201;
            public const int WM_LBUTTONUP = 0x0202;
            public const int WM_SETREDRAW = 0x000B;
            public const int WM_MOUSEACTIVATE = 0x0021;

            public const int WM_KEYDOWN = 0x0100;
            public const int WM_KEYUP = 0x0101;
            public const int WM_CHAR = 0x0102;            

            private Constants()
            {
            }
        }

        [DllImport("User32.dll")]
        static private extern int DrawCaption(IntPtr hWnd, IntPtr hDC, IntPtr rect, int flags);

        [DllImport("User32.dll")]
        static private extern int SetWindowTextW(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)] string title);

        [DllImport("User32.dll")]
        static private extern int GetWindowTextW(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder title, int maximum);

        [DllImport("User32.dll")]
        static private extern int SendMessage(IntPtr hWnd, int message, int wParam, int lParam);

        [DllImport("User32.dll")]
        static private extern int PostMessage(IntPtr hWnd, int message, int wParam, int lParam);

        static public void SetWindowText(Control window, string title)
        {
            HandleRef handle = new HandleRef(window, window.Handle);
            SetWindowTextW(handle.Handle, title);            
        }

        static public void DrawCaption(Control control, Graphics graphics, Rectangle bounds)
        {
            DrawCaption(control, graphics, bounds, Constants.DC_ACTIVE | Constants.DC_TEXT | Constants.DC_GRADIENT);
        }

        static public void DrawCaption(Control control, Graphics graphics, Rectangle bounds, int flags)
        {
            HandleRef handle = new HandleRef(control, control.Handle);
            int sizeOfRect = Marshal.SizeOf(bounds);

            IntPtr pRectCaption = Marshal.AllocCoTaskMem(Marshal.SizeOf(bounds));
            Marshal.StructureToPtr(bounds, pRectCaption, false);

            IntPtr hDC = graphics.GetHdc();
            DrawCaption(handle.Handle, hDC, pRectCaption, flags);
            graphics.ReleaseHdc(hDC);

            Marshal.FreeCoTaskMem(pRectCaption);
        }

        static public void DrawCaption(Control control, Rectangle bounds)
        {
            using (Graphics graphics = control.CreateGraphics())
            {
                DrawCaption(control, graphics, bounds);
            }
        }

        static public string GetWindowText(Control control)
        {
            HandleRef handle = new HandleRef(control, control.Handle);
            int size = 0;
            int sizeIncrease = 2;

            StringBuilder title = new StringBuilder();

            int read = 0;

            do
            {
                size += sizeIncrease;
                title.Capacity = size;
                read = GetWindowTextW(handle.Handle, title, size);
            }
            while (read == size - 1);
            
            return title.ToString();
        }

        static public MouseEventArgs GetMouseEventArgs(Message m)
        {
            System.Diagnostics.Debug.Assert(m.Msg == Constants.WM_LBUTTONUP ||
                m.Msg == Constants.WM_LBUTTONDOWN ||
                m.Msg == Constants.WM_MOUSEMOVE, "Message is not a mouse message");

            MouseButtons buttons = MouseButtons.None;

            switch (m.Msg)
            {
                case Constants.WM_LBUTTONUP:
                case Constants.WM_LBUTTONDOWN:
                    buttons = MouseButtons.Left;
                    break;
            }            

            uint lParam = (uint)m.LParam;
            uint wParam = (uint)m.WParam;
            return new MouseEventArgs(buttons, 1, (int)(lParam & 0xFFFF), (int)((lParam >> 16) & 0xFFFF), 0);
        }

        static public void SendMessage(Control control, int message, int wParam, int lParam)
        {
            HandleRef handle = new HandleRef(control, control.Handle);
            SendMessage(handle.Handle, message, wParam, lParam);
        }

        static public void PostMessage(Control control, int message, int wParam, int lParam)
        {
            HandleRef handle = new HandleRef(control, control.Handle);
            PostMessage(handle.Handle, message, wParam, lParam);
        }
    }
}
