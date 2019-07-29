using System;
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
	}
}
