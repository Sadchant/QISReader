using QisReaderClassLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;

namespace QisReaderBackground
{
    public sealed class UpdateDataBackground : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral; // wird benötigt, um async-Code innerhalb eines Background-Tasks auszuführen
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();

            Scraper scraper = new Scraper();

            await Scraper.InitScraper(scraper);

            taskInstance.Progress = GlobalValues.STARTANMELDUNG;
            try
            {
                await scraper.Login();
            }
            catch
            {
                return;
            }

            // ### zu Noten navigieren, Ergebnis ist String mit Noten-Html-Seite
            string htmlPage = null;
            try
            {
                htmlPage = await scraper.NavigateQis();
            }
            catch
            {
                return;
            }
            HtmlParser htmlParser = new HtmlParser();
            try
            {
                htmlParser.ParseNoten(htmlPage);
            }
            catch
            {
                return;
            }
            // auf Unterschiede prüfen
            NotenData notenData = new NotenData();
            notenData.ProcessNotenData(htmlParser.FachListe);

            NotenData oldNotenData = await JsonManager.Load<NotenData>(GlobalValues.FILE_NOTENDATA);

            if (notenData.IsEqual(oldNotenData)) // wenn sie gleich sind, speichere die neuen notenData ab um das datum zu aktualisieren
            {
                await JsonManager.Save(notenData, GlobalValues.FILE_NOTENDATA);
                SendToast("Noten aktualisiert " + DateTime.Now.ToString());
                return;
            }
            // ansonsten war es nicht gleich, das heißt die Änderungen müssen abgespeichert werden, es muss die Kachel aktualisiert und eine Benachrichtigung verschickt werden

            List<Fach> oldFachListe = await JsonManager.Load<List<Fach>>(GlobalValues.FILE_NOTEN);

            List<string> neueFächer = new List<string>();
            List<string> neueNoten = new List<string>();
            if(notenData.AnzahlEinträge != oldNotenData.AnzahlEinträge) // finde neu eingetragene Fächer
            {
                foreach (Fach fach in htmlParser.FachListe)
                {
                    if ((oldFachListe.Any(oldFach => oldFach.Id != fach.Id))) //findet heraus, welche Ids sich unterscheiden
                        neueFächer.Add(fach.FachName);
                }
            }

            if (notenData.AnzahlNoten != oldNotenData.AnzahlNoten) // finde neu eingetragene Noten
            {
                foreach (Fach fach in htmlParser.FachListe)
                {
                    if ((oldFachListe.Any(oldFach => oldFach.Note != fach.Note))) //findet heraus, welche Ids sich unterscheiden
                        neueNoten.Add(fach.FachName);
                }
            }
            

            

            


            await JsonManager.Save(htmlParser.FachListe, GlobalValues.FILE_NOTEN);
            await JsonManager.Save(htmlParser.LinkDict, GlobalValues.FILE_DETAILSDICTIONARY);
            taskInstance.Progress = GlobalValues.NOTENVERARBEITUNGFERTIG;
            Debug.WriteLine("noten fertig");

            int counter = 0;
            float scaler = 100.0f / (htmlParser.LinkDict.Count); // -1, weil man ja bei 0 anfängt zu zählen
            foreach (int key in htmlParser.LinkDict.Keys)
            {
                try
                {
                    string htmlNotenDetailsPage = await scraper.navigateToNotenSpiegel(htmlParser.LinkDict[key]); // die Seite mit dem Scraper ansurfen
                    htmlParser.NotenDetailsDict[key] = htmlParser.parseNotenDetails(htmlNotenDetailsPage); // die html-Seite in NotenDetailsObjekt parsen und im DetailsDict ablegen
                }
                catch // sollte es aus irgendeinem Grund schief gehen, setzte das zum Key gehörige Detail-Objekt einfach null
                {
                    htmlParser.NotenDetailsDict[key] = null;
                }
                counter++;
                //sendet Fortschritts-Prozentzahl der NotenSpiegelVerarbeitung (reicht von 200 bis 300, also ist 200 = 0% und 300 = 100%)
                Debug.WriteLine(counter + " " + scaler + " " + counter * scaler);
                taskInstance.Progress = (uint)Math.Round(GlobalValues.NOTENSPIEGELPROGRESSSTART + counter * scaler);
                Debug.WriteLine("Progress!");
            }
            // Details-Dict abspeichern
            await JsonManager.Save(htmlParser.NotenDetailsDict, GlobalValues.FILE_NOTENDETAILS);

            // notenData zum schnellen Vergleichen von Änderungen abspeichern
            //NotenData notenData = new NotenData();
            notenData.ProcessNotenData(htmlParser.FachListe);
            await JsonManager.Save(notenData, GlobalValues.FILE_NOTENDATA);
            Debug.WriteLine("alles fertig vom Background!");
            SendToast("Noten aktualisiert" + DateTime.Now.ToString());
            _deferral.Complete();
        }

        public static void SendToast(string message)
        {
            var template = ToastTemplateType.ToastText01;
            var xml = ToastNotificationManager.GetTemplateContent(template);
            var elements = xml.GetElementsByTagName("text");
            var text = xml.CreateTextNode(message);
            elements[0].AppendChild(text);
            var toast = new ToastNotification(xml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}
