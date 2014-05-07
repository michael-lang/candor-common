using System;

namespace Candor.WindowsAzure
{
    public class ContactMessage
    {
        public Contact To { get; set; }
        public Contact From { get; set; }
        public String MessageBody { get; set; }
    }
}
