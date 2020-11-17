using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATM.System
{
    public interface IConsole
    {
        void WriteLine(string lineToWrite);
        void Clear();
    }
}
