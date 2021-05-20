using System;
using System.Collections;
using UnityEngine;

[System.Serializable]
public class CharacterController : MonoBehaviour
{

    public enum Costumes { skater = 0, ramen = 1, swimmer = 2 }
    public enum PermAbilities { jump = 0, fastFall = 1, doubleJump = 2, wallJump = 3, wallSlide = 4, climb = 5 }
    public enum Abilities { punch = 0, kick = 1, dash = 2, uppercut = 3, slide = 4, pullHook = 5, grapplingHook = 6, }
    public enum Weapons { grapplingHook = 0, pullHook = 1 }
    public enum Projectiles { shuriken = 0, sushi = 1 }
    public enum PlayerCollider
    {
        head = 0, handL = 1, handR = 2, spine = 3, uplegL = 4, legL = 5, footL = 6,
        upLegR = 7, legR = 8, footR = 9
    }
    public enum AudioClips { }
    PlayerInput input;
    Rigidbody rb;
    Mesh charMesh;
    Animator anim;

    #region Animations

    public void VelocityToAnim()
    {
        if (!abilities[(int)Abilities.dash].isPerforming && isGrounded)
        {
            anim.SetFloat("velocity_y", Mathf.Abs(rb.velocity.y));
            anim.SetFloat("velocity_x", Mathf.Abs(rb.velocity.x));
        }
        if (!isGrounded)
        {
            anim.SetFloat("velocity_y", 0);
            anim.SetFloat("velocity_x", 0);
        }
    }
    #endregion

    #region Raycasts


