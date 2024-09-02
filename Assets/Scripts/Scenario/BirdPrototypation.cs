using MixedReality.Toolkit.SpatialManipulation;
using TMPro;
using UnityEngine;

namespace Scenario
{
    public class BirdPrototypation: MonoBehaviour
    {

        private GameObject bird;
        public GameObject editMenu;
        public TextMeshPro debugText;

        public void ActivateBirdPrototypation()
        {
            bird = GameObject.FindGameObjectsWithTag("Bird")[0];
            ObjectManipulator objectManipulator = bird.GetComponent<ObjectManipulator>();
            objectManipulator.OnClicked.AddListener(ShowBirdEditMenu);
        }
        
        public void DeActivateBirdPrototypation()
        {
            bird = GameObject.FindGameObjectsWithTag("Bird")[0];
            ObjectManipulator objectManipulator = bird.GetComponent<ObjectManipulator>();
            objectManipulator.OnClicked.RemoveAllListeners();
        }

        public void ShowBirdEditMenu()
        {
            editMenu.SetActive(true);
            debugText.text = "You are modifying the bird properties";
        }

        public void HideBird()
        {
            bird.SetActive(false);
        }

        public void ShowBird()
        {
            bird.SetActive(true);
        }
    }
}