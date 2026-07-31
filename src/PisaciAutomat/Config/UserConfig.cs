using System;
using System.Collections.Generic;
using System.Text;

namespace PisaciAutomat.Config
{
    public class UserConfig
    {
        public bool DarkMode { get; set; }

        public string Jazyk { get; set; }

        public int? UndoLimit { get; set; }

        public bool BracketHighlighted { get; set; }
    }
}
