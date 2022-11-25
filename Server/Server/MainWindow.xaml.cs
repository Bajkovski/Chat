using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Threading;
using System.IO;

namespace Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    enum Command
    {
        Login,      //Log into the server
        Logout,     //Logout of the server
        Message,    //Send a text message to all the chat clients
        List,       //Get a list of users in the chat room from the server
        Accept,
        Decline,
        Register,
        Private,
        Null        //No command
    }

    public partial class MainWindow : Window
    {
        struct ClientInfo
        {
            public Socket socket;   //Socket of the client
            public string strName;  //Name by which the user logged into the chat room
            public string password;
        }

        ArrayList clientList;

        Socket serverSocket;

        byte[] byteData = new byte[1024];


        public MainWindow()
        {
            clientList = new ArrayList();
            InitializeComponent();            

        }

        private delegate void UpdateDelegate(string pMessage);

        private void UpdateMessage(string pMessage)
        {
            this.textBox1.Text += pMessage;
        }      

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //We are using TCP sockets
                //Control.CheckForIllegalCrossThreadCalls = false;
                serverSocket = new Socket(AddressFamily.InterNetwork,
                                          SocketType.Stream,
                                          ProtocolType.Tcp);

                //Assign the any IP of the machine and listen on port number 1000
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 1000);

                //Bind and listen on the given address
                serverSocket.Bind(ipEndPoint);
                serverSocket.Listen(4);

                //Accept the incoming clients
                serverSocket.BeginAccept(new AsyncCallback(OnAccept), null);
                //serverSocket.Accept();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SGSserverTCP");
            }   
        }
        private void OnAccept(IAsyncResult ar)
        {
            try
            {
                Socket clientSocket = serverSocket.EndAccept(ar);

                //Start listening for more clients
                serverSocket.BeginAccept(new AsyncCallback(OnAccept), null);

                //Once the client connects then start receiving the commands from her
                clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None,
                    new AsyncCallback(OnReceive), clientSocket);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SGSserverTCPONACCEPT");
            }
        }

        private void OnReceive(IAsyncResult ar)
        {
            byte[] message;
            Data msgToSend = new Data();
            Data msgReceived = new Data(byteData);
            
            try
            {

                Socket clientSocket = (Socket)ar.AsyncState;
                clientSocket.EndReceive(ar);

              
                if (Command.Register != msgReceived.cmdCommand)
                {
                    msgToSend.cmdCommand = msgReceived.cmdCommand;
                    Console.WriteLine(msgReceived.strMessage);
                    msgToSend.strName = msgReceived.strName;
                    msgToSend.strNameTo = null;
                    Console.WriteLine(msgReceived.cmdCommand);
                }



                switch (msgReceived.cmdCommand)

                {
                    case Command.Private:
                        msgToSend.cmdCommand = Command.Private;
                        msgToSend.strName = msgReceived.strName;
                        msgToSend.strNameTo = msgReceived.strNameTo;
                        //msgToSend.strMessage = msgReceived.strMessage;
                        int sent = 0;
                        int online = 0;

                        msgToSend.strMessage = "Private message from " + msgReceived.strName + ":\n" + msgReceived.strMessage;
                        message = msgToSend.ToByte();
                        foreach (ClientInfo clientInf in clientList)
                        {
                            Console.WriteLine(clientInf.strName);
                            if (!clientInf.strName.Equals(msgReceived.strName))
                            {
                                if (clientInf.strName.Equals(msgReceived.strNameTo))
                                {
                                    online = 1;
                                    if (!(msgReceived.strMessage == null)) // 1 space in the message
                                    {
                                        if (msgReceived.strMessage.Equals("No attached text, unsented!"))
                                        {
                                            sent = 2;
                                            break;
                                        }
                                        clientInf.socket.Send(message, 0, message.Length, SocketFlags.None);
                                        sent = 1;
                                        break;
                                    }
                                    else
                                    {
                                        sent = 3;
                                    }

                                }

                            }
                        }
                        Console.WriteLine("msgRecieved.strMessage: " + msgReceived.strMessage);
                        Console.WriteLine("sent: " + sent);
                        if (sent == 2 || sent == 3)
                        {
                            msgToSend.strMessage = "Private message to " + msgReceived.strNameTo + ":\n" + "No attached text, unsented!";

                        }
                        if (online == 0)
                        {
                            msgToSend.strMessage = "" + msgReceived.strNameTo + " is not online, you can't send private message!";
                        }

                        if (sent == 1)
                        {
                            msgToSend.strMessage = "Private message to " + msgReceived.strNameTo + ":\n" + msgReceived.strMessage;
                        }
                        message = msgToSend.ToByte();
                        clientSocket.BeginSend(message, 0, message.Length, SocketFlags.None, new AsyncCallback(OnSend), clientSocket);
                        break;

                    case Command.Register:

                        msgToSend.cmdCommand = Command.Register;

                        msgToSend.strNameTo = null;
                        int exist = 0;
                        string filePath = @"C:\Users\Bajkovszkij\Documents\Távközlőhál\Piszkált\Server\Server\login.txt";

                        List<ClientInfo> users = new List<ClientInfo>();
                        List<string> line = File.ReadAllLines(filePath).ToList();
                        if (line.Count != 0)
                        {
                            foreach (var item in line)
                            {
                                string[] tmp = item.Split(';');
                                ClientInfo tmpClientInfo = new ClientInfo();
                                tmpClientInfo.strName = tmp[0];
                                tmpClientInfo.password = tmp[1];
                                users.Add(tmpClientInfo);
                            }
                        }

                        //Console.WriteLine(msgReceived.strName);
                        //if ((msgReceived.strName.Contains(' '))|| (msgReceived.strName.Equals("")))  //új rész
                        //{
                            
                        //        msgToSend.strMessage = "The format of the usename is incorrect!";
                           

                        //}

                        //else
                        //{
                            foreach (var item in users)
                            {
                                if (item.strName.Equals(msgReceived.strName))
                                {
                                    msgToSend.strMessage = "This username is unavailable, please choose another!";
                                    exist = 1;
                                    break;
                                }
                            }

                            if (exist == 0)
                            {
                                ClientInfo tmpClientInfo = new ClientInfo();
                                tmpClientInfo.strName = msgReceived.strName;
                                tmpClientInfo.password = msgReceived.strMessage;
                                users.Add(tmpClientInfo);
                                msgToSend.strMessage = "The user has registered, login!";
                            }

                            List<string> output = new List<string>();
                            foreach (var item in users)
                            {
                                output.Add(item.strName + ";" + item.password);
                            }

                            File.WriteAllLines(filePath, output);
                        //}
                        message = msgToSend.ToByte();

                        clientSocket.Send(message);
                        clientSocket.Close();
                        break;




                    case Command.Login:

                        //When a user logs in to the server then we add her to our
                        //list of clients
                        string filePath1 = @"C:\Users\Bajkovszkij\Documents\Távközlőhál\Piszkált\Server\Server\login.txt";
                        List<ClientInfo> users1 = new List<ClientInfo>();
                        List<string> line1 = File.ReadAllLines(filePath1).ToList();
                        int login = 0;
                        foreach (var item in line1)
                        {
                            string[] tmp = item.Split(';');
                            ClientInfo tmpClientInfo = new ClientInfo();
                            tmpClientInfo.strName = tmp[0];
                            tmpClientInfo.password = tmp[1];
                            users1.Add(tmpClientInfo);
                        }
                        foreach (var item in users1)
                        {
                            if (item.strName.Equals(msgReceived.strName))
                            {                               
                                if (item.password.Equals(msgReceived.strMessage))
                                {
                                    login = 1;
                                    foreach (ClientInfo item2 in clientList)
                                    {
                                        if (item2.strName.Equals(msgReceived.strName))
                                        {
                                            login = 2;
                                            break;
                                        }
                                    }
                                    if (login == 2 || login==0)
                                        break;
                                    msgToSend.cmdCommand = Command.Accept;
                                    
                                    
                                    ClientInfo clientInfo = new ClientInfo();
                                    clientInfo.socket = clientSocket;
                                    
                                    clientInfo.strName = msgReceived.strName;

                                    clientList.Add(clientInfo);

                                    //Set the text of the message that we will broadcast to all users
                                    msgToSend.strMessage = "<<<" + msgReceived.strName + " has joined the room>>>";
                                    break;
                                }
                            }
                        }
                        if (login == 0 || login == 2)
                        {
                            msgToSend.cmdCommand = Command.Decline;
                            if(login==0)
                                msgToSend.strMessage = "Incorrect login!";
                            if (login == 2)
                                msgToSend.strMessage = "This user has already logged in!";
                            message = msgToSend.ToByte();
                            clientSocket.Send(message);
                            clientSocket.Close();
                        }
                        else
                        {                            
                            message = msgToSend.ToByte();
                            clientSocket.Send(message);
                        }
                        
                        break;

                    case Command.Logout:

                        //msgToSend.cmdCommand = Command.Decline;
                        //When a user wants to log out of the server then we search for her 
                        //in the list of clients and close the corresponding connection

                        int nIndex = 0;
                        foreach (ClientInfo client in clientList)
                        {
                            if (client.socket == clientSocket)
                            {
                                clientList.RemoveAt(nIndex);
                                break;
                            }
                            ++nIndex;
                        }

                        clientSocket.Close();

                        msgToSend.strMessage = "<<<" + msgReceived.strName + " has left the room>>>";
                        break;

                    case Command.Message:

                        //Set the text of the message that we will broadcast to all users
                        msgToSend.strMessage = msgReceived.strName + ": " + msgReceived.strMessage;
                        break;

                    case Command.List:

                        //Send the names of all users in the chat room to the new user
                        msgToSend.cmdCommand = Command.List;
                        msgToSend.strName = null;
                        msgToSend.strNameTo = null;
                        msgToSend.strMessage = "Online users: ";

                        //Collect the names of the user in the chat room
                        foreach (ClientInfo client in clientList)
                        {
                            //To keep things simple we use asterisk as the marker to separate the user names
                            msgToSend.strMessage += client.strName + "*";
                        }

                        message = msgToSend.ToByte();

                        //Send the name of the users in the chat room
                        clientSocket.BeginSend(message, 0, message.Length, SocketFlags.None,
                                new AsyncCallback(OnSend), clientSocket);
                        break;
                       
                }
                if(msgToSend.cmdCommand != Command.Decline)
                {
                    if (msgToSend.cmdCommand != Command.Register)
                    {
                        if (msgToSend.cmdCommand != Command.Private)
                        {
                            if (msgToSend.cmdCommand != Command.List)   //List messages are not broadcasted
                            {
                                message = msgToSend.ToByte();

                                foreach (ClientInfo clientInfo in clientList)
                                {
                                    if (clientInfo.socket != clientSocket ||
                                msgToSend.cmdCommand != Command.Accept)
                                        //Send the message to all users
                                        //clientInfo.socket.BeginSend(message, 0, message.Length, SocketFlags.None,
                                        //new AsyncCallback(OnSend), clientInfo.socket);
                                        clientInfo.socket.Send(message, 0, message.Length, SocketFlags.None);
                                }
                                //textBox1.Text += msgToSend.strMessage;
                                {
                                    UpdateDelegate update = new UpdateDelegate(UpdateMessage);
                                    this.textBox1.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, update,
                                    msgToSend.strMessage + "\r\n");
                                }
                            }
                        }
                    }

                    //If the user is logging out then we need not listen from her
                    if (msgReceived.cmdCommand != Command.Logout)
                    {
                        if (msgReceived.cmdCommand != Command.Register)
                        {
                            //Start listening to the message send by the user
                            clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), clientSocket);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                
                
                int valid = 0;
                Socket clientSocket = (Socket)ar.AsyncState;
                int nIndex = 0;
                string name = "";
                if (clientList.Contains(clientSocket))
                    valid=1;
                foreach (ClientInfo client in clientList)
                {                    
                        if (client.socket == clientSocket)
                        {
                            Console.WriteLine(clientSocket.ToString());
                            Console.WriteLine(client.socket);
                        name = client.strName;
                            clientList.RemoveAt(nIndex);
                            valid = 1;
                        break;
                           
                        }
                        ++nIndex;
                }

                clientSocket.Close();
                if (valid == 1)
                {
                    msgToSend.strMessage = "<<<" + name + " has lost connection>>>";
                    message = msgToSend.ToByte();

                    foreach (ClientInfo clientInfo in clientList)
                    {
                        if (clientInfo.socket != clientSocket)
                            //Send the message to all users
                            //clientInfo.socket.BeginSend(message, 0, message.Length, SocketFlags.None,
                            //new AsyncCallback(OnSend), clientInfo.socket);
                            clientInfo.socket.Send(message, 0, message.Length, SocketFlags.None);
                    }
                    UpdateDelegate update = new UpdateDelegate(UpdateMessage);
                    this.textBox1.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, update,
                    msgToSend.strMessage + "\r\n");
                }
                
            }
        }

        public void OnSend(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndSend(ar);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SGSserverTCPONRecieve");
            }
        }
    }

    class Data
    {
        //Default constructor
        public Data()
        {
            this.cmdCommand = Command.Null;
            this.strMessage = null;
            this.strName = null;
            this.strNameTo = null;
        }

        //Converts the bytes into an object of type Data
        public Data(byte[] data)
        {
            
            //The first four bytes are for the Command
            this.cmdCommand = (Command)BitConverter.ToInt32(data, 0);

            //The next four store the length of the name
            
            int nameLen = BitConverter.ToInt32(data, 4);
            

            //The next four store the length of the message
            int msgLen = BitConverter.ToInt32(data, 8);

            int nameLenTo = BitConverter.ToInt32(data, 12);

            //This check makes sure that strName has been passed in the array of bytes
            if (nameLen > 0)
                this.strName = Encoding.UTF8.GetString(data, 16, nameLen);
            else
                this.strName = null;

            //This checks for a null message field
            if (msgLen > 0)
                this.strMessage = Encoding.UTF8.GetString(data, 16 + nameLen, msgLen);
            else
                this.strMessage = null;

            if (nameLenTo > 0)
                this.strNameTo = Encoding.UTF8.GetString(data, 16 + nameLen + msgLen, nameLenTo);
            else
                this.strNameTo = null;
        }

        //Converts the Data structure into an array of bytes
        public byte[] ToByte()
        {
            List<byte> result = new List<byte>();

            //First four are for the Command
            result.AddRange(BitConverter.GetBytes((int)cmdCommand));

            //Add the length of the name
            int db = 0;
            int db1 = 0;
            int db2 = 0;
            if (strName != null)
            {
                for (int i = 0; i < strName.Length; i++)
                {
                    char b = strName.ToUpper()[i];
                    if (b == 'Á' || b == 'É' || b == 'Í' || b == 'Ű' || b == 'Ó' || b == 'Ü' || b == 'Ú' || b == 'Ü' || b == 'Ö' || b == 'Ő')
                        db++;
                }
                
                result.AddRange(BitConverter.GetBytes(strName.Length+db));
            }
            else
                result.AddRange(BitConverter.GetBytes(0));

            //Length of the message
            if (strMessage != null)
            {
                for (int i = 0; i < strMessage.Length; i++)
                {
                    char b = strMessage.ToUpper()[i];
                    if (b == 'Á' || b == 'É' || b == 'Í' || b == 'Ű' || b == 'Ó' || b == 'Ü' || b == 'Ú' || b == 'Ü' || b == 'Ö' || b == 'Ő')
                        db1++;
                }
                Console.WriteLine(  db1);
                result.AddRange(BitConverter.GetBytes(strMessage.Length+db1));
            }
            else
                result.AddRange(BitConverter.GetBytes(0));

            if (strNameTo != null)
            {
                for (int i = 0; i < strNameTo.Length; i++)
                {
                    char b = strNameTo.ToUpper()[i];
                    if (b == 'Á' || b == 'É' || b == 'Í' || b == 'Ű' || b == 'Ó' || b == 'Ü' || b == 'Ú' || b == 'Ü' || b == 'Ö' || b == 'Ő')
                        db2++;
                }
                
                result.AddRange(BitConverter.GetBytes(strNameTo.Length + db2));
            }
            else
                result.AddRange(BitConverter.GetBytes(0));

            //Add the name
            if (strName != null)
                result.AddRange(Encoding.UTF8.GetBytes(strName));

            //And, lastly we add the message text to our array of bytes
            if (strMessage != null)
                result.AddRange(Encoding.UTF8.GetBytes(strMessage));

            if (strNameTo != null)
                result.AddRange(Encoding.UTF8.GetBytes(strNameTo));

            return result.ToArray();
        }

        public string strNameTo;
        public string strName;      //Name by which the client logs into the room
        public string strMessage;   //Message text
        public Command cmdCommand;  //Command type (login, logout, send message, etcetera)
    } 
}
