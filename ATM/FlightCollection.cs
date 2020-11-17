using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ATM.System
{
    public class FlightCollection : IFlightCollection
    {
        public List<Flight> FlightList;
        private IFlightCalculator flightCalculator;
        private ICollisionDetector collisionDetector_;

        public FlightCollection(IFlightCalculator flightCalculator, IFlightFilter flightFilter,ICollisionDetector collisionDetector)
        {
            flightFilter.transponderFilterChanged += GetTransponderData;
            this.FlightList = new List<Flight>();
            this.flightCalculator = flightCalculator;
            this.collisionDetector_ = collisionDetector;

        }

        public void GetTransponderData(object sender,TransponderArgs e)
        {
            var transponderList = e.transponderData;

            foreach(TransponderData transponderData in transponderList)
            {
                HandleNewData(transponderData);
            }
            List<Flight> newlist = collisionDetector_.OnFlightsChanged(new FlightArgs() {flights = FlightList});
            for (int i = 0; i < newlist.Count; i++)
            {
                FlightList[i].collision = newlist[i].collision;
            }
            Notify();
        }

        public event EventHandler<FlightArgs> flightsChanged;

        protected virtual void OnFlightsChanged(FlightArgs e)
        {
            flightsChanged?.Invoke(this,e);
        }

        public void HandleNewData(TransponderData newData)
        {
            

            if (FlightList.Exists(f => f.TData.Tag == newData.Tag))
            {
                Flight flight = FlightList.Find(f => f.TData.Tag == newData.Tag);
                FlightList.Remove(flight);

                flight.Speed = flightCalculator.CalculateSpeed(flight.TData, newData);
                flight.Direction = flightCalculator.CalculateDirection(flight.TData, newData);
                flight.TData = newData;
                FlightList.Add(flight);
            }
            else
            {
                FlightList.Add(new Flight(newData));
            }
        }

        public void Notify()
        {
            OnFlightsChanged(new FlightArgs() { flights = FlightList });
        }

        
    }
}
