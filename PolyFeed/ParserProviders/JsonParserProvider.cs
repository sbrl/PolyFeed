using System;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.SyndicationFeed.Atom;
using System.Json;
using System.IO;

namespace PolyFeed.ParserProviders
{
	public class JsonParserProvider : IParserProvider
	{
		private XmlWriter xml = null;
		private AtomFeedWriter feed = null;

		public string Identifier => "json";

		public JsonParserProvider() {

		}

		public void SetOutputFeed(AtomFeedWriter inFeed, XmlWriter inXml) {
			xml = inXml;
			feed = inFeed;
		}

		public async Task ParseWebResponse(FeedSource source, WebResponse response)
		{
			string jsonText;
			using (StreamReader reader = new StreamReader(response.GetResponseStream()))
				jsonText = await reader.ReadToEndAsync();

			JsonValue jsonValue = JsonValue.Parse(jsonText);
			JsonObject json = jsonValue as JsonObject;
			if (json == null)
				throw new ApplicationException("Error: Failed to parse the JSON into an object.");


			throw new NotImplementedException("Error 501: Not implemented :-/");


		}

	}
}
