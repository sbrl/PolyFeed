using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Nett;

namespace PolyFeed
{
	internal class Settings
	{
		public readonly string ProgramName = "PolyFeed";
		public readonly string Description = "creates Atom feeds from websites that don't support it";

		public string ConfigFilepath = "feed.toml";
		public string OutputFilepath = "feed.atom";
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
						Console.WriteLine($"{settings.ProgramName}, {GetProgramVersion()}");
						Console.WriteLine("    By Starbeamrainbowlabs");

						Console.WriteLine();
						Console.WriteLine($"This program {settings.Description}.");
						Console.WriteLine();
						Console.WriteLine("Usage:");
						Console.WriteLine($"    ./{Path.GetFileName(Assembly.GetExecutingAssembly().Location)} [arguments]");
						Console.WriteLine();
						Console.WriteLine("Options:");
						Console.WriteLine("    -h  --help       Displays this message");
						Console.WriteLine("    -v  --version    Outputs the version number of this program");
						Console.WriteLine("    -c  --config     Specifies the location of the feed configuration file to use to generate a feed (default: feed.toml)");
						Console.WriteLine("    -o  --output     Specifies the location to write the output feed to (default: feed.atom)");
						return 0;

					case "-v":
					case "--version":
						Console.WriteLine($"{settings.ProgramName}\t{GetProgramVersion()}");
						return 0;

					case "-c":
					case "--config":
						settings.ConfigFilepath = args[++i];
						break;

					case "-o":
					case "--output":
						settings.OutputFilepath = args[++i];
						break;
				}
			}

			///// 2: Acquire environment variables /////


			///// 3: Run program /////



			return 0;
		}

		private static void run()
		{
			FeedSource feedSource = new FeedSource();
			TomlTable config = Toml.ReadFile(settings.ConfigFilepath, TomlSettings.Create());

			foreach (KeyValuePair<string, TomlObject> item in config) {
				string key = item.Key;
				string value = item.Value.Get<TomlString>().Value;
				feedSource.GetType().GetProperty(value).SetValue(
					feedSource,
					value
				);
			}
		}


		#region Helper Methods

		public static string GetProgramVersion()
		{
			Version version = Assembly.GetExecutingAssembly().GetName().Version;
			return $"{version.Major}.{version.Minor}";
		}

		#endregion

	}
}
