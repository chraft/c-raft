using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Utils
{
    class Recipe
    {
        public int Id { get; set; }
        public int Amount { get; set; }
        public bool Order { get; set; }
        public Dictionary<string,string> Rows{get;set;}

    }
}
