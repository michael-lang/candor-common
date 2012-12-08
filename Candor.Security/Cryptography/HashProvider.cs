using System;
using System.Configuration.Provider;

namespace Candor.Security.Cryptography
{
	// See http://crackstation.net/hashing-security.htm
	// See http://blog.barthe.ph/2012/06/15/howto-store-passwords/
	public abstract class HashProvider : ProviderBase
	{
		/// <summary>
		/// Creates a true random salt with a default length of 256.
		/// </summary>
		/// <returns></returns>
		public virtual String GetSalt()
		{
			return GetSalt(256);
		}
		/// <summary>
		/// Creates a true random salt.
		/// </summary>
		/// <param name="length">length of the salt to return</param>
		/// <returns></returns>
		public abstract String GetSalt(Int32 length);
		/// <summary>
		/// Creates an unreversible hashed value consistently given the same input.
		/// </summary>
		/// <param name="salt">Another non-secret value paired with the secret to
		/// make it more difficult to dictionary attack a collection of hashed values.</param>
		/// <param name="originalValue">The original value to keep secret.</param>
		/// <param name="iterations">The iterations to hash the originalValue and salt.</param>
		/// <returns>The hashed value.</returns>
		public abstract String Hash(String salt, String originalValue, Int32 iterations);


	}
}
