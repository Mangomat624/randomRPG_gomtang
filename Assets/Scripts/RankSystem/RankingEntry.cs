// RankingEntry.cs
using System;

[Serializable]
public class RankingEntry
{
    // JSON 키와 변수 이름을 정확히 일치시켜야 합니다.
    public int rankingId;
    public string playerName;
    public long score; // int의 최대값을 초과할 수 있으므로 long이 안전합니다.
    public string createdAt;
    public int isEnd;
}