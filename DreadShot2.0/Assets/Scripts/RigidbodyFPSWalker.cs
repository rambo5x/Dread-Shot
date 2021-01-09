﻿using UnityEngine;
using System.Collections;

public class RigidbodyFPSWalker : MonoBehaviour {

  //  private Rigidbody rigidbody;
	public float walkSpeed = 6.75f;
    public float runSpeed = 10.0f;
	public float gravity = 10.0f;
	public float maxVelocityChange = 10.0f;
	public bool canJump = true;
	public float jumpHeight = 2.0f;
    public float jumpPower;
    public float slopeLimit = 60f;
	private bool grounded = true;//false
    private bool tooSteep = false;
    private Vector3 groundNormal;
	public PhotonView name;

    public int kills = 0;
    public int deaths = 0;

    public Texture blood;
    public int bloodTimer;

	public GameObject me;
	public GameObject graphics;
	public GameObject ragDoll;
    public GameObject[] bots;
    public GameObject[] activePlayers;

    public int health = 100;

	public GameObject fpsCam;

	public bool isPause = false;

	public AudioSource sound1;
    [HideInInspector]
	public AudioClip soundClip;

    public string thekiller = "";
    public string killerWep = "";
	
	void Awake () {
    //    rigidbody = GetComponent<Rigidbody>();
		GetComponent<Rigidbody>().freezeRotation = true;
		GetComponent<Rigidbody>().useGravity = false;
		name.RPC ("updateName", PhotonTargets.AllBuffered, PhotonNetwork.playerName, health);
        gameObject.name = PhotonNetwork.playerName;

        
	}
	
