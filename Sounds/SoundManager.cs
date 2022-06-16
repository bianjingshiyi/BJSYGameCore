using UnityEngine;
using UnityEngine.Audio;
using System;
using System.Collections.Generic;

namespace BJSYGameCore
{
    public class SoundManager : MonoBehaviour
    {
        #region 公有方法
        public void playSound(AudioSource audioSource, AudioClip clip, bool loop = false, float fadeTime = 0, Action onCompleted = null)
        {
            if (audioSource == null)
                throw new ArgumentNullException(nameof(audioSource));
            if (clip == null)
                throw new ArgumentNullException(nameof(clip));
            if (fadeTime > 0)
            {
                if (audioSource.isPlaying)
                {
                    //fade out if playing
                    fadeAudioSourceVolume(audioSource, linear1to0in1s.getCopy(fadeTime), () =>
                    {
                        audioSource.Stop();
                        audioSource.clip = clip;
                        audioSource.loop = loop;
                        audioSource.Play();
                        _playList.Add(new PlayInfo()
                        {
                            audioSource = audioSource,
                            onCompleted = onCompleted
                        });
                        //fade in
                        fadeAudioSourceVolume(audioSource, linear0to1in1s.getCopy(fadeTime));
                    });
                }
                else
                {
                    audioSource.Stop();
                    audioSource.clip = clip;
                    audioSource.loop = loop;
                    audioSource.Play();
                    _playList.Add(new PlayInfo()
                    {
                        audioSource = audioSource,
                        onCompleted = onCompleted
                    });
                    //fade in
                    fadeAudioSourceVolume(audioSource, linear0to1in1s.getCopy(fadeTime));
                }
            }
            else
            {
                audioSource.Stop();
                audioSource.clip = clip;
                audioSource.loop = loop;
                audioSource.Play();
                _playList.Add(new PlayInfo()
                {
                    audioSource = audioSource,
                    onCompleted = onCompleted
                });
            }
        }
        public AudioSource playSoundAt(Vector3 position, Transform parent, AudioMixerGroup group, AudioClip clip, bool loop = false, float fadeTime = 0, Action<AudioSource> onCompleted = null)
        {
            AudioSource audioSource = Instantiate(_audioSourcePrefab, position, Quaternion.identity, parent);
            audioSource.outputAudioMixerGroup = group;
            if (onCompleted != null)
                playSound(audioSource, clip, loop, fadeTime, () => onCompleted.Invoke(audioSource));
            else
                playSound(audioSource, clip, loop, fadeTime, null);
            return audioSource;
        }
        public void pauseSound(AudioSource audioSource, float fadeTime = 0)
        {
            if (fadeTime > 0)
            {
                fadeAudioSourceVolume(audioSource, linear1to0in1s.getCopy(fadeTime), () =>
                {
                    audioSource.Pause();
                });
            }
            else
            {
                audioSource.Pause();
            }
        }
        public void unPauseSound(AudioSource audioSource, float fadeTime = 0)
        {
            audioSource.UnPause();
            if (fadeTime > 0)
                fadeAudioSourceVolume(audioSource, linear0to1in1s.getCopy(fadeTime));
        }
        public void stopSound(AudioSource audioSource, float fadeTime = 0)
        {
            if (fadeTime > 0)
            {
                fadeAudioSourceVolume(audioSource, linear1to0in1s.getCopy(fadeTime), () =>
                {
                    audioSource.Stop();
                });
            }
            else
                audioSource.Stop();
        }
        public void setParamValueForVolume(string paramName, float value)
        {
            _audioMixer.SetFloat(paramName, Mathf.Log10(value) * 20);
        }
        public float getParamValueForVolume(string paramName)
        {
            if (_audioMixer.GetFloat(paramName, out float value))
            {
                value = Mathf.Pow(10, value / 20);
                return value;
            }
            return 0;
        }
        public AudioMixerGroup[] getAllAudioMixerGroups()
        {
            return _audioMixer.FindMatchingGroups(string.Empty);
        }
        public void fadeAudioSourceVolume(AudioSource audioSource, AnimationCurve curve, Action onCompleted = null)
        {
            _fadeList.Add(new FadeInfo()
            {
                audioSource = audioSource,
                curve = curve,
                onCompleted = onCompleted,
                startTime = Time.time
            });
        }
        #endregion
        #region 私有方法
        #region 生命周期
        protected void Update()
        {
            for (int i = 0; i < _fadeList.Count; i++)
            {
                FadeInfo fade = _fadeList[i];
                if (fade == null || fade.audioSource == null)
                {
                    _fadeList.RemoveAt(i);
                    i--;
                    continue;
                }
                else
                {
                    Keyframe lastFrame = fade.curve.keys[fade.curve.keys.Length - 1];
                    if (Time.time >= fade.startTime + lastFrame.time)
                    {
                        fade.audioSource.volume = lastFrame.value;
                        fade.onCompleted?.Invoke();
                        _fadeList.RemoveAt(i);
                        i--;
                        continue;
                    }
                    else
                        fade.audioSource.volume = fade.curve.Evaluate(Time.time - fade.startTime);
                }
            }
            for (int i = 0; i < _playList.Count; i++)
            {
                PlayInfo play = _playList[i];
                if (play == null || play.audioSource == null)
                {
                    _playList.RemoveAt(i);
                    i--;
                    continue;
                }
                if (!play.audioSource.isPlaying)
                {
                    play.onCompleted?.Invoke();
                    _playList.RemoveAt(i);
                    i--;
                    continue;
                }
            }
        }
        #endregion
        #endregion
        #region 属性字段
        public static readonly AnimationCurve linear1to0in1s = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
        public static readonly AnimationCurve linear0to1in1s = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        [SerializeField]
        private AudioMixer _audioMixer;
        [SerializeField]
        private AudioSource _audioSourcePrefab;
        [SerializeField]
        private List<FadeInfo> _fadeList = new List<FadeInfo>();
        [SerializeField]
        private List<PlayInfo> _playList = new List<PlayInfo>();
        #endregion
        #region 嵌套类型
        [Serializable]
        class FadeInfo
        {
            public AudioSource audioSource;
            public float startTime;
            public AnimationCurve curve;
            public Action onCompleted;
        }
        [Serializable]
        class PlayInfo
        {
            public AudioSource audioSource;
            public Action onCompleted;
        }
        #endregion
    }
}
