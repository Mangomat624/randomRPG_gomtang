// EquipmentGenerator.cs

using UnityEngine;
using UnityEditor;
using System.IO;

public class EquipmentGenerator
{
    [MenuItem("Tools/Data/Generate Equipment Data (Exclude Head)")]
    public static void GenerateAllEquipment()
    {
        var partTypes = System.Enum.GetValues(typeof(EquipmentData.PartType));
        var grades = System.Enum.GetValues(typeof(EquipmentData.Grade));
        string folderPath = "Assets/Resources/EquipmentData";

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        foreach (EquipmentData.PartType part in partTypes)
        {
            // --- 이 부분이 추가되었습니다 ---
            // 만약 현재 부위가 '머리'라면, 이번 반복을 건너뛰고 다음 부위로 넘어갑니다.
            if (part == EquipmentData.PartType.Head)
            {
                continue;
            }

            foreach (EquipmentData.Grade grade in grades)
            {
                EquipmentData newEquipment = ScriptableObject.CreateInstance<EquipmentData>();
                newEquipment.part = part;
                newEquipment.grade = grade;

                CalculateStatsFromTable(newEquipment);

                string fileName = $"{part}_{grade}.asset";
                string fullPath = Path.Combine(folderPath, fileName);
                AssetDatabase.CreateAsset(newEquipment, fullPath);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("'머리'를 제외한 장비 데이터 생성을 완료했습니다! 폴더: " + folderPath);
    }

    // 능력치 계산 함수는 이전과 동일합니다.
    private static void CalculateStatsFromTable(EquipmentData data)
    {
        int gradeIndex = (int)data.grade;

        int[] headHpStats = { 10, 30, 100, 160, 240, 350, 490, 677 };
        int[] torsoDefStats = { 1, 4, 8, 13, 20, 30, 40, 50 };
        int[] legsEvaStats = { 1, 3, 6, 10, 15, 22, 30, 40 };
        int[] weaponAtkStats = { 5, 15, 30, 50, 80, 120, 160, 200 };
        int[] weaponAccStats = { 4, 11, 21, 30, 37, 45, 52, 60 };

        switch (data.part)
        {
            case EquipmentData.PartType.Head:
                data.hp = headHpStats[gradeIndex];
                break;
            case EquipmentData.PartType.Torso:
                data.def = torsoDefStats[gradeIndex];
                break;
            case EquipmentData.PartType.Legs:
                data.eva = legsEvaStats[gradeIndex];
                break;
            case EquipmentData.PartType.Weapon:
                data.atk = weaponAtkStats[gradeIndex];
                data.acc = weaponAccStats[gradeIndex];
                break;
        }
    }
}