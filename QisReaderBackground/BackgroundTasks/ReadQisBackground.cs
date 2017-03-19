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

            JsonManager jsonManager = new JsonManager();

            Scraper scraper = new Scraper();
            InitScraper(scraper);
            await scraper.Login();

            taskInstance.Progress = GlobalValues.START;
            try
            {
                //vorerst ausgeschaltet: komischer Workaround, damit "Anmeldung..." angezeigt wird
                //await Task.Delay(TimeSpan.FromMilliseconds(10));
                await scraper.Login();
            }
            catch (Exception exception)
            {
                if (exception is WrongLoginException)
                    taskInstance.Progress = GlobalValues.WRONGLOGIN;
                else if (exception is AggregateException)
                    taskInstance.Progress = GlobalValues.NOCONNECTION;
                else
                    taskInstance.Progress = GlobalValues.LOGINERROR;
                return;
            }
            taskInstance.Progress = GlobalValues.EINGELOGGT;
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
                    taskInstance.Progress = GlobalValues.NOCONNECTION;
                else //ansonsten ist es eine ScrapQISException oder eine normale Exception
                    taskInstance.Progress = GlobalValues.NOTENNAVIGATIONERROR;
                return;
            }

            Debug.WriteLine("starte noten verarbeitung");
            // ### Noten verarbeiten, Ergebnis ist eine Liste mit Fach-Objekten
            taskInstance.Progress = GlobalValues.NAVIGIERT;
            List<Fach> fachListe = new List<Fach>();
            HtmlParser htmlParser = new HtmlParser();
            try
            {
                htmlParser.ParseNoten(htmlPage);
            }
            catch (Exception) // hier sollte eigentlich nichts schief gehen, wenn doch ist mein htmlParser fehlerhaft!
            {
                taskInstance.Progress = GlobalValues.NOTENPROCESSINGERROR;
                return;
            }
            jsonManager.Save(htmlParser.FachListe, GlobalValues.NOTENFILENAME);
            taskInstance.Progress = GlobalValues.VERARBEITET;
            Debug.WriteLine("noten fertig");

            int counter = 0;
            float scaler = 100 / htmlParser.LinkDict.Count;
            foreach(int key in htmlParser.LinkDict.Keys)
            {
                try
                {
                    string htmlNotenDetailsPage = await scraper.navigateToNotenSpiegel(htmlParser.LinkDict[key]);
                    htmlParser.NotenSpiegelDict[key] = htmlParser.parseNotenDetails(htmlNotenDetailsPage);
                }
                catch
                {
                    htmlParser.NotenSpiegelDict[key] = null;
                }
                taskInstance.Progress = (uint)Math.Round(GlobalValues.NOTENSPIEGELPROGRESSSTART + counter*scaler);
                counter++;
            }
            jsonManager.Save(htmlParser.NotenSpiegelDict, GlobalValues.NOTENDETAILSFILENAME);

            NotenData notenData = new NotenData();
            notenData.ProcessNotenData(htmlParser.FachListe);
            jsonManager.Save(notenData, GlobalValues.NOTENDATAFILENAME);

            _deferral.Complete();
        }

        private void InitScraper(Scraper scraper)
        {
            Dictionary<string, string> hochschulDict = GetHochschulUrlDict();
            scraper.Baseurl = hochschulDict[(string)ApplicationData.Current.LocalSettings.Values[GlobalValues.HOCHSCHULE]]; // in localSettings wird der key abgelegt

            LoginDataSaver loginDataSaver = new LoginDataSaver();
            LoginData loginData = loginDataSaver.GetLoginData();
            if (loginData == null) return; // sollte eigentlich nicht passieren, Backgroundtask sollte immer erst angestoßen werden, nachdem Username + Passwort eingetragen wurde
            scraper.Username = loginData.Username;
            scraper.Password = loginData.Password;
        }

        private Dictionary<string, string> GetHochschulUrlDict()
        {
            Dictionary<string, string> hochschulDict = new Dictionary<string, string>();
            hochschulDict["Hochschule RheinMain"] = "https://qis.hs-rm.de/qisserver/rds?state=";
            hochschulDict["Hochschule Kaiserslautern"] = "https://qis.hs-rm.de/qisserver/rds?state=";
            hochschulDict["Hochschule Darmstadt"] = "https://qis.hs-rm.de/qisserver/rds?state=";
            hochschulDict["Hochschule Mannheim"] = "https://qis.hs-rm.de/qisserver/rds?state=";
            hochschulDict["Hochschule für angewandte Wissenschaften Würzburg-Schweinfurt"] = "https://qis.hs-rm.de/qisserver/rds?state=";
            hochschulDict["Fachhochschule Bingen"] = "https://qis.hs-rm.de/qisserver/rds?state=";
            hochschulDict["Hochschule Geisenheim"] = "https://qis.hs-rm.de/qisserver/rds?state=";
            return hochschulDict;
        }

        
    }
}
