using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransponderReceiver;

namespace ATM.System
{
    public class DataFormatter: IDataFormatter
    {
        public DataFormatter(TransponderReceiver.ITransponderReceiver transponderReceiver)
        {
            transponderReceiver.TransponderDataReady += StringToTransponderData;
            
        }


        public event EventHandler<TransponderArgs> transponderChanged;

        protected virtual void OnTransponderChanged(TransponderArgs e)
        {
            transponderChanged?.Invoke(this, e);
        }

        public void StringToTransponderData(object sender, TransponderReceiver.RawTransponderDataEventArgs e)
        {
            List<TransponderData> transponderList = new List<TransponderData>();
            foreach(var str in e.TransponderData.ToList()) {
                string[] input = str.Split(';');
                
                if(input.Length != 5)
                {
                    throw new InvalidInputException("String was not of the expected format (Tag;X;Y;Altitude;Timestamp)");
                }

                string tag = input[0];
                int X = int.Parse(input[1]);
                int Y = int.Parse(input[2]);
                int altitude = int.Parse(input[3]);
                DateTime timeStamp = DateTime.ParseExact(input[4], "yyyyMMddHHmmssFFF", null);

                transponderList.Add(new TransponderData(tag, X, Y, altitude, timeStamp));
            }

            OnTransponderChanged(new TransponderArgs { transponderData = transponderList });
        }

    }
}
