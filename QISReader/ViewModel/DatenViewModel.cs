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
        public List<Zeile> DatenList { get; }
        public string Überschrift { get; }

        private NotenParser globalNotenParser;
        //SPÄTER DATEN NICHT AUS DEM PARSER HOLEN SONDERN AUS DATEI

        public DatenViewModel()
        {
            DatenList = new List<Zeile>();
            globalNotenParser = App.LogicManager.NotenParser;
            NotenSpiegel aktNotenSpiegel = globalNotenParser.AktNotenSpiegel;
            for(int i=0; i < aktNotenSpiegel.AktDatenBeschriftung.Count; i++)
            {
                DatenList.Add(new Zeile(aktNotenSpiegel.AktDatenBeschriftung[i], aktNotenSpiegel.AktDatenInhalt[i]));
            }
            Überschrift = aktNotenSpiegel.AktÜberschrift;
        }
    }
}
