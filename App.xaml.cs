using System;
using System.Windows;

namespace ACCWindowManager {
	public partial class App : Application {
		public Action ACCProcessDetected;

		public App() {
			m_windowController = new ACCWindowController(this);
			m_mainWindow = new MainWindow(new MainWindowViewModel(m_windowController));

			m_winEventDelegate = new WinAPIHelpers.WinAPI.WinEventDelegate(WinEventProc);
			WinAPIHelpers.WinAPI.SetWinEventHook(WinAPIHelpers.WinAPI.EVENT_SYSTEM_FOREGROUND,
												 WinAPIHelpers.WinAPI.EVENT_SYSTEM_FOREGROUND,
												 IntPtr.Zero,
												 m_winEventDelegate,
												 0,
												 0,
												 WinAPIHelpers.WinAPI.EVENT_OUTOFCONTEXT);

			MainWindow = m_mainWindow;
			MainWindow.Show();

			m_windowController.Initialize();
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

		MainWindow m_mainWindow;
		ACCWindowController m_windowController;

		WinAPIHelpers.WinAPI.WinEventDelegate m_winEventDelegate;
	}
}
