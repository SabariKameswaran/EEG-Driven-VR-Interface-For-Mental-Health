using UnityEngine;
using Interaxon.Libmuse;

public class PlagroundCalmSphere : MonoBehaviour
{
    private Vector3 _startPosition;
    
    void Start()
    {
        _startPosition = transform.position;
    }
    
    void Update()
    {
        if (InteraxonInterfacer.Instance.currentConnectionState != ConnectionState.CONNECTED || !InteraxonInterfacer.Instance.Artifacts.headbandOn)
        {
            //Lerp back to start position
            transform.position = Vector3.Lerp(transform.position, _startPosition, Time.deltaTime * 2f);
            return;
        }

        // Lerp to new position
        transform.position = Vector3.Lerp(transform.position, new Vector3(_startPosition.x, _startPosition.y + InteraxonInterfacer.Instance.calm, _startPosition.z), Time.deltaTime * 1f);
    }
}