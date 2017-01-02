using QISReader.Model;
using QISReader.View;
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
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class NotenSpiegelPage : Page
    {
        private NotenParser globalNotenParser;
        private Dictionary<string, NotenSpiegel> notenSpiegelDict;
        private int maxNotenSpiegelBeschriftungsNumber;


        public NotenSpiegelPage()
        {
            this.InitializeComponent();
            globalNotenParser = App.LogicManager.NotenParser;
            notenSpiegelDict = new Dictionary<string, NotenSpiegel>();            
        }

        private async void navigateToDatenPage(string notenSpiegelhtmlPage)
        {
            //###
            notenSpiegelhtmlPage = await FileReader.readNotenSpiegelPage();
            //###
            try
            {
                globalNotenParser.parseNotenDaten(notenSpiegelhtmlPage);                
            }
            catch
            {
                KeinNotenSpiegelText.Visibility = Visibility.Visible;
                KeinNotenSpiegelText.Text = "Fehler beim Laden der Details";
            }
            DatenFrame.Navigate(typeof(DatenPage));
        }

        private async void navigateToVerteilungsPage(string notenSpiegelhtmlPage)
        {
            //###
            notenSpiegelhtmlPage = await FileReader.readNotenSpiegelPage();
            //###
            try
            {
                globalNotenParser.parseNotenSpiegel(notenSpiegelhtmlPage);
            }
            catch (Exception exception)
            {
                KeinNotenSpiegelText.Visibility = Visibility.Visible;
                if ((exception is KeinNotenSpiegelException))
                    KeinNotenSpiegelText.Text = "Der Klassenspiegel wird aus Datenschutzgründen nicht angezeigt, da zu wenige Leistungen vorliegen.";
                else
                    KeinNotenSpiegelText.Text = "Fehler beim Laden des Notenspiegels";
                return;
            }
            if (ermittlePage(globalNotenParser.AktNotenSpiegel.AktVerteilung.Max()))
                VerteilungsFrame.Navigate(typeof(VerteilungsPage3), maxNotenSpiegelBeschriftungsNumber);
            else
                VerteilungsFrame.Navigate(typeof(VerteilungsPage4), maxNotenSpiegelBeschriftungsNumber);
        }

        //gibt true für VerteilungsPage3 und false für VerteilungsPage4 zurück
        private bool ermittlePage(int maxNumber)
        {
            float dividedNumber = (float)maxNumber / 5;
            int ceiled = (int)Math.Ceiling(dividedNumber);
            maxNotenSpiegelBeschriftungsNumber = ceiled * 5;
            if (ceiled % 3 == 0)
                return false;
            else if (ceiled % 2 == 0)
                return true;
            else
                return ermittlePage(ceiled*5 + 5);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string htmlPage = e.Parameter as string;
            navigateToDatenPage(htmlPage);
            navigateToVerteilungsPage(htmlPage);
            if (Frame.CanGoBack)
            {
                // wird auf Mobile/im Tabletmode nicht beachtet
                Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Visible;
            }
            else
            {
                Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Collapsed;
            }
        }
    }
}
