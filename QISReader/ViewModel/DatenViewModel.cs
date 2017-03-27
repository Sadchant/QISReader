using QISReader.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QISReader.ViewModel
{
    public class Zeile
    {
        public string Beschriftung { get; }
        public string Inhalt { get; }

        public Zeile(string beschriftung, string inhalt)
        {
            Beschriftung = beschriftung;
            Inhalt = inhalt;
        }
    }

    public class DatenViewModel
    {
        public List<Zeile> DatenList { get; set; }
        public string Überschrift { get; set; }

        public void Init(List<string> datenBeschriftungList, List<string> datenInhaltList, string überschrift)
        {
            DatenList = new List<Zeile>();
            for (int i = 0; i < datenBeschriftungList.Count; i++)
            {
                DatenList.Add(new Zeile(datenBeschriftungList[i], datenInhaltList[i]));
            }
            Überschrift = überschrift;
        }
    }
}
