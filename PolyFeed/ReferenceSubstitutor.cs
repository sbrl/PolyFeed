using System;
using System.Text;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Salamander.Core.Lexer;

namespace PolyFeed
{
	internal static class ReferenceSubstitutor {
		private static LexerPool<SubstitutionLexer, SubstitutionToken> lexerPool = new LexerPool<SubstitutionLexer, SubstitutionToken>();

		public static string Replace(string inputString, HtmlNode rootElement)
		{
			StringBuilder result = new StringBuilder();
			SubstitutionLexer lexer = lexerPool.AcquireLexer();
			lexer.Initialise(inputString);

			foreach (LexerToken<SubstitutionToken> nextToken in lexer.TokenStream())
			{
				switch (nextToken.Type) {
					case SubstitutionToken.BraceOpen:
						lexer.SaveRuleStates();
						lexer.EnableRule(SubstitutionToken.Identifier);
						lexer.DisableRule(SubstitutionToken.Text);
						break;
					case SubstitutionToken.BraceClose:
						lexer.RestoreRuleStates();
						break;

					case SubstitutionToken.Text:
						result.Append(nextToken.Value);
						break;

					case SubstitutionToken.Identifier:
						result.Append(rootElement.QuerySelector(nextToken.Value));
						break;
				}
			}
			lexerPool.ReleaseLexer(lexer);

			return result.ToString();
		}
	}
}
