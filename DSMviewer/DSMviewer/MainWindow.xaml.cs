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
        internal ViewHelper helper;
                        
        public MainWindow()
        {
            InitializeComponent();
            ImageBrush background = new ImageBrush();
            background.ImageSource = new BitmapImage(new Uri(@"..\..\..\..\icons\defaultbackground.png",UriKind.Relative));
            this.Background = background;
            mapWidth = 525;
            mapHeight = 525;
        }
        public BitmapImage BackgroundMap { get; private set; }
        public LocationDisplays LocationDefs { get; private set; }
        public LineDisplays LineDefs { get; private set; }
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
        /// <summary>
        /// Updates fields necessary to build the canvas display of the lines and locations over the map.
        /// </summary>
        /// <param name="filepath"></param>
        private void makeCanvas(string filepath)
        {
            helper = ViewHelper.LoadModel(filepath, mapHeight, mapWidth);
            var chooseFeeder = new ChooseContainer(helper.ContainerNames());
            var feederselected = chooseFeeder.ShowDialog();
            if (feederselected == true)
            {
                this.LineDefs = helper.LineOverlays(chooseFeeder.SelectedLine);
            }
            this.LocationDefs = helper.LocationOverlays();
            this.BackgroundMap = helper.GoogleMapsURL();
            var mapdisplay = new MapDisplay(this);
            MainDisplay.Content = mapdisplay;
            
        }

        public void AddDock(UserControl control)
        {
            SideDisplay.Content = control;
        }
    }
}
