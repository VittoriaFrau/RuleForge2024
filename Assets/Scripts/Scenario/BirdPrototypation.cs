using ECAPrototyping.RuleEngine;
using MixedReality.Toolkit.SpatialManipulation;
using TMPro;
using UI;
using UnityEngine;

namespace Scenario
{
    public class BirdPrototypation: MonoBehaviour
    {

        private GameObject bird;
        public GameObject editMenu;
        private TextMeshProUGUI debugText;
        private EditModeController _editModeController;
        private InteractionCreationController _interactionCreationController;
        private GameObject eventHandler;
        private GeneralUIController generalUIController;

        public void ActivateBirdPrototypation()
        {
            eventHandler = GameObject.FindGameObjectWithTag("EventHandler");
            generalUIController = eventHandler.GetComponent<GeneralUIController>();
            
            debugText = GameObject.FindGameObjectWithTag("debugText").GetComponent<TextMeshProUGUI>();
            bird = GameObject.FindGameObjectsWithTag("Bird")[0];
            
            Action action = new Action(bird, "hides");
            generalUIController.InteractionCreationController.RecordActionPressedButton(action, bird);
            
            ObjectManipulator objectManipulator = bird.GetComponent<ObjectManipulator>();
            objectManipulator.OnClicked.AddListener(ShowBirdEditMenu);
            bird.GetComponent<Rigidbody>().useGravity = true;
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