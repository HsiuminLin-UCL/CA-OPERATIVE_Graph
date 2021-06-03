using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Ref: https://www.youtube.com/watch?v=OffZiiq0qcI

public class Node : MonoBehaviour
{
    public GameObject parent;
    void Start()
    {
        //0-Public Voxel / 1-Private Voxel / 2-Semi Voxel
        parent.transform.GetChild(0).gameObject.SetActive(false);
        parent.transform.GetChild(1).gameObject.SetActive(false);
        parent.transform.GetChild(2).gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
