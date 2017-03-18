using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QisReaderClassLibrary
{
    public class Globals
    {
        public const string HOCHSCHULE = "hochschule";

        public const int START = 0;
        public const int EINGELOGGT = 1;
        public const int NAVIGIERT = 2;
        public const int VERARBEITET = 3;

        public const int WRONGLOGIN = 100;
        public const int NOCONNECTION = 101;
        public const int LOGINERROR = 102;
        public const int NOTENNAVIGATIONERROR = 103;
        public const int NOTENPROCESSINGERROR = 104;

        public const int NOTENSPIEGELPROGRESSSTART = 200;
    }
}
