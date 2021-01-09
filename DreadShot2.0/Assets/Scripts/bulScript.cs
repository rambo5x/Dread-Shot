using UnityEngine;
using System.Collections;

public class bulScript : MonoBehaviour
{

    public int dmg;
   // public string name;
    

    void Awake()
    {
        gameObject.name = "bullet";
    }

    void OnCollisionEnter()
    {
        //Destroy(gameObject);
        GetComponent<PhotonView>().RPC("bulletDIE", PhotonTargets.AllBuffered, null);
    }

    [PunRPC]
    public void bulletDIE()
    {
        if (GetComponent<PhotonView>().isMine)
        {
            PhotonNetwork.Destroy(gameObject);
            Destroy(gameObject);
        }
    }

}
