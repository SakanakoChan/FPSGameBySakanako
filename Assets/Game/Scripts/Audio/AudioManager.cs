using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioMixerGroup sfxGroup;
    private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
    public int sourcePoolSize = 20;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        InitializeAudioSourcePool();
    }

    private void Start()
    {
        ApplyVolume("MasterVolume", GameSettings.masterVolume);
        ApplyVolume("SFXVolume", GameSettings.sfxVolume);
    }


    public void PlaySound(AudioClip _audioClip, Vector3 _position)
    {
        AudioSource audioSource = GetAudioSourceFromPool();

        audioSource.transform.position = _position;
        audioSource.clip = _audioClip;
        audioSource.outputAudioMixerGroup = sfxGroup;
        audioSource.Play();

        StartCoroutine(ReturnAudioSourceToPool(audioSource, _audioClip.length));
    }

    public void PlaySound(AudioClip _audioClip, Vector3 _position, float _pitch)
    {
        AudioSource audioSource = GetAudioSourceFromPool();

        audioSource.transform.position = _position;
        audioSource.clip = _audioClip;
        audioSource.pitch = _pitch;
        audioSource.outputAudioMixerGroup = sfxGroup;
        audioSource.Play();

        StartCoroutine(ReturnAudioSourceToPool(audioSource, _audioClip.length));
    }

    private AudioSource GetAudioSourceFromPool()
    {
        if (audioSourcePool.Count > 0)
        {
            return audioSourcePool.Dequeue();
        }
        else
        {
            GameObject obj = new GameObject("Audio Source");
            obj.transform.SetParent(transform);
            AudioSource audioSource = obj.AddComponent<AudioSource>();
            return audioSource;
        }
    }

    private IEnumerator ReturnAudioSourceToPool(AudioSource _audioSource, float _delay)
    {
        yield return new WaitForSeconds(_delay);

        _audioSource.Stop();
        _audioSource.clip = null;
        _audioSource.loop = false;
        _audioSource.pitch = 1;
        _audioSource.volume = 1;
        _audioSource.transform.SetParent(transform);

        audioSourcePool.Enqueue(_audioSource);
    }

    private void InitializeAudioSourcePool()
    {
        for (int i = 0; i < sourcePoolSize; i++)
        {
            GameObject obj = new GameObject("Audio Source");
            obj.transform.SetParent(transform);

            AudioSource audioSource = obj.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

            audioSourcePool.Enqueue(audioSource);
        }
    }

    public void ApplyVolume(string _mixerParamName, float _settingValue)
    {
        if (mixer != null)
        {
            float volumeValue = Mathf.Clamp(_settingValue, 0.0001f, 1f);
            float db = Mathf.Log10(volumeValue) * 20;

            mixer.SetFloat(_mixerParamName, db);
        }
    }
}
