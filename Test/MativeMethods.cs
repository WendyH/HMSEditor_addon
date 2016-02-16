using System;
using System.Runtime.InteropServices;

namespace test {
    static class NativeMethods {
        [DllImport("User32")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);
    }
}
