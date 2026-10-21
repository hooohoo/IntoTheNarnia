using UnityEngine;

public static class GameHelper
{
    public static Vector3 GetHeightMapPos(Vector3 origin)
    {
        origin.y += 200;
        return Physics.Raycast(origin, Vector3.down, out var hit, Mathf.Infinity, ~LayerMask.GetMask("Player"), QueryTriggerInteraction.Ignore) ? hit.point : Vector3.zero;
    }
}
