using QISReader.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QISReader.ViewModel
{
    public class BestandenViewModel
    {
        private NotenParser globalNotenParser = App.LogicManager.NotenParser;
        public string BestandenText { get; }
        public string NichtBestandenText { get; }
        public string TeilnehmerText { get; }
        public float NichtBestandenRectWidth { get; set; }

        private int bestandenCount;
        private int nichtBestandenCount;
        private int teilnehmer;


        public BestandenViewModel()
        {
            var bestandenAnzahl = globalNotenParser.AktNotenSpiegel.AktVerteilung.Take(globalNotenParser.AktNotenSpiegel.AktVerteilung.Count - 1);
            foreach(int notenAnzahl in bestandenAnzahl)
            {
                bestandenCount += notenAnzahl;
            }
            nichtBestandenCount = globalNotenParser.AktNotenSpiegel.AktVerteilung.Last();
            teilnehmer = bestandenCount + nichtBestandenCount;

            float percentScaler = 100.0f / teilnehmer;
            BestandenText = "Bestanden: " + bestandenCount + " (" + Math.Round((Decimal)(bestandenCount * percentScaler), 1, MidpointRounding.AwayFromZero)  + "%)";
            NichtBestandenText = "Nicht Bestanden: " + nichtBestandenCount + " (" + Math.Round((Decimal)(nichtBestandenCount * percentScaler), 1, MidpointRounding.AwayFromZero) + "%)";
            TeilnehmerText = "Teilnehmer: " + teilnehmer;
        }

        public void Init(float gridWidth)
        {
            NichtBestandenRectWidth = gridWidth / teilnehmer * nichtBestandenCount;
        }
    }
}
