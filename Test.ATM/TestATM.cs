using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ATM;
using ATM.System;
using Castle.Core.Smtp;
using NSubstitute;
using TransponderReceiver;

namespace Test.ATM
{

    [TestFixture]
    public class TestATM
    {
        #region Flight

        [TestFixture]
        public class FlightUnitTest
        {
            [Test]
            public void Flight_CompareTwoFlights_ExpectTrue()
            {
                //arrange and act
                Flight flightA = new Flight(new TransponderData("TEST", 0, 0, 0, DateTime.Now));
                Flight flightB = new Flight(new TransponderData("TEST", 0, 0, 0, DateTime.Now));

                //assert
                Assert.That(flightA.Equals(flightB), Is.True);
            }

            [Test]
            public void Flight_CompareTwoFlights_ExpectFalse()
            {
                //arrange and act
                Flight flightA = new Flight(new TransponderData("TEST", 0, 0, 0, DateTime.Now));
                Flight flightB = new Flight(new TransponderData("BEST", 100, 110, 110, DateTime.Now));

                //assert
                Assert.That(flightA.Equals(flightB), Is.False);
            }
        }

        #endregion

        #region FlightCalculator

        public class FlightCalculatorUnitTest
        {
            private FlightCalculator uut;

            [SetUp]
            public void setUp()
            {
                //arrange
                uut = new FlightCalculator();
            }

            [TestCase(10, 40, 70, -100, 156.80)]
            [TestCase(10, 40, 10, 80, 0)]
            [TestCase(10, 40, 10, 10, 180)]
            [TestCase(10, 40, 10, 40, 0)]
            [TestCase(10, 100, 11, 10, 179.36)]
            [TestCase(10, 100, 11, 200, 0.57)]
            [TestCase(0, 0, -1, -100, 180.57)]
            public void CalculateDirection_add2TransponderData_c(int x1, int y1, int x2, int y2, double c)
            {
                //arrange
                TransponderData t1 = new TransponderData("TEST", x1, y1, 100, new DateTime());
                TransponderData t2 = new TransponderData("TEST1", x2, y2, 100, new DateTime());

                //act
                double degree = uut.CalculateDirection(t1, t2);

                //assert
                Assert.That(degree, Is.InRange(c, c + 0.01));
            }

            [TestCase(0, 0, 3, 4, 1, 5)]
            [TestCase(0, 0, 100, 100, 1, 141.42)]
            [TestCase(0, 0, 100, 100, 2, 70.71)]
            [TestCase(0, 0, -100, -100, 1, 141.42)]
            [TestCase(0, 0, -100, -100, 2, 70.71)]
            [TestCase(0, 0, 0, 0, 2, 0)]
            [TestCase(0, 0, 30000, 30000, 1, 42426.40)]
            public void CalculateSpeed_add2TransponderData_c(int x1, int y1, int x2, int y2, int time, double c)
            {
                //arrange
                TransponderData t1 = new TransponderData("TEST1", x1, y1, 100, new DateTime(2019, 10, 20, 10, 10, 0, 0));
                var t2 = new TransponderData("TEST2", x2, y2, 100, new DateTime(2019, 10, 20, 10, 10, time, 0));

                //act
                double degree = uut.CalculateSpeed(t1, t2);

                //assert
                Assert.That(degree, Is.InRange(c, c + 0.01));
            }

        }

        #endregion

        #region FlightCollection
        [TestFixture]
        public class FlightCollectionUnitTest
        {
            private IFlightCollection uut;
            private IFlightCalculator stubCalculator;
            private IFlightFilter stubFilter;
            private ICollisionDetector stubCollisionDetector;
            private FlightArgs receivedArgs;
            private FlightArgs setArgs;
            private int numberOfFlightsChangedEvents;

            internal class StubCalculator: IFlightCalculator
            {
                public StubCalculator()
                {

                }

                public double CalculateSpeed(TransponderData t1, TransponderData t2)
                {
                    return 5;
                }

                public double CalculateDirection(TransponderData t1, TransponderData t2)
                {
                    return 5;
                }
            }
            [SetUp]
            public void SetUp()
            {
                //arrange
                stubCalculator = new StubCalculator();
                stubFilter = Substitute.For<IFlightFilter>();
                stubCollisionDetector = Substitute.For<ICollisionDetector>();
                uut = new FlightCollection(stubCalculator, stubFilter,stubCollisionDetector);
                receivedArgs = null;
                setArgs = new FlightArgs();
                setArgs.flights = new List<Flight>();

                TransponderData td = new TransponderData("aaa",0,0,0,new DateTime());
                Flight f = new Flight(td);
                stubCollisionDetector.OnFlightsChanged(Arg.Any<FlightArgs>()).ReturnsForAnyArgs(setArgs.flights);


                numberOfFlightsChangedEvents = 0;

                uut.flightsChanged += (s, e) =>
                {
                    receivedArgs = e;
                    numberOfFlightsChangedEvents++;
                };
            }

