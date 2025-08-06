using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }
    public MapCycleManager cycleManager;
    public ScreenFader fader;
    public GameObject faderObject;
    public CinemachineConfiner2D cameraConfiner;
    public CinemachineConfiner2D minimapcamConfiner;

    private RewardManager _rewardManager;
    [SerializeField] private List<MapComponent> _mapInstances = new();
    [SerializeField] private List<MapComponent> _tutorialMaps = new();

    private GameObject _playerInstance;
    private int _currentIndex = -1;
    private int _tutorialIndex = -1;
    private bool _isTransitioning = false;
    private bool _inTutorial = false;
    private bool _isInitialized = false;

    [SerializeField] private float _transitionDuration;
    [SerializeField] private float _transitionSpawnDuration;
    [SerializeField] private float _loadingDuration = 1.5f;

    [SerializeField] private Transform _backgroundFollowParent;
    private GameObject _currentBackground;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
       // DontDestroyOnLoad(gameObject);
        _rewardManager = GetComponent<RewardManager>();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "InGame_Build") // 인게임 씬 이름으로 수정하세요
        {
         
        }
    }

    private void OnEnable()
    {
        InitializeMapState();
    }

    public void InitializeMapState()
    {

        _playerInstance = GameObject.FindWithTag("Player");
        if (_playerInstance == null)
        {
            Debug.LogError("[MapManager] 플레이어를 찾을 수 없습니다!");
            return;
        }

        faderObject.SetActive(true);

        var townPrefab = cycleManager.LoadTownMap();
        var townInstance = Instantiate(townPrefab);
        townInstance.SetActive(false);
        _mapInstances.Add(townInstance.GetComponent<MapComponent>());

        _currentIndex = 0;
        StartCoroutine(StartInitialMap());
    }

    private IEnumerator StartInitialMap()
    {
        yield return new WaitForSeconds(_loadingDuration);
        yield return ActivateMapRoutine(_currentIndex);
        yield return StartCoroutine(fader.FadeIn());
        faderObject.SetActive(false);
    }

    public void GoToNextMap()
    {
        if (_isTransitioning) return;
        StartCoroutine(TransitionToNextMap());
    }

    private IEnumerator TransitionToNextMap()
    {
        _isTransitioning = true;
        faderObject.SetActive(true);
        yield return StartCoroutine(fader.FadeOut(false));

        if (_currentIndex >= 0 && _currentIndex < _mapInstances.Count)
            _mapInstances[_currentIndex].Deactivate();

        _currentIndex++;

        if (_currentIndex < _mapInstances.Count)
        {
            var map = _mapInstances[_currentIndex];
            map.gameObject.SetActive(true);
            
            yield return ActivateMapRoutine(_currentIndex);

            if (_playerInstance != null && map.playerSpawnPoint != null)
                _playerInstance.transform.position = map.playerSpawnPoint.position;

            if (map.cameraBounds != null)
            {
                cameraConfiner.BoundingShape2D = map.cameraBounds;
                minimapcamConfiner.BoundingShape2D = map.cameraBounds;

                cameraConfiner.InvalidateBoundingShapeCache();
                minimapcamConfiner.InvalidateBoundingShapeCache();
            }

            ObjectPooler.ReturnAllActiveObjects();
            yield return new WaitForSeconds(_transitionDuration);

            yield return StartCoroutine(fader.FadeIn());
            yield return new WaitForSeconds(_transitionSpawnDuration);
            var controller = _playerInstance.GetComponent<PlayerController>();
            if (controller != null)
                controller.currentMapRoot = map.mapRoot;
        }
        else
        {
            Debug.Log("[MapManager] 모든 맵 클리어!");
            GameManager.Instance.ShowClearUI();
        }

        faderObject.SetActive(false);
        _isTransitioning = false;
    }

    public IEnumerator ResetToTown()
    {
        foreach (var map in _mapInstances)
            Destroy(map.gameObject);
        _mapInstances.Clear();

        foreach (var map in _tutorialMaps)
            if (map != null) Destroy(map.gameObject);
        _tutorialMaps.Clear();

        var townPrefab = cycleManager.LoadTownMap();
        var townInstance = Instantiate(townPrefab);
        townInstance.SetActive(false);
        _mapInstances.Add(townInstance.GetComponent<MapComponent>());

        _currentIndex = 0;
        yield return ActivateMapRoutine(_currentIndex);
    }

    public void EnterCombat()
    {
        if (_isTransitioning) return;
        _inTutorial = false;
        StartCoroutine(StartCombatRoutine());
    }

    private IEnumerator StartCombatRoutine()
    {
        faderObject.SetActive(true);
        yield return fader.FadeOut();
        yield return new WaitForSeconds(_loadingDuration);

        if (_currentIndex >= 0 && _currentIndex < _mapInstances.Count)
            _mapInstances[_currentIndex].Deactivate();

        _mapInstances.Clear();
        _currentIndex = 0;

        var combatPrefabs = cycleManager.BuildCombatCycle();
        foreach (var prefab in combatPrefabs)
        {
            var instance = Instantiate(prefab);
            instance.SetActive(false);
            _mapInstances.Add(instance.GetComponent<MapComponent>());
        }

        yield return ActivateMapRoutine(_currentIndex);
        yield return fader.FadeIn();
        faderObject.SetActive(false);
        _isTransitioning = false;
    }

    public void EnterTutorial()
    {
        if (_isTransitioning) return;
        _inTutorial = true;
        StartCoroutine(StartTutorialRoutine());
    }

    private IEnumerator StartTutorialRoutine()
    {
        faderObject.SetActive(true);
        yield return fader.FadeOut();
        yield return new WaitForSeconds(_loadingDuration);

        if (_currentIndex >= 0 && _currentIndex < _mapInstances.Count)
            _mapInstances[_currentIndex].Deactivate();

        _tutorialMaps.Clear();
        _tutorialIndex = 0;

        var tutorialPrefabs = Resources.LoadAll<GameObject>("Maps/Tutorial");
        foreach (var prefab in tutorialPrefabs)
        {
            var instance = Instantiate(prefab);
            instance.SetActive(false);
            _tutorialMaps.Add(instance.GetComponent<MapComponent>());
        }

        yield return ActivateTutorialMap(_tutorialIndex);
        yield return fader.FadeIn();
        faderObject.SetActive(false);
        _isTransitioning = false;
    }

    public void GoToNextTutorialMap()
    {
        if (_isTransitioning || !_inTutorial) return;
        StartCoroutine(TransitionToNextTutorialMap());
    }

    private IEnumerator TransitionToNextTutorialMap()
    {
        _isTransitioning = true;
        faderObject.SetActive(true);
        yield return fader.FadeOut(false);

        if (_tutorialIndex >= 0 && _tutorialIndex < _tutorialMaps.Count)
            _tutorialMaps[_tutorialIndex].Deactivate();

        _tutorialIndex++;

        if (_tutorialIndex < _tutorialMaps.Count)
        {
            yield return ActivateTutorialMap(_tutorialIndex);
            yield return fader.FadeIn();
        }
        else
        {
            _inTutorial = false;
            GameManager.Instance.HandleRespawn();
        }

        faderObject.SetActive(false);
        _isTransitioning = false;
    }

    private IEnumerator ActivateMapRoutine(int index)
    {
        var map = _mapInstances[index];
        return ActivateMapCommon(map);
    }

    private IEnumerator ActivateTutorialMap(int index)
    {
        var map = _tutorialMaps[index];
        return ActivateMapCommon(map);
    }

    private IEnumerator ActivateMapCommon(MapComponent map)
    {
        map.gameObject.SetActive(true);

        if (_playerInstance != null && map.playerSpawnPoint != null)
            _playerInstance.transform.position = map.playerSpawnPoint.position;

        if (_currentBackground != null)
            Destroy(_currentBackground);

        yield return new WaitForSeconds(0.2f);

        if (map.backgroundPrefab != null && _backgroundFollowParent != null)
        {
            _currentBackground = Instantiate(map.backgroundPrefab, _backgroundFollowParent);
            _currentBackground.transform.localPosition = new Vector3(0, 0, 30);
            _currentBackground.transform.localRotation = Quaternion.identity;
            _currentBackground.transform.localScale = Vector3.one;
        }

        var controller = _playerInstance.GetComponent<PlayerController>();
        if (controller != null)
            controller.currentMapRoot = map.mapRoot;

        if (map.cameraBounds != null)
        {
            cameraConfiner.BoundingShape2D = map.cameraBounds;
            minimapcamConfiner.BoundingShape2D = map.cameraBounds;

            cameraConfiner.InvalidateBoundingShapeCache();
            minimapcamConfiner.InvalidateBoundingShapeCache();
        }

        var vcam = FindObjectOfType<CinemachineVirtualCamera>();
        if (vcam != null)
            vcam.Follow = _playerInstance.transform;

        MapMusicManager.PlayMusicForMap(map);

        yield return new WaitForSeconds(0.5f);
        map.Activate();
    }
}
