using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    internal abstract class StyleUIE
    {

        public static Regex regex = new Regex(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$"); // Регулярка на проверку почты;

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

            panelGot.BeginAnimation(FrameworkElement.MarginProperty, AnimationPanel(panelGot, TimeSpan.FromMilliseconds(350), new Thickness(0), but1));
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
        private static AnimationTimeline AnimationPanel(Grid panel, TimeSpan duration, Thickness moveTo, Button but = null)
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

        #endregion

        #region PersonArea.xaml.cs


        #endregion

        /// <summary>
        /// Перемещени UI объекта на форме
        /// </summary>
        /// <typeparam name="T">UI элементы</typeparam>
        /// <param name="obj">Объект к который будет "анимирован"</param>
        /// <param name="duration">Время за которое анимация завершится</param>
        /// <param name="moveTo">Путь</param>
        /// <param name="completed">Событие Completed</param>
        /// <returns>Анимация</returns>
        public static ThicknessAnimation AnimationObject<T>(T obj, TimeSpan duration, Thickness moveTo, EventHandler completed = null)
        {
            ThicknessAnimation animation = new ThicknessAnimation
            {
                To = moveTo,
                Duration = duration
            };

            if (completed != null)
                animation.Completed += completed;

            return animation;
        }

        /// <summary>
        /// Создание анимации
        /// </summary>
        /// <param name="from">Начальная точка</param>       
        /// <param name="duration">Время движения анимации</param>
        /// <param name="to">Конечная точка</param>
        /// <param name="completed">Действие завершения</param>
        /// <returns>Анимация</returns>
        public static DoubleAnimation DoubleAnimation(double from, TimeSpan duration, double to, EventHandler completed = null)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                Duration = duration,
                From = from,
                To = to
            };

            if (completed != null)
            {
                animation.Completed += completed;
            }

            return animation;
        }
    }
}
