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
        //Public Voxel / Private Voxel / Semi Voxel
        parent.transform.GetChild(0).gameObject.SetActive(false);
        parent.transform.GetChild(1).gameObject.SetActive(false);
        parent.transform.GetChild(2).gameObject.SetActive(false);
        //GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Basic");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
