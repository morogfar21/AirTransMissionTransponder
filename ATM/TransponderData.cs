using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ATM.System
{
    public class TransponderData
    {
        public string Tag { private set; get; }
        public int X { private set; get; }
        public int Y { private set; get; }
        public int Altitude { private set; get; }
        public DateTime Time { private set; get; }

        public TransponderData(string tag, int X, int Y, int altitude, DateTime time)
        {
            this.Tag = tag;
            this.X = X;
            this.Y = Y;
            this.Altitude = altitude;
            this.Time = time;
        }

    }
}
