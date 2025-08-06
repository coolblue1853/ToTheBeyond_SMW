using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTrigger : MonoBehaviour
{
    private bool _isPlayerNearby = false;
    private bool _canEnter = false;
    private bool _inputLocked = false;
    [SerializeField] private float _inputCooldown = 1f; 
    public void EnablePortal() => _canEnter = true;  // 맵이 클리어되면 호출됨
    

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            _isPlayerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            _isPlayerNearby = false;
    }

    private void Update()
    {
        if (!_isPlayerNearby || !_canEnter || _inputLocked) return;

        if (Input.GetKeyDown(KeyCode.V))
        {
            Debug.Log("문 진입! 다음 맵으로 이동");
            _inputLocked = true;
            MapManager.Instance.GoToNextMap();
            ResetPortal();
            StartCoroutine(UnlockInputAfterDelay());
        }
    }

    private IEnumerator UnlockInputAfterDelay()
    {
        yield return new WaitForSeconds(_inputCooldown);
        _inputLocked = false;
    }

    
    public void ResetPortal()
    {
        _canEnter = false;
        _isPlayerNearby = false;
    }
}
