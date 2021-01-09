using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour {

    public InRoomChat irc;

    [PunRPC]
    public void addFeed(string feed)
    {
        if (GetComponent<PhotonView>().isMine)
        {
            irc.addKill(feed);
        }
    }


    [PunRPC]
    public void deleteBot()
    {
        GameObject[] bots = GameObject.FindGameObjectsWithTag("AI");
        Destroy(bots[Random.Range(0, bots.Length)]);
    }

}
