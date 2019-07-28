using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Salamander.Core.Helpers;

namespace Salamander.Core.Lexer
{
	public class Lexer<TokenType>
	{
		/// <summary>
		/// The rules that should be used during the lexing process.
		/// </summary>
		public List<LexerRule<TokenType>> Rules { get; private set; } = new List<LexerRule<TokenType>>();
		/// <summary>
		/// Tokens in this list will be matched against, but not emitted by the lexer
		/// into the main token stream.
		/// Useful for catching and disposing of sequences of characters you don't want escaping
		/// or breaking your parser.
		/// </summary>
		public List<TokenType> IgnoreTokens { get; private set; } = new List<TokenType>();

		/// <summary>
		/// Whether the lexer should be verbose and log a bunch of debugging information 
		/// to the console.
		/// </summary>
		public bool Verbose { get; set; } = false;

		/// <summary>
		/// The number of the line that currently being scanned.
		/// </summary>
		public int CurrentLineNumber { get; private set; } = 0;
		/// <summary>
		/// The number of characters on the current line that have been scanned.
		/// </summary>
		/// <value>The current line position.</value>
		public int CurrentLinePos { get; private set; } = 0;
		/// <summary>
		/// The total number of characters currently scanned by this lexer instance.
		/// Only updated every newline!
		/// </summary>
		public int TotalCharsScanned { get; private set; } = 0;

		/// <summary>
		/// The internal stream that we should read from when lexing.
		/// </summary>
		private StreamReader textStream;

		/// <summary>
		/// A stack of rule states.
		/// Whether rules are enabled or disabled can be recursively saved and restored - 
		/// this <see cref="Stack{T}" /> is how the lexer saves this information.
		/// </summary>
		private Stack<Dictionary<LexerRule<TokenType>, bool>> EnabledStateStack = new Stack<Dictionary<LexerRule<TokenType>, bool>>();

		/// <summary>
		/// Creates a new <see cref="Lexer{TokenType}" />, optionally containing the given 
		/// <see cref="LexerRule{TokenType}" /> instances.
		/// </summary>
		/// <param name="initialRules">The rules to add to the new <see cref="Lexer{TokenType}" />.</param>
		public Lexer(params LexerRule<TokenType>[] initialRules)
		{
			AddRules(initialRules);
		}

		/// <summary>
		/// Adds a single lexing rule to the <see cref="Lexer{TokenType}" />.
		/// </summary>
		/// <param name="newRule">The rule to add.</param>
		public void AddRule(LexerRule<TokenType> newRule)
			=> Rules.Add(newRule);
		/// <summary>
		/// Adds a bunch of lexing rules to the <see cref="Lexer{TokenType}" />.
		/// </summary>
		/// <param name="newRules">The rules to add.</param>
		public void AddRules(IEnumerable<LexerRule<TokenType>> newRules)
			=> Rules.AddRange(newRules);

		/// <summary>
		/// Reinitialises the parser with a new input stream.
		/// </summary>
		/// <remarks>
		/// Child classes should override this method to do their own state initialisation,
		/// as lexers MAY be re-used on multiple input streams.
		/// Implementors must be careful not to forget to call this base method though.
		/// </remarks>
		/// <param name="reader">The <see cref="StreamReader"/> to use as the new input stream..</param>
		public virtual void Initialise(StreamReader reader)
		{
			// Reset the counters
			CurrentLineNumber = 0;
			CurrentLinePos = 0;
			TotalCharsScanned = 0;

			// Reset the state stack
			EnabledStateStack.Clear();

			// Re-enable all rules
			EnableAllRules();

			textStream = reader;
		}
		public void Initialise(string input)
		{
			MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
			Initialise(new StreamReader(stream));
		}

