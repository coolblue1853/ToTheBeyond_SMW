using UnityEngine;

public class GravityController
{
    private readonly Rigidbody2D _rb;
    private readonly float _originalGravity;
    private int _suppressCount = 0;

    public GravityController(Rigidbody2D rb)
    {
        _rb = rb;
        _originalGravity = rb.gravityScale;
    }

    // 중력 0으로 변경 
    public void Suppress()
    {
        _suppressCount++;
        _rb.gravityScale = 0f;
    }

    // 기존 중력으로 초기화 
    public void Restore()
    {
        _suppressCount = Mathf.Max(0, _suppressCount - 1);
        if (_suppressCount == 0)
        {
            _rb.gravityScale = _originalGravity;
        }
    }

    public void ForceReset()
    {
        _suppressCount = 0;
        _rb.gravityScale = _originalGravity;
    }
}
