using UnityEngine;

[CreateAssetMenu(fileName = "New Movement", menuName = "Scriptable/Movement")]
public class Movement : ScriptableObject
{
    public string title;
    public Sprite sprite;
    public float duration;  
    public float coolDown;

    public float xDirection;
    public float yDirection;
    public float zDirection;

    public Vector3 velocityBeforeMove;

    public float moveSpeed;
    public ForceMode forceMode;

    public GameObject abilityVFX;
    public AnimationClip animClip;
    public AudioClip sound;
}

