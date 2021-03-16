using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Text;

namespace StatFeed.Class
{
    public class GameModel
    {
        //Current UserName
        public static string Current_UserName;

        //Properties

        public int ServiceTypeID;
        public int ID { get; set; }
        public string Name { get; set; }
        public string Platform_PC { get; set; }
        public string Platform_Xbox { get; set; }
        public string Platform_PSN { get; set; }

        public string BackgroundURL { get; set; }

        //Instantiating a new game (Without any variables assigned)
        public GameModel()
        {
        }

        //Instantiating a new platform
        public GameModel(int a, string b, string pc, string xbox, string psn, string backgroundURL)
        {
            ServiceTypeID = 1;
            ID = a;
            Name = b;
            Platform_PC = pc;
            Platform_Xbox = xbox;
            Platform_PSN = psn;
            BackgroundURL = backgroundURL;
        }

        //Methods 
        
        public override string ToString()
        {
            return Name;
        }
        
        public static string GetSteamID(string UserName)
        {
            //this function takes the stored or typed in Username and converts it to a steam ID to use 
            var SteamID_prefixurl = "http://api.steampowered.com/ISteamUser/ResolveVanityURL/v0001/?key=27619344E1EF913B5337CB9D0B3F186E&vanityurl=";
            var SteamID_url = SteamID_prefixurl + UserName;

            WebClient SteamID_client = new WebClient();
            string profileinfocode = SteamID_client.DownloadString(SteamID_url);

            dynamic SteamID_dobj = JsonConvert.DeserializeObject<dynamic>(profileinfocode);

            //this sets the steamid variable to the SteamID of the username that was just entered
            string steamId = SteamID_dobj["response"]["steamid"].ToString();
            return steamId;


        }       
        public static string FormatValue(string value)
        {
            if (value == null)
                value = "value";
            if (value.Length == 0)
                return value;

            //Removes weird characters
            value = value.Replace("„", "");
            value = value.Replace("¢", "");
            value = value.Replace("â", "");
            value = value.Replace('_', ' ');
            value = value.Replace('.', ' ');
            value = value.Replace(',', ' ');


            StringBuilder result = new StringBuilder(value);

            result[0] = char.ToUpper(result[0]);
            for (int i = 1; i < result.Length; ++i)
            {
                if (char.IsWhiteSpace(result[i - 1]))
                    result[i] = char.ToUpper(result[i]);
            }

            return result.ToString();
        }       
    }
}
