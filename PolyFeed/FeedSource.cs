using System;

namespace PolyFeed
{
	public enum SourceType { HTML, XML, JSON };

	public class FeedSource
	{
		/// <summary>
		/// The url of the source document to parse.
		/// </summary>
		/// <value>The URL.</value>
		public string Url { get; set; }

		/// <summary>
		/// The type of source document to expect.
		/// </summary>
		public SourceType SourceType { get; set; }

		/// <summary>
		/// The title of the feed.
		/// Supports the same {} syntax as <see cref="EntryTitle" />.
		/// </summary>
		public string Title { get; set; }
		/// <summary>
		/// The subtitle of the feed.
		/// Supports the same {} syntax as <see cref="EntryTitle" />.
		/// </summary>
		/// <value>The subtitle.</value>
		public string Subtitle { get; set; }


		#region Entries

		/// <summary>
		/// A selector that matches against an element that contains the URL that an
		/// entry should link to.
		/// Relative to the element selected by <see cref="EntrySelector" />.
		/// </summary>
		public string EntryUrlSelector { get; set; }
		/// <summary>
		/// The name of the attribute on the element selected by <see cref="EntryUrlSelector" />.
		/// Set to an empty string to select the content of the element instead of the 
		/// content of an attribute.
		/// </summary>
		public string EntryUrlAttribute { get; set; } = "";

		/// <summary>
		/// The selector that specifies the location of nodes in the object model that 
		/// should be added to the feed.
		/// The format varies depending on the <see cref="SourceType" />.
		///  - HTML: CSS selector (e.g. main > article)
		///  - XML: XPath (e.g. //element_name)
		///  - JSON: Dotted object  (e.g. items.fruit)
		/// </summary>
		public string EntrySelector { get; set; }
		/// <summary>
		/// The title of an entry.
		/// Selectors may be included in curly braces {} to substitute in content.
		/// Such selectors are relative to the current feed entry.
		/// The format varies in the samem  way as <see cref="EntrySelector" /> does.
		/// </summary>
		public string EntryTitle { get; set; }
		/// <summary>
		/// Same as <see cref="EntryTitle" />, but for the body of an entry. HTML is allowed.
		/// </summary>
		public string EntryContent { get; set; }

		/// <summary>
		/// The selector for the node that contains the date published for an entry.
		/// </summary>
		public string EntryPublishedSelector { get; set; }

		/// <summary>
		/// The name of the attribute that contains the date published for an entry.
		/// Set to <see cref="string.Empty" /> to use the content of the node itself.
		/// </summary>
		public string EntryPublishedAttribute { get; set; }

		/// <summary>
		/// Same as <see cref="EntryPublishedSelector" />, but for the last updated.
		/// If not specified, the last updated will be omitted.
		/// </summary>
		public string EntryLastUpdatedSelector { get; set; }
		/// <summary>
		/// Same as <see cref="EntryPublishedAttribute" />.
		/// </summary>
		public string EntryLastUpdatedAttribute { get; set; }

		#endregion

	}
}
