using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using WinForms.Windows;

namespace WinForms.Server
{
    public class ServerObject
    {
        private static TcpListener tcpListener; // сервер для прослушивания

        public readonly List<ClientObject> clients = new List<ClientObject>(); // все подключения

        public Form_Server form;
        private Thread clientThread;

        public ServerObject(Form_Server form)
        {
            this.form = form;
        }

        protected internal void AddConnection(ClientObject clientObject)
        {
            clients.Add(clientObject);
        }

        protected internal void RemoveConnection(int id)
        {
            // получаем по id закрытое подключение
            var client = clients.FirstOrDefault(c => c.Id == id);
            // и удаляем его из списка подключений
            if (client != null)
                clients.Remove(client);
        }

        private delegate void Del(string text);

        // прослушивание входящих подключений
        protected internal void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 8005);
                tcpListener.Start();
                form.Status = "Сервер запущен. Ожидание подключений...";

                while (true)
                {
                    var tcpClient = tcpListener.AcceptTcpClient();

                    var clientObject = new ClientObject(tcpClient, this, form);
                    clientThread = new Thread(new ThreadStart(clientObject.Process));
                    //clientThread.IsBackground = true;
                    clientThread.Start();
                }
            }
            catch (SocketException ex) when (ex.ErrorCode == 10004)
            {
                //clientThread.Abort();
                return;
            }
            catch (Exception)
            {
                Disconnect();
            }
        }

        // отключение всех клиентов
        protected internal void Disconnect()
        {
            tcpListener.Stop(); //остановка сервера

            foreach (var t in clients)
            {
                t.Close(); //отключение клиента
            }
        }
    }
}
