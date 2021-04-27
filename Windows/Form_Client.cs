using System;
using System.Windows.Forms;
using WinForms.Client;
using System.Drawing;

namespace WinForms.Windows
{
    public partial class Form_Client : Form
    {
        public ClientObject client;
        private readonly string ip;

        public Form_Client(string ip)
        {
            InitializeComponent();
            this.ip = ip;
        }

        private delegate void Delegate(object value);

        public int progress
        {
            get => pbar.Value;
            set => pbar?.Invoke(new Delegate((s) => pbar.Value = (int)value), "newText");
        }

        public string ErrorText
        {
            get => lblError.Text;
            set => lblError?.Invoke(new Delegate((s) => lblError.Text = (string)value), "newText");
        }
        public Color ErrorColor
        {
            set => lblError?.Invoke(new Delegate((s) => lblError.ForeColor = value), "newText");
        }

        public bool cmbBoxEnabled
        {
            set => cmbbxName?.Invoke(new Delegate((s) => cmbbxName.Enabled = value), "newText");
        }

        public string Status
        {
            get => lblStatusUI.Text;
            set
            {
                lblStatusUI?.Invoke(new Delegate((s) => lblStatusUI.Text = (string)value), "newText");
                lblStatusUI?.Invoke(
                    lblStatusUI.Text == "Подключено"
                        ? new Delegate((s) => lblStatusUI.ForeColor = Color.Green)
                        : new Delegate((s) => lblStatusUI.ForeColor = Color.Red), "newText");
            }
        }

        public string UserName { get => cmbbxName.Text; set => cmbbxName.Text = value; }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            // Проверка непустого имени
            if (string.IsNullOrEmpty(cmbbxName.Text))
            {
                this.ErrorText = "Ошибка: Не выбрано имя.";
                this.ErrorColor = Color.Red;
                return;
            }
            // Первое подключение
            if (client == null)
            {
                client = new ClientObject(this, ip);
                client.Connect();
            }

            switch (client?.client.Connected)
            {
                // Блокировка повторного подключения
                case true: return;
                // Подключение при разрыве
                case false:
                    client = new ClientObject(this, ip);
                    client.Connect();
                    break;
                default: client.Connect();
                    break;
            }
        }

        private void Form_Client_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (client != null)
            {
                client.receiveThread.Abort();
                client.Disconnect();
                client.form = null;
            }
            var formEnter = (Form_Enter)Application.OpenForms["Form_Enter"];
            formEnter?.Show();
        }

       
    }
}
