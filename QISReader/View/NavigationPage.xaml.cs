using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace QISReader.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NavigationPage : Page
    {
        public NavigationPage()
        {
            this.InitializeComponent();
            App.NavigationManager.InsertContentFrame(ContentFrame);
            App.NavigationManager.InsertListBoxes(TopListBox, BottomListBox);

            EinstellungenPage.LogoutEvent += LogoutNavigation; 
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // in den NavigationEventArgs wird mitgegeben, ob nach App-Start direkt zur NavigationPage navigiert wurde
            // etwas umständlich, kann wieder in die app-behind wenns auf dem smartphone nichts bringt
            Debug.WriteLine("Zur NavigationPage navigated");
            if ((bool)e.Parameter)
            {
                App.LogicManager.FachManager.FachListe = await App.LogicManager.NotenDataSaver.LoadNoten();
            }
            NotenListBoxItem.IsSelected = true;
            TopListBox.SelectedItem = NotenListBoxItem;
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }

        private void TopListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            Debug.WriteLine("TopListBox_SelectionChanged");
            // wenn die änderung bloß deaktiviert, also -1 ist, lass die andere Listbox in Ruhe
            // wenn die änderung dagegen ein item auswählt, deaktiviere die andere Listbox
            if (TopListBox.SelectedIndex != -1)
                BottomListBox.SelectedIndex = -1;
            var selectedItem = TopListBox.SelectedItem;
            if (NotenListBoxItem.IsSelected)
            {
                Debug.WriteLine("navigiere zu Noten");
                ContentFrame.Navigate(typeof(NotenPage));
            }
            else if (StatistikenListBoxItem.IsSelected)
            {
                Debug.WriteLine("navigiere zu Statistiken");
                ContentFrame.Navigate(typeof(StatistikenPage));
            }
        }

        private void BottomListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // wenn die änderung bloß deaktiviert, also -1 ist, lass den anderen in Ruhe
            // wenn die änderung dagegen ein item auswählt, deaktiviere die andere Listbox
            if (BottomListBox.SelectedIndex != -1)
                TopListBox.SelectedIndex = -1;
            if (EinstellungenListBoxItem.IsSelected)
            {
                Debug.WriteLine("navigiere zu Einstellungen");
                ContentFrame.Navigate(typeof(EinstellungenPage));
            }
        }

        private void LogoutNavigation()
        {
            Frame.Navigate(typeof(LoginPage));
            App.LogicManager.InitLogic();
        }
    }
}
