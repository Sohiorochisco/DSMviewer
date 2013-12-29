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
using IdentifiedObjects;

namespace DSMviewer
{
    /// <summary>
    /// Interaction logic for LocationDisplay.xaml
    /// </summary>
    public partial class LocationDisplay : Page
    {
        internal LocationDisplay(ViewHelper thisHelper,Location location)
        {
            helper = thisHelper;
            locationIconGrid = new Grid();
            makeGridPattern();
            helper.BuildLocationView(location, setLocationIcons);
            InitializeComponent();
        }

        private bool setLocationIcons(int column, int row, System.Windows.Controls.Image image)
        {
            Grid.SetRow(image, row);
            Grid.SetColumn(image, column);
            locationIconGrid.Children.Add(image);
            return true;
        }
        private void makeGridPattern()
        {
            for (int x = 0; x < 16; x++)
            {
                locationIconGrid.RowDefinitions.Add(new RowDefinition());
            }
            for (int y = 0; y < 16; y++)
            {
                locationIconGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
        }

        ViewHelper helper;
        Grid locationIconGrid;
    }
}
