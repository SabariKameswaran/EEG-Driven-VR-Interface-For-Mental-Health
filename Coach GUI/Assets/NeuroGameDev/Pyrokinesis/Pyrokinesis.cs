using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interaxon.Libmuse;

public class Pyrokinesis : MonoBehaviour
{
    public GameObject fireParticle;
    public float easeFactor = 1.1f; // Above 1 is easier between 0 and 1 is harder

    // Update is called once per frame
    void Update()
    {
        if(InteraxonInterfacer.Instance.currentConnectionState != ConnectionState.CONNECTED || InteraxonInterfacer.Instance.focus == 0f)
        {
            fireParticle.SetActive(false);
            return;
        }
        else
        {
            fireParticle.SetActive(true);
        }
        
        fireParticle.transform.localScale = new Vector3(1f, 0.01f + Math.Clamp((1.1f - InteraxonInterfacer.Instance.calm), 0 , 2) * (0.5f + (InteraxonInterfacer.Instance.focus * easeFactor)) * easeFactor, 1f);
        
    }
}
