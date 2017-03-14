using QISReader.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace QISReader.ViewModel
{
    public class NavigationManager
    {
        private Frame contentFrame;
        private ListBox topListBox;
        private ListBox bottomListBox;

        public void InsertContentFrame(Frame contentFrame)
        {
            this.contentFrame = contentFrame;
        }

        public void InsertListBoxes(ListBox topListBox, ListBox bottomListBox)
        {
            this.topListBox = topListBox;
            this.bottomListBox = bottomListBox;
        }

        public void ManageBackRequest(BackRequestedEventArgs e)
        {
            // überprüfe ob sich noch niemand drum gekümmert hat
            if (!e.Handled)
            {
                // navigier defaultmäßig im Frame zurück
                //Frame frame = Window.Current.Content as Frame;
                if (contentFrame.CanGoBack)
                {
                    // Navigation zurück zur Login-Page ist nicht erlaubt, dafür ist die Logout-Funktion da!
                    //if (!contentFrame.BackStack.LastOrDefault().SourcePageType.Equals(typeof(LoginPage)))
                    contentFrame.GoBack();

                    // fixe ListBox-Auswahl, nachdem Back gegangen wurde
                    correctListBoxSelection(contentFrame);

                    // setzte Handled auf true, damit das System nicht in die letzte App navigiert
                    e.Handled = true;
                }
            }
        }

        // wenn man zurück geht hat die ListBox immernoch die falsche Page als aktiv markiert, diese Methode fixt das
        private void correctListBoxSelection(Frame frame)
        {
            if (frame.SourcePageType.Equals(typeof(NotenPage)))
            {
                topListBox.SelectedIndex = 0;
                bottomListBox.SelectedIndex = -1;
            }
            else if (frame.SourcePageType.Equals(typeof(StatistikenPage)))
            {
                topListBox.SelectedIndex = 1;
                bottomListBox.SelectedIndex = -1;
            }
            else if (frame.SourcePageType.Equals(typeof(EinstellungenPage)))
            {
                topListBox.SelectedIndex = -1;
                bottomListBox.SelectedIndex = 0;
            }
        }
    }
}