    public RaycastHit MouseToWorldPos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, 1000f, ~LayerMask.GetMask("Player")))
            return hitInfo;
        else return default;
        //  Debug.Log("MouseToWorldPos hit " + hitInfo.transform.name + " at point " + hitInfo.point);

    }
    bool IsGrounded()
    {
        Debug.DrawRay(transform.position, Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, groundDistance, LayerMask.GetMask("Terrain")))
        {
            // Debug.Log("RTaycast hit : " + hit.collider.gameObject.name);
            // Setze Abilities die nur 1 mal nach Bodenkontakt benutzt werden sollen auf true
            p_abilities[(int)PermAbilities.jump].canPerform = true;
            p_abilities[(int)PermAbilities.doubleJump].canPerform = true;
            abilities[(int)Abilities.uppercut].canPerform = true;
            abilities[(int)Abilities.dash].canPerform = true;
            abilities[(int)Abilities.slide].canPerform = true;
            anim.SetBool("isGrounded", true);
            return true;
        }
        else return false;
    }
    #endregion

    #region CharacterStats

    [Header("CharacterStats")]
    public float movementSpeed;
    public ForceMode moveMode;
    public float maxHealth;
    public float curHealth;
    public int damage;
    public float mana;


    public float downGravityForce;
    public float upGravityForce;

    public float dressDuration;
    // Ist AttackDamage noetig? Wird der Wert im Laufe des Spieles steigen, oder sind die Gegner simple 1-2-3 ... 20 (Boss) hits?
    // Agility, Armor, MagicArmor, ....
    #endregion

    #region References

    [Header("Collider")]
    [NamedArray(typeof(PlayerCollider))]
    public Collider[] colliders;
    //public Collider handAttackCol;
    //public Collider footAttackCol;


    // Referenz zu structs
    [Header("Costumes")]
    [NamedArray(typeof(Costumes))]
    public Costume[] costumes;
    public Costume activeCostume;
    //[Header("MovementAbilities")]
    //public MOVEMENTS movement;
    [Header("PermAbilities")]
    [NamedArray(typeof(PermAbilities))]
    public PermAbility[] p_abilities;

    [Header("DamageAbilities")]
    [NamedArray(typeof(Abilities))]
    public Ability[] abilities;
    public Ability[] activeAbilities;

    [Header("Weapons")]
    [NamedArray(typeof(Weapons))]
    public Transform weaponHolder;
    public GameObject[] weapons;
    [Header("Projectiles")]
    [NamedArray(typeof(Projectiles))]
    public GameObject[] projectiles;
    public Projectile activeProjectile;

    [Header("AudiClips")]
    public AudioSource audioSource;
    [NamedArray(typeof(AudioClips))]
    [Header("Debug")]

    public float groundDistance = 2f;
    public float wallDistance;
    public bool isGrounded;
    public bool canDoubleJump; // public zum debuggen

    public bool isMovementLocked;
    public bool isDashing;
    public bool isWallSliding;

    public Vector3 moveDirection;
    #endregion

    #region Character Status

    // Was passiert, wenn der Character seine Richtung spontan ändert (A <--> D)
    bool IsDirectionChange()
    {
        Vector3 rotation = transform.rotation.eulerAngles;

        if (rotation.y < 100f && input.Horizontal < 0f)
            return true;
        else if (rotation.y > 100f && input.Horizontal > 0f)
            return true;
        else
            return false;
    }

    // Wird vermutlich zu Break() / Bremsen()
    void StopMovement()
    {
        rb.velocity = Vector3.zero;
    }


    void Movement()
    {
        // Vielleicht Vector3 fuer 3-D Option?
        rb.AddForce(Vector2.right * movementSpeed * Time.deltaTime * input.Horizontal, moveMode);
    }

    // Zusätzliche Gravity um die Flowtyness in der Luft und an Abhängen zu verfeinern
    void ApplyAdditionalGravity()
    {
        if (rb.useGravity == true)
        {
            // Wenn Character fällt
            if (rb.velocity.y < -0.5f)
            {
                rb.AddForce(Vector3.down * downGravityForce * Time.deltaTime, ForceMode.VelocityChange);
            }
            // Wenn Character steigt
            if (rb.velocity.y > 1f)
            {
                rb.AddForce(Vector3.down * upGravityForce * Time.deltaTime, ForceMode.VelocityChange);
            }
        }
    }

    #endregion

    #region Abilities

    //
    //                                      HOW TO DO MULTIPLE STUFF IN COROUTINES
    //

    private delegate IEnumerator CoroutineDelegate();
    private CoroutineDelegate crd;

    // Coroutine fuer alle CD Abilities
    // Setzt canPerform, isPerforming und kuemmert sich um refresh
    IEnumerator CastCDAbility(IEnumerator coroutine, Ability ability)
    {
        if (!ability.isPerforming && ability.canPerform)
        {
            // Ruft andere Coroutinen / Methoden auf, die zu allen Attacken gehoren
            PlayAbilitySound(ability);
            StartCoroutine(UIManager.instance.UpdateAbilityCD(ability));

            ability.canPerform = false;
            ability.isPerforming = true;
            StartCoroutine(coroutine);
            yield return new WaitForSeconds(ability.duration);
            ability.isPerforming = false;
        }

        if (!ability.canPerform && isGrounded)      // isGroudned, damit abilities nicht 2 mal in Luft benutzt werden koennen
        {
            yield return new WaitForSeconds(ability.coolDown);
            ability.canPerform = true;
        }
    }

    // Coroutine fuer alle dauerhaften Abilities oder nicht durch cooldown resetbare Abilities wie WallSlide, Jump, Climb
    IEnumerator CastPermAbility(IEnumerator coroutine, PermAbility ability)
    {
        // Ruft andere Coroutinen / Methoden auf, die zu allen Attacken gehoren
        PlayPAbilitySound(ability);

        StartCoroutine(coroutine);
        yield return new WaitForSeconds(ability.duration);
    }

    // Rotiert die Spielerblickrichtung zur Mouseposition X
    public void RotatePlayerToMouse(Vector2 mousePos)
    {
        if (mousePos.x <= transform.position.x)
        {
            transform.rotation = Quaternion.Euler(0f, 270f, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        }
    }

    // Abspielen eines Soundeffects
    public void PlayAbilitySound(Ability ability)
    {
        audioSource.clip = ability.sound;
        audioSource.Play();
    }

    public void PlayPAbilitySound(PermAbility ability)
    {
        if (!audioSource.isPlaying)
        {
            audioSource.clip = ability.sound;
            audioSource.Play();
        }
    }

    // Fuegt nach Zeit Kraft hinzu : Wird beim Animator benutzt Physikzeitpunkt mit Animation zu vereinen
    private IEnumerator ForceWithDelay(float delay, Vector3 direction, float speed, ForceMode fm)
    {
        yield return new WaitForSeconds(delay);
        rb.AddForce(direction * speed, fm);
    }


    //
    //                  COROUTINEN                    ! Was die einzelnen Abilities machen !
    //
    #region Costume Abilities

    public IEnumerator SkaterAbility0CR()
    {
        throw new NotImplementedException();
    }

    public IEnumerator SkaterAbility1CR()
    {
        throw new NotImplementedException();
    }

    public IEnumerator SkaterAbility2CR()
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Damage Abilities 

    private IEnumerator PunchCR(Ability ability)
    {
        colliders[(int)PlayerCollider.handR].enabled = true;       // Collider entweder enablen oder go.SetActive oder isTrigger  ?!?!
        GameObject vfxInstance = Instantiate(ability.abilityVFX);
        yield return new WaitForSeconds(ability.duration);
        colliders[(int)PlayerCollider.handR].enabled = false;
        Destroy(vfxInstance);
    }

    private IEnumerator KickCR(Ability ability)
    {
        colliders[(int)PlayerCollider.footR].enabled = true;       // Collider entweder enablen oder go.SetActive oder isTrigger ?!?!
        yield return new WaitForSeconds(ability.duration);
    }

    private IEnumerator ActiveProjThrowCR(Ability ability)
    {
        GameObject instance = Instantiate(activeProjectile.projectileMesh);
        yield return new WaitForSeconds(activeProjectile.lifeTime);
        Destroy(instance);
    }

    private IEnumerator UppercutCR(Ability ability)
    {
        //  anim.SetBool("isAttacking", true);
        anim.SetBool("isUppercut", true);

        isMovementLocked = true;

        GameObject vfxInstance = Instantiate(ability.abilityVFX, transform);
        vfxInstance.transform.position += 2 * transform.forward;

        // velocity vor dem Movement???

        moveDirection = ability.xDirection * transform.right + ability.yDirection * transform.up + ability.zDirection * transform.forward;

        rb.AddForce(moveDirection * ability.playerMoveSpeed, ForceMode.VelocityChange);
        yield return new WaitForSeconds(ability.duration / 2f);
        rb.velocity = Vector3.zero;
        StartCoroutine(ForceWithDelay(0f, transform.up, ability.playerMoveSpeed, ForceMode.Impulse));

        yield return new WaitForSeconds(ability.duration);

        Destroy(vfxInstance);
        isMovementLocked = false;

        anim.SetBool("isUppercut", false);
        //  anim.SetBool("isAttacking", true);
    }

    private IEnumerator SlideCR(Ability ability)
    {
        isMovementLocked = true;

        GameObject vfxInstance = Instantiate(ability.abilityVFX, transform);

        moveDirection = ability.xDirection * transform.right + ability.yDirection * transform.up + ability.zDirection * transform.forward;
        rb.AddForce(moveDirection * ability.playerMoveSpeed, ForceMode.VelocityChange);

        yield return new WaitForSeconds(ability.duration);

        isMovementLocked = false;
    }

    IEnumerator DashCR(Ability ability)
    {
        anim.SetBool("isDash", true);

        moveDirection = ability.xDirection * transform.right + ability.yDirection * transform.up + ability.zDirection * transform.forward;

        isDashing = true;
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        DoVFXParent(ability.abilityVFX, transform.position - new Vector3(0f, 1.25f, 0.7f), transform.rotation);
        rb.AddForce(moveDirection * ability.playerMoveSpeed, ForceMode.VelocityChange);

        yield return new WaitForSeconds(ability.duration);

        isDashing = false;
        rb.useGravity = true;
        anim.SetBool("isDash", false);
    }

    IEnumerator PullHookCR(Ability ability)
    {

        GameObject instance = Instantiate(weapons[(int)Weapons.pullHook], weaponHolder);

        RaycastHit hitInfo;
        if (Physics.Raycast(weaponHolder.transform.position, weaponHolder.forward, out hitInfo, ability.maxRange, ~LayerMask.GetMask("Terrain")))
        {
            RotatePlayerToMouse(new Vector2(hitInfo.point.x, hitInfo.point.y));

            Debug.Log("Hook hat " + hitInfo.collider.gameObject.name + " getroffen !!");

            hitInfo.collider.transform.Translate(new Vector3((weaponHolder.transform.position - hitInfo.transform.position).x, 0f, 0f) * ability.projMoveSpeed);

            // hitInfo.collider.transform.Translate((new Vector3((weaponHolder.transform.position - hitInfo.transform.position).normalized.x,0f,0f) * abilities.hook.playerMoveSpeed));
        }
        yield return new WaitForSeconds(ability.duration);

        Destroy(instance);
    }

    public bool isHookHit;

    IEnumerator CheckForHit(float speed, Vector2 direction)
    {
        for (int i = 0; i < 100; i++)
        {
            yield return new WaitForSeconds(0.15f);
            if (isHookHit)
            {
                rb.velocity = Vector3.zero;
                rb.velocity = speed * direction;
                break;
            }
        }

    }

    IEnumerator GrapplingHookCR(Ability ability)
    {
        // Cache den RaycastHit mit Hilfe des GameManagers in der Variable hit          (rufe die Methode MouseToWorldPos() nur 1 mal auf !!! )
        RaycastHit hit = MouseToWorldPos();
        Vector2 hit2D = new Vector2(hit.point.x, hit.point.y);
        Vector2 weaponHolder2D = new Vector2(weaponHolder.position.x, weaponHolder.position.y);

        isHookHit = false;

        // Wenn der Raycast etwas getroffen hat, das einen Namen besitzt, erst dann mache überhaupt irgendwas
        if (hit.transform != null)
        {
            RotatePlayerToMouse(hit2D);

            isMovementLocked = true;

            weaponHolder.transform.LookAt(hit2D, Vector3.up);

            GameObject instance = Instantiate(weapons[(int)Weapons.grapplingHook], weaponHolder);

            StartCoroutine(CheckForHit(ability.playerMoveSpeed, (hit2D - weaponHolder2D).normalized));

            // t = s/v
            float duration = (hit2D - weaponHolder2D).magnitude / rb.velocity.magnitude;

            // Zerstöre bei Treffer und anziehen schneller, bei zu weit weg nach grapplinghook duration
            if ((transform.position - hit.transform.position).magnitude <= 1f)
            {
                yield return new WaitForEndOfFrame();
            }
            else
            {
                yield return new WaitForSeconds(ability.duration);
            }

            Destroy(instance);

            isMovementLocked = false;
        }

    }

    #endregion

    #region Permanent Abilities

    private IEnumerator JumpCR(PermAbility ability)
    {
        anim.SetBool("isJump", true);

        GameObject vfxInstance = Instantiate(ability.abilityVFX, transform.position, transform.rotation);

        moveDirection = ability.xDirection * transform.right + ability.yDirection * transform.up + ability.zDirection * transform.forward;
        rb.velocity = new Vector3(rb.velocity.x, 0f, 0f);
        //  rb.AddForce(moveDirection * ability.moveSpeed, ForceMode.VelocityChange);
        StartCoroutine(ForceWithDelay(ability.duration, moveDirection, ability.playerMoveSpeed, ForceMode.VelocityChange));
        // position wird justiert wenn Character gerigged und zb an Bein oder Arm gespawned

        yield return new WaitForEndOfFrame();

        Destroy(vfxInstance, 2f);
        anim.SetBool("isJump", false);
    }

    private IEnumerator DoubleJumpCR(PermAbility ability)
    {
        anim.SetBool("isDoubleJump", true);

        GameObject vfxInstance = Instantiate(ability.abilityVFX, transform.position, transform.rotation);

        moveDirection = ability.xDirection * transform.right + ability.yDirection * transform.up + ability.zDirection * transform.forward;
        ability.canPerform = false;
        rb.velocity = new Vector3(rb.velocity.x, 0f, 0f);

        // position wird justiert wenn Character gerigged und zb an Bein oder Arm gespawned

        rb.AddForce(moveDirection * ability.playerMoveSpeed, ForceMode.VelocityChange);
        yield return new WaitForEndOfFrame();

        Destroy(vfxInstance, 2f);

        anim.SetBool("isDoubleJump", false);
    }

    public IEnumerator FastFallCR(PermAbility ability)
    {
        rb.AddForce(Vector3.down * ability.playerMoveSpeed, ForceMode.VelocityChange);
        yield return new WaitForEndOfFrame();
    }

    public bool CheckWallCollision()
    {
        return Physics.Raycast(transform.position, transform.right, wallDistance);
    }

    private IEnumerator WallSlideCR(PermAbility ability)
    {
        bool walCol = Physics.Raycast(transform.position, transform.forward, wallDistance);
        Debug.DrawRay(transform.position, wallDistance * transform.forward, Color.green);

        Debug.Log("WallSlidePerforming!");
        rb.useGravity = false;
        ability.isPerforming = true;
        rb.velocity = new Vector3(0f, ability.playerMoveSpeed * ability.yDirection, 0f);
        abilities[(int)PermAbilities.doubleJump].canPerform = true;

        //if (!walCol && !abilities[(int)Abilities.dash].isPerforming)
        //{
        //    rb.useGravity = true;
        //    ability.isPerforming = false;
        //}
        yield return new WaitForEndOfFrame();
    }

    private IEnumerator WallJumpCR(PermAbility ability)
    {
        moveDirection = ability.xDirection * transform.right + ability.yDirection * transform.up + ability.zDirection * transform.forward;
        transform.Rotate(Vector3.up, 180f);
        rb.AddForce(ability.playerMoveSpeed * moveDirection, ForceMode.VelocityChange);

        yield return new WaitForEndOfFrame();
    }

    #endregion


    #endregion

    #region VFX

    // Erhalte die duration eines Effektes
    float VFXDuration(GameObject VFX)
    {
        return VFX.GetComponent<ParticleSystem>().main.duration;
    }

    // Spawne VFX, setze Parent und Zerstöre nach Zeit duration
    void DoVFXParent(GameObject VFX, Vector3 pos, Quaternion rot)
    {
        GameObject VFXInstance = Instantiate(VFX, pos, rot);
        VFXInstance.transform.SetParent(transform);
        Destroy(VFXInstance, VFXDuration(VFX) + 0.01f);
    }


    #endregion

    #region Interactions
    public void TakeDamage(int damage)
    {
        DamageTextController.CreateDamageText(damage.ToString(), transform, DamageTextController.damageTextEnemy);
        curHealth -= damage;
        if (curHealth <= 0)
            StartCoroutine(DieCR());

        // Tell UI Manager to Update 
        UIManager.instance.onUIchanged();
    }

    public IEnumerator DieCR()
    {
        DamageTextController.CreateDamageText("YOU DIED MY MAAAAN", transform, DamageTextController.damageTextEnemy);
        yield return new WaitForEndOfFrame();
        curHealth += maxHealth;

        // Tell UI Manager to Update 
        UIManager.instance.onUIchanged();
    }
    #endregion

    #region Input and Actions

    public void GetInputs()
    {
        // Checke ob Movement erlaubt
        if (!isMovementLocked)
        {
            if (IsDirectionChange() && !abilities[(int)Abilities.dash].isPerforming)
            {
                rb.velocity = new Vector2(-rb.velocity.x / 3f, rb.velocity.y);
                transform.Rotate(Vector3.up, 180f);
            }
            // Betrag notwendig? Oder eher Input != 0? Testen! TO DO
            // InputManager Snap austesten !  DONE ! ---> Snap lässt Geschwindigkeit fast nahezu beibehalten bei Richtungswechsel
            if (Mathf.Abs(input.Horizontal) > 0)
            {
                Movement();
            }
            if (input.IsJump)
            {
                if (isGrounded)
                {
                    StartCoroutine(CastPermAbility(JumpCR(p_abilities[(int)PermAbilities.jump]), p_abilities[(int)PermAbilities.jump]));
                }
                else if (p_abilities[(int)PermAbilities.wallSlide].isPerforming)
                {
                    StartCoroutine(CastPermAbility(WallJumpCR(p_abilities[(int)PermAbilities.wallJump]), p_abilities[(int)PermAbilities.wallJump]));
                }
                else if (p_abilities[(int)PermAbilities.doubleJump].canPerform)
                {
                    StartCoroutine(CastPermAbility(DoubleJumpCR(p_abilities[(int)PermAbilities.doubleJump]), p_abilities[(int)PermAbilities.doubleJump]));
                }
            }

            if (input.IsDash && abilities[(int)Abilities.dash].canPerform)
            {
                StartCoroutine(CastCDAbility(DashCR(abilities[(int)Abilities.dash]), abilities[(int)Abilities.dash]));
            }

            if (input.Vertical < 0f && input.IsVerticalDown)
            {
                if (isGrounded)
                    StopMovement();
                else
                    StartCoroutine(CastPermAbility(FastFallCR(p_abilities[(int)PermAbilities.fastFall]), p_abilities[(int)PermAbilities.fastFall]));
            }
            if (input.Vertical > 0f && input.IsVerticalDown)
            {
                //  if (isWallSliding) ;
                // StartCoroutine(CastCDAbility(ClimbCR(), abilities.fastFall));

            }
            //
            // Attacks
            //
            #region IJKL OLD MOVEMENT 
            //if (!isMouseInput)
            //{
            //    if (input.IsAttackLeft)
            //    {
            //        if (abilities.dash.isPerforming && abilities.uppercut.canPerform)
            //            StartCoroutine(CastCDAbility(UppercutCR(), abilities.uppercut));

            //        else
            //            StartCoroutine(CastCDAbility(PunchCR(), abilities.punch));
            //    }

            //    if (input.IsAttackRight)
            //    {
            //        if (abilities.dash.isPerforming && abilities.slide.canPerform)
            //            StartCoroutine(CastCDAbility(SlideCR(), abilities.slide));
            //        else
            //            StartCoroutine(CastCDAbility(KickCR(), abilities.kick));

            //    }
            //    if (input.IsAttackDown)
            //    {
            //        StartCoroutine(CastCDAbility(PullHookCR(), abilities.pullHook));
            //    }
            //    if (input.IsAttackUp)
            //    {
            //        StartCoroutine(CastCDAbility(GrapplingHookCR(), abilities.grapplingHook));
            //    }
            //}
            #endregion

            if (input.mouse0)
            {
                TakeDamage(10);
                //  StartCoroutine(activeCostume.ability0.title + "CR");
            }
            if (input.mouse1)
            {
                //  StartCoroutine(activeCostume.ability1.title + "CR");
            }
            if (input.mouse2)
            {
                //  StartCoroutine(activeCostume.ability2.title + "CR");
            }
            if (input.mouseScroll.magnitude > 0f)
            {
                StartCoroutine(CastCDAbility(KickCR(abilities[(int)Abilities.kick]), abilities[(int)Abilities.kick]));
                Debug.Log("Kick");
            }
            if (Mathf.Abs(input.mouseX) >= 0)
            {

            }
            if (Mathf.Abs(input.mouseY) >= 0)
            {

            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                StartCoroutine(CastCDAbility(UppercutCR(abilities[(int)Abilities.uppercut]), abilities[(int)Abilities.uppercut]));
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                StartCoroutine(CastCDAbility(GrapplingHookCR(abilities[(int)Abilities.grapplingHook]), abilities[(int)Abilities.grapplingHook]));
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                StartCoroutine(CastCDAbility(PullHookCR(abilities[(int)Abilities.pullHook]), abilities[(int)Abilities.pullHook]));
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                StartCoroutine(CastCDAbility(SlideCR(abilities[(int)Abilities.slide]), abilities[(int)Abilities.slide]));
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SwitchCostume(costumes[(int)Costumes.skater]);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SwitchCostume(costumes[(int)Costumes.ramen]);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SwitchCostume(costumes[(int)Costumes.swimmer]);
            }
        }
    }

    #endregion

    #region Costume Switch
    public void SwitchCostume(Costume newCostume)
    {
        StartCoroutine(SwitchCostumeCR(newCostume));
        UIManager.instance.onUIchanged();
    }



    public IEnumerator SwitchCostumeCR(Costume newCostume)
    {
        if (newCostume != activeCostume)
        {
            GameObject vfxInstance = Instantiate(newCostume.dressVFX, transform);
            //  Destroy(charMesh);

            activeCostume = newCostume;
            rend.sharedMesh = activeCostume.charMesh;
            activeAbilities = activeCostume.costumeAbilities;

            yield return new WaitForSeconds(dressDuration);

            Destroy(vfxInstance, dressDuration);
        }
    }
    #endregion

    #region Collisions
    //// Wenn Enemy Attack Collider den Player trifft
    public void OnTriggerEnter(Collider col)
    {

        if (col.CompareTag("EnemyAttack"))
        {
            Enemy enemy = col.gameObject.GetComponentInParent<Enemy>();

            if (!enemy.isDead)
            {
                TakeDamage(enemy.damage);
                DamageTextController.CreateDamageText(enemy.damage.ToString(), transform, DamageTextController.damageTextEnemy);
                // Wenn Leben null dann stirb! 
                if (enemy.health <= 0)
                {
                    enemy.StartCoroutine("DieCR");
                }
                Debug.Log("´Player took Damage from Enemy!");
            }
        }
    }

    public void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag("Slideable"))
        {
            Debug.Log(collision.collider);
            foreach (ContactPoint contact in collision.contacts)
            {
                Vector3 normal = contact.normal;
                Vector3 point = contact.point;
                Debug.Log("near slideable");
                rb.useGravity = false;
                rb.velocity = Vector3.zero;
                rb.MovePosition(transform.position + stickyForce * (Vector3.Cross(normal, transform.position - point)));
            }
        }
    }

    //public void NearSlideable(Collision collision)
    //{
    //   Collider[] slidecols = Physics.OverlapSphere(transform.position, 10f);
    //    foreach (Collider slidecol in slidecols)
    //    {
    //        if(slidecol.CompareTag("Slideable"))
    //        {
    //            Vector3 normal = collision.contacts[0].normal;
    //            Vector3 point = collision.contacts[0].point;
    //            Debug.Log("near slideable");
    //            rb.useGravity = false;
    //            rb.velocity = Vector3.zero;
    //            rb.MovePosition(transform.position + stickyForce * (Vector3.Cross(normal, transform.position - point)));
    //        }
    //    }
    //}


    public bool isSlideState;
    public float stickyForce = 100f;


    //public void OnCollisionStay(Collision collision)
    // { 
    //    foreach (ContactPoint contact in collision.contacts)
    //    {
    //        //Debug.Log(collision.contacts[0].otherCollider.name + collision.contacts[0].thisCollider.name);
    //        if (contact.otherCollider.CompareTag("Slideable"))
    //        {
    //             Debug.Log(contact.thisCollider.name + "hat Slideable getroffen");
    //             //  isSlideMode = true;
    //             Vector3 direction = (contact.point - transform.position);
    //             Debug.DrawLine(transform.position, contact.point, Color.red);
    //             rb.useGravity = false;
    //             rb.AddForce(stickyForce * direction);
    //            // SlideMode(collision);
    //        }
    //    }
    //}

    // public void SlideMode(Collision collision)
    // {

    //     // Debug.Log("Der andere Collider = " + contact.otherCollider.name + " Und dieser Collider ist : " + contact.thisCollider.name);

    //     Debug.Log("Stickt gerade!!!");
    //     //  transform.rotation = Quaternion.Euler(collision.contacts[0].normal);

    // }
    #endregion

    #region Visual Debug
    // Zum Debuggen und gucken von Walldistance und Grounddistance
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 directionWall = transform.TransformDirection(Vector3.forward) * wallDistance;
        Gizmos.DrawRay(transform.position, directionWall);

        Gizmos.color = Color.red;
        Vector3 directionGround = transform.TransformDirection(Vector3.down) * groundDistance;
        Gizmos.DrawRay(transform.position, directionGround);
    }
    #endregion

    SkinnedMeshRenderer rend;
    // Start is called before the first frame update
    void Start()
    {
        foreach (Ability ability in abilities)
        {
            ability.isPerforming = false;
            ability.canPerform = true;
        }

        curHealth = maxHealth;

        input = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
        rend = GetComponentInChildren<SkinnedMeshRenderer>();

        // Costume dependencies laden
        activeCostume = costumes[(int)Costumes.skater];          // Starte mit Skater Costume jedes mal
        activeAbilities = activeCostume.costumeAbilities;
        rend.sharedMesh = activeCostume.charMesh;

        // Aus dem Mesh Object die Components laden
        anim = GetComponentInChildren<Animator>();
        colliders = GetComponentsInChildren<Collider>();

        //Audio Source
        audioSource = GetComponent<AudioSource>();
        // Damit UI beim Start richtig angezeigt wird! Im Player kommt der Aufruf aufgrund der Instanzierungsreihenfolge sicher
        UIManager.instance.onUIchanged();
    }

    // Update is called once per frame
    void Update()
    {

        // Fuer double Jump detection, sonst nur wenn erster jump gemacht wurde
        isGrounded = IsGrounded();

        ApplyAdditionalGravity();

        // Wenn sich der Character in der Luft befindet, checke erst DANN ob er an der Wand haengt

        if (!isGrounded)
        {
            anim.SetBool("isGrounded", false);
            //if (CheckWallCollision())
            //{
            //    StartCoroutine(CastPermAbility(WallSlideCR(abilities[(int)PermAbilities.wallSlide]), abilities[(int)PermAbilities.wallSlide]));
            //}
        }

        VelocityToAnim();
        GetInputs();
    }

}


public class NamedArrayAttribute : PropertyAttribute
{
    public Type TargetEnum;
    public NamedArrayAttribute(Type TargetEnum)
    {
        this.TargetEnum = TargetEnum;
    }
}