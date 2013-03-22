using System;
using System.Collections.Specialized;

namespace Candor.Configuration.Provider
{
    class MockUserProvider : Security.UserProvider
    {
        public MockUserProvider()
        {
            this.Initialize("mock", new NameValueCollection());
        }
        public override Security.User GetUserByID(Guid userID)
        {
            throw new NotImplementedException();
        }

        public override Security.User GetUserByName(string name)
        {
            throw new NotImplementedException();
        }

        public override Security.UserIdentity AuthenticateUser(string name, string password, Security.UserSessionDurationType duration, string ipAddress, ExecutionResults result)
        {
            throw new NotImplementedException();
        }

        public override Security.UserIdentity AuthenticateUser(string token, Security.UserSessionDurationType duration, string ipAddress, ExecutionResults result)
        {
            throw new NotImplementedException();
        }

        public override Security.UserIdentity RegisterUser(Security.User user, Security.UserSessionDurationType duration, string ipAddress, ExecutionResults result)
        {
            throw new NotImplementedException();
        }

        public override bool UpdateUser(Security.User item, string currentPassword, string ipAddress, ExecutionResults result)
        {
            throw new NotImplementedException();
        }

        public override string GenerateUserResetCode(string name)
        {
            throw new NotImplementedException();
        }
    }
}
