using System;
using System.Threading;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;

namespace PolyFeed.Helpers
{
	public static class HtmlHelpers
	{
		public static string QuerySelectorAttributeOrText(this HtmlNode htmlNode, SelectorSettings settings)
		{
			HtmlNode selectedNode = htmlNode.QuerySelector(settings.Selector);

			if (selectedNode == null)
				throw new ApplicationException($"Error: Selector {settings.Selector} failed to find any elements.");

			// Hack: Add physical newlines to <br />s to make date parsing easier for LastUpdated / Published.
			// Also means that we match Firefox functionality.
			foreach (HtmlNode nextNode in htmlNode.QuerySelectorAll("br")) {
				nextNode.InnerHtml = "\n";
			}

			if (string.IsNullOrWhiteSpace(settings.Attribute))
				return selectedNode.InnerText;

			return selectedNode.Attributes[settings.Attribute].Value;
		}
		public static string QuerySelectorAttributeOrHtml(this HtmlNode htmlNode, SelectorSettings settings)
		{
			HtmlNode selectedNode = htmlNode.QuerySelector(settings.Selector);

			if (selectedNode == null)
				throw new ApplicationException($"Error: Selector {settings.Selector} failed to find any elements.");

			if (string.IsNullOrWhiteSpace(settings.Attribute))
				return selectedNode.InnerHtml;

			return selectedNode.Attributes[settings.Attribute].Value;
		}

		/// <summary>
		/// Searches for and converts all the links that are children of the current 
		/// <see cref="HtmlNode" /> to absolute URIs.
		/// </summary>
		/// <param name="rootNode">The root node to search from.</param>
		/// <param name="baseUri">The base URI to use for conversion.</param>
		/// <returns>The number of nodes updated.</returns>
		public static int AbsolutifyUris(this HtmlNode rootNode, Uri baseUri)
		{
			int nodesUpdated = 0;
			Parallel.ForEach(rootNode.QuerySelectorAll("a, img"), (HtmlNode node) => {
				string attributeName = null;
				if (node.Attributes["href"] != null) attributeName = "href";
				if (node.Attributes["src"] != null) attributeName = "src";

				if (attributeName == null || node.Attributes[attributeName] == null)
					return;

				node.Attributes[attributeName].Value = new Uri(
					baseUri,
					node.Attributes[attributeName].Value
				).ToString();

				Interlocked.Increment(ref nodesUpdated);
			});
			return nodesUpdated;
		}
	}
}
