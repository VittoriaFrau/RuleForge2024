using UnityEngine;

namespace UI
{
    public class Prototypation: MonoBehaviour
    {
        private GameObject eventHandler;
        
        private void Start()
        {
            eventHandler = GameObject.Find("EventHandler");
        }
        
        public void ShowEditMenu()
        {
            GeneralUIController.Instance.SetSelectedObject(gameObject);
            GeneralUIController.Instance.EditModeState();
        }
        
    }
}