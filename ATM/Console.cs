using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATM.System;

namespace ATM.System
{
    //Excluded from code coverage since stdout prints cannot be tested.
    [ExcludeFromCodeCoverageAttribute]
    class Console : IConsole
    {
        public void WriteLine(string lineToWrite)
        {
            global::System.Console.WriteLine(lineToWrite);
        }

        public void Clear()
        {
            global::System.Console.Clear();
        }
    }
}
