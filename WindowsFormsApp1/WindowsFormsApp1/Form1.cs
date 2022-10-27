using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp1 {
    [StructLayout(LayoutKind.Sequential)]
    public struct CWPRETSTRUCT {
        public IntPtr lResult;
        public IntPtr lParam;
        public IntPtr wParam;
        public uint message;
        public IntPtr hWnd;
    }

    public partial class Form1 : Form {
        private delegate IntPtr CallWndRetProc(int nCode, IntPtr wParam, IntPtr lParam);

        const int WH_CALLWNDPROCRET = 12;

        private static IntPtr _hHook = IntPtr.Zero;
        private CallWndRetProc _hookCallback;

        public Form1() {
            InitializeComponent();

            var unmanagedThreadId = AppDomain.GetCurrentThreadId();
            _hookCallback = new CallWndRetProc(WndProcRetCallback);
            _hHook = SetWindowsHookEx(WH_CALLWNDPROCRET, _hookCallback, IntPtr.Zero, (uint)unmanagedThreadId);
        }

        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, CallWndRetProc callback, IntPtr hInstance, uint threadId);

        [DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        [DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, int wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);

        public IntPtr WndProcRetCallback(int code, IntPtr wParam, IntPtr lParam) {
            if (code >= 0) {
                try {
                    var wndProcRetArgs = (CWPRETSTRUCT)Marshal.PtrToStructure(lParam, typeof(CWPRETSTRUCT));

                    Console.Write("hwnd: {0,10}  ", wndProcRetArgs.hWnd);
                    Console.Write("lParam: {0,10}  ", wndProcRetArgs.lParam);
                    Console.Write("wParam: {0,10}  ", wndProcRetArgs.wParam);
                    Console.Write("message: {0,10}  ", wndProcRetArgs.message);
                    Console.Write("result: {0,10}\n", wndProcRetArgs.lResult);
                } catch (Exception e) {
                    Console.WriteLine(e.Message);
                }

                return (IntPtr)1;
            } else
                return CallNextHookEx(_hHook, code, (int)wParam, lParam);
        }

        private void Form1_Load(object sender, EventArgs e) {
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            UnhookWindowsHookEx(_hHook);
        }
    }
}
