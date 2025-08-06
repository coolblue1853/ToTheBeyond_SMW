using System;
using UnityEngine;

public class PlayerDash
{
    private readonly Rigidbody2D _rb;
    private readonly IStatProvider _stat;

    private int _dashChainRemain;
    private float _chainTimer;
    private float _cooldownTimer;
    private float _dashTimer;
    private readonly float _dashDuration = 0.18f;
    private bool _cooldownStarted;

    public bool IsDashing { get; private set; }
    public bool DashRequested { get; set; }
    private PlayerController _playerController;
    

    public PlayerDash(Rigidbody2D rb, IStatProvider stat, PlayerController playerController)
    {
        this._rb = rb;
        this._stat = stat;
        _playerController = playerController;
        _dashChainRemain = stat.MaxDashCount;
    }


    // 대쉬 쿨타임 관리 
    public void UpdateTimers()
    {
        Decrement(ref _chainTimer);
        Decrement(ref _cooldownTimer);
        Decrement(ref _dashTimer);

        if (_dashTimer <= 0f && IsDashing)
        {
            IsDashing = false;
            _playerController.gravityCtrl.Restore(); 
            _playerController.effectController.StopTrailEffect();
        }

        if (_chainTimer <= 0f &&
            _dashChainRemain < _stat.MaxDashCount &&
            _cooldownTimer <= 0f &&
            !_cooldownStarted)
        {
            _cooldownTimer = _stat.DashCooldown;
            _cooldownStarted = true;
        }

        if (_cooldownStarted && _cooldownTimer <= 0f)
        {
            _dashChainRemain = _stat.MaxDashCount;
            _cooldownStarted = false;
        }
    }

    public bool TryDash(int facingDir)
    {
        if (IsDashing || _dashChainRemain <= 0 || _cooldownTimer > 0f) return false;

        _dashChainRemain--;
        _chainTimer = _stat.DashChainWindow;
        _dashTimer = _dashDuration;
        IsDashing = true;
        _playerController.effectController.PlayTrailEffect();

        _playerController.gravityCtrl.Suppress(); // 중력 제거
        _rb.velocity = Vector2.zero;
        _rb.AddForce(new Vector2(facingDir * _stat.DashSpeed, 0f), ForceMode2D.Impulse);

        _playerController.GetComponent<PlayerHealth>()?.StartInvincibility(_dashDuration);
        return true;
    }

    public void ResetInputs() => DashRequested = false;

    // 시간 감소, 추후 유틸리티로 빼는게 좋을 것으로 보임  
    private void Decrement(ref float timer)
    {
        if (timer > 0f) timer -= Time.deltaTime;
    }
}
