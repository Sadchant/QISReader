using QisReaderClassLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QisReaderBackground
{
    public class KeinNotenSpiegelException : Exception { }

    public class HtmlParser
    {
        public List<Fach> FachListe { get; set; }
        public Dictionary<int, string> LinkDict { get; set; }
        public Dictionary<int, NotenDetails> NotenSpiegelDict { get; set; }

        //Liste und Dictioanries initialisieren
        public HtmlParser()
        {
            FachListe = new List<Fach>();
            LinkDict = new Dictionary<int, string>();
            NotenSpiegelDict = new Dictionary<int, NotenDetails>();
        }

        private string GetSubStringBetween(string source, string beginString, string endString)
        {
            if (!(source.Contains(beginString) && source.Contains(endString)))
                return null;
            int Start = source.IndexOf(beginString, 0) + beginString.Length;
            int End = source.IndexOf(endString, Start);
            return source.Substring(Start, End - Start);
        }

        // parsed die Noten in FachListe
        // befüllt das LinkDict mit den Links auf die Notenspiegel
        public void ParseNoten(string htmlPage)
        {
            string table = GetSubStringBetween(htmlPage, @"<table border=""0"">", "</table>");

            table = Regex.Replace(table, @"\t|\n|\r", ""); // Aller Whitespace muss entfernt werden, sonst kann das HTML später schlecht geparsed werden

            string begintr = "<tr";
            string endtr = "</tr>";

            string begintd = "<td";
            string endtd = "</td>";

            List<string> notenLines = new List<string>();
            string aktTrString;
            string aktTdString;
            int aktTrEndeIndex;
            int aktTdEndeIndex;
            List<string> aktTdList; // enthält alle <td> in einer tablerow mit HTML-Tags
            List<string> aktCleanedTdList; // enthält alle <td> in einer tablerow aufgeräumt ohne html
            string linkIndicator = "<a href=";
            Boolean isUeberschrift = false;

            for (int index = 0; ; index += begintr.Length) // iteriert über alle <tr> also über alle Zeilen
            {
                index = table.IndexOf(begintr, index);
                if (index == -1) // wenns nicht gefunden wurde beende die schleife
                    break;
                aktTrEndeIndex = table.IndexOf(endtr, index + begintr.Length);
                aktTrString = table.Substring(index, aktTrEndeIndex - index);

                if (aktTrString.Contains(">NA<")) // wenn man "Nicht Angetreten" ist, ignoriere die gesamte Zeile, soll nicht in der Liste angezeigt werden
                    continue;

                if (aktTrString.Contains("ECTS-Kontostand")) // der Kontostand soll auch ignoriert werden (später extra anzeigen!)
                    continue;

                aktTdList = new List<string>();
                for (int j = 0; ; j += begintd.Length) // iteriert über all tds und füllt sie in aktTds
                {
                    j = aktTrString.IndexOf(begintd, j);
                    if (j == -1)
                        break;
                    aktTdEndeIndex = aktTrString.IndexOf(endtd, j + begintd.Length);
                    aktTdString = aktTrString.Substring(j, aktTdEndeIndex - j);
                    aktTdList.Add(aktTdString);
                }

                if (aktTdList.Count <= 0) // wenn zB ths in der zeile waren
                    continue;
                isUeberschrift = aktTdList.First().Contains("qis_kontoOnTop");  // zuerst herausfinden ob es eine Überschrift ist, dann html löschen, dann abspeichern

                aktCleanedTdList = new List<string>();
                foreach (string aktTd in aktTdList) //geht über die Elemente einer Zeile
                {
                    Match match;
                    if (aktTd.Contains(linkIndicator))
                    {
                        Regex regex = new Regex(@"<a href=""(.*)""><img"); // das Pattern, wo der Link zum Notenspiegel liegt
                        Match htmlMatch = regex.Match(aktTd);
                        int fachnummer = Int32.Parse(aktCleanedTdList.First()); // das erste Element ist die Prüsfungsnummer, die schon ausgelesen wurde, wenn ein Link gefunden wird
                        LinkDict.Add(fachnummer, htmlMatch.Groups[1].Value);
                        match = new Regex(@">(.*)<a").Match(aktTd);
                    }
                    else
                        match = new Regex(@">(.*)").Match(aktTd);
                    if (match.Success && match.Groups.Count > 1)
                    {
                        aktTdString = match.Groups[1].Value;
                    }
                    else
                        aktTdString = "";
                    aktCleanedTdList.Add(aktTdString.Trim()); // Trim(): schneidet Whitespace an Anfang und Ende ab
                }

                if (isUeberschrift) // es ist eine Fachüberschrift
                {
                    string[] daten = new string[] { aktCleanedTdList[0], aktCleanedTdList[1], aktCleanedTdList[4], aktCleanedTdList[5], aktCleanedTdList[6] };
                    FachListe.Add(BuildFachHeader(daten));
                }
                else // es ist ein Fachinhalt
                {
                    string[] daten = new string[] { aktCleanedTdList[0], aktCleanedTdList[1], aktCleanedTdList[2], aktCleanedTdList[4], aktCleanedTdList[5], aktCleanedTdList[6], aktCleanedTdList[8] };
                    FachListe.Add(BuildFachInhalt(daten));
                }
            }
        }

        private FachHeader BuildFachHeader(string[] daten)
        {
            int memberId = 0; // dient dazu, im boolean-Feld festzulegen, ob die Variable im Fach existiert oder nicht
            FachHeader fachHeader = new FachHeader();
            memberId = 0;
            if (!string.IsNullOrEmpty(daten[memberId]))
            {
                fachHeader.Vorhanden[memberId] = true;
                fachHeader.Id = int.Parse(daten[memberId]);
            }
            memberId = 1;
            if (!string.IsNullOrEmpty(daten[memberId]))
            {
                fachHeader.Vorhanden[memberId] = true;
                fachHeader.FachName = daten[memberId];
            }
            memberId = 2;
            if (!string.IsNullOrEmpty(daten[memberId]))
            {
                fachHeader.Vorhanden[memberId] = true;
                fachHeader.Note = float.Parse(daten[memberId]);
            }
            memberId = 3;
            if (!string.IsNullOrEmpty(daten[memberId]))
            {
                fachHeader.Vorhanden[memberId] = true;
                if (daten[memberId] == "bestanden")
                    fachHeader.Bestanden = true;
                else if (daten[memberId] == "nicht bestanden")
                    fachHeader.Bestanden = false;
                else
                    fachHeader.Vorhanden[memberId] = false;
            }
            memberId = 4;
            if (!string.IsNullOrEmpty(daten[memberId]))
            {
                fachHeader.Vorhanden[memberId] = true;
                fachHeader.Cp = float.Parse(daten[memberId]);
            }
            return fachHeader;
        }

        private FachInhalt BuildFachInhalt(string[] daten)
        {
            int memberId = 0; // dient dazu, im boolean-Feld festzulegen, ob die Variable im Fach existiert oder nicht
            FachInhalt fachInhalt = new FachInhalt();
            memberId = 0;
            if (!string.IsNullOrEmpty(daten[memberId]))
            {
                fachInhalt.Vorhanden[memberId] = true;
                fachInhalt.Id = int.Parse(daten[memberId]);
            }
            memberId = 1;
            if (!string.IsNullOrEmpty(daten[memberId]))
            {
                fachInhalt.Vorhanden[memberId] = true;
                fachInhalt.FachName = daten[memberId];
            }
            memberId = 2;
            if (!string.IsNullOrEmpty(daten[memberId]))
            {
                fachInhalt.Vorhanden[memberId] = true;
                fachInhalt.Semester = daten[2];
            }
            memberId = 3;
            if (!string.IsNullOrEmpty(daten[memberId]))
            {
                fachInhalt.Vorhanden[memberId] = true;
                fachInhalt.Note = float.Parse(daten[memberId]);
            }
            memberId = 4;
            if (!string.IsNullOrEmpty(daten[memberId]))
            {
                fachInhalt.Vorhanden[memberId] = true;
                if (daten[memberId] == "bestanden")
                    fachInhalt.Bestanden = true;
                else
                    fachInhalt.Bestanden = false;
            }
            memberId = 5;
            if (!string.IsNullOrEmpty(daten[memberId]))
            {
                fachInhalt.Vorhanden[memberId] = true;
                fachInhalt.Cp = float.Parse(daten[memberId]);
            }
            memberId = 6;
            if (!string.IsNullOrEmpty(daten[memberId]))
            {
                fachInhalt.Vorhanden[memberId] = true;
                fachInhalt.Versuch = int.Parse(daten[6]);
            }
            return fachInhalt;
        }

        // filtert alle Notendetails (Tabelle mit Prüfungsnummer etc. und Notenspiegel) aus der Html-Seite und gibt sie als NotenDetails-Objekt zurück
        public NotenDetails parseNotenDetails(string htmlPage)
        {
            NotenDetails notenDetails = new NotenDetails();
            ParseNotenDaten(htmlPage, notenDetails);
            ExtractÜberschrift(notenDetails);

            ParseNotenSpiegel(htmlPage, notenDetails);
            if (notenDetails.Verteilung == null) // wenn es keinen NotenSpiegel gibt braucht man auch nicht die eigene Note oder den Durchschnitt suchen
                return notenDetails;       
            FindEigeneNote(notenDetails);
            GetDurchschnitt(htmlPage, notenDetails);
            return notenDetails;
        }

        // extrahier NotenDaten aus der Html-Seite
        private void ParseNotenDaten(string htmlPage, NotenDetails notenDetails)
        {
            string table = GetSubStringBetween(htmlPage, @"<table border=""0"" width=""100%"">", "</table>");
            table = Regex.Replace(table, @"\t|\n|\r", ""); // Aller Whitespace muss entfernt werden, sonst kann das HTML später schlecht geparsed werden
            string beginth = "<th";
            string endth = "</th>";
            string aktThString;
            int aktThEndeIndex;

            notenDetails.AktDatenBeschriftung.Clear();
            for (int index = 0; ; index += beginth.Length) // iteriert über alle <th>
            {
                index = table.IndexOf(beginth, index);
                if (index == -1) // wenns nicht gefunden wurde beende die schleife
                    break;
                aktThEndeIndex = table.IndexOf(endth, index + beginth.Length);
                aktThString = table.Substring(index, aktThEndeIndex - index);
                Match match = new Regex(@">(.*)").Match(aktThString);
                if (match.Success && match.Groups.Count > 1)
                {
                    string matchResult = match.Groups[1].Value.Replace("&nbsp;", ""); // aus dem matchresult komischen HTML-Müll entfernen
                    notenDetails.AktDatenBeschriftung.Add(matchResult);
                }
            }

            string begintd = "<td";
            string endtd = "</td>";
            string aktTdString;
            int aktTdEndeIndex;

            notenDetails.AktDatenInhalt.Clear();
            for (int index = 0; ; index += begintd.Length) // iteriert über alle <th>
            {
                index = table.IndexOf(begintd, index);
                if (index == -1) // wenns nicht gefunden wurde beende die schleife
                    break;
                aktTdEndeIndex = table.IndexOf(endtd, index + begintd.Length);
                aktTdString = table.Substring(index, aktTdEndeIndex - index);
                Match match = new Regex(@">(.*)").Match(aktTdString);
                if (match.Success && match.Groups.Count > 1)
                {

                    string matchResult = match.Groups[1].Value.Trim(); // aus dem matchresult Whitespace entfernen
                    notenDetails.AktDatenInhalt.Add(matchResult);
                }
            }
        }
        // extrahiert die Verteilung aus der NotenSpiegel-Html-Seite und gibt sie als int-Liste zurück
        private void ParseNotenSpiegel(string htmlPage, NotenDetails notenDetails)
        {
            if (htmlPage.Contains("da zu wenige Leistungen vorliegen.")) // kein Notenspiegel wird dargestellt durch: Verteilung = null
            {
                notenDetails.Verteilung = null;
                return;
            }
                
            string table = GetSubStringBetween(htmlPage, @"<table border=""0"" align=""left"" width=""60%"">", "</table>");
            table = Regex.Replace(table, @"\t|\n|\r", ""); // Aller Whitespace muss entfernt werden, sonst kann das HTML später schlecht geparsed werden

            string begintr = "<tr";
            string endtr = "</tr>";
            string aktTrString;
            int aktTrEndeIndex;
            int trIndex = 0; // dient zum zählen, um den unerwünschten Teil der Html-Tabelle am Ende zu ignorieren

            // alte Verteilung löschen
            notenDetails.Verteilung.Clear();
            for (int index = 0; ; index += begintr.Length) // iteriert über alle <tr>
            {
                if (trIndex > 4) // die Tabelle beinhaltet noch mehr Werte als die Notenverteilung, die wird hiermit ignoriert
                    break;
                index = table.IndexOf(begintr, index);
                if (index == -1) // wenns nicht gefunden wurde beende die schleife
                    break;
                aktTrEndeIndex = table.IndexOf(endtr, index + begintr.Length);
                aktTrString = table.Substring(index, aktTrEndeIndex - index);

                if (!aktTrString.Contains(@"class=""tabelle1""")) // Zeilen mit "class="tabelleheader"" auf diese Weise herausfiltern, Achtung, der trIndex wird dabei auch nicht erhöht!
                    continue;

                string verteilung = GetSubStringBetween(aktTrString, @"</td><td", "</td>");
                Match match = new Regex(@">(.*)").Match(verteilung);
                if (match.Success && match.Groups.Count > 1)
                {
                    string matchResult = match.Groups[1].Value;
                    if (matchResult.Contains("inklusive"))  // das "inklusive Ihrer Leistung muss noch herausgefiltert werden, falls vorhanden
                    {
                        Match zweiterMatch = new Regex(@"(.*)\(inklusive Ihrer Leistung\)").Match(matchResult);
                        matchResult = zweiterMatch.Groups[1].Value;
                    }
                    notenDetails.Verteilung.Add(int.Parse(matchResult.Trim()));
                }
                trIndex++;
            }
        }

        // filtert aus den beiden Listen den Fachnamen heraus, speichert ihn in der Überschrift-Property und löscht ihn aus der Beschriftungs- und Daten-Liste
        private void ExtractÜberschrift(NotenDetails notenDetails)
        {
            notenDetails.AktÜberschrift = "";
            int überschriftIndex = notenDetails.AktDatenBeschriftung.IndexOf("Prüfungstext");
            notenDetails.AktÜberschrift = notenDetails.AktDatenInhalt[überschriftIndex];
            notenDetails.AktDatenBeschriftung.RemoveAt(überschriftIndex);
            notenDetails.AktDatenInhalt.RemoveAt(überschriftIndex);
        }

        // findet die eigene in den Noten-Daten
        private void FindEigeneNote(NotenDetails notenDetails)
        {
            int noteIndex = notenDetails.AktDatenBeschriftung.IndexOf("Note");
            notenDetails.AktEigeneNote = float.Parse(notenDetails.AktDatenInhalt[noteIndex]);
        }

        // sucht den Durchschnitt aus der html-Seite heraus und
        private void GetDurchschnitt(string htmlPage, NotenDetails notenDetails)
        {
            string durchschnittsLine = GetSubStringBetween(htmlPage, @"Durchschnittsnote", "</tr>");
            string durchschnittsLineEdited = GetSubStringBetween(durchschnittsLine, @"</td>", "</td>"); // etwas umständlich aber so geht es
            Match match = new Regex(@">(.*)").Match(durchschnittsLineEdited);
            notenDetails.AktDurchschnitt = 0;
            if (match.Success && match.Groups.Count > 1)
            {
                string res = match.Groups[1].Value;
                notenDetails.AktDurchschnitt = float.Parse(match.Groups[1].Value);
            }
        }
    }
}
