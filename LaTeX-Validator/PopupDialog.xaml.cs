using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LaTeX_Validator.Extensions;

namespace LaTeX_Validator
{
    /// <summary>
    /// Interaktionslogik für PopupDialog.xaml
    /// </summary>
    public partial class PopupDialog : UserControl
    {
        public PopupDialog()
        {
            this.InitializeComponent();
        }

        private void PopupOk_Clicked(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            this.WindowIsClosing?.Invoke(this.LabelsBox.Text, this.FillwordsBox.Text);
        }

        public event Action<string,string> WindowIsClosing = null!;
    }
}
