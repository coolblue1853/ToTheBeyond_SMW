using UnityEngine;
using UnityEngine.InputSystem;
using MoreMountains.Feedbacks;
[RequireComponent(typeof(Rigidbody2D))]
public sealed class PlayerController : MonoBehaviour
{
    // 능력치 관련
    public RuntimeStat runtimeStat;
    [SerializeField] private string statAssetName = "Stats/TestStats";
    public StatHandler stat;

    // 체력 관련
    private HealthUIController _healthUI;
    public PlayerHealth _playerHealth;

    // 장비 관련
    public PlayerWeaponHandler _weaponHandler;
    public PlayerArmorHandler _armorHandler;
    public ElementController elementController;
    public ExpController expController;
    public PlayerEffectController effectController;
    public PlayerDefaultDebuffHandler defaultDebuffHandler;

    // 이동 관련
    private Rigidbody2D _rigidbody;
    private PlayerJump _jump;
    private PlayerDash _dash;
    public PlayerMovement movement;
    public bool isControllable = true;
    public Vector2 aimInput { get; private set; }
    public GravityController gravityCtrl { get; private set; }

    [SerializeField] private Transform _groundPivot;
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private MMF_Player _jumpFeedbacks;
    [SerializeField] private GameObject _footPivot;

    // 맵 관련
    public Transform currentMapRoot;

    // 애니메이션 관련
    private PlayerAnimatorController _animatorController; 
    
    // 인벤토리 관련
    [SerializeField] private GameObject _inventory;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _weaponHandler = GetComponent<PlayerWeaponHandler>();
        _armorHandler = GetComponent<PlayerArmorHandler>();
        expController = GetComponent<ExpController>();
        effectController = GetComponent<PlayerEffectController>();
        defaultDebuffHandler = GetComponent<PlayerDefaultDebuffHandler>();
        _animatorController = GetComponent<PlayerAnimatorController>();
        gravityCtrl = new GravityController(_rigidbody);

        if (!_groundPivot) _groundPivot = _footPivot.transform;

        if (stat == null)
        {
            stat = Resources.Load<StatHandler>(statAssetName);
            if (stat == null)
            {
                Debug.LogError("PlayerController: Could not load StatHandler from Resources!");
                return;
            }
        }

        runtimeStat = new RuntimeStat(stat);

        _dash = new PlayerDash(_rigidbody, runtimeStat, this);
        movement = new PlayerMovement(_rigidbody, runtimeStat);
        _jump = new PlayerJump(_rigidbody, runtimeStat, _groundPivot, _groundMask);
        _jump.OnJumped += () =>
        {
            if (_jumpFeedbacks != null)
            {
                _jumpFeedbacks.transform.position = _groundPivot.position;
                _jumpFeedbacks.PlayFeedbacks();
            }
        };
    }


    private void Start()
    {
        _healthUI = HealthUIController.Instance;
        _playerHealth = GetComponent<PlayerHealth>();
        _playerHealth.Init(this);
        _healthUI?.SetPlayer(_playerHealth);
    }

    private void Update()
    {
        if (!isControllable)
            return;

        _jump.UpdateTimers();
        _dash.UpdateTimers();

        if (CanAct())
        {
            if (_jump.JumpRequested) _jump.TryJump();
            if (_dash.DashRequested && _dash.TryDash(movement.FacingDirection))
            {
                _weaponHandler.equippedWeapon?.OnDash();
            }
        }
        _jump.ResetInputs();
        _dash.ResetInputs();

        UpdateAnimation();

        //인벤토리 입력
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            InventoryManager.Instance.TogleUI();
        }
  

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Alpha1))
            runtimeStat.AddModifier(new StatModifier(StatType.CritChance, 1f));
        if (Input.GetKeyDown(KeyCode.Alpha2))
            runtimeStat.AddModifier(new StatModifier(StatType.AttackSpeed, 1f));
        if (Input.GetKeyDown(KeyCode.Alpha3))
            _playerHealth.TakeDamage(10);
        if (Input.GetKeyDown(KeyCode.F1))
            runtimeStat.AddModifier(new StatModifier(StatType.BaseDamage, 10f));
        if (Input.GetKeyDown(KeyCode.F2))
            runtimeStat.AddModifier(new StatModifier(StatType.MaxAmmo, 0.5f));
        if (Input.GetKeyDown(KeyCode.F4))
            runtimeStat.AddModifier(new StatModifier(StatType.BulletSpeed, 1f));
        if (Input.GetKeyDown(KeyCode.F5))
            runtimeStat.AddModifier(new StatModifier(StatType.BulletsPerShot, 1));
