using UnityEngine;
using Interaxon.Libmuse;

public class PlaygroundJawClench : MonoBehaviour
{
    public GameObject jaw;
    private Vector3 _startPosition;

    // Start is called before the first frame update
    void Start()
    {
        _startPosition = jaw.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if(InteraxonInterfacer.Instance.Artifacts.jawClench)
        {
            // Lerp to new position
            jaw.transform.localPosition = Vector3.Lerp(jaw.transform.localPosition, new Vector3(_startPosition.x, _startPosition.y + 0.25f, _startPosition.z), Time.deltaTime * 2f);
        
        }
        else
        {
            //Lerp back to start position
            jaw.transform.localPosition = Vector3.Lerp(jaw.transform.localPosition, _startPosition, Time.deltaTime * 1f);
        }
    }
}
