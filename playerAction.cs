using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.UI;

public class playerAction : MonoBehaviour {
	
	public Vector2 playerPosition;
	
	public GameObject controller;
	public GameObject statsCtrl;
	public statsController dataBase;
	
	public weaponProperties hitWeapon;
	public int moveindex;
	public int maxAction;
	public Vector3 positionOffset;

	public Vector2[,] toInstantiate;
	public weaponProperties[,] weapons;
	public List<int> attackWeapon = new List <int> ();
	private GameController gctrl;
	[SerializeField] actionController actrl;
	
	public int energy;
	public int exp;
	public Text energyDisplay;
	public Text expDisplay;
	public bool[] upgradeOn;


	[SerializeField] int playerIndex;

	//This script is only on "my" player, not the dummy of the other player

	void Start () {
		controller = GameObject.Find ("GameController(Clone)");
		statsCtrl = GameObject.Find("gameData");
		actrl = GameObject.Find ("actionController").GetComponent<actionController> ();
		dataBase = statsCtrl.GetComponent<statsController> ();
		gctrl=controller.GetComponent<GameController>();
		upgradeOn = gctrl.upgradeOn;
		energy = dataBase.startingEnergy;
		energyDisplay = GameObject.Find("energy").GetComponent<Text>();
		energyDisplay.text = "Energy:"+energy.ToString();
		expDisplay = GameObject.Find ("exp").GetComponent<Text>();
		expDisplay.text = "Exp:0";
	}
	
	void Update(){
		if(Input.GetKey("space")&&gctrl.commandable){			//I don't know how to send several lists at once
			this.GetComponent <PhotonView>().RPC("clearData",PhotonTargets.All);
			for(int i=0;i<gctrl.playerAttack.Count;i++){
				this.GetComponent <PhotonView>().RPC("addTarget",PhotonTargets.All,gctrl.playerAttack[i]);
			}
			for(int j=0;j<gctrl.playerMovement.Count;j++){
				if (j==0){
					this.GetComponent <PhotonView>().RPC("addMovement",PhotonTargets.All,new Vector2 (0,0));
				}else{
					this.GetComponent <PhotonView>().RPC("addMovement",PhotonTargets.All,gctrl.playerMovement[j]-gctrl.playerMovement[j-1]);
				}
			}
			for(int j=0;j<gctrl.playerMovement.Count;j++){
				this.GetComponent <PhotonView>().RPC("addWeapon",PhotonTargets.All,gctrl.attackWeapon[j]);
			}
			for(int j=0;j<gctrl.playerMovement.Count;j++){
				this.GetComponent <PhotonView>().RPC("addExtra",PhotonTargets.All,gctrl.extraMovement[j]);
			}
			playerPosition = gctrl.playerMovement[0];
			this.GetComponent <PhotonView>().RPC("getReady",PhotonTargets.All,PhotonNetwork.playerName,playerPosition);
			gctrl.commandable = false;
		}
	}

}

