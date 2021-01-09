using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class roomMan : Photon.MonoBehaviour
{

    public string verNum = "0.3";
    public string roomName = "room01";
    public string playerName = "player 420";
    public Transform spawnPoint;
    public GameObject playerPref;
    public GameObject playerPref2;
    public bool isConnected = false;
    public bool isInRoom = false;
    public bool mapChange = false;
    public bool isWarmupMap = true, isWarehouseMap = false;
    public GameObject[] ais;
    public GameObject[] curAis;
    public int kd;
   
   // public Text warningText;

    public InRoomChat chat;

    public Transform[] spawnPoints;


    void Update()
    {
        if (isInRoom)
        {
            chat.enabled = true;
        }
        else
        {
          //  chat.enabled = false;
        }

        curAis = GameObject.FindGameObjectsWithTag("AI");

        if (Input.GetKeyDown(KeyCode.E) && !isInRoom)
        {
            spawnAI();
        }

        if(PlayerPrefs.GetInt("kills") >= 1)
        {
            kd = PlayerPrefs.GetInt("kills") / PlayerPrefs.GetInt("deaths");
        }
        else
        {
            kd = 0;
        }


        PhotonNetwork.player.SetScore(PlayerPrefs.GetInt("kills"));

    }

    void Start()
    {
     //   warningText = GetComponent<Text>();
        roomName = "Room " + Random.Range(0, 999);
        playerName = "Player " + Random.Range(0, 999);
        PhotonNetwork.ConnectUsingSettings(verNum);
        Debug.Log("Starting Connection!");

     //   PlayerPrefs.SetInt("kills", 0);
     //   PlayerPrefs.SetInt("deaths", 0);
    }

    public void OnJoinedLobby()
    {
        //PhotonNetwork.JoinOrCreateRoom (roomName, null, null);
        isConnected = true;
        Debug.Log("Starting Server!");
    }

    public void OnJoinedRoom()
    {
        PhotonNetwork.playerName = playerName;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject pl in players)
        {
            if(pl.name == PhotonNetwork.playerName)
            {
                PhotonNetwork.playerName = playerName + " " + Random.Range(0, 999);
            }
        }

        isConnected = false;
        isInRoom = true;
        //spawnPlayer ();
    }


    public void spawnPlayer(string prefName)
    {
        isInRoom = false;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (PhotonPlayer pl2 in PhotonNetwork.playerList)
        {
            if (pl2.NickName == PhotonNetwork.playerName)
            {
                PhotonNetwork.playerName = playerName + " " + Random.Range(0, 999);
            }
        }

        GameObject pl = PhotonNetwork.Instantiate(prefName, spawnPoints[Random.Range(0, spawnPoints.Length)].position, spawnPoint.rotation, 0) as GameObject;
        pl.GetComponent<RigidbodyFPSWalker>().enabled = true;
        pl.GetComponent<RigidbodyFPSWalker>().fpsCam.SetActive(true);
        pl.GetComponent<RigidbodyFPSWalker>().graphics.SetActive(false);

    }

    public void spawnAI()
    {
       
        GameObject ai1 = PhotonNetwork.Instantiate(ais[Random.Range(0, ais.Length)].name, spawnPoints[Random.Range(0, spawnPoints.Length)].position, spawnPoint.rotation, 0) as GameObject;
        ai1.GetComponent<AIController>().isMine = true;
    }

    void OnGUI()
    {

        if (isConnected)
        {
            GUILayout.BeginArea(new Rect(Screen.width / 2 - 250, Screen.height / 2 - 250, 500, 500));
            playerName = GUILayout.TextField(playerName);
            roomName = GUILayout.TextField(roomName);

          

            if (GUILayout.Button("Maps"))
            {
                mapChange = true;
                isConnected = false;
            }

            if (GUILayout.Button("Create"))
            {
                PhotonNetwork.JoinOrCreateRoom(roomName, null, null);
            }

            foreach (RoomInfo game in PhotonNetwork.GetRoomList())
            {
                if (GUILayout.Button(game.Name + " " + game.PlayerCount + " | " + game.MaxPlayers))
                {
                   
                        PhotonNetwork.JoinOrCreateRoom(game.Name, null, null);
                }
            }
            GUILayout.EndArea();
        }

        if (mapChange)
        {
            GUILayout.BeginArea(new Rect(Screen.width / 2 - 250, Screen.height / 2 - 250, 500, 500));
            if (GUILayout.Button("Back"))
            {
                mapChange = false;
                isConnected = true;
            }
            if (GUILayout.Button("Warmup Arena Map"))
            {
                
                PhotonNetwork.Disconnect();
                SceneManager.LoadScene("DeveloperArena");
                isWarmupMap = true;
                isWarehouseMap = false;
            }
            if (GUILayout.Button("Warehouse Map"))
            {
                PhotonNetwork.Disconnect();
                SceneManager.LoadScene("WarehouseMap");
                isWarmupMap = false;
                isWarehouseMap = true;
            }
        /*    if (GUILayout.Button("Pool Map"))
            {
                PhotonNetwork.Disconnect();
                SceneManager.LoadScene("PooldayMap");
                isWarmupMap = false;
                isWarehouseMap = true;
            }
            if (GUILayout.Button("Italy Map"))
            {
                PhotonNetwork.Disconnect();
                SceneManager.LoadScene("ItalyMap");
                isWarmupMap = false;
                isWarehouseMap = true;
            }
            if (GUILayout.Button("Aztec Map"))
            {
                PhotonNetwork.Disconnect();
                SceneManager.LoadScene("AztecMap");
                isWarmupMap = false;
                isWarehouseMap = true;
            }
            if (GUILayout.Button("Desert Map"))
            {
                PhotonNetwork.Disconnect();
                SceneManager.LoadScene("DesertMap");
                isWarmupMap = false;
                isWarehouseMap = true;
            }
            if (GUILayout.Button("IceWorld Map"))
            {
                PhotonNetwork.Disconnect();
                SceneManager.LoadScene("IceWorldMap");
                isWarmupMap = false;
                isWarehouseMap = true;
            }*/
            GUILayout.EndArea();
        }

        if (isInRoom) {
      //      warningText.GetComponent<Text>().text = "";
            GUILayout.BeginArea(new Rect(Screen.width / 2 - 250, Screen.height / 2 - 250, 500, 500));

            GUILayout.Box("Bots: x" + curAis.Length);
            GUILayout.Box("Score: " + PhotonNetwork.player.GetScore());
            GUILayout.Box("Kills: " + PlayerPrefs.GetInt("kills") + " | " + "Deaths: " + PlayerPrefs.GetInt("deaths") + " | K/D: " + kd );
            if (GUILayout.Button("Assault"))
            {
                spawnPlayer(playerPref2.name);
            }
            if (GUILayout.Button("SWAT"))
            {
                spawnPlayer(playerPref.name);
            }
        
            
            if (GUILayout.Button("Disconnect"))
            {
                PhotonNetwork.Disconnect();
                SceneManager.LoadScene(0);
            }

            if (GUILayout.Button("Spawn 2 Bots"))
            {
                spawnAI();
                spawnAI();
            }

            if (GUILayout.Button("Delete Bot"))
            {
                GameObject.Find("_NetworkScripts").GetComponent<PhotonView>().RPC("deleteBot", PhotonTargets.AllBuffered, null);
            }


            GUILayout.EndArea();
        }
    }


}
