using UnityEngine;

[CreateAssetMenu(fileName ="New Projectile", menuName = "Scriptable/Projectiles")]
public class Projectile : ScriptableObject
{
    public string title;
    public int damage;
    public float moveSpeed;
    public float maxRange;
    public float lifeTime;
    public float aoeRange;
    public int aoeDamage;
    public float coolDown;

    public GameObject projectileMesh;
    public GameObject projectileVFX;
}
