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
			ACCPathNotRegistered,
			ACCIsNotRunning,
			ACCMainWindowNotFound,
		}

		public Action ACCDetected;
		public Action ACCResized;

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
			}
		}

		public ACCWindowController(App app) {
			m_gamePath = Properties.Settings.Default.GamePath;

			m_settings = ACCData.DefaultWindowSettings.AllSettings.ToList();

			KeyValuePair<string, WindowProperties>? selectedProperty = m_settings.Find(setting => setting.Key == Properties.Settings.Default.SelectedProperty);
			if (selectedProperty == null) {
				selectedProperty = m_settings.First();
			}
			m_selectedWindowProperties = (KeyValuePair<string, WindowProperties>)selectedProperty;

			app.ACCProcessDetected += OnACCDetected;
		}

		public void Initialize() {
			var accProcess = Process.FindProcess(ACCData.ProcessInfo.AppName);
			if (accProcess != null) {
				ACCDetected?.Invoke();
				ResizeACCWindow();
			}
		}

		public ErrorCode LaunchACC() {
			var steamProcess = Process.FindProcess(ACCData.ProcessInfo.SteamAppName);
			if (steamProcess == null) {
				return ErrorCode.SteamNotFound;
			}

			var accProcess = Process.FindProcess(ACCData.ProcessInfo.AppName);
			if (accProcess != null) {
				return ErrorCode.ACCAlreadyRunning;
			}

			if (m_gamePath.Length == 0) {
				return ErrorCode.ACCPathNotRegistered;
			}

			accProcess = new System.Diagnostics.Process();
			accProcess.StartInfo.FileName = m_gamePath;
			accProcess.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(m_gamePath);
			accProcess.Start();

			Task.Delay(10000).ContinueWith(_ => ResizeACCWindow());

			return ErrorCode.NoError;
		}

		public ErrorCode ResizeACCWindow() {
			var accProcess = Process.FindProcess(ACCData.ProcessInfo.AppName);
			if (accProcess == null) {
				return ErrorCode.ACCIsNotRunning;
			}

			var accWindows = WinAPIHelpers.WindowFinder.GetProcessWindows(accProcess);
			if (accWindows == null) {
				return ErrorCode.ACCIsNotRunning;
			}

			var mainWindow = accWindows.FirstOrDefault(w => w.Name.Contains(ACCData.ProcessInfo.MainWindowName));
			if (mainWindow == null) {
				return ErrorCode.ACCMainWindowNotFound;
			}

			WindowManager.ApplyChanges(mainWindow, m_selectedWindowProperties.Value);
			ACCResized?.Invoke();

			Properties.Settings.Default.SelectedProperty = m_selectedWindowProperties.Key;
			Properties.Settings.Default.GamePath = accProcess.MainModule.FileName;
			App.SettingsSaveRequested();

			return ErrorCode.NoError;
		}

		public void OnACCDetected() {
			ACCDetected?.Invoke();
			Task.Delay(10000).ContinueWith(_ => ResizeACCWindow());
		}

		private List<KeyValuePair<string, WindowProperties>> m_settings;
		private KeyValuePair<string, WindowProperties> m_selectedWindowProperties;
		private string m_gamePath = "";

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string propertyName) {
			if (PropertyChanged != null) {
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
