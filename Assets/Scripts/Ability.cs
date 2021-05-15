using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName ="New Ability", menuName = "Scriptable/Ability")]
public class Ability : PermAbility
{
    public int damage;
    public float projMoveSpeed;
    public float coolDown;
}
