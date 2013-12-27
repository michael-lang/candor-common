using System;
using System.Collections.Generic;
using Common.Logging;
using prov = Candor.Configuration.Provider;

namespace Candor.Security
{
    /*  Configuration Example (web.config):
    Don't delete other configuration, instead just merge this config into your existing configuration.
    This sample will not work exactly as shown because you will need to plug in the correct AppFabric cache name, web service url, username, password, and database connection string; but otherwise it should work.

    <configuration>
        <configSections>
            <sectionGroup name="Candor.Security">
                <section name="UserManager"
                    type="Candor.Configuration.Provider.ProviderConfigurationSection, Candor.Configuration" />
            </sectionGroup>
        </configSections>
	
        <connectionStrings>
            <add name="MainConnection"
                     connectionString="[...]"/>
        </connectionStrings>
	
        <Candor.Security>
            <UserManager defaultProvider="AppFabric" useEnvironmentSwitch="false">
                <providers>
                    <!-- These are just examples: Copy just these provider types that make sense for your project
                        The type attributes below show the suggested project name and class name for each type of provider for consistency across projects.
                    -->
                    <add name="AppFabric"
                        type="Candor.Security.Caching.AppFabricProvider.AppFabricUserProvider, Candor.Security.Caching.AppFabricProvider"
                             TargetProviderName="Oracle"
                             CacheName="[SomeCacheNameHere]" />
                    <add name="WebService"
                             type="Candor.Security.WebServiceProvider.WebServiceUserProvider, Candor.Security.WebServiceProvider"
                             webServiceUrl = "http://test.domain.com/[ProjectName]/[WebServiceName].asmx"
                             webServiceUserName="[AppSpecificID]"
                             webServicePassword="[AppSpecificPassword]" />
                    <add name="SQL"
                             type="Candor.Security.Data.SQLProvider.SQLUserProvider, Candor.Security.Data.SQLProvider"
                             connectionName="MainConnection" />
                    <add name="Dummy"
                             type="Candor.Security.Tests.MockProviders.MockUserProvider, Candor.Security.Tests" />
                </providers>
            </UserManager>
        </Candor.Security>
    </configuration>

        The WebService provider is the only one that needs to be transformed per environment.  A configuration transform might look like the following, such as Web.Test.config.  You should add or change settings to your transform shown here, but do not delete any other sections in your transforms that are not in this example.  This sample will not work exactly as shown because you will need to plug in the correct web service url, username, and password, and database connection string; but otherwise it should work.
	
        <configuration xmlns:xdt="http://schemas.microsoft.com/XML-Document-Transform">
        <appSettings>
            <add key="CurrentEnvironment" value="Test" xdt:Transform="SetAttributes" xdt:Locator="Match(key)" />
        </appSettings>
        <connectionStrings>
            <add name="MainConnection"
                     connectionString="[...]"
                     xdt:Transform="SetAttributes"
                     xdt:Locator="Match(name)"/>
        </connectionStrings>
	
        <Candor.Security>
            <UserManager>
                <providers>
                    <add name="WebService"
                             type="Candor.Security.WebServiceProvider.WebServiceUserProvider, Candor.Security.WebServiceProvider"
                             webServiceUrl="http://domain.com/[ProjectName]/[WebServiceName].asmx"
                             webServiceUserName="[AppSpecificID]"
                             webServicePassword="[AppSpecificPassword]"
                             xdt:Transform="Replace"
                             xdt:Locator="Match(name)" />
                </providers>
            </UserManager>
        </Candor.Security>
					
    */
    /// <summary>
    /// The user manager is a singleton access point to get to the collection of configured
    /// UserProvider.  If the providers are already configured via code configuration then
    /// this manager class will get the providers from there.  If they are not configured
    /// by code then the providers must be defined in the configuration file for the AppDomain.
    /// </summary>
    public static class UserManager
    {
        private static ILog _logProvider;

        /// <summary>
        /// Gets or sets the log destination for this type.  If not set, it will be automatically loaded when needed.
        /// </summary>
        public static ILog LogProvider
        {
            get { return _logProvider ?? (_logProvider = LogManager.GetLogger(typeof(UserManager))); }
            set { _logProvider = value; }
        }
        /// <summary>
        /// Gets the default provider instance.
        /// </summary>
        public static UserProvider Provider
        {
            get { return Providers.ActiveProvider; }
        }
        /// <summary>
        /// Gets all the configured User providers.
        /// </summary>
        public static prov.ProviderCollection<UserProvider> Providers
        {
            get
            {
                var resolver = prov.ProviderResolver<UserProvider>.Get;
                if (resolver.Providers.Count == 0)
                    resolver.Providers = new prov.ProviderCollection<UserProvider>(typeof(UserManager));

                return resolver.Providers;
            }
        }

