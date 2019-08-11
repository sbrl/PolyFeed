using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Atom;
using PolyFeed.Helpers;

namespace PolyFeed.ParserProviders
{
	public class HtmlParserProvider : IParserProvider
	{
		private XmlWriter xml = null;
		private AtomFeedWriter feed = null;

		public string Identifier => "html";

		public HtmlParserProvider()
		{
		}

		public void SetOutputFeed(AtomFeedWriter inFeed, XmlWriter inXml) {
			xml = inXml;
			feed = inFeed;
		}

		public async Task ParseWebResponse(FeedSource source, WebResponse response)
		{
			await Console.Error.WriteLineAsync("[Builder/Html] Parsing Html");

			// Parse the HTML
			HtmlDocument html = new HtmlDocument();
			using (StreamReader reader = new StreamReader(response.GetResponseStream()))
				html.LoadHtml(await reader.ReadToEndAsync());

			HtmlNode document = html.DocumentNode;

			document.AbsolutifyUris(new Uri(source.Feed.Url));


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
			foreach (HtmlNode nextNode in document.QuerySelectorAll(source.Entries.Selector))
			{
				await addEntry(source, nextNode);
			}
		}

		private async Task addEntry(FeedSource source, HtmlNode nextNode)
		{
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

			if (source.Entries.LastUpdated != null) {
				nextItem.LastUpdated = DateTime.Parse(
					nextNode.QuerySelectorAttributeOrText(
						source.Entries.LastUpdated
					)
				);
			}
			else if (source.Entries.Published != null) // Use the publish date if available
				nextItem.LastUpdated = nextItem.Published;
			else // It requires one, apparently
				nextItem.LastUpdated = DateTimeOffset.Now;


			if (source.Entries.AuthorName != null) {
				SyndicationPerson author = new SyndicationPerson(
					nextNode.QuerySelectorAttributeOrText(source.Entries.AuthorName).Trim(),
					""
				);
				if (source.Entries.AuthorUrl != null)
					author.Uri = nextNode.QuerySelectorAttributeOrText(source.Entries.AuthorUrl);

				nextItem.AddContributor(author);
			}
			else
				nextItem.AddContributor(new SyndicationPerson("Unknown", ""));


			await feed.Write(nextItem);
		}
	}
}
