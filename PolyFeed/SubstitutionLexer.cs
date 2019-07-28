using System;
using System.Collections.Generic;
using System.IO;
using Salamander.Core.Lexer;

namespace PolyFeed
{
	internal enum SubstitutionToken
	{
		Unknown = 0,

		Text,

		BraceOpen,
		BraceClose,
		Identifier

	}

	internal class SubstitutionLexer : Lexer<SubstitutionToken>
	{
		public SubstitutionLexer()
		{
			AddRules(new List<LexerRule<SubstitutionToken>>() {
				new LexerRule<SubstitutionToken>(SubstitutionToken.Text, @"[^{}]+"),
				new LexerRule<SubstitutionToken>(SubstitutionToken.Identifier, @"[^{}]+"),
				new LexerRule<SubstitutionToken>(SubstitutionToken.BraceOpen, @"\{"),
				new LexerRule<SubstitutionToken>(SubstitutionToken.BraceClose, @"\}"),
			});
		}

		public override void Initialise(StreamReader reader)
		{
			base.Initialise(reader);

			DisableRule(SubstitutionToken.Identifier);
		}
	}
}
