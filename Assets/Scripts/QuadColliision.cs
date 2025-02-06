using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadColliision : MonoBehaviour
{
    System.Action<string> callback;
    string letter;

    bool setupCheck = false;
    public void Setup(System.Action<string> c, string l)
    {
        callback = c;
        letter = l;
        setupCheck = true;

    }

    //private void OnCollisionEnter(Collision collision)
    //{        
    //    if(setupCheck)
    //        callback(letter);
    //}
    
}
