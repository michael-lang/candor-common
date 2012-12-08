using System;
using prov = Candor.Configuration.Provider;

namespace Candor.Security.Cryptography
{
	public class HashManager
	{
		#region Fields
		private static prov.ProviderCollection<HashProvider> _providers;
		#endregion Fields

		#region Properties
		/// <summary>
		/// Gets the default provider instance.
		/// </summary>
		public static HashProvider Provider
		{
			get { return Providers.ActiveProvider; }
		}
		/// <summary>
		/// Gets all the configured Authorization providers.
		/// </summary>
		public static prov.ProviderCollection<HashProvider> Providers
		{
			get
			{
				if (_providers == null)
					_providers = new prov.ProviderCollection<HashProvider>(typeof(HashManager));
				return _providers;
			}
		}
		#endregion Properties

		#region Methods

		/// <summary>
		/// Creates a true random salt with a default length of 256.
		/// </summary>
		/// <returns></returns>
		public static String GetSalt()
		{
			return Provider.GetSalt();
		}
		/// <summary>
		/// Creates a true random salt.
		/// </summary>
		/// <param name="length"></param>
		/// <returns></returns>
		public static String GetSalt(Int32 length)
		{
			return Provider.GetSalt(length);
		}
		/// <summary>
		/// Creates an unreversible hashed value consistently given the same input.
		/// </summary>
		/// <param name="salt">Another non-secret value paired with the secret to
		/// make it more difficult to dictionary attack a collection of hashed values.</param>
		/// <param name="originalValue">The original value to keep secret.</param>
		/// <param name="iterations">The iterations to hash the originalValue and salt.</param>
		/// <returns>The hashed value.</returns>
		public static String Hash(String salt, String originalValue, Int32 iterations)
		{
			return Provider.Hash(salt, originalValue, iterations);
		}
		#endregion Methods
	}
}