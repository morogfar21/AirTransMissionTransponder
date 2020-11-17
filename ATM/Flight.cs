using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATM.System
{
    public class Flight
    {
        public TransponderData TData { get; set; }
        public double Speed { get; set; }
        public double Direction { get; set; }
        public bool collision { get; set; }

        public Flight(TransponderData data)
        {
            TData = data;
            Speed = 0;
            Direction = 0;
            collision = false;
        }

        private bool Equals(Flight compare)
        {
            if (this.TData.Tag == compare.TData.Tag)
            {
                return true;
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Flight);
        }
    }
}
