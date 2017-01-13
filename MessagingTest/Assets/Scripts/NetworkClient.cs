using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using UnityEngine;
using UnityEngine.UI;

public static class NetworkClient {

    public static void connectToHost(AsyncCallback callFunction) {
        IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAdress = ipHostInfo.AddressList[0];
        IPEndPoint remoteEP = new IPEndPoint(ipAdress, 11000);
        Socket sender = new Socket(ipAdress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        sender.BeginConnect(remoteEP, callFunction, sender);
    }

    public static void GetResponse(stateObject state, AsyncCallback callback) {
        state.workSocket.BeginReceive(state.buffer, 0,stateObject.bufferSize,0, callback, state);
    }

    public static void ParseResponse(IAsyncResult ar, MessageInterface callback)
    {
        stateObject state = (stateObject)ar.AsyncState;

        Socket handler = state.workSocket;
        int read = handler.EndReceive(ar);
        state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, read));
        string content = state.sb.ToString();
        string text = "";

        if (content.Contains("<EOF>"))
        {
            content = content.Replace("<EOF>", "");
            if (content == "")
            {
                text += "No hosts available";
            }
            else
            {
                for (int i = 0; i < content.Length;)
                {
                    int hostNameLen = content.IndexOf('-', i);
                    string subString = content.Substring(i, hostNameLen - i);
                    text += subString + '\n';
                    i += hostNameLen + 1;
                }
            }
        }
        else
        {
        }
        callback.response = content;
        callback.responseUpdated = true;
        state.sb.Length = 0;
        state.sb.Capacity = 0;
    }

    public class stateObject
    {
        public Socket workSocket = null;
        public const int bufferSize = 1024;
        public byte[] buffer = new byte[1024];
        public StringBuilder sb = new StringBuilder();
    }


    


    static void getConnection(IAsyncResult ar) {
        Socket listener = (Socket)ar.AsyncState;
        Socket connection = listener.EndAccept(ar);
        //Begin multiplayer connection
    }

    public static void startHost(string name) {
        IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAdress = ipHostInfo.AddressList[0];
        IPEndPoint remoteEP = new IPEndPoint(ipAdress, 11000);
        Socket sender = new Socket(ipAdress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        sender.Connect(remoteEP);
        string request = "Host" + name + "<EOF>";
        byte[] bytes = new byte[1024];
        bytes = Encoding.ASCII.GetBytes(request);
        sender.Send(bytes);
        sender.Receive(bytes);
        string response = Encoding.ASCII.GetString(bytes);
        if (response == "Valid")
        {
            sender.BeginAccept(new AsyncCallback(getConnection), sender);
        }
        else {
            //Get another host name.
        }
    }
}
