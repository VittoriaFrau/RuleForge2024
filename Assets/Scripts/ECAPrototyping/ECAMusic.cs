using System;
using System.Collections;
using System.Security.Cryptography;
using ECAPrototyping.Utils;
using UnityEngine;

namespace ECAPrototyping.RuleEngine
{
    /// <summary>
    /// A <b>musicDevice</b> is a type of ECAObject, it represents a piano, a guitar or a vintage radio.
    /// It can be controlled by the player.
    /// </summary>
    [DisallowMultipleComponent]
    [ECARules4All("musicDevice")]
    [RequireComponent(typeof(ECAObject))]

    public class ECAMusic : MonoBehaviour
    {
        
        /// <summary>
        /// <b>isPlaying</b> is a boolean that indicates if the object is reproducing music.
        /// </summary>
        [StateVariable("mode", ECARules4AllType.Boolean)] 
        public ECABoolean isPlaying = new ECABoolean(ECABoolean.BoolType.OFF);

        private AudioSource _audioSource;
        private bool musicPaused = true;
        private void Start()
        {
            _audioSource = gameObject.GetComponent<AudioSource>();
        }

        /// <summary>
        /// <b>TurnON</b> turns on or off the audio source of the selected object. 
        /// </summary>
        [Action(typeof(ECAMusic), "turns", typeof(ECABoolean))]
        public void Turn(ECABoolean isPlaying)
        {
            musicPaused = !musicPaused;

            if(musicPaused){
                _audioSource.Pause();
            } else{
                _audioSource.Play();
            }
        }

        /// <summary>
        /// <b>Volume</b> . 
        /// </summary>
        [Action(typeof(ECAMusic), "changes", "volume", "to", typeof(float))]
        public void ChangeVolume(float volume)
        {
            _audioSource.volume = volume;
        }
        

    }
}