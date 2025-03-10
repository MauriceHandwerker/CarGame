using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spin : MonoBehaviour
{
    public Vector3 spinSpeed = new Vector3(0, 50, 0);

    void Update()
    {
        transform.Rotate(spinSpeed * Time.deltaTime);
    }
}
