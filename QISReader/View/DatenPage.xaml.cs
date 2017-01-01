using QISReader.Model;
using QISReader.ViewModel;
using System;
using System.Collections.Generic;
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
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace QISReader.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DatenPage : Page
    {
        private NotenParser globalNotenParser;

        public DatenViewModel ViewModel { get; set; }

        public DatenPage()
        {
            this.InitializeComponent();
            DataContextChanged += (s, e) =>
            {
                ViewModel = DataContext as DatenViewModel;
            };

            globalNotenParser = App.LogicManager.NotenParser;
            generateTable();
        }

        private void generateTable()
        {
            DatenÜberschriftTextBlock.Text = globalNotenParser.AktNotenSpiegel.AktÜberschrift;
            int aktIndex = 0;
            var r = App.Current.Resources;
            var t = this.Resources;
            var d = (Style)Resources["datenRectStyle"];
            //foreach (string datenBeschriftung in globalNotenParser.AktNotenSpiegel.AktDatenBeschriftung)
            //{
            //    DatenTabelleGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            //    Rectangle line = new Rectangle { Style = (Style)Resources["datenRectStyle"] };
            //    DatenTabelleGrid.Children.Add(line);
            //    Grid.SetRow(line, aktIndex+1); //+1, weil in der ersten Reihe defaultmäßig das erste Rect sitzt

            //    string datenInhalt = globalNotenParser.AktNotenSpiegel.AktDatenInhalt[aktIndex];

            //    aktIndex++;
            //}
        }
    }
}
