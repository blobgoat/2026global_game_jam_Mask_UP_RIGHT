using UnityEngine;

public enum ActionType
{
    Move,
    Wait,
    Collide,
    None
}

[System.Serializable]

// for plan actions should only have move and wait. No collide.
public struct PlanAction
{
    public ActionType type;
    public Vector2Int v; // direction for Move
}


