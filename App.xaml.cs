using System;
using System.Windows;

namespace ACCWindowManager {
	public partial class App : Application {
		public Action ACCProcessDetected;

		public App() {
			m_winEventDelegate = new WinAPIHelpers.WinAPI.WinEventDelegate(WinEventProc);
			WinAPIHelpers.WinAPI.SetWinEventHook(WinAPIHelpers.WinAPI.EVENT_SYSTEM_FOREGROUND,
												 WinAPIHelpers.WinAPI.EVENT_SYSTEM_FOREGROUND,
												 IntPtr.Zero,
												 m_winEventDelegate,
												 0,
												 0,
												 WinAPIHelpers.WinAPI.EVENT_OUTOFCONTEXT);
		}

		public static void SettingsSaveRequested() {
			ACCWindowManager.Properties.Settings.Default.Save();
		}

		private void ApplicationExited(object sender, ExitEventArgs e) {
			SettingsSaveRequested();
		}

		public void WinEventProc(IntPtr hWinEventHook, int eventType, int hWnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime) {
			string activeWindowTitle = WinAPIHelpers.WindowFinder.GetActiveWindowTitle();
			if (activeWindowTitle != null && activeWindowTitle.Contains(ACCData.ProcessInfo.AppName)) {
				ACCProcessDetected?.Invoke();
			}
		}

		WinAPIHelpers.WinAPI.WinEventDelegate m_winEventDelegate;
	}
}
