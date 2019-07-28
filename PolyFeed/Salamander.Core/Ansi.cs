using System;

namespace Salamander.Core.Helpers
{
	public static class Ansi
	{
		/// <summary>
		/// Whether we should *actually* emit ANSI escape codes or not.
		/// Useful when we want to output to a log file, for example.
		/// </summary>
		public static bool Enabled { get; set; } = true;

		// Solution on how to output ANSI escape codes in C# from here:
		// https://www.jerriepelser.com/blog/using-ansi-color-codes-in-net-console-apps
		public static string Reset => Enabled ? "\u001b[0m" : "";
		public static string HiCol => Enabled ? "\u001b[1m" : "";
		public static string Underline => Enabled ? "\u001b[4m" : "";
		public static string Inverse => Enabled ? "\u001b[7m" : "";

		public static string FBlack => Enabled ? "\u001b[30m" : "";
		public static string FRed => Enabled ? "\u001b[31m" : "";
		public static string FGreen => Enabled ? "\u001b[32m" : "";
		public static string FYellow => Enabled ? "\u001b[33m" : "";
		public static string FBlue => Enabled ? "\u001b[34m" : "";
		public static string FMagenta => Enabled ? "\u001b[35m" : "";
		public static string FCyan => Enabled ? "\u001b[36m" : "";
		public static string FWhite => Enabled ? "\u001b[37m" : "";

		public static string BBlack => Enabled ? "\u001b[40m" : "";
		public static string BRed => Enabled ? "\u001b[41m" : "";
		public static string BGreen => Enabled ? "\u001b[42m" : "";
		public static string BYellow => Enabled ? "\u001b[43m" : "";
		public static string BBlue => Enabled ? "\u001b[44m" : "";
		public static string BMagenta => Enabled ? "\u001b[45m" : "";
		public static string BCyan => Enabled ? "\u001b[46m" : "";
		public static string BWhite => Enabled ? "\u001b[47m" : "";

		// Thanks to http://ascii-table.com/ansi-escape-sequences.php for the following ANSI escape sequences
		public static string Up(int lines = 1) => Enabled ? $"\u001b[{lines}A" : "";
		public static string Down(int lines = 1) => Enabled ? $"\u001b[{lines}B" : "";
		public static string Right(int lines = 1) => Enabled ? $"\u001b[{lines}C" : "";
		public static string Left(int lines = 1) => Enabled ? $"\u001b[{lines}D" : "";

		//public static string JumpTo(Vector2 pos) => $"\u001b[{pos.Y};{pos.X}H" : "";

		public static string CursorPosSave => Enabled ? $"\u001b[s" : "";
		public static string CursorPosRestore => Enabled ? $"\u001b[u" : "";
	}
}
