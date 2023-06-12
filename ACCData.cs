using ACCWindowManager;
using System.Collections.Generic;

namespace ACCData {
	public static class ProcessInfo {
		public static string AppName = "AC2";
		public static string MainWindowName = "AC2";

		public static string SteamAppName = "steam";
	}

	public static class DefaultWindowSettings {
		private static uint DefaultStyle = 0x140B0000;
		private static uint DefaultExStyle = 0x20040800;

		private static WindowProperties m_tripleFullHD = new WindowProperties() {
			PosX = -1920,
			PosY = 0,
			Width = 5760,
			Height = 1080,
			Style = DefaultStyle,
			ExStyle = DefaultExStyle
		};

		private static WindowProperties m_tripleFullHDOffsetLeft = new WindowProperties() {
			PosX = -3840,
			PosY = 0,
			Width = 5760,
			Height = 1080,
			Style = DefaultStyle,
			ExStyle = DefaultExStyle
		};

		private static WindowProperties m_tripleFullHDOffsetRight = new WindowProperties() {
			PosX = 0,
			PosY = 0,
			Width = 5760,
			Height = 1080,
			Style = DefaultStyle,
			ExStyle = DefaultExStyle
		};

		private static WindowProperties m_triple4k = new WindowProperties() {
			PosX = -2560,
			PosY = 0,
			Width = 7680,
			Height = 1440,
			Style = DefaultStyle,
			ExStyle = DefaultExStyle
		};

		private static WindowProperties m_triple4kOffsetLeft = new WindowProperties() {
			PosX = -5120,
			PosY = 0,
			Width = 7680,
			Height = 1440,
			Style = DefaultStyle,
			ExStyle = DefaultExStyle
		};

		private static WindowProperties m_triple4kOffsetRight = new WindowProperties() {
			PosX = 0,
			PosY = 0,
			Width = 7680,
			Height = 1440,
			Style = DefaultStyle,
			ExStyle = DefaultExStyle
		};

		public static Dictionary<string, WindowProperties> AllSettings = new Dictionary<string, WindowProperties> {
			{ "Triple Monitors - 3x1080p", m_tripleFullHD },
			{ "Triple Monitors - 3x1440p", m_triple4k },
			{ "Triple Monitors - 3x1080p (Offset Left)", m_tripleFullHDOffsetLeft },
			{ "Triple Monitors - 3x1080p (Offset Right)", m_tripleFullHDOffsetRight },
			{ "Triple Monitors - 3x1440p (Offset Left)", m_triple4kOffsetLeft },
			{ "Triple Monitors - 3x1440p (Offset Right)", m_triple4kOffsetRight }
		};
	}
}
