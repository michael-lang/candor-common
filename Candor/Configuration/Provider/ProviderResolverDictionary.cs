using System;
using System.Collections.Generic;

namespace Candor.Configuration.Provider
{
    /// <summary>
    /// An internal only dictionary of resolvers for all provider types.
    /// </summary>
    /// <remarks>Used only by ProviderResolver</remarks>
    internal static class ProviderResolverDictionary
    {
        private static Dictionary<Type, object> _resolvers;
        /// <summary>
        /// Gets the dictionary of resolvers.
        /// </summary>
        public static Dictionary<Type, object> Resolvers
        {
            get
            {
                if (_resolvers == null)
                    _resolvers = new Dictionary<Type, object>();
                return _resolvers;
            }
        }
    }
}
