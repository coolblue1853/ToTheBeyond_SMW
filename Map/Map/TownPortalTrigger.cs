using UnityEngine;
using System.Collections;

public class TownPortalTrigger : MonoBehaviour
{
    private bool _isPlayerNearby = false;
    private bool _inputLocked = false;
    private bool _hasEnteredCombat = false;
    [SerializeField] private float _inputCooldown = 5f;

    private void OnEnable()
    {
        _hasEnteredCombat = false;
    }

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
        if (!_isPlayerNearby || _inputLocked || _hasEnteredCombat) return;

        if (Input.GetKeyDown(KeyCode.V))
        {
            Debug.Log("탐험 시작!");
            _inputLocked = true;
            _hasEnteredCombat = true;

            MapManager.Instance.EnterCombat(); // 씬 전환

        }
    }
}
