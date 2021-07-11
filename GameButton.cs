using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace TestTaskGF
{
    public enum FigureColor
    {
        Black =  -16777216,
        Blue =   -16776961,
        Green =  -16744448,
        Pink =   -2461482,
        Orange = -23296
    }
    public enum Figure : int 
    {
        F1Square = '\u25A0',
        F2Triangle = '\u25B2',
        F3Diamond = '\u25C6',
        F4Cicle = '\u25CF',
        F5Star = '\u2605',
        F6GorLine = '\u2583',
        F7VerLine = '\u258D'
    }
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
