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
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    

    public partial class MainWindow : Window
    {

        public Socket clientSocket;
        public string strName;
        byte[] byteData = new byte[1024];
       

        public delegate string getNameDelegate();
        public delegate string getNameDelegate1();
        public delegate string getNameDelegate2();
        public delegate void UjFormDelegate();
        public string message;

        public MainWindow()
        {
            InitializeComponent();
        }

        public string getLoginName()
        {
            return this.textBox1.Text;
        }

        public string getIP()
        {
            return this.textBox2.Text;
        }

        public string getPassword()
        {
            return this.textBox3.Password;
        }

        public string addMessage()
        {
            return this.textBox4.Text= message;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            
                try
                {
               
                    string l_ip;
                    clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    //IPAddress ipAddress = IPAddress.Parse(this.textBox2.Text);

                    //getNameDelegate IP = new getNameDelegate(getIP);
                    //l_ip = (string)this.Dispatcher.Invoke(IP, null);
                    IPAddress ipAddress = IPAddress.Parse(this.textBox2.Text);
                    //Server is listening on port 1000
                    IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 1000);

                    //Connect to the server
                    //clientSocket.Connect(ipEndPoint);
                    clientSocket.BeginConnect(ipEndPoint, new AsyncCallback(OnConnect), null);
                
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "SGSclient");
                }
            
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {

            try
            {
               
                    clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPAddress ipAddress = IPAddress.Parse(this.textBox2.Text);
                    IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 1000);
                    clientSocket.BeginConnect(ipEndPoint, new AsyncCallback(OnRegister), null);
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SGSclient");
            }
            
        }

        private void OnRegister(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndConnect(ar);

                //We are connected so we login into the server
                string l_fhName;
                string l_fhPass;
                string l_fhMsg;
                int ok = 0;

                Data msgToSend = new Data();
                msgToSend.cmdCommand = Command.Register;

                //l_fhName = this.textBox1.Text;
                getNameDelegate1 fhName = new getNameDelegate1(getLoginName);
                getNameDelegate1 fhPass = new getNameDelegate1(getPassword);
                
                l_fhName = (string)this.textBox1.Dispatcher.Invoke(fhName, null);
                l_fhPass = (string)this.textBox3.Dispatcher.Invoke(fhPass, null);

                if (l_fhName.Contains(' '))
                {
                    message = "The format of the usename is incorrect!";
                    getNameDelegate fhMsg = new getNameDelegate(addMessage);
                    l_fhMsg = (string)this.textBox4.Dispatcher.Invoke(fhMsg, null);
                    ok = 1;

                }

                else if(l_fhName.Equals(""))
                {
                    message = "You must enter the username!";
                    getNameDelegate fhMsg = new getNameDelegate(addMessage);
                    l_fhMsg = (string)this.textBox4.Dispatcher.Invoke(fhMsg, null);
                    ok = 1;

                }

                else if (l_fhPass.Equals(""))
                {
                    message = "You must enter the password!";
                    getNameDelegate fhMsg = new getNameDelegate(addMessage);
                    l_fhMsg = (string)this.textBox4.Dispatcher.Invoke(fhMsg, null);
                    ok = 1;

                }

                if (ok == 0)
                {
                    msgToSend.strName = l_fhName;
                    msgToSend.strMessage = l_fhPass;
                    msgToSend.strNameTo = null;

                    byte[] b = msgToSend.ToByte();

                    //Send the message to the server
                    clientSocket.BeginSend(b, 0, b.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SGSclientRegister");
            }

        }
        private void OnRecieve(IAsyncResult ar)
       {
            clientSocket = (Socket)ar.AsyncState;
            clientSocket.EndReceive(ar);
        }

            private void OnSend(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndSend(ar);
                byte[] byteData = new byte[1024];
                                
                clientSocket.Receive(byteData, 0, 1024, SocketFlags.None);
                Data msg = new Data(byteData);

                if (msg.cmdCommand == Command.Register)
                {
                    Console.WriteLine(msg.strMessage);
                    string l_fhMsg;
                    message = msg.strMessage;
                    getNameDelegate2 fhMsg = new getNameDelegate2(addMessage);
                    l_fhMsg = (string)this.textBox4.Dispatcher.Invoke(fhMsg, null);
                    Data msgToSend = new Data();
                    msgToSend.cmdCommand = Command.Logout;
                    msgToSend.strMessage = null;
                    msgToSend.strName = null;
                    msgToSend.strNameTo = null;
                    byte[] b = msgToSend.ToByte();
                    clientSocket.Send(b);
                }
                else if (msg.cmdCommand == Command.Decline)
                {
                    Console.WriteLine("decline");
                    string l_fhMsg;
                    message = msg.strMessage;
                    getNameDelegate2 fhMsg = new getNameDelegate2(addMessage);
                    l_fhMsg = (string)this.textBox4.Dispatcher.Invoke(fhMsg, null);
                    //clientSocket.Close();
                }
                else
                {

                    UjFormDelegate pForm = new UjFormDelegate(UjForm);
                    this.Dispatcher.Invoke(pForm, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SGSclient");
            }


        }

        private void UjForm()
        {
            CliensMessage uj_form;
            uj_form = new CliensMessage(clientSocket,textBox1.Text);
            uj_form.Show();
            Close();
        }

        private void OnConnect(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndConnect(ar);

                //We are connected so we login into the server
                string l_fhName;
                string l_fhPass;
                string l_fhMsg;
                int ok = 0;


                Data msgToSend = new Data();
                msgToSend.cmdCommand = Command.Login;

                //l_fhName = this.textBox1.Text;
                getNameDelegate fhName = new getNameDelegate(getLoginName);
                getNameDelegate fhPass = new getNameDelegate(getPassword);

                l_fhName = (string)this.textBox1.Dispatcher.Invoke(fhName, null);
                l_fhPass = (string)this.textBox3.Dispatcher.Invoke(fhPass, null);

                if (l_fhName.Contains(' '))
                {
                    message = "The format of the usename is incorrect!";
                    getNameDelegate fhMsg = new getNameDelegate(addMessage);
                    l_fhMsg = (string)this.textBox4.Dispatcher.Invoke(fhMsg, null);
                    ok = 1;

                }
                else if (l_fhName.Equals(""))
                {
                    message = "You must enter the username!";
                    getNameDelegate fhMsg = new getNameDelegate(addMessage);
                    l_fhMsg = (string)this.textBox4.Dispatcher.Invoke(fhMsg, null);
                    ok = 1;

                }

                else if (l_fhPass.Equals(""))
                {
                    message = "You must enter the password!";
                    getNameDelegate fhMsg = new getNameDelegate(addMessage);
                    l_fhMsg = (string)this.textBox4.Dispatcher.Invoke(fhMsg, null);
                    ok = 1;

                }

                Console.WriteLine(ok);
                if (ok == 0)
                {
                    msgToSend.strName = l_fhName;
                    msgToSend.strMessage = l_fhPass;
                    msgToSend.strNameTo = null;

                    byte[] b = msgToSend.ToByte();
                    //Send the message to the server
                    clientSocket.BeginSend(b, 0, b.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SGSclientOnconnect");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
