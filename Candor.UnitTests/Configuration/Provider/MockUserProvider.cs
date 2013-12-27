using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Candor.Security;

namespace Candor.Configuration.Provider
{
    public sealed class MockUserProvider : UserProvider
    {
        public MockUserProvider()
        {
            Initialize("mock", new NameValueCollection());
        }
        public override User GetUserByID(Guid userId)
        {
            throw new NotImplementedException();
        }

        public override User GetUserByName(string name)
        {
            throw new NotImplementedException();
        }

        protected override void InsertUserHistory(AuthenticationHistory history)
        {
            throw new NotImplementedException();
        }

        protected override void SaveUserSession(UserSession session)
        {
            throw new NotImplementedException();
        }

        protected override void SaveUser(User user)
        {
            throw new NotImplementedException();
        }

        protected override void SaveUserSalt(UserSalt salt)
        {
            throw new NotImplementedException();
        }

        protected override UserSalt GetUserSalt(Guid userId)
        {
            throw new NotImplementedException();
        }

        public override List<UserSession> GetLatestUserSessions(Guid userId, Int32 take)
        {
            throw new NotImplementedException();
        }

        protected override UserSession GetUserSession(Guid renewalToken)
        {
            throw new NotImplementedException();
        }

        protected override int GetRecentFailedUserNameAuthenticationCount(string name)
        {
            throw new NotImplementedException();
        }

        protected override AuthenticationHistory GetSessionAuthenticationHistory(UserSession session)
        {
            throw new NotImplementedException();
        }
    }
}
