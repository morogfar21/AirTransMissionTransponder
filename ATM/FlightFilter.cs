using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATM.System
{
    public class FlightFilter : IFlightFilter
    {
        public List<TransponderData> transponderList {get; private set; }
        

        public FlightFilter(IDataFormatter dataFormatter)
        {
            dataFormatter.transponderChanged += FilterFlight;
        }

        public event EventHandler<TransponderArgs> transponderFilterChanged;

        public void FilterFlight(object sender, TransponderArgs e)
        {
            transponderList = e.transponderData;
            foreach (var transponder in transponderList.ToList())
            {
                if (transponder.X < 10000 || transponder.X > 90000)
                {
                    transponderList.Remove(transponder);

                }
                else if (transponder.Y < 10000 || transponder.Y > 90000)
                {
                    transponderList.Remove(transponder);

                }
                else if (transponder.Altitude < 500 || transponder.Altitude > 20000)
                {
                    transponderList.Remove(transponder);
                }
            }
            transponderFilterChanged?.Invoke(this, new TransponderArgs { transponderData = transponderList });
        }


    }
}
