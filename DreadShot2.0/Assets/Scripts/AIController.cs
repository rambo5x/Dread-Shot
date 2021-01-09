using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour
{
    public bool isMine = false;
    public NavMeshAgent na;
    public GameObject[] targets;

    public bool isRanged = false;

    public GameObject me;
    public GameObject ragDoll;
    public GameObject bullet;
    public float bulletSpeed = 300f;
    public Transform muzzle;

    public PhotonView pv;

    public float fireRate = 1;
    public int blDamage = 3;

    public Animation am;
    public AnimationClip reload;
    public AnimationClip run;

    public int ammo = 10;
    public int maxAmmo = 10;

    public bool inTrig = false;
    public bool isEmpty = true;
    public int timer = 30;

    public AudioClip shootSound;
    public AudioSource aS;

    public int health = 100;
    public string killername = "";
    public string killerwep = "";
    public string botName;
    public int damage = 20;
    public int locTimer = 20;

    public PhotonView nameTag;

    void Awake()
    {
        botName = "Bot: " + Random.Range(0, 999);
        targets = GameObject.FindGameObjectsWithTag("Player");
        if (isMine)
        {
            Debug.Log("setting target");

            GetComponent<PhotonView>().RPC("setTarget", PhotonTargets.AllBuffered, null);
        }
        target();
        gameObject.name = name;

        nameTag.RPC("updateName", PhotonTargets.AllBuffered, name, health);
    }

    void FixedUpdate()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] ais = GameObject.FindGameObjectsWithTag("AI");

        if (inTrig && isRanged)
        {
            Vector3 lookAt = new Vector3(na.destination.x, this.transform.position.y, na.destination.z);
            transform.LookAt(lookAt);
        }

        if(players.Length >= 1)
        {
            targets = players;
        }
        else
        {
            if(ais.Length >= 1)
            {
                targets = ais;
            }
           
        }

        if (isRanged)
        {
            if (am.IsPlaying(reload.name))
            {
                GetComponent<Rigidbody>().isKinematic = true;
                na.isStopped = true;
            }
            else
            {
                GetComponent<Rigidbody>().isKinematic = false;
                na.isStopped = false;
                pv.RPC("playAnim", PhotonTargets.AllBuffered, run.name);
            }
        }

        float speedMultiplier = 1.0f - 0.9f * Vector3.Angle(transform.forward, na.steeringTarget - transform.position) / 180.0f;
        na.speed = 7 * speedMultiplier;
               
           if(na.hasPath == false)
           {
               Debug.Log("dest is null");
               target();
           } 

        target();

        if (health <= 0)
        {
            GetComponent<PhotonView>().RPC("DIE", PhotonTargets.AllBuffered, null);
        }

        if (!inTrig)
        {
          locTimer = timer;
        }
      
        if (inTrig)
        {
            locTimer += -1;

         
          //  shoot();
          if(locTimer <= 0)
            {
                shoot();
                locTimer = timer;
            }
        }
    }

    [PunRPC]
    public void playAnim(string animName)
    {
        am.CrossFade(animName);
    }

    public void target()
    {
        Debug.Log("target");
        if (isMine && !na.hasPath && targets.Length >= 1)
        {
            Debug.Log("setting target");
        
            GetComponent<PhotonView>().RPC("setTarget", PhotonTargets.AllBuffered, null);
        }
    }

    void OnCollisionStay(Collision col)
    {
        if (col.transform.tag == "Player")
        {
            col.transform.GetComponent<PhotonView>().RPC("applyDamage", PhotonTargets.AllBuffered, damage, botName, "Melee");
        }

        if (col.transform.tag == "AI")
        {
            col.transform.GetComponent<PhotonView>().RPC("AIDamage", PhotonTargets.AllBuffered, damage, botName, "Melee");
        }

    }

    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player")
        {
            inTrig = true;
            na.destination = col.transform.position;
            transform.LookAt(col.transform);
            Debug.Log("entered!");


            if (col.gameObject == null)
            {
                inTrig = false;
            }

        }

        if (col.tag == "AI")
        {
            inTrig = true;
            na.destination = col.transform.position;
          //  transform.LookAt(col.transform);
            Debug.Log("entered!");


            if (col.gameObject == null)
            {
                inTrig = false;
            }

        }

    }

    void OnCollisionEnter(Collision col)
    {

        if (col.transform.tag == "Bullet")
        {
            Debug.Log("Hit");
            int dmg = col.transform.GetComponent<bulScript>().dmg;
            string aiName = col.transform.GetComponent<bulScript>().name;
            GetComponent<PhotonView>().RPC("AIDamage", PhotonTargets.All, dmg, aiName, "M4");
        }
    }





    void OnTriggerExit(Collider col)
    {
        if (col.tag == "Player" || col.tag == "AI")
        {
            inTrig = false;
        }
    }

   // public GameObject muzzleFlash;

    public void shoot()
    {

        if (isRanged && inTrig)
        {
            if (ammo >= 1 && !am.IsPlaying(reload.name))
            {
                // Debug.Log(ammo);
                ammo += -1;
            //    GameObject muz = PhotonNetwork.Instantiate(muzzleFlash.name, muzzle.position, muzzle.rotation, 0);
             //   Destroy(muz, 0.5f);
                GameObject bl = PhotonNetwork.Instantiate(bullet.name, muzzle.position, muzzle.rotation, 0) as GameObject;
                bl.GetComponent<Rigidbody>().AddForce(transform.forward * bulletSpeed);
                bl.GetComponent<bulScript>().dmg = blDamage;
                bl.GetComponent<bulScript>().name = name;
                pv.RPC("playShoot", PhotonTargets.AllBuffered, null);
            }
            else
            {
                doReload();
            }
        }
    }

    public void doReload()
    {
        
        Debug.Log("Reloading");
        ammo = maxAmmo;
        pv.RPC("playAnim", PhotonTargets.AllBuffered, reload.name);
    }

    [PunRPC]
    public void exitTrig()
    {
        inTrig = false;
    }


    [PunRPC]
    public void playShoot()
    {
        aS.PlayOneShot(shootSound);
    }

    [PunRPC]
    public void setTarget()
    {
       // if (targets.Length >= 1)
       // {

            Debug.Log("setting target");
            na.SetDestination(targets[Random.Range(0, targets.Length)].transform.position);
      //  }
    }

    [PunRPC]
    public void AIDamage(int damage, string pName, string wpnName)
    {
        killername = pName;
        killerwep = wpnName;
        health -= damage;
        na.destination = GameObject.Find(pName).transform.position;
        Debug.Log(health);
        nameTag.RPC("updateName", PhotonTargets.AllBuffered, name, health);
    }

    [PunRPC]
    public void DIE()
    {
        if (GetComponent<PhotonView>().isMine)
        {
            PhotonNetwork.Destroy(me);
            Destroy(me);
            GameObject rDoll = PhotonNetwork.Instantiate(ragDoll.name, transform.position, transform.rotation, 0);
            PhotonNetwork.Destroy(rDoll);
            Destroy(rDoll);
         //   Destroy(rDoll, 3);
            GameObject.Find("_ROOM").GetComponent<roomMan>().spawnAI();
         //   Cursor.lockState = CursorLockMode.None;
          //  Cursor.visible = true;
            GameObject.Find("_NetworkScripts").GetComponent<PhotonView>().RPC("addFeed", PhotonTargets.All, killername + " [" + killerwep + "] " + botName);
            GameObject killer = GameObject.Find(killername);
            killer.GetComponent<PhotonView>().RPC("exitTrig", PhotonTargets.AllBuffered, null);
            killer.GetComponent<PhotonView>().RPC("addKill", PhotonTargets.AllBuffered, null);

        }


    }


}
