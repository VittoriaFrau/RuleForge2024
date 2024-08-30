using System;
using System.Collections;
using System.Security.Cryptography;
using ECAPrototyping.Utils;
using UnityEngine;

namespace ECAPrototyping.RuleEngine
{
    /// <summary>
    /// A <b>Character</b> is a type of ECAObject, it represents an animal, a humanoid, a robot or a generic creature.
    /// A Character can be autonomous or controlled by the player.
    /// </summary>
    [DisallowMultipleComponent]
    [ECARules4All("character")]
    [RequireComponent(typeof(ECAObject), typeof(Animator))]

    public class ECACharacter : MonoBehaviour
    {
        /// <summary>
        /// <b>GameRender</b> is the renderer of the object.
        /// </summary>
        private Renderer[] gameRenderer;
        
        
        /// <summary>
        /// <b> Color </b> is the color of the object 
        /// </summary>
        [StateVariable("color", ECARules4AllType.Color)] 
        public Color color;
        

        private void Awake()
        {
            gameRenderer = this.gameObject.GetComponents<Renderer>();
            color = gameRenderer[0].material.color;
        }
        
        /// <summary>
        /// <b>WaveHand</b> makes the character wave its hand. It makes it wave its hand if it is not already.
        /// </summary>
        /// <param name="animator"></param>
        [Action(typeof(ECACharacter), "waves hand")]
        public void WaveHand()
        {
            Animator _animator = gameObject.GetComponent<Animator>();
            _animator.SetTrigger("wave");
        }

        /// <summary>
        /// <b>Dancing</b> makes the character dance. It makes it dance if it is not already.
        /// </summary>
        /// <param name="animator"></param>
        [Action(typeof(ECACharacter), "dances")]
        public void Dance()
        {
            Animator _animator = gameObject.GetComponent<Animator>();
            _animator.SetTrigger("dance");
            
        }

        public void ChangeColor(string newColor)
        {
            //convert string to color
            ColorUtility.TryParseHtmlString(newColor, out color);
            gameRenderer[0].material.color = color;
        }
        
    }
}