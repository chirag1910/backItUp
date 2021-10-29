using System.Windows;
using System.Windows.Controls;

namespace BackItUp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        public static Frame mainFrame;
        public MainWindow()
        {
            InitializeComponent();

            mainFrame = contentFrame;

            this.SizeChanged += (s, e) => {
                if (e.NewSize.Width > 1000)
                {
                    navbarColumn.Width = new GridLength(260);
                }
                else
                {
                    navbarColumn.Width = new GridLength(60);
                }
            };
        }
    }
}
