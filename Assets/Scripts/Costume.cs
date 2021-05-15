using UnityEngine;

[CreateAssetMenu(fileName = "New Costume", menuName = "Scriptable/Costume")]
public class Costume : ScriptableObject
{
    public string title;
    public Sprite sprite;

    [NamedArray(typeof(Ability))]
    public Ability[] costumeAbilities;
    public Mesh charMesh;
    public GameObject dressVFX;
    public AudioClip indiviualDressAC;
}
