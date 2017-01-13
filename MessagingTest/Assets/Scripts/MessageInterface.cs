using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using System.Text;
using System.Net.Sockets;

public class MessageInterface : MonoBehaviour {
    public bool responseUpdated = false;
    public string response;


    Socket serverConnection;

	// Use this for initialization
	void Start () {
        NetworkClient.connectToHost(new AsyncCallback(connected));
	}




    void connected(IAsyncResult ar) {
        string content = "Chats<EOF>";
        byte[] bytes = new byte[1024];
        bytes = Encoding.ASCII.GetBytes(content);
        Socket handler = (Socket)ar.AsyncState;
        serverConnection = handler;
        handler.Send(bytes);
        NetworkClient.stateObject state = new NetworkClient.stateObject();
        state.workSocket = handler;
        NetworkClient.GetResponse(state, new AsyncCallback(getCurrentChats));
        handler.BeginReceive(state.buffer, 0, NetworkClient.stateObject.bufferSize, 0, new AsyncCallback(getCurrentChats), state);
    }


    void getCurrentChats(IAsyncResult ar) {
        NetworkClient.ParseResponse(ar, this);
    }
	
	// Update is called once per frame
	void Update () {
        if (responseUpdated){
            GameObject.Find("ResponseText").GetComponent<Text>().text = response;
            responseUpdated = false;
        }
       
	}

    public void SubmitServerRequest() {
        byte[] bytes = new byte[1024];
        bytes = Encoding.ASCII.GetBytes("StartChat" + GameObject.Find("NameField").transform.FindChild("Text").GetComponent<Text>().text + "<EOF>");
        serverConnection.Send(bytes);
        NetworkClient.stateObject state = new NetworkClient.stateObject();
        state.workSocket = serverConnection;
        NetworkClient.GetResponse(state, new AsyncCallback(getCurrentChats));
        serverConnection.BeginReceive(state.buffer, 0, NetworkClient.stateObject.bufferSize, 0, new AsyncCallback(getCurrentChats), state);
    }
}
