using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ProjectNamespace
{
	internal class Settings
	{
		public readonly string ProgramName = "PolyFeed";
		public readonly string Description = "creates Atom feeds from websites that don't support it";
		// Settings here
	}

	class Program
	{
		private static Settings settings = new Settings();

		public static int Main(string[] args)
		{

			///// 1: Parse arguments /////
			List<string> extras = new List<string>();
			for (int i = 0; i < args.Length; i++)
			{
				if (!args[i].StartsWith("-"))
				{
					extras.Add(args[i]);
					continue;
				}

				switch (args[i])
				{
					case "-h":
					case "--help":
						Console.WriteLine($"{settings.ProgramName}, {getProgramVersion()}");
						Console.WriteLine("    By Starbeamrainbowlabs");

						Console.WriteLine();
						Console.WriteLine($"This program {settings.Description}.");
						Console.WriteLine();
						Console.WriteLine("Usage:");
						Console.WriteLine($"    ./{Path.GetFileName(Assembly.GetExecutingAssembly().Location)} [arguments]");
						Console.WriteLine();
						Console.WriteLine("Options:");
						Console.WriteLine("    -h  --help    Displays this message");
						Console.WriteLine("    -v  --version Outputs the version number of this program");
						return 0;

					case "-v":
					case "--version":
						Console.WriteLine($"{settings.ProgramName}\t{getProgramVersion()}");
						return 0;
				}
			}

			///// 2: Acquire environment variables /////


			///// 3: Run program /////



			return 0;
		}


		#region Helper Methods

		private static string getProgramVersion()
		{
			Version version = Assembly.GetExecutingAssembly().GetName().Version;
			return $"{version.Major}.{version.Minor}";
		}

		#endregion

	}
}
