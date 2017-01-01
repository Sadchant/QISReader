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


        public NotenSpiegelPage()
        {
            this.InitializeComponent();
            globalNotenParser = App.LogicManager.NotenParser;
            notenSpiegelDict = new Dictionary<string, NotenSpiegel>();            
        }

        private async void navigateToDatenUC(string notenSpiegelhtmlPage)
        {
            notenSpiegelhtmlPage = await FileReader.readNotenSpiegelPage();
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

        private async void selectVerteilungsPage(string notenSpiegelhtmlPage)
        {
            notenSpiegelhtmlPage = await FileReader.readNotenSpiegelPage();
            try
            {
                globalNotenParser.parseNotenSpiegel(notenSpiegelhtmlPage);
            }
            catch (Exception exception)
            {
                KeinNotenSpiegelText.Visibility = Visibility.Visible;
                if (!(exception is KeinNotenSpiegelException))
                    KeinNotenSpiegelText.Text = "Fehler beim Laden des Notenspiegels";
                return;
            }
            int highestNumber = globalNotenParser.AktNotenSpiegel.AktVerteilung.Max();
            float dividedNumber = (float)highestNumber / 5;
            Debug.WriteLine(dividedNumber);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string htmlPage = e.Parameter as string;
            navigateToDatenUC(htmlPage);
            selectVerteilungsPage(htmlPage);
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
