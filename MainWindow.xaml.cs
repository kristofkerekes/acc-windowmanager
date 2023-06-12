using System.ComponentModel;
using System.Windows;

namespace ACCWindowManager {
	public class MainWindowViewModel : INotifyPropertyChanged {
		public ACCWindowController WindowController { get; }
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

		public MainWindowViewModel(ACCWindowController windowController) {
			WindowController = windowController;

			WindowController.ACCDetected += OnACCDetected;
			WindowController.ACCResized += OnACCResized;
		}

		~MainWindowViewModel() {
			WindowController.ACCDetected -= OnACCDetected;
			WindowController.ACCResized -= OnACCResized;
		}

		private string m_errorMessage = "";
		private string m_feedbackMessage = "";

		private void HandleError(ACCWindowController.ErrorCode errorCode) {
			switch (errorCode) {
				case ACCWindowController.ErrorCode.NoError:
					ErrorMessage = "";
					break;
				case ACCWindowController.ErrorCode.SteamNotFound:
					ErrorMessage = "Steam is not running.";
					break;
				case ACCWindowController.ErrorCode.ACCAlreadyRunning:
					ErrorMessage = "Assetto Corsa Competizione is already running.";
					break;
				case ACCWindowController.ErrorCode.ACCPathNotRegistered:
					ErrorMessage = "Assetto Corsa Competizione path not registered. Launch manually!";
					break;
				case ACCWindowController.ErrorCode.ACCIsNotRunning:
					ErrorMessage = "Assetto Corsa Competizione is not running.";
					break;
				case ACCWindowController.ErrorCode.ACCMainWindowNotFound:
					ErrorMessage = "Assetto Corsa Competizione main window was not found.";
					break;
			}
		}

		public void OnApplyClicked() {
			var errorCode = WindowController.ResizeACCWindow();
			HandleError(errorCode);
		}

		public void OnLaunchClicked() {
			var errorCode = WindowController.LaunchACC();
			HandleError(errorCode);
			if (errorCode != ACCWindowController.ErrorCode.NoError) {
				return;
			}

			FeedbackMessage = "Launched Assetto Corsa Competizione, automatic resizing.";
		}

		private void OnACCDetected() {
			ErrorMessage = "";
			FeedbackMessage = "Detected Assetto Corsa Competizione, automatic resizing.";
		}

		private void OnACCResized() {
			ErrorMessage = "";
			FeedbackMessage = "";
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string propertyName) {
			if (PropertyChanged != null) {
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}

	public partial class MainWindow : System.Windows.Window {
		private MainWindowViewModel ViewModel;

		public MainWindow(MainWindowViewModel vm) {
			ViewModel = vm;
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
