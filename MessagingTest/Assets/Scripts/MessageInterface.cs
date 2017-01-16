using UnityEngine;
using System;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;

public class MessageInterface : MonoBehaviour {
    NetworkClient client;
    bool hosting;
    bool inChat;
    Socket serverConnection;
    List<Action<string>> queue = new List<Action<string>>();
    List<string> queueParam = new List<string>();
	// Use this for initialization
	void Start () {
        client = new NetworkClient(messageRecieved);
        client.startConnection();
	}


    void messageRecieved(string message, int connection) {
        //Connection 0 is always server
        if (connection == 0) {
            if (message == "Connected")
            {
                client.sendMessage("Chats", connection);
                return;
            }
            else if (message.StartsWith("Endpoint"))
            {
                message = message.Remove(0, 8);
                int endOfAddress = message.IndexOf(']');
                string ipString = message.Substring(0, endOfAddress + 1);
                string portString = message.Remove(0, endOfAddress + 2);
                IPAddress ip;
                IPAddress.TryParse(ipString, out ip);
                int port;
                int.TryParse(portString, out port);
                IPEndPoint otherUser = new IPEndPoint(ip, port);
                client.startConnection(otherUser);
                return;
            }
            
        }
        queue.Add(handleMessage);
        queueParam.Add(message);
    }

    void handleMessage(string message) {
        Dropdown temp = GameObject.Find("ChatSelect").GetComponent<Dropdown>();
        if (message == "No Chats Available")
        {
            temp.interactable = false;
            temp.options.Clear();
        }
        if (message.StartsWith("Chats"))
        {
            temp.interactable = true;
            temp.options.Clear();
            message = message.Remove(0, 5);
            while (message != "")
            {
                string chatName;
                int endOfName = message.IndexOf('\n');
                chatName = message.Substring(0, endOfName);
                message = message.Remove(0, endOfName + 1);
                temp.options.Add(new Dropdown.OptionData(chatName));
            }
        }
        if (inChat) {
            GameObject.Find("ResponseText").GetComponent<Text>().text += message;
        }
        GameObject.Find("ResponseText").GetComponent<Text>().text = message;

    }



	
	// Update is called once per frame
	void Update () {
        if (queue.Count > 0) {
            for (int i = 0; i < queue.Count; i++) {
                queue[i](queueParam[i]);
            }
        }
	}

    public void SubmitServerRequest() {
        if (inChat)
        {
            client.sendMessage(GameObject.Find("SendField").transform.FindChild("Text").GetComponent<Text>().text, 1);
        }
        else
        {
            int portNum = NetworkClient.getFreePort();
            string message = "StartChat" + GameObject.Find("NameField").transform.FindChild("Text").GetComponent<Text>().text + "\n" + portNum.ToString();
            client.sendMessage(message, 0);
            client.startHosting(portNum);
            GameObject.Find("SendButton").transform.FindChild("ButtonText").GetComponent<Text>().text = "Send";
            inChat = true;
        }
    }

    public void SubmitChatRequest() {
        string name = GameObject.Find("ChatSelect").transform.FindChild("Label").GetComponent<Text>().text;
        string message = "Connect" + name;
        client.sendMessage(message, 0);
        GameObject.Find("SendButton").transform.FindChild("ButtonText").GetComponent<Text>().text = "Send";
    }
}
 