            [Test]
            public void FlightCollection_ReceiveData_RaisesEvent()
            {
                //arrange
                List<TransponderData> testList = new List<TransponderData>();
                TransponderData testData = new TransponderData("TEST", 0, 0, 0, DateTime.Now);
                testList.Add(testData);

                //act
                stubFilter.transponderFilterChanged += Raise.EventWith(this, new TransponderArgs() { transponderData = testList });

                //assert
                Assert.That(numberOfFlightsChangedEvents, Is.EqualTo(1));
            }

            [Test]
            public void FlightCollection_ReceiveData_RaiseEventWithExpectedContent()
            {
                //arrange
                List<TransponderData> testList = new List<TransponderData>();
                TransponderData testData = new TransponderData("TEST", 0, 0, 0, DateTime.Now);
                testList.Add(testData);
                Flight expectedFlight = new Flight(testData);
                setArgs.flights.Add(expectedFlight);
                
                //act
                stubFilter.transponderFilterChanged += Raise.EventWith(this, new TransponderArgs() { transponderData = testList });
                

                //assert
                Assert.That(receivedArgs.flights[0].Equals(expectedFlight),Is.True);
            }

            [Test]
            public void FlightCollection_ReceiveSameDataTwice_SpeedAndDirectionSet()
            {
                //arrange
                List<TransponderData> testList = new List<TransponderData>()
                {
                    new TransponderData("TEST", 0, 0, 0, DateTime.Now)
                };

                //act
                stubFilter.transponderFilterChanged += Raise.EventWith(this, new TransponderArgs() { transponderData = testList });
                stubFilter.transponderFilterChanged += Raise.EventWith(this, new TransponderArgs() { transponderData = testList });

                //assert
                Assert.That(receivedArgs.flights[0].Speed, Is.EqualTo(5));
                Assert.That(receivedArgs.flights[0].Direction, Is.EqualTo(5));
            }

            [Test]
            public void FlightCollection_ReceiveMultipleFlights_RaisedEventContainsNoDuplicates()
            {
                //arrange
                List<TransponderData> testList = new List<TransponderData>()
                {
                    new TransponderData("TEST", 0, 0, 0, DateTime.Now),
                    new TransponderData("TEST", 0, 0, 0, DateTime.Now),
                    new TransponderData("OTHERTEST", 0, 0, 0, DateTime.Now)
                };

                //act
                stubFilter.transponderFilterChanged += Raise.EventWith(this, new TransponderArgs() { transponderData = testList });

                //assert
                Assert.That(receivedArgs.flights.Count, Is.EqualTo(2));
            }

            [Test]
            public void FlightCollection_ReceiveSameTwoFlightsTwiceButWithDifferentCoordinates_RaisedEventContainsTwoFlights()
            {
                List<TransponderData> testList1 = new List<TransponderData>()
                {
                    new TransponderData("TEST", 0, 0, 0, DateTime.Now),
                    new TransponderData("OTHERTEST", 0, 0, 0, DateTime.Now)
                };

                List<TransponderData> testList2 = new List<TransponderData>()
                {
                    new TransponderData("TEST", 500, 500, 500, DateTime.Now),
                    new TransponderData("OTHERTEST", 500, 500, 500, DateTime.Now.AddSeconds(10))
                };

                List<TransponderData> testList3 = new List<TransponderData>()
                {
                    new TransponderData("OTHERTEST", 500, 500, 500, DateTime.Now),
                    new TransponderData("TEST", 1000, 1000, 1000, DateTime.Now.AddSeconds(20))
                };

                stubFilter.transponderFilterChanged += Raise.EventWith(this, new TransponderArgs() { transponderData = testList1 });
                stubFilter.transponderFilterChanged += Raise.EventWith(this, new TransponderArgs() { transponderData = testList2 });
                stubFilter.transponderFilterChanged += Raise.EventWith(this, new TransponderArgs() { transponderData = testList3 });

                Assert.That(receivedArgs.flights.Count, Is.EqualTo(2));
            }
        }

