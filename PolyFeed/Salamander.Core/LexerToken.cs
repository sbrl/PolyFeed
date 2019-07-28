using System;
using System.Text.RegularExpressions;

namespace Salamander.Core.Lexer
{
	public class LexerToken<TokenType>
	{
		private int _lineNumber = -1, _columnNumber = -1;
		public int LineNumber {
			get => _lineNumber;
			set {
				if (_lineNumber != -1)
					throw new InvalidOperationException("Can't overwrite existing line number data");
				if (value < 0)
					throw new ArgumentException("Error: Negative line numbers don't make sense.");

				_lineNumber = value;
			}
		}
		public int ColumnNumber {
			get => _columnNumber;
			set {
				if(_columnNumber != -1)
					throw new InvalidOperationException("Can't overwrite existing column number data");
				if(value < 0)
					throw new ArgumentException("Error: Negative column numbers don't make sense.");

				_columnNumber = value;
			}
		}

		public readonly bool IsNullMatch = false;
		public readonly LexerRule<TokenType> Rule = null;
		public readonly Match RegexMatch;

		public TokenType Type {
			get {
				try
				{
					return Rule.Type;
				}
				catch (NullReferenceException)
				{
					return default(TokenType);
				}
			}
		}
		private string nullValueData;
		public string Value {
			get {
				return IsNullMatch ? nullValueData : RegexMatch.Value;
			}
		}

		public LexerToken(LexerRule<TokenType> inRule, Match inMatch)
		{
			Rule = inRule;
			RegexMatch = inMatch;
		}
		public LexerToken(string unknownData)
		{
			IsNullMatch = true;
			nullValueData = unknownData;
		}


		#region Overrides

		public override string ToString()
		{
			return $"[LexerToken @ {LineNumber}:{ColumnNumber} Type={Type}, Value={Value}]";
		}

		#endregion
	}
}
