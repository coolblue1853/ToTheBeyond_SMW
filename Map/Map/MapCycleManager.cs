using System.Collections.Generic;
using UnityEngine;

public class MapCycleManager : MonoBehaviour
{
    public string currentTheme = "Forest";
    public int minNormalCount = 2;
    public int maxNormalCount = 3;

    public GameObject LoadTownMap()
    {
        return Resources.Load<GameObject>("Maps/Town/TownMap");
    }

    public List<GameObject> BuildCombatCycle()
    {
        List<GameObject> result = new();

        // 1. 일반맵 타입 1 (Normal1 폴더)
        var normal1List = new List<GameObject>(Resources.LoadAll<GameObject>($"Maps/{currentTheme}/Normal1"));
        var firstNormals = PickRandom(normal1List, minNormalCount, maxNormalCount);
        result.AddRange(firstNormals);

        // 2. 미드보스
        var midBoss = Resources.Load<GameObject>($"Maps/{currentTheme}/MidBoss/{currentTheme}_MidBoss");
        if (midBoss != null) result.Add(midBoss);

        // 3. 일반맵 타입 2 (Normal2 폴더)
        var normal2List = new List<GameObject>(Resources.LoadAll<GameObject>($"Maps/{currentTheme}/Normal2"));
        var secondNormals = PickRandom(normal2List, minNormalCount, maxNormalCount);
        result.AddRange(secondNormals);

        // 4. 최종보스
        var finalBoss = Resources.Load<GameObject>($"Maps/{currentTheme}/FinalBoss/{currentTheme}_FinalBoss");
        if (finalBoss != null) result.Add(finalBoss);

        return result;
    }


    private List<GameObject> PickRandom(List<GameObject> source, int min, int max)
    {
        int count = Mathf.Min(Random.Range(min, max + 1), source.Count);
        List<GameObject> result = new();
        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, source.Count);
            result.Add(source[index]);
            source.RemoveAt(index);
        }
        return result;
    }
}