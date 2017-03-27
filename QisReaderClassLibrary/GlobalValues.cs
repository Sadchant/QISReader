using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QisReaderClassLibrary
{
    public class GlobalValues
    {
        public const string SETTINGS_HOCHSCHULE = "hochschule";

        public const int STARTANMELDUNG = 0;
        public const int STARTNOTENNAVIGATION = 1;
        public const int STARTNOTENVERARBEITUNG = 2;
        public const int NOTENVERARBEITUNGFERTIG = 3;

        public const int WRONGLOGIN = 100;
        public const int KEINEVERBINDUNG = 101;
        public const int LOGINFEHLER = 102;
        public const int NOTENNAVIGATIONSFEHLER = 103;
        public const int NOTENVERARBEITUNGFEHLER = 104;

        public const int NOTENSPIEGELPROGRESSSTART = 200;

        public const string FILE_NOTENDATA = "notendata.json";
        public const string FILE_NOTEN = "noten.json";
        public const string FILE_NOTENDETAILS = "notendetails.json";
        public const string FILE_DETAILSDICTIONARY = "detailsdictionary.json";

        public const string HOCHSCHULDICTFILENAME = "hochschuldict.json";

        public const string SETTINGS_AUTOUPDATE = "autoupdate";
        public const string SETTINGS_UPDATERATE = "updaterate";

        public const uint UPDATERATE_ALLE_30_MINUTEN = 30;
        public const uint UPDATERATE_EINMAL_PRO_STUNDE = 60;
        public const uint UPDATERATE_ALLE_2_STUNDEN = 120;
        public const uint UPDATERATE_ALLE_6_STUNDENN = 360;
        public const uint UPDATERATE_EINMAL_PRO_TAG = 1440;
    }
}
