using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instructions : MonoBehaviour
{
    public float after = 4f;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(after);
        gameObject.SetActive(false);
    }

}
