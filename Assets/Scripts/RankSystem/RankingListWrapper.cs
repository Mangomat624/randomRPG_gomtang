// RankingListWrapper.cs
using System;
using System.Collections.Generic;

[Serializable]
public class RankingListWrapper
{
    // "items"는 우리가 임의로 정한 '배열의 이름'입니다.
    public List<RankingEntry> items;
}