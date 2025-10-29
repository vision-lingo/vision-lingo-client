using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;

public class SoundController : MonoBehaviour
{
    [SerializeField] private AudioMixer _masterMixer;
    [SerializeField] private AudioSource _audioMusic;
    [SerializeField] private AudioSource _audioSfx;

    private Dictionary<string, AudioClip> _dicSfx = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> _dicMusic = new Dictionary<string, AudioClip>();

    
    public AudioSource AudioMusic { get { return _audioMusic; } }
    public AudioSource AudioSfx { get { return _audioSfx; } }

    public float CurrMaxsterVolume {get; private set;} 
    
    private void Start()
    {
        LocalSFXLoad();
        LocalMusicLoad();
    }
    /// <summary>
    /// 0 : Master(All)
    /// 1 : Music 
    /// 2 : Sfx
    /// _Volume = 0~1
    /// </summary>
    public int SetAudioVolume(int _Type, float _Volume)
    {
        switch(_Type)
        {
            case 0:
                // 선형 값을 로그 데시벨(-80~0) 스케일로 변환
                float volumeInDB = Mathf.Log10(_Volume) * 20;

                // 0에 가까운 값은 무한대로 발산하므로 최솟값(-80 dB)으로 설정
                if (_Volume <= 0.0001f)
                {
                    volumeInDB = -80f;
                }
                _masterMixer.SetFloat("MasterVolume", volumeInDB);
                CurrMaxsterVolume = _Volume;
                //_masterMixer.audioMixer.
                break;
            case 1:
                _audioMusic.volume = _Volume;
                break;
            case 2:
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
        _audioMusic.Play();
        //WhirlwindOfJoy
    }
    public void StopMusic()
    {
        _audioMusic.Stop();
        _audioMusic.clip = null;
    }

}
