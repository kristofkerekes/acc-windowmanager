using ACCWindowManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace WinAPIHelpers {
	public static class WinAPI {
		public const int GWL_STYLE = -16;
		public const int GWL_EXSTYLE = -20;

		public const uint SWP_NOSIZE = 0x01;
		public const uint SWP_NOMOVE = 0x02;
		public const uint SWP_NOZORDER = 0x04;
		public const uint SWP_NOACTIVATE = 0x10;
		public const uint SWP_NOOWNERZORDER = 0x200;
		public const uint SWP_NOSENDCHANGING = 0x400;
		public const uint SWP_FRAMECHANGED = 0x20;

		public const uint WS_THICKFRAME = 0x40000;
		public const uint WS_DLGFRAME = 0x400000;
		public const uint WS_BORDER = 0x800000;

		public const uint WS_EX_DLGMODALFRAME = 1;
		public const uint WS_EX_WINDOWEDGE = 0x100;
		public const uint WS_EX_CLIENTEDGE = 0200;
		public const uint WS_EX_STATICEDGE = 0x20000;

		public const int SW_SHOWNOACTIVATE = 4;
		public const int SW_RESTORE = 9;

		public const int WM_EXITSIZEMOVE = 0x0232;

		public const int EVENT_OUTOFCONTEXT = 0x0000;
		public const int EVENT_SYSTEM_FOREGROUND = 0x0003;

		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
		public delegate bool EnumWindowsProc(int hwnd, IntPtr lParam);

		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
		public delegate void WinEventDelegate(IntPtr hWinEventHook, int eventType, int hWnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

		[DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
		public static extern bool EnumThreadWindows(int dwThreadId, EnumWindowsProc lpfn, IntPtr lParam);

		[DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
		public static extern int GetWindowText(int hWnd, StringBuilder lpString, int nMaxCount);

		[DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
		public static extern bool IsWindow(int hWnd);

		[DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
		public static extern bool GetWindowInfo(int hwnd, ref WINDOWINFO pwi);

		[DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
		public static extern int SetWindowLong(int hWnd, int nIndex, uint dwNewLong);

		[DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
		public static extern bool SetWindowPos(int hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

		[DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
		public static extern bool IsIconic(int hWnd);

		[DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
		public static extern bool ShowWindow(int hWnd, int nCmdShow);

		[DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
		public static extern int GetForegroundWindow();

		[DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
		public static extern int SendMessage(int hWnd, int msg, int wParam, int lParam);

		[DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
		public static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, int idProcess, int idThread, int dwFlags);
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct RECT {
		public int left;
		public int top;
		public int right;
		public int bottom;

		public int Width { get { return right - left; } }
		public int Height { get { return bottom - top; } }

		public static void CopyRect(RECT rcSrc, ref RECT rcDest) {
			rcDest.left = rcSrc.left;
			rcDest.top = rcSrc.top;
			rcDest.right = rcSrc.right;
			rcDest.bottom = rcSrc.bottom;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WINDOWINFO {
		public uint cbSize;
		public RECT rcWindow;
		public RECT rcClient;
		public uint dwStyle;
		public uint dwExStyle;
		public uint dwWindowStatus;
		public uint cxWindowBorders;
		public uint cyWindowBorders;
		public ushort atomWindowType;
		public ushort wCreatorVersion;
	}

	public static class WindowFinder {
		private class ETWPCallbackParam {
			public int MainWindowHandle;
			public List<Window> WindowList;
		}

		private static bool EnumThreadWndProc(int hwnd, IntPtr lParam) {
			if (lParam != IntPtr.Zero) {
				ETWPCallbackParam param = (ETWPCallbackParam)GCHandle.FromIntPtr(lParam).Target;

				if (param.MainWindowHandle != 0 && param.MainWindowHandle == hwnd) {
					param.WindowList.Insert(0, new Window(hwnd));
				} else {
					param.WindowList.Add(new Window(hwnd));
				}

				return true;
			}
			return false;
		}

		private static void RestoreWindow(int hWnd) {
			if (hWnd == 0 || !WinAPI.IsWindow(hWnd))
				return;

			if (WinAPI.IsIconic(hWnd)) {
				WinAPI.ShowWindow(hWnd, WinAPI.SW_SHOWNOACTIVATE);
			}
		}

		public static List<Window> GetProcessWindows(Process process) {
			WinAPI.EnumWindowsProc etwp = new WinAPI.EnumWindowsProc(EnumThreadWndProc);
			ETWPCallbackParam param = new ETWPCallbackParam() {
				MainWindowHandle = (int)process.MainWindowHandle,
				WindowList = new List<Window>()
			};
			GCHandle gch = GCHandle.Alloc(param);

			RestoreWindow(param.MainWindowHandle);

			foreach (ProcessThread thread in process.Threads) {
				WinAPI.EnumThreadWindows(thread.Id, etwp, GCHandle.ToIntPtr(gch));
			}
			gch.Free();

			return param.WindowList;
		}

		public static string GetActiveWindowTitle() {
			const int nChars = 256;
			int hWnd = WinAPI.GetForegroundWindow();

			StringBuilder Buff = new StringBuilder(nChars);
			if (WinAPI.GetWindowText(hWnd, Buff, nChars) <= 0) {
				return null;
			}

			return Buff.ToString();
		}
	}
}
