using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using WinForms.Windows;

namespace WinForms.Server
{
    public class ClientObject
    {
        protected internal int Id { get; private set; }
        private NetworkStream Stream { get; set; }
        private string userName;
        private readonly TcpClient client;
        private readonly ServerObject server; // объект сервера

        private bool BadConnectionFlag = false;
        private string message2;
        private Thread check;

        private readonly Form_Server form;

        public bool IsStarted { get; set; } = false;

        public ClientObject(TcpClient tcpClient, ServerObject serverObject, Form_Server form)
        {
            var j = 0;
            foreach (var t in serverObject.clients)
            {
                if (t.Id > j)
                    continue;
                if (t.Id == j)
                    j++;
            }
            Id = j;
            client = tcpClient;
            server = serverObject;
            // Добавить в список клиентов себя
            serverObject.AddConnection(this);

            this.form = form;
        }

        public void Process()
        {
            try
            {
                form.Status = "Кто-то подключился";
                Stream = client.GetStream();

                // получаем имя пользователя
                var message = GetMessage();
                userName = message;

                form.Names(Id, message);

                while (true) {
                    try
                    {
                        check = new Thread(new ParameterizedThreadStart(GetMessageY));
                        check.Start();
                        while (!IsStarted)
                        {
                            if (BadConnectionFlag)
                            {
                                Close();
                                throw new Exception();
                            }
                            Thread.Sleep(100);    
                        }
                        IsStarted = false;
                        BadConnectionFlag = false;
                    }
                    catch
                    {
                        throw new Exception($"Ошибка: Подключение у клиента {userName} разорвано.");
                    }
                    
                    message = "Start";
                    var data = Encoding.UTF8.GetBytes(message);
                
                    Stream.Write(data, 0, data.Length); //Работай!

                    form.Pbars(Id, 0);
                    var value = 0;
                    while (true)
                    {
                    
                        Thread.Sleep(1000);

                        value += 10;
                        if (value >= 90)
                        {
                            value = 90;
                            try
                            {
                                //message = GetMessage();
                                if (message2 == "Complete")
                                    break;
                                if (BadConnectionFlag)
                                {
                                    Close();
                                    form.Pbars(Id, 0);
                                    throw new Exception($"Ошибка: Подключение у клиента {userName} разорвано.");
                                }
                            }
                            catch (Exception ex)
                            {
                                form.Status = ex.Message;
                                throw new Exception(ex.Message);
                            }
                        }
                        form.Pbars(Id, value);
                    }
                    form.Pbars(Id, 100);
                    Thread.Sleep(1000);
                    form.Pbars(Id, 0);
                }
            }
            catch (Exception ex)
            {
                form.Status = ex.Message;
                form.Names(Id, "Not Connected");
                //throw new Exception(ex.Message);
            }
            finally
            {
                // в случае выхода из цикла закрываем ресурсы
                server.RemoveConnection(this.Id);
                Close();
            }
        }

        // чтение входящего сообщения и преобразование в строку
        private string GetMessage()
        {
            var data = new byte[client.ReceiveBufferSize];
            var bytes = Stream.Read(data, 0, client.ReceiveBufferSize);
            if(bytes > 0)
            {
                // Строка, содержащая ответ от сервера
                var message = Encoding.UTF8.GetString(data, 0, bytes);

                return message;
            }
            else
            {
                Close();
                return null;
            }
            
        }

        // чтение входящего сообщения и преобразование в строку
        private void GetMessageY(object mess)
        {
            try 
            { 
                var data = new byte[client.ReceiveBufferSize];
                var bytes = Stream.Read(data, 0, client.ReceiveBufferSize);
                if (bytes > 0)
                {
                    // Строка, содержащая ответ от сервера
                    mess = Encoding.UTF8.GetString(data, 0, bytes);
                    message2 = (string)mess;
                }
                else
                {
                    BadConnectionFlag = true;
                }
            }
            catch
            {
                BadConnectionFlag = true;
            }
        }

        // закрытие подключения
        protected internal void Close()
        {
            Stream?.Close();
            client?.Close();
        }
    }
}