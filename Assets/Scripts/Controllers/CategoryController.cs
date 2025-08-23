using System;
using System.Collections.Generic;
using System.Linq;
using MixedReality.Toolkit.UX;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers
{
    public class CategoryController : MonoBehaviour
    {
        public enum CategoryObjectSelected
        {
            SingleObject,
            Category,
            AllObjects
        }

        public GameObject CategoryMenu;
        public TextMeshProUGUI SingleObjectLabel;
        public TextMeshProUGUI CategoryLabel;
        public FontIconSelector CategoryIcon;
        public GameObject SingleObjectButton;
        public GameObject CategoryButton;
        public GameObject AllObjectsButton;

        private List<GameObject> buttons;
        public CategoryObjectSelected categoryObjectSelected;

        private void Awake()
        {
            buttons = new List<GameObject> { SingleObjectButton, CategoryButton, AllObjectsButton };
        }

        private void Start()
        {
            HideCategoryMenu();
            ResetCategory();
        }

        public void ShowCategoryMenu()
        {
            categoryObjectSelected = CategoryObjectSelected.SingleObject;
            CategoryMenu.SetActive(true);
            HandleCategoryButtons(SingleObjectButton);
        }

        public void HideCategoryMenu()
        {
            CategoryMenu.SetActive(false);
            ResetCategory();
        }

        public void CustomizeCategoryMenu(GameObject gameObject)
        {
            string objectCategory = Utils.GetECALastScriptFromECAObject(gameObject);

            GeneralUIController.Instance.SetDebugText(
                $"Are you selecting the {gameObject.name}, any {objectCategory}, or any object?"
            );

            ShowCategoryMenu();
            CategoryLabel.text = objectCategory;
            SingleObjectLabel.text = gameObject.name;

            CategoryIcon.CurrentIconName = Utils.GetIconForECACategory(objectCategory) ?? CategoryIcon.CurrentIconName;
        }

        public void SetCategory(string category)
        {
            if (Enum.TryParse(category, out CategoryObjectSelected categorySelected))
            {
                categoryObjectSelected = categorySelected;
                HandleCategoryButtons(buttons.FirstOrDefault(b => b.name.Equals(category)));
                Debug.Log($"Category set to: {categoryObjectSelected}");
                if (GeneralUIController.Instance.isRecording)
                {
                    if (GeneralUIController.Instance.InteractionCreationController.Modality == InteractionCreationController.Modalities.Proximity)
                    {
                        GeneralUIController.Instance.recordedEvents
                        .Where(e => e.Modality == GeneralUIController.Instance.InteractionCreationController.Modality)
                        .ToList()
                        .ForEach(e => e.ChangeObjectCategory(categoryObjectSelected));
                    }
                    else
                    {
                        GeneralUIController.Instance.recordedEvents
                        .Where(e => e.Modality == GeneralUIController.Instance.InteractionCreationController.Modality)
                        .ToList()
                        .ForEach(e => e.ChangeSubjectCategory(categoryObjectSelected));
                    }
                }
            }
        }

        private void HandleCategoryButtons(GameObject buttonPressed)
        {
            buttons.ForEach(button => Utils.TogglePressableButton(button != buttonPressed, button));
        }

        public void ResetCategory()
        {
            categoryObjectSelected = CategoryObjectSelected.SingleObject;
            HandleCategoryButtons(SingleObjectButton);
        }
    }
}
