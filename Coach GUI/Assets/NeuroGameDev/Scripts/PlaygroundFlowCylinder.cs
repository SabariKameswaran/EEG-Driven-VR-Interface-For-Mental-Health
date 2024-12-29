using UnityEngine;
using Interaxon.Libmuse;

public class PlaygroundFlowCylinder : MonoBehaviour
{
    private float _startScaleY;

    // Start is called before the first frame update
    void Start()
    {
        _startScaleY = transform.localScale.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (InteraxonInterfacer.Instance.currentConnectionState != ConnectionState.CONNECTED || !InteraxonInterfacer.Instance.Artifacts.headbandOn)
        {
            //Lerp back to original scale
            transform.localScale = Vector3.Lerp(transform.localScale,
                new Vector3(transform.localScale.x, _startScaleY, transform.localScale.z), Time.deltaTime * 2f);

            return;
        }

        // Lerp to new scale in Y axis with flow
        transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(transform.localScale.x, _startScaleY + (InteraxonInterfacer.Instance.flow * 10), transform.localScale.z), Time.deltaTime * 1f);
    }
}