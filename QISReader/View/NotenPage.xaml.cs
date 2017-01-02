using QISReader.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace QISReader
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class NotenPage : Page
    {
        Dictionary<float, SolidColorBrush> notenFarbenDict = new Dictionary<float, SolidColorBrush>();
        int spaceAboveFachHeader = 30;
        int spaceAboveFachInhalt = 5;
        int aktSpaceAbove;
        FachHeader aktFachHeader;
        FachInhalt aktFachInhalt;
        bool isFirst = true;
        int aktRow = 1; // in aktRow kommt ein FachText rein, steht auf 1 weil auf dem 0ten die Überschrift "Notenübersicht" steht
        private Dictionary<int, string> linkDict;

        public NotenPage()
        {
            this.InitializeComponent();
            //testMethode();

            notenFarbenDict.Add(0.0f, (SolidColorBrush)Resources["bestandenBrush"]);
            notenFarbenDict.Add(1.0f, (SolidColorBrush)Resources["sehrgutBrush"]);
            notenFarbenDict.Add(1.3f, (SolidColorBrush)Resources["sehrgutBrush"]);
            notenFarbenDict.Add(1.7f, (SolidColorBrush)Resources["gutBrush"]);
            notenFarbenDict.Add(2.0f, (SolidColorBrush)Resources["gutBrush"]);
            notenFarbenDict.Add(2.3f, (SolidColorBrush)Resources["gutBrush"]);
            notenFarbenDict.Add(2.7f, (SolidColorBrush)Resources["befriedigendBrush"]);
            notenFarbenDict.Add(3.0f, (SolidColorBrush)Resources["befriedigendBrush"]);
            notenFarbenDict.Add(3.3f, (SolidColorBrush)Resources["befriedigendBrush"]);
            notenFarbenDict.Add(3.7f, (SolidColorBrush)Resources["ausreichendBrush"]);
            notenFarbenDict.Add(4.0f, (SolidColorBrush)Resources["ausreichendBrush"]);
            notenFarbenDict.Add(5.0f, (SolidColorBrush)Resources["durchgefallenBrush"]);

            linkDict = App.LogicManager.NotenParser.LinkDict;
            List<Fach> fachList = App.LogicManager.FachManager.FachListe;
            // wenn nichts drin war ist was schief gelaufen, sollte nicht vorkommen!
            if (fachList.Count == 0)
            {
                Debug.WriteLine("Achtung! FachListe ist leer!");
                return;
            }
            int i = 0; // nur für die Anzeige der Versuche da
            bool[] notenToShow = App.LogicManager.FachManager.getVersucheToShow(); // hole Liste um nur die Versuche anzuzeigen, die interessant sind
            foreach (Fach aktFach in fachList)
            {
                if (aktFach is FachHeader)
                {
                    aktFachHeader = (FachHeader)aktFach;
                    aktSpaceAbove = spaceAboveFachHeader;

                    // das Rect für die Hintergrundfarbe
                    // zuerst Farbe festlegen
                    SolidColorBrush backgroundColor;
                    if (aktFachHeader.Vorhanden[2] && aktFachHeader.Note > 0) // Note
                    {
                        backgroundColor = getColorFromNote(aktFachHeader.Note);
                    }
                    else if (aktFachHeader.Vorhanden[3])
                    {
                        if (aktFachHeader.Bestanden)
                            backgroundColor = notenFarbenDict[0.0f]; // 0.0 ist der Key für das bestanden-Blau
                        else
                            backgroundColor = notenFarbenDict[5.0f]; // wenn nicht bestanden nutze das 5.0-Rot
                    }
                    else
                        backgroundColor = new SolidColorBrush(Colors.LightGray);
                    Rectangle testRect = new Rectangle { Fill = backgroundColor };
                    addToNotenGrid(testRect, aktRow, 0);
                    Grid.SetColumnSpan(testRect, 11);

                    if (aktFachHeader.Vorhanden[1]) // Fachname
                    {
                        TextBlock fachText = new TextBlock { Text = aktFachHeader.FachName, FontSize = 20, /*Margin = new Thickness(20, 0, 0, 0),*/ FontWeight = FontWeights.Bold, TextWrapping = TextWrapping.Wrap, MaxWidth = 480, HorizontalAlignment = HorizontalAlignment.Left };
                        addToNotenGrid(fachText, aktRow, 1);
                    }
                    if (aktFachHeader.Vorhanden[2]) // Note
                    {
                        TextBlock notenText = new TextBlock { Text = aktFachHeader.Note.ToString("0.0"), FontSize = 20, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Right };
                        addToNotenGrid(notenText, aktRow, 3);
                    }
                    if (aktFachHeader.Vorhanden[4]) // Cp
                    {
                        TextBlock cpText = new TextBlock { Text = aktFachHeader.Cp.ToString("0.0") + " CP", FontSize = 20, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Right };
                        addToNotenGrid(cpText, aktRow, 7);
                    }
                }
                else
                {
                    aktFachInhalt = (FachInhalt)aktFach;
                    aktSpaceAbove = spaceAboveFachInhalt;


                    // das Rect für die Hintergrundfarbe
                    // zuerst Farbe festlegen
                    SolidColorBrush backgroundColor;
                    if (aktFachInhalt.Vorhanden[3] && aktFachInhalt.Note > 0) // Note
                    {
                        backgroundColor = getColorFromNote(aktFachInhalt.Note);
                    }
                    else if (aktFachInhalt.Vorhanden[4])
                    {
                        if (aktFachInhalt.Bestanden)
                            backgroundColor = notenFarbenDict[0.0f]; // 0.0 ist der Key für das bestanden-Blau
                        else
                            backgroundColor = notenFarbenDict[5.0f]; // wenn nicht bestanden nutze das 5.0-Rot
                    }
                    else
                        backgroundColor = new SolidColorBrush(Colors.LightGray);


                    Rectangle testRect = new Rectangle { Fill = backgroundColor };
                    addToNotenGrid(testRect, aktRow, 0);
                    Grid.SetColumnSpan(testRect, 11);

                    if (aktFachInhalt.Vorhanden[1]) // Fachname
                    {
                        TextBlock fachText = new TextBlock { Text = aktFachInhalt.FachName, FontSize = 18, /*Margin = new Thickness(40, 0, 0, 0),*/ TextWrapping = TextWrapping.Wrap, MaxWidth = 480, /*FontStyle = FontStyle.Italic*/ };
                        addToNotenGrid(fachText, aktRow, 1);
                    }
                    if (aktFachInhalt.Vorhanden[3]) // Note
                    {
                        TextBlock notenText = new TextBlock { Text = aktFachInhalt.Note.ToString("0.0"), FontSize = 18, HorizontalAlignment = HorizontalAlignment.Right, /*FontStyle = FontStyle.Italic*/ };
                        addToNotenGrid(notenText, aktRow, 3);
                    }
                    if (aktFachInhalt.Vorhanden[6]) // Versuch
                    {
                        if (notenToShow[i]) // die trues im bool-Array notenToShow wurden durch getNotenToShow() an den gewünschten Stellen auf true gesetzt
                        {
                            TextBlock versuchText = new TextBlock { Text = "Versuch " + aktFachInhalt.Versuch.ToString(), FontSize = 18, HorizontalAlignment = HorizontalAlignment.Right, /*FontStyle = FontStyle.Italic*/ };
                            addToNotenGrid(versuchText, aktRow, 5);
                        }                        
                        
                    }
                    if (aktFachInhalt.Vorhanden[5]) // Cp
                    {
                        TextBlock cpText = new TextBlock { Text = aktFachInhalt.Cp.ToString("0.0") + " Cp", FontSize = 18, HorizontalAlignment = HorizontalAlignment.Right, /*FontStyle = FontStyle.Italic*/ };
                        addToNotenGrid(cpText, aktRow, 7);
                    }
                    if (linkDict.ContainsKey(aktFachInhalt.Id))
                    {
                        Button notenSpiegelButton = new Button { Content = "Notenspiegel", Padding = new Thickness(0), Margin = new Thickness(0,0,15,0), CommandParameter=aktFachInhalt.Id };
                        notenSpiegelButton.Click += NotenSpiegelClick;
                        addToNotenGrid(notenSpiegelButton, aktRow, 9);
                    }
                    //NotenGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                }
                aktRow += 2;
                // wenns nicht der erste ist, mache eine Leerzeile
                if (isFirst)
                {                    
                    isFirst = false;
                }
                else
                {
                    NotenGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(aktSpaceAbove) });
                }
                NotenGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                i++;
            }

            //NotenGrid.RowDefinitions.Add(new RowDefinition { Height=GridLength.Auto });
            //Rectangle aktRect = new Rectangle { Width = 1000, Height=100, Fill=new SolidColorBrush(Colors.Red) } ;
            //NotenGrid.Children.Add(aktRect);
            //Grid.SetRow(aktRect, 1);
        }

        private async void NotenSpiegelClick(object sender, RoutedEventArgs e)
        {
            // wenn die NotenSpiegel-Seite für das Fach noch nicht da ist (wird in Dictionary gespeichert), navigiere dorthin
            // entscheide, welche NotenSpiegelPage genutzt wird
            int id = (int)((Button)sender).CommandParameter;
            string notenSpiegelHTMLPage = await App.LogicManager.Scraper.navigateToNotenSpiegel(App.LogicManager.NotenParser.LinkDict[id]);
            if (NotenSpiegelFrame.Visibility == Visibility.Collapsed)
            {
                this.Frame.Navigate(typeof(NotenSpiegelPage), notenSpiegelHTMLPage);                
            }
            else
            {
                NotenSpiegelFrame.Navigate(typeof(NotenSpiegelPage), notenSpiegelHTMLPage);
            }
        }

        private SolidColorBrush getColorFromNote(float note)
        {
            if (note < 1.0 + 0.15)
            {
                return notenFarbenDict[1.0f];
            }
            else if (note >= 1.0 + 0.15 && note < 1.3 + 0.15)
            {
                return notenFarbenDict[1.3f];
            }
            else if (note >= 1.3 + 0.15 && note < 1.7 + 0.15)
            {
                return notenFarbenDict[1.7f];
            }
            else if (note >= 1.7 + 0.15 && note < 2.0 + 0.15)
            {
                return notenFarbenDict[2.0f];
            }
            else if (note >= 2.0 + 0.15 && note < 2.3 + 0.15)
            {
                return notenFarbenDict[2.3f];
            }
            else if (note >= 2.3 + 0.15 && note < 2.7 + 0.15)
            {
                return notenFarbenDict[2.7f];
            }
            else if (note >= 2.7 + 0.15 && note < 3.0 + 0.15)
            {
                return notenFarbenDict[3.0f];
            }
            else if (note >= 3.0 + 0.15 && note < 3.3 + 0.15)
            {
                return notenFarbenDict[3.3f];
            }
            else if (note >= 3.3 + 0.15 && note < 3.7 + 0.15)
            {
                return notenFarbenDict[3.7f];
            }
            else if (note >= 3.7 + 0.15 && note <= 4.0)
            {
                return notenFarbenDict[4.0f];
            }
            else if (note > 4.0 )
            {
                return notenFarbenDict[5.0f];
            }
            return new SolidColorBrush(Colors.Gray);
        }

        private void addToNotenGrid(FrameworkElement addThis, int row, int column)
        {
            NotenGrid.Children.Add(addThis);
            Grid.SetRow(addThis, row);
            Grid.SetColumn(addThis, column);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (Frame.BackStack.LastOrDefault().SourcePageType.Equals(typeof(LoginPage)))
            {
                Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Collapsed;
                return;
            }
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

        // kommt später weg
        //private void InfoButton_Click(object sender, RoutedEventArgs e)
        //{
        //    var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
        //    var scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
        //    var size = new Size(bounds.Width * scaleFactor, bounds.Height * scaleFactor);

        //    BoundsText.Text = bounds.ToString();
        //    ScaleFactorText.Text = scaleFactor.ToString();
        //    SizeText.Text = size.ToString();
        //}
        private async void testMethode()
        {
            /*Scraper2 testScraper = new Scraper2();
            string html = await testScraper.ClientScraper();*/
            FileReader fileReader = new FileReader();
            string html = await fileReader.readNotenPage();
            List<String> notenStringList = App.LogicManager.NotenParser.parseNoten(html);
            App.LogicManager.FachManager.buildFachObjektList(notenStringList);

            string notenSpiegelPage = await FileReader.readNotenSpiegelPage();
            App.LogicManager.NotenParser.parseNotenSpiegel(notenSpiegelPage);
        }
    }
}
