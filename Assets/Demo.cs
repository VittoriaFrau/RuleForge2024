using System;
using System.Collections;
using System.Collections.Generic;
using ECAPrototyping.RuleEngine;
using TMPro;
using UI;
using UnityEngine;

public class Demo : MonoBehaviour
{

    RuleEngine ruleEngine;
    void Start()
    {
        ruleEngine = RuleEngine.GetInstance();
    }
    // Start is called before the first frame update
    [ContextMenu("CubeMoves")]
    public void CubeMoves()
    {
        GeneralUIController.Instance.UIstate = GeneralUIController.UIState.Play;
        ruleEngine.ExecuteAction(new ECAPrototyping.RuleEngine.Action(GameObject.Find("newCube"), "moves near", GameObject.Find("Sphere1")));
    }

    [ContextMenu("DuplicateCube")]
    public void DuplicateCube()
    {
        ruleEngine.ExecuteAction(new ECAPrototyping.RuleEngine.Action(GameObject.Find("Cube1"), "is duplicated", 1));
    }

    [ContextMenu("Speed")]
    public void Speed()
    {

        ruleEngine.ExecuteAction(new ECAPrototyping.RuleEngine.Action(GameObject.Find("Belt1"), "decreases speed"));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            TextMeshPro textMeshPro = GameObject.Find("Counter1").GetComponent<TextMeshPro>();
            int counterValue = Convert.ToInt32(textMeshPro.text);
            textMeshPro.text = (counterValue + 1).ToString();
            //GeneralUIController.Instance.CombineRulesController.manualOppositeActions.Add(new ECAPrototyping.RuleEngine.Action(gameObject, "resets"));
            GeneralUIController.Instance.CombineRulesController.manualOppositeActions.Add(new ECAPrototyping.RuleEngine.Action(GameObject.Find("Counter1"), "resets counter"));
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            TextMeshPro textMeshPro = GameObject.Find("Counter1").GetComponent<TextMeshPro>();
            int counterValue = Convert.ToInt32(textMeshPro.text);
            textMeshPro.text = (counterValue - 1).ToString();
            GeneralUIController.Instance.CombineRulesController.manualOppositeActions.Add(new ECAPrototyping.RuleEngine.Action(GameObject.Find("Counter1"), "resets counter"));
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Speed();
        }
    }

}