#endif
     
    }

    public void UpdateLevel(int level)
    {
        _weaponHandler.equippedWeapon.SetUpgradeLevel(level);
        _armorHandler.equippedArmor.SetUpgradeLevel(level);
    }


    // 움직임 관리 
    private void FixedUpdate()
    {
        if (!isControllable)
            return;

        _jump.CheckGround();

        bool canMove = CanAct();

        if (canMove && _jump.MoveInput.x != 0)
        {
            movement.UpdateFacing(_jump.MoveInput.x);
            _weaponHandler.equippedWeapon?.SetDirection(Mathf.RoundToInt(Mathf.Sign(_jump.MoveInput.x)));
        }

        movement.Move(_dash.IsDashing, canMove ? _jump.MoveInput : Vector2.zero);

    }

    // 이동 관련 다리 애니메이션 갱신
    private void UpdateAnimation()
    {
        _animatorController.UpdateLegAnimation(
            _dash.IsDashing,
            _jump.IsGrounded,
            _rigidbody.velocity.y,
            Mathf.Abs(_rigidbody.velocity.x)
        );
    }

    // 뉴 인풋 시스템과 연결된 조작 함수 
    private void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        _jump.MoveInput = input;

        if (CanAct() && input.x != 0)
        {
            movement.UpdateFacing(input.x);
            _weaponHandler.equippedWeapon?.SetDirection(Mathf.RoundToInt(Mathf.Sign(input.x)));
        }
    }

    private void OnJump(InputValue value)
    {
        if (_jump == null) return;
        _jump.JumpRequested = value.isPressed;
    }

    private void OnDash(InputValue value)
    {
        if (_dash == null) return;
        _dash.DashRequested = value.isPressed;
    }

    private void OnDownJump(InputValue value)
    {
        if (value.isPressed && _jump.IsGrounded)
        {
            StartCoroutine(_jump.PlatformDropCoroutine(GetComponent<Collider2D>()));
            _jump.CancelJump();
        }
    }
    private void OnAttack(InputValue value)
    {
        if (!CanAct()) return;

        if (_weaponHandler?.equippedWeapon == null)
            return;

        bool isPressed = value.isPressed;
        var weapon = _weaponHandler.equippedWeapon;

        if (weapon is ChargeRangedWeapon chargeWeapon)
        {
            if (isPressed) chargeWeapon.Attack();
            else chargeWeapon.ReleaseCharge();
        }
        else if (weapon is PressRangedWeapon pressWeapon)
        {
            if (isPressed) pressWeapon.Attack();
            else pressWeapon.StopAttack();
        }
        else
        {
            if (isPressed) weapon.Attack();
        }
    }

    private void OnSkillA(InputValue value)
    {
        if (!DeathCheck()) return;
        if (value.isPressed)
            _armorHandler?.equippedArmor?.ActivateSkill((int)SkillSlot.A);
    }

    private void OnSkillS(InputValue value)
    {
       if (!DeathCheck()) return;
        _weaponHandler?.equippedWeapon?.HandleSkillInput(SkillSlot.S, value.isPressed);
    }

    private void OnSkillD(InputValue value)
    {
       if (!DeathCheck()) return;
        _weaponHandler?.equippedWeapon?.HandleSkillInput(SkillSlot.D, value.isPressed);
    }

    private void OnSkillF(InputValue value)
    {
       if (!DeathCheck()) return;
        _weaponHandler?.equippedWeapon?.HandleSkillInput(SkillSlot.F, value.isPressed);
    }

    private void OnAim(InputValue value) => aimInput = value.Get<Vector2>();

    private void OnReload(InputValue value)
    {
        if (!(_weaponHandler?.equippedWeapon is RangedWeapon ranged)) return;
        ranged.Reload();
    }

    public void ResetPlayerState()
    {
        runtimeStat = new RuntimeStat(stat);
        expController.ResetExp();
        if (_playerHealth != null)
        {
            _playerHealth.Init(this);
            _playerHealth.ResetHealth();
        }

        _weaponHandler.ResetWeapon();
        _armorHandler.ResetArmor();
        InventoryManager.Instance.ResetInventory();
        gravityCtrl.Restore();
        isControllable = true;
    }


    public void ResetVelocity() => _rigidbody.velocity = Vector2.zero;

    public bool CanAct()
    {
        return isControllable &&
               (_weaponHandler.equippedWeapon == null ||
                !_weaponHandler.equippedWeapon.isMovementLocked);
    }
    public bool DeathCheck()
    {
        return isControllable;
    }

}