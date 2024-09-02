using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI.RuleEditor
{
    public class Preview : MonoBehaviour
    {
        public GameObject cubeToShow;

        public TextMeshProUGUI text;

        public RuleManager _ruleManager;

        // Start is called before the first frame update
        void Start()
        {
           
            _ruleManager.InitializeVariables();
        }

        // Update is called once per frame
        private void ChangeForPreview()
        {
            cubeToShow.gameObject.SetActive(true);
            text.text = "banana changes gravity to OFF";
        }
    }
}