using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("# Game Control")]
    public bool isLive;
    public float gameTime;
    public float maxGameTime = 2 * 10f;

    [Header("# Player Info")]
    public int playerId;
    public float health;
    public float maxHealth = 100;
    public int level;
    public int kill;
    public int exp;
    public int[] nextExp = { 3, 5, 10, 100, 150, 210, 280, 360, 450, 600 };
    static public Dictionary<int, float> upgrades;

    [Header("# Game Object")]
    public Player player;
    public PoolManager pool;
    public LevelUp uiLevelUp;
    public Result uiResult;
    public Transform uiJoy;
    public Transform dashButton;
    public GameObject enemyCleaner;
    public GameObject PauseScene;
    public GameObject HUD;
    public GameObject PoolManager;

    void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
    }

    public void GameStart(int id)
    {
        upgrades = new Dictionary<int, float>();
        playerId = id;
        health = maxHealth;

        if (EffectManager.Instance != null)
        {
            EffectManager.Instance.Reset();
        }

        // 게임 시작
        player.gameObject.SetActive(true);
        Resume();
        AudioManager.instance.PlayBgm(true);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
    }

    public void GameOver()
    {
        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        isLive = false;

        yield return new WaitForSeconds(0.5f);

        uiResult.gameObject.SetActive(true);
        uiResult.Lose();
        Stop();

        AudioManager.instance.PlayBgm(false);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Lose);
    }

    public void GameVictory()
    {
        StartCoroutine(GameVictoryRoutine());
    }

    IEnumerator GameVictoryRoutine()
    {
        isLive = false;
        enemyCleaner.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        uiResult.gameObject.SetActive(true);
        uiResult.Win();
        Stop();

        AudioManager.instance.PlayBgm(false);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Win);
    }

    public void GameRetry()
    {
        SceneManager.LoadScene(0);
        PauseScene.SetActive(false);
    }

    public void GameQuit()
    {
        Application.Quit();
        PauseScene.SetActive(false);
    }

    void Update()
    {
        if (!isLive)
            return;

        gameTime += Time.deltaTime;

        if (gameTime > maxGameTime)
        {
            gameTime = maxGameTime;
            GameVictory();
        }
    }

    public void GetExp()
    {
        if (!isLive)
            return;

        exp++;

        if (exp == nextExp[Mathf.Min(level, nextExp.Length - 1)])
        {
            level++;
            exp = 0;

            // 레벨업 UI 표시
            if (uiLevelUp != null)
            {
                uiLevelUp.Show();
            }
        }
    }

    public void Stop()
    {
        isLive = false;
        Time.timeScale = 0;
        uiJoy.localScale = Vector3.zero;
        dashButton.localScale = Vector3.zero;
    }

    public void Resume()
    {
        isLive = true;
        Time.timeScale = 1;
        uiJoy.localScale = Vector3.one;
        dashButton.localScale = Vector3.one;
        PauseScene.SetActive(false);
        HUD.SetActive(true);
    }

    public void PauseToggle()
    {
        if (!PauseScene.activeSelf)
        {
            Stop();
            PauseScene.SetActive(true);
            HUD.SetActive(false);
        }
        else
        {
            Resume();
            PauseScene.SetActive(false);
        }
    }
}