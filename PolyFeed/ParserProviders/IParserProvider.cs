using System;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
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
		/// The identifier of this provider.
		/// Used in the .toml configuration file to specify which parser to use.
		/// </summary>
		string Identifier { get; }

		/// <summary>
		/// Sets the output feed that parsed output should be written to.
		/// </summary>
		/// <param name="feed">The output feed writer that output should be written to.</param>
		/// <param name="xml">The underlying XML feed try not to use this unless you *really* have to.</param>
		void SetOutputFeed(AtomFeedWriter feed, XmlWriter xml);
		/// <summary>
		/// Parses a web response that's paired with a given <see cref="FeedSource" />.
		/// </summary>
		/// <param name="source">The <see cref="FeedSource"/> object that the <paramref name="response"/> was generated from.</param>
		/// <param name="response">The <see cref="WebResponse"/> in question needs parsing.</param>
		Task ParseWebResponse(FeedSource source, WebResponse response);
	}
}