		/// <summary>
		/// Performs the lexing process itself in an incremental manner.
		/// Note that a single Lexer may only do a single lex at a time - even if it's the 
		/// same document multiple times over.
		/// </summary>
		/// <returns>A stream of lexical tokens.</returns>
		public IEnumerable<LexerToken<TokenType>> TokenStream()
		{
			string nextLine;
			List<LexerToken<TokenType>> matches = new List<LexerToken<TokenType>>();
			while ((nextLine = textStream.ReadLine()) != null)
			{
				CurrentLinePos = 0;

				while (CurrentLinePos < nextLine.Length)
				{
					matches.Clear();
					foreach (LexerRule<TokenType> rule in Rules)
					{
						if (!rule.Enabled) continue;

						Match nextMatch = rule.RegEx.Match(nextLine, CurrentLinePos);
						if (!nextMatch.Success) continue;

						matches.Add(
							new LexerToken<TokenType>(rule, nextMatch)
							{
								LineNumber = CurrentLineNumber,
								ColumnNumber = nextMatch.Index
							}
						);
					}

					if (matches.Count == 0)
					{
						string unknownTokenContent = nextLine.Substring(CurrentLinePos);
						if (Verbose) Console.WriteLine($"{Ansi.FRed}[Unknown Token: No matches found for this line]{Ansi.Reset} {0}", unknownTokenContent);
						yield return new LexerToken<TokenType>(unknownTokenContent)
						{
							LineNumber = CurrentLineNumber,
							ColumnNumber = CurrentLinePos
						};
						break;
					}

					matches.Sort((LexerToken<TokenType> a, LexerToken<TokenType> b) => {
						// Match of offset position position
						int result = a.ColumnNumber - b.ColumnNumber;
						// If they both start at the same position, then go with highest priority one
						if (result == 0)
							result = b.Rule.Priority - a.Rule.Priority;
						// Failing that, try the longest one
						if (result == 0)
							result = b.RegexMatch.Length - a.RegexMatch.Length;

						return result;
					});
					LexerToken<TokenType> selectedToken = matches[0];
					int selectedTokenOffset = nextLine.IndexOf(selectedToken.RegexMatch.Value, CurrentLinePos) - CurrentLinePos;

					if (selectedTokenOffset > 0)
					{
						string extraTokenContent = nextLine.Substring(CurrentLinePos, selectedTokenOffset);
						int unmatchedLinePos = CurrentLinePos;
						CurrentLinePos += selectedTokenOffset;
						if (Verbose) Console.WriteLine($"{Ansi.FRed}[Unmatched content]{Ansi.Reset} '{extraTokenContent}'");
						// Return the an unknown token, but only if we're not meant to be ignoring them
						if (!IgnoreTokens.Contains((TokenType)Enum.ToObject(typeof(TokenType), 0)))
						{
							yield return new LexerToken<TokenType>(extraTokenContent)
							{
								LineNumber = CurrentLineNumber,
								ColumnNumber = unmatchedLinePos
							};
						}
					}

					CurrentLinePos += selectedToken.RegexMatch.Length;
					if (Verbose) Console.WriteLine($"{(IgnoreTokens.Contains(selectedToken.Type) ? Ansi.FBlack : Ansi.FGreen)}{selectedToken}{Ansi.Reset}");

					// Yield the token, but only if we aren't supposed to be ignoring it
					if (IgnoreTokens.Contains(selectedToken.Type))
						continue;
					yield return selectedToken;
				}

				if (Verbose) Console.WriteLine($"{Ansi.FBlue}[Lexer]{Ansi.Reset} Next line");
				CurrentLineNumber++;
				TotalCharsScanned += CurrentLinePos;
			}
		}


		#region Rule Management

		/// <summary>
		/// Enables all <see cref="LexerRule{TokenType}" />s currently registered against
		/// this Lexer.
		/// </summary>
		public void EnableAllRules() => EnableRulesByPrefix("");
		/// <summary>
		/// Disables all <see cref="LexerRule{TokenType}" />s currently registered against
		/// this Lexer.
		/// </summary>
		public void DisableAllRules() => DisableRulesByPrefix("");

		/// <summary>
		/// Enables the rule that matches against the given <see cref="TokenType" />.
		/// </summary>
		/// <param name="type">The token type to use to find the rule to enable.</param>
		public void EnableRule(TokenType type) => SetRule(type, true);
		/// <summary>
		/// Disables the rule that matches against the given <see cref="TokenType" />.
		/// </summary>
		/// <param name="type">The token type to use to find the rule to disable.</param>
		public void DisableRule(TokenType type) => SetRule(type, false);

