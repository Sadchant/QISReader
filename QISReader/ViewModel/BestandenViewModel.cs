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
        public string BestandenText { get; set; }
        public string NichtBestandenText { get; set; }
        public string TeilnehmerText { get; set; }
        public float NichtBestandenRectWidth { get; set; }

        public void Init(float gridWidth, List<int> verteilung)
        {
            int bestandenCount = 0;
            int nichtBestandenCount;
            int teilnehmer;

            var bestandenAnzahl = verteilung.Take(verteilung.Count - 1);
            foreach (int notenAnzahl in bestandenAnzahl)
            {
                bestandenCount += notenAnzahl;
            }
            nichtBestandenCount = verteilung.Last();
            teilnehmer = bestandenCount + nichtBestandenCount;

            float percentScaler = 100.0f / teilnehmer;
            BestandenText = "Bestanden: " + bestandenCount + " (" + Math.Round((Decimal)(bestandenCount * percentScaler), 1, MidpointRounding.AwayFromZero) + "%)";
            NichtBestandenText = "Nicht Bestanden: " + nichtBestandenCount + " (" + Math.Round((Decimal)(nichtBestandenCount * percentScaler), 1, MidpointRounding.AwayFromZero) + "%)";
            TeilnehmerText = "Teilnehmer: " + teilnehmer;

            NichtBestandenRectWidth = gridWidth / teilnehmer * nichtBestandenCount;
        }
    }
}
