using UnityEngine;

public class PlayerWeaponHandler : MonoBehaviour
{
    [HideInInspector] public Weapon equippedWeapon;
    [SerializeField] private GameObject _basicWeapon;
    [SerializeField] private Transform _weaponHolder;
    [SerializeField] public Transform leftWeaponHolder;

    public WorldWeapon nearbyWeapon;
    public SpriteRenderer rightWeaponSprtieRenderer;
    public SpriteRenderer leftWeaponSprtieRenderer;
    public PlayerController playerController;
    public ElementController elementController;

    public bool isFusion = false; // 융합 무기를 사용중인지 체크 

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        elementController = playerController.elementController;
    }

    private void Start()
    {
        ResetWeapon();
    }

    // 초기 무기로 교체
    public void ResetWeapon()
    {
        GameObject go = Instantiate(_basicWeapon, transform.position, Quaternion.identity);
        WorldWeapon worldWeapon = go.GetComponent<WorldWeapon>();
        SwapWeapon(worldWeapon, true);
    }

    public void OnInteract()
    {
        if (nearbyWeapon != null)
        {
            SwapWeapon(nearbyWeapon);
        }
    }

    // 무기 교체
    public void SwapWeapon(WorldWeapon worldWeapon, bool isDestory = false)
    {
        Weapon newWeapon = worldWeapon.weaponComponent;
        newWeapon.playerController = playerController;
        Vector3 dropPosition = worldWeapon.transform.position;
        Weapon dropWeapon = null;

        // 이전에 합성 무기였다면 
        if (isFusion)
        {

            if (!isDestory)
            {
                Destroy(equippedWeapon.gameObject);
                dropWeapon = elementController.NotifyFusionBreakByWeapon().weaponComponent;
                isFusion = false;
            }
            else
            {
                Destroy(equippedWeapon.gameObject);
                isFusion = false;
            }
        }


        // 월드에 놓지 않고 파괴하는경우 
        if (!isDestory)
        {
            elementController.weaponElementType = newWeapon.data.elementType;
            elementController.weaponType = newWeapon.data.weaponType;
        }

        if (equippedWeapon != null)
        {
            equippedWeapon.Unequip();
            if (!isDestory)
            {
                ReuseWorldWeaponDrop(equippedWeapon, dropPosition);
            }
            else
            {
                Destroy(equippedWeapon.gameObject);
            }

        }


        // 무기 교체 및 애니메이션 변경 
        equippedWeapon = newWeapon;
        Vector3 scale = newWeapon.transform.localScale;
        newWeapon.transform.localScale = new Vector3(Mathf.Abs(scale.x) * playerController.movement.FacingDirection, scale.y, scale.z);

        var animator = playerController.GetComponent<PlayerAnimatorController>();
        if (animator != null && newWeapon.data is WeaponDataSO weaponData)
        {
            animator.PlayUpperBodyAnimation(weaponData.upperBodyIdleAnim);
        }

        newWeapon.transform.SetParent(_weaponHolder);
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;

        equippedWeapon.Equip(playerController.runtimeStat, playerController.runtimeStat, this.transform);
        equippedWeapon.SetUpgradeLevel(playerController.expController.CurrentLevel);
        equippedWeapon?.SetDirection(playerController.movement.FacingDirection);

        // 기존 월드 무기는 파괴 대신 비활성화
        worldWeapon.gameObject.SetActive(false);

        // WorldWeapon 참조 저장
        newWeapon.worldWeaponRef = worldWeapon;

        if (!isDestory)
        {
            elementController.CheckFusionElement();
        }
    }

    // 손에 들고 있던 무기를 월드로 
    private void ReuseWorldWeaponDrop(Weapon weapon, Vector3 position)
    {
        if (weapon.worldWeaponRef == null)
        {
            CreateWorldWeaponDrop(weapon, position);
            return;
        }

        var worldWeapon = weapon.worldWeaponRef;
        var worldWeaponObj = worldWeapon.gameObject;

        // 1. 위치 이동
        worldWeaponObj.transform.position = position;
        worldWeaponObj.SetActive(true);

        // 2. 무기와의 연결 해제 및 부모 이동
        weapon.transform.SetParent(null); // 기존 부모에서 떼기
        weapon.transform.SetParent(worldWeaponObj.transform); // 월드 무기 자식으로 설정
        worldWeaponObj.transform.SetParent(playerController.currentMapRoot);
        weapon.transform.localPosition = weapon.worldOffset;
        weapon.transform.localRotation = Quaternion.identity;
        weapon.transform.localScale = Vector3.one;

        // 3. 콜라이더 켜기
        var collider = worldWeaponObj.GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = true;

        // 4. 무기 정보 다시 연결
        worldWeapon.weaponComponent = weapon;

        // 5. 디테일 표시
        worldWeapon.SetDetail(playerController.expController.CurrentLevel);

        // 6. 맵 루트에 붙이기
        worldWeaponObj.transform.SetParent(playerController.currentMapRoot, worldPositionStays: true);
    }


    // 융합 무기를 사용중이었던 경우 아예 새 무기 생성 후 월드로 
    private void CreateWorldWeaponDrop(Weapon weapon, Vector3 position)
    {
        GameObject obj = new GameObject("WorldWeapon");
        obj.transform.position = position;
        obj.AddComponent<CircleCollider2D>().isTrigger = true;
        var worldWeapon = obj.AddComponent<WorldWeapon>();
        worldWeapon.weaponComponent = weapon;
        worldWeapon.transform.SetParent(playerController.currentMapRoot);

        weapon.transform.SetParent(obj.transform);
        weapon.transform.localPosition = weapon.worldOffset;
        weapon.transform.localRotation = Quaternion.identity;
        weapon.transform.localScale = Vector3.one;


        var sr = weapon.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.enabled = true;

        weapon.worldWeaponRef = worldWeapon;

        if (playerController != null)
        {
            obj.transform.SetParent(playerController.currentMapRoot, worldPositionStays: true);
            worldWeapon.SetDetail(playerController.expController.CurrentLevel);
        }
    }

}
