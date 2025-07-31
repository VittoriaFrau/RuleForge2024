using UnityEngine;

namespace UI
{
    public class Prototypation: MonoBehaviour
    {
        public void ShowEditMenu()
        {
            GeneralUIController.Instance.SetSelectedObject(gameObject);
            GeneralUIController.Instance.EditModeState();
        }

    }
}