using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace osu_CollectionManager
{
    /// <summary>
    /// Logique d'interaction pour BeatmapInput.xaml
    /// </summary>
    public partial class BeatmapInput : Window
    {
        public BeatmapInput(bool valid)
        {
            InitializeComponent();
            if (valid)
            {
                InvalidLabel.Visibility = Visibility.Hidden;
                InvalidLabel.Margin = new Thickness(0, 0, 0, 28);
            }
            this.SizeToContent = SizeToContent.WidthAndHeight;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            AnswerBox.Focus();
            AnswerBox.SelectAll();
        }

        public string Answer
        {
            get { return AnswerBox.Text; }
        }
    }
}
