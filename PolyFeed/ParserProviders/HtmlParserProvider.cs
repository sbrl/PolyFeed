using System;
using System.Net;
using Microsoft.SyndicationFeed.Atom;

namespace PolyFeed.ParserProviders
{
	public class HtmlParserProvider : IParserProvider
	{
		public HtmlParserProvider()
		{
		}

		public void ParseWebResponse(FeedSource source, WebResponse response)
		{
			throw new NotImplementedException();
		}

		public void SetOutputFeed(AtomFeedWriter feed)
		{
			throw new NotImplementedException();
		}
	}
}
