using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Raycast_player : MonoBehaviour
{
    void Update()
    {
        RaycastHit hit;
        Debug.DrawRay(transform.position, transform.forward * 10, Color.green);
        if(Physics.Raycast(transform.position, transform.forward*10, out hit, 10))
        {
            Debug.Log(hit.transform.name);
        }
    }
}
