using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class networkController : MonoBehaviour {
	
	[SerializeField] Text connectionText;
	[SerializeField] Vector2[] spawnPoints;
	[SerializeField] Camera sceneCamera;
	[SerializeField] GameObject controller; 
	[SerializeField] GameObject serverWindow;
	[SerializeField] InputField username;
	[SerializeField] InputField roomName;
	[SerializeField] InputField roomList;
	[SerializeField] InputField messageWindow;

	[SerializeField] actionController actrl;
	[SerializeField] GameController gctrl;

	PhotonView photonView;
	
	GameObject player;
	Queue<string> messages;
	const int messageCount = 6;

	void Start () {
		photonView = GetComponent<PhotonView> ();
		controller = GameObject.Find ("GameController(Clone)");
		messages = new Queue<string> (messageCount);
		gctrl=controller.GetComponent<GameController>();
		PhotonNetwork.logLevel = PhotonLogLevel.Full;
		PhotonNetwork.ConnectUsingSettings ("0.2");
		StartCoroutine ("UpdateConnectionString");
		PhotonNetwork.autoJoinLobby = true;
	}
	
	IEnumerator UpdateConnectionString () 
	{
		while(true)
		{
			connectionText.text = PhotonNetwork.connectionStateDetailed.ToString ();
			yield return null;
		}
	}

	void OnJoinedLobby()
	{
		serverWindow.SetActive (true);
	}

	public void joinRoom(){
		PhotonNetwork.player.name = username.text;
		RoomOptions roomOptions = new RoomOptions(){ isVisible = true, maxPlayers = 2 };PhotonNetwork.JoinOrCreateRoom (roomName.text, roomOptions, TypedLobby.Default);
	}
	
	void OnReceivedRoomListUpdate()
	{
		print ("new room!");
		roomList.text = "";
		RoomInfo[] rooms = PhotonNetwork.GetRoomList ();
		foreach(RoomInfo room in rooms)
			roomList.text += room.name + "\n";
	}
	
	void OnJoinedRoom()
	{
		serverWindow.SetActive (false);
		StartSpawnProcess (0f);
		StopCoroutine ("UpdateConnectionString");
		connectionText.text = "";
	}

	//the code is devised based on a multiplayer FPS tutorial so the players are spawned over and over again
	//now it looks like game should end when one player dies, but maybe the rules will be changed so I decide to keep this
	void StartSpawnProcess (float respawnTime)
	{
		sceneCamera.enabled = false;
		StartCoroutine ("SpawnPlayer", respawnTime);
	}
	
	IEnumerator SpawnPlayer(float respawnTime)
	{
		yield return new WaitForSeconds(respawnTime);
		player = PhotonNetwork.Instantiate ("SSplayer", 
		                                    gctrl.positionTransform (spawnPoints[PhotonNetwork.playerList.Length]),
		                                    Quaternion.identity,
		                                    0);
		gctrl.clearSetup(spawnPoints[PhotonNetwork.playerList.Length]);
		player.GetComponent<playerNetworkMover> ().RespawnMe += StartSpawnProcess;
		player.GetComponent<playerNetworkMover> ().SendNetworkMessage += AddMessage;
		actrl.SendNetworkMessage += AddMessage;
	}
	void AddMessage(string message)
	{
		photonView.RPC ("AddMessage_RPC", PhotonTargets.All, message);
	}
	
	[PunRPC]
	public void AddMessage_RPC(string message)
	{
		messages.Enqueue (message);
		if(messages.Count > messageCount)
			messages.Dequeue();
		
		messageWindow.text = "";
		foreach(string m in messages)
			messageWindow.text += m + "\n";
	}
}
