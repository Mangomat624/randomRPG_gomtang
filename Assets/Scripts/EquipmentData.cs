using UnityEngine;

[CreateAssetMenu(fileName = "NewEquipmentData", menuName = "Data/Equipment Data")]
public class EquipmentData : ScriptableObject
{
    public enum PartType { Head, Torso, Legs, Weapon }
    public enum Grade { F, D, C, B, A, S, SS, SSS, X }

    public PartType part;
    public Grade grade;
    public int hp;
    public int def;
    public int eva;
    public int atk;
    public int acc;
}