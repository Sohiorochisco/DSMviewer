using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DSMviewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ViewHelper helper;
                        
        public MainWindow()
        {
            InitializeComponent();
            mapWidth = 525;
            mapHeight = 525;

        }
        private double mapWidth;
        private double mapHeight;
        private void ChooseFile(object sender, RoutedEventArgs e)
        {
            var fileSelection = new Microsoft.Win32.OpenFileDialog();
            fileSelection.Filter = "Xlsx files (*.xlsx)|*.xlsx";
            var result = fileSelection.ShowDialog();
            var newFile = sender as MenuItem;
            if (result == true)
            {
                this.makeCanvas(fileSelection.FileName);
            }
                                          
        }

        private void makeCanvas(string filepath)
        {
            helper = ViewHelper.LoadModel(filepath, mapHeight, mapWidth);
            var chooseFeeder = new ChooseContainer(helper.ContainerNames());
            var feederselected = chooseFeeder.ShowDialog();
            IEnumerable<Line> powerlines;
            if (feederselected == true)
            {
                powerlines = helper.LineOverlays(chooseFeeder.SelectedLine);
            }
            else
            {
                powerlines = null;
            }
            var locationDisplays = helper.LocationOverlays();
            Image map = new Image();
            map.Source = new BitmapImage(new Uri(helper.GoogleMapsURL(), UriKind.Absolute));
            var mapView = new Canvas();
            mapView.Children.Add(map);
            foreach (Ellipse ellipse in locationDisplays)
            {
                ellipse.Stroke = new SolidColorBrush(new Color { G = 250, B = 50, R = 100 });
                ellipse.StrokeThickness = 3;
                ellipse.Fill = new SolidColorBrush(new Color { G = 100, B = 100, R = 10 });
                ellipse.IsEnabled = true;
                mapView.Children.Add(ellipse);
            }
            foreach (Line line in powerlines)
            {
                line.Stroke = new SolidColorBrush(new Color { B = 240,G = 240});
                line.StrokeThickness = 6;
                line.IsEnabled = true;
                mapView.Children.Add(line);
            }
            Grid.SetRow(mapView, 1);
            this.areaViewLayout.Children.Add(mapView);
        }
    }
}
