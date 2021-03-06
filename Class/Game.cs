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
        public int GameID { get; set; }
        public string GameName { get; set; }
        public string Platform_PC { get; set; }
        public string Platform_Xbox { get; set; }
        public string Platform_PSN { get; set; }

        public string BackgroundURL { get; set; }

        //Instantiating a new game (Without any variables assigned)
        public GameModel()
        {
        }

        //Instantiating a new platform
        public GameModel(int a, string b, string pc, string xbox, string psn, string BackgroundURL)
        {
            GameID = a;
            GameName = b;
            Platform_PC = pc;
            Platform_Xbox = xbox;
            Platform_PSN = psn;
        }

        //Methods 
        
        public override string ToString()
        {
            return GameName;
        }
        public static bool CheckUsername(int GameID, int chosen_service, string UserName)
        {
            //checks to see if username exists

            //Fortnite 
            if (GameID == 1)
            {
                try
                {
                    //checks to see if fortnite stats exist against name and if so adds the object to the games dropdown                
                    var Fortnite_header = "TRN-Api-Key: 417501fe-eeb1-45d1-ace6-57a28ebfc8d5";
                    var Fortnite_prefixurl = "https://api.fortnitetracker.com/v1/profile/";
                    //sends type of service and PlatformID to a method that returns the correct abbreviation for the service
                    string Fortnite_platformName = SqliteDataAccess.GetGameAbbreviation(GameID, chosen_service);


                    var Fortnite_url = Fortnite_prefixurl + Fortnite_platformName + "/" + UserName + "/";

                    WebClient Fortnite_client = new WebClient();

                    Fortnite_client.Headers.Add(Fortnite_header);

                    string profileinfocode = Fortnite_client.DownloadString(Fortnite_url);

                    //convert the JSON string to a series of objects
                    dynamic dobj = JsonConvert.DeserializeObject<dynamic>(profileinfocode);

                    dynamic UsernameCheck = dobj.epicUserHandle;

                    if (UsernameCheck is null)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }
            //Apex Legends 
            if (GameID == 2)
            {
                try
                {
                    var Apex_Legends_header = "TRN-Api-Key: 417501fe-eeb1-45d1-ace6-57a28ebfc8d5";
                    var Apex_Legends_prefixurl = "https://public-api.tracker.gg/v2/apex/standard/profile/";
                    string Apex_Legends_platformName = SqliteDataAccess.GetGameAbbreviation(GameID, chosen_service);

                    var Apex_Legends_url = Apex_Legends_prefixurl + Apex_Legends_platformName + "/" + UserName + "/";

                    WebClient Apex_Legends_client = new WebClient();

                    Apex_Legends_client.Headers.Add(Apex_Legends_header);

                    string profileinfocode = Apex_Legends_client.DownloadString(Apex_Legends_url);

                    //convert the JSON string to a series of objects
                    dynamic dobj = JsonConvert.DeserializeObject<dynamic>(profileinfocode);

                    dynamic UsernameCheck = dobj.data.platformInfo.platformUserHandle;

                    if (UsernameCheck is null)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }
            //Overwatch 
            if (GameID == 3)
            {
                try
                {
                    //checks to see if Overwatch stats exist against name and if so adds the object to the games dropdown                
                    var Overwatch_header = "TRN-Api-Key: 417501fe-eeb1-45d1-ace6-57a28ebfc8d5";
                    var Overwatch_prefixurl = "https://public-api.tracker.gg/v2/overwatch/standard/profile/";
                    var Overwatch_platformName = "battlenet" + "/";

                    //saves original UserName and then modifies it to remove "#" and replace with "%23"
                    var Original_UserName = UserName;
                    var Modified_UserName = UserName.Replace("#", "%23");

                    var Overwatch_url = Overwatch_prefixurl + Overwatch_platformName + Modified_UserName;

                    WebClient Overwatch_client = new WebClient();

                    Overwatch_client.Headers.Add(Overwatch_header);

                    string profileinfocode = Overwatch_client.DownloadString(Overwatch_url);

                    //convert the JSON string to a series of objects
                    dynamic dobj = JsonConvert.DeserializeObject<dynamic>(profileinfocode);

                    dynamic UsernameCheck = dobj.data.platformInfo.platformUserHandle;

                    if (UsernameCheck is null)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }
            // CSGO
            if (GameID == 4)
            {
                try
                {
                    string steamid = GetSteamID(UserName);

                    var prefixurl = "http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?";
                    var midurl = "key=27619344E1EF913B5337CB9D0B3F186E&steamids=";
                    var format = "&format = json";
                    var url = prefixurl + midurl + steamid + format;


                    WebClient client = new WebClient();
                    string statinfo = client.DownloadString(url);

                    //convert the JSON string to a series of objects
                    dynamic dobj = JsonConvert.DeserializeObject<dynamic>(statinfo);

                    dynamic UsernameCheck = dobj.response.players;

                    if (UsernameCheck is null)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }


            //Takes platformid, gameid, chosen_service and username (converts if necessary) and returns a boolean on whether it's a valid username that exists
            return false;
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
