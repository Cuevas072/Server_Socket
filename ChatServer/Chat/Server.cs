using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using Chat;
using Serialization;
using System.Threading;
using System.Text;


namespace ChatServer.Chat
{
    class Server
    {
        Socket socket;
        Thread listenThread;
        Hashtable usersTable;        

        public Server()
        {
            try
            {
                Console.WriteLine("Server Iniciado");
                IPHostEntry host = Dns.GetHostEntry("localhost");
                IPAddress addr = host.AddressList[0];
                IPEndPoint endPoint = new IPEndPoint(addr, 4404);

                socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(endPoint);
                socket.Listen(10);
                
                listenThread = new Thread(this.Listen);
                listenThread.Start();
                usersTable = new Hashtable(); //Almacenar los usuarios
                
            }
            catch (Exception e)
            {
                Console.WriteLine("{0}", e.Message);
            }
            
        }

        /// <summary>
        /// Listo para aceptar la conexion de algun cliente
        /// </summary>
        private void Listen()
        {
            Socket cliente;
            while (true)
            {
                cliente = this.socket.Accept();
                //listenThread = new Thread(this.ListenClient);
                //listenThread.Start(cliente);
                ThreadPool.QueueUserWorkItem(ListenClient,cliente);
            }
            
        }

        /// <summary>
        /// Escucha al Cliente
        /// </summary>
        /// <param name="o">Socket client</param>
        private void ListenClient(object o)
        {
            Socket client = (Socket)o;
            object received;
            User x = null;
            do
            {                
                received = this.Receive(client);
            
            } while (!(received is User));

            this.usersTable.Add(received, client);
            this.BroadCast(received);
            this.SendAllUsers(client);
            x = (User)received;
            Console.WriteLine("Se conecto a: " + x.nick);

            while (true)
            {
                received = this.Receive(client);
                if (received is Message) 
                {
                    this.SendMessage((Message)received);
                }
            }
        }

        /// <summary>
        /// Envia un objeto a todos los usuarios conectados
        /// </summary>
        /// <param name="o">Objeto a Enviar </param>
        private void BroadCast(object o)
        {
            foreach (DictionaryEntry d in this.usersTable) 
            {
                this.Send((Socket)d.Value, o);            
            }
            
        }

        /// <summary>
        /// Actualizar la tabla de usuarios por cada usuario que se conecte 
        /// </summary>
        /// <param name="s">Socket client</param>
        private void SendAllUsers(Socket s)
        {
            foreach (DictionaryEntry d in this.usersTable)
            {
                this.Send(s, d.Key);
            }
        }

        /// <summary>
        /// Envia un mensaje a un destino
        /// </summary>
        /// <param name="m">Message to send</param>
        private void SendMessage(Message m)
        {
            User usuarioAenviar;
            foreach (DictionaryEntry d in this.usersTable)
            {
                usuarioAenviar = (User)d.Key;

                if (usuarioAenviar.id == m.to.id)
                {

                    try{
                        this.Send((Socket)d.Value, m);
                    }
                    catch {
                        Message error = new Message(m.to, m.from, "El usuario se murio alv noestema");
                        m = error;
                        this.Send((Socket)d.Value, m);
                    }
                    Console.WriteLine("Remitente: " + m.from.nick);
                    Console.WriteLine("Destinatario: " + m.to.nick);
                    Console.WriteLine("El mensaje: " + m.msg);
                    Console.WriteLine();
                    break;
                }
               
            }
        }

        /// <summary>
        /// Envia un objeto a un cliente
        /// </summary>
        /// <param name="s">Socket client</param>
        /// <param name="o">Object to send</param>
        private void Send(Socket s, object o)
        {
            byte[] buffer = new byte[1024];
            byte[] obj = BinarySerialization.Serializate(o);
            Array.Copy(obj, buffer, obj.Length);
            s.Send(buffer);
        }

        /// <summary>
        /// Receive all the serialized object
        /// </summary>
        /// <param name="s">Socket that receive the object</param>
        /// <returns>Object received from client</returns>
        /// 
        private object Receive(Socket s)
        {
            byte[] buffer = new byte[1024];
            //try
            //{
                s.Receive(buffer);
                return BinarySerialization.Deserializate(buffer);
            //}
            //catch (SocketException e)
            //{
            //    Console.WriteLine("Usuario Desconectado");                
            //    return BinarySerialization.Deserializate(Encoding.BigEndianUnicode.GetBytes("Me desconecte. El servidor arrojo el mensaje: \n"+e.ToString()));

            //}
            
        }

        
    }
}
