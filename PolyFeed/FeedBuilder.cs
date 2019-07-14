using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
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

			using StreamReader reader = new StreamReader(response.GetResponseStream());


		}
	}
}
