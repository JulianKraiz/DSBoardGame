using BoardGame.Script.Events;
using UnityEngine;

public class SoulCacheManager : MonoBehaviour
{
    public int SoulCounter { get; set; }

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
        SoulCounter = 0;

        soulCountText = GameObject.Find("SoulCountText").GetComponent<TextMesh>();
        soulCountGainText = GameObject.Find("SoulAddedText").GetComponent<TextMesh>();
        gameState = GameObject.Find("GameStateManager").GetComponent<GameStateManager>();

        anchorTo = GameObject.Find("SoulGainAnchorTo").transform.localPosition;
        anchorFrom = GameObject.Find("SoulGainAnchorFrom").transform.localPosition;
        timeToReachTarget = 3;

        soulCountText.text = string.Empty;
        
        EventManager.StartListeningObject(EventTypes.AddSoulsToCache, AddOrRemoveSouls);
        EventManager.StartListeningGameObject(EventTypes.TileCleared, CountAndAddSoulsGained);
    }

    private void AddOrRemoveSouls(object countObject)
    {
        SoulCounter += (int)countObject;
    }

    private void CountAndAddSoulsGained(GameObject tile)
    {
        var tileManager = tile.GetComponent<TileManager>();
        var gain = tileManager.monsterSettings.soulGainPerPlayer * tileManager.players.Count;
        gain += tileManager.monsterSettings.soulGainBonus;
        
        AddOrRemoveSouls(gain);
        soulCountGainText.text = $"+ {gain}";
        soulCountGainText.gameObject.SetActive(true);
        soulCountGainText.transform.localPosition = anchorFrom;

        Invoke(nameof(InitiateMoveTextAndMerge), 10);
        
    }

    void Update()
    {
        soulCountText.text = $"{SoulCounter} Souls";

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
    }

    private void HideGainText()
    {
        moving = false;
        soulCountGainText.gameObject.SetActive(false);
    }


}
