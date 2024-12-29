using UnityEngine;
using Interaxon.Libmuse;

public class PlaygroundFocusCube : MonoBehaviour
{
    private Vector3 _startRotation;

    // Start is called before the first frame update
    void Start()
    {
        _startRotation = transform.rotation.eulerAngles;
    }
    
    void Update()
    {
        if (InteraxonInterfacer.Instance.currentConnectionState != ConnectionState.CONNECTED || !InteraxonInterfacer.Instance.Artifacts.headbandOn)
        {
            // Lerp back to start rotation
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(_startRotation), Time.deltaTime * 2f);
            return;
        }

        // Make Cube spin on its Y axis with focus
        transform.Rotate(0, InteraxonInterfacer.Instance.focus * 10, 0);
    }
}