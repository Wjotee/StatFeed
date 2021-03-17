using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StatFeed.Class;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using Dapper;

namespace StatFeed
{
    public class SqliteDataAccess
    {
        //Properties
        public static SubscribedGameModel CurrentSubscriptionSelected;
        public static GameModel CurrentGameSubscriptionSelected;


        //MISC Methods
        private static string LoadConnectionString(string id = "Default")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }
        public static bool FirstTime()
        {
            //This counts the returned objects of SubscribedGames to check if it's the users first time on the software or not
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<SubscribedGameModel>("select * from Subscription", new DynamicParameters());
                int count = output.Count();

                if (count > 0)
                {
                    //This isn't the users first time
                    return false;
                }
                else
                {
                    //This is the users first time
                    return true;
                }
            }
        }
                

        //GAME Methods
        public static List<GameModel> GetAvailableGames()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var AvailableGames = cnn.Query<GameModel>("select * from Game", new DynamicParameters()).ToList();
                return AvailableGames;
            }
        }
        public static GameModel SelectGame(int ID)
        {
            GameModel Game = new GameModel();

            //Takes GameID and returns object of that game
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                List<GameModel> Games = cnn.Query<GameModel>("select * from Game where ID =" + ID, new DynamicParameters()).ToList();

                foreach (var item in Games)
                {
                    Game = item;
                }
            }

            return Game;
        }
        public static string GetGameAbbreviation(int GameID, int Chosen_Service)
        {
            string Abbreviation = "0";

            //Takes GameID and returns object of that game
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                List<GameModel> Games = cnn.Query<GameModel>("select * from Game where ID =" + GameID, new DynamicParameters()).ToList();

                foreach (var item in Games)
                {
                    //If Chosen_Service is PC
                    if (Chosen_Service == 1)
                    {
                        Abbreviation = item.Platform_PC;
                    }
                    //If Chosen_Service is Xbox
                    if (Chosen_Service == 2)
                    {
                        Abbreviation = item.Platform_Xbox;
                    }
                    //If Chosen_Service is PSN
                    if (Chosen_Service == 3)
                    {
                        Abbreviation = item.Platform_PSN;
                    }
                }
            }
            return Abbreviation;
        }


        //FINANCE Methods
        public static List<FinanceModel> GetAvailableFinance()
        {
             using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var AvailableFinance = cnn.Query<FinanceModel>("select * from Finance", new DynamicParameters()).ToList();
                return AvailableFinance;
            }
        }
        public static FinanceModel SelectFinance(int ID)
        {
            FinanceModel Finance = new FinanceModel();

            //Takes GameID and returns object of that game
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                List<FinanceModel> Finances = cnn.Query<FinanceModel>("select * from Finance where ID =" + ID, new DynamicParameters()).ToList();

                foreach (var item in Finances)
                {
                    Finance = item;
                }
            }

            return Finance;
        }


        //SUBSCRIPTION Methods
        public static SubscribedGameModel GetSubscription(int SubscriptionID)
        {
            SubscribedGameModel Subscription = new SubscribedGameModel();

            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                List<SubscribedGameModel> Subscriptions = cnn.Query<SubscribedGameModel>("select *  from Subscription where SubscriptionID =" + SubscriptionID, new DynamicParameters()).ToList();

                foreach (var item in Subscriptions)
                {
                    Subscription = item;
                }
            }
            return Subscription;
        }        
        public static List<SubscribedGameModel> GetSubscriptionList()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                List<SubscribedGameModel> output = cnn.Query<SubscribedGameModel>("select * from Subscription", new DynamicParameters()).ToList();
                return output;
            }
        }
        public static void SaveSubscribedGame(int ServiceTypeID, int ID, string UserName, int Chosen_Service, string APIKey, string APISecret, int Last_Selected, string Custom_Background)
        {
            //Takes GameID of current game, Username (After it has been verified) and the Chosen_Service and writes it to the table
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute("insert into Subscription (ServiceTypeID, ID, UserName, Chosen_Service, APIKey, APISecret, Last_Selected, Custom_Background) values('" + ServiceTypeID + "','" + ID + "','" + UserName + "','" + Chosen_Service + "','" + APIKey + "','" + APISecret + "','" + Last_Selected + "','" + Custom_Background + "')");
            }
        }
        public static void SetToDefaultBackgroundSubscription(int SubscriptionID)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute("UPDATE Subscription SET Custom_Background = 'Default' WHERE SubscriptionID =" + SubscriptionID);
            }
        }
        public static void DeleteSubscribedGame(int SubscriptionID)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Query<SubscribedGameModel>("delete from Subscription where SubscriptionID =" + SubscriptionID, new DynamicParameters());
            }
        }


        //STAT Methods
        public static List<StatModel> GetStats(int SubscriptionID)
        {
            //Takes the SubscriptionID of the currently selected game and returns a list that can be placed into the Stat Combobox
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                List<StatModel> Stats = cnn.Query<StatModel>("select * from Stat where SubscriptionID =" + SubscriptionID, new DynamicParameters()).ToList();
                return Stats;
            }
        }
        public static void SaveStats(List<StatModel> Stats)
        {
            //Takes a list of Stats and writes them to the Stat table. This could either be the whole subscription list of stats or could be just one subscription (To save on API usage)            
            foreach (StatModel stat in Stats)
            {
                //Checks if stat exists. Will update or insert based on that search                
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    //Checks to see if the stat exists
                    List<StatModel> output = cnn.Query<StatModel>("select * from stat where SubscriptionID = @SubscriptionID and StatName = @StatName", stat).ToList();

                    //If the list comes back empty then INSERT information
                    if (output.Count == 0)
                    {
                        cnn.Execute("insert into Stat (SubscriptionID, StatName, StatValue_1, StatValue_2, StatValue_3) values(@SubscriptionID, @StatName, @StatValue_1, @StatValue_2, @StatValue_3)", stat);
                    }
                    //If the list comes back full meaning that a stat already exists then UPDATE information
                    else
                    {
                        cnn.Execute("Update Stat Set StatValue_1 = @StatValue_1, StatValue_2 = @StatValue_2, StatValue_3 = @StatValue_3 where SubscriptionID = @SubscriptionID and StatName = @StatName", stat);
                    }
                }
            }
        }
        public static void DeleteStats(int SubscriptionID)
        {
            //Takes the SubscriptionID, searches the database and deletes what it finds
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Query<SubscribedGameModel>("delete from Stat where SubscriptionID =" + SubscriptionID, new DynamicParameters());
            }
        }

        public static StatModel GetLastSelectedStat()
        {
            StatModel output = new StatModel();

            //This gets the current ID for the Display Command
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {               

                List<StatModel> TempStat = cnn.Query<StatModel>("select * from STAT where Last_Selected = 1", new DynamicParameters()).ToList();

                foreach (var Stat in TempStat)
                {
                    output = Stat;
                }
            }
            return output;
        }


        //USER SETTINGS Methods
        public static void SetGamesComboCheckpoint(SubscribedGameModel Subscription)
        {
            //This will revert all the Last_Selected properties to "0" and then set the passed subscriptionID to "1"
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                //This sets all the values to 0
                cnn.Execute("update Subscription set Last_Selected = 0");
                //This sets the Last_Selected to 1 for that last selected game
                cnn.Execute("update Subscription set Last_Selected = 1 where SubscriptionID = @SubscriptionID", Subscription);
            }
        }
        public static void SetStatsComboCheckpoint(int StatID)
        {
            //This will revert all the Last_Selected properties to "0" and then set the passed StatID to "1"
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                //This sets all the values to 0
                cnn.Execute("update Stat set Last_Selected = 0");
                //This sets the Last_Selected to 1 for that last selected stat
                cnn.Execute("update Stat set Last_Selected = 1 where StatID =" + StatID);
            }
        }
        public static int GetGamesComboCheckpoint()
        {
            //Finds the subscription with the Last_Selected property set to "1"
            int SubscriptionID = 0;

            //Takes GameID and returns object of that game
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                List<SubscribedGameModel> Subscription = cnn.Query<SubscribedGameModel>("select * from Subscription where Last_Selected = 1").ToList();

                //If no subscription was last selected 
                if (Subscription.Count == 0)
                {
                    List<SubscribedGameModel> TopSubscription = cnn.Query<SubscribedGameModel>("select * from Subscription limit 1").ToList();

                    foreach (var item in TopSubscription)
                    {
                        SubscriptionID = item.SubscriptionID;
                        return SubscriptionID;
                    }
                }
                else
                {
                    foreach (var item in Subscription)
                    {
                        SubscriptionID = item.SubscriptionID;
                        return SubscriptionID;
                    }
                }
            }
            return SubscriptionID;
        }
        public static int GetStatsComboCheckpoint(int SubscriptionID)
        {
            //Finds the subscription with the Last_Selected property set to "1"
            int StatID = 0;

            //To check that this stat is related to the game chosen, pass the CurrentGame SubscriptionID. If the Stat shares this then leave it
            //If the stat does not have the same subscription ID (The game combobox has been changed by the user) then reset it to the top selected 
            //Of that CurrentGame SubscriptionID

            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                //Creates a list and populates it with the last selected stat of that CurrentGame
                List<StatModel> StatCheck = cnn.Query<StatModel>("select * from Stat where Last_Selected = 1 and SubscriptionID =" + SubscriptionID).ToList();

                //If the game has been changed the list will return empty 
                if (StatCheck.Count == 0)
                {
                    //This will return the top result of the CurrentGame
                    List<StatModel> Stat = cnn.Query<StatModel>("select * from stat where SubscriptionID = " + SubscriptionID + " limit 1" + SubscriptionID).ToList();

                    foreach (var item in Stat)
                    {
                        StatID = item.StatID;
                        return StatID;
                    }

                }
                else
                {
                    foreach (var item in StatCheck)
                    {
                        StatID = item.StatID;
                        return StatID;
                    }
                }
            }
            return StatID;
        }


        //DISPLAY COMMANDS (USER SETTINGS) Methods
        public static int GetCurrentDisplayCommandID()
        {
            int output = 1;

            //This gets the current ID for the Display Command
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                List<int> DisplayCommandID = new List<int>();

                DisplayCommandID = cnn.Query<int>("select DisplayCommandID from UserSettings limit 1").ToList();

                foreach (var ID in DisplayCommandID)
                {
                    output = ID;                    
                }
            }
            return output;
        }
        public static DisplayCommandModel GetCurrentDisplayCommand()
        {
            //Takes the CurrentDisplayCommandID and returns the correct object from the DisplayCommands Table
            int CurrentDisplayCommandID = GetCurrentDisplayCommandID();

            //Creates new object for return
            DisplayCommandModel CurrentDisplayCommand = new DisplayCommandModel();

            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                List<DisplayCommandModel> output = cnn.Query<DisplayCommandModel>("select * from DisplayCommands where ID = " + CurrentDisplayCommandID, new DynamicParameters()).ToList();

                foreach (var item in output)
                {
                    CurrentDisplayCommand = item;
                    return CurrentDisplayCommand;
                }                
            }
            return CurrentDisplayCommand;
        }
        public static List<DisplayCommandModel> GetAllDisplayCommands()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                List<DisplayCommandModel> DisplayCommands = cnn.Query<DisplayCommandModel>("select * from DisplayCommands", new DynamicParameters()).ToList();
                return DisplayCommands;
            }
        }
        public static void SetDisplayCommand(int DisplayCommandID)
        {
            //This sets to the latest display command
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {                
                cnn.Execute("update UserSettings set DisplayCommandID = " + DisplayCommandID);
            }
        }
        public static void SetDisplayBrightness(string BrightnessSetting)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute("update UserSettings set DisplayBrightnessSetting = '" + BrightnessSetting + "'");
            }
        }
        public static string GetCurrentDisplayBrightness()
        {
            string output = "High";

            List<string> BrightnessSetting = new List<string>();
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                BrightnessSetting = cnn.Query<string>("select DisplayBrightnessSetting from UserSettings limit 1").ToList();

                foreach (var value in BrightnessSetting)
                {
                    output = value;
                }
            } 
            return output;
        }

        //SERVICE Methods
        public static int GetServiceUpdateTimerDuration(int ServiceID)
        {
            int UpdateTime = 150;

            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                List<ServiceTypeModel> output = cnn.Query<ServiceTypeModel>("select* from Service where ServiceID =" + ServiceID, new DynamicParameters()).ToList();

                foreach (var item in output)
                {
                   UpdateTime  = item.ServiceUpdateTimer;
                }                
            }

            return UpdateTime;
        }


        //COM PORT Methods
        public static string GetLastCOMPort()
        {
            //This gets the last COM port that the user saved
            string output = "No Port";

            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                List<string> COMport = new List<string>();

                COMport = cnn.Query<string>("select COM_Port from UserSettings limit 1").ToList();

                foreach (var item in COMport)
                {
                    output = item;
                    return output;
                }

            }
            return output;
        }
        public static void SetLastCOMPort(string COMPort)
        {
            //This takes a COM port value and sets it to the passed parameter
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute("update UserSettings set COM_Port = '" + COMPort + "'");
            }
        }
    }
}



