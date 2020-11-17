using System;
using System.Collections.Generic;
using TransponderReceiver;

namespace ATM.System
{
    public class TransponderArgs : EventArgs
    {
        public List<TransponderData> transponderData { get; set; }
    }

    public interface IDataFormatter
    {
        event EventHandler<TransponderArgs> transponderChanged;
        
        void StringToTransponderData(object v, RawTransponderDataEventArgs rawTransponderDataEventArgs);
    }
}