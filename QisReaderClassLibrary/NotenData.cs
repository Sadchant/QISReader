using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace QisReaderClassLibrary
{
    [DataContract]
    [KnownTypeAttribute(typeof(NotenData))]
    public class NotenData
    {
        [DataMember]
        public int AnzahlEinträge { get; set; }
        [DataMember]
        public int AnzahlNoten { get; set; }
        [DataMember]
        public DateTime LastRefreshTime { get; set; }

        // extrahiert aus fachListe die für das Refreshen relevante Zahlen und speichert sie und die letzte Aktualisierungszeit
        public void ProcessNotenData(List<Fach> fachListe)
        {
            AnzahlEinträge = fachListe.Count;

            int notenCounter = 0;
            foreach (Fach fach in fachListe)
            {
                if (fach.Note != null)
                    notenCounter++;
            }
            AnzahlNoten = notenCounter;
            LastRefreshTime = DateTime.Now;
        }

        public bool IsEqual(NotenData notenData)
        {
            bool equal = true;
            if (notenData.AnzahlEinträge != AnzahlEinträge || notenData.AnzahlNoten != AnzahlNoten)
                equal = false;
            return equal;
        }
    }
}
