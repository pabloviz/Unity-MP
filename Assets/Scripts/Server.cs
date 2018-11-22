﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Net;
using System.IO;

public class Server : MonoBehaviour {

	public int port = 6321;

	private List<ServerClient> clients;
	private List<ServerClient> disconnectList;

	private TcpListener server;
	private bool serverStarted;

	public void Init(){
		DontDestroyOnLoad (gameObject);
		clients = new List<ServerClient> ();
		disconnectList = new List<ServerClient> ();

		try{
			//acceptem de tothom
			server = new TcpListener(IPAddress.Any,port);
			server.Start();
			StartListening();

		}catch(Exception e){
			Debug.Log ("Socket error : " + e.Message);
		}
	}

	private void Update(){
		if (!serverStarted)
			return;

		foreach (ServerClient c in clients) {
			//is the client still connected?
			if (!IsConnected (c.tcp)) {
				c.tcp.Close ();
				disconnectList.Add (c);
				continue;
			} else {
				NetworkStream s = c.tcp.GetStream ();
				if (s.DataAvailable) {
					StreamReader reader = new StreamReader (s, true);
					string data = reader.ReadLine (); //sempre escrivim una linea

					if (data != null)
						OnIncomingData (c, data);
				}
			}

		}

		for (int i = 0; i < disconnectList.Count - 1; i++) {

			//Tell our player somebody has disconnected


			clients.Remove (disconnectList [i]);
			disconnectList.RemoveAt (i);
		}
		
	}


	private void StartListening(){

		server.BeginAcceptTcpClient (AcceptTecpClient, server);
	}

	private void AcceptTecpClient(IAsyncResult ar){

		TcpListener listener = (TcpListener)ar.AsyncState;
		ServerClient sc = new ServerClient (listener.EndAcceptTcpClient (ar));
		clients.Add (sc);
		StartListening ();
		Debug.Log ("Somebody has connected");
	}
		
	private bool IsConnected(TcpClient c){
		try{
			if(c != null && c.Client != null && c.Client.Connected){
				//????
				if(c.Client.Poll(0,SelectMode.SelectRead)) return !(c.Client.Receive(new byte[1],SocketFlags.Peek)==0);
				return true;
			}else return false;
		}
		catch{
			return false;
		}
	}

	private void Broadcast(string data, List<ServerClient> cl){
		foreach(ServerClient sc in cl){
			try{
				StreamWriter writer = new StreamWriter(sc.tcp.GetStream());
				writer.WriteLine(data);
				writer.Flush();
			}catch (Exception e){
				Debug.Log ("Write error : " + e.Message);
			}
		}
	}

	private void OnIncomingData(ServerClient c, string data){
		Debug.Log (c.clientName + " : " + data);
	}
}

public class ServerClient
{
	public string clientName;
	public TcpClient tcp;

	public ServerClient(TcpClient tcp){
		this.tcp = tcp;
	}
}