using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ClientServerDiplom
{

    /// <summary>
    /// Класс, стилей и логики с анимациями
    /// </summary>
    internal abstract class  StyleUIE 
    {
        #region Authorization.xaml.cs

        /// <summary>
        /// Установка фокуса для кнопки
        /// </summary>
        /// <param name="take">Получение фокуса</param>
        /// <param name="lost">Потеря фокуса</param>
        public static void SetFocusButton(Button take, Button lost)
        {
            int fontSize = (int)take.FontSize;
            
            take.Foreground = Brushes.White;
            take.FontSize = lost.FontSize;
            take.BorderThickness = new Thickness(0, 0, 0, 3);

            lost.FontSize = fontSize;
            lost.Foreground = Brushes.Gray;
            lost.BorderThickness = new Thickness(0);
        }

        /// <summary>
        /// Меняет местами панели
        /// </summary>
        /// <param name="panelGot">Панель которая получила фокус</param>
        /// <param name="panelSign2">Панель которая потеряла фокус</param>
        /// <param name="but1">Кнопка которая произвела дейстивие</param>
        /// <param name="but2">Вторая кнопка</param>
        public static void SwapSignPanel(Grid panelGot, Grid panelLost, Button but1, Button but2)
        {
            but1.IsEnabled = false;
            but2.IsEnabled = false;

            panelGot.BeginAnimation(FrameworkElement.MarginProperty, AnimationPanel(panelGot, TimeSpan.FromMilliseconds(350), new Thickness(0)  , but1));
            Thickness marginPanelLost = (panelLost.Name.Equals("signUpGrid")) ? new Thickness(panelLost.ActualWidth + 15, 0, -panelLost.ActualWidth - 15, 0) : new Thickness(-panelLost.ActualWidth - 15, 0, panelLost.ActualWidth + 15, 0);
            panelLost.BeginAnimation(FrameworkElement.MarginProperty, AnimationPanel(panelLost, TimeSpan.FromMilliseconds(350), marginPanelLost, but2));
        }
        /// <summary>
        /// Создание анимации (передвижения вправо\влево)
        /// </summary>
        /// <param name="panelSign">Панель которая движется</param>       
        /// <param name="duration">Время движения анимации</param>
        /// <param name="moveTo">Конечная точка</param>
        /// <param name="but">Кнопка на которую было произведено действие</param>
        /// <returns>Анимация</returns>
        private static AnimationTimeline AnimationPanel(Grid panel, TimeSpan duration , Thickness moveTo , Button but = null)
        {
            ThicknessAnimation animation = new ThicknessAnimation
            {
                Duration = duration, 
                From = panel.Margin,
                To = moveTo
            };

            if (but != null)
            {
                animation.Completed += (s, e) =>
                {
                    but.IsEnabled = true;
                };
            }
            return animation;
        }

        #endregion

        #region SendMailPass.xaml.cs

        /// <summary>
        /// Создание анимации (передвижения вправо\влево)
        /// </summary>
        /// <param name="window">Окно которая движется</param>       
        /// <param name="duration">Время движения анимации</param>
        /// <param name="moveTo">Конечная точка</param>
        /// <param name="but">Кнопка на которую было произведено действие</param>
        /// <returns>Анимация</returns>
        public static AnimationTimeline AnimationPanel(Window window, TimeSpan duration, double moveTo, Button but = null)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                Duration = duration,
                From = window.ActualHeight,
                To = moveTo
            };

            if (but != null)
            {
                animation.Completed += (s, e) =>
                {
                    but.IsEnabled = true;
                };
            }
            return animation;
        }

        #endregion
    }
}
