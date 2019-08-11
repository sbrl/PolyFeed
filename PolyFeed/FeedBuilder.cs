using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Atom;
using PolyFeed.Helpers;
using PolyFeed.ParserProviders;

namespace PolyFeed
{
	public class FeedBuilder
	{
		MemoryStream stream = new MemoryStream();
		XmlWriter xml = null;
		AtomFeedWriter feed = null;

		public FeedBuilder() {
			xml = XmlWriter.Create(stream, new XmlWriterSettings() {
				Indent = true,
				Encoding = new UTF8Encoding(false),
				WriteEndDocumentOnClose = true
			});
			feed = new AtomFeedWriter(xml, null, new AtomFormatter() { UseCDATA = true });
		}

		public async Task AddSource(FeedSource source) {
			await Console.Error.WriteLineAsync("[Builder] Downloading content");
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(source.Feed.Url);

			request.UserAgent = UserAgentHelper.UserAgent;
			WebResponse response = await request.GetResponseAsync();

			await Console.Error.WriteLineAsync("[Builder] Generating feed header");

			// Write the header
			await feed.WriteGenerator("Polyfeed", "https://github.com/sbrl/PolyFeed.git", Program.GetProgramVersion());
			await feed.WriteId(source.Feed.Url);
			await feed.Write(new SyndicationLink(new Uri(source.Feed.Url), AtomLinkTypes.Self));
			string lastModified = response.Headers.Get("last-modified");
			if (string.IsNullOrWhiteSpace(lastModified))
				await feed.WriteUpdated(DateTimeOffset.Now);
			else
				await feed.WriteUpdated(DateTimeOffset.Parse(lastModified));

			string contentType = response.Headers.Get("content-type");

			IParserProvider provider = GetProvider(source.Feed.SourceType);
			if(provider == null)
				throw new ApplicationException($"Error: A provider for the source type {source.Feed.SourceType} wasn't found.");

			provider.SetOutputFeed(feed, xml);
			await provider.ParseWebResponse(source, response);

			await Console.Error.WriteLineAsync("[Builder] Done!");
		}

		private IParserProvider GetProvider(string identifier)
		{
			IEnumerable<Type> possibleTypes = ReflectionUtilities.IterateImplementingTypes(
				typeof(IParserProvider),
				Assembly.GetExecutingAssembly()
			);

			foreach (Type next in possibleTypes) {
				IParserProvider candidate = (IParserProvider)Activator.CreateInstance(next);
				if (candidate.Identifier == identifier)
					return candidate;
			}

			return null;
		}

		public async Task<string> Render()
		{
			await feed.Flush();
			xml.WriteEndDocument();
			xml.Flush();
			xml.Close();
			return Encoding.UTF8.GetString(stream.ToArray());
		}
	}
}
