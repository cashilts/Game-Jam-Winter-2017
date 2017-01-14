using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using System.Text;
using System.Net.Sockets;

public class MessageInterface : MonoBehaviour {
    public bool responseUpdated = false;
    public string response;

    bool hosting;
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


    public void getCurrentChats(IAsyncResult ar) {
        NetworkClient.ParseResponse(ar, this);
    }
	
	// Update is called once per frame
	void Update () {
        if (responseUpdated){
            Dropdown temp = GameObject.Find("ChatSelect").GetComponent<Dropdown>();
            if (response == "No Chats Available")
            {
                temp.interactable = false;
                temp.options.Clear();
                temp.options.Add(new Dropdown.OptionData(response));
            }
            else {
                if (response.StartsWith("Chats"))
                {
                    temp.interactable = true;
                    temp.options.Clear();
                    response = response.Remove(0, 5);
                    while (response != "")
                    {
                        string chatName;
                        int endOfName = response.IndexOf('\n');
                        chatName = response.Substring(0, endOfName);
                        response = response.Remove(0, endOfName + 1);
                        temp.options.Add(new Dropdown.OptionData(chatName));
                    }
                }
                else if (response.StartsWith("Endpoint")) {
                    response = response.Remove(0, 8);

                }
            }
            responseUpdated = false;
        }
       
	}

    public void SubmitServerRequest() {
        int portNum = NetworkClient.getFreePort();
        byte[] bytes = new byte[1024];
        bytes = Encoding.ASCII.GetBytes("StartChat" + GameObject.Find("NameField").transform.FindChild("Text").GetComponent<Text>().text + "\n" + portNum.ToString() + "<EOF>");
        serverConnection.Send(bytes);
        NetworkClient.stateObject state = new NetworkClient.stateObject();
        state.workSocket = serverConnection;
        NetworkClient.GetResponse(state, new AsyncCallback(getCurrentChats));
        serverConnection.BeginReceive(state.buffer, 0, NetworkClient.stateObject.bufferSize, 0, new AsyncCallback(getCurrentChats), state);
        NetworkClient.startListener(portNum);
    }

    public void SubmitChatRequest() {
        string name = GameObject.Find("ChatSelect").transform.FindChild("Label").GetComponent<Text>().text;
        byte[] bytes = new byte[1024];
        bytes = Encoding.ASCII.GetBytes("Connect" + name + "<EOF>");
        serverConnection.Send(bytes);
    }
}
 