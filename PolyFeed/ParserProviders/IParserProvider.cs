using System;
using System.Net;
using Microsoft.SyndicationFeed.Atom;

namespace PolyFeed.ParserProviders
{
	/// <summary>
	/// Defines the functionality that a source parser should provide.
	/// Sources are represented by a <see cref="FeedSource" /> object, and source parsers 
	/// are responsible for parsing it and populating a given atom feed.
	/// </summary>
	public interface IParserProvider
	{
		/// <summary>
		/// Sets the output feed that parsed output should be written to.
		/// </summary>
		/// <param name="feed">The output feed writer that output should be written to.</param>
		void SetOutputFeed(AtomFeedWriter feed);
		/// <summary>
		/// Parses a web response that's paired with a given <see cref="FeedSource" />.
		/// </summary>
		/// <param name="source">The <see cref="FeedSource"/> object that the <paramref name="response"/> was generated from.</param>
		/// <param name="response">The <see cref="WebResponse"/> in question needs parsing.</param>
		void ParseWebResponse(FeedSource source, WebResponse response);
	}
}
