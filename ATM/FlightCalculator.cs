using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATM.System
{
    public class FlightCalculator: IFlightCalculator
    {
        //Calculates the speed of a flight based on old and new TransponderData
        public double CalculateSpeed(TransponderData oldData, TransponderData newData)
        {
            int deltaX = oldData.X - newData.X;
            int deltaY = oldData.Y - newData.Y;

            var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
            TimeSpan timespan = newData.Time - oldData.Time;
            double timedif = timespan.TotalMilliseconds;
            return distance/(timedif/1000);
        }

        //Calculates the direction of a flight based on old and new TransponderData
        public double CalculateDirection(TransponderData oldData, TransponderData newData)
        {
            double deltaX = newData.X - oldData.X;
            double deltaY = newData.Y - oldData.Y;

            double toDegreeFactor = 360 / (2 * Math.PI);

            double angle = Math.Atan(deltaY/deltaX)*toDegreeFactor;

            double degree = 0;
            if(newData.X > oldData.X)
            {
                degree = 90 - angle;
            } else if (newData.X < oldData.X)
            {
                degree = 360 - (angle + 90);
            } else
            {
                if(newData.Y > oldData.Y)
                {
                    degree = 0;
                } else if(newData.Y < oldData.Y)
                {
                    degree = 180;
                } else
                {
                    degree = 0;
                }
            }

            return degree;
        }

    }
}
