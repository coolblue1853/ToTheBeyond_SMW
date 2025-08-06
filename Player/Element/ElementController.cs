using System.Collections.Generic;
using UnityEngine;

public enum ArmorElementType
{
    Basic ,Water, Fire, Grass, Ice, Darkness, Light
}

public enum WeaponType
{
    Gun, Bow, Spear, Knuckle, Staff, Sword, GreatSword, Hammer, Claw, SwordShield // 무기 종류
}
public enum WeaponElementType
{
    Basic,  Wind, Luck, Blood, Chaos, Poison, Steel, Soul,
    Steam, Abyss, Fisher, Pirate, WarMachine, Ash, Alchemist, WorldTree, MushroomSpore, Druid, VampirePlant,
    Blizzard, IceMartial, Frostbite, DragonKnight, Gambler, GrimReaper, PlagueDoctor, Vampire, DarkKnight, Cleric, Paladin, Inquisitor, Astrologer, Immortal
}
public enum CombinedElementType
{
    // 방어구와 무기의 합성 속성들
    Steam, Abyss, Fisher, Pirate, WarMachine, Ash, Alchemist, WorldTree, MushroomSpore, Druid, VampirePlant,
    Blizzard, IceMartial, Frostbite, DragonKnight, Gambler, GrimReaper, PlagueDoctor, Vampire, DarkKnight, Cleric, Paladin, Inquisitor, Astrologer, Immortal
}


[System.Serializable] 
public class ArmorCombination
{
    public ArmorElementType elementType;
    public WorldArmor armorPrefab;  // 해당 속성에 맞는 방어구 프리팹
}

[System.Serializable] 
public class WeaponCombination
{
    public WeaponType type;  // 무기 타입
    public WeaponElementType elementType;  // 무기 속성
    public WorldWeapon weaponPrefab;  // 해당 속성에 맞는 무기 프리팹
}

[System.Serializable]
public class ArmorFusionEntry
{
    public CombinedElementType combinedElement;
    public WorldArmor armorPrefab;
}
[System.Serializable]
public class WeaponFusionEntry
{
    public CombinedElementType combinedElement;
    public WeaponType weaponType;
    public WorldWeapon weaponPrefab;
}

public class ElementController : MonoBehaviour
{   
    private PlayerArmorHandler _armorHandler;
    private PlayerWeaponHandler _weaponHandler;
    //합성 속성
    public Dictionary<(ArmorElementType, WeaponElementType), CombinedElementType> armorWeaponCombinationDictionary;

    // 방어구 속성 슬롯
    [SerializeField] private List<ArmorCombination> _armorList;
    public Dictionary<ArmorElementType, WorldArmor> armorDictionary;

    // 무기 속성 슬롯
    [SerializeField] private List<WeaponCombination> _weaponList;
    public Dictionary<(WeaponElementType, WeaponType), WorldWeapon> weaponDictionary;
    
    // 합성 속성 슬롯
    [SerializeField] private List<ArmorFusionEntry> _armorFusionEntries;
    [SerializeField] private List<WeaponFusionEntry> _weaponFusionEntries;
    public Dictionary<CombinedElementType, WorldArmor> armorFusionDictionary;
    public Dictionary<(CombinedElementType, WeaponType), WorldWeapon> weaponFusionDictionary;

    
    public WeaponElementType weaponElementType;
    public WeaponType weaponType;
    public ArmorElementType armorElementType;
    
    
    private void Awake()
    {
        SetFusionDict();
        armorDictionary = new Dictionary<ArmorElementType, WorldArmor>();
        _armorHandler = transform.root.GetComponent<PlayerArmorHandler>();
        _weaponHandler = transform.root.GetComponent<PlayerWeaponHandler>();
        // 리스트에서 딕셔너리로 방어구 조합을 등록
        foreach (var combination in _armorList)
        {
            if (!armorDictionary.ContainsKey(combination.elementType))
            {
                armorDictionary.Add(combination.elementType, combination.armorPrefab);
            }
        }
        
        weaponDictionary = new Dictionary<(WeaponElementType, WeaponType), WorldWeapon>();

        // 리스트에서 딕셔너리로 무기 조합을 등록
        foreach (var combination in _weaponList)
        {
            var key = (ElementType: combination.elementType, Type: combination.type);
            if (!weaponDictionary.ContainsKey(key))
            {
                weaponDictionary.Add(key, combination.weaponPrefab);
            }
        }
        
        // weaponFusionDictionary 초기화
        weaponFusionDictionary = new();
        foreach (var entry in _weaponFusionEntries)
        {
            var key = (CombinedElement: entry.combinedElement, WeaponType: entry.weaponType);
            if (!weaponFusionDictionary.ContainsKey(key))
            {
                weaponFusionDictionary.Add(key, entry.weaponPrefab);
            }
        }

        // armorFusionDictionary 초기화
        armorFusionDictionary = new();
        foreach (var entry in _armorFusionEntries)
        {
            if (!armorFusionDictionary.ContainsKey(entry.combinedElement))
            {
                armorFusionDictionary.Add(entry.combinedElement, entry.armorPrefab);
            }
        }
    }
    
    
    // 무기 속성 및 타입에 맞는 무기를 가져오는 함수
    public WorldWeapon GetWeaponByTypeAndElement(WeaponElementType elementType, WeaponType weaponType)
    {
        var key = (elementType, weaponType);
        if (weaponDictionary.ContainsKey(key))
        {
            return weaponDictionary[key];
        }
        else
        {
            return null;
        }
    }
    
