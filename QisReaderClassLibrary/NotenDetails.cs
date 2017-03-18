using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace QisReaderClassLibrary
{
    [DataContract]
    [KnownTypeAttribute(typeof(NotenSpiegelDict))]
    public class NotenSpiegelDict
    {
        [DataMember]
        public List<string> Keys_NotenNamen { get; set; }
        [DataMember]
        public List<NotenDetails> Values_NotenSpiegel { get; set; }
    }

    [DataContract]
    [KnownTypeAttribute(typeof(NotenDetails))]
    public class NotenDetails
    {
        [DataMember]
        public string AktÜberschrift { get; set; }
        [DataMember]
        public List<string> AktDatenBeschriftung { get; set; }
        [DataMember]
        public List<string> AktDatenInhalt { get; set; }
        [DataMember]
        public List<int> Verteilung { get; set; }
        [DataMember]
        public float AktDurchschnitt { get; set; }
        [DataMember]
        public float AktEigeneNote { get; set; } // hier etwas Fehl am Platz, aber sonst keinen besseren Platz gefunden


        public NotenDetails()
        {
            AktDatenBeschriftung = new List<string>();
            AktDatenInhalt = new List<string>();
            Verteilung = new List<int>();
        }
    }
}
