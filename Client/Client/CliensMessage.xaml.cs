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
    /// Interaction logic for CliensMessage.xaml
    /// </summary>
    
    public partial class CliensMessage : Window
    {
        public Socket ClientSocket;
        public string LoginName;
        byte[] byteData = new byte[1024];
        public delegate string IsButtonOn();

        private delegate void UpdateDelegate(string pMessage);

       
        private void TextBox2_TextChanged(object sender, TextChangedEventArgs e)
        {
             
            button1.IsEnabled = true;
        }
        
        private void UpdateMessage(string pMessage)
        {
            this.textBox1.Text += pMessage;
        }

        public CliensMessage()
        {
            InitializeComponent();
        }

        public CliensMessage(Socket pSocket, String pName)
        {
            InitializeComponent();

            ClientSocket = pSocket;
            LoginName = pName;
            this.Title = pName;
            Data msgToSend = new Data();
            msgToSend.cmdCommand = Command.List;

            msgToSend.strName = null;
            msgToSend.strMessage = null;
            msgToSend.strNameTo = null;
            byte[] b = msgToSend.ToByte();
           
            ClientSocket.Send(b);

            ClientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None,
                    new AsyncCallback(OnReceive), ClientSocket);

            //ClientSocket.Receive(byteData,SocketFlags.None);

        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                Socket clientSocket = (Socket)ar.AsyncState;
                clientSocket.EndReceive(ar);
                //Transform the array of bytes received from the user into an
                //intelligent form of object Data
                Data msgReceived = new Data(byteData);

                ClientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), ClientSocket);
                UpdateDelegate update = new UpdateDelegate(UpdateMessage);
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, update, msgReceived.strMessage + "\r\n");
            }
            catch(Exception ex)
            { MessageBox.Show(ex.Message, "Szerver kapcsolat megszakadt"); }
           
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {            
            Data msgToSend = new Data();
            msgToSend.cmdCommand = Command.Message;
            msgToSend.strName = LoginName;               
            msgToSend.strNameTo = null;
            if (textBox2.Text.StartsWith("@"))
            {
                msgToSend.cmdCommand = Command.Private;
                string[] tmp = textBox2.Text.Split(' ');
                msgToSend.strNameTo = tmp[0].Trim('@');
                Console.WriteLine("tmp.length= "+tmp.Length);
                if (tmp.Length > 1)
                {
                    msgToSend.strMessage = textBox2.Text.Remove(0, tmp[0].Length + 1);
                    Console.WriteLine(msgToSend.strMessage);
                }
                else if(tmp.Length==1)
                {                    
                    msgToSend.strMessage = "No attached text, unsented!";
                }

            }
            else
            { 
                msgToSend.strMessage = textBox2.Text; 
            }
           
            byte[] b = msgToSend.ToByte();
            ClientSocket.Send(b);
            textBox2.Text = "";
            button1.IsEnabled = false;
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Data msgToSend = new Data();
            msgToSend.cmdCommand = Command.Logout;
            msgToSend.strMessage = null;
            msgToSend.strName = LoginName;
            msgToSend.strNameTo = null;
            byte[] b = msgToSend.ToByte();
            ClientSocket.Send(b);
            Close();
        }

      


    }
}
