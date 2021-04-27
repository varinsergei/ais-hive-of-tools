using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using WinForms.Windows;
using System.Drawing;

namespace WinForms.Client
{
    public class ClientObject
    {
        public bool IsConnect { get; private set; } = false;
        private string userName { get; set; }
        // Порт
        private const int port = 8005;
        public int Port
        {
            get => port;
            set => throw new NotImplementedException();
        }

        // Адресс
        private readonly string address;
        public string Address
        {
            get => address;
            set => throw new NotImplementedException();
        }

        // Поток сообщений и TCP Client
        public TcpClient client { get; private set; }
        private NetworkStream stream;

        public Thread receiveThread;

        // Форма
        public Form_Client form { get; set; }

        public ClientObject(Form_Client form, string ip)
        {
            address = ip;
            client = new TcpClient();
            this.form = form;
            userName = form.UserName; 
        }

        public void Connect()
        {
            try
            {
                form.cmbBoxEnabled = false;
                var t = client.Connected;
                client.Connect(address, port); //подключение клиента
                stream = client.GetStream(); // получаем поток

                form.Status = "Connected";

                var message = userName;
                var data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);

                // запускаем новый поток для получения данных
                receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                receiveThread.Start(); //старт потока

                form.ErrorText = "OK";
                form.ErrorColor = Color.Green;
            }
            catch (Exception ex)
            {
                form.ErrorText = ex.Message;
                form.ErrorColor = Color.Red;
                form.cmbBoxEnabled = true;
                //throw new Exception(ex.Message);
            }
        }

        // отправка сообщений
        private void SendMessage()
        {
            var message = "Complete";
            var data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }
        // получение сообщений
        private void ReceiveMessage()
        {
            while (true)
            {
                string message = null;
                try
                {
                    if (!client.Connected)
                        throw new Exception();
                    var data = new byte[client.ReceiveBufferSize];
                    var bytes = stream.Read(data, 0, client.ReceiveBufferSize);
                    if (bytes > 0)
                    {
                        // Строка, содержащая ответ от сервера
                        message = Encoding.UTF8.GetString(data, 0, bytes);
                    }
                    else
                    {
                        Disconnect();
                    }

                    form.ErrorText = "OK!";
                    form.ErrorColor = Color.Green;
                }
                catch
                {
                    if (form != null)
                    {
                        form.ErrorText = "Ошибка: подключение прервано";
                        form.ErrorColor = Color.Red;
                        form.Status = "Disconnected";
                        form.cmbBoxEnabled = true;
                    }
                    Disconnect();
                    break;
                }
                if (message == "Start")
                {
                    Working();
                }
            }
        }

        public void Disconnect()
        {
            stream?.Close();//отключение потока
            client?.Close();//отключение клиента
            //receiveThread.Abort();
        }

        private void Working()
        {
            while (true)
            {
                if (form.progress >= 100)
                {
                    SendMessage();
                    break;
                }
                Thread.Sleep(1000);
                form.progress += 10;
            }
            Thread.Sleep(500);
            form.progress = 0;
        }
    }
}
