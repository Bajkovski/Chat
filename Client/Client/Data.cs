using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client
{

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

    class Data
    {
        //Default constructor
        public Data()
        {
            this.cmdCommand = Command.Null;
            this.strMessage = null;
            this.strName = null;
            this.strNameTo=null;
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
                this.strNameTo = Encoding.UTF8.GetString(data, 16 + nameLen+ msgLen,nameLenTo);
            else
                this.strNameTo = null;
        }

        //Converts the Data structure into an array of bytes
        public byte[] ToByte()
        {
            int db = 0;
            int db1 = 0;
            int db2 = 0;
            List<byte> result = new List<byte>();

            //First four are for the Command
            result.AddRange(BitConverter.GetBytes((int)cmdCommand));

            //Add the length of the name
            if (strName != null)
            {
                for (int i = 0; i < strName.Length; i++)
                {
                    char b = strName.ToUpper()[i];
                    if (b == 'Á' || b == 'É' || b == 'Í' || b == 'Ű' || b == 'Ó' || b == 'Ü' || b == 'Ú' || b == 'Ü' || b == 'Ö' || b == 'Ő')
                        db++;
                }

                result.AddRange(BitConverter.GetBytes(strName.Length + db));
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
                Console.WriteLine(db1);
                result.AddRange(BitConverter.GetBytes(strMessage.Length + db1));
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
                Console.WriteLine(db1);
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
