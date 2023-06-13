﻿using System.Windows;

namespace ACCWindowManager {
	public partial class App : Application {
		public App() {
			m_windowController = new ACCWindowController();

			m_TrayIconWindow = new TrayIconWindow();
			m_TrayIconWindow.OpenRequested += OnOpenRequested;
			m_TrayIconWindow.ExitRequested += OnExitRequested;


			MainWindow = m_TrayIconWindow;
			if (!ACCWindowManager.Properties.Settings.Default.WasOnTray) {
				OpenMainWindow();
			}

			m_windowController.Initialize();
		}

		private void OpenMainWindow() {
			m_mainWindow = new MainWindow(new MainWindowViewModel(m_windowController));
			m_mainWindow.MinimizeToTrayRequested += OnMinimizeToTrayRequested;
			m_mainWindow.Show();
		}

		private void OnOpenRequested() {
			if (m_mainWindow == null) {
				OpenMainWindow();
			}
			m_mainWindow.Activate();

			ACCWindowManager.Properties.Settings.Default.WasOnTray = false;
			SettingsSaveRequested();
		}

		private void OnExitRequested() {
			Shutdown();
		}

		private void OnMinimizeToTrayRequested() {
			m_mainWindow = null;

			ACCWindowManager.Properties.Settings.Default.WasOnTray = true;
			SettingsSaveRequested();
		}

		public static void SettingsSaveRequested() {
			ACCWindowManager.Properties.Settings.Default.Save();
		}

		private void ApplicationExited(object sender, ExitEventArgs e) {
			SettingsSaveRequested();
		}

		ACCWindowController m_windowController;
		TrayIconWindow m_TrayIconWindow;
		MainWindow m_mainWindow;
	}
}
