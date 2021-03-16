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
            Main_OLEDDisplay_Textbox.Text = "Oops!";
            Not_Connected_Dialogue.Visibility = Visibility.Visible;
        }



        public void UpdateDisplayPage()
        {
            HideElements();

            if (DisplayModel.SearchForLastDisplay())
            {
                //if the display is connected take globally available current stat and change main text
                Main_OLEDDisplay_Textbox.Text = MainPage.GlobalCurrentStat;

                //Make COM port box visible
                Display_COMport_Combobox.Visibility = Visibility.Visible;
                UpdateComPortsCombo();

                //Make DisplayCommands box visible
                Display_Command_Combobox.Visibility = Visibility.Visible;
                Display_Command_Combobox.ItemsSource = PopulateDisplayCommandCombo();
                SetDisplayCommandComboboxIndex(SqliteDataAccess.GetCurrentDisplayCommandID());

                //Make Disconnected dialogue hidden
                Not_Connected_Dialogue.Visibility = Visibility.Hidden;
            }            
        }

        public void UpdateComPortsCombo()
        {
            //Iterate through the ports and add to the combo box
            foreach (var Port in DisplayModel.FindAllPorts())
            {
                if (Port != "No Port")
                {
                    Display_COMport_Combobox.Items.Add(Port);
                    int Selected = 0;
                    int count = Display_COMport_Combobox.Items.Count;
                    for (int i = 0; (i <= (count - 1)); i++)
                    {
                        Display_COMport_Combobox.SelectedIndex = i;
                        if ((string)(Display_COMport_Combobox.SelectedValue) == SqliteDataAccess.GetLastCOMPort())
                        {
                            Selected = i;
                            Display_COMport_Combobox.SelectedIndex = Selected;
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


            //Change the visual representation on screen 

            //Resend data to OLED display            
            DisplayCommandModel CurrentDisplayCommand = new DisplayCommandModel();
            CurrentDisplayCommand = SqliteDataAccess.GetCurrentDisplayCommand();
            string CurrentPort = SqliteDataAccess.GetLastCOMPort();
            StatModel currentStat = new StatModel();
            currentStat = SqliteDataAccess.GetLastSelectedStat();

            DisplayModel.SendToPort(currentStat.StatName, currentStat.StatValue_1, currentStat.StatValue_2, currentStat.StatValue_3, CurrentPort, CurrentDisplayCommand.Command);
        }
    }
}
