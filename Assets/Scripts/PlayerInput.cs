using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    #region Movement
    // Benutze fuer alles was cheatanfällig ist getter, setter ???
    private float horizontal;
    public float Horizontal
    {
        get { return horizontal; }
        private set { horizontal = value;}
    }

    private float vertical;
    public float Vertical
    {
        get { return vertical; }
        private set { vertical = value; }
    }

    private bool isVerticalDown;
    public bool IsVerticalDown
    {
        get { return isVerticalDown; }
        private set { isVerticalDown = value; }
    }   

    private bool isDash;
    public bool IsDash
    {
        get { return isDash; }
        private set { isDash = value; }
    }

    private bool isJump;
    public bool IsJump
    {
        get { return isJump; }
        private set { isJump = value; }
    }
    #endregion

    #region Attacks

    private bool isAttackRight;
    public bool IsAttackRight
    {
        get { return isAttackRight; }
        set { isAttackRight = value; }
    }

    private bool isAttackLeft;
    public bool IsAttackLeft
    {
        get { return isAttackLeft; }
        set { isAttackLeft = value; }
    }

    private bool isAttackUp;
    public bool IsAttackUp
    {
        get { return isAttackUp; }
        set { isAttackUp = value; }
    }

    private bool isAttackDown;
    public bool IsAttackDown
    {
        get { return isAttackDown; }
        set { isAttackDown = value; }
    }

    #endregion

    private bool isPauseMenu;
    public bool IsPauseMenu
    {
        get { return isPauseMenu; }
        set { isPauseMenu = value; }
    }

    public bool canInput = true;

    public float mouseX;
    public float mouseY;
    public Vector2 mouseScroll;
    public bool mouse0;
    public bool mouse1;
    public bool mouse2;


    // Start is called before the first frame update
    private void Start()
    {
        
    }

    // Inputs werden jedem Frame abgefragt und gespeichert
    private void Update()
    {
        if (canInput)
        {
            mouse0 = Input.GetMouseButtonDown(0);
            mouse1 = Input.GetMouseButtonDown(1);
            mouse2 = Input.GetMouseButtonDown(2);

            mouseScroll = Input.mouseScrollDelta;

            mouseX = Input.GetAxis("MouseX");
            mouseY = Input.GetAxis("MouseY");

            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");

            isVerticalDown = Input.GetButtonDown("Vertical");
            isDash = Input.GetButtonDown("Dash");

            isAttackLeft = Input.GetButtonDown("AttackLeft");
            isAttackRight = Input.GetButtonDown("AttackRight");
            isAttackUp = Input.GetButtonDown("AttackUp");
            isAttackDown = Input.GetButtonDown("AttackDown");

            isJump = Input.GetButtonDown("Jump");

            isPauseMenu = Input.GetButtonDown("Escape");
        }
    }
}
