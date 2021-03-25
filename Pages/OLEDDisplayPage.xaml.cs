using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using StatFeed.Class;

namespace StatFeed.Pages
{
    /// <summary>
    /// Interaction logic for OLEDDisplayPage.xaml
    /// </summary>
    public partial class OLEDDisplayPage : Page
    {


        public OLEDDisplayPage()
        {
            InitializeComponent();
            OnLoad();
        }

        public void OnLoad()
        {
            //Hide elements as default (if not display connected it stays this way)
            UpdateDisplayPage();

            //Set background 
            BitmapImage BackgroundBitmap = new BitmapImage();
            BackgroundBitmap.BeginInit();
            BackgroundBitmap.UriSource = new Uri(MainPage.CurrentBackgroundURL, UriKind.RelativeOrAbsolute);
            BackgroundBitmap.EndInit();
            Background_Image.Source = BackgroundBitmap;
        }
        private void OLEDDisplayPage_Background_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.NavigationService.GoBack();
        }
        public void HideElements()
        {
            Display_Command_Combobox.Visibility = Visibility.Hidden;
            Display_COMport_Combobox.Visibility = Visibility.Hidden;
            Display_Brightness_Combobox.Visibility = Visibility.Hidden;
            MainOLED_Game_Textbox.Text = "Oops!";            

            Not_Connected_Dialogue.Visibility = Visibility.Visible;            
        }
        public void UpdateDisplayPage()
        {
            HideElements();

            if (DisplayModel.SearchForLastDisplay())
            {  
                //Make COM port box visible
                Display_COMport_Combobox.Visibility = Visibility.Visible;
                SetComPortsComboIndex();

                //Make DisplayCommands box visible
                Display_Command_Combobox.Visibility = Visibility.Visible;
                Display_Command_Combobox.ItemsSource = PopulateDisplayCommandCombo();
                SetDisplayCommandComboboxIndex(SqliteDataAccess.GetCurrentDisplayCommandID());

                //Make DisplayBrightness combobox visible
                Display_Brightness_Combobox.Visibility = Visibility.Visible;
                Display_Brightness_Combobox.ItemsSource = PopulateDisplayBrightnessCombo();
                SetDisplayBrightnessComboboxIndex();

                //Set display mockup
                SetDisplayMockup(SqliteDataAccess.GetCurrentDisplayCommandID(), SqliteDataAccess.GetLastSavedStat());

                //Make Disconnected dialogue hidden
                Not_Connected_Dialogue.Visibility = Visibility.Hidden;
            }            
        }
        public void SetComPortsComboIndex()
        {
            //Iterate through the ports and add to the combo box
            foreach (var Port in DisplayModel.FindAllPorts())
            {
                if (Port != "No Port")
                {
                    Display_COMport_Combobox.Items.Add(Port); 

                    for (int i = 0; (i < Display_COMport_Combobox.Items.Count); i++)
                    {
                        if (Display_COMport_Combobox.Items[i].ToString() == SqliteDataAccess.GetLastCOMPort())
                        {
                            Display_COMport_Combobox.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }
        }
        public List<ComboBoxPair> PopulateDisplayCommandCombo()
        {
            var Commands = SqliteDataAccess.GetAllDisplayCommands();

            List<ComboBoxPair> myPairs = new List<ComboBoxPair>();

            foreach (var command in Commands)
            {
                myPairs.Add(new ComboBoxPair(command.ID, command.Name));
            }

            return myPairs;
        }      
        public List<string> PopulateDisplayBrightnessCombo()
        {
            List<string> BrightnessSettingsAvailable = new List<string>();

            BrightnessSettingsAvailable.Add("High");
            BrightnessSettingsAvailable.Add("Low");

            return BrightnessSettingsAvailable;
        }
        public void SetDisplayCommandComboboxIndex(int DisplayCommandID)
        {
            //This iterates through the combobox and sets the index to the last user set one

            for (int i = 0; i < Display_Command_Combobox.Items.Count; i++)
            {
                if (((ComboBoxPair)Display_Command_Combobox.Items[i]).ID == DisplayCommandID)
                {
                    Display_Command_Combobox.SelectedIndex = i;
                    break;
                }
            }
        }
        public void SetDisplayBrightnessComboboxIndex()
        {
            //This iterates through the combobox and sets the index to the last user set one

            String DatabaseBrightness = SqliteDataAccess.GetCurrentDisplayBrightness();

            for (int i = 0; i < Display_Brightness_Combobox.Items.Count; i++)
            {  
                if (Display_Brightness_Combobox.Items[i].ToString() == DatabaseBrightness)
                {
                    Display_Brightness_Combobox.SelectedIndex = i;
                    break;
                }
            }
        }
        public void SetDisplayMockup(int DisplayCommandID, StatModel currentStat)
        {      
            //This sets the visual representation on the window
            if (DisplayCommandID == 1)
            {
                //Set display to game
                MainOLED_Game_Textbox.Visibility = Visibility.Visible;
                MainOLED_Finance.Visibility = Visibility.Hidden;
                string formatstat = DisplayModel.FormatTo000000(currentStat.StatValue_1);
                MainOLED_Game_Textbox.Text = formatstat;
            }
            if (DisplayCommandID == 2)
            {
                //Set display to finance
                MainOLED_Game_Textbox.Visibility = Visibility.Hidden;
                MainOLED_Finance.Visibility = Visibility.Visible;
                Triangle_Up.Visibility = Visibility.Hidden;
                Triangle_Down.Visibility = Visibility.Hidden;
                Long_Triangle_Down.Visibility = Visibility.Hidden;
                Long_Triangle_Up.Visibility = Visibility.Hidden;

                MainOLED_Finance_StatName_Textbox.Text = currentStat.StatName;
                MainOLED_Finance_StatValue1_Textbox.Text = currentStat.StatValue_1;

                //if its shorter than 5 characters
                if (currentStat.StatName.Length < 5)
                {
                    if (currentStat.StatValue_2 != "0")
                    {

                        MainOLED_Finance_StatValue2_Textbox.Text = currentStat.StatValue_2;
                        if (currentStat.StatValue_2[0] == '-')
                        {
                            Triangle_Down.Visibility = Visibility.Visible;
                            Triangle_Up.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            Triangle_Down.Visibility = Visibility.Hidden;
                            Triangle_Up.Visibility = Visibility.Visible;
                        }
                    }
                    if (currentStat.StatValue_3 != "0")
                    {
                        MainOLED_Finance_StatValue3_Textbox.Text = currentStat.StatValue_3;
                    }
                }
                //if the name is 5 characters or longer
                if (currentStat.StatName.Length > 4)
                {
                    if (currentStat.StatValue_2 != "0")
                    {

                        MainOLED_Finance_StatValue3_Textbox.Text = currentStat.StatValue_2;
                        if (currentStat.StatValue_2[0] == '-')
                        {
                            Long_Triangle_Down.Visibility = Visibility.Visible;
                            Long_Triangle_Up.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            Long_Triangle_Down.Visibility = Visibility.Hidden;
                            Long_Triangle_Up.Visibility = Visibility.Visible;
                        }
                    }
                }


                
            }
        }
        private void Refresh_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!DisplayModel.SearchForLastDisplay())
            {
                //No display has been found or one has been found but not previous saved one
                string[] FoundPorts = DisplayModel.FindAllPorts();

                if (FoundPorts[0] != "No Port")
                {                    
                    SqliteDataAccess.SetLastCOMPort(FoundPorts[0]);
                }
                else
                {
                    SqliteDataAccess.SetLastCOMPort("No Port");
                }
            }
            UpdateDisplayPage();
        }
        private void Display_Command_Combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            //if the Display Command Combo box is changed then save the changed state to the database
            ComboBoxPair DisplayCommandComboboxSelection = (ComboBoxPair)Display_Command_Combobox.SelectedItem;
            SqliteDataAccess.SetDisplayCommand(DisplayCommandComboboxSelection.ID);            

            //Resend data to OLED display            
            DisplayCommandModel CurrentDisplayCommand = new DisplayCommandModel();
            CurrentDisplayCommand = SqliteDataAccess.GetCurrentDisplayCommand();
            string CurrentPort = SqliteDataAccess.GetLastCOMPort();
            StatModel currentStat = new StatModel();
            currentStat = SqliteDataAccess.GetLastSavedStat();

            DisplayModel.SendToPort(currentStat.StatName, currentStat.StatValue_1, currentStat.StatValue_2, currentStat.StatValue_3, CurrentPort, CurrentDisplayCommand.Command);

            //Change the visual representation on screen 
            SetDisplayMockup(DisplayCommandComboboxSelection.ID, currentStat);
        }
        private void Display_Brightness_Combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Display_Brightness_Combobox.SelectedItem != null)
            {
                //if the Display brightness combo box is change then save the changed state to the database
                string BrightnessSetting = Display_Brightness_Combobox.SelectedItem.ToString();
                SqliteDataAccess.SetDisplayBrightness(BrightnessSetting);

                //Then send a command to the display 
                string CurrentPort = SqliteDataAccess.GetLastCOMPort();
                DisplayModel.SendToPort(BrightnessSetting, "0", "0", "0", CurrentPort, "SCRN");
            }            
        }
        private void Display_COMport_Combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (Display_COMport_Combobox.SelectedItem != null)
            //{
            //    //if the Display COM port combo box is change then save the changed state to the database
            //    string COMport = Display_COMport_Combobox.SelectedItem.ToString();
            //    SqliteDataAccess.SetLastCOMPort(COMport);

            //    //Then send a command to the display (has to search for latest display command and stat)
            //    DisplayCommandModel CurrentDisplayCommand = new DisplayCommandModel();
            //    CurrentDisplayCommand = SqliteDataAccess.GetCurrentDisplayCommand();

            //    StatModel currentStat = new StatModel();
            //    currentStat = SqliteDataAccess.GetLastSelectedStat();

            //    DisplayModel.SendToPort(currentStat.StatName, currentStat.StatValue_1, currentStat.StatValue_2, currentStat.StatValue_3, COMport, CurrentDisplayCommand.Command);
            //}
        }
    }
}
