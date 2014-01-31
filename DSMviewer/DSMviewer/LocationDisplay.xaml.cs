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
    public partial class LocationDisplay : UserControl{
        public LocationDisplay(SchematicMaker sm){
            DataContext = sm;
            InitializeComponent();
        }

        private void ChangeState(object sender, MouseButtonEventArgs e){
            var im = (Image)sender;
            var li = (LocationIcon)im.DataContext;
            if (li.BackingObject == null){
                return;
            }
            var bo = (Equipment)li.BackingObject;
            if(bo.EquipmentType == EquipmentTopoTypes.Switch){
                Switch s = (Switch)bo;
                s.SwitchState = (s.SwitchState == SwitchState.closed)? SwitchState.open : SwitchState.closed;
                string icn = String.Format("Switch{0}",s.State());
                li.Icon = SchematicMaker.GetObjectImage(icn);
            }
            return;
        }
    }
}
