using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATM.System
{

    public interface IRender
    {

        void RenderFlights(object sender, FlightArgs e);
        event EventHandler<FlightArgs> flightsChanged;
    }
}
