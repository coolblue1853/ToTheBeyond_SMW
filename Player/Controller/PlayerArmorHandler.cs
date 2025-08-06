using UnityEngine;

public class PlayerArmorHandler : MonoBehaviour
{
    public Armor equippedArmor;
    public PlayerController playerController;
    private PlayerSpriteController _playerSpriteController;

    [SerializeField] private Transform _armorHolder;
    [SerializeField] public ArmorElement nearbyArmor;
    [SerializeField] private ArmorElementType _beforeElementType;

    public ElementController elementController;
    private Sprite _beforeSprite;
    public bool isFusion = false; // 이전에 융합 무기를 사용중이었는지 여부

    private void Awake()
    {
        _playerSpriteController = GetComponent<PlayerSpriteController>();
        playerController = GetComponent<PlayerController>();
        elementController = playerController.elementController;
    }

    public void ResetArmor()
    {
        CheckArmorElemet(ArmorElementType.Basic);
    }
    
    private void Start()
    {
        CheckArmorElemet(ArmorElementType.Basic);
    }

    public void OnInteract()
    {
        if ( nearbyArmor != null)
        {
            CheckArmorElemet(nearbyArmor);
        }
    }

    // 최초 방어구 장착
    private void CheckArmorElemet(ArmorElementType armorElementType)
    {
        // 기초 장착에는 다른 기능은 필요 없음
        if (armorElementType == ArmorElementType.Basic)
        {
            _beforeElementType = ArmorElementType.Basic;
            SwapArmor(elementController.armorDictionary[ArmorElementType.Basic]);
            elementController.armorElementType = ArmorElementType.Basic;
            return;
        }
    }

    // 이후 방어구 교체, 속성 확인 및 장착 실행
    public void CheckArmorElemet(ArmorElement newElement)
    {
        if(isFusion)
        {
            _beforeElementType = elementController.armorElementType;
            elementController.NotifyFusionBreakByArmor();
            isFusion = false;
        }
        
        // 1. 새로운 속성의 방어구 장착
        if (elementController.armorDictionary.ContainsKey(newElement._type))
        {
            SwapArmor(elementController.armorDictionary[newElement._type]);
            
            // 기존 방어구가 기본속성이면 파괴
            if (_beforeElementType == ArmorElementType.Basic)
            {               
                elementController.armorElementType = newElement._type;
                _beforeElementType = newElement._type;
                _beforeSprite = newElement.spriteRenderer.sprite;
                Destroy(newElement.gameObject);
            }
            else           
            {    
                elementController.armorElementType = newElement._type;
               var tempType = _beforeElementType;
               _beforeElementType = newElement._type;
               newElement._type = tempType;

               var tempSprite = _beforeSprite;
               _beforeSprite = newElement.spriteRenderer.sprite;
               newElement.spriteRenderer.sprite = tempSprite;
               nearbyArmor.SetDetail(elementController, playerController.expController.CurrentLevel);
            } 
        }
    
    
        // 2. 기존 방어구 속성이 기본이라면 그냥 장착, 아니면 속성 변경
        // 이제 속성을 합칠지 확인하는 과정.
        if(!isFusion)
            elementController.CheckFusionElement();
        
        // 3. 합성 속성이 해제되면 다시 기존 속성으로 변경
    }

    public void SwapArmor(WorldArmor worldArmor) // 아머 교체
    {
        Armor newArmor = worldArmor.armorComponent;
        Vector3 dropPosition = newArmor.transform.position;

        // 기존 아머 장착 해제 및 비활성화
        if (equippedArmor != null)
        {
            equippedArmor.Unequip();
        }

        // 프리팹 인스턴스를 생성한 후 부모 설정
        Armor instantiatedArmor = Instantiate(newArmor, _armorHolder);
        instantiatedArmor.transform.localPosition = Vector3.zero;
        instantiatedArmor.transform.localRotation = Quaternion.identity;

        // 새로운 아머의 SpriteRenderer 비활성화
        SetArmorSpriteRendererActive(instantiatedArmor, false);

        equippedArmor = instantiatedArmor; // 장착된 아머를 업데이트
        equippedArmor.Equip(playerController.runtimeStat);
        equippedArmor.playerController = playerController;
        equippedArmor.SetUpgradeLevel(playerController.expController.CurrentLevel);
        _playerSpriteController.ChangeSprite(newArmor.data.armorSprites);
    }

    // 아머의 SpriteRenderer 활성화/비활성화 처리
    private void SetArmorSpriteRendererActive(Armor armor, bool isActive)
    {
        // 아머의 SpriteRenderer를 찾아서 활성화/비활성화 처리
        SpriteRenderer spriteRenderer = armor.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = isActive;
        }
    }

}
