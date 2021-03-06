﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace StatFeed.Class
{
    
    class DisplayModel
    {
        //Properties
        public static string Format_statvalue;        
        
        //Methods
        public static string FormatTo000000(string statvalue)
        {
            //Uses a try and catch statement to ensure that any stat values that aren't numbers still get displayed (like percentages etc)
            try
            {
                //Converts value into integer
                int int_statvalue = Int32.Parse(statvalue);

                //Formats number to be 6 0's long
                Format_statvalue = string.Format("{0:D6}", int_statvalue);
                return Format_statvalue;
            }
            catch
            {
                Format_statvalue = statvalue;
                return Format_statvalue;
            }            
        }

        //Method for senddata
        public static void SendToPort(string stat, string CurrentPort)
        {
            try
            {                
                //Comm serial port to arduino                    
                SerialPort checkport = new SerialPort(CurrentPort, 9600);
                checkport.ReadTimeout = 2000;
                checkport.WriteTimeout = 2000;
                checkport.Open();
                checkport.WriteLine(stat);                
                checkport.Close();
            }
            catch
            {
                //Display has been unplugged in between the load and now.
                //Clear the current COM port saved in database 
                SqliteDataAccess.SetLastCOMPort("No Port");
            }
        }

        //Method for findingport (Autoconnect, no need for text file creation is it's using an arduino that's transmitting the same code)
        public static string[] FindAllPorts()
        {            
            string[] AvailablePorts;   
            AvailablePorts = SerialPort.GetPortNames();

            if (AvailablePorts.Length > 0)
            {
                //If there are ports in this list then return the list
                return AvailablePorts;
            }
            else
            {
                //if there are NOT any ports available then return "No Port"
                string[] NoPort = new string[1] { "No Port" };
                return NoPort;
            }            
        }

        public static bool SearchForLastDisplay()
        {
            //This takes the last know value (From database) and compares it to what can be found through SerialPort.GetPortNames()
            string CurrentPort = SqliteDataAccess.GetLastCOMPort();
            string[] FoundPorts = FindAllPorts();

            foreach (var Port in FoundPorts)
            {
                //If the current port matches any names from the list of available ports then it's available and returns true
                if (CurrentPort == Port)
                {
                    if (CurrentPort != "No Port")
                    {
                        //Device is connected 
                        return true;
                    }                    
                }                
            }
            //If no ports are found (Because no display is connected)
            return false;
        }
    }
}
