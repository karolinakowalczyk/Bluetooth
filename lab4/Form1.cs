using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using Brecham.Obex;
using System.Threading;

namespace lab4
{
    public partial class Form1 : Form
    {
        BluetoothDeviceInfo[] _devices;
        BluetoothDeviceInfo chosenDevice;

        public Form1()
        {
            InitializeComponent();
        }

        //Wyszukwanie urządzenia
        public void searchDevices()
        {
            BluetoothClient client = new BluetoothClient();
            _devices = client.DiscoverDevices();

            foreach (BluetoothDeviceInfo device in _devices)
            {
                listBox1.Items.Add(device.DeviceName + "(" + device.DeviceAddress + ")" + "\u000D\u000A");
            }
            

        }

        //Parowanie
        private void pairDevice()
        {
            try
            {
                chosenDevice = _devices[listBox1.SelectedIndex];
                BluetoothSecurity.PairRequest(chosenDevice.DeviceAddress, null);
                button3.Enabled = true;
                button4.Enabled = true;
            }
            catch
            {
                MessageBox.Show("Nie można połączyć się z " + chosenDevice.DeviceName.ToString());
            }

        }

        private void fileOpener()
        {
            bool exist = false;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.ShowDialog();

            foreach (String fileName in openFileDialog1.FileNames)
            {
                if (fileName != null)
                {
                    foreach (string item in listBox3.Items)
                        if (item == fileName)
                            exist = true;
                }

                if (!exist)
                {
                    listBox3.Items.Add(fileName);
                }
                else
                {
                    MessageBox.Show("Wybrany plik jest już na liście.");
                }
            }
        }

        private void SendFiles(object o)
        {
            foreach (string fileName in listBox3.Items)
                {
                    try
                    {
                        
                        SynchronizationContext cntx = o as SynchronizationContext;
                        cntx.Send(UpdateStartSendingMess, fileName);
                        Uri uri = new Uri("obex://" + chosenDevice.DeviceAddress.ToString() + "/" + fileName);
                        ObexWebRequest request = new ObexWebRequest(uri);
                        request.ReadFile(fileName);
                        ObexWebResponse response = (ObexWebResponse)request.GetResponse();
                        cntx.Send(UpdateCorrectSendingMess, response.StatusCode);
                    }
                    catch (Exception error)
                    {
                    SynchronizationContext cntxerr = o as SynchronizationContext;
                    String mess = error.Message;
                    cntxerr.Send(UpdateErrorSendingMess, mess);
                }
                }
            
        }

        private void UpdateStartSendingMess(object fileName)
        {
            listBox2.Items.Add("Rozpoczęto wysyłanie.\r\n");
        }

        private void UpdateCorrectSendingMess(object response)
        {
            listBox2.Items.Add("Odpowiedz serwera:" + response + "\r\n");
        }

        private void UpdateErrorSendingMess(object mess)
        {
            listBox2.Items.Add("Wystapił błąd! " + mess + "\r\n");
        }


        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            searchDevices();
            button1.Enabled = true;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            pairDevice();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                String text = listBox1.SelectedItem.ToString();
                chosenDevice = _devices[listBox1.SelectedIndex];
                textBox2.Text = chosenDevice.DeviceName.ToString();
                textBox3.Text = chosenDevice.DeviceAddress.ToString();
                textBox4.Text = chosenDevice.Rssi.ToString();
                button2.Enabled = true;
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listBox3.Items.Count == 0)
            {
                listBox2.Items.Add("Brak plików!" + "\r\n");
            }
            else
            {
                Thread sendFilesthr = new Thread(SendFiles);
                sendFilesthr.Start(SynchronizationContext.Current);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            fileOpener();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            listBox3.Items.Clear();
        }
    }
}
