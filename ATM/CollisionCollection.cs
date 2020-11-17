using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Constraints;

namespace ATM.System
{
    public class CollisionCollection: ICollisionCollection
    {
        public List<Collision> Collisions { get; private set; } = new List<Collision>();
        
        public CollisionCollection()
        {
            
        }

        public CollisionCollection(List<Collision> collisions)
        {
            Collisions = collisions;
        }

        //Takes a new List of collisions, creates a list of collisions that are in the supplied list and not in the first
        //Sets the collection to the updated list and returns the new collisions
        public List<Collision> HandleUpdatedCollisions(List<Collision> updatedCollisions)
        {
            List<Collision> newCollisions = updatedCollisions.Except(Collisions).ToList();

            Collisions = updatedCollisions;

            return newCollisions;

        }
    }
}