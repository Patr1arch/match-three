using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace TestTaskGF
{
    public enum Figure : int { Triangle = '\u25B2', Square = '\u25A0', Cicle = '\u25CF', Diamond = '\u25C6', Star = '\u2605' }
    class GameButton : Button
    {
        Figure currFigure;
        public Figure CurrentFigure
        {
            get { return currFigure; }
            set
            {
                Content = (char)value;
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
