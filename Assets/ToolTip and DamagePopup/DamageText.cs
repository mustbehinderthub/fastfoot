using System.Collections;
using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    public Animator animator;
    private TMP_Text damageText;

    void OnEnable()                             // da sonst aus irgendeinen grund SetText vor Start ausgefuerht wird und zu nUll reference fuehrt
    {
        AnimatorClipInfo[] clipinfo = animator.GetCurrentAnimatorClipInfo(0);       // reference to animator clip array? first object is our animation

        Destroy(gameObject, clipinfo[0].clip.length - 0.05f);           // -0.01f da sonst animation wieder von vorne beginnt aka Zahl wird wieder angezeigt
        damageText = GetComponentInChildren<TMP_Text>();      // Reference zum objetc, dass animator traegt???? EASY?????!
    }

    public void SetText(string text)
    {
        damageText.text = text;
    }
}
