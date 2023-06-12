using System.Windows;

namespace ACCWindowManager {
	public partial class App : Application {
		public App() {
			m_windowController = new ACCWindowController();

			MainWindow = new MainWindow(new MainWindowViewModel(m_windowController));
			MainWindow.Show();

			m_windowController.Initialize();
		}

		public static void SettingsSaveRequested() {
			ACCWindowManager.Properties.Settings.Default.Save();
		}

		private void ApplicationExited(object sender, ExitEventArgs e) {
			SettingsSaveRequested();
		}

		ACCWindowController m_windowController;
	}
}
