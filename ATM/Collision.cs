using System;

namespace ATM.System
{
    public class Collision
    {
        public Flight FlightA { get; private set; }
        public Flight FlightB { get; private set; }

        public Collision(Flight flightA, Flight flightB)
        {
            SetFlights(flightA, flightB);       
        }

        //this method makes sure that two instances of Collision with FlightA and FlightB are created with opposite
        //orders of flights (eg. Collision(FlightABC, FlightDEF) and Collision(FlightDEF, FlightABC)
        //will always return the same hashcode and compare properly.
        private void SetFlights(Flight a, Flight b)
        {
            if (a.TData.Tag.GetHashCode() >= b.TData.Tag.GetHashCode())
            {
                FlightA = a;
                FlightB = b;
            }
            else
            {
                FlightA = b;
                FlightB = a;
            }
        }

        private bool Equals(Collision compare)
        {
            if (this.FlightA.TData.Tag == compare.FlightA.TData.Tag && this.FlightB.TData.Tag == compare.FlightB.TData.Tag)
            {
                return true;
            }
            else return false;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Collision);
        }

        public override int GetHashCode()
        {
            return string.Format(this.FlightA.TData.Tag + this.FlightB.TData.Tag).GetHashCode();
        }

        public override string ToString()
        {
            return "Collision between " + FlightA.TData.Tag + " and " + FlightB.TData.Tag;
        }
    }
}