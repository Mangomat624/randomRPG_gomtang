using UnityEngine;

[CreateAssetMenu(fileName = "NewMonsterData", menuName = "Data/Monster Data")]
public class MonsterData : ScriptableObject
{
    public int stage;
    public string monsterName; 
    public int hp;
    public int atk;
    public int eva;
    public string des;
    // 특성(Trait)을 담을 변수 추가
}