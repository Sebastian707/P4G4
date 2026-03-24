using UnityEngine;

public class AddPointsButton : MonoBehaviour
{
    public PointManager pointManager;

    [Tooltip("How many points to add when clicked")]
    public int pointsToAdd = 50;

    public void AddPoints()
    {
        if (pointManager != null)
        {
            pointManager.AddPoints(pointsToAdd);
        }
    }
}