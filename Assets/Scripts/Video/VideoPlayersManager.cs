using BoardGame.Script.Events;
using System;
using UnityEngine;
using UnityEngine.Video;

public class VideoPlayersManager : MonoBehaviour
{
    public VideoPlayer VictoryPlayer;
    public VideoPlayer YouDiedPlayer;
    public GameObject OcclusionPlane;

    void Start()
    {
        EventManager.StartListening(GameObjectEventType.TileCleared, ShowVictoryClip);
        EventManager.StartListening(GameObjectEventType.PlayerUnitKilled, ShowYouDiedClip);
    }

    private void ShowYouDiedClip(GameObject arg0)
    {
        StartPlayer(YouDiedPlayer);
    }

    private void ShowVictoryClip(GameObject _)
    {
        StartPlayer(VictoryPlayer);
    }

    void Update()
    {
    }

    private void StartPlayer(VideoPlayer player)
    {
        OcclusionPlane.SetActive(true);
        player.enabled = true;
        player.Play();
        player.loopPointReached += StopPlayer;
    }

    private void StopPlayer(VideoPlayer player)
    {
        OcclusionPlane.SetActive(false);
        player.loopPointReached -= StopPlayer;
        player.enabled = false;
    }
}
