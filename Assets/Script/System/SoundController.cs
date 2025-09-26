using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    [SerializeField] private AudioSource _audioMusic;
    [SerializeField] private AudioSource _audioSfx;

    private Dictionary<string, AudioClip> _dicSfx = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> _dicMusic = new Dictionary<string, AudioClip>();


    public AudioSource AudioMusic { get { return _audioMusic; } }
    public AudioSource AudioSfx { get { return _audioSfx; } }

    private void Start()
    {
        LocalSFXLoad();
    }
    /// <summary>
    /// 0 : Music 
    /// 1 : Sfx
    /// </summary>
    public int SetAudioVolume(int _Type, float _Volume)
    {
        switch(_Type)
        {
            case 0:
                _audioMusic.volume = _Volume;
                break;
            case 1:
                _audioSfx.volume = _Volume;
                break;
            default:
                return -1;
        }
        return 0;
    }
    public void LocalSFXLoad()
    {
        AudioClip[] t_Clip = Resources.LoadAll<AudioClip>("Sound/SFX");
        int t_Count = t_Clip.Length;
        for (int i = 0; i < t_Count; i++)
            _dicSfx.Add(t_Clip[i].name, t_Clip[i]);
    }
    public void LocalMusicLoad()
    {
        AudioClip[] t_Clip = Resources.LoadAll<AudioClip>("Sound/Music");
        int t_Count = t_Clip.Length;
        for (int i = 0; i < t_Count; i++)
            _dicMusic.Add(t_Clip[i].name, t_Clip[i]);
    }
    /// <summary>
    /// local load
    /// </summary>
    /// <param name="_Name"></param>
    public void PlayOneShot(string _Name)
    {
        _audioSfx.PlayOneShot(_dicSfx[_Name]);
    }
    /// <summary>
    /// TODO: Fade in/out 구현 필요
    /// </summary>
    /// <param name="_Name"></param>
    public void PlayMusic(string _Name)
    {
        _audioMusic.clip = _dicMusic[_Name];
    }

}
