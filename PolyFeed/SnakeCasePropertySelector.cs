using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Nett;

namespace PolyFeed
{
	public class SnakeCasePropertySelector : ITargetPropertySelector
	{
		public SnakeCasePropertySelector()
		{
		}

		public PropertyInfo TryGetTargetProperty(string key, Type target)
		{
			string transformedKey = Regex.Replace(
				key,
				@"(^|_)[A-Za-z0-9]",
				(match) => match.Value.Replace("_", "").ToUpper()
			);

			//Console.WriteLine($"{key} -> {transformedKey}");

			return target.GetProperty(transformedKey);
		}
	}
}
