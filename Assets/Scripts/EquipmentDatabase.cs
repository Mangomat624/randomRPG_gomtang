// EquipmentDatabase.cs

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "EquipmentDatabase", menuName = "Database/Equipment Database")]
public class EquipmentDatabase : ScriptableObject
{
    // 이 리스트에 프로젝트에 생성된 모든 EquipmentData.asset 파일들을 넣어줍니다.
    public List<EquipmentData> allEquipment;

    // 부위와 등급에 맞는 EquipmentData를 찾아주는 기능
    public EquipmentData GetData(EquipmentData.PartType part, EquipmentData.Grade grade)
    {
        return allEquipment.FirstOrDefault(data => data.part == part && data.grade == grade);
    }
}