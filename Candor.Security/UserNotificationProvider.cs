using System;
using System.Configuration.Provider;

namespace Candor.Security
{
    public abstract class UserNotificationProvider : ProviderBase
    {
        /// <summary>
        /// Sends a password reset notification to the specified user.
        /// </summary>
        /// <param name="userName">The user to notify.</param>
        /// <param name="result">
        /// A placeholder for errors in the system, such as email not available now.
        /// Do not notify caller if the user does not exist, for security reasons.</param>
        /// <remarks>
        /// 
        /// </remarks>
        public virtual bool NotifyPasswordReset(String userName, ExecutionResults result)
        {   //Either configure the Azure implementation, or create your own.
            result.AppendError("The forgot password functionality is not yet implemented.");
            return false;
        }
    }
}
