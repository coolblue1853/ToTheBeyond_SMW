using System;
using UnityEngine;
using System.Collections;

public class TutorialPortalTrigger : MonoBehaviour
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
            _isPlayerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            _isPlayerNearby = false;
    }

    private void Update()
    {
        if (!_isPlayerNearby || _inputLocked) return;

        if (Input.GetKeyDown(KeyCode.V))
        {
            _inputLocked = true;
            MapManager.Instance.EnterTutorial();    
        }
    }

}