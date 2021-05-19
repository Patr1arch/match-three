using System;
using System.Collections.Generic;
using System.Text;

namespace TestTaskGF.Models
{
    class Player
    {
        public int Points { get; set; }
        public string Name { get; set; }

        public Player(string _name, int _pts)
        {
            Name = _name;
            Points = _pts;
        }
    }
}
