using UnityEngine;
using System.Collections;

public class NextTutorialPortalTrigger : MonoBehaviour
{
    private bool _isPlayerNearby = false;
    private bool _inputLocked = false;
    [SerializeField] private float _inputCooldown = 1f;

    private void OnEnable()
    {
        _inputLocked = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerNearby = true;
        }
        
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerNearby = false;
        }

    }

    private void Update()
    {
        if (!_isPlayerNearby || _inputLocked) return;

        if (Input.GetKeyDown(KeyCode.V))  // 'V' 키를 누르면 다음 튜토리얼 맵으로 이동
        {
            Debug.Log("[NextTutorialPortal] 다음 튜토리얼 맵으로 이동!");
            _inputLocked = true;
            MapManager.Instance.GoToNextTutorialMap();
        }
    }


}