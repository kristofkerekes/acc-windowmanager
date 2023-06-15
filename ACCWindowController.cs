using ProcessHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace ACCWindowManager {
	public class ACCWindowController : INotifyPropertyChanged {
		public enum ErrorCode {
			NoError,
			SteamNotFound,
			ACCAlreadyRunning,
			ACCIsNotRunning,
			ACCMainWindowNotFound,
		}

		public Action ACCDetected;
		public Action ACCResized;
		public Action SelectedWindowPropertiesChanged;

		public List<KeyValuePair<string, WindowProperties>> Settings {
			get { return m_settings; }
			set {
				m_settings = value;
				OnPropertyChanged(nameof(Settings));
			}
		}

		public KeyValuePair<string, WindowProperties> SelectedWindowProperties {
			get { return m_selectedWindowProperties; }
			set {
				m_selectedWindowProperties = value;
				OnPropertyChanged(nameof(SelectedWindowProperties));
				SelectedWindowPropertiesChanged?.Invoke();

				Properties.Settings.Default.SelectedProperty = SelectedWindowProperties.Key;
			}
		}

		public KeyValuePair<string, WindowProperties> CustomWindowProperties {
			get { return m_customWindowProperties; }
			set {
				m_customWindowProperties = value;
				OnPropertyChanged(nameof(CustomWindowProperties));

				Properties.Settings.Default.CustomWindowProperties = CustomWindowProperties.Value;
			}
		}

		public ACCWindowController() {
			Settings = ACCData.DefaultWindowSettings.AllSettings.ToList();

			var customProperties = Properties.Settings.Default.CustomWindowProperties;
			if (customProperties == null) {
				customProperties = Settings.First().Value;
			}
			CustomWindowProperties = new KeyValuePair<string, WindowProperties>(ACCData.DefaultWindowSettings.CustomSettingsName, customProperties);
			Settings.Add(CustomWindowProperties);

			KeyValuePair<string, WindowProperties>? selectedProperty = Settings.Find(setting => setting.Key == Properties.Settings.Default.SelectedProperty);
			if (selectedProperty == null) {
				selectedProperty = Settings.First();
			}
			SelectedWindowProperties = (KeyValuePair<string, WindowProperties>)selectedProperty;

			m_winEventDelegate = new WinAPIHelpers.WinAPI.WinEventDelegate(WinEventProc);
			WinAPIHelpers.WinAPI.SetWinEventHook(WinAPIHelpers.WinAPI.EVENT_SYSTEM_FOREGROUND,
												 WinAPIHelpers.WinAPI.EVENT_SYSTEM_FOREGROUND,
												 IntPtr.Zero,
												 m_winEventDelegate,
												 0,
												 0,
												 WinAPIHelpers.WinAPI.EVENT_OUTOFCONTEXT);
		}

		public void Initialize() {
			Window accMainWindow;
			var errorCode = GetACCWindow(out accMainWindow);
			if (errorCode != ErrorCode.NoError) {
				return;
			}

			ACCDetected?.Invoke();
			ResizeACCWindow();
		}

		public ErrorCode LaunchACC() {
			var accProcess = Process.FindProcess(ACCData.ProcessInfo.AppName);
			if (accProcess != null) {
				return ErrorCode.ACCAlreadyRunning;
			}

			var accStarterProcess = new System.Diagnostics.Process();
			accStarterProcess.StartInfo.FileName = "steam://rungameid/" + ACCData.ProcessInfo.AppID;
			accStarterProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
			accStarterProcess.Start();

			return ErrorCode.NoError;
		}

		public ErrorCode ResizeACCWindow() {
			Window accMainWindow;
			var errorCode = GetACCWindow(out accMainWindow);
			if (errorCode != ErrorCode.NoError) {
				return errorCode;
			}

			ResizeACCWindow(accMainWindow);
			return ErrorCode.NoError;
		}

		private ErrorCode GetACCWindow(out Window mainWindow) {
			mainWindow = null;

			var accProcess = Process.FindProcess(ACCData.ProcessInfo.AppName);
			if (accProcess == null) {
				return ErrorCode.ACCIsNotRunning;
			}

			var accWindows = WinAPIHelpers.WindowFinder.GetProcessWindows(accProcess);
			if (accWindows == null) {
				return ErrorCode.ACCIsNotRunning;
			}

			mainWindow = accWindows.FirstOrDefault(w => w.Name.Contains(ACCData.ProcessInfo.MainWindowName));
			if (mainWindow == null) {
				return ErrorCode.ACCMainWindowNotFound;
			}

			return ErrorCode.NoError;
		}

		private void ResizeACCWindow(Window mainWindow) {
			if (!mainWindow.WindowInfo.ToProperties().Equals(m_selectedWindowProperties.Value)) {
				WindowManager.ApplyChanges(mainWindow, m_selectedWindowProperties.Value);
			}
			ACCResized?.Invoke();
		}

		public void OnACCDetected() {
			Window accMainWindow;
			var errorCode = GetACCWindow(out accMainWindow);
			if (errorCode != ErrorCode.NoError) {
				return;
			}

			if (!accMainWindow.WindowInfo.ToProperties().Equals(m_selectedWindowProperties.Value)) {
				ACCDetected?.Invoke();
				Task.Delay(10000).ContinueWith(_ => ResizeACCWindow(accMainWindow));
			}
		}

		private List<KeyValuePair<string, WindowProperties>> m_settings;
		private KeyValuePair<string, WindowProperties> m_selectedWindowProperties;
		private KeyValuePair<string, WindowProperties> m_customWindowProperties;

		public void WinEventProc(IntPtr hWinEventHook, int eventType, int hWnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime) {
			string activeWindowTitle = WinAPIHelpers.WindowFinder.GetActiveWindowTitle();
			if (activeWindowTitle != null && activeWindowTitle.Contains(ACCData.ProcessInfo.AppName)) {
				OnACCDetected();
			}
		}

		WinAPIHelpers.WinAPI.WinEventDelegate m_winEventDelegate;

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string propertyName) {
			if (PropertyChanged != null) {
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
