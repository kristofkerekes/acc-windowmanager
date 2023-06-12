using ACCData;
using ProcessHelpers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ACCWindowManager {
	internal class MainWindowViewModel : INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;

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

		public string GamePath {
			get { return m_gamePath; }
			set {
				m_gamePath = value;
				OnPropertyChanged(nameof(GamePath));
			}
		}

		public string ErrorMessage {
			get { return m_errorMessage; }
			set {
				m_errorMessage = value;
				OnPropertyChanged(nameof(ErrorMessage));
			}
		}

		public string FeedbackMessage {
			get { return m_feedbackMessage; }
			set {
				m_feedbackMessage = value;
				OnPropertyChanged(nameof(FeedbackMessage));
			}
		}

		public MainWindowViewModel() {
			Settings = new DefaultWindowSettings().AllSettings.ToList();

			KeyValuePair<string, WindowProperties>? selectedProperty = Settings.Find(setting => setting.Key == Properties.Settings.Default.SelectedProperty);
			if (selectedProperty == null) {
				selectedProperty = Settings.First();
			}
			SelectedWindowProperties = (KeyValuePair<string, WindowProperties>)selectedProperty;

			GamePath = Properties.Settings.Default.GamePath;

			((App)Application.Current).ACCProcessDetected += OnACCDetected;

			var accProcess = Process.FindProcess(ACCData.ProcessInfo.AppName);
			if (accProcess != null) {
				OnACCDetected();
			}
		}

		private void OnPropertyChanged(string propertyName) {
			if (PropertyChanged != null) {
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		private List<KeyValuePair<string, WindowProperties>> m_settings;
		private KeyValuePair<string, WindowProperties> m_selectedWindowProperties;
		private string m_gamePath = "";
		private string m_errorMessage = "";
		private string m_feedbackMessage = "";

		public void OnApplyClicked() {
			ErrorMessage = "";

			var accProcess = Process.FindProcess(ACCData.ProcessInfo.AppName);
			if (accProcess == null) {
				ErrorMessage = "Assetto Corsa Competizione is not running.";
				return;
			}

			var accWindows = WinAPIHelpers.WindowFinder.GetProcessWindows(accProcess);
			if (accWindows == null) {
				ErrorMessage = "Assetto Corsa Competizione is not running.";
				return;
			}

			var mainWindow = accWindows.FirstOrDefault(w => w.Name.Contains(ACCData.ProcessInfo.MainWindowName));
			if (mainWindow == null) {
				ErrorMessage = "Assetto Corsa Competizione main window was not found.";
				return;
			}

			WindowManager.ApplyChanges(mainWindow, m_selectedWindowProperties.Value);

			Properties.Settings.Default.SelectedProperty = m_selectedWindowProperties.Key;
			Properties.Settings.Default.GamePath = accProcess.MainModule.FileName;
			App.SettingsSaveRequested();
		}

		public void OnLaunchClicked() {
			ErrorMessage = "";

			var steamProcess = Process.FindProcess(ACCData.ProcessInfo.SteamAppName);
			if (steamProcess == null) {
				ErrorMessage = "Steam is not running.";
				return;
			}

			var accProcess = Process.FindProcess(ACCData.ProcessInfo.AppName);
			if (accProcess != null) {
				ErrorMessage = "Assetto Corsa Competizione is already running.";
				return;
			}

			if (m_gamePath.Length == 0) {
				ErrorMessage = "Assetto Corsa Competizione path not registered. Launch manually!";
				return;
			}

			accProcess = new System.Diagnostics.Process();
			accProcess.StartInfo.FileName = m_gamePath;
			accProcess.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(m_gamePath);
			accProcess.Start();

			FeedbackMessage = "Detected Assetto Corsa Competizione. Automatic Resizing.";

			Task.Delay(10000).ContinueWith(_ => {
				OnApplyClicked();
				FeedbackMessage = "";
			});
		}

		private void OnACCDetected() {
			ErrorMessage = "";
			FeedbackMessage = "Detected Assetto Corsa Competizione. Automatic Resizing.";

			Task.Delay(10000).ContinueWith(_ => {
				OnApplyClicked();
				FeedbackMessage = "";
			});
		}
	}

	public partial class MainWindow : System.Windows.Window {
		private MainWindowViewModel ViewModel = new MainWindowViewModel();

		public MainWindow() {
			DataContext = ViewModel;
			InitializeComponent();
		}

		private void MinimizeToTray() {
			WindowState = WindowState.Minimized;
			ShowInTaskbar = false;

			Properties.Settings.Default.WasOnTray = true;
			App.SettingsSaveRequested();
		}

		private void MaximizeFromTray() {
			ShowInTaskbar = true;
			WindowState = WindowState.Normal;
			Activate();

			Properties.Settings.Default.WasOnTray = false;
			App.SettingsSaveRequested();
		}

		private void OnApplyClicked(object sender, System.Windows.RoutedEventArgs e) {
			ViewModel.OnApplyClicked();
		}

		private void OnLaunchClicked(object sender, System.Windows.RoutedEventArgs e) {
			ViewModel.OnLaunchClicked();
		}

		private void TrayIconDoubleClicked(object sender, System.Windows.Input.MouseButtonEventArgs e) {
			MaximizeFromTray();
		}

		private void OnOpenRequested(object sender, RoutedEventArgs e) {
			MaximizeFromTray();
		}

		private void OnExitRequested(object sender, RoutedEventArgs e) {
			Application.Current.Shutdown();
		}

		private void OnWindowStateChanged(object sender, System.EventArgs e) {
			switch (WindowState) {
				case WindowState.Minimized:
					MinimizeToTray();
					break;
				case WindowState.Maximized:
				case WindowState.Normal:
					break;
			}
		}

		private void OnWindowLoaded(object sender, System.EventArgs e) {
			if (Properties.Settings.Default.WasOnTray) {
				MinimizeToTray();
			}
		}
	}
}
