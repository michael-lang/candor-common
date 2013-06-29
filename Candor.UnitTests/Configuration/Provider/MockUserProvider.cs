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
        public override Security.User GetUserByID(Guid userId)
        {
            throw new NotImplementedException();
        }

        public override Security.User GetUserByName(string name)
        {
            throw new NotImplementedException();
        }

        protected override void InsertUserHistory(Security.AuthenticationHistory history)
        {
            throw new NotImplementedException();
        }

        protected override void SaveUserSession(Security.UserSession session)
        {
            throw new NotImplementedException();
        }

        protected override void SaveUser(Security.User user)
        {
            throw new NotImplementedException();
        }

        protected override void SaveUserSalt(Security.UserSalt salt)
        {
            throw new NotImplementedException();
        }

        protected override Security.UserSalt GetUserSalt(Guid userId)
        {
            throw new NotImplementedException();
        }

        protected override Security.UserSession GetUserSession(Guid renewalToken)
        {
            throw new NotImplementedException();
        }

        protected override int GetRecentFailedUserNameAuthenticationCount(string name)
        {
            throw new NotImplementedException();
        }

        protected override Security.AuthenticationHistory GetSessionAuthenticationHistory(Security.UserSession session)
        {
            throw new NotImplementedException();
        }
    }
}