        #endregion

        #region Render
        public class RenderUnitTest
        {
            [SetUp]
            public void SetUp()
            {

            }

            [Test]
            public void RenderFlights_renderflighta_b()
            {
                //arrange
                var stubFlightFilter = Substitute.For<IFlightFilter>();
                var stubFlightCalculator = Substitute.For<IFlightCalculator>();
                var stubFlightCollision = Substitute.For<ICollisionDetector>();
                var stubFlightCollection = Substitute.For<IFlightCollection>();
                var stubConsole = Substitute.For<IConsole>();

                var uutRender = new Render(stubFlightCollection,stubConsole, false);

                List<Flight> resultListe = null;
                List<Flight> flightliste = new List<Flight>();
                var flight = new Flight(new TransponderData("TTT", 1, 1, 1, new DateTime(2017, 10, 5, 23, 10, 45, 666)));
                flight.Direction = 100;
                flight.Speed = 50;

                flightliste.Add(flight);

                uutRender.flightsChanged += (o, e) => { resultListe = e.flights;};

                //act
                uutRender.RenderFlights(this,new FlightArgs(){flights = flightliste});

                //assert
                Assert.That(resultListe[0],Is.EqualTo(flightliste[0]));
            }

            [Test]
            public void flightsChanged_addArgumentAndRaiseEvent_rasiedEvent()
            {
                //arrange
                var stubFlightCollection = Substitute.For<IFlightCollection>();
                var stubConsole = Substitute.For<IConsole>();
                var uutRender = new Render(stubFlightCollection, stubConsole, false);

                List<Flight> resultListe = null;

                List<Flight> flightliste = new List<Flight>();
                var flight = new Flight(new TransponderData("TTT", 1, 1, 1, new DateTime(2017, 10, 5, 23, 10, 45, 666)));
                flight.Direction = 100;
                flight.Speed = 50;

                flightliste.Add(flight);

                //act
                uutRender.flightsChanged += (o, e) => { resultListe = e.flights; };
                stubFlightCollection.flightsChanged += Raise.EventWith(this, new FlightArgs() {flights = flightliste});
                
                //assert
                Assert.That(resultListe[0],Is.EqualTo(flightliste[0]));
            }

            

        }
        #endregion

        #region DataFormatter
        public class DataFormatterUnitTest
        {
            private DataFormatter uut;
            private ITransponderReceiver fakeReceiver;
            private IFlightFilter fakeFilter;

            [SetUp]
            public void SetUp()
            {
                fakeReceiver = Substitute.For<ITransponderReceiver>();
                fakeFilter = Substitute.For<IFlightFilter>();

                uut = new DataFormatter(fakeReceiver);
            }

            [TestCase("TTT10;10000;30000;14000;20101006213456789", 10000)]
            [TestCase("TTT10;10001;30000;14000;20101006213456789", 10001)]
            [TestCase("TTT10;9999;30000;14000;20101006213456789", 9999)]
            [TestCase("TTT10;90000;30000;14000;20101006213456789", 90000)]
            [TestCase("TTT10;90001;30000;14000;20101006213456789", 90001)]
            [TestCase("TTT10;89999;30000;14000;20101006213456789", 89999)]
            public void StringToTransponderData_addStringaToGetXKoor_b(string a, int b)
            {
                //Arrange
                List<string> flightList = new List<string>();
                flightList.Add(a);

                List<TransponderData> resultList = null;

                fakeReceiver.TransponderDataReady += uut.StringToTransponderData;

                uut.transponderChanged += (o, e) => { resultList = e.transponderData; };

                //Act
                fakeReceiver.TransponderDataReady += Raise.EventWith(this, new RawTransponderDataEventArgs(flightList));

                //Assert
                Assert.That(resultList[0].X, Is.EqualTo(b));
            }

            [TestCase("TTT10;90001;30000;14000")]
            [TestCase("TTT10;89999;30000;14000;20101006213456789;100")]
            public void StringToTransponderData_addAWrongString_exception(string a)
            {
                //Arrange
                List<string> flightList = new List<string>();
                flightList.Add(a);

                //act and assert
                Assert.That(() => uut.StringToTransponderData(this, new RawTransponderDataEventArgs(flightList)), Throws.TypeOf<InvalidInputException>());
            }
        }

        


        #endregion

        #region Log
        [TestFixture]
        public class LogUnitTest
        {
            private Log uut;
            private ICollisionDetector _fakeCollisionDetector;

