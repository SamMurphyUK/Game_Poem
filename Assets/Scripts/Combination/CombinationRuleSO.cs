using UnityEngine;

// One node in the emotion graph. Two bodies combine when either side lists the
// other as matchingType; both then take this node's result (sprite + type).
//
// Dark family: Passion+Desire -> PassionateDesire; Envy+Joy -> EnviousJoy;
// PassionateDesire+EnviousJoy -> Win.
// Light family: Passion+Desire -> DesiredPassion; Envy+Joy -> JoyousEnvy;
// DesiredPassion+JoyousEnvy -> Win.
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

    public bool CanCombineWith(CombinationRuleSO other)
    {
        if (other == null)
        {
            return false;
        }

        return matchingType == other || other.matchingType == this;
    }

    public CombinationRuleSO ResultWith(CombinationRuleSO other)
    {
        if (!CanCombineWith(other))
        {
            return null;
        }

        if (result != null)
        {
            return result;
        }

        return other.result;
    }
}