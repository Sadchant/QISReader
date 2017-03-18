using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace QisReaderClassLibrary
{
    [DataContract]
    [KnownTypeAttribute(typeof(FachHeader))]
    [KnownTypeAttribute(typeof(FachInhalt))]
    public class Fach
    {
    }

    [DataContract]
    [KnownTypeAttribute(typeof(FachHeader))]
    public class FachHeader : Fach
    {
        // vorhanden gibt an, ob die Membervariable in der QIS-Tabelle überhaupt angegeben wurde, wenn nicht bleibt sie leer
        [DataMember]
        public bool[] Vorhanden { get; set; } // werden in c# mit false initialisiert, müssen danach also nach Bedarf nur auf true gesetzt werden
        [DataMember]
        public int Id { get; set; } // 0
        [DataMember]
        public string FachName { get; set; } // 1
        [DataMember]
        public float Note { get; set; } // 2
        [DataMember]
        public bool Bestanden { get; set; } // 3
        [DataMember]
        public float Cp { get; set; } // 4

        public FachHeader()
        {
            Vorhanden = new bool[5];
        }
    }

    [DataContract]
    [KnownTypeAttribute(typeof(FachInhalt))]
    public class FachInhalt : Fach
    {
        [DataMember]
        public bool[] Vorhanden { get; set; }
        [DataMember]
        public int Id { get; set; }// 0
        [DataMember]
        public string FachName { get; set; } // 1
        [DataMember]
        public string Semester { get; set; } // 2
        [DataMember]
        public float Note { get; set; } // 3
        [DataMember]
        public bool Bestanden { get; set; } // 4
        [DataMember]
        public float Cp { get; set; } // 5
        [DataMember]
        public int Versuch { get; set; } // 6

        public FachInhalt()
        {
            Vorhanden = new bool[7];
        }
    }
}
