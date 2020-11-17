using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATM.System
{
    public class FlightArgs : EventArgs
    {
        public List<Flight> flights { get; set; }
    }

    public interface IFlightCollection
    {
        event EventHandler<FlightArgs> flightsChanged;

        void GetTransponderData(object sender, TransponderArgs e);
        void HandleNewData(TransponderData newData);
        void Notify();


    }
}
