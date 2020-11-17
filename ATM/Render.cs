using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATM.System
{
    public class Render : IRender
    {
        private bool renderInGui_ = true;
        private IConsole console_;

        public Render(IFlightCollection col,IConsole console,bool renderInGui=true)
        {
            renderInGui_ = renderInGui;
            console_ = console;
            col.flightsChanged += RenderFlights;
        }


        public void RenderFlights(object sender, FlightArgs e)
        {
            flightsChanged?.Invoke(this, e);

            List<Flight> flights = e.flights;
            console_.Clear();

            for (int i = 0; i < flights.Count; i++)
            {
                Flight f = flights[i];
                if (renderInGui_ && Program.shapes[i] != null)
                    //This is not covered by tests, since we do not test the GUI implementation and this affects the GUI
                    Program.setflight(f.TData.X / 200, f.TData.Y / 200, i,f.collision);

                console_.WriteLine(string.Format("Flight: {0}\n\tPosition X:{1} Y:{2}\tAltitude: {3}\n\tSpeed: {4}\t\tDirection {5}\n\tCollision {6}\n", f.TData.Tag, f.TData.X, f.TData.Y, f.TData.Altitude, f.Speed, f.Direction,f.collision));
            }
            
        }

        

        public event EventHandler<FlightArgs> flightsChanged;
    }
}
