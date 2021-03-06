using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatFeed.Class
{
    public class ComboBoxPair
    {
        public string Name { get; set; }
        public int ID { get; set; }

        public ComboBoxPair (int iD, string name)
        {
            ID = iD;
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

        public List<ComboBoxPair> MyPairs
        {
            get 
            { 
                return MyPairs;
            }
        }
    }
}
