﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QISReader.Model
{
    public class NotenSpiegel
    {
        public string AktÜberschrift { get; set; }
        public List<string> AktDatenBeschriftung { get; set; }
        public List<string> AktDatenInhalt { get; set; }
        public List<int> AktVerteilung { get; set; }
        public float AktDurchschnitt { get; set; }


        public NotenSpiegel()
        {
            AktDatenBeschriftung = new List<string>();
            AktDatenInhalt = new List<string>();
            AktVerteilung = new List<int>();
        }
    }

    public class KeinNotenSpiegelException : System.Exception { }

    public class NotenParser
    {
        public Dictionary<int, string> LinkDict { get; set; } 
        public NotenSpiegel AktNotenSpiegel { get; set; }

        public NotenParser()
        {
            LinkDict = new Dictionary<int, string>();
            AktNotenSpiegel = new NotenSpiegel();
        }


        private string getSubStringBetween(string source, string beginString, string endString)
        {
            if (!(source.Contains(beginString) && source.Contains(endString)))
                return null;
            int Start = source.IndexOf(beginString, 0) + beginString.Length;
            int End = source.IndexOf(endString, Start);
            return source.Substring(Start, End - Start);
        }

        // parsed die Noten in eine String-Liste
        public List<string> parseNoten(string htmlPage)
        {
            Debug.WriteLine("parseNoten");
            string table = getSubStringBetween(htmlPage, @"<table border=""0"">", "</table>");

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
            int lineIndex = 0;

            for (int index = 0; ; index += begintr.Length) // iteriert über alle <tr>
            {
                index = table.IndexOf(begintr, index);                
                if (index == -1) // wenns nicht gefunden wurde beende die schleife
                    break;
                aktTrEndeIndex = table.IndexOf(endtr, index + begintr.Length);
                aktTrString = table.Substring(index, aktTrEndeIndex - index);

                if (aktTrString.Contains(">NA<")) // wenn man "Nicht Angetreten" ist, ignoriere die gesamte Zeile, soll nicht in der Liste angezeigt werden
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
                aktCleanedTdList.Add((lineIndex++).ToString());
                foreach (string aktTd in aktTdList.Skip(1)) // das erste Element, die Prüsfungsnummer dient nur dem Link-Dictionary und wird durch unseren lineIndex ersetzt
                {
                    Match match;
                    if (aktTd.Contains(linkIndicator))
                    {
                        Regex regex = new Regex(@"<a href=""(.*)""><img"); // das Pattern, wo der Link zum Notenspiegel liegt
                        Match htmlMatch = regex.Match(aktTd);
                        Debug.WriteLine(aktCleanedTdList.First());
                        int fachnummer = Int32.Parse(aktCleanedTdList.First()); // das erste element hat niemals einen link und wurde bereits hinzugefügt
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
                    notenLines.Add(aktCleanedTdList[0] + ";" + aktCleanedTdList[1] + ";" + aktCleanedTdList[4] + ";" + aktCleanedTdList[5] + ";" + aktCleanedTdList[6]);
                }
                else // es ist ein Fachinhalt
                {
                    notenLines.Add(aktCleanedTdList[0] + ";" + aktCleanedTdList[1] + ";" + aktCleanedTdList[2] + ";" + aktCleanedTdList[4] + ";" + aktCleanedTdList[5] + ";" + aktCleanedTdList[6] + ";" + aktCleanedTdList[8]);
                }
            }
            var removeThis = notenLines.SingleOrDefault(s => s.Contains("ECTS-Kontostand")); // den Noten-Durchschnitt vom QIS löschen
            if (removeThis != null)
                notenLines.Remove(removeThis);
            return notenLines;
        }


        // extrahiert die Verteilung aus der NotenSpiegel-Html-Seite und gibt sie als int-Liste zurück
        public void parseNotenSpiegel(string htmlPage)
        {
            if (htmlPage.Contains("da zu wenige Leistungen vorliegen."))
                throw new KeinNotenSpiegelException();
            string table = getSubStringBetween(htmlPage, @"<table border=""0"" align=""left"" width=""60%"">", "</table>");
            table = Regex.Replace(table, @"\t|\n|\r", ""); // Aller Whitespace muss entfernt werden, sonst kann das HTML später schlecht geparsed werden

            string begintr = "<tr";
            string endtr = "</tr>";
            string aktTrString;
            int aktTrEndeIndex;
            int trIndex = 0; // dient zum zählen, um den unerwünschten Teil der Html-Tabelle am Ende zu ignorieren

            // alte Verteilung löschen
            AktNotenSpiegel.AktVerteilung.Clear();
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

                string verteilung = getSubStringBetween(aktTrString, @"</td><td", "</td>");
                Match match = new Regex(@">(.*)").Match(verteilung);
                if (match.Success && match.Groups.Count > 1)
                {
                    string matchResult = match.Groups[1].Value;
                    if (matchResult.Contains("inklusive"))  // das "inklusive Ihrer Leistung muss noch herausgefiltert werden, falls vorhanden
                    {
                        Match zweiterMatch = new Regex(@"(.*)\(inklusive Ihrer Leistung\)").Match(matchResult);
                        matchResult = zweiterMatch.Groups[1].Value;
                    }
                    AktNotenSpiegel.AktVerteilung.Add(int.Parse(matchResult.Trim()));
                }
                trIndex++;
            }
            AktNotenSpiegel.AktDurchschnitt = getDurchschnitt(htmlPage);
        }

        public void parseNotenDaten(string htmlPage)
        {
            string table = getSubStringBetween(htmlPage, @"<table border=""0"" width=""100%"">", "</table>");
            table = Regex.Replace(table, @"\t|\n|\r", ""); // Aller Whitespace muss entfernt werden, sonst kann das HTML später schlecht geparsed werden
            string beginth = "<th";
            string endth = "</th>";
            string aktThString;
            int aktThEndeIndex;

            AktNotenSpiegel.AktDatenBeschriftung.Clear();
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
                    AktNotenSpiegel.AktDatenBeschriftung.Add(matchResult);
                }
            }

            string begintd = "<td";
            string endtd = "</td>";
            string aktTdString;
            int aktTdEndeIndex;

            AktNotenSpiegel.AktDatenInhalt.Clear();
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
                    AktNotenSpiegel.AktDatenInhalt.Add(matchResult);
                }
            }
            extractÜberschrift();
        }

        // filtert aus den beiden Listen den Fachnamen heraus, speichert ihn in der Überschrift-Property und löscht ihn aus der Beschriftungs- und Daten-Liste
        private void extractÜberschrift()
        {
            AktNotenSpiegel.AktÜberschrift = "";
            int überschriftIndex = AktNotenSpiegel.AktDatenBeschriftung.IndexOf("Prüfungstext");
            AktNotenSpiegel.AktÜberschrift = AktNotenSpiegel.AktDatenInhalt[überschriftIndex];
            AktNotenSpiegel.AktDatenBeschriftung.RemoveAt(überschriftIndex);
            AktNotenSpiegel.AktDatenInhalt.RemoveAt(überschriftIndex);
        }

        // sucht den Durchschnitt aus der html-Seite heraus und
        private float getDurchschnitt(string htmlPage)
        {
            string durchschnittsLine = getSubStringBetween(htmlPage, @"Durchschnittsnote", "</tr>");
            string durchschnittsLineEdited = getSubStringBetween(durchschnittsLine, @"</td>", "</td>"); // etwas umständlich aber so geht es
            Match match = new Regex(@">(.*)").Match(durchschnittsLineEdited);
            if (match.Success && match.Groups.Count > 1)
            {
                string res = match.Groups[1].Value;
                return float.Parse(match.Groups[1].Value);
            }
            return 0;
        }
    }

    

    
}