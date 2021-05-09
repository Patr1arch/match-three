using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace TestTaskGF
{
    public enum Figure : int { Triangle = '\u25B2', Square = '\u25A0', Cicle = '\u25CF', Diamond = '\u25C6', Star = '\u2605' }
    class GameButton : Button
    {
        public Figure CurrentFigure { get; set; }
    }
}
