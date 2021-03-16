using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatFeed.Class
{
    public class FinanceModel
    {
        //Properties
        public int ServiceTypeID;
        public int ID { get; set; }
        public string Name { get; set; }
        public string APIURL { get; set; }
        public bool KeyRequired { get; set; }
        public bool SecretRequired { get; set; }
        public string BackgroundURL { get; set; }


        public FinanceModel()
        {

        }

        public FinanceModel(int iD, string name, string aPIURL, bool keyRequired, bool secretRequired, string backgroundURL)
        {
            ServiceTypeID = 2;
            ID = iD;
            Name = name;
            APIURL = aPIURL;
            KeyRequired = keyRequired;
            SecretRequired = secretRequired;
            BackgroundURL = backgroundURL;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
