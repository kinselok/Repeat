using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimPlus1 : MonoBehaviour
{
    public GameObject Plus1;
    private float A;


    // Use this for initialization
    void Start()
    {
        A = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {

        if (A > 0.01)
        {
            Plus1.transform.position = new Vector3(Plus1.transform.position.x, Plus1.transform.position.y + 9f * Time.deltaTime, Plus1.transform.position.z);
            A -= 0.025f;
            Color changedColor = new Color(0.2566305f, 0.745283f, 0.2739844f, A);
            Plus1.GetComponent<Text>().color = changedColor;
        }
        else
        {
            Destroy(Plus1);
        }
    }
}
