using System.Collections;
using UnityEngine;

public class DamageTextController : MonoBehaviour
{
    public static DamageText damageTextPlayer;
    public static DamageText damageTextEnemy;

    private static GameObject canvas;

    public static void Initialize()                     // wird in PlayerStats aufgerufen und initalisiert !!!!!
    {
        canvas = GameObject.Find("DamageTextCanvas");
        damageTextPlayer = Resources.Load<DamageText>("DamageText/DamageTextParentPlayer");                // laedt aus bestimmtem Path!!!! vorsicht !!!!
        damageTextEnemy = Resources.Load<DamageText>("DamageText/DamageTextParentEnemy");                // laedt aus bestimmtem Path!!!! vorsicht !!!!
    }

    public static void CreateDamageText(string text, Transform location, DamageText damageText)
    {
        DamageText instance = Instantiate(damageText);
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(location.position);     // convert objects world position to screen position

        instance.transform.SetParent(canvas.transform, false);
        instance.transform.position = screenPosition;

        instance.SetText(text);
    }
}
