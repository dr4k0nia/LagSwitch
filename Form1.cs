using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace LagSwitch
{
    public partial class Form1 : Form
    {
        [DllImport("User32.dll")]
        private static extern bool GetAsyncKeyState(ushort vKey);

        private bool active = false;
        private string gamepath;
        private ushort hotkey;
        private int duration;


        public Form1()
        {
            InitializeComponent();
            comboBox1.DataSource = Enum.GetValues(typeof(VirtualKeys)); //Add the virtual keys to the the combobox
        }


        private void button1_Click(object sender, EventArgs e)
        {
            //We use OpenFileDialog() to find the path of our game executable, the textbox is set to ReadOnly
            using (OpenFileDialog selectExecutable = new OpenFileDialog())
            {
                //Use this to filter acceptable input file types, you can find out how to do that yourself ;)
                //selectExecutable.Filter = ""; 

                if (selectExecutable.ShowDialog() == DialogResult.OK)
                {
                    gamepath = textBox1.Text = selectExecutable.FileName;
                }

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Resolve hotkey string from our combobox to a ushort of the VirtualKeys enum
            hotkey = (ushort)Enum.Parse(typeof(VirtualKeys), comboBox1.SelectedItem.ToString());

            //Convert string from our combobox into int (Int32)
            duration = Convert.ToInt32(comboBox2.SelectedItem);

            //Check if our gamepath is longer than 0, you could additionally improve this by with File.Exists() to check if the filepath is valid
            if (gamepath.Length == 0)
            {
                MessageBox.Show("No game executable path was set", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            //Toggle our bool
            active = !active;

            //Change button text according to state
            if (active)
            {
                button2.Text = "stop";
                timer1.Start();
            }
            else
            {
                button2.Text = "start";
                timer1.Stop();
            }
        }


        //Timer to check for hotkey press
        private void timer1_Tick(object sender, EventArgs e)
        {
            //Check if our hotkey was pressed, you could improve this by using RegisterHotKey() and use events instead of a timer
            if (GetAsyncKeyState(hotkey))
            {
                //This is the main path of our Lagswitch we are adding firewall rules to block traffic of our game executable
                //This can obviously be improved a lot, just take a look at the arguments ;)
                ProcessStartInfo AddRuleIn = new ProcessStartInfo("cmd.exe", "/c netsh advfirewall firewall add rule name=\"UCLagSwitch\" dir=in action=block program=\"" + gamepath + "\" enable=yes");
                ProcessStartInfo AddRuleOut = new ProcessStartInfo("cmd.exe", "/c netsh advfirewall firewall add rule name=\"UCLagSwitch\" dir=out action=block program=\"" + gamepath + "\" enable=yes");
                ProcessStartInfo DeleteRule = new ProcessStartInfo("cmd.exe", "/c netsh advfirewall firewall delete rule name=\"UCLagSwitch\" program=\"" + gamepath + "\"");

                //Use WindowStyle Hidden to hide the command line windows
                AddRuleIn.WindowStyle = ProcessWindowStyle.Hidden;
                AddRuleOut.WindowStyle = ProcessWindowStyle.Hidden;
                DeleteRule.WindowStyle = ProcessWindowStyle.Hidden;


                //Add the firewall rules
                Process.Start(AddRuleIn);
                Process.Start(AddRuleOut);
                Console.Beep();
                //Sleep for the set duration
                Thread.Sleep(duration);
                //Delete the firewall rule
                Process.Start(DeleteRule);
            }
        }
    }
}
