using UnityEngine;

public class TutorialStyleGiver : MonoBehaviour, IDamageable
{
    public PointManager pointManager;
    public void ApplyDamage(Weapon weapon, float amount)
    {
        pointManager.AddPoints(amount * 500);
    }

}
