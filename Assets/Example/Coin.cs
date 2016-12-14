using UnityEngine;
using System;
using System.Collections.Generic;

public class Coin : MonoBehaviour {

	void OnTriggerEnter()
    {
        Destroy(transform.parent.gameObject);
    }
}
