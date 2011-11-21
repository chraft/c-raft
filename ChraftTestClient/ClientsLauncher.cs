#region C#raft License
// This file is part of C#raft. Copyright C#raft Team 
// 
// C#raft is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
#endregion
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
using System.Threading;
using Chraft.Net;

namespace ChraftTestClient
{
    public partial class ClientsLauncher : Form
    {
        private TestClient[] _testClients;
        private IPEndPoint _ip;
        private int _port;

        private System.Threading.Timer _clientStartTimer;

        public ClientsLauncher()
        {
            InitializeComponent();
            PacketMap.Initialize();
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
            Random randGen = new Random();
            for (int i = 0; i < numClients; ++i)
                _testClients[i] = new TestClient("TC" + i, randGen);

            _numClients = numClients;

            _clientStartTimer = new System.Threading.Timer(StartClients,null, 0, 20000);
        }

        private int _lastIndex;
        private int _numClients;
        private void StartClients(object state)
        {
            if (_lastIndex == _numClients)
            {
                _clientStartTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _clientStartTimer.Dispose();
                _clientStartTimer = null;
                _lastIndex = 0;
                return;
            }
            try
            {
                int end = _lastIndex + 20;
                int i;
                for (i = _lastIndex; i < end && i < _numClients; ++i)
                    _testClients[i].Start(_ip);

                _lastIndex = i;
            }
            catch (Exception e)
            {
                using (StreamWriter sw = new StreamWriter("clientlauncher_error.log", true))
                {
                    sw.WriteLine(DateTime.Now + " - " + e);
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
            if(_clientStartTimer != null)
            {
                _clientStartTimer.Dispose();
                _clientStartTimer = null;
            }
            if (_testClients != null)
            {
                for (int i = 0; i < _testClients.Length; ++i)
                {
                    _testClients[i].Dispose();
                }
                _lastIndex = 0;
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