	void FixedUpdate () {

        kills = PlayerPrefs.GetInt("kills");
        deaths = PlayerPrefs.GetInt("deaths");

		if (!tooSteep || !grounded) {
			// Calculate how fast we should be moving
			Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
			targetVelocity = transform.TransformDirection(targetVelocity);

            if (Input.GetKey(KeyCode.LeftShift))
            {
                targetVelocity *= runSpeed;
            }
            else
            {
                targetVelocity *= walkSpeed;
            }
			
			// Apply a force that attempts to reach our target velocity
			Vector3 velocity = GetComponent<Rigidbody>().velocity;
			Vector3 velocityChange = (targetVelocity - velocity);
			velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
			velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
			velocityChange.y = 0;
			GetComponent<Rigidbody>().AddForce(velocityChange, ForceMode.VelocityChange);

            /* Get the velocity
            Vector3 horizontalMove = GetComponent<Rigidbody>().velocity;
            // Don't use the vertical velocity
           // horizontalMove.y = 0;
            // Calculate the approximate distance that will be traversed
            float distance = horizontalMove.magnitude * Time.fixedDeltaTime;
            // Normalize horizontalMove since it should be used to indicate direction
            horizontalMove.Normalize();
            RaycastHit hit;
            
            // Check if the body's current velocity will result in a collision
            if (GetComponent<Rigidbody>().SweepTest(horizontalMove, out hit, distance))
        //    if(get)
            {
                // If so, stop the movement
                Debug.Log("Is colliding");
                GetComponent<Rigidbody>().velocity = new Vector3(0, GetComponent<Rigidbody>().velocity.y, 0);
                canJump = false;
            }
            else
            {
                Debug.Log("Not colliding");
                canJump = true;
            }*/

            // Jump
            bloodTimer += -1;

            if (grounded && Input.GetButton("Jump")) {
                
				GetComponent<Rigidbody>().velocity = new Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
                
      /*          if(GetComponent<Rigidbody>().velocity.y > CalculateJumpVerticalSpeed())
                {
                    GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
                } */
            }
		}
        grounded = false;
        groundNormal = Vector3.zero;
        tooSteep = true;
        playerListView();
		
		// We apply gravity manually for more tuning control
		GetComponent<Rigidbody>().AddForce(new Vector3 (0, -gravity * GetComponent<Rigidbody>().mass, 0));
		
		//grounded = false;

		if (health <= 0) {
			GetComponent<PhotonView>().RPC ("DIE", PhotonTargets.AllBuffered, null);
		}

        bots = GameObject.FindGameObjectsWithTag("AI");

        if (isPause)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        } 
        /*else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        } */
	}

    void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contactPoint in collision.contacts)
        {
            groundNormal += contactPoint.normal;
            if (contactPoint.normal.y > Mathf.Cos(slopeLimit * Mathf.Deg2Rad))
            {
                tooSteep = false;
            }
        }
        if (collision.contacts.Length > 0)
        {
            groundNormal /= collision.contacts.Length;
        }
        grounded = true;
    }

    /*  void OnCollisionEnter(Collision collider)
      {
          if (collider.gameObject.tag == "Generic") // this string is your newly created tag
          {
              // TODO: anything you want
              // Even you can get Bullet object
              Debug.Log("Collided");
           //   GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
              canJump = false;
          }
      }

      void OnCollisionStay(Collision other)
      {

          if (other.gameObject.tag == "Generic") // this string is your newly created tag
          {
              // TODO: anything you want
              // Even you can get Bullet object
              //   Debug.Log("Collided");
              Debug.Log("Is colliding");
        //      GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
              canJump = false;
          }
      }

      void OnCollisionExit(Collision other)
      {
          Debug.Log("Not colliding");
          canJump = true;
      } */

    void OnCollisionStay () {
		grounded = true;    
	}
	
	float CalculateJumpVerticalSpeed () {
		// From the jump height and gravity we deduce the upwards speed 
		// for the character to reach at the apex.
        jumpPower = Mathf.Sqrt(2 * jumpHeight * gravity);
        return jumpPower;
	}

   

    void playerListView()
    {
        if (Input.GetKeyDown(KeyCode.CapsLock))
        {
            isPause = true;
          /*  if (!isPause)
            {
                isPause = true;
            }else
            {
                isPause = false;
            }*/
        }
        if (Input.GetKeyUp(KeyCode.CapsLock))
        {
            isPause = false;
        } 
    }


	void OnGUI(){
		GUI.Box (new Rect (10, 10, 100, 30), "HP | " + health);

        if(bloodTimer >= 1)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blood, ScaleMode.StretchToFill);
        }

      

		if (isPause) {
           
			GUILayout.BeginArea (new Rect(Screen.width/2 - 250, Screen.height/2 - 250, 500,500));
            GUILayout.Box("ActiveBots: " + "x" + bots.Length);
            GUILayout.Box("Score: " + "\nKills: " + kills + " / "  + "Deaths: " + deaths);
            GUILayout.Box("Players:     Name        K/D   ");
            foreach (PhotonPlayer pl in PhotonNetwork.playerList)
            {
                GUILayout.Box(pl.NickName + " | " + pl.GetScore());
            }
            if (GUILayout.Button("Disconnect"))
            {
                PhotonNetwork.Disconnect();
                Application.LoadLevel(0);
            }
            

            GUILayout.EndArea();
        }
	}

   


    public void getClip (AudioClip soundToRecieve){
		Debug.Log (soundToRecieve.name);
		soundClip = soundToRecieve;
	}



	[PunRPC]
	public void applyDamage(int dmg, string killername, string weaponName){
		health = health - dmg;
      //  GameObject.Find(killername).GetComponent<PhotonView>().RPC("addKill", PhotonTargets.AllBuffered);
		name.RPC ("updateName", PhotonTargets.AllBuffered, PhotonNetwork.playerName, health);
        thekiller = killername;
        killerWep = weaponName;
		Debug.Log ("hit!" + health);
        bloodTimer = 20;
	}


    void OnCollisionEnter(Collision col)
    {
        if (col.transform.tag == "Bullet")
        {
            Debug.Log("Hit");
            int dmg = col.transform.GetComponent<bulScript>().dmg;
            string aiName = col.transform.GetComponent<bulScript>().name;
            GetComponent<PhotonView>().RPC("applyDamage", PhotonTargets.All, dmg, aiName, "M4");
        }
    }


    [PunRPC]
	public void DIE(){
        if (GetComponent<PhotonView>().isMine) {
       
         
            PhotonNetwork.Destroy(me);
            Destroy(me);
            GameObject rDoll = PhotonNetwork.Instantiate(ragDoll.name, transform.position, transform.rotation, 0);
            Destroy(rDoll, 3);
            GameObject.Find("_ROOM").GetComponent<roomMan>().OnJoinedRoom();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            GameObject.Find("_NetworkScripts").GetComponent<PhotonView>().RPC("addFeed", PhotonTargets.All, thekiller + " [" + killerWep + "] " + PhotonNetwork.playerName);

            Debug.Log("die");
            PlayerPrefs.SetInt("deaths", deaths + 1);

            GameObject killer = GameObject.Find(thekiller);
            killer.GetComponent<PhotonView>().RPC("exitTrig", PhotonTargets.AllBuffered, null);
            killer.GetComponent<PhotonView>().RPC("addKill", PhotonTargets.AllBuffered, null);
        }
			

	}

    [PunRPC]
    public void addKill()
    {
        PlayerPrefs.SetInt("kills", kills + 1);
        Debug.Log("added kill: " + kills);
    }

//	public AudioClip[] sounds;


	[PunRPC]
	public void playSound(){
		Debug.Log ("Sound: " + soundClip.name);
		sound1.PlayOneShot (soundClip);
	}
}

