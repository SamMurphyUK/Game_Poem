using UnityEngine;

[CreateAssetMenu(fileName = "CombinationRule_", menuName = "Game Poem/Combination Rule")]
public class CombinationRuleSO : ScriptableObject
{
    public CombinationRuleSO matchingType;
    public CombinationRuleSO result;
    public Sprite sprite;

    public CombinationRuleSO GetMatchingType()
    {
        return matchingType;
    }

    public CombinationRuleSO GetResult()
    {
        return result;
    }
}