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
    /// Interaction logic for MapDisplay.xaml
    /// </summary>
    public partial class MapDisplay : UserControl
    {
        public MapDisplay(Window mainWindow)
        {
            InitializeComponent();
            DataContext = mainWindow;
        }

        private void OpenLocationView(object sender, MouseButtonEventArgs e){
            var locIcon = (Ellipse)sender;
            var location = (LocationDef)locIcon.DataContext;
            var maker = new SchematicMaker(location.BackingLocation, 15, 15);
            var locDisplay = new LocationDisplay(maker);
            //Display the LocationDisplay in its own window
            Window extLocationDisplay = new Window(){
                Title = location.Description,
                Content = locDisplay
            };

            //For debug only 
            //Image testImage = new Image();
            //testImage.Source = new BitmapImage(maker.IconList[0].Icon);
            //Window testImageDisplay = new Window(){
            //    Title = "Test Image",
            //    Content = testImage
            //};
            //testImageDisplay.Show();
            locDisplay.IsEnabled = true;
            extLocationDisplay.Show();


            /*This should display the LocationDisplay control alongside the MapDisplay, but
             for some reason the LocationDisplay control is never visible, and the vshost seems
             to crash when this is attempted*/

            //MainWindow parent = (MainWindow)Window.GetWindow(this);
            //parent.AddDock(locDisplay);
        }
    }
}