            [SetUp]
            public void SetUp()
            {
                //arrange
                _fakeCollisionDetector = Substitute.For<ICollisionDetector>();
                uut = new Log(_fakeCollisionDetector);
            }

            [TearDown]
            public void TearDown()
            {
                if (File.Exists(Log.LogFile))
                {
                    File.Delete(Log.LogFile);
                }
            }

            [Test]
            public void ReadLine_addedCollision_LoggerPrintsToLastLineOfLogFile()
            {
                //arrange
                Flight flightA = new Flight(new TransponderData("ABC123", 0, 0, 0, DateTime.Now));
                Flight flightB = new Flight(new TransponderData("CAT234", 0, 0, 0, DateTime.Now));
                Collision newCollision = new Collision(flightA,flightB);

                //act
                _fakeCollisionDetector.NewCollision += Raise.EventWith(this,
                    new CollisionArgs(new Collision(flightA, flightB)));

                string lastLineInLog = File.ReadLines(Log.LogFile).Last();
                string expectedText = uut.CollisionToLogString(newCollision);

                //assert
                Assert.That(lastLineInLog.Equals(expectedText));
            }

            [Test]
            public void LogFile_addedCollision_LoggerCreatesLogFile()
            {
                //arrange
                Flight flightA = new Flight(new TransponderData("ABC123", 0, 0, 0, DateTime.Now));
                Flight flightB = new Flight(new TransponderData("CAT234", 0, 0, 0, DateTime.Now));
                Collision newCollision = new Collision(flightA, flightB);

                //act
                _fakeCollisionDetector.NewCollision += Raise.EventWith(this,
                    new CollisionArgs(new Collision(flightA, flightB)));

                //assert
                Assert.That(File.Exists(Log.LogFile));
            }

        }
        #endregion

        #region FlightFilter

        
        public class FlightFilterUnitTest
        {
            private FlightFilter uut;
            private IDataFormatter fakeDataFormatter;
            private IFlightCollection fakeFlightCollection;

            [SetUp]
            public void SetUp()
            {
                //arrange
                fakeDataFormatter = Substitute.For<IDataFormatter>();
                fakeFlightCollection = Substitute.For<IFlightCollection>();

                uut = new FlightFilter(fakeDataFormatter);

            }


            //--Lower and higher valid boundaries
            [TestCase(10000, 10000, 500, 1)]
            [TestCase(10000, 10000, 20000, 1)]

            [TestCase(90000, 10000, 500, 1)]
            [TestCase(90000, 10000, 20000, 1)]

            [TestCase(10000, 90000, 500, 1)]
            [TestCase(10000, 90000, 20000, 1)]

            [TestCase(90000, 90000, 500, 1)]
            [TestCase(90000, 90000, 20000, 1)]

            //----lower invalid boundary
            [TestCase(10000, 10000, 499, 0)]
            [TestCase(10000, 10000, 500, 1)]

            [TestCase(9999, 10000, 499, 0)]
            [TestCase(9999, 10000, 500, 0)]

            [TestCase(10000, 9999, 499, 0)]
            [TestCase(10000, 9999, 500, 0)]

            [TestCase(9999, 9999, 499, 0)]
            [TestCase(9999, 9999, 500, 0)]

            //------ higher invalid boundary
            [TestCase(90000, 90000, 20001, 0)]
            [TestCase(90000, 90000, 20000, 1)]

            [TestCase(90001, 90000, 20001, 0)]
            [TestCase(90001, 90000, 20000, 0)]

            [TestCase(90000, 90001, 20001, 0)]
            [TestCase(90000, 90001, 20000, 0)]

            [TestCase(90001, 90001, 20001, 0)]
            [TestCase(90001, 90001, 20000, 0)]

            public void transponderFilterChanged_raiseEvent_FilterFlightCalled(int x, int y, int altitude, int numberOfValidFlights)
            {
                //arrange
                List<TransponderData> testList = new List<TransponderData>();
                TransponderData testData = new TransponderData("TTT", x, y, altitude, new DateTime(2017, 10, 14, 15, 20, 45, 333));
                testList.Add(testData);

                List<TransponderData> resultList = null;
                
                //act
                uut.transponderFilterChanged += (o, e) => { resultList = e.transponderData; };
                fakeDataFormatter.transponderChanged += Raise.EventWith(this, new TransponderArgs() { transponderData = testList });

                //assert
                Assert.That(resultList.Count, Is.EqualTo(numberOfValidFlights));
            }
        }
        #endregion

