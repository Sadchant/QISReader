using QISReader.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;

namespace QISReader.ViewModel
{
    public class LoginViewModel
    {
        private List<string> _hochschulen;
        private string _selectedHochschule;
        private string _nutzername;
        private string _passwort;
        private string _infotext;

        public delegate void EventMethod();
        public event EventMethod StartAnmeldungEvent;
        public event EventMethod WrongLoginEvent;
        public event EventMethod KeineVerbindungEvent;
        public event EventMethod LoginFehlerEvent;
        public event EventMethod StartNotenNavigationEvent;
        public event EventMethod NotenNavigationsFehlerEvent;
        public event EventMethod StartNotenVerarbeitungEvent;
        public event EventMethod NotenVerarbeitungFehlerEvent;
        public event EventMethod NotenVerarbeitungFertigEvent;

        Scraper globalScraper;
        NotenParser globalNotenParser;
        FachManager globalFachManager;


        private void addHochschulen()
        {
            _hochschulen = new List<string>();
            _hochschulen.Add("Hochschule RheinMain");
            _hochschulen.Add("Hochschule Kaiserslautern");
            _hochschulen.Add("Hochschule Darmstadt");
            _hochschulen.Add("Hochschule Mannheim");
            _hochschulen.Add("Hochschule für angewandte Wissenschaften Würzburg-Schweinfurt");
            _hochschulen.Add("Fachhochschule Bingen");
            _hochschulen.Add("Hochschule Geisenheim");

            _hochschulen.Add("Hochschule RheinMain");
            _hochschulen.Add("Hochschule Kaiserslautern");
            _hochschulen.Add("Hochschule Darmstadt");
            _hochschulen.Add("Hochschule Mannheim");
            _hochschulen.Add("Hochschule für angewandte Wissenschaften Würzburg-Schweinfurt");
            _hochschulen.Add("Fachhochschule Bingen");
            _hochschulen.Add("Hochschule Geisenheim");
        }

        private async Task<string> readInfoTextFile()
        {
            var notenFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Resources/InfoText.txt"));
            string result = await Windows.Storage.FileIO.ReadTextAsync(notenFile);
            return result;
        }

        public LoginViewModel()
        {
            globalScraper = App.LogicManager.Scraper;
            globalNotenParser = App.LogicManager.NotenParser;
            globalFachManager = App.LogicManager.FachManager;
        }

        public async void loadContent()
        {
            addHochschulen();
            _infotext = await readInfoTextFile();
        }


        public List<string> Hochschulnamen
        {
            get
            {
                return _hochschulen;
            }
        }

        public string SelectedHochschule
        {
            set
            {
                _selectedHochschule = value;
            }
            get
            {
                return _selectedHochschule;
            }
        }

        public string Nutzername
        {
            set
            {
                _nutzername = value;
            }
            get
            {
                return _nutzername;
            }
        }

        public string Passwort
        {
            set
            {
                _passwort = value;
            }
            get
            {
                return _passwort;                
            }
        }

        public string InfoText
        {
            get
            {
                return _infotext;
            }
        }

        public ICommand LoginButtonClicked
        {
            get
            {
                return new DelegateCommand(Login);
            }
        }

        private async void Login()
        {
            globalScraper.Username = _nutzername;
            globalScraper.Password = _passwort;
            globalScraper.Baseurl = "https://qis.hs-rm.de/qisserver/rds?state=";
            
            //### Anmedlung
            string htmlPage; // hier wird die vom Scrapen erzeugte HTML-Seite drin gespeichert
            List<string> notenStringList;
            StartAnmeldungEvent();
            Debug.WriteLine("StartAnmeldungEvent");
            try
            {
                //komischer Workaround, damit "Anmeldung..." angezeigt wird
                await Task.Delay(TimeSpan.FromMilliseconds(10));
                await globalScraper.Login();
            }
            catch (Exception exception)
            {
                if (exception is WrongLoginException)
                    WrongLoginEvent();
                else if (exception is AggregateException)
                    KeineVerbindungEvent();
                else
                    LoginFehlerEvent();
                return;
            }

            // ### zu Noten navigieren, Ergebnis ist String mit Noten-Html-Seite
            StartNotenNavigationEvent();
            htmlPage = await globalScraper.NavigateQis();
            try
            {
                htmlPage = await globalScraper.NavigateQis();
            }
            catch (Exception exception)
            {
                if (exception is AggregateException)
                    KeineVerbindungEvent();
                else //ansonsten ist es eine ScrapQISException oder eine normale Exception
                    NotenNavigationsFehlerEvent();
                return;
            }

            // ### Noten verarbeiten, Ergebnis ist eine Liste mit Fach-Objekten
            StartNotenVerarbeitungEvent();
            try
            {
                notenStringList = globalNotenParser.parseNoten(htmlPage);
                globalFachManager.buildFachObjektList(notenStringList);
            }
            catch (Exception) // hier sollte eigentlich nichts schief gehen, wenn doch ist mein htmlParser fehlerhaft!
            {
                NotenVerarbeitungFehlerEvent();
                return;
            }
            NotenVerarbeitungFertigEvent();
        }
    }
}
