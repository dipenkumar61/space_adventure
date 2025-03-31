using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Target : MonoBehaviour
{
    [SerializeField] GameObject myText;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitForSeconds(6);
        myText.SetActive(true); // Enable the text so it shows
        yield return new WaitForSeconds(4);
        myText.SetActive(false); // Disable the text so it is hidden
    }


}
