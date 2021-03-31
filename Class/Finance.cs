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

        public static List<string> CryptocurrencyAutoFill(int ServiceTypeID)
        {
            List<string> CryptoAutoFillList = new List<string>();
                        
            //Returns list of all Cryptocurrency symbols
            if (ServiceTypeID == 1)
            {
                try
                {
                    string URL_Header = "";
                    string URL = "https://api.binance.com/api/v3/ticker/24hr";

                    //Returns JSON Object of API result
                    dynamic dobj = StatModel.ReturnDobj(URL, URL_Header);
                    
                    foreach (var SymbolElement in dobj)
                    {
                        string TickerName = SymbolElement["symbol"];
                        CryptoAutoFillList.Add(TickerName);
                    }

                    CryptoAutoFillList.Sort();
                    return CryptoAutoFillList;
                }
                catch
                {
                    
                }
            }
            //Returns all the US Market symbols
            if (ServiceTypeID == 2)
            {
                try
                {
                    //Create URL
                    string URL_Header = "";
                    string URL_Prefix = "https://finnhub.io/api/v1/stock/symbol?exchange=US&token=";
                    string Temp_APIKey = "c14ddmf48v6t40fvdb2g";


                    string URL = URL_Prefix + Temp_APIKey;

                    //Returns JSON Object of API result
                    dynamic dobj = StatModel.ReturnDobj(URL, URL_Header);

                    foreach (var SymbolElement in dobj)
                    {
                        string TickerName = SymbolElement["symbol"];
                        CryptoAutoFillList.Add(TickerName);
                    }

                    CryptoAutoFillList.Sort();
                    return CryptoAutoFillList;
                }
                catch
                {

                }
            }


            return CryptoAutoFillList;
        }
    }
}
