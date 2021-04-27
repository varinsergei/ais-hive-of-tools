using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using WinForms.Server;

//TODO: Сделать возможность отключения станка из диспетчера
//TODO: Сделать динамическое создание форм станков

namespace WinForms.Windows
{
    public partial class Form_Server : Form
    {
        public ServerObject server; // сервер
        private Thread listenThread; // потока для прослушивания

        public readonly List<Label> names = new List<Label>();
        public readonly List<ProgressBar> pbars = new List<ProgressBar>();

        public Form_Server()
        {
            InitializeComponent(); 
            names.Add(lblType1);
            names.Add(lblType2);
            names.Add(lblType3);
            names.Add(lblType4);
            pbars.Add(progressBar1);
            pbars.Add(progressBar2);
            pbars.Add(progressBar3);
            pbars.Add(progressBar4);
        }

        private delegate void Delegate(string text);

        public string Status
        {
            get => lblStatusLog.Text;
            set => lblStatusLog?.Invoke(new Delegate((s) => lblStatusLog.Text = value), "newText");
        }

        public void Names(int id, string name)
        {
            names[id]?.Invoke(new Delegate((s) => names[id].Text = name), "newText");
        }

        public void Pbars(int id, int value)
        {
            pbars[id]?.Invoke(new Delegate((s) => pbars[id].Value = value), "newText");
        }

        private void btnServStart_Click(object sender, EventArgs e)
        {
            if (server != null || listenThread != null) return;
            try
            {
                server = new ServerObject(this);
                listenThread = new Thread(new ThreadStart(server.Listen));
                listenThread.Start(); //старт потока
            }
            catch (Exception ex)
            {
                server?.Disconnect();
                Status = ex.Message;
                throw new Exception(ex.Message);
            }
        }

        private void btnServerStop_Click(object sender, EventArgs e)
        {
            lblStatusLog.Hide();
            if (server != null) { 
                server.Disconnect();
                listenThread.Abort();
                listenThread = null;
                server = null;
            }
            Thread.Sleep(40);
            Status = "Остановлен.";
            lblStatusLog.Show();
        }

        private void btnStart1_Click(object sender, EventArgs e)
        {
            if (server == null || server.clients.Count < 1) return;
            foreach (var client in server.clients)
                if (client.Id == 0)
                    client.IsStarted = true;
        }

        private void btnStart2_Click(object sender, EventArgs e)
        {
            if (server == null || server.clients.Count < 2) return;
            foreach (var client in server.clients)
                if (client.Id == 1)
                    client.IsStarted = true;
        }

        private void btnStart3_Click(object sender, EventArgs e)
        {
            if (server == null || server.clients.Count < 3) return;
            foreach (var client in server.clients)
                if (client.Id == 2)
                    client.IsStarted = true;
        }

        private void btnStart4_Click(object sender, EventArgs e)
        {
            if (server == null || server.clients.Count < 4) return;
            foreach (var client in server.clients)
                if (client.Id == 3)
                    client.IsStarted = true;
        }

        private void Form_Server_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (server != null)
            {
                server.Disconnect();
                listenThread.Abort();
                listenThread = null;
                server.form = null;
            }
            var formEnter = (Form_Enter)Application.OpenForms["Form_Enter"];
            formEnter?.Show();
        }

    }
}
