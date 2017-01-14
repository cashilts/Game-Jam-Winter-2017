using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MessagingServer
{
    public partial class Form1 : Form
    {

        Dictionary<string, IPEndPoint> Chats = new Dictionary<string, IPEndPoint>();
        Dictionary<EndPoint, Socket> Connections = new Dictionary<EndPoint, Socket>();
        ManualResetEvent allDone = new ManualResetEvent(true);

        public class stateObject
        {
            public Socket workSocket = null;
            public const int bufferSize = 1024;
            public byte[] buffer = new byte[1024];
            public StringBuilder sb = new StringBuilder();
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            startListener();
        }

        void startListener() {
            byte[] bytes = new byte[1024];
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAdress = ipHostInfo.AddressList[0];
            label1.Text = ipAdress.ToString();
            IPEndPoint localEndPoint = new IPEndPoint(ipAdress, 11000);

            Socket listener = new Socket(ipAdress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            while (true)
            {
                try
                {
                    listener.Bind(localEndPoint);
                    listener.Listen(10);
                    while (true)
                    {
                        allDone.Reset();
                        listener.BeginAccept(new AsyncCallback(this.AcceptCallback), listener);
                        allDone.WaitOne();

                    }


                }
                catch { }
            }
        }


        void AcceptCallback(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            allDone.Set();

            stateObject state = new stateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, stateObject.bufferSize, 0, new AsyncCallback(ReadCallback), state);
        }


        void ReadCallback(IAsyncResult ar)
        {
            stateObject state = (stateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            int read = handler.EndReceive(ar);
      
            state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, read));
            string content = state.sb.ToString();
            if (content.Contains("<EOF>"))
            {
                content = content.Replace("<EOF>", "");
                if (content.StartsWith("Chats"))
                {
                    if (Chats.Count == 0)
                    {
                        state.sb.Clear();
                        content = "No chats available";

                    }
                    else
                    {
                        state.sb.Clear();
                        state.sb.Append("Chats");
                        List<string> chatNames = Chats.Keys.ToList<string>();
                        int chatNum = chatNames.Count;
                        for (int i = 0; i < chatNum; i++)
                        {
                            state.sb.Append(chatNames[i] + "\n");
                        }
                        content = state.sb.ToString();
                        state.sb.Clear();
                    }
                    byte[] bytes = new byte[1024];
                    bytes = Encoding.ASCII.GetBytes(content);
                    handler.Send(bytes);
                    handler.BeginReceive(state.buffer, 0, stateObject.bufferSize, 0, new AsyncCallback(ReadCallback), state);
                }
                else if (content.StartsWith("StartChat"))
                {
                    state.sb.Clear();
                    content = content.Remove(0, 9);
                    int endOfName = content.IndexOf('\n');
                    string name = content.Substring(0, endOfName);
                    content = content.Remove(0, endOfName + 1);
                    int portNum = Convert.ToInt32(content);
                    IPEndPoint otherEnd = handler.RemoteEndPoint as IPEndPoint;
                    otherEnd.Port = portNum;
                    Chats.Add(name, otherEnd);
                    byte[] bytes = new byte[1024];
                    bytes = Encoding.ASCII.GetBytes(name + " has been registered");
                    handler.Send(bytes);
                    handler.BeginReceive(state.buffer, 0, stateObject.bufferSize, 0, new AsyncCallback(ReadCallback), state);
                }
                else if (content.StartsWith("Connect")) {
                    state.sb.Clear();
                    content = content.Remove(0, 7);
                    IPEndPoint toSend;
                    Chats.TryGetValue(content, out toSend);
                    byte[] bytes = new byte[1024];
                    bytes = Encoding.ASCII.GetBytes("Endpoint" + toSend.ToString());
                    handler.Send(bytes);
                    handler.BeginReceive(state.buffer, 0, stateObject.bufferSize, 0, new AsyncCallback(ReadCallback), state);
                    Console.WriteLine(toSend.ToString());
                }
            }
        }
    }
}
