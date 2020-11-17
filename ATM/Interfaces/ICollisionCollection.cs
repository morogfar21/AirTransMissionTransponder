using System.Collections.Generic;

namespace ATM.System
{
    public interface ICollisionCollection
    {
        List<Collision> HandleUpdatedCollisions(List<Collision> newCollisions);

        List<Collision> Collisions { get;}
    }
}