        /// <summary>
        /// Validates that a password meets minimum requirements.
        /// </summary>
        /// <param name="password"></param>
        /// <param name="results"></param>
        /// <returns></returns>
        public static bool ValidatePassword(string password, ExecutionResults results)
        {
            return Provider.ValidatePassword(password, results);
        }
        /// <summary>
        /// Validates that a string is a valid email address format.
        /// </summary>
        /// <returns></returns>
        public static bool ValidateEmailAddressFormat(string emailAddress)
        {
            return Provider.ValidateEmailAddressFormat(emailAddress);
        }
        /// <summary>
        /// Validates that the specified name meets minimum requirements.
        /// </summary>
        /// <param name="name">The desired name/alias.</param>
        /// <param name="result">Any error messages about the desired name.</param>
        /// <returns></returns>
        public static bool ValidateName(string name, ExecutionResults result)
        {
            return Provider.ValidateName(name, result);
        }
        /// <summary>
        /// Gets a user by identity.
        /// </summary>
        /// <param name="userID">The unique identity.</param>
        /// <returns></returns>
        public static User GetUserByID(Guid userID)
        {
            return Provider.GetUserByID(userID);
        }
        /// <summary>
        /// Gets a user by name.
        /// </summary>
        /// <param name="name">The unique sign in name.</param>
        /// <returns></returns>
        public static User GetUserByName(string name)
        {
            return Provider.GetUserByName(name);
        }
        /// <summary>
        /// Authenticates against the data store and returns a UserIdentity given 
        /// a user name, and password.
        /// </summary>
        /// <param name="name">The unique user name.</param>
        /// <param name="password">The matching password.</param>
        /// <param name="ipAddress">The internet address where the user is connecting from.</param>
        /// <param name="duration">The amount of time that the issued token will be valid.</param>
        /// <param name="result">A ExecutionResults instance to add applicable
        /// warning and error messages to.</param>
        /// <returns>
        /// A valid user instance.  If the user did not exist or the 
        /// credentials are incorrect then the IsAuthenticated flag
        /// will be false.  If the credentials were correct the 
        /// IsAuthenticated flag will be true.
        /// </returns>
        public static UserIdentity AuthenticateUser(string name, string password, UserSessionDurationType duration, string ipAddress,
                                                      ExecutionResults result)
        {
            return Provider.AuthenticateUser(name, password, duration, ipAddress, result);
        }
        /// <summary>
        /// Authenticates against the data store and returns a UserIdentity given
        /// a token returned from a previous authentication.
        /// </summary>
        /// <param name="token">The unique token.</param>
        /// <param name="duration">The amount of time that the renewed token will be valid.</param>
        /// <param name="ipAddress">The internet address where the user is connecting from.</param>
        /// <param name="result">A ExecutionResults instance to add applicable
        /// warning and error messages to.</param>
        /// <returns>
        /// A valid user identity instance.  If the token is incorrect or expired
        /// then the IsAuthenticated flag will be false.  Otherwise the identity
        /// will be authenticated.
        /// </returns>
        public static UserIdentity AuthenticateUser(string token, UserSessionDurationType duration, String ipAddress, ExecutionResults result)
        {
            return Provider.AuthenticateUser(token, duration, ipAddress, result);
        }
        /// <summary>
        /// Registers a new user.  The PasswordHash property should be the actual password.
        /// </summary>
        /// <param name="user">A user with a raw password which is turned into a password hash as part of registration.</param>
        /// <param name="duration">The amount of time that the initial session will be valid.</param>
        /// <param name="ipAddress">The internet address where the user is connecting from.</param>
        /// <param name="result">A ExecutionResults instance to add applicable
        /// warning and error messages to.</param>
        /// <returns>A boolean indicating success (true) or failure (false).</returns>
        public static UserIdentity RegisterUser(User user, UserSessionDurationType duration, String ipAddress, ExecutionResults result)
        {
            return Provider.RegisterUser(user, duration, ipAddress, result);
        }
        /// <summary>
        /// updates a user's name and/or password.
        /// </summary>
        /// <param name="item">The user details to be saved.  If Password is empty is it not changed.  If specified it should be the new raw password (not a hash).</param>
        /// <param name="currentPassword">The current raw password for the user used to authenticate that the change can be made.</param>
        /// <param name="ipAddress">The internet address where the user is connecting from.</param>
        /// <param name="result">A ExecutionResults instance to add applicable
        /// warning and error messages to.</param>
        /// <returns>A boolean indicating success (true) or failure (false).</returns>
        public static bool UpdateUser(User item, String currentPassword, String ipAddress, ExecutionResults result)
        {
            return Provider.UpdateUser(item, currentPassword, ipAddress, result);
        }
        /// <summary>
        /// Generates a new password reset code for a user and stores that as the current code valid
        /// for the next hour.
        /// </summary>
        /// <param name="name">The user name / email address.</param>
        /// <returns>If the user exists, then a reset code string; otherwise null.</returns>
        public static String GenerateUserResetCode(String name)
        {
            return Provider.GenerateUserResetCode(name);
        }

        /// <summary>
        /// Gets the latest session(s) for a given user.
        /// </summary>
        /// <param name="userId">The unique identity.</param>
        /// <param name="take">The maximum number of sessions to retrieve.</param>
        /// <returns>A list of sessions; If empty then the user has never logged in (such as a no-show guest).</returns>
        public static List<UserSession> GetLatestUserSessions(Guid userId, Int32 take)
        {
            return Provider.GetLatestUserSessions(userId, take);
        }
    }
}