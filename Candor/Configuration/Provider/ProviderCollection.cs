using System;
using System.Configuration;
using System.Configuration.Provider;
using System.Web.Configuration;
using Common.Logging;
using prov = System.Configuration.Provider;

namespace Candor.Configuration.Provider
{
	/// <summary>
	/// Represents a strongly typed collection of provider implementations.
	/// </summary>
	/// <typeparam name="T">The type of provider that will be in the list. The provider MUST inherit 
	/// from System.Configuration.Provider.ProviderBase.</typeparam>
	public sealed class ProviderCollection<T> : prov.ProviderCollection
		where T : prov.ProviderBase
	{
		private readonly Type _parentType;
		private string _configSectionName;
		private ProviderConfigurationSection _config;
		private T _provider;

		/// <summary>
		/// Instantiates a new empty provider collection that does not use configuration, 
		/// and instead allows for adding providers at run time.
		/// </summary>
		public ProviderCollection() { }
		/// <summary>
		/// Instantiates a new ProviderCollection and initializes it from the configuration for the specified parent type.
		/// </summary>
		/// <param name="parentType"></param>
		public ProviderCollection( Type parentType )
		{
			_parentType = parentType;
			_configSectionName = _parentType.Namespace + "/" + _parentType.Name;
			InstantiateProviders();
		}
		/// <summary>
		/// Instantiates a new ProviderCollection and initializes it from the configuration 
		/// for the specified parent type and the specified configuration section.
		/// </summary>
		/// <param name="parentType"></param>
		/// <param name="configSectionName"></param>
		public ProviderCollection( Type parentType, string configSectionName )
		{
			_parentType = parentType;
			_configSectionName = configSectionName;
			InstantiateProviders();
		}

		private ILog _logProvider;
		/// <summary>
		/// Gets or sets the log destination for this collection.  If not set, it will be automatically loaded when needed.
		/// </summary>
		public ILog LogProvider
		{
			get { return _logProvider ?? (_logProvider = LogManager.GetLogger(typeof (T))); }
		    set { _logProvider = value; }
		}
		/// <summary>
		/// Gets the provider to be activated for the current environment.
		/// </summary>
		public T ActiveProvider
		{
			get
			{
				AssertProviderDefined();
				return _provider;
			}
		}
		/// <summary>
		/// Instantiates all configured providers.
		/// </summary>
		public void InstantiateProviders()
		{
			InstantiateProviders(_configSectionName);
		}
		/// <summary>
		/// Instantiates all configured providers.
		/// </summary>
		public void InstantiateProviders(string configSectionName )
		{
			_configSectionName = configSectionName;
			if (LogProvider != null)
				LogProvider.Debug("Initializing " + (_configSectionName ?? ""));
			try
			{
				_config = ConfigurationManager.GetSection(_configSectionName) as ProviderConfigurationSection;
				if (_config == null)
					throw new ProviderException(string.Format("'{0}' section missing.", _configSectionName));
				if (_config.Providers == null || _config.Providers.Count == 0)
					throw new ProviderException(string.Format("No '{0}' providers have been configured.", 
                        String.IsNullOrWhiteSpace(_configSectionName) ? typeof(T).FullName : _configSectionName));

				ProvidersHelper.InstantiateProviders(_config.Providers, this, typeof(T));

				SetActiveProvider(_config.DefaultProvider);
			}
			catch (Exception ex)
			{
				if (LogProvider != null)
					LogProvider.Fatal("Could not Instantiate providers from configuration.", ex);
				throw;
			}
		}
		/// <summary>
		/// Throws an exception if no active provider is defined.
		/// </summary>
		public void AssertProviderDefined()
		{
			try
			{
				if (Count == 0)
					throw new ProviderException(string.Format("No '{0}' providers have been configured.",
                        String.IsNullOrWhiteSpace(_configSectionName) ? typeof(T).FullName : _configSectionName));

				if (_provider == null)
					throw new ProviderException(string.Format("The {0} provider to use was not specified.  Use the 'defaultProvider' attribute or set it via code.", typeof(T).FullName));
			}
			catch (Exception ex)
			{
				if (LogProvider != null)
					LogProvider.Fatal(ex.Message, ex);
				throw;
			}
		}
		/// <summary>
		/// Sets the current provider to the provider of the specified name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool SetActiveProvider( string name )
		{
			_provider = null;
			if (string.IsNullOrEmpty(name))
				return false;

			_provider = this[name];
			if (_provider == null)
				throw new ProviderException(string.Format("{0} provider '{1}' is not defined.", typeof(T).FullName, name));

			if (LogProvider != null)
				LogProvider.Info(string.Format("ActiveProvider set to:'{0}'", _provider.Name));
			return true;
		}
		/// <summary>
		/// Sets the current provider to the specified instance.  If it does not currently exist in the collection then it will be added.
		/// </summary>
		/// <param name="provider"></param>
		/// <returns></returns>
		public bool SetActiveProvider( T provider )
		{
			_provider = null;
			if (provider == null)
				return false;

			T existing = (T)base[provider.Name];
			if (existing == null)
				Add(provider);
			else if (existing != provider)
			{
				Remove(provider.Name);
				Add(provider);
			}

			_provider = provider;

			if (LogProvider != null)
				LogProvider.Info(string.Format("ActiveProvider set to:'{0}'", _provider.Name));
			return true;
		}
		/// <summary>
		/// Adds a provider to the collection.
		/// </summary>
		/// <param name="provider">The provider to add to the collection.</param>
		public override void Add( prov.ProviderBase provider )
		{
			if (provider == null)
				throw new ArgumentNullException("provider");

			if (!(provider is T))
				throw new ArgumentException("Input must be of type " + typeof(T).FullName);

			base.Add(provider);
		}
		/// <summary>
		/// Adds a provider to the collection.
		/// </summary>
		/// <param name="provider">The provider to add to the collection.</param>
		/// <remarks>This method overload helps chaining for code configuration.</remarks>
		public T Add( T provider )
		{
			if (provider == null)
				throw new ArgumentNullException("provider");

			base.Add(provider);
			return provider;
		}

		/// <summary>
		/// Indexer that get a provider from the collection by name.
		/// </summary>
		/// <param name="name">The name of the provider to get.</param>
		/// <returns>The provider that had the given name.</returns>
		public new T this[string name]
		{
			get
			{
				return (T)base[name];
			}
		}
	}
}