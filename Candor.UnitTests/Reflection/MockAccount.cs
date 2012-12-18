using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Candor.Reflection
{
    public class MockAccount
    {
        [DisplayName("Account Number")]
        [Description("The unqiue number for this account")]
        public long AccountNumber { get; set; }
        //intentionally no display name or description attributes for tests
        public string Blah { get; set; }
        //intentionally no display name or description attributes for tests.
        public AccountType AccountType { get; set; }
    }
}