        #region Collision

        public class CollisionUnitTest
        {
            [SetUp]
            public void SetUp()
            {

            }

            [TestCase("ABC", "DEF", "ABC", "DEF", true)]
            [TestCase("ABC", "DEF", "DEF", "ABC", true)]
            [TestCase("DEF", "ABC", "ABC", "DEF", true)]
            [TestCase("DEF", "ABC", "DEF", "ABC", true)]
            [TestCase("ABC", "DEF", "ABC", "123", false)]
            [TestCase("ABC", "DEF", "123", "DEF", false)]
            [TestCase("ABC", "123", "ABC", "DEF", false)]
            [TestCase("123", "DEF", "ABC", "DEF", false)]
            public void Collision_CompareCollisionWithTagsAandBToCollisionWithTagCandD_expected(string tagA, string tagB, string tagC, string tagD, bool expected)
            {
                //arrange
                TransponderData dataA = new TransponderData(tagA, 0, 0, 0, DateTime.Now);
                TransponderData dataB = new TransponderData(tagB, 0, 0, 0, DateTime.Now);
                TransponderData dataC = new TransponderData(tagC, 0, 0, 0, DateTime.Now);
                TransponderData dataD = new TransponderData(tagD, 0, 0, 0, DateTime.Now);
                
                //act
                Collision collisionA = new Collision(new Flight(dataA),new Flight(dataB));
                Collision collisionB = new Collision(new Flight(dataC),new Flight(dataD));

                //assert
                bool compare = collisionA.Equals(collisionB);
                Assert.That(compare, Is.EqualTo(expected));
            }
        }
        #endregion

        #region CollisionCollection
        public class CollisionCollectionUnitTest
        {
            [SetUp]
            public void SetUp()
            {

            }

            [Test]
            public void Collisions_callContructor_DefaultConstructorListIsEmpty()
            {
                //arrange and act
                CollisionCollection uut = new CollisionCollection();
                
                //assert
                Assert.That(uut.Collisions.Count,Is.Zero);
            }

            [Test]
            public void Collisions_addMultipleFlights_ListConstructorListNotEmpty()
            {
                //arrange
                TransponderData dummyData = new TransponderData("ABC123", 5000, 5000, 5000, DateTime.Now);
                Flight flightA = new Flight(dummyData);
                Flight flightB = new Flight(dummyData);
                Collision testCollision = new Collision(flightA,flightB);

                List<Collision> testList = new List<Collision>();
                testList.Add(testCollision);
                testList.Add(testCollision);
                testList.Add(testCollision);
                
                //act
                CollisionCollection uut = new CollisionCollection(testList);

                //assert
                Assert.That(uut.Collisions.Count,Is.GreaterThan(0));
            }
        }
        #endregion

        #region CollisionDetector
        [TestFixture]
        public class CollisionDetectorUnitTest
        {
            private CollisionDetector uut;
            private IFlightCollection _fakeFlightCollection;
            private CollisionArgs receivedArgs;
            private int numberOfCollisionEvents;

            [SetUp]
            public void SetUp()
            {
                //arrange
                uut = new CollisionDetector(new CollisionCollection());

                receivedArgs = null;
                numberOfCollisionEvents = 0;

                uut.NewCollision += (s, a) =>
                {
                    receivedArgs = a;
                    numberOfCollisionEvents++;
                };
            }

            [Test]
            public void OnFlightsChanged_addMultipleFlights_CollisionEmits()
            {
                //arrange
                List<Flight> testFlights = new List<Flight>
                {
                    new Flight(new TransponderData("ABC123", 4900, 5000, 2000, DateTime.Now)),
                    new Flight(new TransponderData("BOB123", 5100, 5000, 2100, DateTime.Now))
                };

                //act
                uut.DetectCollisions(testFlights);
                //assert
                Assert.That(numberOfCollisionEvents, Is.EqualTo(1));
            }

            [Test]
            public void OnFlightsChanged_addMultipleFlights_CollisionEmitsOnlyOnce()
            {
                //arrange
                List<Flight> testFlights = new List<Flight>
                {
                    new Flight(new TransponderData("ABC123", 4900, 5000, 2000, DateTime.Now)),
                    new Flight(new TransponderData("BOB123", 5100, 5000, 2100, DateTime.Now))
                };

                //act
                uut.OnFlightsChanged(new FlightArgs() {flights = testFlights});
                uut.OnFlightsChanged(new FlightArgs() {flights = testFlights});

                //assert
                Assert.That(numberOfCollisionEvents, Is.EqualTo(1));

            }

