using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "MosterDatabase", menuName = "Database/Monster Database")]
public class MonsterDatabase : ScriptableObject
{
    public List<MonsterData> allMonsters;

    // 파라미터 타입을 'EquipmentData.PartType', 'EquipmentData.Grade'로 명확히 지정
    public MonsterData GetMonstersByStage(int stage)
    {
        // Where를 사용해 stage가 일치하는 몬스터를 찾아 반환
        return allMonsters.FirstOrDefault(data => data.stage == stage);
    }
}