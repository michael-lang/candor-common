using System;
using System.Security.Cryptography;
using System.Text;

namespace Candor.Security.Cryptography
{
	public class SHA2HashProvider : HashProvider
	{
		/// <summary>
		/// Creates a true random salt.
		/// </summary>
		/// <param name="length">length of the salt to return</param>
		/// <returns></returns>
		public override string GetSalt(int length)
		{
			RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
			Byte[] salt = new Byte[length / 8];	//32 length = 256 bits
			random.GetBytes(salt);
			return BytesToHex(salt);
		}
		/// <summary>
		/// Creates a non-reversible hashed value consistently given the same input.
		/// </summary>
		/// <param name="salt">Another semi-secret value paired with the secret to
		/// make it more difficult to dictionary attack a collection of hashed values.</param>
		/// <param name="originalValue">The original value to keep secret.</param>
		/// <param name="iterations">The iterations to hash the originalValue and salt.</param>
		/// <returns>The hashed value.</returns>
		public override string Hash(string salt, string originalValue, int iterations)
		{
			if (iterations < 500)
				throw new ArgumentOutOfRangeException("iterations", "hash iterations must be equal to or greater than 500");
			String passwordHashed = originalValue;
			for (Int32 i = 1; i <= iterations; i++)
			{
				passwordHashed = Sha256Hex(salt + passwordHashed + SaltModifier ?? "");
			}
			return passwordHashed;
		}

		private String Sha256Hex(String stringToHash)
		{
			SHA256Managed hash = new SHA256Managed();
			byte[] utf8 = UTF8Encoding.UTF8.GetBytes(stringToHash);
			return BytesToHex(hash.ComputeHash(utf8));
		}
		private String BytesToHex(Byte[] toConvert)
		{
			StringBuilder output = new StringBuilder(toConvert.Length * 2);
			foreach (Byte b in toConvert)
				output.Append(b.ToString("x2"));
			return output.ToString();
		}

	}
}
