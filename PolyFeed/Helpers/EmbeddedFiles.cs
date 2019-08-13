using System;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SBRL.Utilities
{
	/// <summary>
	/// A collection of static methods for manipulating embedded resources.
	/// </summary>
	/// <description>
	/// From https://gist.github.com/sbrl/aabfcfe87396b8c05d3263887b807d23. You may have seen 
	/// this in several other ACWs I've done. Proof I wrote this is available upon request, 
	/// of course.
	/// 
	/// v0.6.2, by Starbeamrainbowlabs <feedback@starbeamrainbowlabs.com>
	/// Last updated 28th November 2018.
	/// Licensed under MPL-2.0.
	/// 
	/// Changelog:
	/// v0.1 (25th July 2016):
	/// 	- Initial release.
	/// v0.2 (8th August 2016):
	/// 	- Changed namespace.
	/// v0.3 (21st January 2017):
	/// 	- Added GetRawReader().
	/// v0.4 (8th April 2017):
	/// 	- Removed unnecessary using statement.
	/// v0.5 (3rd September 2017):
	/// 	- Changed namespace
	/// v0.6 (12th October 2018):
	/// 	- Fixed assembly / calling assembly bugs
	/// v0.6.1 (17th october 2018):
	/// 	- Fix crash in ReadAllText(filename)
	/// v0.6.2 (28th November 2018):
	/// 	- Fix assembly targeting bug in ReadAllBytesAsync()
	/// </description>
	public static class EmbeddedFiles
	{
		/// <summary>
		/// An array of the filenames of all the resources embedded in the target assembly.
		/// </summary>
		/// <param name="targetAssembly">The target assembly to extract a resource list for.</param>
		/// <value>The resource list.</value>
		public static string[] ResourceList(Assembly targetAssembly)
		{
			return targetAssembly.GetManifestResourceNames();
		}
		/// <summary>
		/// An array of the filenames of all the resources embedded in the calling assembly.
		/// </summary>
		/// <value>The resource list.</value>
		public static string[] ResourceList()
		{
			return ResourceList(Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Gets a list of resources embedded in the calling assembly as a string.
		/// </summary>
		public static string GetResourceListText()
		{
			return GetResourceListText(Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Gets a list of resources embedded in the target assembly as a string.
		/// </summary>
		/// <param name="targetAssembly">The target assembly to extract a resource list from.</param>
		public static string GetResourceListText(Assembly targetAssembly)
		{
			StringWriter result = new StringWriter();
			result.WriteLine("Files embedded in {0}:", targetAssembly.GetName().Name);
			foreach (string filename in ResourceList(targetAssembly))
				result.WriteLine(" - {0}", filename);
			return result.ToString();
		}
		/// <summary>
		/// Writes a list of resources embedded in the calling assembly to the standard output.
		/// </summary>
		public static void WriteResourceList()
		{
			Console.WriteLine(GetResourceListText(Assembly.GetCallingAssembly()));
		}

		/// <summary>
		/// Gets a StreamReader attached to the specified embedded resource.
		/// </summary>
		/// <param name="filename">The filename of the embedded resource to get a StreamReader of.</param>
		/// <returns>A StreamReader attached to the specified embedded resource.</returns>
		public static StreamReader GetReader(string filename)
		{
			return new StreamReader(GetRawReader(filename));
		}

		/// <summary>
		/// Gets a raw Stream that's attached to the specified embedded resource 
		/// in the calling assembly.
		/// Useful when you want to copy an embedded resource to some other stream.
		/// </summary>
		/// <param name="filename">The path to the embedded resource.</param>
		/// <returns>A raw Stream object attached to the specified file.</returns>
		public static Stream GetRawReader(string filename)
		{
			return GetRawReader(Assembly.GetCallingAssembly(), filename);
		}
		/// <summary>
		/// Gets a raw Stream that's attached to the specified embedded resource 
		/// in the specified assembly.
		/// Useful when you want to copy an embedded resource to some other stream.
		/// </summary>
		/// <param name="targetAssembly">The assembly to search for the filename in.</param>
		/// <param name="filename">The path to the embedded resource.</param>
		/// <returns>A raw Stream object attached to the specified file.</returns>
		public static Stream GetRawReader(Assembly targetAssembly, string filename)
		{
			return targetAssembly.GetManifestResourceStream(filename);
		}

		/// <summary>
		/// Gets the specified embedded resource's content as a byte array.
		/// </summary>
		/// <param name="filename">The filename of the embedded resource to get conteent of.</param>
		/// <returns>The specified embedded resource's content as a byte array.</returns>
		public static byte[] ReadAllBytes(string filename)
		{
			// Referencing the Result property will block until the async method completes
			return ReadAllBytesAsync(filename).Result;
		}
		/// <summary>
		/// Gets the content of the resource that's embedded in the specified 
		/// assembly as a byte array asynchronously.
		/// </summary>
		/// <param name="targetAssembly">The assembly to search for the file in.</param>
		/// <param name="filename">The filename of the embedded resource to get content of.</param>
		/// <returns>The specified embedded resource's content as a byte array.</returns>
		public static async Task<byte[]> ReadAllBytesAsync(Assembly targetAssembly, string filename)
		{
			using (Stream resourceStream = targetAssembly.GetManifestResourceStream(filename))
			using (MemoryStream temp = new MemoryStream())
			{
				await resourceStream.CopyToAsync(temp);
				return temp.ToArray();
			}
		}
		public static async Task<byte[]> ReadAllBytesAsync(string filename)
		{
			return await ReadAllBytesAsync(Assembly.GetCallingAssembly(), filename);
		}


		/// <summary>
		/// Gets all the text stored in the resource that's embedded in the
		/// calling assembly.
		/// </summary>
		/// <param name="filename">The filename to fetch the content of.</param>
		/// <returns>All the text stored in the specified embedded resource.</returns>
		public static string ReadAllText(string filename)
		{
			return ReadAllTextAsync(Assembly.GetCallingAssembly(), filename).Result;
		}
		/// <summary>
		/// Gets all the text stored in the resource that's embedded in the
		/// specified assembly.
		/// </summary>
		/// <param name="targetAssembly">The assembly from in which to look for the target embedded resource.</param>
		/// <param name="filename">The filename to fetch the content of.</param>
		/// <returns>All the text stored in the specified embedded resource.</returns>
		public static string ReadAllText(Assembly targetAssembly, string filename)
		{
			return ReadAllTextAsync(targetAssembly, filename).Result;
		}

		/// <summary>
		/// Gets all the text stored in the resource that's embedded in the 
		/// specified assembly asynchronously.
		/// </summary>
		/// <param name="filename">The filename to fetch the content of.</param>
		/// <returns>All the text stored in the specified embedded resource.</returns>
		public static async Task<string> ReadAllTextAsync(Assembly targetAssembly, string filename)
		{
			using (StreamReader resourceReader = new StreamReader(targetAssembly.GetManifestResourceStream(filename)))
			{
				return await resourceReader.ReadToEndAsync();
			}
		}
		/// <summary>
		/// Gets all the text stored in the resource that's embedded in the 
		/// calling assembly asynchronously.
		/// </summary>
		/// <param name="filename">The filename to fetch the content of.</param>
		/// <returns>All the text stored in the specified embedded resource.</returns>
		public static async Task<string> ReadAllTextAsync(string filename)
		{
			return await ReadAllTextAsync(Assembly.GetCallingAssembly(), filename);
		}


		/// <summary>
		/// Enumerates the lines of text in the embedded resource that's 
		/// embedded in the calling assembly.
		/// </summary>
		/// <param name="filename">The filename of the embedded resource to enumerate.</param>
		/// <returns>An IEnumerator that enumerates the specified embedded resource.</returns>
		public static IEnumerable<string> EnumerateLines(string filename)
		{
			return EnumerateLines(Assembly.GetCallingAssembly(), filename);
		}
		/// <summary>
		/// Enumerates the lines of text in the embedded resource that's 
		/// embedded in the specified assembly.
		/// </summary>
		/// <param name="filename">The filename of the embedded resource to enumerate.</param>
		/// <returns>An IEnumerator that enumerates the specified embedded resource.</returns>
		public static IEnumerable<string> EnumerateLines(Assembly targetAssembly, string filename)
		{
			foreach (Task<string> nextLine in EnumerateLinesAsync(targetAssembly, filename))
				yield return nextLine.Result;
		}

		/// <summary>
		/// Enumerates the lines of text in the resource that's embedded in the 
		/// specified assembly asynchronously.
		/// Each successive call returns a task that, when complete, returns 
		/// the next line of text stored in the embedded resource.
		/// </summary>
		/// <param name="targetAssembly">The target assembly in which to look for the embedded resource.</param>
		/// <param name="filename">The filename of the embedded resource to enumerate.</param>
		/// <returns>An IEnumerator that enumerates the specified embedded resource.</returns>
		public static IEnumerable<Task<string>> EnumerateLinesAsync(Assembly targetAssembly, string filename)
		{
			using (StreamReader resourceReader = new StreamReader(targetAssembly.GetManifestResourceStream(filename)))
			{
				while (!resourceReader.EndOfStream)
					yield return resourceReader.ReadLineAsync();
			}
		}
		/// <summary>
		/// Enumerates the lines of text in the resource that's embedded in the 
		/// calling assembly asynchronously.
		/// Each successive call returns a task that, when complete, returns 
		/// the next line of text stored in the embedded resource.
		/// </summary>
		/// <param name="filename">The filename of the embedded resource to enumerate.</param>
		/// <returns>An IEnumerator that enumerates the specified embedded resource.</returns>
		public static IEnumerable<Task<string>> EnumerateLinesAsync(string filename)
		{
			return EnumerateLinesAsync(Assembly.GetCallingAssembly(), filename);
		}

		/// <summary>
		/// Gets all the lines of text in the specified embedded resource.
		/// You might find EnumerateLines(string filename) more useful depending on your situation.
		/// </summary>
		/// <param name="filename">The filename to obtain the lines of text from.</param>
		/// <returns>A list of lines in the specified embedded resource.</returns>
		public static List<string> GetAllLines(string filename)
		{
			// Referencing the Result property will block until the async method completes
			return GetAllLinesAsync(filename).Result;
		}
		/// <summary>
		/// Gets all the lines of text in the resource that's embedded in the 
		/// calling assembly asynchronously.
		/// </summary>
		/// <param name="filename">The filename to obtain the lines of text from.</param>
		/// <returns>A list of lines in the specified embedded resource.</returns>
		public static async Task<List<string>> GetAllLinesAsync(string filename)
		{
			return await GetAllLinesAsync(Assembly.GetCallingAssembly(), filename);
		}
		/// <summary>
		/// Gets all the lines of text in the resource that's embedded in the 
		/// specified assembly asynchronously.
		/// </summary>
		/// <param name="filename">The filename to obtain the lines of text from.</param>
		/// <returns>A list of lines in the specified embedded resource.</returns>
		public static async Task<List<string>> GetAllLinesAsync(Assembly targetAssembly, string filename)
		{
			List<string> result = new List<string>();
			foreach (Task<string> nextLine in EnumerateLinesAsync(targetAssembly, filename))
				result.Add(await nextLine);
			return result;
		}
	}
}
