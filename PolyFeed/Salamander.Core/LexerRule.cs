using System;
using System.Text.RegularExpressions;

namespace Salamander.Core.Lexer
{
	public class LexerRule<TokenType>
	{
		/// <summary>
		/// The token type that a match against this rule should generate.
		/// </summary>
		public readonly TokenType Type;
		/// <summary>
		/// The regular expression to use to find matches.
		/// </summary>
		public readonly Regex RegEx;
		/// <summary>
		/// The priority of this rule.
		/// </summary>
		/// <remarks>
		/// If there are multiple matches, then the one with the highest priority will be matched 
		/// against first.
		/// Failing that, the longest match will be taken first.
		/// Note that even if a match has a higher priority, a match from a lower priority rule 
		/// will be used instead if it occurs earlier in the source, as this will result in fewer 
		/// unmatched characters.
		/// </remarks>
		public int Priority { get; set; } = 0;
		/// <summary>
		/// Whether this rule is currently enabled or not. This can be changed on-the-fly whilst lexing.
		/// Sometimes useful when handling more complicated logic.
		/// Be careful though, as if you start needing this, perhaps you should evaluate whether 
		/// utilising the fuller capabilities of the parser would be more appropriate instead.
		/// </summary>
		public bool Enabled { get; set; } = true;

		public LexerRule(TokenType inName, string inRegEx, RegexOptions inRegexOptions = RegexOptions.None, int inPriority = 0)
		{
			if (!typeof(TokenType).IsEnum)
				throw new ArgumentException($"Error: inName must be an enum - {typeof(TokenType)} passed");

			Type = inName;
			RegEx = new Regex(inRegEx, inRegexOptions | RegexOptions.Compiled);
			Priority = inPriority;
		}

		public bool Toggle()
		{
			Enabled = !Enabled;
			return Enabled;
		}
	}
}
