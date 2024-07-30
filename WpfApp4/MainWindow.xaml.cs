using MahApps.Metro.Controls;
using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace WpfApp4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static Regex pattern = new Regex(@"[^\x20-\x7e]");

        private static byte[] HeaderByte = new byte[] { 0xF0, 0x43, 0x10, 0x7F, 0x1C, 0x00 };

        private enum Mode
        {
            Performance,
            Voice,
            Master
        }

        private Mode _mode; 






        public MainWindow()
        {
            InitializeComponent();

            initDevices();

            _mode = Mode.Performance; 
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(pattern.IsMatch((e.Source as TextBox).Text)) {
                error_label.Content = "It can only contain alphanumeric characters";
            }

            else
            {
                error_label.Content = ""; 
            }
        }

        private void GetNameBtn_Click(object sender, RoutedEventArgs e)
        {
            var performanceName = textBox.Text;

            if (pattern.IsMatch(performanceName))
            {
                error_label.Content = "It can only contain alphanumeric characters";

                return;
            }

            // performance name is valid

            var performaneNameCharArray = performanceName.ToCharArray();

            var bufferList = new List<byte[]>();

            int i = 0;

            foreach (char character in performaneNameCharArray)
            {
                bufferList.Add(HeaderByte);
                bufferList.Add(new byte[] { 0x30, 0x00, Convert.ToByte(i.ToString("x"), 16), Convert.ToByte(character) });

                bufferList.Add(new byte[] { 0xF7 });

                i++;


            }
            byte modeByte = modeToModeByte();

            for (int j = i; j < 20; j++)
            {
                bufferList.Add(HeaderByte);
                bufferList.Add(new byte[] { modeByte, 0x00, Convert.ToByte(j.ToString("x"), 16), 0x20 });

                bufferList.Add(new byte[] { 0xF7 });
            }

            try
            {
                using (var midiOut = new MidiOut(comboBoxMidiOutDevices.SelectedIndex))
                {
                    foreach (var listItem in bufferList)
                    {
                        midiOut.SendBuffer(listItem);
                    }
                }

            }
            catch (Exception x)
            {

                MessageBox.Show(x.Message);
            }



        }

        private byte modeToModeByte()
        {
            byte modeByte;
            switch (_mode)
            {
                case Mode.Performance:
                    modeByte = 0x30;
                    break;
                case Mode.Voice:
                    modeByte = 0x40;
                    break;
                case Mode.Master:
                    modeByte = 0x50;
                    break;
                default:
                    modeByte = 0x30;
                    break;
            }

            return modeByte;
        }

        public void initDevices()
        {
            for (int device = 0; device < MidiIn.NumberOfDevices; device++)
            {
                comboBoxMidiInDevices.Items.Add(MidiIn.DeviceInfo(device).ProductName);
            }
            if (comboBoxMidiInDevices.Items.Count > 0)
            {
                comboBoxMidiInDevices.SelectedIndex = 0;
            }

            int selectedComboBoxItem = -1; 
            for (int device = 0; device < MidiOut.NumberOfDevices; device++)
            {
                comboBoxMidiOutDevices.Items.Add(MidiOut.DeviceInfo(device).ProductName);

                if (MidiOut.DeviceInfo(device).ProductName.StartsWith("Yamaha"))
                {
                    selectedComboBoxItem = device; 
                }
            }

            if (comboBoxMidiOutDevices.Items.Count > 0)
            {
                comboBoxMidiOutDevices.SelectedIndex = 0;
            }

            if(selectedComboBoxItem != -1)
            {
                comboBoxMidiOutDevices.SelectedIndex = selectedComboBoxItem; 
            }



        }

        private void perfRBtn_Checked(object sender, RoutedEventArgs e)
        {
            _mode = Mode.Performance; 
        }

        private void voxRBtn_Checked(object sender, RoutedEventArgs e)
        {
            _mode = Mode.Voice; 
        }

        private void masterRBtn_Checked(object sender, RoutedEventArgs e)
        {
            _mode = Mode.Master; 
        }
    }
}
