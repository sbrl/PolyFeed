using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;

namespace PolyFeed.Helpers
{
	internal static class ReflectionUtilities
	{
		public static bool Verbose = true;

		public static IEnumerable<Assembly> IterateLoadedAssemblies()
		{
			return AppDomain.CurrentDomain.GetAssemblies();
		}

		public static IEnumerable<Type> IterateAllLoadedTypes()
		{
			return IterateLoadedAssemblies()
				.SelectMany((Assembly nextAssembly) => nextAssembly.GetTypes());
		}

		public static IEnumerable<Type> IterateLoadedTypes(Assembly targetAssembly)
		{
			return targetAssembly.GetTypes();
		}

		public static void LoadAssembliesDirectory(string directory)
		{
			foreach (string nextDll in Directory.EnumerateFiles(directory, "*.dll"))
			{
				AssemblyName assemblyName = AssemblyName.GetAssemblyName(nextDll);

				// Check that a strongname is actually present
				if (assemblyName.GetPublicKeyToken().Length < 8)
					continue;

				// Verify the strongname is valid.
				/*try
				{
					bool isOk = false;
					NativeMethods.StrongNameSignatureVerificationEx(nextDll, 0xFF, ref isOk);
					if (!isOk)
						continue;
				}
				catch (DllNotFoundException error)
				{
					Console.Error.WriteLine($"Warning: Unable to verify the integrity of the StrongName for '{nextDll}' (DLL not found: {error.Message}).");
				}*/

				try
				{
					Assembly.LoadFrom(nextDll);
					// FUTURE: Consider using Assembly.ReflectionOnlyLoadFrom in a separate AppDomain to figure out if there's anything useful in an assembly before loading it for reals
				}
				catch (BadImageFormatException error)
				{
					if (Verbose) Console.Error.WriteLine($"Error loading '{nextDll}': {error.Message}");
				}
			}
		}


		/// <summary>
		/// Searches the types present in the specified assembly to find a type that implements 
		/// the specified interface.
		/// </summary>
		/// <param name="targetInterface">The target interface that returned types should implement.</param>
		/// <param name="assemblyToSearch">The assembly to search through for matching types.</param>
		public static IEnumerable<Type> IterateImplementingTypes(Type targetInterface, Assembly assemblyToSearch)
		{
			if (!targetInterface.IsInterface)
				throw new ArgumentException($"Error: The specified type {targetInterface} is not an " +
					"interface, so it can't be used to search for implementing types.");

			// FUTURE: Add caching here? Reflection is slow
			foreach (Type nextType in IterateAllLoadedTypes())
			{
				// Interfaces implement themselves, but we don't want to return the interface itself
				if (nextType == targetInterface)
					continue;

				// Make sure it implements the specified interface
				if (!targetInterface.IsAssignableFrom(nextType))
					continue;

				yield return nextType;
			}
		}
	}
}