using System.Collections;
using UnityEngine;

public abstract class ArmorSkill : MonoBehaviour
{
    protected SkillSO _skill;
    protected RuntimeStat _stat;
    protected Transform _owner;
    protected int _level = 1;

    [SerializeField] private float _delayTime = 1f;

    // 방어구 스킬 외부 초기화
    public virtual void Initialize(SkillSO skillData, Transform ownerTransform, Armor armor , int skillLevel)
    {
        _skill = skillData;
        _owner = ownerTransform;
        _stat = armor.playerStat;
        _level = skillLevel;
    }

    public abstract bool Activate();

    protected IEnumerator DelayRoutine()
    {
        yield return new WaitForSeconds(_delayTime);
    }
}
