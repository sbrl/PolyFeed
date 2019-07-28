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

namespace PolyFeed
{
	public class FeedBuilder
	{
		StringBuilder result = new StringBuilder();
		XmlWriter xml = null;
		AtomFeedWriter feed = null;

		public FeedBuilder() {
			xml = XmlWriter.Create(result);
			feed = new AtomFeedWriter(xml);
		}

		public async Task AddSource(FeedSource source) {
			WebResponse response = await WebRequest.Create(source.Url).GetResponseAsync();



			// Write the header
			await feed.WriteGenerator("Polyfeed", "https://gitlab.com/sbrl/PolyFeed.git", Program.GetProgramVersion());
			await feed.WriteId(source.Url);
			string lastModified = response.Headers.Get("last-modified");
			if (string.IsNullOrWhiteSpace(lastModified))
				await feed.WriteUpdated(DateTimeOffset.Now);
			else
				await feed.WriteUpdated(DateTimeOffset.Parse(lastModified));

			string contentType = response.Headers.Get("content-type");

			switch (source.SourceType) {
				case SourceType.HTML:
					await AddSourceHtml(source, response);
					break;
				default:
					throw new NotImplementedException($"Error: The source type {source.SourceType} hasn't been implemented yet.");
			}
		}

		private async Task AddSourceHtml(FeedSource source, WebResponse response) {
			HtmlDocument html = new HtmlDocument();
			using (StreamReader reader = new StreamReader(response.GetResponseStream()))
				html.LoadHtml(await reader.ReadToEndAsync());

			HtmlNode document = html.DocumentNode;

			await feed.WriteTitle(ReferenceSubstitutor.Replace(source.Title, document));
			await feed.WriteSubtitle(ReferenceSubstitutor.Replace(source.Subtitle, document));

			foreach (HtmlNode nextNode in document.QuerySelectorAll(source.EntrySelector)) {
				HtmlNode urlNode = nextNode.QuerySelector(source.EntryUrlSelector);
				string url = source.EntryUrlAttribute == string.Empty ? 
					urlNode.InnerText : urlNode.Attributes[source.EntryUrlAttribute].DeEntitizeValue;


				SyndicationItem nextItem = new SyndicationItem() {
					Id = url,
					Title = ReferenceSubstitutor.Replace(source.EntryTitle, nextNode),
					Description = ReferenceSubstitutor.Replace(source.EntryContent, nextNode)
				};

				if (source.EntryPublishedSelector != string.Empty) {
					HtmlNode publishedNode = nextNode.QuerySelector(source.EntryPublishedSelector);
					nextItem.Published = DateTime.Parse(
						source.EntryPublishedAttribute == string.Empty
							? publishedNode.InnerText
							: publishedNode.Attributes[source.EntryPublishedAttribute].DeEntitizeValue
					);

				}
				if (source.EntryPublishedSelector != string.Empty) {
					HtmlNode lastUpdatedNode = nextNode.QuerySelector(source.EntryLastUpdatedSelector);
					nextItem.Published = DateTime.Parse(
						source.EntryLastUpdatedAttribute == string.Empty
							? lastUpdatedNode.InnerText
							: lastUpdatedNode.Attributes[source.EntryLastUpdatedAttribute].DeEntitizeValue
					);
				}
			}
		}
	}
}
