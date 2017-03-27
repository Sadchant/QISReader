using QISReader.Model;
using QisReaderClassLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;

namespace QISReader.ViewModel
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private List<string> _hochschulen;
        private string _selectedHochschule;
        private string _nutzername;
        private string _passwort;
        private string _infotext;

        private bool loggingIn = false; //wird auf true gesetzt, wenn man sich anfängt einzuloggen, damit man es währenddessen nicht wiederholen kann

        public event PropertyChangedEventHandler PropertyChanged;

        ReadQis readQisBackgroundTaskManager;

        public LoginViewModel()
        {
            _hochschulen = new List<string>();
            // wenn es Fehler gibt, ist man sich nicht mehr am einloggen
            App.LogicManager.ReadQis.KeineVerbindungEvent += SetLoggingInFalse;
            App.LogicManager.ReadQis.LoginFehlerEvent += SetLoggingInFalse;
            App.LogicManager.ReadQis.NotenNavigationsFehlerEvent += SetLoggingInFalse;
            App.LogicManager.ReadQis.NotenVerarbeitungFehlerEvent += SetLoggingInFalse;
            App.LogicManager.ReadQis.WrongLoginEvent += SetLoggingInFalse;

            // wenn es fertig ist, ist man sich auch nicht mehr am einloggenawait Task.Delay(TimeSpan.FromMilliseconds(10));
            App.LogicManager.ReadQis.NotenVerarbeitungFertigEvent += SetLoggingInFalse; 
        }

        private async Task<string> ReadInfoTextFile()
        {
            var notenFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Resources/InfoText.txt"));
            string result = await FileIO.ReadTextAsync(notenFile);
            return result;
        }

        public async void LoadContent()
        {
            //await Task.Delay(TimeSpan.FromMilliseconds(3000));
            Debug.WriteLine("lade Hochschulen");
            Dictionary<string, string> hochschulDict = await JsonManager.LoadFromResources<Dictionary<string, string>>(GlobalValues.HOCHSCHULDICTFILENAME);
            _hochschulen = hochschulDict.Keys.ToList(); // die Values sind uninteressant, da sie die Links beinhalten
            _infotext = await ReadInfoTextFile();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Hochschulnamen"));
            Debug.WriteLine("fertig geladen");
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
            // wenn man sich am einloggen ist, blocke weitere Login-Versuche
            if (loggingIn)
                return;
            // war man noch nicht eingeloggt, merke, dass man sich jetzt am einloggen ist
            loggingIn = true;

            // Hole aktuelle Logik-Objekte
            readQisBackgroundTaskManager = App.LogicManager.ReadQis;

            ApplicationData.Current.LocalSettings.Values[GlobalValues.SETTINGS_HOCHSCHULE] = _selectedHochschule; // abspeichern in LocalSettings, damit der Backgroundtask damit arbeiten kann
            LoginDataSaver loginDataSaver = new LoginDataSaver();
            loginDataSaver.SetLoginData(_nutzername, _passwort);

            // starte den Login im BackgroundTask
            await App.LogicManager.ReadQis.StartReadQis();
        }

        // sollte es einen Fehler geben, ist man sich nicht mehr am einloggen
        private void SetLoggingInFalse()
        {
            loggingIn = false;
        }
    }
}
