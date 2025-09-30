using UnityEngine;

[CreateAssetMenu(fileName = "NewEquipmentData", menuName = "Data/Equipment Data")]
public class EquipmentData : ScriptableObject
{
    // PartType과 Grade의 정의를 이 스크립트 안에만 둡니다.
    public enum PartType { Head, Torso, Legs, Weapon }
    public enum Grade { F, D, C, B, A, S, SS, SSS, X }

    public PartType part;
    public Grade grade;
    public int hp, def, eva, atk, acc;
}