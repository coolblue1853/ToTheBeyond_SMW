using UnityEngine;

public class PlayerMovement
{
    private readonly Rigidbody2D _rb;
    private readonly IStatProvider _stat;

    public int FacingDirection { get; set; } = 1;

    public PlayerMovement(Rigidbody2D rb, IStatProvider stat)
    {
        _rb = rb;
        _stat = stat;
    }

    // 이동시 캐릭터 전환 
    public void UpdateFacing(float horizontal)
    {
        if (horizontal > 0.01f) FacingDirection = 1;
        if (horizontal < -0.01f) FacingDirection = -1;
    }

    public void Move(bool isDashing, Vector2 moveInput)
    {
        if (!isDashing)
            _rb.velocity = new Vector2(moveInput.x * _stat.MoveSpeed, _rb.velocity.y);

        Vector3 scale = _rb.transform.localScale;
        _rb.transform.localScale = new Vector3(FacingDirection * Mathf.Abs(scale.x), scale.y, scale.z);
    }
}
