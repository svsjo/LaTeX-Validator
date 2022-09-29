using System;
using System.Windows;
using System.Windows.Controls;

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
