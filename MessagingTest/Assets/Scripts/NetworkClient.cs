using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using System.Collections.Generic;

public class NetworkClient {
    Action<string,int> callbackFunction;
    List<stateObject> connections = new List<stateObject>();


    public class stateObject
    {
        public Socket workSocket = null;
        public const int bufferSize = 1024;
        public byte[] buffer = new byte[1024];
        public StringBuilder sb = new StringBuilder();
    }

    //Network clients are created with a callback to a function for getting messages
    public NetworkClient(Action<string,int> callback){
        callbackFunction = callback;
    }


    /// <summary>
    /// Attempts a connection with the IPEndPoint provided
    /// </summary>
    /// <param name="connection">IPEndPoint to connect to</param>
    public void startConnection(IPEndPoint connection) {
        Socket sender = new Socket(connection.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        sender.BeginConnect(connection, onConnect, sender);
    }

    public void startConnection() {
        IPAddress ip = IPAddress.Parse("[fe80::9f8:2795:d630:6e42%18]");
        int port = 11000;
        IPEndPoint defaultEndPoint = new IPEndPoint(ip, port);
        startConnection(defaultEndPoint);
    }

    /// <summary>
    /// Called when connection is accepted, registers the connection into a list and gives the connection number to the callback
    /// </summary>
    /// <param name="ar"></param>
    void onConnect(IAsyncResult ar) {
        stateObject newState = new stateObject();
        newState.workSocket = (Socket)ar.AsyncState;
        connections.Add(newState);
        int connectionNum = connections.IndexOf(newState);
        newState.workSocket.BeginReceive(newState.buffer, 0, stateObject.bufferSize, 0, onReceive, newState); ;
        callbackFunction("Connected", connectionNum);
    }

    /// <summary>
    /// Called when a message is recived from any connection, if the message is not completed it continues to recieve the message.  
    /// </summary>
    /// <param name="ar"></param>
    void onReceive(IAsyncResult ar) {
        stateObject state = (stateObject)ar.AsyncState;

        Socket handler = state.workSocket;
        int read = handler.EndReceive(ar);
        state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, read));
        string content = state.sb.ToString();

        if (content.Contains("<EOF>"))
        {
            state.sb.Length = 0;
            state.sb.Capacity = 0;
            content = content.Replace("<EOF>", "");
            callbackFunction(content, connections.IndexOf(state));
        }
        state.workSocket.BeginReceive(state.buffer, 0, stateObject.bufferSize, 0, onReceive, state);
    }


    /// <summary>
    /// Sends a message to the connection num
    /// </summary>
    /// <param name="message">String message to send</param>
    /// <param name="connection">Connection number to send message to </param>
    public void sendMessage(string message, int connection) {
        message += "<EOF>";
        byte[] byteMessage = new byte[1024];
        byteMessage = Encoding.ASCII.GetBytes(message);
 
        stateObject currentConnection = connections[connection];
        try {
            currentConnection.workSocket.Send(byteMessage);
        }
        catch {
            callbackFunction("Error connection does not exist", connection);
        }
    }

    public void startHosting(int port) {
        byte[] bytes = new byte[1024];
        IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAdress = ipHostInfo.AddressList[0];
        IPEndPoint localEndPoint = new IPEndPoint(ipAdress, port);

        Socket listener = new Socket(ipAdress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(localEndPoint);
        listener.Listen(10);
        listener.BeginAccept(new AsyncCallback(onAccept), listener);
    }


    void onAccept(IAsyncResult ar) {
        Socket listener = (Socket)ar.AsyncState;
        Socket handler = listener.EndAccept(ar);
        allDone.Set();

        stateObject state = new stateObject();
        state.workSocket = handler;
        connections.Add(state);
        int index = connections.IndexOf(state);
        handler.BeginReceive(state.buffer, 0, stateObject.bufferSize, 0, onReceive, state);
        callbackFunction("Accepted", index);
    }



    static ManualResetEvent allDone = new ManualResetEvent(true);
    static MessageInterface callbackClass;


 

    public static int getFreePort() {
        TcpListener l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        int port = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        return port;

    }
 

}
