using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public DeathUI deathUI;
    public ClearUI clearUI; // 추가
    public MapManager mapManager;
    public PlayerController playerController;
    public ScreenFader fader;
    public GameObject faderObject;
    private float _waitTime = 0.5f;

    private void Awake()
    {
        Instance = this;
    }
    
    public void ShowDeathUI()
    {
         deathUI.Show();
    }
    public void ShowClearUI() => clearUI.Show(); // 클리어 UI 띄우기

    public void HandleRespawn()
    {
        StartCoroutine(RespawnToTownRoutine());
    }

    void ResetUI()
    {
        deathUI.Hide();
        clearUI.Hide(); // 둘 다 꺼주기
    }
    private IEnumerator RespawnToTownRoutine()
    {
        faderObject.SetActive(true);
        yield return StartCoroutine(fader.FadeOut());

        ResetUI();
        yield return StartCoroutine(mapManager.ResetToTown());
        playerController.ResetPlayerState();
        playerController.isControllable = false;
        ObjectPooler.ReturnAllActiveObjects();

        yield return StartCoroutine(fader.FadeIn());
        playerController.isControllable = true;
        faderObject.SetActive(false);
    }
}
