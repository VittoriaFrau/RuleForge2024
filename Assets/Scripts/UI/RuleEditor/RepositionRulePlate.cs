using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MixedReality.Toolkit.SpatialManipulation;
using TMPro;
using UI;
using UI.RuleEditor;
using UnityEngine;

public class RepositionRulePlate : MonoBehaviour
{
    public enum PlateState
    {
        Default, Moving
    }
    
    private PlateState _plateState;
    public ObjectManipulator rulePlateManipulator;
    
    private BoundsControl _boundsController;
    private RuleManager _ruleManager;
    private Vector3 originalLocalPlatePosition = new (-14.4f, -119.0f, 774.0f);
    private GameObject ruleEditorPlate;
    
    //All the buttons but the reposition plate one
    public List<GameObject> ruleEditorButtons;

    // Start is called before the first frame update
    void Start()
    {
        _plateState = PlateState.Default;
        
        _ruleManager = GameObject.FindGameObjectWithTag("EventHandler").GetComponent<RuleManager>();
        
        _ruleManager.DeactivateRuleDebugText();
        
        ruleEditorPlate = GameObject.FindGameObjectsWithTag("RuleUtils")
            .ToList().Find(x=>x.name=="RuleEditorPlate");
        _boundsController = ruleEditorPlate.GetComponent<BoundsControl>();
        if(rulePlateManipulator==null) rulePlateManipulator = ruleEditorPlate.GetComponent<ObjectManipulator>();
        
    }

    public void ResetPlateOriginalPosition()
    {
        ruleEditorPlate.transform.localPosition= originalLocalPlatePosition;
    }

    public void MovePlate()
    {
        if (_plateState == PlateState.Default)
        {
            rulePlateManipulator.enabled = true;
            _boundsController.enabled = true;
            _plateState = PlateState.Moving;
            _ruleManager.ActivateDebugTextWithMessage("Moving the plate, click again the moving button to stop moving.");
            ChangeButtonAppearence(false);
        }
        else
        {
            StopMoving();
            ChangeButtonAppearence(true);
        }
    }

    public void StopMoving()
    {
        if(_plateState == PlateState.Moving)
        {
            rulePlateManipulator.enabled = false;
            _boundsController.enabled = false;
            _plateState = PlateState.Default;

            _ruleManager.DeactivateRuleDebugText();
        }
       
    }

    public void ChangeButtonAppearence(bool active)
    {
        foreach (var button in ruleEditorButtons)
        {
            Utils.SetStatusButton(active, button);
        }
    }
}
