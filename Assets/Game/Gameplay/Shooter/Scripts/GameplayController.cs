using UnityEngine;

public class GameplayController : MonoBehaviour
{
    [Header("Main Settings")]
    [SerializeField] private AudioEvent musicEvent = null;
    [SerializeField] private Camera mainCamera = null;
    [SerializeField] private GameplayUI gameplayUI = null;
    [SerializeField] private AudioEvent defeatEvent = null;

    [Header("Controllers Settings")]
    [SerializeField] private PlayerController playerController = null;
    [SerializeField] private WaveController waveController = null;
    [SerializeField] private EnemyPoolController enemyPoolController = null;
    [SerializeField] private ConsumablePoolController consumablePoolController = null;
    [SerializeField] private RuneController runeController = null;
    [SerializeField] private WinAnimationController winAnimationController = null;

    public void Start()
    {
        GameManager.Instance.AudioManager.PlayAudio(musicEvent);

        StartControllers();
        gameplayUI.Init(
            onEnablePlayerInput: () =>
            {
                playerController.EnableInput(true);
                GameManager.Instance.AudioManager.ToggleMusic(true);
            });

        GameManager.Instance.LoadingManager.SetFinishTransitionCallback(
            callback: () =>
            {
                playerController.EnableInput(true);
                waveController.StartWave();
            });
    }

    private void StartControllers()
    {
        playerController.Init(gameplayUI.UpdatePlayerHealth, PlayerDefeat, onPause: () => gameplayUI.TogglePause(true));
        runeController.Init(gameplayUI.UpdateRuneHealth, PlayerDefeat);
        waveController.Init(enemyPoolController, PlayerWin, gameplayUI.UpdateWave);
        consumablePoolController.Init();
        enemyPoolController.Init(runeController.transform, mainCamera,
            onKillEnemy: (position) =>
            {
                waveController.OnKillEnemy();
                consumablePoolController.TryDropConsumable(position);
            });
    }

    private void PlayerDefeat()
    {
        gameplayUI.OpenLosePanel();
        playerController.PlayerDefeat();
        waveController.StopWave();
        enemyPoolController.EnemyList.ForEach(e => e.SetWinState());

        GameManager.Instance.AudioManager.StopCurrentMusic(
            onSuccess: () =>
            {
                GameManager.Instance.AudioManager.PlayAudio(defeatEvent);
            });
    }

    private void PlayerWin()
    {
        playerController.EnableInputOnlyUI();

        winAnimationController.PlayWinAnimation(runeController.transform.position, gameplayUI.OpenWinPanel);
    }
}
