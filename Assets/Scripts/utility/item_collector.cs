using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class item_collector : MonoBehaviour
{
    private int  mask= 3;

    [SerializeField] private AudioSource collectSoundEffect;
    [SerializeField] private Text cherriesText;
   

    private void OnTriggerEnter2D(Collider2D collection)
    {
        if (!collection.CompareTag("Finish"))Destroy(collection.gameObject);
        collectSoundEffect.Play();
        mask++;
        cherriesText.text = "masks:" + mask;

    }
}
