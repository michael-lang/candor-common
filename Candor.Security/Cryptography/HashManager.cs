using System;
using System.Collections.Generic;
using System.Linq;
using prov = Candor.Configuration.Provider;

namespace Candor.Security.Cryptography
{
	public class HashManager
	{
		private static prov.ProviderCollection<HashProvider> _providers;
        private static List<HashProvider> _currentProviders;

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

        /// <summary>
        /// Gets a provider to use for creating a new hash (not hashing to match an existing hash)
        /// </summary>
        /// <returns>A pseudo-random hashprovider of the available non-obsolete providers.</returns>
        public static HashProvider SelectProvider()
        {
            if (_currentProviders == null)
            {
                _currentProviders = new List<HashProvider>();
                foreach (HashProvider provider in Providers)
                    if (!provider.IsObsolete)
                        _currentProviders.Add(provider);
            }
            if (_currentProviders.Count == 0)
                return Provider; //use current obsolete one as fallback
            int index = new Random(DateTime.Now.Second).Next(0, _currentProviders.Count - 1);
            return _currentProviders[index];
        }

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
	}
}