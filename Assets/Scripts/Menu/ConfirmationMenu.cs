using Assets.Scripts.Menu.Model;
using BoardGame.Script.Events;
using System;
using UnityEngine;

public class ConfirmationMenu : MonoBehaviour
{
    public TextMesh descriptionMesh;
    public MeshRenderer yesBackground;
    public MeshRenderer noBackground;
    public GameObject background;
    public GameObject occlusionPlane;

    private Action actionToExecute;

    void Start()
    {
        occlusionPlane.SetActive(false);

        yesBackground.enabled = false;
        yesBackground.GetComponent<RaiseEventOnEnterExit>().PositionEnter += ShowYesBackground;
        yesBackground.GetComponent<RaiseEventOnEnterExit>().PositionExit += HideYesBackground;
        yesBackground.GetComponent<RaiseEventOnClicked>().PositionClicked += ExecuteAction;

        noBackground.enabled = false;
        noBackground.GetComponent<RaiseEventOnEnterExit>().PositionEnter += ShowNoBackground;
        noBackground.GetComponent<RaiseEventOnEnterExit>().PositionExit += HideNoBackground;
        noBackground.GetComponent<RaiseEventOnClicked>().PositionClicked += Cancel;

        background.SetActive(false);

        EventManager.StartListening(ObjectEventType.GetActionConfirmation, SetupFromEvent);
    }

    private void Cancel(GameObject position)
    {
        occlusionPlane.SetActive(false);
        actionToExecute = null;
        background.SetActive(false);
        descriptionMesh.text = "";
    }

    private void ExecuteAction(GameObject position)
    {
        occlusionPlane.SetActive(false);
        background.SetActive(false);
        descriptionMesh.text = "";
        actionToExecute();
        actionToExecute = null;

    }

    private void HideNoBackground(GameObject position)
    {
        noBackground.enabled = false;
    }

    private void ShowNoBackground(GameObject position)
    {
        noBackground.enabled = true;
        yesBackground.enabled = false;
    }

    private void HideYesBackground(GameObject position)
    {
        yesBackground.enabled = false;
    }

    private void ShowYesBackground(GameObject position)
    {
        yesBackground.enabled = true;
        noBackground.enabled = false;
    }

    void Update()
    {

    }

    public void SetupFromEvent(object load)
    {
        var config = (ConfirmationMenuLoad)load;
        SetupConfirmation(config);
    }

    public void SetupConfirmation(ConfirmationMenuLoad configuration)
    {
        occlusionPlane.SetActive(true);
        noBackground.enabled = false;
        yesBackground.enabled = false;
        background.SetActive(true);
        descriptionMesh.text = configuration.Description;
        actionToExecute = configuration.Action;
    }
}
