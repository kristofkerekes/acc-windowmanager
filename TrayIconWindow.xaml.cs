using System;
using System.Windows;

namespace ACCWindowManager {
	public partial class TrayIconWindow : System.Windows.Window {
		public Action OpenRequested;
		public Action ExitRequested;

		public TrayIconWindow() {
			InitializeComponent();
		}

		private void TrayIconDoubleClicked(object sender, System.Windows.Input.MouseButtonEventArgs e) {
			OpenRequested?.Invoke();
		}

		private void OnOpenRequested(object sender, RoutedEventArgs e) {
			OpenRequested?.Invoke();
		}

		private void OnExitRequested(object sender, RoutedEventArgs e) {
			ExitRequested?.Invoke();
		}
	}
}
