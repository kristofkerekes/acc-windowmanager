using System.Text;
using WinAPIHelpers;

namespace ACCWindowManager {
	public struct WindowProperties {
		public int PosX;
		public int PosY;
		public int Width;
		public int Height;
		public uint Style;
		public uint ExStyle;
	}

	public class Window {
		public int HandleID => m_handleID;
		public string Name => m_nameID;
		public WINDOWINFO WindowInfo => m_windowInfo;

		public Window(int handleID) {
			m_handleID = handleID;

			StringBuilder sb = new StringBuilder(256);
			WinAPI.GetWindowText(m_handleID, sb, sb.Capacity - 1);
			m_nameID = sb.ToString();

			WinAPI.GetWindowInfo(m_handleID, ref m_windowInfo);
		}

		int m_handleID;
		string m_nameID;
		WINDOWINFO m_windowInfo;
	}

	public static class WindowManager {
		public static void ApplyChanges(Window window, WindowProperties properties) {
			uint uFlags = WinAPI.SWP_NOSIZE | WinAPI.SWP_NOMOVE | WinAPI.SWP_NOZORDER | WinAPI.SWP_NOACTIVATE | WinAPI.SWP_NOOWNERZORDER | WinAPI.SWP_NOSENDCHANGING;

			if (window.WindowInfo.dwStyle != properties.Style) {
				WinAPI.SetWindowLong(window.HandleID, WinAPI.GWL_STYLE, properties.Style);
				uFlags |= WinAPI.SWP_FRAMECHANGED;
			}

			if (window.WindowInfo.dwExStyle != properties.ExStyle) {
				WinAPI.SetWindowLong(window.HandleID, WinAPI.GWL_EXSTYLE, properties.ExStyle);
				uFlags |= WinAPI.SWP_FRAMECHANGED;
			}

			if (window.WindowInfo.rcWindow.left != properties.PosX || window.WindowInfo.rcWindow.top != properties.PosY) {
				uFlags ^= WinAPI.SWP_NOMOVE;
			}

			if (window.WindowInfo.rcWindow.Height != properties.Height || window.WindowInfo.rcWindow.Width != properties.Width) {
				uFlags ^= WinAPI.SWP_NOSIZE;
			}

			WinAPI.ShowWindow(window.HandleID, WinAPI.SW_SHOWNOACTIVATE);
			WinAPI.SetWindowPos(window.HandleID, 0, properties.PosX, properties.PosY, properties.Width, properties.Height, uFlags);

			WinAPI.SendMessage(window.HandleID, WinAPI.WM_EXITSIZEMOVE, 0, 0);
		}
	}
}
