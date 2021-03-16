using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatFeed.Class
{
    public class DisplayCommandModel
    {

        //Properties
        public int ID { get; set; }
        public string Name { get; set; }
        public string Command { get; set; }

        public DisplayCommandModel()
        {

        }

        public DisplayCommandModel(int iD, string name, string command)
        {
            ID = iD;
            Name = name;
            Command = command;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
