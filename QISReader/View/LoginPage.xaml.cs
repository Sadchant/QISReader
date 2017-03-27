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
using Windows.UI.Core;
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
            App.LogicManager.ReadQis.StartAnmeldungEvent += HandleStartAnmeldung;
            App.LogicManager.ReadQis.WrongLoginEvent += HandleWrongLogin;
            App.LogicManager.ReadQis.KeineVerbindungEvent += HandleKeineVerbindung;
            App.LogicManager.ReadQis.LoginFehlerEvent += HandleLoginFehler;
            App.LogicManager.ReadQis.StartNotenNavigationEvent += HandleStartNotenNavigvation;
            App.LogicManager.ReadQis.NotenNavigationsFehlerEvent += HandleNotenNavigationsFehler;
            App.LogicManager.ReadQis.StartNotenVerarbeitungEvent += HandleStartNotenVerarbeitung;
            App.LogicManager.ReadQis.NotenVerarbeitungFehlerEvent += HandleNotenVerarbeitungFehler;
            App.LogicManager.ReadQis.NotenVerarbeitungFertigEvent += HandleNotenVerarbeitungFertig;

            redBrush = new SolidColorBrush(Colors.Red);
            accentBrush = new SolidColorBrush((Color)this.Resources["SystemAccentColor"]);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ((LoginViewModel)this.DataContext).LoadContent();
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

        private async void HandleError()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                LoginProgressRing.Visibility = Visibility.Collapsed;
                ProgressText.Foreground = redBrush;
                AnmeldeButton.IsEnabled = true; //Button wieder aktivieren, damit man es erneut versuchen kann
            });
        }

        private async void HandleStartAnmeldung()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                AnmeldeButton.IsEnabled = false; //Anmelde-Button deaktivieren, damit man sich während dem EInloggen nicht nochmal einloggt
                ProgressText.Foreground = accentBrush;
                LoginProgressRing.Visibility = Visibility.Visible;
                ProgressText.Visibility = Visibility.Visible;
                ProgressText.Text = "Anmeldung...";
            });
        }

        private async void HandleWrongLogin()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                 HandleError();
                 AnmeldeButton.Content = "Anmelden";
                 ProgressText.Text = "Falscher Nutzername oder falsches Passwort!";
                 UsernameTextBox.BorderBrush = redBrush;
                 PasswordBox.BorderBrush = redBrush;
             });
        }

        private async void HandleKeineVerbindung()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                HandleError();
                ProgressText.Text = "Fehler: Keine Verbindung möglich!";
                AnmeldeButton.Content = "Erneut versuchen";
            });
        }

        private async void HandleLoginFehler()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                HandleError();
                ProgressText.Text = "Fehler bei der Anmeldung. Möglicherweise hilft ein App-Restart!";
            });
        }

        private async void HandleStartNotenNavigvation()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ProgressText.Text = "Navigiere zu Noten...";
            });
        }

        private async void HandleNotenNavigationsFehler()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                HandleError();
                ProgressText.Text = "Fehler bei der Webbrowser-Simulation!";
            });
        }

        private async void HandleStartNotenVerarbeitung()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ProgressText.Text = "Verarbeite Noten...";
            });
        }

        private async void HandleNotenVerarbeitungFehler()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                HandleError();
                ProgressText.Text = "Fehler: Notenverarbeitung fehlgeschlagen!";
            });
        }

        private async void HandleNotenVerarbeitungFertig()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                LoginProgressRing.Visibility = Visibility.Collapsed;
                this.Frame.Navigate(typeof(NavigationPage)); // Noten sind schon geladen und müssen nicht aus dem jsonFile wiederhergestellt werden, also false mitgeben
            });
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
