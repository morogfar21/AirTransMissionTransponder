using System;
using System.Collections.Generic;

namespace ATM.System
{ 
    public class CollisionArgs : EventArgs
    {
        public Collision Collision { get; private set; }

        public CollisionArgs(Collision col)
        {
            this.Collision = col;
        }
    }

    public interface ICollisionDetector
    {
        void DetectCollisions(List<Flight> flightList);

        event EventHandler<CollisionArgs> NewCollision;
        List<Flight> OnFlightsChanged(FlightArgs e);
        List<Flight> raiseCollisionFlags(List<Flight> flights);
    }
}