    // 합성 속성이 있는지 확인해서 각각 방어구와 무기를 교체하는 함수
    public void CheckFusionElement()
    {
        if (armorWeaponCombinationDictionary == null)
            return;

        if (armorWeaponCombinationDictionary.TryGetValue((armorElementType, weaponElementType), out var checkFusionType))
        {
            if (!armorFusionDictionary.ContainsKey(checkFusionType) || !weaponFusionDictionary.ContainsKey((checkFusionType, weaponType)))
                return;
            // 프리팹을 인스턴스화
            var armorPrefab = armorFusionDictionary[checkFusionType];
            var weaponPrefab = weaponFusionDictionary[(checkFusionType, weaponType)];

            WorldArmor armorInstance = Instantiate(armorPrefab, _armorHandler.transform.position, Quaternion.identity);
            WorldWeapon weaponInstance = Instantiate(weaponPrefab, _weaponHandler.transform.position, Quaternion.identity);

            _weaponHandler.equippedWeapon.Unequip();
            Destroy(_weaponHandler.equippedWeapon.gameObject);
            _armorHandler.SwapArmor(armorInstance);
            _weaponHandler.SwapWeapon(weaponInstance, true);
            Destroy(armorInstance.gameObject);

            _armorHandler.isFusion = true;
            _weaponHandler.isFusion = true;
        }
    }

    // 무기 교체로 인해 합성 속성이 깨졌을 경우 
    public WorldWeapon NotifyFusionBreakByWeapon()
    {
        var armorPrefab = armorDictionary[armorElementType];
        var weaponPrefab = weaponDictionary[(weaponElementType, weaponType)];
        WorldArmor armorInstance = Instantiate(armorPrefab, _armorHandler.transform.position, Quaternion.identity);
        WorldWeapon weaponInstance = Instantiate(weaponPrefab, _weaponHandler.transform.position, Quaternion.identity);
        weaponInstance.transform.SetParent(GameManager.Instance.playerController.currentMapRoot);
        _armorHandler.SwapArmor(armorInstance);
        Destroy(armorInstance.gameObject);
        _armorHandler.isFusion = false;
        _weaponHandler.isFusion = false;
        return weaponInstance;
    }
    // 방어구 교체로 인해 합성 속성이 깨졌을 경우 
    public void NotifyFusionBreakByArmor()
    {
        _weaponHandler.isFusion = false;
        _armorHandler.isFusion = false;
        Destroy((_weaponHandler.equippedWeapon.gameObject));
        var weaponPrefab = weaponDictionary[(weaponElementType, weaponType)];
        WorldWeapon weaponInstance = Instantiate(weaponPrefab, _weaponHandler.transform.position, Quaternion.identity);
        _weaponHandler.SwapWeapon(weaponInstance,true);
        Destroy(weaponInstance.gameObject);
    }


