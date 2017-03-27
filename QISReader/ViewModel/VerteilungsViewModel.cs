using QISReader.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace QISReader.ViewModel
{
    public class VerteilungsViewModel
    {
        private List<int> verteilung { get; set; }
        private float durchschnitt { get; set; }
        private float eigeneNote { get; set; }

        public float[] NotenBalkenHeights { get; }
        public int[] NotenAnzahlText { get; }
        public Thickness[] NotenAnzahlTextMargins { get; }
        public bool[] NotenAnzahlDrüber { get; }
        public Brush[] NotenAnzahlTextColors { get; }

        public float[] BeschriftungsWerte { get; set; }

        public Thickness DurchschnittMargin { get; set; }
        public Thickness EigeneNoteMargin { get; set; }

        public string DurchschnittText { get; set; }
        public string EigeneNoteText { get; set; }
        public int DurchschnittColumn { get; set; }
        public int EigeneNoteColumn { get; set; }
        public SolidColorBrush DurchschnittColor { get; set; }
        public SolidColorBrush EigeneNoteColor { get; set; }

        private const int NOTENANZAHL = 5;

        private SolidColorBrush whiteBrush = new SolidColorBrush(Colors.White);
        private SolidColorBrush blackBrush = new SolidColorBrush(Colors.DarkGray);

        // Arrays initialisieren
        public VerteilungsViewModel()
        {
            NotenBalkenHeights = new float[NOTENANZAHL];
            NotenAnzahlText = new int[NOTENANZAHL];
            NotenAnzahlDrüber = new bool[NOTENANZAHL]; // bool-Arrays sind nach dem Initialisieren false
            NotenAnzahlTextMargins = new Thickness[NOTENANZAHL];
            NotenAnzahlTextColors = new SolidColorBrush[NOTENANZAHL];
        }

        internal void InsertValues(List<int> verteilung, float durchschnitt, float eigeneNote)
        {
            this.verteilung = verteilung;
            this.durchschnitt = durchschnitt;
            this.eigeneNote = eigeneNote;
        }

        public void Init(bool has3Rows, int maxBeschriftung, float gridHeight, float anzahlBeschriftungAbstandnachOben, float anzahlBeschriftungHeight, float firstGridHeight, ResourceDictionary resources)
        {
            setBeschriftung(has3Rows, maxBeschriftung);
            calculateBalkenData(maxBeschriftung, gridHeight, anzahlBeschriftungAbstandnachOben, anzahlBeschriftungHeight);
            calculateDurchschnittData(firstGridHeight, gridHeight, resources, anzahlBeschriftungAbstandnachOben, anzahlBeschriftungHeight);
            calculateEigeneNoteData(firstGridHeight, gridHeight, resources, anzahlBeschriftungAbstandnachOben, anzahlBeschriftungHeight);
        }

        // füllt das Array für die Beschriftung am linken Rand
        private void setBeschriftung(bool has3Rows, int maxBeschriftung)
        {
            if (has3Rows)
            {
                BeschriftungsWerte = new float[3];
                BeschriftungsWerte[0] = 0;
                BeschriftungsWerte[1] = maxBeschriftung/2;
                BeschriftungsWerte[2] = maxBeschriftung;
            }
            else
            {
                BeschriftungsWerte = new float[4];
                BeschriftungsWerte[0] = 0;
                BeschriftungsWerte[1] = maxBeschriftung / 3;
                BeschriftungsWerte[2] = BeschriftungsWerte[1] * 2;
                BeschriftungsWerte[3] = maxBeschriftung;
            }                
        }

        private void calculateBalkenData(int maxBeschriftung, float gridHeight, float anzahlBeschriftungAbstandnachOben, float anzahlBeschriftungHeight)
        {
            float scaler = gridHeight / maxBeschriftung;
            for(int i=0; i<NOTENANZAHL; i++)
            {
                NotenAnzahlText[i] = verteilung[i];
                float aktBalkenHeight = verteilung[i] * scaler;
                NotenBalkenHeights[i] = aktBalkenHeight;

                // Text für Anzahl-Beschriftung setzen
                NotenAnzahlText[i] = verteilung[i];
                // Farbe und Position der Anzahl pro Note bestimmen:
                float aktNotenAnzahlTopMargin = gridHeight - aktBalkenHeight + anzahlBeschriftungAbstandnachOben;
                NotenAnzahlTextColors[i] = whiteBrush;
                if (aktNotenAnzahlTopMargin + anzahlBeschriftungHeight + anzahlBeschriftungAbstandnachOben > gridHeight)
                {
                    aktNotenAnzahlTopMargin -= (anzahlBeschriftungAbstandnachOben*2 + anzahlBeschriftungHeight);
                    NotenAnzahlTextColors[i] = blackBrush;
                    NotenAnzahlDrüber[i] = true;
                }
                NotenAnzahlTextMargins[i] = new Thickness(0, aktNotenAnzahlTopMargin, 0, 0);
            }
        }

        private void calculateDurchschnittData(float firstGridHeight, float gridHeight, ResourceDictionary resources, float anzahlBeschriftungAbstandnachOben, float anzahlBeschriftungHeight)
        {
            //zuerst die Platzierung des Durchschnitts berechnen
            DurchschnittText = "Durchschnitt: " + durchschnitt;
            float topmargin = firstGridHeight + gridHeight - (anzahlBeschriftungAbstandnachOben*2) - anzahlBeschriftungHeight;
            int index = 0;
            if (durchschnitt >= 1.0 && durchschnitt < 1.3 + 0.15)
            {
                index = 0;
                DurchschnittColumn = 1;
                DurchschnittColor = (SolidColorBrush)resources["sehrgutDarkBrush"];
            }
            else if (durchschnitt >= 1.3 + 0.15 && durchschnitt < 2.3 + 0.15)
            {
                index = 1;
                DurchschnittColumn = 3;
                DurchschnittColor = (SolidColorBrush)resources["gutDarkBrush"];
            }
            else if (durchschnitt >= 2.3 + 0.15 && durchschnitt < 3.3 + 0.15)
            {
                index = 2;
                DurchschnittColumn = 5;
                DurchschnittColor = (SolidColorBrush)resources["befriedigendDarkBrush"];
            }
            else if (durchschnitt >= 3.3 + 0.15 && durchschnitt < 4.0 + 0.15)
            {
                index = 3;
                DurchschnittColumn = 7;
                DurchschnittColor = (SolidColorBrush)resources["ausreichendDarkBrush"];
            }
            else if (durchschnitt >= 4.0 + 0.15)
            {
                index = 4;
                DurchschnittColumn = 9;
                DurchschnittColor = (SolidColorBrush)resources["durchgefallenDarkBrush"];
            }
            topmargin -= NotenBalkenHeights[index]; // die Balkenhöhe muss noch abgezogen werden, da bei Margin ja von oben gezählt wird
            if (NotenAnzahlDrüber[index]) // sollte schon die Zahl für die Anzahl über dem Balken stehen, rücke eins höher
                topmargin -= (anzahlBeschriftungAbstandnachOben + anzahlBeschriftungHeight);
            DurchschnittMargin = new Thickness(0, topmargin, 0, 0);
        }

        private void calculateEigeneNoteData(float firstGridHeight, float gridHeight, ResourceDictionary resources, float anzahlBeschriftungAbstandnachOben, float anzahlBeschriftungHeight)
        {
            //zuerst die Platzierung der eigenen Note berechnen
            EigeneNoteText = "Deine Note: " + eigeneNote;
            float topmargin = firstGridHeight + gridHeight - (anzahlBeschriftungAbstandnachOben*2) - anzahlBeschriftungHeight;
            int index = 0;
            if (eigeneNote >= 1.0 && eigeneNote < 1.3 + 0.15)
            {
                index = 0;
                EigeneNoteColumn = 1;
                EigeneNoteColor = (SolidColorBrush)resources["sehrgutDarkBrush"];
            }
            else if (eigeneNote >= 1.3 + 0.15 && eigeneNote < 2.3 + 0.15)
            {
                index = 1;
                EigeneNoteColumn = 3;
                EigeneNoteColor = (SolidColorBrush)resources["gutDarkBrush"];
            }
            else if (eigeneNote >= 2.3 + 0.15 && eigeneNote < 3.3 + 0.15)
            {
                index = 2;
                EigeneNoteColumn = 5;
                EigeneNoteColor = (SolidColorBrush)resources["befriedigendDarkBrush"];
            }
            else if (eigeneNote >= 3.3 + 0.15 && eigeneNote < 4.0 + 0.15)
            {
                index = 3;
                EigeneNoteColumn = 7;
                EigeneNoteColor = (SolidColorBrush)resources["ausreichendDarkBrush"];
            }
            else if (eigeneNote >= 4.0 + 0.15)
            {
                index = 4;
                EigeneNoteColumn = 9;
                EigeneNoteColor = (SolidColorBrush)resources["durchgefallenDarkBrush"];
            }
            topmargin -= NotenBalkenHeights[index]; // die Balkenhöhe muss noch abgezogen werden, da bei Margin ja von oben gezählt wird
            if (NotenAnzahlDrüber[index]) // sollte schon die Zahl für die Anzahl über dem Balken stehen, rücke eins höher
                topmargin -= (anzahlBeschriftungAbstandnachOben + anzahlBeschriftungHeight);
            if (DurchschnittColumn == EigeneNoteColumn) // wenn schon der Durschnitt in dieser Spalte steht, rücke noch eins höher
                topmargin -= (anzahlBeschriftungAbstandnachOben + anzahlBeschriftungHeight);
            EigeneNoteMargin = new Thickness(0, topmargin, 0, 0);
        }


    }
}