            [Test]
            public void OnFlightsChanged_addMultipleFlights_PassingFlightsEmitOnlyOnce()
            {
                //arrange
                List<Flight> testFlights = new List<Flight>
                {
                    new Flight(new TransponderData("ABC123", 4900, 5000, 2000, DateTime.Now)),
                    new Flight(new TransponderData("BOB123", 5100, 5000, 2100, DateTime.Now))
                };

                //act
                uut.OnFlightsChanged(new FlightArgs() { flights = testFlights });

                testFlights.Clear();
                testFlights.Add(new Flight(new TransponderData("ABC123", 5100, 5000, 2000, DateTime.Now)));
                testFlights.Add(new Flight(new TransponderData("BOB123", 4900, 5000, 2100, DateTime.Now)));

                uut.OnFlightsChanged(new FlightArgs() { flights = testFlights });

                //assert
                Assert.That(numberOfCollisionEvents, Is.EqualTo(1));
            }

            [Test]
            public void OnFlightsChanged_addMultipleFlights_ThreeFlightsInCollisionEmitsThrice()
            {
                //arrange
                List<Flight> testFlights = new List<Flight>
                {
                    new Flight(new TransponderData("ABC123", 4900, 5000, 2000, DateTime.Now)),
                    new Flight(new TransponderData("BOB123", 5100, 5000, 2100, DateTime.Now)),
                    new Flight(new TransponderData("KAT666", 5000, 5000, 2050, DateTime.Now))
                };

                //act
                uut.OnFlightsChanged(new FlightArgs() { flights = testFlights });

                //assert
                Assert.That(numberOfCollisionEvents, Is.EqualTo(3));
            }

            [Test]
            public void OnFlightsChanged_addMultipleFlights_PreviouslyInCollisionNoLongerInCollision()
            {
                //arrange
                List<Flight> testFlights = new List<Flight>
                {
                    new Flight(new TransponderData("ABC123", 4900, 5000, 2000, DateTime.Now)),
                    new Flight(new TransponderData("BOB123", 5100, 5000, 2100, DateTime.Now))
                };

                //act
                uut.OnFlightsChanged(new FlightArgs() { flights = testFlights });

                Collision oldCollision = receivedArgs.Collision;

                testFlights.Clear();
                testFlights.Add(new Flight(new TransponderData("ABC123", 2000, 5000, 2000, DateTime.Now)));
                testFlights.Add(new Flight(new TransponderData("BOB123", 18000, 5000, 2100, DateTime.Now)));

                uut.OnFlightsChanged(new FlightArgs() { flights = testFlights });

                //assert
                Assert.That(uut.Collisions, Does.Not.Contain(oldCollision));

            }

            [Test]
            public void OnFlightsChanged_addMultipleFlights_NoCollisions()
            {
                //arrange
                List<Flight> testFlights = new List<Flight>
                {
                    new Flight(new TransponderData("ABC123", 10000, 45000, 8000, DateTime.Now)),
                    new Flight(new TransponderData("BOB123", 90000, 45000, 6000, DateTime.Now)),
                    new Flight(new TransponderData("BOB123", 45000, 10000, 4000, DateTime.Now)),
                    new Flight(new TransponderData("BOB123", 45000, 90000, 2000, DateTime.Now))
                };

                //act
                uut.OnFlightsChanged(new FlightArgs() { flights = testFlights });

                //assert
                Assert.That(numberOfCollisionEvents, Is.EqualTo(0));
                Assert.That(uut.Collisions, Is.Empty);
            }

            [Test]
            public void OnFlightsChanged_addMultipleFlights_NoCollisionsEdgeCases()
            {
                //arrange
                List<Flight> testFlights = new List<Flight>
                {
                    new Flight(new TransponderData("ABC123", 5000, 45000, 4000, DateTime.Now)),
                    new Flight(new TransponderData("BOB123", 10000, 45000, 4000, DateTime.Now)),
                    new Flight(new TransponderData("BOB123", 10000, 50000, 4000, DateTime.Now)),
                    new Flight(new TransponderData("BOB123", 10000, 50000,4300, DateTime.Now))
                };

                //act
                uut.OnFlightsChanged(new FlightArgs() { flights = testFlights });

                //assert
                Assert.That(numberOfCollisionEvents, Is.EqualTo(0));
                Assert.That(uut.Collisions, Is.Empty);
            }
        }
        #endregion
    }
}
