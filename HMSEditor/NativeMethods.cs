/* This code is released under WTFPL Version 2 (http://www.wtfpl.net/) * Created by WendyH. Copyleft. */
using System;
using System.Runtime.InteropServices;
// ReSharper disable MemberCanBePrivate.Local

namespace HMSEditorNS {
    internal static class NativeMethods {
        public const uint WM_NEXTDLGCTL = 0x28;
        public const uint WM_SETFOCUS   = 0x0007;
        public const uint WM_SYSCOMMAND = 274;
        public const uint SC_MINIMIZE   = 0xF020;
        public const uint WM_KEYDOWN    = 0x0100;
        public const uint WM_NCLBUTTONDOWN = 0xA1;
        public const uint HT_CAPTION       = 0x2;

        public const uint COINIT_MULTITHREADED     = 0x0; //Initializes the thread for multi-threaded object concurrency.
        public const uint COINIT_APARTMENTTHREADED = 0x2; //Initializes the thread for apartment-threaded object concurrency
        public const uint COINIT_DISABLE_OLE1DDE   = 0x4; //Disables DDE for OLE1 support
        public const uint COINIT_SPEED_OVER_MEMORY = 0x8; //Trade memory for speed

        [DllImport("Ole32")]
        public static extern int CoInitializeEx(IntPtr pvReserved, uint dwCoInit);

        [DllImport("Ole32")]
        public static extern int CoInitialize(IntPtr pvReserved);

        [DllImport("User32")]
        public static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("User32")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndParent);

        [DllImport("Wintrust.dll", PreserveSig = true, SetLastError = false)]
        public static extern uint WinVerifyTrust(IntPtr hWnd, IntPtr pgActionID, IntPtr pWinTrustData); // 4 class AuthenticodeTools

        [DllImport("User32", CharSet = CharSet.Unicode)]
        public static extern bool SendNotifyMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImportAttribute("User32")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        #region methods FastColoredTextBox used
        [DllImport("User32", CharSet = CharSet.Unicode)]
        public static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("User32", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("User32")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("User32")]
        public static extern bool CreateCaret(IntPtr hWnd, IntPtr hBitmap, int nWidth, int nHeight);

        [DllImport("User32")]
        public static extern bool SetCaretPos(int x, int y);

        [DllImport("User32")]
        public static extern bool DestroyCaret();

        [DllImport("User32")]
        public static extern bool ShowCaret(IntPtr hWnd);

        [DllImport("User32")]
        public static extern bool SetCaretBlinkTime(uint uMSeconds);

        [DllImport("User32")]
        public static extern bool HideCaret(IntPtr hWnd);

        [DllImport("User32")]
        public static extern IntPtr GetOpenClipboardWindow();

        [DllImport("User32")]
        public static extern IntPtr CloseClipboard();

        [DllImport("Imm32")]
        public static extern IntPtr ImmGetContext(IntPtr hWnd);

        [DllImport("Imm32")]
        public static extern IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hIMC);

        [DllImport("kernel32.dll")]
        static extern void GetNativeSystemInfo(ref SYSTEM_INFO lpSystemInfo);

        [DllImport("kernel32.dll")]
        static extern void GetSystemInfo(ref SYSTEM_INFO lpSystemInfo);

        [DllImport("User32")]
        public static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("User32")]
        public static extern void ReleaseDC(IntPtr hwnd, IntPtr dc);

        [StructLayout(LayoutKind.Sequential)]
        struct SYSTEM_INFO {
            public ushort  wProcessorArchitecture;
            public ushort  wReserved;
            public uint    dwPageSize;
            public IntPtr  lpMinimumApplicationAddress;
            public IntPtr  lpMaximumApplicationAddress;
            public UIntPtr dwActiveProcessorMask;
            public uint    dwNumberOfProcessors;
            public uint    dwProcessorType;
            public uint    dwAllocationGranularity;
            public ushort  wProcessorLevel;
            public ushort  wProcessorRevision;
        }

        public enum Platform { X86, X64, Unknown }

        public static Platform GetOperationSystemPlatform() {
            var sysInfo = new SYSTEM_INFO();

            // WinXP and older - use GetNativeSystemInfo
            if (Environment.OSVersion.Version.Major > 5 ||
                (Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1)) {
                GetNativeSystemInfo(ref sysInfo);
            } else {
                GetSystemInfo(ref sysInfo);
            }

            switch (sysInfo.wProcessorArchitecture) {
                case PROCESSOR_ARCHITECTURE_IA64:
                case PROCESSOR_ARCHITECTURE_AMD64:
                    return Platform.X64;

                case PROCESSOR_ARCHITECTURE_INTEL:
                    return Platform.X86;

                default:
                    return Platform.Unknown;
            }
        }

        const ushort PROCESSOR_ARCHITECTURE_INTEL = 0;
        const ushort PROCESSOR_ARCHITECTURE_IA64  = 6;
        const ushort PROCESSOR_ARCHITECTURE_AMD64 = 9;

        public const uint WM_SETREDRAW = 0x0B;
        #endregion

        public static void SendNotifyKey(IntPtr hwnd, int key) {
            SendNotifyMessage(hwnd, WM_KEYDOWN, (IntPtr)key, IntPtr.Zero);
        }

        [DllImport("gdi32.dll")]
        public static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        public static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        public static IntPtr FindWindowByCaption(string caption)
        {
            return FindWindowByCaption(IntPtr.Zero, caption);
        }

        [DllImport("USER32.dll")]
        private static extern short GetKeyState(VirtualKeyStates nVirtKey);

        public static bool KeyState(VirtualKeyStates nVirtKey) {
            return Convert.ToBoolean(GetKeyState(nVirtKey) & 0x8000);
        }

        public enum VirtualKeyStates : int {
            VK_SHIFT = 0x10,
            VK_CONTROL = 0x11
        }
    }
}