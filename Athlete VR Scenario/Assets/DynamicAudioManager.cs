using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DynamicAudioManager : MonoBehaviour
{
    public AudioSource waterBlockAudioSource; 
    private Dictionary<int, AudioClip> audioMap;

    private string apiUrl = "http://localhost:5000/vr_score"; 
    private Coroutine crossfadeCoroutine = null; 

    public float crossfadeDuration = 1.0f; 

    void Start()
    {
        InitializeAudioMap();

        StartCoroutine(FetchDataAndPlayAudio());
    }

    void InitializeAudioMap()
    {
        audioMap = new Dictionary<int, AudioClip>();

        audioMap[0] = Resources.Load<AudioClip>("Audio/Base 1");
        audioMap[1] = Resources.Load<AudioClip>("Audio/Calm 1");
        audioMap[2] = Resources.Load<AudioClip>("Audio/Medium 1");
        audioMap[3] = Resources.Load<AudioClip>("Audio/High 1");
        audioMap[4] = Resources.Load<AudioClip>("Audio/Very High");
        audioMap[5] = Resources.Load<AudioClip>("Audio/Wind");

        foreach (var key in audioMap.Keys)
        {
            if (audioMap[key] == null)
            {
                Debug.LogError($"Audio file for key {key} could not be loaded. Check file name or path.");
            }
        }
    }

    IEnumerator FetchDataAndPlayAudio()
    {
        while (true)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string response = request.downloadHandler.text;

                    if (int.TryParse(response, out int apiValue) && audioMap.ContainsKey(apiValue))
                    {
                        PlayAudioWithCrossfade(apiValue);
                    }
                    else
                    {
                        Debug.LogError("Invalid API response or missing audio clip: " + response);
                    }
                }
                else
                {
                    Debug.LogError("Failed to fetch data from API: " + request.error);
                }
            }

            yield return new WaitForSeconds(5f);
        }
    }

    void PlayAudioWithCrossfade(int number)
    {
        if (audioMap.TryGetValue(number, out AudioClip newClip))
        {
            if (crossfadeCoroutine != null)
            {
                StopCoroutine(crossfadeCoroutine);
            }

            crossfadeCoroutine = StartCoroutine(CrossfadeAudio(newClip));
        }
        else
        {
            Debug.LogError("No audio clip assigned for the number: " + number);
        }
    }

    IEnumerator CrossfadeAudio(AudioClip newClip)
    {
        float startVolume = waterBlockAudioSource.volume;

        for (float t = 0; t < crossfadeDuration; t += Time.deltaTime)
        {
            waterBlockAudioSource.volume = Mathf.Lerp(startVolume, 0, t / crossfadeDuration);
            yield return null;
        }

        waterBlockAudioSource.Stop();
        waterBlockAudioSource.clip = newClip;
        waterBlockAudioSource.Play();

        for (float t = 0; t < crossfadeDuration; t += Time.deltaTime)
        {
            waterBlockAudioSource.volume = Mathf.Lerp(0, startVolume, t / crossfadeDuration);
            yield return null;
        }

        waterBlockAudioSource.volume = startVolume;

        crossfadeCoroutine = null;
    }
}
