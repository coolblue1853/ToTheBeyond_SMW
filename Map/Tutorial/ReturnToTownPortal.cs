using UnityEngine;
using System.Collections;

public class ReturnToTownPortal : MonoBehaviour
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
            Debug.Log("입장");
            _isPlayerNearby = true;
        }
     
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("퇴장");
            _isPlayerNearby = false;
        }
     
    }

    private void Update()
    {
        if (!_isPlayerNearby || _inputLocked) return;

        if (Input.GetKeyDown(KeyCode.V))
        {
            Debug.Log("[ReturnToTownPortal] 마을로 복귀합니다");
            _inputLocked = true;
            GameManager.Instance.HandleRespawn();
        }
    }


}