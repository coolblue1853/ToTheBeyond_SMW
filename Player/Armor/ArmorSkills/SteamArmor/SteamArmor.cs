using System.Collections;
using UnityEngine;
using DarkTonic.MasterAudio;
public class SteamArmor : Armor
{
    // 열기 수치를 사용하는 증기 특성의 방어구
    [SerializeField] private float _maxHeat = 100f;
    [SerializeField] private float _coolRate = 10f;
    [SerializeField] private float _overheatDuration = 3f;

    public float CurrentHeat { get; private set; } = 0f;
    public bool IsOverheated { get; private set; } = false; // 오버히트시 공격, 스킬 사용불가 
    public float MaxHeat => _maxHeat;

    private float _lastAttackTime;
    [SerializeField] private string _whistle;
    private void Update()
    {
        if (IsOverheated) return;

        if (Time.time - _lastAttackTime > 2f && CurrentHeat > 0f)
        {
            CurrentHeat = Mathf.MoveTowards(CurrentHeat, 0f, _coolRate * Time.deltaTime);
        }
    }

    public override void Equip(RuntimeStat stat)
    {
        base.Equip(stat);
        HeatUIManager.Instance?.SetSteamArmor(this);
    }

    public override void Unequip()
    {
        base.Unequip();
        HeatUIManager.Instance?.Clear();
    }

    // 공격시 열기 증가를 위한 함수 
    public void AddHeat(float amount)
    {
        if (IsOverheated) return;

        _lastAttackTime = Time.time;
        CurrentHeat += amount;
        if (CurrentHeat >= _maxHeat)
        {
            StartCoroutine(OverheatRoutine());
        }
    }

    // 시간이 지나면 자동으로 열기 감소
    public void ReduceHeat(float amount)
    {
        CurrentHeat = Mathf.Clamp(CurrentHeat - amount, 0f, _maxHeat);
    }

    private IEnumerator OverheatRoutine()
    {
        IsOverheated = true;
        MasterAudio.PlaySound(_whistle);
        yield return new WaitForSeconds(_overheatDuration);
        CurrentHeat = 0f;
        IsOverheated = false;
    }
}