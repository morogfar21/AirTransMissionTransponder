using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TransponderReceiver;

namespace ATM.System
{
    //Excluded from code coverage since we cannot test main.
    [ExcludeFromCodeCoverageAttribute]
    public class Program : Application
    {
        private static Program gui;
        private static Window window;
        private static Canvas can;
        public static Shape[] shapes;

        [STAThread]
        static void Main(string[] args)
        {
            Thread t = new Thread(guimetode);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();

            ITransponderReceiver receiver = TransponderReceiver.TransponderReceiverFactory.CreateTransponderDataReceiver();

            IDataFormatter dataFormatter = new DataFormatter(receiver);
            IFlightFilter flightFilter = new FlightFilter(dataFormatter);
            ICollisionDetector collisionDetector = new CollisionDetector(new CollisionCollection());
            IFlightCollection flightCollection = new FlightCollection(new FlightCalculator(),flightFilter,collisionDetector);
            ILog logger = new Log(collisionDetector);
            IConsole console = new Console();
            IRender render = new Render(flightCollection, console, true);

            t.Join();

            while (true)
            {

            }
        }

        [STAThread]
        static public void guimetode()
        {
            gui = new Program();
            window = new Window();

            var field = new Rectangle();
            field.Stroke = Brushes.Red;
            field.Width = 500;
            field.Height = 500;
            can = new Canvas();
            can.Children.Add(field);
            Canvas.SetTop(field,0);
            Canvas.SetLeft(field, 0);
            window.Content = can;

            shapes = new Ellipse[50];
            for(int i=0;i<50;i++) {
                shapes[i] = new Ellipse();
            shapes[i].Height = 10;
            shapes[i].Width = 10;
            can.Children.Add(shapes[i]);
            can.Background = Brushes.Gray;
            Canvas.SetTop(shapes[i], 100);
            Canvas.SetLeft(shapes[i], 600);
            shapes[i].Fill = Brushes.Green;
            }
            window.Show();
            gui.Run();


        }
        public delegate void koorDelegate();

        public static void setflight(double x, double y,int index,bool collision)
        {
            koorDelegate metode = delegate
            {
                if (collision)
                    shapes[index].Fill = Brushes.Red;
                else
                    shapes[index].Fill = Brushes.Green;
                Canvas.SetTop(shapes[index], y);
                Canvas.SetLeft(shapes[index], x);
            };
            Application.Current.Dispatcher.BeginInvoke(metode);
        }
    }
}
