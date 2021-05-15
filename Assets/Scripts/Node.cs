using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Ref: https://www.youtube.com/watch?v=OffZiiq0qcI

public class Node : MonoBehaviour
{

    public GameObject parent;
    // Start is called before the first frame update
    void Start()
    {
        parent.transform.GetChild(0).gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
