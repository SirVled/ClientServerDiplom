using MahApps.Metro.Controls;
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

namespace ClientServerDiplom
{
    /// <summary>
    /// Логика взаимодействия для FeedPublic.xaml
    /// </summary>
    public partial class FeedPublic : MetroWindow
    {
        public FeedPublic()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Загрузка окна
        /// </summary>
        /// <param name="sender">Window</param>
        /// <param name="e">Loaded</param>
        private void Start(object sender, RoutedEventArgs e)
        {


            levelUser.Content = $"Level : {Person.level}";
            nameUserLB.Text = Person.login;
            try { imageUser.Fill = new ImageBrush(new BitmapImage(new Uri(Person.image, UriKind.Absolute))); }
            catch { }
        }
    }
}
