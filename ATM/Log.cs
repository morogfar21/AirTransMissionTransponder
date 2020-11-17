using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATM.System;
using NUnit.Framework.Internal;

namespace ATM.System
{
    public class Log : ILog
    {
        // Fil angivelse på skrivebord.
        public static string LogFile =
            Environment.GetFolderPath(
                Environment.SpecialFolder.Desktop) + "\\Log.txt";

        // tid + txt

        public Log(ICollisionDetector collisionDetector)
        {
            collisionDetector.NewCollision += Logwriter;
        }

        public void Logwriter(object sender, CollisionArgs e)
        {
            var col = e.Collision;

            File.AppendAllText(LogFile, CollisionToLogString(col));
            File.AppendAllText(LogFile, "\n");
        }

        public string CollisionToLogString(Collision collision)
        {
            return DateTime.Now + ": " + collision.ToString();
        }
    }
}