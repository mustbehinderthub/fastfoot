using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Permanent Ability", menuName = "Scriptable/PermanentAbility")]
public class PermAbility : ScriptableObject
{
    public string title;
    public Sprite sprite; 
    public float maxRange;
    public float duration;

    public bool isPerforming = false;
    public bool canPerform = true;

    public float xDirection;
    public float yDirection;
    public float zDirection;

    public float playerMoveSpeed;
    public ForceMode forceMode;

    public GameObject abilityVFX;
    public AnimationClip animClip;
    public AudioClip sound;
    public float audioDelay;
}
