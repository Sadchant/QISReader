using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using QISReader.Model;
using Windows.UI.Core;
using QISReader.ViewModel;
using QISReader.View;
using Windows.Storage;
using QisReaderClassLibrary;
using System.Threading.Tasks;

namespace QISReader
{
    /// <summary>
    /// Stellt das anwendungsspezifische Verhalten bereit, um die Standardanwendungsklasse zu ergänzen.
    /// </summary>
    sealed partial class App : Application
    {
        public static LogicManager LogicManager;
        public static NavigationManager NavigationManager { get; set; }
        private Frame contentFrame;


        public event EventHandler<BackRequestedEventArgs> OnBackRequested;
        /// <summary>
        /// Initialisiert das Singletonanwendungsobjekt. Dies ist die erste Zeile von erstelltem Code
        /// und daher das logische Äquivalent von main() bzw. WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            //BackgroundTaskManager.Unregister();

            LogicManager = new LogicManager();
            NavigationManager = new NavigationManager();
            InitSettings();
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Anwendung durch den Endbenutzer normal gestartet wird. Weitere Einstiegspunkte
        /// werden z. B. verwendet, wenn die Anwendung gestartet wird, um eine bestimmte Datei zu öffnen.
        /// </summary>
        /// <param name="e">Details über Startanforderung und -prozess.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            await LogicManager.InitLogic();
            Frame rootFrame = Window.Current.Content as Frame;

            // App-Initialisierung nicht wiederholen, wenn das Fenster bereits Inhalte enthält.
            // Nur sicherstellen, dass das Fenster aktiv ist.
            if (rootFrame == null)
            {
                // Frame erstellen, der als Navigationskontext fungiert und zum Parameter der ersten Seite navigieren
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Zustand von zuvor angehaltener Anwendung laden
                }

                // Den Frame im aktuellen Fenster platzieren
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // Wenn der Navigationsstapel nicht wiederhergestellt wird, zur ersten Seite navigieren
                    // und die neue Seite konfigurieren, indem die erforderlichen Informationen als Navigationsparameter
                    // übergeben werden
                    if (await JsonManager.FileExists(GlobalValues.FILE_NOTEN))
                        rootFrame.Navigate(typeof(NavigationPage));
                    else
                        rootFrame.Navigate(typeof(LoginPage), e.Arguments);                        
                }
                // Sicherstellen, dass das aktuelle Fenster aktiv ist
                Window.Current.Activate();
            }
            //jedes Mal wenn das BackRequested-Event gefeuert wird, rufe auch App_BackRequested auf
            SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested; 
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Navigation auf eine bestimmte Seite fehlschlägt
        /// </summary>
        /// <param name="sender">Der Rahmen, bei dem die Navigation fehlgeschlagen ist</param>
        /// <param name="e">Details über den Navigationsfehler</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Ausführung der Anwendung angehalten wird.  Der Anwendungszustand wird gespeichert,
        /// ohne zu wissen, ob die Anwendung beendet oder fortgesetzt wird und die Speicherinhalte dabei
        /// unbeschädigt bleiben.
        /// </summary>
        /// <param name="sender">Die Quelle der Anhalteanforderung.</param>
        /// <param name="e">Details zur Anhalteanforderung.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Anwendungszustand speichern und alle Hintergrundaktivitäten beenden
            deferral.Complete();
        }

       

        // kümmert sich um die Rücknavigation via "Hardware"-Buttons oder eingeblendetem Desktop-Zurück-Button
        private void App_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (OnBackRequested != null) { OnBackRequested(this, e); }

            NavigationManager.ManageBackRequest(e);
        }

        private void InitSettings()
        {
            if (ApplicationData.Current.LocalSettings.Values[GlobalValues.SETTINGS_AUTOUPDATE] == null)
                ApplicationData.Current.LocalSettings.Values[GlobalValues.SETTINGS_AUTOUPDATE] = true;
            if (ApplicationData.Current.LocalSettings.Values[GlobalValues.SETTINGS_UPDATERATE] == null)
                ApplicationData.Current.LocalSettings.Values[GlobalValues.SETTINGS_UPDATERATE] = GlobalValues.UPDATERATE_ALLE_30_MINUTEN;
                //ApplicationData.Current.LocalSettings.Values[GlobalValues.SETTINGS_UPDATERATE] = GlobalValues.UPDATERATE_ALLE_2_STUNDEN;
        }
    }
}
