using Assets.Scripts.ActiveUnitDisplay;
using Assets.Scripts.Unit;
using BoardGame.Script.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class AttacksRadialDisplayContainer : MonoBehaviour
{
    private Vector3 positionOffset;
    private Vector3 displayScale;

    private GameObject currentUnit;

    private Transform RightAttack1;
    private Transform RightAttack2;
    private Transform RightAttack3;
    private Transform LeftAttack1;
    private Transform LeftAttack2;
    private Transform LeftAttack3;

    public GameObject RadialDisplayTemplate;

    private GameObject RightAttack1Display;
    private GameObject RightAttack2Display;
    private GameObject RightAttack3Display;
    private GameObject LeftAttack1Display;
    private GameObject LeftAttack2Display;
    private GameObject LeftAttack3Display;

    private GameObject RightAttack1Background;
    private GameObject RightAttack2Background;
    private GameObject RightAttack3Background;
    private GameObject LeftAttack1Background;
    private GameObject LeftAttack2Background;
    private GameObject LeftAttack3Background;

    private GameObject RightAttack1BackgroundHover;
    private GameObject RightAttack2BackgroundHover;
    private GameObject RightAttack3BackgroundHover;
    private GameObject LeftAttack1BackgroundHover;
    private GameObject LeftAttack2BackgroundHover;
    private GameObject LeftAttack3BackgroundHover;

    void Start()
    {
        positionOffset = new Vector3(0, 1, 0);
        displayScale = new Vector3(0.2f, 0.2f, 0.2f);
        EventManager.StartListeningGameObject(EventTypes.ActiveUnitSelected, ToggleRadialMenu);
        EventManager.StartListeningGameObject(EventTypes.UnitIsActivated, SetupUnitAttackPanels);
        EventManager.StartListeningGameObject(EventTypes.CloseAttackDial, HideRadialMenu);

        RightAttack1 = transform.Find("Attack1Position");
        RightAttack2 = transform.Find("Attack2Position");
        RightAttack3 = transform.Find("Attack3Position");
        LeftAttack1 = transform.Find("Attack4Position");
        LeftAttack2 = transform.Find("Attack5Position");
        LeftAttack3 = transform.Find("Attack6Position");

        RightAttack1Background = transform.Find("radial_top_right").gameObject;
        RightAttack2Background = transform.Find("radial_middle_right").gameObject;
        RightAttack3Background = transform.Find("radial_bottom_right").gameObject;
        LeftAttack1Background = transform.Find("radial_top_left").gameObject;
        LeftAttack2Background = transform.Find("radial_middle_left").gameObject;
        LeftAttack3Background = transform.Find("radial_bottom_left").gameObject;

        RightAttack1BackgroundHover = transform.Find("radial_top_right_hover").gameObject;
        RightAttack2BackgroundHover = transform.Find("radial_middle_right_hover").gameObject;
        RightAttack3BackgroundHover = transform.Find("radial_bottom_right_hover").gameObject;
        LeftAttack1BackgroundHover = transform.Find("radial_top_left_hover").gameObject;
        LeftAttack2BackgroundHover = transform.Find("radial_middle_left_hover").gameObject;
        LeftAttack3BackgroundHover = transform.Find("radial_bottom_left_hover").gameObject;
    }

    void Update()
    {
        if(currentUnit != null)
        {
            CenterOnUnit(currentUnit);
        }
    }

    private void SetupUnitAttackPanels(GameObject unit)
    {
        gameObject.SetActive(false);
        ClearAllDisplay();
        currentUnit = unit;

        var basicProperties = currentUnit.GetComponent<UnitBasicProperties>();
        if (basicProperties.leftEquipement != null)
        {
            var index = 0;
            var equipementProperties = basicProperties.leftEquipement.GetComponent<EquipementProperties>();
            foreach (var attack in equipementProperties.attackList)
            {
                var display = Instantiate(RadialDisplayTemplate, transform) as GameObject;
                display.transform.localScale = displayScale;
                SetLeftAttack(display, attack, basicProperties, index);
                index++;
            }

            index = 0;
            equipementProperties = basicProperties.rightEquipement.GetComponent<EquipementProperties>();
            foreach (var attack in equipementProperties.attackList)
            {
                var display = Instantiate(RadialDisplayTemplate, transform) as GameObject;
                display.transform.localScale = displayScale;
                SetRightAttack(display, attack, basicProperties, index);
                index++;
            }
        }
    }

    private void ToggleRadialMenu(GameObject unit)
    {
        CenterOnUnit(unit);
        gameObject.SetActive(!gameObject.activeSelf);
    }

    private void HideRadialMenu(GameObject _)
    {
        gameObject.SetActive(false);
    }

    private void CenterOnUnit(GameObject unit)
    {
        transform.position = unit.transform.position + positionOffset;
    }

    private void SetLeftAttack(GameObject display, AttackDetail attack, UnitBasicProperties unitProperties, int index)
    {
        var displayBehavior = display.GetComponent<AttackRadialDisplayBehavior>();
        
        if (index == 0)
        {
            displayBehavior.Initialize(LeftAttack1, LeftAttack1Background, LeftAttack1BackgroundHover);
            LeftAttack1Display = display;
        }
        else if (index == 1)
        {
            displayBehavior.Initialize(LeftAttack2, LeftAttack2Background, LeftAttack2BackgroundHover);
            LeftAttack2Display = display;
        }
        else if (index == 2)
        {
            displayBehavior.Initialize(LeftAttack3, LeftAttack3Background, LeftAttack3BackgroundHover);
            LeftAttack3Display = display;
        }

        displayBehavior.SetupTile(attack, unitProperties);
    }

    private void SetRightAttack(GameObject display, AttackDetail attack, UnitBasicProperties unitProperties, int index)
    {
        var displayBehavior = display.GetComponent<AttackRadialDisplayBehavior>();
        if (index == 0)
        {
            displayBehavior.Initialize(RightAttack1, RightAttack1Background, RightAttack1BackgroundHover);
            RightAttack1Display = display;
        }
        else if (index == 1)
        {
            displayBehavior.Initialize(RightAttack2, RightAttack2Background, RightAttack2BackgroundHover);
            RightAttack2Display = display;
        }
        else if (index == 2)
        {
            displayBehavior.Initialize(RightAttack3, RightAttack3Background, RightAttack3BackgroundHover);
            RightAttack3Display = display;
        }

        displayBehavior.SetupTile(attack, unitProperties);
    }

    private void ClearAllDisplay()
    {
        RightAttack1Background.SetActive(false);
        RightAttack1BackgroundHover.SetActive(false);
        RightAttack2Background.SetActive(false);
        RightAttack2BackgroundHover.SetActive(false);
        RightAttack3Background.SetActive(false);
        RightAttack3BackgroundHover.SetActive(false);
        LeftAttack1Background.SetActive(false);
        LeftAttack1BackgroundHover.SetActive(false);
        LeftAttack2Background.SetActive(false);
        LeftAttack2BackgroundHover.SetActive(false);
        LeftAttack3Background.SetActive(false);
        LeftAttack3BackgroundHover.SetActive(false);

        if (RightAttack1Display != null)
        {
            Destroy(RightAttack1Display);
        }
        if (RightAttack2Display != null)
        {
            Destroy(RightAttack2Display);
        }
        if (RightAttack3Display != null)
        {
            Destroy(RightAttack3Display);
        }
        if (LeftAttack1Display != null)
        {
            Destroy(LeftAttack1Display);
        }
        if (LeftAttack2Display != null)
        {
            Destroy(LeftAttack2Display);
        }
        if (LeftAttack3Display != null)
        {
            Destroy(LeftAttack3Display);
        }
    }
}
