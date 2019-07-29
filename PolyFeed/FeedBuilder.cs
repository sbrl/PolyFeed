using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Atom;
using PolyFeed.Helpers;

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
			WebResponse response = await WebRequest.Create(source.Feed.Url).GetResponseAsync();

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

			switch (source.Feed.Type) {
				case SourceType.HTML:
					await AddSourceHtml(source, response);
					break;
				default:
					throw new NotImplementedException($"Error: The source type {source.Feed.Type} hasn't been implemented yet.");
			}

			await Console.Error.WriteLineAsync("[Builder] Done!");
		}

		private async Task AddSourceHtml(FeedSource source, WebResponse response) {
			await Console.Error.WriteLineAsync("[Builder/Html] Parsing Html");

			// Parse the HTML
			HtmlDocument html = new HtmlDocument();
			using (StreamReader reader = new StreamReader(response.GetResponseStream()))
				html.LoadHtml(await reader.ReadToEndAsync());

			HtmlNode document = html.DocumentNode;

			await Console.Error.WriteLineAsync("[Builder/Html] Generating feed content");

			// Add the title
			await feed.WriteTitle(ReferenceSubstitutor.Replace(source.Feed.Title, document));
			await feed.WriteSubtitle(ReferenceSubstitutor.Replace(source.Feed.Subtitle, document));

			// Add the logo
			if (source.Feed.Logo != null) {
				HtmlNode logoNode = document.QuerySelector(source.Feed.Logo.Selector);
				xml.WriteElementString("logo", logoNode.Attributes[source.Feed.Logo.Attribute].Value);
			}

			// Add the feed entries
			foreach (HtmlNode nextNode in document.QuerySelectorAll(source.Entries.Selector)) {
				HtmlNode urlNode = nextNode.QuerySelector(source.Entries.Url.Selector);
				if (urlNode == null)
					throw new ApplicationException("Error: Failed to match entry url selector against an element.");

				string url = source.Entries.Url.Attribute == string.Empty ?
					urlNode.InnerText : urlNode.Attributes[source.Entries.Url.Attribute].DeEntitizeValue;

				Uri entryUri = new Uri(new Uri(source.Feed.Url), new Uri(url));
				AtomEntry nextItem = new AtomEntry() {
					Id = entryUri.ToString(),
					Title = ReferenceSubstitutor.Replace(source.Entries.Title, nextNode),
					Description = ReferenceSubstitutor.Replace(source.Entries.Content, nextNode),
					ContentType = "html"
				};
				nextItem.AddLink(new SyndicationLink(entryUri, AtomLinkTypes.Alternate));

				if (source.Entries.Published != null) {
					nextItem.Published = DateTime.Parse(
						nextNode.QuerySelectorAttributeOrText(
							source.Entries.Published
						)
					);

				}
				if (source.Entries.Published != null) {
					nextItem.LastUpdated = DateTime.Parse(
						nextNode.QuerySelectorAttributeOrText(
							source.Entries.LastUpdated
						)
					);
				}
				else // It requires one, apparently
					nextItem.LastUpdated = DateTimeOffset.Now;

				SyndicationPerson author = new SyndicationPerson(
					nextNode.QuerySelectorAttributeOrText(source.Entries.AuthorName).Trim(),
					""
				);
				if(source.Entries.AuthorUrl != null)
					author.Uri = nextNode.QuerySelectorAttributeOrText(source.Entries.AuthorUrl);

				nextItem.AddContributor(author);

				await feed.Write(nextItem);

			}
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
