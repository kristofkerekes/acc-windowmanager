using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

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

		public Visibility CustomSettingsVisible {
			get { return m_customSettingsVisible; }
			set {
				m_customSettingsVisible = value;
				OnPropertyChanged(nameof(CustomSettingsVisible));
			}
		}

		public int WidthInput {
			get { return m_widthInput; }
			set {
				m_widthInput = value;
				OnPropertyChanged(nameof(WidthInput));

				// Invoke setter after modifying nested property
				WindowController.CustomWindowProperties.Value.Width = value;
				WindowController.CustomWindowProperties = WindowController.CustomWindowProperties;
			}
		}

		public int HeightInput {
			get { return m_heightInput; }
			set {
				m_heightInput = value;
				OnPropertyChanged(nameof(HeightInput));

				WindowController.CustomWindowProperties.Value.Height = value;
				WindowController.CustomWindowProperties = WindowController.CustomWindowProperties;
			}
		}

		public int PosXInput {
			get { return m_posXInput; }
			set {
				m_posXInput = value;
				OnPropertyChanged(nameof(PosXInput));

				WindowController.CustomWindowProperties.Value.PosX = value;
				WindowController.CustomWindowProperties = WindowController.CustomWindowProperties;
			}
		}

		public int PosYInput {
			get { return m_posYInput; }
			set {
				m_posYInput = value;
				OnPropertyChanged(nameof(PosYInput));

				WindowController.CustomWindowProperties.Value.PosY = value;
				WindowController.CustomWindowProperties = WindowController.CustomWindowProperties;
			}
		}

		public MainWindowViewModel(ACCWindowController windowController) {
			WindowController = windowController;
			SetCustomSettingsProperties();

			WindowController.ACCDetected += OnACCDetected;
			WindowController.ACCResized += OnACCResized;
			WindowController.SelectedWindowPropertiesChanged += SetCustomSettingsProperties;
		}

		~MainWindowViewModel() {
			WindowController.ACCDetected -= OnACCDetected;
			WindowController.ACCResized -= OnACCResized;
			WindowController.SelectedWindowPropertiesChanged -= SetCustomSettingsProperties;
		}

		private string m_errorMessage = "";
		private string m_feedbackMessage = "";
		private Visibility m_customSettingsVisible = Visibility.Collapsed;
		private int m_widthInput;
		private int m_heightInput;
		private int m_posXInput;
		private int m_posYInput;

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

		private void SetCustomSettingsProperties() {
			CustomSettingsVisible = WindowController.SelectedWindowProperties.Key == ACCData.DefaultWindowSettings.CustomSettingsName ?
					  Visibility.Visible :
					  Visibility.Collapsed;

			WidthInput = WindowController.CustomWindowProperties.Value.Width;
			HeightInput = WindowController.CustomWindowProperties.Value.Height;
			PosXInput = WindowController.CustomWindowProperties.Value.PosX;
			PosYInput = WindowController.CustomWindowProperties.Value.PosY;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string propertyName) {
			if (PropertyChanged != null) {
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}

	public partial class MainWindow : System.Windows.Window {
		public Action MinimizeToTrayRequested;
		private MainWindowViewModel ViewModel;

		public MainWindow(MainWindowViewModel vm) {
			ViewModel = vm;
			DataContext = ViewModel;
			InitializeComponent();
		}

		private void OnApplyClicked(object sender, System.Windows.RoutedEventArgs e) {
			ViewModel.OnApplyClicked();
		}

		private void OnLaunchClicked(object sender, System.Windows.RoutedEventArgs e) {
			ViewModel.OnLaunchClicked();
		}

		private void OnWindowStateChanged(object sender, System.EventArgs e) {
			switch (WindowState) {
				case WindowState.Minimized:
					Close();
					MinimizeToTrayRequested?.Invoke();
					break;
				case WindowState.Maximized:
				case WindowState.Normal:
					break;
			}
		}

		private void NumberValidationTextBox(object sender, TextCompositionEventArgs e) {
			Regex regex = new Regex("[^0-9]+");
			e.Handled = regex.IsMatch(e.Text);
		}

		private void OnWindowClosing(object sender, CancelEventArgs e) {
			MinimizeToTrayRequested?.Invoke();
		}
	}
}
