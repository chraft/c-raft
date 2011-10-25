using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChraftTestClient
{
    public partial class ClientsLauncher : Form
    {
        private TestClient[] _testClients;
        private IPEndPoint _ip;
        private int _port;

        public ClientsLauncher()
        {
            InitializeComponent();
        }

        private void launchButton_Click(object sender, EventArgs e)
        {
            IPAddress address;
            Resolve(ipText.Text, out address);

            if (address == IPAddress.None)
            {
                MessageBox.Show("Cannot resolve the address");
                return;
            }

            if (!int.TryParse(portText.Text, out _port))
            {
                MessageBox.Show("Wrong port");
                return;
            }

            _ip = new IPEndPoint(address, _port);

            int numClients;

            if (!int.TryParse(clientsNumText.Text, out numClients))
            {
                MessageBox.Show("Wrong clients number");
                return;
            }          

            _testClients = new TestClient[numClients];
            Task.Factory.StartNew(() => StartAndCreateClients(numClients));
        }

        public static bool Resolve(string addr, out IPAddress outValue)
        {
            try
            {
                outValue = IPAddress.Parse(addr);
                return true;
            }
            catch
            {
                try
                {
                    IPHostEntry iphe = Dns.GetHostEntry(addr);

                    if (iphe.AddressList.Length > 0)
                    {
                        if(string.IsNullOrEmpty(addr))
                            outValue = iphe.AddressList[iphe.AddressList.Length - 2];
                        else
                            outValue = iphe.AddressList[iphe.AddressList.Length - 1];
                        return true;
                    }
                }
                catch
                {
                }
            }

            outValue = IPAddress.None;
            return false;
        }

        private void StartAndCreateClients(int numClients)
        {
            for (int i = 0; i < numClients; ++i)
                _testClients[i] = new TestClient("TestClient" + i);

            try
            {
                for (int i = 0; i < numClients; ++i)
                    _testClients[i].Start(_ip);
            }
            catch (Exception e)
            {               
                using(StreamWriter sw = new StreamWriter("clientlauncher_error.log", true))
                {
                    sw.WriteLine(DateTime.Now + " - " + e.Message);
                }
            }
            
        }

        private void ClientsLauncher_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_testClients != null)
            {
                for (int i = 0; i < _testClients.Length; ++i)
                {
                    _testClients[i].Dispose();
                }

                _testClients = null;
            }
        }

        private void disposeButton_Click(object sender, EventArgs e)
        {
            if (_testClients != null)
            {
                for (int i = 0; i < _testClients.Length; ++i)
                {
                    _testClients[i].Dispose();
                }

                _testClients = null;
            }
        }

        private void SendMessage_Click(object sender, EventArgs e)
        {
            if (_testClients != null)
            {
                foreach (TestClient test in _testClients)
                {
                    test.SendMessage(ChatBox.Text);
                }
            }
        }
    }
}