		/// <summary>
		/// Sets the enabled status of the rule that matches against the given 
		/// <see cref="TokenType" /> to the given state.
		/// </summary>
		/// <param name="type">The <see cref="TokenType" /> to use to find the rule to 
		/// sets the enabled state of.</param>
		/// <param name="state">Whether to enable or disable the rule. <see langword="true"/> = enable it, <see langword="false"/> = disable it.</param>
		public void SetRule(TokenType type, bool state)
		{
			foreach (LexerRule<TokenType> rule in Rules)
			{
				// We have to do a string comparison here because of the generic type we're using in multiple nested
				// classes
				if (Enum.GetName(rule.Type.GetType(), rule.Type) == Enum.GetName(type.GetType(), type))
				{
					rule.Enabled = state;
					return;
				}
			}
		}

		/// <summary>
		/// Toggles the enabled status of multiple rules by finding rules that generate 
		/// tokens whose name begins with a specific substring.
		/// </summary>
		/// <param name="tokenTypePrefix">The prefix to use when finding rules to toggle.</param>
		public void ToggleRulesByPrefix(string tokenTypePrefix)
		{
			foreach (LexerRule<TokenType> rule in Rules)
			{
				// We have to do a string comparison here because of the generic type we're using in multiple nested
				// classes
				if (Enum.GetName(rule.Type.GetType(), rule.Type).StartsWith(tokenTypePrefix, StringComparison.CurrentCulture))
					rule.Enabled = !rule.Enabled;
			}
		}
		/// <summary>
		/// Enables multiple rules by finding rules that generate 
		/// tokens whose name begins with a specific substring.
		/// </summary>
		/// <param name="tokenTypePrefix">The prefix to use when finding rules to enable.</param>
		public void EnableRulesByPrefix(string tokenTypePrefix)
			=> SetRulesByPrefix(tokenTypePrefix, true);
		/// <summary>
		/// Disables multiple rules by finding rules that generate 
		/// tokens whose name begins with a specific substring.
		/// </summary>
		/// <param name="tokenTypePrefix">The prefix to use when finding rules to disable.</param>
		public void DisableRulesByPrefix(string tokenTypePrefix)
			=> SetRulesByPrefix(tokenTypePrefix, false);

		/// <summary>
		/// Set the enabled status of multiple rules by finding rules that generate 
		/// tokens whose name begins with a specific substring.
		/// </summary>
		/// <param name="tokenTypePrefix">The prefix to use when finding rules to set the 
		/// status of.</param>
		public void SetRulesByPrefix(string tokenTypePrefix, bool state)
		{
			foreach (LexerRule<TokenType> rule in Rules)
			{
				// We have to do a string comparison here because of the generic type we're using in multiple nested
				// classes
				if (Enum.GetName(rule.Type.GetType(), rule.Type).StartsWith(tokenTypePrefix, StringComparison.CurrentCulture))
				{
					//if(Verbose) Console.WriteLine($"{Ansi.FBlue}[Lexer/Rules] {Ansi.FCyan}Setting {rule.Type} to {state}");
					rule.Enabled = state;
				}
			}
		}

		/// <summary>
		/// Saves the current rule states (i.e. whether they are enabled or not) as a snapshot to an
		/// internal stack.
		/// </summary>
		public void SaveRuleStates()
		{
			Dictionary<LexerRule<TokenType>, bool> states = new Dictionary<LexerRule<TokenType>, bool>();
			foreach (LexerRule<TokenType> nextRule in Rules)
				states[nextRule] = nextRule.Enabled;

			EnabledStateStack.Push(states);
		}
		/// <summary>
		/// Restores the top-most rule states snapshot from the internal stack.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if there aren't any states left on the stack to restore.</exception>
		public void RestoreRuleStates()
		{
			if (EnabledStateStack.Count < 1)
				throw new InvalidOperationException("Error: Can't restore the lexer rule states when no states have been saved!");

			Dictionary<LexerRule<TokenType>, bool> states = EnabledStateStack.Pop();
			foreach (KeyValuePair<LexerRule<TokenType>, bool> nextRulePair in states)
				nextRulePair.Key.Enabled = nextRulePair.Value;
		}


		#endregion

	}
}
