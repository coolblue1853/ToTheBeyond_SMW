using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump
{
    private readonly Rigidbody2D _rb;
    private readonly IStatProvider _stat;
    private readonly Transform _groundPivot;
    private readonly LayerMask _groundMask;

    private int _jumpsRemaining;
    private float _coyoteTimer;
    private const float _coyoteTime = 0.1f;
    public event System.Action OnJumped;
    public Vector2 MoveInput { get; set; }
    public bool JumpRequested { get; set; }
    public bool IsGrounded { get; private set; }

    public PlayerJump(Rigidbody2D rb, IStatProvider stat, Transform groundPivot, LayerMask groundMask)
    {
        _rb = rb;
        _stat = stat;
        _groundPivot = groundPivot;
        _groundMask = groundMask;
        _jumpsRemaining = stat.MaxJumps - 1;
    }

    public void UpdateTimers() => _coyoteTimer -= Time.deltaTime;

    // 충돌 체크에 의한 Ground 체크 
    public void CheckGround()
    {
        bool wasGrounded = IsGrounded;
        IsGrounded = false;

        Collider2D hit = Physics2D.OverlapCircle(_groundPivot.position, 0.25f, _groundMask);
        if (hit != null)
        {
            if (hit.TryGetComponent(out PlatformEffector2D eff))
            {
                if (_rb.velocity.y <= 0f && _groundPivot.position.y > hit.bounds.center.y)
                    IsGrounded = true;
            }
            else
            {
                IsGrounded = true;
            }
        }

        if (IsGrounded && !wasGrounded)
        {
            _coyoteTimer = _coyoteTime;
            _jumpsRemaining = _stat.MaxJumps - 1;
        }
        else if (!IsGrounded && wasGrounded)
        {
            _coyoteTimer = _coyoteTime;
        }
    }


    public void TryJump()
    {
        bool canGroundJump = IsGrounded || _coyoteTimer > 0f;
        bool canAirJump = _jumpsRemaining > 0;
        if (!canGroundJump && !canAirJump) return;

        _rb.velocity = new Vector2(_rb.velocity.x, 0f);
        _rb.AddForce(Vector2.up * _stat.JumpForce, ForceMode2D.Impulse);

        if (!canGroundJump) _jumpsRemaining--;
        IsGrounded = false;
        _coyoteTimer = 0f;
        
        OnJumped?.Invoke();
    }

    // 아랫점프시 일정시간동안 충돌 무시 
    public IEnumerator PlatformDropCoroutine(Collider2D col)
    {
        ContactFilter2D filter = new() { layerMask = _groundMask, useTriggers = false };
        var results = new List<Collider2D>();
        Physics2D.OverlapCollider(col, filter, results);

        foreach (var r in results)
            if (r.TryGetComponent(out PlatformEffector2D eff))
                Physics2D.IgnoreCollision(col, r, true);

        yield return new WaitForSeconds(0.35f);

        foreach (var r in results)
            if (r) Physics2D.IgnoreCollision(col, r, false);
    }

    public void CancelJump() => JumpRequested = false;

    public void ResetInputs() => JumpRequested = false;
}
