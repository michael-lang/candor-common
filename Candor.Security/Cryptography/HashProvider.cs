using System;
using System.Configuration.Provider;
using Candor.Configuration.Provider;

namespace Candor.Security.Cryptography
{
	// See http://crackstation.net/hashing-security.htm
	// See http://blog.barthe.ph/2012/06/15/howto-store-passwords/
	public abstract class HashProvider : ProviderBase
	{
        /// <summary>
        /// Gets or sets if this provider is obsolete, and thus should not be
        /// used for generating new hashed passwords.
        /// </summary>
        /// <remarks>
        /// Obsolete hash providers remain for users that have not changed their password
        /// since it was made obsolete.  It remains so they can still sign in.  At that time
        /// a new hash should be generated from that raw password using another non-obsolete
        /// hash provider.
        /// Making a provider obsolete is a way to migrate your database to use a new
        /// algorithm in the future when the current one in use becomes insufficiently 
        /// secure; all without requiring users to reset their passwords.
        /// </remarks>
        public virtual Boolean IsObsolete { get; set; }
        /// <summary>
        /// Gets or sets an additional salt to be added to all hashes.
        /// </summary>
        /// <remarks>
        /// This allows for a code added salt in addition to the salt stored in a
        /// database per user.  This should not replace a salt per user (or 
        /// other record type).  Having a database and configuration salt requires
        /// more of the system to be compromized before hashed data is compromized.
        /// </remarks>
        public virtual String SaltModifier { get; set; }
        /// <summary>
        /// Initializes this provider's base settings.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);
            IsObsolete = config.GetBooleanValue("IsObsolete", false);
            SaltModifier = config.GetStringValue("SaltModifier", null); //none by default.
        }
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
