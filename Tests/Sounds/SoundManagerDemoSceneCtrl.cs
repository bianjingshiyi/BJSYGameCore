using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BJSYGameCore.Tests
{
    class SoundManagerDemoSceneCtrl : MonoBehaviour
    {
        protected void Awake()
        {
            _slider.onValueChanged.AddListener(onSliderValueChangedCallback);
        }
        protected void Update()
        {
            _text.text = _soundManager.getParamValueForVolume(_masterVolumeName).ToString();
        }
        private void onSliderValueChangedCallback(float value)
        {
            _soundManager.setParamValueForVolume(_masterVolumeName, value);
        }
        [SerializeField]
        private SoundManager _soundManager;
        [SerializeField]
        private Slider _slider;
        [SerializeField]
        private Text _text;
        [SerializeField]
        private string _masterVolumeName = "MasterVolume";
    }
}