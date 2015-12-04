/* This code is released under WTFPL Version 2 (http://www.wtfpl.net/) * Created by WendyH. Copyleft. */
using System;
using System.Runtime.InteropServices;

namespace HMSEditorNS {
    internal static class NativeMethods {
        [DllImport("User32")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndParent);

        [DllImport("Wintrust.dll", PreserveSig = true, SetLastError = false)]
        public static extern uint WinVerifyTrust(IntPtr hWnd, IntPtr pgActionID, IntPtr pWinTrustData); // 4 class AuthenticodeTools

        #region methods FastColoredTextBox used
        [DllImport("User32", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("User32")]
        public static extern bool CreateCaret(IntPtr hWnd, int hBitmap, int nWidth, int nHeight);

        [DllImport("User32")]
        public static extern bool SetCaretPos(int x, int y);

        [DllImport("User32")]
        public static extern bool DestroyCaret();

        [DllImport("User32")]
        public static extern bool ShowCaret(IntPtr hWnd);

        [DllImport("User32")]
        public static extern bool HideCaret(IntPtr hWnd);

        [DllImport("user32")]
        public static extern IntPtr GetOpenClipboardWindow();

        [DllImport("user32")]
        public static extern IntPtr CloseClipboard();

        [DllImport("Imm32")]
        public static extern IntPtr ImmGetContext(IntPtr hWnd);

        [DllImport("Imm32")]
        public static extern IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hIMC);

        [DllImport("kernel32.dll")]
        static extern void GetNativeSystemInfo(ref SYSTEM_INFO lpSystemInfo);

        [DllImport("kernel32.dll")]
        static extern void GetSystemInfo(ref SYSTEM_INFO lpSystemInfo);

        [StructLayout(LayoutKind.Sequential)]
        struct SYSTEM_INFO {
            public ushort wProcessorArchitecture;
            public ushort wReserved;
            public uint dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public UIntPtr dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public ushort wProcessorLevel;
            public ushort wProcessorRevision;
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
        const ushort PROCESSOR_ARCHITECTURE_IA64 = 6;
        const ushort PROCESSOR_ARCHITECTURE_AMD64 = 9;
        const ushort PROCESSOR_ARCHITECTURE_UNKNOWN = 0xFFFF;

        public const uint WM_SETREDRAW = 0x0B;
        #endregion
    }
}