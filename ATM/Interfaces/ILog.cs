using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATM.System;

namespace ATM.System
{
    public interface ILog
    {
        void Logwriter(object sender, CollisionArgs e);
        string CollisionToLogString(Collision collision);
    }
}
