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
using System.Windows.Shapes;

namespace DSMviewer
{
    /// <summary>
    /// Interaction logic for ChooseContainer.xaml
    /// </summary>
    public partial class ChooseContainer : Window
    {
        public ChooseContainer(IEnumerable<string> theseLineNames)
        {
            lineNames = theseLineNames;
            InitializeComponent();
            this.DataContext = lineNames;
        }
        IEnumerable<string> lineNames;

        private void selectLine(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            SelectedLine = comboBox.SelectedItem as string;
        }

        private void chooseLine(object sender, RoutedEventArgs e)
        {
            if (SelectedLine != null)
            {
                this.DialogResult = true;
            }
        }

        internal string SelectedLine{private set; get;}

        
        
    }
}
