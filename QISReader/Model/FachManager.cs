using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QISReader.Model
{
    public class Fach
    {        
    }

    public class FachHeader : Fach
    {
        // vorhanden gibt an, ob die Membervariable in der QIS-Tabelle überhaupt angegeben wurde, wenn nicht bleibt sie leer
        public bool[] Vorhanden { get; set; } // werden in c# mit false initialisiert, müssen danach also nach Bedarf nur auf true gesetzt werden
        public int Id { get; set; } // 0
        public string FachName { get; set; } // 1
        public float Note { get; set; } // 2
        public bool Bestanden { get; set; } // 3
        public float Cp { get; set; } // 4

        public FachHeader()
        {
            Vorhanden = new bool[5];
        }
    }

    public class FachInhalt : Fach
    {
        public bool[] Vorhanden { get; set; }
        public int Id { get; set; }// 0
        public string FachName { get; set; } // 1
        public string Semester { get; set; } // 2
        public float Note { get; set; } // 3
        public bool Bestanden { get; set; } // 4
        public float Cp { get; set; } // 5
        public int Versuch { get; set; } // 6

        public FachInhalt()
        {
            Vorhanden = new bool[7];
        }
    }

    public class FachManager
    {
        internal List<Fach> FachListe { get; set; }

        public FachManager()
        {
            FachListe = new List<Fach>();
        }

        // erzeugt aus Liste von Strings, von dem jedes Element ein Fach darstellt, eine Liste von Fach-Objekten, entweder FachHeader oder FachInhalt und speichert das Ergebnis in FachListe
        public void buildFachObjektList(List<string> fachStringList)
        {
            int memberId = 0; // dient dazu, im boolean-Feld festzulegen, ob die Variable im Fach existiert oder nicht
            foreach(string aktFachString in fachStringList) // iteriert über jeden String in der Liste und generiert ein Fach-Objekt daraus
            {
                string[] teile = aktFachString.Split(';'); // Größe der Liste zeigt an, ob es ein FachHeader oder ein Fachinhalt ist

                if (teile.Length == 5) // Fachüberschrift
                {
                    FachHeader fachHeader = new FachHeader();
                    memberId = 0;
                    if (!string.IsNullOrEmpty(teile[memberId]))
                    {
                        fachHeader.Vorhanden[memberId] = true;
                        fachHeader.Id = int.Parse(teile[memberId]);
                    }
                    memberId = 1;
                    if (!string.IsNullOrEmpty(teile[memberId]))
                    {
                        fachHeader.Vorhanden[memberId] = true;
                        fachHeader.FachName = teile[memberId];
                    }
                    memberId = 2;
                    if (!string.IsNullOrEmpty(teile[memberId]))
                    {
                        fachHeader.Vorhanden[memberId] = true;
                        fachHeader.Note = float.Parse(teile[memberId]);
                    }
                    memberId = 3;
                    if (!string.IsNullOrEmpty(teile[memberId]))
                    {
                        fachHeader.Vorhanden[memberId] = true;
                        if (teile[memberId] == "bestanden")
                            fachHeader.Bestanden = true;
                        else if (teile[memberId] == "nicht bestanden")
                            fachHeader.Bestanden = false;
                        else
                            fachHeader.Vorhanden[memberId] = false;
                    }
                    memberId = 4;
                    if (!string.IsNullOrEmpty(teile[memberId]))
                    {
                        fachHeader.Vorhanden[memberId] = true;
                        fachHeader.Cp = float.Parse(teile[memberId]);
                    }

                    /*fachHeader.FachName = teile[1];
                    if (string.IsNullOrEmpty(teile[2])) fachHeader.Note = -1;
                    if (teile[3] == "bestanden") fachHeader.Bestanden = true; else fachHeader.Bestanden = false;
                    fachHeader.Cp = float.Parse(teile[4]);*/

                    FachListe.Add(fachHeader);
                }
                else if (teile.Length == 7) // Fachinhalt
                {
                    FachInhalt fachInhalt = new FachInhalt();
                    memberId = 0;
                    if (!string.IsNullOrEmpty(teile[memberId]))
                    {
                        fachInhalt.Vorhanden[memberId] = true;
                        fachInhalt.Id = int.Parse(teile[memberId]);
                    }
                    memberId = 1;
                    if (!string.IsNullOrEmpty(teile[memberId]))
                    {
                        fachInhalt.Vorhanden[memberId] = true;
                        fachInhalt.FachName = teile[memberId];
                    }
                    memberId = 2;
                    if (!string.IsNullOrEmpty(teile[memberId]))
                    {
                        fachInhalt.Vorhanden[memberId] = true;
                        fachInhalt.Semester = teile[2];
                    }
                    memberId = 3;
                    if (!string.IsNullOrEmpty(teile[memberId]))
                    {
                        fachInhalt.Vorhanden[memberId] = true;
                        fachInhalt.Note = float.Parse(teile[memberId]);
                    }
                    memberId = 4;
                    if (!string.IsNullOrEmpty(teile[memberId]))
                    {
                        fachInhalt.Vorhanden[memberId] = true;
                        if (teile[memberId] == "bestanden")
                            fachInhalt.Bestanden = true;
                        else
                            fachInhalt.Bestanden = false;
                    }
                    memberId = 5;
                    if (!string.IsNullOrEmpty(teile[memberId]))
                    {
                        fachInhalt.Vorhanden[memberId] = true;
                        fachInhalt.Cp = float.Parse(teile[memberId]);
                    }
                    memberId = 6;
                    if (!string.IsNullOrEmpty(teile[memberId]))
                    {
                        fachInhalt.Vorhanden[memberId] = true;
                        fachInhalt.Versuch = int.Parse(teile[6]);
                    }
                    /*fachInhalt.Id = int.Parse(teile[0]);
                    fachInhalt.FachName = teile[1];
                    fachInhalt.Semester = teile[2];
                    fachInhalt.Note = float.Parse(teile[3]);
                    if (teile[4] == "bestanden") fachInhalt.Bestanden = true; else fachInhalt.Bestanden = false;
                    fachInhalt.Cp = float.Parse(teile[5]);
                    fachInhalt.Versuch = int.Parse(teile[6]);*/
                    FachListe.Add(fachInhalt);
                }
            }
        }

        // die App soll nur "Versuch 1" anzeigen, wenn es einen zweiten Versuch gibt oder ein Fach nicht bestanden wurde und es noch keinen zweiten Versuch gibt
        // diese Funktion gibt ein Array aus booleans zurück, die so lang ist wie die Fachliste mit trues an der Stelle, wo der Versuch angezeigt werden soll
        public bool[] getVersucheToShow()
        {
            bool[] versucheToShow = new bool[FachListe.Count];

            if (FachListe == null || FachListe.Count == 0)
            {
                Debug.WriteLine("Liste ist leer, Funktion zu früh aufgerufen!");
                return null;
            }
            int i = 0, j = 0;
            foreach (Fach aktFach in FachListe)
            {
                if (aktFach is FachInhalt)
                {
                    FachInhalt aktFachInhalt = (FachInhalt)aktFach;
                    if (aktFachInhalt.Versuch > 1) // wenn der Versuch 2 oder größer ist, soll auf jeden Fall "Versuch 2" ... angezeigt werden
                    {
                        versucheToShow[i] = true; // also setze den aktuellen Index auf true
                        j = 0;
                        foreach (Fach secondAktFach in FachListe) // und den dazugehörigen ersten Versuch suchen
                        {                            
                            if (secondAktFach is FachInhalt)
                            {
                                if (string.Equals(((FachInhalt)secondAktFach).FachName, aktFachInhalt.FachName)) // wenn die Fächer gleich heißen
                                {
                                    versucheToShow[j] = true;
                                }
                            }
                            j++;
                        }
                    }
                    if (aktFachInhalt.Vorhanden[4] && !aktFachInhalt.Bestanden) // wenn es der erste Versuch war, aber man nicht bestanden hat, setze auch auf true
                    {
                        versucheToShow[i] = true;
                    }
                }
                i++;
            }

            return versucheToShow;
        }
    }
}
