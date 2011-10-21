using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChraftTestClient
{
    public partial class ClientsLauncher : Form
    {
        private TestClient[] _testClients;
        public ClientsLauncher()
        {
            InitializeComponent();
        }

        private void launchButton_Click(object sender, EventArgs e)
        {
            int numClients;

            if (!int.TryParse(clientsNumText.Text, out numClients))
            {
                MessageBox.Show("Wrong clients number");
                return;
            }
            _testClients = new TestClient[numClients];
            Task.Factory.StartNew(() => StartAndCreateClients(numClients));
        }

        private void StartAndCreateClients(int numClients)
        {
            for (int i = 0; i < numClients; ++i)
                _testClients[i] = new TestClient("TestClient" + i);

            try
            {
                for (int i = 0; i < numClients; ++i)
                    _testClients[i].Start("127.0.0.1", "25565");
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
    }
}
