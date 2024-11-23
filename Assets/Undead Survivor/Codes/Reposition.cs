using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reposition : MonoBehaviour
{
    Collider2D coll;
    GameObject cameraMain;

    void Awake()
    {
        coll = GetComponent<Collider2D>();
        cameraMain = GameObject.Find("Main Camera");
    }
    void Update(){
        if( transform.tag == "Ground"){
            Vector3 cameraPos = cameraMain.transform.position;
            Vector3 myPos = transform.position;
            float diffX = cameraPos.x - myPos.x;
            float diffY = cameraPos.y - myPos.y;
            if (Mathf.Abs(diffX) >= 40 || Mathf.Abs(diffY) >= 40){
                float dirX = diffX < 0 ? -1 : 1;
                float dirY = diffY < 0 ? -1 : 1;
                int corrX = (int) Mathf.Abs(diffX/20);
                int corrY = (int) Mathf.Abs(diffY/20);
                diffX = Mathf.Abs(diffX);
                diffY = Mathf.Abs(diffY);
                if (diffX > diffY)
                {
                    transform.Translate(Vector3.right * dirX * 30 * corrX);
                }
                else if (diffX < diffY)
                {
                    transform.Translate(Vector3.up * dirY * 30 * corrY);
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Area"))
            return;

        Vector3 playerPos = GameManager.Instance.player.transform.position;
        Vector3 myPos = transform.position;


        switch (transform.tag)
        {
            case "Ground":
                /*
                float diffX = playerPos.x - myPos.x;
                float diffY = playerPos.y - myPos.y;
                float dirX = diffX < 0 ? -1 : 1;
                float dirY = diffY < 0 ? -1 : 1;
                diffX = Mathf.Abs(diffX);
                diffY = Mathf.Abs(diffY);
                if (diffX > diffY)
                {
                    transform.Translate(Vector3.right * dirX * 40);
                }
                else if (diffX < diffY)
                {
                    transform.Translate(Vector3.up * dirY * 40);
                }
                */
                break;
            case "Enemy":
                if (coll.enabled)
                {
                    Vector3 dist = playerPos - myPos;
                    Vector3 ran = new Vector3(Random.Range(-3, 3), Random.Range(-3, 3), 0);
                    transform.Translate(ran + dist * 2);
                }
                break;
        }
    }
}