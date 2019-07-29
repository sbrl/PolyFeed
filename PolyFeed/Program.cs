using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nett;

namespace PolyFeed
{
	internal class Settings
	{
		public readonly string ProgramName = "PolyFeed";
		public readonly string Description = "creates Atom feeds from websites that don't support it";

		public string ConfigFilepath = null;
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
						showHelp();
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

			if (settings.ConfigFilepath == null) {
				Console.Error.WriteLine("Error: No configuration filepath detected. Try " +
					"using --help to show usage information.");
				return 1;
			}

			///// 2: Acquire environment variables /////


			///// 3: Run program /////

			return run().Result;
		}

		private static void showHelp()
		{
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
			Console.WriteLine("    -c  --config     Specifies the location of the TOML feed configuration file to use to generate a feed");
			Console.WriteLine("    -o  --output     Specifies the location to write the output feed to (default: feed.atom)");
		}

		private static async Task<int> run()
		{
			TomlSettings parseSettings = TomlSettings.Create(s =>
				s.ConfigurePropertyMapping(m => m.UseTargetPropertySelector(new SnakeCasePropertySelector()))
			);
			FeedSource feedSource = Toml.ReadFile<FeedSource>(settings.ConfigFilepath, parseSettings);

			if (feedSource == null) {
				Console.Error.WriteLine("Error: Somethine went wrong when parsing your settings file :-(");
				return 1;
			}

			if (!string.IsNullOrWhiteSpace(feedSource.Feed.Output))
				settings.OutputFilepath = feedSource.Feed.Output;

			FeedBuilder feedBuilder = new FeedBuilder();
			try {
				await feedBuilder.AddSource(feedSource);
			} catch (ApplicationException error) {
				Console.Error.WriteLine(error.Message);
				return 2;
			}
			await Console.Error.WriteLineAsync($"[Output] Writing feed to {settings.OutputFilepath}");
			File.WriteAllText(settings.OutputFilepath, await feedBuilder.Render());

			return 0;
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
