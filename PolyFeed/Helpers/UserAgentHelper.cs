using System;
using System.Reflection;

namespace PolyFeed.Helpers
{
	public static class UserAgentHelper
	{
		public static string UserAgent {
			get {
				return $"PolyFeed/{version} ({os_name} {cpu_arch}; +https://github.com/sbrl/PolyFeed) .NET-CLR/{clr_version} {mono_info}";
			}
		}

		private static string version => Program.GetProgramVersion();
		private static string os_name => Environment.OSVersion.Platform.ToString().Replace("Unix", "Linux");
		private static string cpu_arch => System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString();

		private static string clr_version => Environment.Version.ToString();

		private static string mono_info {
			get {
				Type type = Type.GetType("Mono.Runtime");
				if (type == null)
					return string.Empty;

				MethodInfo displayName = type.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
				if (displayName != null)
					return $"Mono/{(string)displayName.Invoke(null, null)}";
				
				return string.Empty;
			}
		}
	}
}
