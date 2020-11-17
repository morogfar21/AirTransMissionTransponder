using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATM.System
{
    public class InvalidInputException: Exception
    {
        public InvalidInputException(string s): base(s)
        {

        }
    }
}
