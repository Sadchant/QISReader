using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace QisReaderClassLibrary
{
    // existiert für jede Note, bei der man auf "info" klicken kann, beinhaltet Detaildaten und Notenspiegel falls vorhanden
    [DataContract]
    [KnownTypeAttribute(typeof(NotenDetails))]
    public class NotenDetails
    {
        [DataMember]
        public string Überschrift { get; set; }
        [DataMember]
        public List<string> DatenBeschriftung { get; set; }
        [DataMember]
        public List<string> DatenInhalt { get; set; }
        [DataMember]
        public List<int> Verteilung { get; set; }
        [DataMember]
        public float Durchschnitt { get; set; }
        [DataMember]
        public float EigeneNote { get; set; } // hier etwas Fehl am Platz, aber sonst keinen besseren Platz gefunden

        public NotenDetails()
        {
            DatenBeschriftung = new List<string>();
            DatenInhalt = new List<string>();
            Verteilung = new List<int>();
        }
    }
}
