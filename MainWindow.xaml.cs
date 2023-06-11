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

		public List<KeyValuePair<string, WindowProperties>> DefaultSettings {
			get { return m_defaultSettings; }
			set {
				m_defaultSettings = value;
				OnPropertyChanged(nameof(DefaultSettings));
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
			DefaultSettings = new DefaultWindowSettings().AllSettings.ToList();

			KeyValuePair<string, WindowProperties>? selectedProperty = DefaultSettings.Find(setting => setting.Key == Properties.Settings.Default.SelectedProperty);
			if (selectedProperty == null) {
				selectedProperty = DefaultSettings.First();
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

		private List<KeyValuePair<string, WindowProperties>> m_defaultSettings;
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
			});
		}

		private void OnACCDetected() {
			ErrorMessage = "";
			FeedbackMessage = "Detected Assetto Corsa Competizione. Automatic Resizing.";

			Task.Delay(10000).ContinueWith(_ => {
				OnApplyClicked();
			});
		}
	}

	public partial class MainWindow : System.Windows.Window {
		private MainWindowViewModel ViewModel = new MainWindowViewModel();

		public MainWindow() {
			DataContext = ViewModel;
			InitializeComponent();
		}

		private void OnApplyClicked(object sender, System.Windows.RoutedEventArgs e) {
			ViewModel.OnApplyClicked();
		}

		private void OnLaunchClicked(object sender, System.Windows.RoutedEventArgs e) {
			ViewModel.OnLaunchClicked();
		}
	}
}
