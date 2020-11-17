using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATM.System
{

    public interface IFlightFilter
    {
        event EventHandler<TransponderArgs> transponderFilterChanged;
        void FilterFlight(object sender, TransponderArgs e);
    }
}
