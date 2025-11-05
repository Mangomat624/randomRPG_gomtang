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
    public Sprite idle;
    public Sprite fight;
    public Sprite died;
}