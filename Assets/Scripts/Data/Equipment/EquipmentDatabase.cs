using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "EquipmentDatabase", menuName = "Database/Equipment Database")]
public class EquipmentDatabase : ScriptableObject
{
    public List<EquipmentData> allEquipment;

    // 파라미터 타입을 'EquipmentData.PartType', 'EquipmentData.Grade'로 명확히 지정
    public EquipmentData GetData(EquipmentData.PartType part, EquipmentData.Grade grade)
    {
        return allEquipment.FirstOrDefault(data => data.part == part && data.grade == grade);
    }
}