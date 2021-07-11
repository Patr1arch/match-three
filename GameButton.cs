using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace TestTaskGF
{
    public enum FigureColor
    {
        Blue = -16776961,
        Black = -16777216,
        Pink = -2461482,
        Orange = -23296,
        Green = -16744448
    }
    public enum Figure : int { Triangle = '\u25B2', Square = '\u25A0', Cicle = '\u25CF', Diamond = '\u25C6', Star = '\u2605' }
    class GameButton : Button
    {
        private (Figure, FigureColor) currFigure;
        public (Figure, FigureColor) CurrentFigure
        {
            get { return currFigure; }
            set
            {
                Content = (char)value.Item1;
                Foreground = (SolidColorBrush) new BrushConverter().
                    ConvertFromString("#" + value.Item2.ToString("X"));
                currFigure = value;
            }
        }

        public void SwapFigures(GameButton gameButton)
        {
            var temp = CurrentFigure;
            CurrentFigure = gameButton.CurrentFigure;
            // Content = (char)currFigure;
            gameButton.CurrentFigure = temp;
        }
    }
}
