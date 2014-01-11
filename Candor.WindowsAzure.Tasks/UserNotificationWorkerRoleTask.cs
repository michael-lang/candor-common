using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Mail;
using Candor.Configuration.Provider;
using Candor.Security;
using Microsoft.WindowsAzure.Storage.Queue;
using Xipton.Razor;

namespace Candor.WindowsAzure.Tasks
{
    /// <summary>
    /// Sends password reset notifications.
    /// </summary>
    /// <remarks>
    /// Your worker project must have a folder /Views/Account/ with 2 views, ForgotPassword.cshtml and ForgotPasswordNoUser.cshtml.
    /// </remarks>
    public sealed class UserNotificationWorkerRoleTask : CloudQueueWorkerRoleTask<User>
    {
        private RazorMachine _razorMachine;
        private int _port = 25;
        /// <summary>
        /// Gets or sets the server name or address to use when sending
        /// email messages.
        /// </summary>
        public string SmtpServer { get; set; }
        /// <summary>
        /// Gets or sets the port used for SMTP transactions.
        /// </summary>
        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }
        /// <summary>
        /// Gets or sets the credentials user name to send to the SMTP server.
        /// </summary>
        public string SmtpUserName { get; set; }
        /// <summary>
        /// Gets or sets the credentials password to send to the SMTP server.
        /// </summary>
        public string SmtpPassword { get; set; }
        /// <summary>
        /// Gets or sets the email address used as the from address.
        /// </summary>
        public string FromAddress { get; set; }
        /// <summary>
        /// Creates a new instance, initialized with a name equal to the type name.
        /// </summary>
        public UserNotificationWorkerRoleTask()
        {
            Initialize(typeof(UserNotificationWorkerRoleTask).Name, new NameValueCollection());
        }
        /// <summary>
        /// Creates a new instance, initialized with a specific name.
        /// </summary>
        /// <param name="name"></param>
        public UserNotificationWorkerRoleTask(string name)
        {
            Initialize(name, new NameValueCollection());
        }
        /// <summary>
        /// Initializes the provider.  Already called by the 2 constructors.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="configValue"></param>
        public override void Initialize(string name, NameValueCollection configValue)
        {
            QueueName = "UserForgotPassword";
            base.Initialize(name, configValue);

            SmtpServer = configValue.GetStringValue("SmtpServer", "");
            Port = configValue.GetInt32Value("Port", Port);
            SmtpUserName = configValue.GetStringValue("SmtpUserName", SmtpUserName);
            SmtpPassword = configValue.GetStringValue("SmtpPassword", SmtpPassword);
            FromAddress = configValue.GetStringValue("DefaultFromAddress", FromAddress);

            _razorMachine = new RazorMachine();
        }
        /// <summary>
        /// Processes a notification queue and sends the notification email.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool ProcessMessage(CloudQueueMessage message)
        {
            var change = QueueProxy.DeserializeRecordChangeNotification(message);

            var msg = new MailMessage();
            msg.To.Add(change.PartitionKey);
            msg.From = new MailAddress(FromAddress, FromAddress);
            msg.IsBodyHtml = true;
            msg.Priority = MailPriority.Normal;

            var user = ProviderResolver<UserProvider>.Get.Provider.GetUserByName(change.PartitionKey);
            var emailAddress = change.PartitionKey;
            try
            {

                if (user == null)
                {
                    msg.Subject = "Temporary Password - No Account found";
                    var template = _razorMachine.ExecuteUrl("~/Account/ForgotPasswordNoUser",
                        new {UserName = emailAddress});
                    msg.Body = template.Result;
                    if (string.IsNullOrWhiteSpace(msg.Body))
                    {
                        LogProvider.Error("No email body generated for /Views/Account/ForgotPasswordNoUser.cshtml");
                        return true; //this is very likely on purpose.
                    }
                }
                else
                {
                    var resetCode = ProviderResolver<UserProvider>.Get.Provider.GenerateUserResetCode(emailAddress);
                    user.PasswordHash = resetCode;
                    msg.Subject = "Temporary Password";
                    var template = _razorMachine.ExecuteUrl("~/Account/ForgotPassword", user);
                    msg.Body = template.Result;
                    if (string.IsNullOrWhiteSpace(msg.Body))
                    {
                        LogProvider.Error("No email body generated for /Views/Account/ForgotPassword.cshtml");
                        return false;
                    }
                }
            }
            catch (Exception exBody)
            {
                LogProvider.ErrorFormat("Failed to generate body for password reset to email to address '{0}'; User:{1};", exBody,
                    emailAddress, user != null);
                return false;
            }

            var client = new SmtpClient { Host = SmtpServer, Port = Port };
            if (SmtpUserName.Length > 0)
                client.Credentials = new NetworkCredential(SmtpUserName, SmtpPassword);

            // try to send Mail
            try
            {
                client.Send(msg);
                return true;
            }
            catch (Exception ex)
            {
                LogProvider.ErrorFormat("Failed to send password reset to email to address '{0}'; User:{1}", ex,
                    emailAddress, user != null);
                return false;
            }
        }
    }
}
