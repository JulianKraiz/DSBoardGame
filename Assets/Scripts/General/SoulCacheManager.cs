using Assets.Scripts.Tile;
using BoardGame.Script.Events;
using UnityEngine;

public class SoulCacheManager : MonoBehaviour
{
    public int SoulCounter;
    public int SoulWaitingToBeAdded;

    private GameStateManager gameState;
    private TextMesh soulCountText;
    private TextMesh soulCountGainText;

    float t;
    Vector3 startPosition;
    Vector3 anchorTo;
    Vector3 anchorFrom;
    float timeToReachTarget;
    bool moving;

    void Start()
    {
        soulCountText = GameObject.Find("SoulCountText").GetComponent<TextMesh>();
        soulCountGainText = GameObject.Find("SoulAddedText").GetComponent<TextMesh>();
        gameState = GameObject.Find("GameStateManager").GetComponent<GameStateManager>();

        anchorTo = GameObject.Find("SoulGainAnchorTo").transform.localPosition;
        anchorFrom = GameObject.Find("SoulGainAnchorFrom").transform.localPosition;
        timeToReachTarget = 3;

        soulCountText.text = string.Empty;

        EventManager.StartListening(ObjectEventType.AddSoulsToCache, AddOrRemoveSouls);
    }

    private void AddOrRemoveSouls(object countObject)
    {
        var soulsToAdd = (int)countObject;
        SoulWaitingToBeAdded += soulsToAdd;
        soulCountGainText.text = $"{((soulsToAdd > 0) ? "+" : "")} {soulsToAdd}";
        soulCountGainText.color = new Color(1, 1, 1, 1);
        soulCountGainText.gameObject.SetActive(true);
        soulCountGainText.transform.localPosition = anchorFrom;

        Invoke(nameof(InitiateMoveTextAndMerge), 10);
    }

    void Update()
    {
        soulCountText.text = $"{gameState.soulCache} Souls";

        if (moving)
        {
            t += Time.deltaTime / timeToReachTarget;
            soulCountGainText.transform.localPosition = Vector3.Lerp(anchorFrom, anchorTo, t);
            soulCountGainText.color = new Color(1, 1, 1, 1 - t);
        }
    }



    private void InitiateMoveTextAndMerge()
    {
        t = 0;
        moving = true;
        Invoke(nameof(HideGainText), timeToReachTarget);

        gameState.soulCache += SoulWaitingToBeAdded;
        SoulWaitingToBeAdded = 0;
    }

    private void HideGainText()
    {
        moving = false;
        soulCountGainText.gameObject.SetActive(false);
    }
}
