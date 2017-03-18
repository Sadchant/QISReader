using QISReader.Model;
using QISReader.View;
using QISReader.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics; // muss später raus!
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace QISReader
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        Scraper globalScraper;
        NotenParser globalNotenParser;
        FachManager globalFachManager;

        private LoginViewModel loginViewModel;
        private SolidColorBrush redBrush;
        private SolidColorBrush accentBrush;

        public LoginViewModel ViewModel { get; set; }

        public LoginPage()
        {
            this.InitializeComponent();
            DataContextChanged += (s, e) =>
            {
                ViewModel = DataContext as LoginViewModel;
            };
            loginViewModel = (LoginViewModel)this.DataContext;
            loginViewModel.StartAnmeldungEvent += HandleStartAnmeldung;
            loginViewModel.WrongLoginEvent += HandleWrongLogin;
            loginViewModel.KeineVerbindungEvent += HandleKeineVerbindung;
            loginViewModel.LoginFehlerEvent += HandleLoginFehler;
            loginViewModel.StartNotenNavigationEvent += HandleStartNotenNavigvation;
            loginViewModel.NotenNavigationsFehlerEvent += HandleNotenNavigationsFehler;
            loginViewModel.StartNotenVerarbeitungEvent += HandleStartNotenVerarbeitung;
            loginViewModel.NotenVerarbeitungFehlerEvent += HandleNotenVerarbeitungFehler;
            loginViewModel.NotenVerarbeitungFertigEvent += HandleNotenVerarbeitungFertig;

            redBrush = new SolidColorBrush(Colors.Red);
            accentBrush = new SolidColorBrush((Color)this.Resources["SystemAccentColor"]);

            globalScraper = App.LogicManager.Scraper;
            globalNotenParser = App.LogicManager.NotenParser;
            globalFachManager = App.LogicManager.FachManager;
        }

        

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void InfoTextBox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            InfoPanel.Visibility = Visibility.Visible;
            //await new MessageDialog(loginViewModel.InfoText).ShowAsync();
        }

        private void InfoCloseButton_Click(object sender, RoutedEventArgs e)
        {
            InfoPanel.Visibility = Visibility.Collapsed;
        }

        private async void AnmeldeButton_Click(object sender, RoutedEventArgs e)
        {
            //testMethode();
            //Farben für den Statustext
            

            // ausgefüllte Felder in Scraper eintragen
            globalScraper.Username = UsernameTextBox.Text;
            globalScraper.Password = PasswordBox.Password;
            globalScraper.Baseurl = "https://qis.hs-rm.de/qisserver/rds?state=";
            

            // ProgressText-Farbe zurücksetzen
            ProgressText.Foreground = accentBrush;


            //### Anmedlung
            LoginProgressRing.Visibility = Visibility.Visible;
            ProgressText.Visibility = Visibility.Visible;
            ProgressText.Text = "Anmeldung...";
            

            string htmlPage; // hier wird die vom Scrapen erzeugte HTML-Seite drin gespeichert
            List<string> notenStringList;
            try
            {
                await globalScraper.Login();
            }
            catch (Exception exception)
            {
                LoginProgressRing.Visibility = Visibility.Collapsed;
                ProgressText.Foreground = redBrush;
                if (exception is WrongLoginException)
                {
                    // Falls vorher die Verbindung fehlgeschlagen ist und jetzt ein falscher Benutzername eingegeben wurde
                    AnmeldeButton.Content = "Anmelden";

                    ProgressText.Text = "Falscher Nutzername oder falsches Passwort!";
                    UsernameTextBox.BorderBrush = redBrush;
                    PasswordBox.BorderBrush = redBrush;
                }
                else if (exception is AggregateException)
                {
                    ProgressText.Text = "Fehler: Keine Verbindung möglich!";
                    AnmeldeButton.Content = "Erneut versuchen";
                }
                else
                    ProgressText.Text = "Fehler bei der Anmeldung. Möglicherweise hilft ein App-Restart!";

                return;
            }
            
            // ### zu Noten navigieren, Ergebnis ist String mit Noten-Html-Seite
            ProgressText.Text = "Navigiere zu Noten...";
            try
            {
                htmlPage = await globalScraper.NavigateQis();
            }
            catch (Exception exception)
            {
                LoginProgressRing.Visibility = Visibility.Collapsed;
                ProgressText.Foreground = redBrush;

                if (exception is AggregateException)
                {
                    ProgressText.Text = "Fehler: Verbindung abgebrochen!";
                    AnmeldeButton.Content = "Erneut versuchen";
                }
                else //ansonsten ist es eine ScrapQISException oder eine normale Exception
                    ProgressText.Text = "Fehler bei der Webbrowser-Simulation!";

                return;
            }

            // ### Noten verarbeiten, Ergebnis ist eine Liste mit Fach-Objekten
            ProgressText.Text = "Verarbeite Noten...";
            NotenParser notenParser = new NotenParser();
            try
            {
                notenStringList = notenParser.parseNoten(htmlPage);
                globalFachManager.buildFachObjektList(notenStringList);
            }
            catch (Exception) // hier sollte eigentlich nichts schief gehen, wenn doch ist mein htmlParser fehlerhaft!
            {
                LoginProgressRing.Visibility = Visibility.Collapsed;
                ProgressText.Foreground = redBrush;
                ProgressText.Text = "Fehler: Notenverarbeitung fehlgeschlagen!";
                return;
            }
            LoginProgressRing.Visibility = Visibility.Collapsed;
            this.Frame.Navigate(typeof(NotenPage));
            //LoginProgressRing.Visibility = Visibility.Collapsed;
        }

        private void UsernameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Farben der TextFleder zurücksetzen, falls sie vorher rot waren wegen falscher Eingabe
            UsernameTextBox.BorderBrush = new SolidColorBrush(Colors.Black);
            PasswordBox.BorderBrush = new SolidColorBrush(Colors.Black);
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Farben der TextFleder zurücksetzen, falls sie vorher rot waren wegen falscher Eingabe
            UsernameTextBox.BorderBrush = new SolidColorBrush(Colors.Black);
            PasswordBox.BorderBrush = new SolidColorBrush(Colors.Black);
        }

        private async void testMethode()
        {
            Scraper2 testScraper = new Scraper2();
            string html = await testScraper.ClientScraper();
            NotenParser notenParser = new NotenParser();
            List<String> notenStringList = notenParser.parseNoten(html);
            FachManager fachManager = new FachManager();
            fachManager.buildFachObjektList(notenStringList);
        }



        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ((LoginViewModel)this.DataContext).loadContent();
        }        

        private void HandleError()
        {
            LoginProgressRing.Visibility = Visibility.Collapsed;
            ProgressText.Foreground = redBrush;
            AnmeldeButton.IsEnabled = true; //Button wieder aktivieren, damit man es erneut versuchen kann
        }

        private void HandleStartAnmeldung()
        {
            AnmeldeButton.IsEnabled = false; //Anmelde-Button deaktivieren, damit man sich während dem EInloggen nicht nochmal einloggt
            ProgressText.Foreground = accentBrush;
            LoginProgressRing.Visibility = Visibility.Visible;
            ProgressText.Visibility = Visibility.Visible;
            ProgressText.Text = "Anmeldung...";
        }

        private void HandleWrongLogin()
        {
            HandleError();
            AnmeldeButton.Content = "Anmelden";
            ProgressText.Text = "Falscher Nutzername oder falsches Passwort!";
            UsernameTextBox.BorderBrush = redBrush;
            PasswordBox.BorderBrush = redBrush;
        }

        private void HandleKeineVerbindung()
        {
            HandleError();
            ProgressText.Text = "Fehler: Keine Verbindung möglich!";
            AnmeldeButton.Content = "Erneut versuchen";
        }

        private void HandleLoginFehler()
        {
            HandleError();
            ProgressText.Text = "Fehler bei der Anmeldung. Möglicherweise hilft ein App-Restart!";
        }

        private void HandleStartNotenNavigvation()
        {
            ProgressText.Text = "Navigiere zu Noten...";
        }

        private void HandleNotenNavigationsFehler()
        {
            ProgressText.Text = "Fehler bei der Webbrowser-Simulation!";
        }

        private void HandleStartNotenVerarbeitung()
        {
            ProgressText.Text = "Verarbeite Noten...";
        }

        private void HandleNotenVerarbeitungFehler()
        {
            HandleError();
            ProgressText.Text = "Fehler: Notenverarbeitung fehlgeschlagen!";
        }

        private void HandleNotenVerarbeitungFertig()
        {
            LoginProgressRing.Visibility = Visibility.Collapsed;
            this.Frame.Navigate(typeof(NavigationPage), false); // Noten sind schon geladen und müssen nicht aus dem jsonFile wiederhergestellt werden, also false mitgeben
        }

        private void RelativePanel_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            // Command auslösen, wenn Enter gedrückt wurde
            if (e.Key == Windows.System.VirtualKey.Enter && ViewModel.LoginButtonClicked.CanExecute(null))
            {
                ViewModel.LoginButtonClicked.Execute(null);
                e.Handled = true; // ansonsten wird der Command zweimal ausgeführt, warum auch immer...
            }
            
                
        }
    }
}
