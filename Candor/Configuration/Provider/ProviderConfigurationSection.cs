
using System.Configuration;

namespace Candor.Configuration.Provider
{
	/// <summary>
	/// A custom configuration section for any provider abstraction.
	/// </summary>
	public class ProviderConfigurationSection : ConfigurationSection
	{
		/// <summary>
		/// Creates a new instance of ProviderConfigurationSection
		/// </summary>
		public ProviderConfigurationSection()
		{
		}
		/// <summary>
		/// The provider definitions.
		/// </summary>
		[ConfigurationProperty("providers")]
		public ProviderSettingsCollection Providers
		{
			get { return ((ProviderSettingsCollection)base["providers"]); }
		}
		/// <summary>
		/// Gets or sets the name of the default provider.
		/// </summary>
		[ConfigurationProperty("defaultProvider", IsRequired = false, DefaultValue = "")]
		public string DefaultProvider
		{
			get { return ((string)base["defaultProvider"]); }
			set { base["defaultProvider"] = value; }
		}
	}
}