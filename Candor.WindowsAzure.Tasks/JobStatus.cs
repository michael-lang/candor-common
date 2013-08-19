using System;

namespace Candor.WindowsAzure.Tasks
{
    public class JobStatus
    {
        public String QueueName { get; set; }
        public DateTime RunDateTime { get; set; }
        public Boolean Success { get; set; }
        public String Message { get; set; }
    }
}
