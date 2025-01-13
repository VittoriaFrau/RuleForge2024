using System;
using System.Collections;
using System.Security.Cryptography;
using ECAPrototyping.Utils;
using MixedReality.Toolkit.UX;
using UI;
using UnityEngine;

namespace ECAPrototyping.RuleEngine
{
    /// <summary>
    /// <b>ECAObject</b> is the base class for all objects that can be used in the rule engine.
    /// All the other classes in this package inherit from this class or one of its subclasses.
    /// </summary>
    [DisallowMultipleComponent]
    [ECARules4All("object")]
    public class ECAObject : MonoBehaviour
    {
        /// <summary>
        /// <b>GameRender</b> is the renderer of the object.
        /// </summary>
        private Renderer gameRenderer;
        
        /// <summary>
        /// <b> Color </b> is the color of the object 
        /// </summary>
        [StateVariable("color", ECARules4AllType.Color)] 
        public Color color;
        
        /// <summary>
        /// <b>isVisible</b> is a boolean that indicates if the object is visible.
        /// If the object is invisible, it will not be rendered but it will still collide with other objects.
        /// </summary>
        [StateVariable("visible", ECARules4AllType.Boolean)] 
        public ECABoolean isVisible = new (ECABoolean.BoolType.YES);
        
        /// <summary>
        /// <b> Gravity </b> is a boolean that indicates if the object is affected by gravity.
        /// </summary>
        [StateVariable("gravity", ECARules4AllType.Boolean)] 
        public ECABoolean isUsingGravity = new(ECABoolean.BoolType.YES);
        
        

        private void Awake()
        {
            gameRenderer = this.gameObject.GetComponent<Renderer>();
            if(gameRenderer == null)
                gameRenderer = this.gameObject.AddComponent<MeshRenderer>();
            color = gameRenderer.material.color;
        }
        

        /// <summary>
        /// <b>Shows</b> makes the object visible. It makes it visible if it is not already.
        /// </summary>
        [Action(typeof(ECAObject), "shows")]
        public void Shows()
        {
            //check if the object is already visible
            /*if (this.gameObject.activeInHierarchy)
            {
                isVisible.Assign(ECABoolean.BoolType.YES);
            }
            else
            {
                isVisible.Assign(ECABoolean.BoolType.NO);
            }*/
            isVisible.Assign(ECABoolean.BoolType.YES);
            UpdateVisibility();
        }
        
        
        /// <summary>
        /// <b>Hides</b> makes the object invisible. It makes it invisible if it is not already.
        /// </summary>
        [Action(typeof(ECAObject), "hides")]
        public void Hides()
        {
            isVisible.Assign(ECABoolean.BoolType.NO);
            UpdateVisibility();
        }
        
        
        /// <summary>
        /// <b>ShowsHides</b> sets the visibility of the object, defined by a parameter.
        /// </summary>
        /// <param name="yesNo">The new visibility state for the object. </param>
        [Action(typeof(ECAObject), "changes", "visible", "to", typeof(YesNo))]
        public void ShowsHides(ECABoolean yesNo)
        {
            isVisible = yesNo;
            UpdateVisibility();
        }
        
        private void UpdateVisibility()
        {
           
            this.gameObject.SetActive(isVisible);
        }
        
        
        /// <summary>
        /// <b>SetsColor</b> sets the color of the light source to the given value.
        /// </summary>
        /// <param name="inputColor">The new color value.</param>
        [Action(typeof(ECAObject), "changes","color", "to", typeof(ECAColor))]
        public void ChangesColor(ECAColor inputColor)
        {
            color = inputColor.Color;
            if (this.gameObject.GetComponent<Light>() != null)
            {
                this.gameObject.GetComponent<Light>().color = color;
            }
            else
            {
                gameRenderer.material.color = color;
            }
        }
            
        
        
        public void ChangeColor(string newColor)
        {
            //convert string to color
            ColorUtility.TryParseHtmlString(newColor, out color);
            gameRenderer.material.color = color;
        }
        
        /// <summary>
        /// <b>GravityON</b> sets to true the gravity.
        [Action(typeof(ECAObject), "gravityON")]
        public void GravityON()
        {
            isUsingGravity = ECABoolean.YES;
            UpdateGravity();
        }
        
        /// <summary>
        /// <b>GravityOFF</b> sets to false the gravity.
        [Action(typeof(ECAObject), "gravityOFF")]
        public void GravityOFF()
        {
            isUsingGravity = ECABoolean.NO;
            UpdateGravity();
        }

        /// <summary>
        /// <b>UpdateGravity</b> updates the gravity of the object.
        /// </summary>
        public void UpdateGravity()
        {
            switch (isUsingGravity.GetBoolType())
            {
               case ECABoolean.BoolType.YES:
                   gameObject.GetComponent<Rigidbody>().useGravity = true;
                   break;
               case ECABoolean.BoolType.NO:
                   gameObject.GetComponent<Rigidbody>().useGravity = false;
                   break;
            }

            //TEST
            if (gameObject.name.Equals("feather"))
            {
                /*Test test = GameObject.FindGameObjectWithTag("EventHandler").GetComponent<Test>();
                if (test != null)
                {
                    if (isUsingGravity.GetBoolType() == ECABoolean.BoolType.YES)
                        test.StopLeviosa();
                    else test.StartLeviosa();
                }*/
            }
        }

        /// <summary>
        /// <b>delete_object</b> deletes the object from the scene.
        /// </summary>
        /// <param name="obj"> is the object to delete from the scene </param>
        [Action(typeof(ECAObject), "deleted")]
        public void DeleteObject()
        {
            Destroy(this.gameObject);
            
        }
    }
}