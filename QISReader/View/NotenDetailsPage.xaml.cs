using QISReader.Model;
using QISReader.View;
using QisReaderClassLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    // wird von NotenDetailsPage an VerteilungsPage3/VerteilungsPage4 übergeben beim navigieren
    public class VerteilungsPageNavigationsArgs
    {
        public int MaxNotenSpiegelBeschriftungsNumber { get; }
        public List<int> Verteilung { get; set; }
        public float Durchschnitt { get; set; }
        public float EigeneNote { get; set; }
        public VerteilungsPageNavigationsArgs(int maxNotenSpiegelBeschriftungsNumber, List<int> verteilung, float durchschnitt, float eigeneNote)
        {
            MaxNotenSpiegelBeschriftungsNumber = maxNotenSpiegelBeschriftungsNumber;
            Verteilung = verteilung;
            Durchschnitt = durchschnitt;
            EigeneNote = eigeneNote;
        }
    }
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class NotenDetailsPage : Page
    {
        private Dictionary<string, NotenDetails> notenSpiegelDict;
        private int maxNotenSpiegelBeschriftungsNumber;


        public NotenDetailsPage()
        {
            this.InitializeComponent();
            notenSpiegelDict = new Dictionary<string, NotenDetails>();
        }

        

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            NotenDetailsNavigationArgs notenDetailsNavigationArgs = e.Parameter as NotenDetailsNavigationArgs;
            NotenDetails notenDetails = notenDetailsNavigationArgs.NotenDetails;

            if (notenDetailsNavigationArgs.NavigateToCollapsed)
            {
                // wird auf Mobile/im Tabletmode nicht beachtet
                Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Visible;
            }
            else
            {
                Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Collapsed;
            }

            if (notenDetails == null) // ist null, wenn gerade noch geladen wird oder wenn der Eintrag nicht gefunden wurde
            {
                bool idNotFound = false;
                if (await JsonManager.FileExists(GlobalValues.FILE_NOTENDETAILS)) // wenn der File da ist, aber null mitgegeben wurde, gab es einen Fehler
                    idNotFound = true;
                DatenFrame.Navigate(typeof(LoadDetailsPage), idNotFound); //wenn true mitgegeben wird, wird ein Fehler angezeigt
                return;
            }

            DatenFrame.Navigate(typeof(DatenPage), notenDetails);

           

            if (notenDetails.Verteilung == null) // sollte keine Verteilung da sein, Meldung anzeigen
            {                
                KeinNotenSpiegelText.Text = "Der Klassenspiegel wird aus Datenschutzgründen nicht angezeigt, da zu wenige Leistungen vorliegen.";
                KeinNotenSpiegelText.Visibility = Visibility.Visible;
                return;
            }
            // ansonsten Notenspiegelverteilung und BestandenPage anzeigen
            navigateToVerteilungsPage(notenDetails);
            BestandenFrame.Navigate(typeof(BestandenPage), notenDetails);
        }

        private void navigateToVerteilungsPage(NotenDetails notenDetails)
        {
            bool navigateToVerteilungsPage4 = ermittlePage(notenDetails.Verteilung.Max()); //nach oben gezogen, weil die methode maxNotenSpiegelBeschriftungsNumber setzt
            VerteilungsPageNavigationsArgs navigationArgs = new VerteilungsPageNavigationsArgs(maxNotenSpiegelBeschriftungsNumber, notenDetails.Verteilung, notenDetails.Durchschnitt, notenDetails.EigeneNote);
            if (navigateToVerteilungsPage4)
                VerteilungsFrame.Navigate(typeof(VerteilungsPage4), navigationArgs);
            else
                VerteilungsFrame.Navigate(typeof(VerteilungsPage3), navigationArgs);            
        }

        //gibt true für VerteilungsPage4 und false für VerteilungsPage3 zurück
        private bool ermittlePage(int maxNumber)
        {
            float dividedNumber = (float)maxNumber / 5;
            int ceiled = (int)Math.Ceiling(dividedNumber);
            maxNotenSpiegelBeschriftungsNumber = ceiled * 5;
            if (ceiled % 3 == 0) // VerteilungsPage4, wenn es durch 3 teilbar ist (jaa so ist es!)
                return true;
            else if (ceiled % 2 == 0) // VerteilungsPage3 wenn es durch 2 teilbar ist
                return false;
            else
                return ermittlePage(ceiled*5 + 5);
        }        
    }
}
