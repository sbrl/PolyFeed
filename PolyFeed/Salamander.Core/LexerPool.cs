using System;
using System.Collections.Generic;

namespace Salamander.Core.Lexer
{
	/// <summary>
	/// Represents a pool of reusable <see cref="Lexer{TokenType}"/>s.
	/// Useful to avoid memory churn when lexing lots of different input streams.
	/// </summary>
	public class LexerPool<T, E> where T : Lexer<E>, new()
	{
		private List<T> freeLexers = new List<T>();

		public LexerPool()
		{
		}

		public T AcquireLexer()
		{
			if (freeLexers.Count > 0)
			{
				T lexer = freeLexers[0];
				freeLexers.Remove(lexer);
				return lexer;
			}
			return new T();
		}

		public void ReleaseLexer(T lexer)
		{
			freeLexers.Add(lexer);
		}
	}
}
