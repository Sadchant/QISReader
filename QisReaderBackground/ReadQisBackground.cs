using QisReaderClassLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace QisReaderBackground
{
    public sealed class ReadQisBackground : IBackgroundTask
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
                //await Task.Delay(TimeSpan.FromMilliseconds(10));
                await scraper.Login();
            }
            catch (Exception exception)
            {
                if (exception is WrongLoginException)
                    taskInstance.Progress = GlobalValues.WRONGLOGIN;
                else if (exception is AggregateException)
                    taskInstance.Progress = GlobalValues.KEINEVERBINDUNG;
                else
                    taskInstance.Progress = GlobalValues.LOGINFEHLER;
                return;
            }
            taskInstance.Progress = GlobalValues.STARTNOTENNAVIGATION;
            Debug.WriteLine("starte noten navigation");

            // ### zu Noten navigieren, Ergebnis ist String mit Noten-Html-Seite
            string htmlPage = null;
            try
            {
                htmlPage = await scraper.NavigateQis();
            }
            catch (Exception exception)
            {
                if (exception is AggregateException)
                    taskInstance.Progress = GlobalValues.KEINEVERBINDUNG;
                else //ansonsten ist es eine ScrapQISException oder eine normale Exception
                    taskInstance.Progress = GlobalValues.NOTENNAVIGATIONSFEHLER;
                return;
            }

            Debug.WriteLine("starte noten verarbeitung");
            // ### Noten verarbeiten, Ergebnis ist eine Liste mit Fach-Objekten
            taskInstance.Progress = GlobalValues.STARTNOTENVERARBEITUNG;
            HtmlParser htmlParser = new HtmlParser();
            
            try
            {
                htmlParser.ParseNoten(htmlPage);
            }
            catch (Exception) // hier sollte eigentlich nichts schief gehen, wenn doch ist mein htmlParser fehlerhaft!
            {
                taskInstance.Progress = GlobalValues.NOTENVERARBEITUNGFEHLER;
                return;
            }
            // NotenListe abspeichern
            await JsonManager.Save(htmlParser.FachListe, GlobalValues.FILE_NOTEN);
            await JsonManager.Save(htmlParser.LinkDict, GlobalValues.FILE_DETAILSDICTIONARY);
            taskInstance.Progress = GlobalValues.NOTENVERARBEITUNGFERTIG;
            Debug.WriteLine("noten fertig");

            int counter = 0;
            float scaler = 100.0f / (htmlParser.LinkDict.Count); // -1, weil man ja bei 0 anfängt zu zählen
            foreach(int key in htmlParser.LinkDict.Keys)
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
            NotenData notenData = new NotenData();
            notenData.ProcessNotenData(htmlParser.FachListe);
            await JsonManager.Save(notenData, GlobalValues.FILE_NOTENDATA);
            Debug.WriteLine("alles fertig vom Background!");
            _deferral.Complete();
        }

               
    }
}
