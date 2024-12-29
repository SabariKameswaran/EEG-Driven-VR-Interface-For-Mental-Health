using UnityEngine;
using Interaxon.Libmuse;

public class PlaygroundEyeBlink : MonoBehaviour
{
    
    public Material eyeClosedMaterial;
    public Material eyeOpenMaterial;

    private Renderer _eyeRenderer;

    // Start is called before the first frame update
    void Start()
    {
        _eyeRenderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(InteraxonInterfacer.Instance.Artifacts.blink)
        {
            _eyeRenderer.material = eyeClosedMaterial;
        }
        else
        {
            _eyeRenderer.material = eyeOpenMaterial;
        }
    }
}
