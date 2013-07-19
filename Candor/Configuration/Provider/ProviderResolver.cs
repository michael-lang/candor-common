using prov = System.Configuration.Provider;

namespace Candor.Configuration.Provider
{
    /// <summary>
    /// A bridge between applications using Provider model and depedency injection.
    /// </summary>
    /// <remarks>
    /// Sample xml configuration for base type 'Company.App.SomethingProvider':
    /// &lt;Company.App.SomethingProvider defaultProvider="cache" &gt;
    ///  &lt;providers&gt;
    ///	  &lt;add name="cache" type="Company.App.CacheProviders.CacheSomethingProvider, Company.App.CacheProviders"
    ///	   delegateProviderName="sql"/&gt; 
    ///	  &lt;add name="sql" type="Company.App.SqlProviders.SomethingProvider, Company.App.SqlProviders"
    ///	   connectionName="DefaultConnection"/&gt;
    ///	 &lt;/providers&gt;
    /// &lt;/Company.App.SomethingProvider&gt;
    /// 
    /// Sample equivalent code configuration (also no static Manager class is required): 
    /// ProviderResolver&lt;SomethingProvider&gt;.Configure()
    ///  .Append(new SqlSomethingProvider("sql") { ConnectionName = "DefaultConnection"})
    ///  .AppendActive(new CacheSomethingProvider("cache") { DelegateProviderName = "sql"});
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class ProviderResolver<T>
        where T : prov.ProviderBase
    {
        private ProviderCollection<T> _providers;
        
        /// <summary>
        /// Gets the active provider from the <see cref="Providers"/> collection.
        /// </summary>
        public T Provider
        {
            get { return Providers.ActiveProvider; }
        }

        /// <summary>
        /// Gets or sets the configured providers (xml or code config).
        /// </summary>
        /// <remarks>These are lazy loaded on the first and only first request.
        /// If the value is set to null it will be reloaded again as needed.
        /// In the code configuration example you can set this to a collection of
        /// desired providers and then it will not be loaded from configuration xml.</remarks>
        public ProviderCollection<T> Providers
        {
            get
            {
                if (_providers == null)
                    _providers = new ProviderCollection<T>(typeof(T));
                return _providers;
            }
            set { _providers = value; }
        }

        /// <summary>
        /// Appends another provider and returns the resolver for chaining purposes.
        /// </summary>
        /// <param name="provider">The provider instance to append.</param>
        /// <returns></returns>
        public ProviderResolver<T> Append(T provider)
        {
            Providers.Add(provider);
            return this;
        }

        /// <summary>
        /// Appends another provider instance and makes it the active provider, then returns the resolver for chaining purposes.
        /// </summary>
        /// <param name="provider">The provider instance to append.</param>
        /// <returns></returns>
        public ProviderResolver<T> AppendActive(T provider)
        {
            Providers.Add(provider);
            Providers.SetActiveProvider(provider);
            return this;
        }

        /// <summary>
        /// Gets the resolver instance for the generic type and lazy loads it as needed.
        /// </summary>
        public static ProviderResolver<T> Get
        {
            get
            {
                if (!ProviderResolverDictionary.Resolvers.ContainsKey(typeof(T)))
                {
                    var resolver = new ProviderResolver<T>();
                    //this one loads from configuration, since it wasn't configured
                    ProviderResolverDictionary.Resolvers[typeof(T)] = resolver;
                    return resolver;
                }
                return (ProviderResolver<T>)ProviderResolverDictionary.Resolvers[typeof(T)];
            }
        }

        /// <summary>
        /// Sets the <see cref="Providers"/> collection to a new list and returns the resolver for further configuration.
        /// </summary>
        /// <returns></returns>
        public static ProviderResolver<T> Configure()
        {
            var resolver = new ProviderResolver<T> { Providers = new ProviderCollection<T>() };
            ProviderResolverDictionary.Resolvers[typeof(T)] = resolver;
            return resolver;
        }
    }
}
