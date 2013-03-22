using Candor.Security;
using Candor.Security.Web;
using NUnit.Framework;
using sql = Candor.Security.SqlProvider;

namespace Candor.Configuration.Provider
{
    [TestFixture]
    public class ProviderResolverTests
    {
        [Test]
        public void TestCodeConfig()
        {
            ProviderResolver<SecurityContextProvider>.Configure()
                .AppendActive(new WebSecurityContextProvider("web"));
            
            Assert.IsNotNull(ProviderResolver<SecurityContextProvider>.Get.Provider);
        }

        [Test]
        public void TestCodeConfig2()
        {
            ProviderResolver<UserProvider>.Configure()
                .Append(new sql.UserProvider("sql") {PasswordRegexExpression = ".*"})
                .AppendActive(new MockUserProvider());

            //assert that a provider is configured with the option specified (although not the best expression for passwords :)
            Assert.AreEqual(".*", ProviderResolver<UserProvider>.Get.Providers["sql"].PasswordRegexExpression);
            //assert that the main provider is the mock one set as 'active' during configure
            Assert.AreEqual(typeof(MockUserProvider), ProviderResolver<UserProvider>.Get.Provider.GetType());
        }
    }
}