    // 합성속성 딕셔너리 
    public void SetFusionDict()
    {        
        // 딕셔너리 초기화
        armorWeaponCombinationDictionary = new Dictionary<(ArmorElementType,WeaponElementType), CombinedElementType>();
       // 합성 속성 등록 (방어구 속성 + 무기 속성 = 합성 속성)
       // armorWeaponCombinationDictionary.Add((ArmorElementType.Water, WeaponElementType.Soul, weapon), CombinedElementType.Abyss);  // 물+영혼 = 심해
        armorWeaponCombinationDictionary.Add((ArmorElementType.Water, WeaponElementType.Steel), CombinedElementType.Steam);  // 물+강철 = 스팀펑크 기관총
       // armorWeaponCombinationDictionary.Add((ArmorElementType.Water, WeaponElementType.Luck), CombinedElementType.Fisher);  // 물+행운 = 낚시꾼
       // armorWeaponCombinationDictionary.Add((ArmorElementType.Water, WeaponElementType.Basic), CombinedElementType.Pirate);  // 물+바다 = 해적
       // armorWeaponCombinationDictionary.Add((ArmorElementType.Fire, WeaponElementType.Steel), CombinedElementType.WarMachine);  // 불+강철 = 전쟁기계
       // armorWeaponCombinationDictionary.Add((ArmorElementType.Fire, WeaponElementType.Wind), CombinedElementType.Ash);  // 불+바람 = 재-다크엘프
       // armorWeaponCombinationDictionary.Add((ArmorElementType.Fire, WeaponElementType.Poison), CombinedElementType.Alchemist);  // 불+독 = 연금술사
       // armorWeaponCombinationDictionary.Add((ArmorElementType.Fire, WeaponElementType.Blood), CombinedElementType.WorldTree);  // 불+피 = 광염
       // armorWeaponCombinationDictionary.Add((ArmorElementType.Grass, WeaponElementType.Luck), CombinedElementType.WorldTree);  // 풀+행운 = 세계수
       // armorWeaponCombinationDictionary.Add((ArmorElementType.Grass, WeaponElementType.Poison), CombinedElementType.MushroomSpore);  // 풀+독 = 버섯포자
       // armorWeaponCombinationDictionary.Add((ArmorElementType.Grass, WeaponElementType.Soul), CombinedElementType.Druid);  // 풀+영혼 = 드루이드
       // armorWeaponCombinationDictionary.Add((ArmorElementType.Grass, WeaponElementType.Blood), CombinedElementType.VampirePlant);  // 풀+피 = 흡혈식물
       // armorWeaponCombinationDictionary.Add((ArmorElementType.Ice, WeaponElementType.Wind), CombinedElementType.Blizzard);  // 얼음+바람 = 눈보라
       // armorWeaponCombinationDictionary.Add((ArmorElementType.Ice, WeaponElementType.Steel), CombinedElementType.IceMartial);  // 얼음+강철 = 얼음무협
       // armorWeaponCombinationDictionary.Add((ArmorElementType.Ice, WeaponElementType.Poison), CombinedElementType.Frostbite);  // 얼음+독 = 오한빙결
       // armorWeaponCombinationDictionary.Add((ArmorElementType.Ice, WeaponElementType.Chaos), CombinedElementType.DragonKnight);  // 얼음+혼돈 = 용기사
       // armorWeaponCombinationDictionary.Add((ArmorElementType.Darkness, WeaponElementType.Luck), CombinedElementType.Gambler);  // 어둠+행운 = 도박사
       // armorWeaponCombinationDictionary.Add((ArmorElementType.Darkness, WeaponElementType.Chaos), CombinedElementType.GrimReaper);  // 어둠+혼돈 = 저승사자
       // armorWeaponCombinationDictionary.Add((ArmorElementType.Darkness, WeaponElementType.Poison), CombinedElementType.PlagueDoctor);  // 어둠+독 = 역병의사
        armorWeaponCombinationDictionary.Add((ArmorElementType.Darkness, WeaponElementType.Blood), CombinedElementType.Vampire);  // 어둠+피 = 뱀파이어
       // armorWeaponCombinationDictionary.Add((ArmorElementType.Darkness, WeaponElementType.Steel), CombinedElementType.DarkKnight);  // 어둠+강철 = 어둠기사
       // armorWeaponCombinationDictionary.Add((ArmorElementType.Light, WeaponElementType.Soul), CombinedElementType.Cleric);  // 빛+영혼 = 성직자
       // armorWeaponCombinationDictionary.Add((ArmorElementType.Light, WeaponElementType.Steel), CombinedElementType.Paladin);  // 빛+강철 = 팔라딘
       // armorWeaponCombinationDictionary.Add((ArmorElementType.Light, WeaponElementType.Chaos), CombinedElementType.Inquisitor);  // 빛+혼돈 = 이단 심문관
       // armorWeaponCombinationDictionary.Add((ArmorElementType.Light, WeaponElementType.Luck), CombinedElementType.Astrologer);  // 빛+행운 = 점성술사
        //armorWeaponCombinationDictionary.Add((ArmorElementType.Light, WeaponElementType.Wind), CombinedElementType.Immortal);  // 빛+바람 = 신선
    }
}