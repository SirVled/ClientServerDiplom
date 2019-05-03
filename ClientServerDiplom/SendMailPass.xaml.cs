using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ClientServerDiplom
{
    /// <summary>
    /// Логика взаимодействия для SendMailPass.xaml
    /// </summary>
    public partial class SendMailPass : MetroWindow
    {
        public SendMailPass()
        {
            InitializeComponent();
            thisWindow = this;
        }

        private Regex regex = new Regex(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$"); // Регулярка на проверку почты;

        public static SendMailPass thisWindow { get; set; } // Окно

        private string mailU { get; set; } // Почта пользователя;
        public static int codeU { get; set; } // Код восстановления; 

        #region Работа с UI элементами на форме

        /// <summary>
        /// Получение фокуса у TextBox
        /// </summary>
        /// <param name="sender">TextBox</param>
        /// <param name="e">GotFocus</param>
        private void GotFocusTB(object sender, RoutedEventArgs e)
        {
            Border s = (Border)(sender as TextBox).Parent;
            s.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF8B93B8"));
        }

        /// <summary>
        /// Потеря фокуса у TextBox
        /// </summary>
        /// <param name="sender">TextBox</param>
        /// <param name="e">LostFocus</param>
        private void LostFocusTB(object sender, RoutedEventArgs e)
        {
            Border s = ((sender is TextBox) == true) ? (Border)(sender as TextBox).Parent : (Border)(sender as PasswordBox).Parent;
            s.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6D738E"));
        }

        /// <summary>
        /// Нажатие на окно
        /// </summary>
        /// <param name="sender">Window</param>
        /// <param name="e">MouseLeftDown</param>
        private void LostFocusUIE(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            butOk.Focus();
        }

        /// <summary>
        /// Установка правил на ввод данных в TextBox
        /// </summary>
        /// <param name="sender">TextBox</param>
        /// <param name="e">TextInput</param>
        private void TextValidInput(object sender, TextChangedEventArgs e)
        {      
            butOk.IsEnabled = regex.IsMatch((sender as TextBox).Text);           
        }

        /// <summary>
        /// Нажатие на OK
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Click</param>
        private void OkBut(object sender, RoutedEventArgs e)
        {
            mailU = emailTB.Text;      
            OperationServer.SendMsgClient(128, 101, mailU);           
        }
       
        /// <summary>
        /// Повторная отправка сообщения с кодом на почту пользователя
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Click</param>
        private void SendMessageMail(object sender, RoutedEventArgs e)
        {
            OperationServer.SendMsgClient(128, 102, mailU);
            codeText.Text = codeText.Text.Remove(codeText.Text.Length - 5, 5);
            SetTimer();
            (sender as Button).IsEnabled = false;
        }

        /// <summary>
        /// Подтверждение ввода восстановительного кода
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Click</param>
        private void СonfirmRecovery(object sender, RoutedEventArgs e)
        {
            if (code.Text.Equals(codeU.ToString()))
            {
                MessageBox.Show("Пароль отправлен вам на почту");
                OperationServer.SendMsgClient(128, 103, mailU);
                Close();
            }
            else
                MessageBox.Show("Неверно введен код!");
        }

        #endregion

        /// <summary>
        /// Отображение панели с кодом потверждения
        /// </summary>
        /// <param name="isVisibility">Состояние отображения</param>
        public void ShowPanelCode(bool isVisibility)
        {
            thisWindow.Dispatcher.BeginInvoke(new ThreadStart(() => { 

                if (isVisibility)
                {
                    panelCode.Visibility = Visibility.Visible;
                    butOk.Visibility = Visibility.Hidden;
                    errorLb.Visibility = Visibility.Hidden;
                    textMsg.Text = textMsg.Text.Replace("###", emailTB.Text.Trim());
                    SetTimer();
                    this.BeginAnimation(HeightProperty, StyleUIE.AnimationPanel(this, TimeSpan.FromMilliseconds(450), 450));
                    OperationServer.SendMsgClient(128, 102, mailU);
                }
                else
                {
                    double opacity = 0;
                    DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
                    errorLb.Visibility = Visibility.Visible;
                    butOk.IsEnabled = false;
                    timer.Tick += (s, e) =>
                    {
                        errorLb.Opacity = opacity += 0.05;

                        if(opacity >= 1)
                        {
                            butOk.IsEnabled = true;
                            timer.Stop();
                        }
                    };

                    timer.Start();
                }
            }));
        }

        /// <summary>
        /// Установка таймера на повторное отправление кода 
        /// </summary>
        private void SetTimer()
        {
            DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            int minute = 1;
            int second = 30;

            string textCode = codeText.Text;

            timer.Tick += (s, es) =>
            {
                string time = (second > 10) ? $"0{minute}:{--second}" : $"0{minute}:0{--second}";
                codeText.Text = textCode + time;
                if(second == 0)
                {
                    if (minute == 0)
                    {
                        timeCodeBut.IsEnabled = true;
                        timer.Stop();
                    }
                    else
                    {
                        second = 60;
                        minute--;
                    }
                }
              
            };

            timer.Start();
        }   
    }
}
