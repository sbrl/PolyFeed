using System;

namespace PolyFeed
{
	public enum SourceType { HTML, XML, JSON };

	public class SelectorSettings
	{
		/// <summary>
		/// A selector that matches against an element to select.
		/// </summary>
		public string Selector { get; set; }
		/// <summary>
		/// The name of the attribute to get the value of.
		/// Set to an empty string to select the content of the element instead of the 
		/// content of an attribute.
		/// </summary>
		public string Attribute { get; set; }

		public override string ToString()
		{
			return $"[SelectorSettings Selector = {Selector}, Attribute = {Attribute}]";
		}
	}

	public class FeedSettings
	{
		public string Output { get; set; }

		/// <summary>
		/// The url of the source document to parse.
		/// </summary>
		/// <value>The URL.</value>
		public string Url { get; set; }

		/// <summary>
		/// The type of source document to expect.
		/// </summary>
		public string SourceType { get; set; }


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

		/// <summary>
		/// Selector that matches against the feed logo url.
		/// </summary>
		public SelectorSettings Logo { get; set; }
	}

	public class EntrySettings
	{
		/// <summary>
		/// The selector that specifies the location of nodes in the object model that 
		/// should be added to the feed.
		/// The format varies depending on the <see cref="SourceType" />.
		///  - HTML: CSS selector (e.g. main > article)
		///  - XML: XPath (e.g. //element_name)
		///  - JSON: Dotted object  (e.g. items.fruit)
		/// </summary>
		public string Selector { get; set; }
		/// <summary>
		/// Selector settings to get the URL that an entry should link to.
		/// </summary>
		public SelectorSettings Url { get; set; } = new SelectorSettings() { Attribute = "href" };

		/// <summary>
		/// The title of an entry.
		/// Selectors may be included in curly braces {} to substitute in content.
		/// Such selectors are relative to the current feed entry.
		/// The format varies in the same way as <see cref="Selector" /> does.
		/// </summary>
		public string Title { get; set; }
		/// <summary>
		/// Same as <see cref="Title" />, but for the body of an entry. HTML is allowed.
		/// </summary>
		public string Content { get; set; }

		/// <summary>
		/// The selector for the date published for an entry.
		/// </summary>
		public SelectorSettings Published { get; set; }

		/// <summary>
		/// The selector for the date published for an entry.
		/// </summary>
		public SelectorSettings LastUpdated { get; set; }

		/// <summary>
		/// The selector for the name of the author of an entry.
		/// </summary>
		public SelectorSettings AuthorName { get; set; }
		/// <summary>
		/// The selector for the url that points to a page that represents 
		/// the author of an entry.
		/// </summary>
		public SelectorSettings AuthorUrl { get; set; }

	}

	public class FeedSource
	{
		public FeedSettings Feed { get; set; }
		public EntrySettings Entries { get; set; }
	}
}
