using BoardGame.Script.Events;
using UnityEngine;
using UnityEngine.Video;

public class VideoPlayersManager : MonoBehaviour
{
    public VideoPlayer VictoryPlayer { get; set; }
    void Start()
    {
        VictoryPlayer = transform.Find("VictoryPlayer").GetComponent<VideoPlayer>();
        VictoryPlayer.enabled = false;

        EventManager.StartListening(GameObjectEventType.TileCleared, ShowVictoryClip);
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
        player.enabled = true;
        player.Play();
        player.loopPointReached += StopPlayer;
    }

    private void StopPlayer(VideoPlayer player)
    {
        player.loopPointReached -= StopPlayer;
        player.enabled = false;
    }
}
