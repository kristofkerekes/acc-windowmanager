using System;
using System.Text;
using WinAPIHelpers;

namespace ACCWindowManager {
	[Serializable]
	public class WindowProperties {
		public int PosX { get; set; }
		public int PosY { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public uint Style { get; set; }
		public uint ExStyle { get; set; }

		public WindowProperties() { }

		public bool Equals(WindowProperties other) {
			return PosX == other.PosX && PosY == other.PosY && Width == other.Width && Height == other.Height && Style == other.Style && ExStyle == other.ExStyle;
		}
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
		public static WindowProperties ToProperties(this WINDOWINFO windowInfo) {
			return new WindowProperties() {
				PosX = windowInfo.rcWindow.left,
				PosY = windowInfo.rcWindow.top,
				Width = windowInfo.rcWindow.Width,
				Height = windowInfo.rcWindow.Height,
				Style = windowInfo.dwStyle,
				ExStyle = windowInfo.dwExStyle
			};
		}

		public static void ApplyChanges(Window window, WindowProperties properties) {
			uint uFlags = WinAPI.SWP_NOSIZE | WinAPI.SWP_NOMOVE | WinAPI.SWP_NOZORDER | WinAPI.SWP_NOACTIVATE | WinAPI.SWP_NOOWNERZORDER | WinAPI.SWP_NOSENDCHANGING;
			var currentProperties = window.WindowInfo.ToProperties();

			if (currentProperties.Style != properties.Style) {
				WinAPI.SetWindowLong(window.HandleID, WinAPI.GWL_STYLE, properties.Style);
				uFlags |= WinAPI.SWP_FRAMECHANGED;
			}

			if (currentProperties.ExStyle != properties.ExStyle) {
				WinAPI.SetWindowLong(window.HandleID, WinAPI.GWL_EXSTYLE, properties.ExStyle);
				uFlags |= WinAPI.SWP_FRAMECHANGED;
			}

			if (currentProperties.PosX != properties.PosX || currentProperties.PosY != properties.PosY) {
				uFlags ^= WinAPI.SWP_NOMOVE;
			}

			if (currentProperties.Height != properties.Height || currentProperties.Width != properties.Width) {
				uFlags ^= WinAPI.SWP_NOSIZE;
			}

			WinAPI.ShowWindow(window.HandleID, WinAPI.SW_SHOWNOACTIVATE);
			WinAPI.SetWindowPos(window.HandleID, 0, properties.PosX, properties.PosY, properties.Width, properties.Height, uFlags);

			WinAPI.SendMessage(window.HandleID, WinAPI.WM_EXITSIZEMOVE, 0, 0);
		}
	}
}
