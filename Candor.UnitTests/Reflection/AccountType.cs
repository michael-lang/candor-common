using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Candor.Reflection
{
    public enum AccountType
    {
		[Description("The first type of account")]
		Type1,
		//intentionally no attributes for tests
		Type2
    }
}
