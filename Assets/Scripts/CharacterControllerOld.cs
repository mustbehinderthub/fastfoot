using System.Collections;
using UnityEngine;

namespace old
{
    public class CharacterControllerOld : MonoBehaviour
    {
        #region Variables

        [Header("Variables")]
        PlayerInput input;
        Rigidbody rb;


        public float groundDistance = 2f;
        public float wallDistance;
        public float wallSlideSpeed;
        public float wallJumpSpeed;
        public float dashTime;
        public bool isGrounded;
        public bool canDoubleJump; // public zum debuggen

        public bool isMovementLocked;
        public bool isDashing;
        public bool isWallSliding;

        public float downGravityForce;
        public float upGravityForce;

        #endregion

        #region References

        [Header("Collider")]
        public Collider handAttackCol;
        public Collider footAttackCol;


        // Struct in der alle Effekt enthalten sind, Reigenfolge egal (besser als Liste)
        [System.Serializable]
        public struct VFX
        {
            public GameObject DashSmokeVFX;
            public GameObject JumpSmokeVFX;
            public GameObject punchVFX;
            public GameObject kickVFX;
            public GameObject activeProjectileVFX;
        }

        // Struct in der alle Abilities enthalten sind, Reigenfolge egal (besser als Liste)
        [System.Serializable]
        public struct ABILITIES
        {
            public Ability punch;
            public Ability kick;
            public Ability uppercut;
            public Ability slide;
            public Ability dash;
        }

        // Struct in der alle Projectiles enthalten sind, Reigenfolge egal (besser als Liste)
        [System.Serializable]
        public struct PROJECTILES
        {
            public Projectile simpleProjectile;
        }

        // Referenz zu structs
        [Header("VFX")]
        public VFX CharacterVFX;
        [Header("Abilities")]
        public ABILITIES characterAbilities;
        [Header("Projectiles")]
        public PROJECTILES characterProjectiles;
        public Projectile activeProjectile;
        #endregion

        #region CharacterStats

        [SerializeField]
        private float movementSpeed;
        public float MovementSpeed
        {
            get { return movementSpeed; }
            private set { movementSpeed = value; }
        }

        [SerializeField]
        private float dashSpeed;
        public float DashSpeed
        {
            get { return dashSpeed; }
            private set { dashSpeed = value; }
        }

        [SerializeField]
        private float health;
        public float Health
        {
            get { return health; }
            private set { health = value; }
        }

        [SerializeField]
        private float mana;
        public float Mana
        {
            get { return mana; }
            private set { mana = value; }
        }

        [SerializeField]
        private float jumpSpeed;
        public float JumpSpeed
        {
            get { return jumpSpeed; }
            private set { jumpSpeed = value; }
        }

        [SerializeField]
        private float fastFallSpeed;
        public float FastFallSpeed
        {
            get { return fastFallSpeed; }
            private set { fastFallSpeed = value; }
        }

        // Ist AttackDamage noetig? Wird der Wert im Laufe des Spieles steigen, oder sind die Gegner simple 1-2-3 ... 20 (Boss) hits?
        // Agility, Armor, MagicArmor, ....
        #endregion

        #region Character Movement

        bool IsGrounded()
        {
            if (Physics.Raycast(transform.position, Vector3.down, groundDistance))
            {
                canDoubleJump = true;
                return true;
            }
            else return false;
        }

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

        void StopMovement()
        {
            rb.velocity = Vector3.zero;
        }

        void Movement()
        {
            // Vielleicht Vector3 fuer 3-D Option?
            rb.AddForce(Vector2.right * MovementSpeed * Time.deltaTime * input.Horizontal, ForceMode.VelocityChange);
        }

        void Dash()
        {
            rb.velocity = Vector3.zero;

            StartCoroutine(WhileDashing());
            // Instantiate VFX
            DoVFXParent(CharacterVFX.DashSmokeVFX, transform.position - new Vector3(0f, 1.25f, 0.7f), transform.rotation);

            // Do movement stuff
            rb.AddForce(transform.forward * DashSpeed, ForceMode.VelocityChange);
        }

        IEnumerator WhileDashing()
        {
            isDashing = true;
            rb.useGravity = false;
            yield return new WaitForSeconds(dashTime);
            isDashing = false;
            rb.useGravity = true;
        }

        void Jump()
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, 0f);

            // position wird justiert wenn Character gerigged und zb an Bein oder Arm gespawned

            DoVFX(CharacterVFX.JumpSmokeVFX, transform.position - new Vector3(0f, 1.5f, 0f), transform.rotation);
            rb.AddForce(Vector2.up * JumpSpeed, ForceMode.VelocityChange);
        }

        void WallSlide()
        {
            bool walCol = Physics.Raycast(transform.position, transform.forward, wallDistance);

            if (walCol && !isWallSliding)
            {
                rb.useGravity = false;
                isWallSliding = true;
                rb.velocity = new Vector3(0f, wallSlideSpeed, 0f);
            }
            if (!walCol && !isDashing)
            {
                rb.useGravity = true;
                isWallSliding = false;
            }
        }

        void WallJump()
        {
            transform.Rotate(Vector3.up, 180f);
            rb.AddForce(wallJumpSpeed * transform.forward + jumpSpeed * transform.up, ForceMode.VelocityChange);
        }


        void FastFall()
        {
            rb.AddForce(Vector3.down * FastFallSpeed, ForceMode.VelocityChange);
        }

        void ApplyAdditionalGravity()
        {
            if (rb.useGravity == true)
            {
                if (rb.velocity.y < -0.5f)
                {
                    rb.AddForce(Vector3.down * downGravityForce * Time.deltaTime, ForceMode.VelocityChange);
                }

                if (rb.velocity.y > 1f)
                {
                    rb.AddForce(Vector3.down * upGravityForce * Time.deltaTime, ForceMode.VelocityChange);
                }
            }
        }

        #endregion

        #region Attacks

        //
        // Methode 1 - Jede Taste ist ein bestimmter, richtungsunabängiger Move
        //

        // Fehlt irgendwo eine Liste oder eine STRUCT mit allen Projectiles??!!!

        //
        //                                      HOW TO DO MULTIPLE STUFF IN COROUTINES
        //

        void Punch()
        {
            crd = SimplePunchCR;
            StartCoroutine(crd());
        }

        void Kick()
        {
            crd = SimpleKickCR;
            StartCoroutine(crd());
        }

        void Throw()
        {
            crd = ActiveProjThrowCR;
            StartCoroutine(crd());
        }

        void DashingUppercut()
        {
            crd = DashingUppercutCR;
            StartCoroutine(crd());
        }

        void DashingSlide()
        {
            crd = DashingSlideCR;
            StartCoroutine(crd());
        }


        private delegate IEnumerator CoroutineDelegate();
        private CoroutineDelegate crd;


        private IEnumerator SimplePunchCR()
        {
            handAttackCol.enabled = true;       // Collider entweder enablen oder go.SetActive oder isTrigger  ?!?!
            GameObject vfxInstance = Instantiate(characterAbilities.punch.abilityVFX);
            yield return new WaitForSeconds(characterAbilities.punch.duration);
            handAttackCol.enabled = false;
            Destroy(vfxInstance);
        }
        private IEnumerator SimpleKickCR()
        {
            footAttackCol.enabled = true;       // Collider entweder enablen oder go.SetActive oder isTrigger ?!?!
            yield return new WaitForSeconds(characterAbilities.kick.duration);
        }
        private IEnumerator ActiveProjThrowCR()
        {
            GameObject instance = Instantiate(activeProjectile.projectileMesh);
            yield return new WaitForSeconds(activeProjectile.lifeTime);
            Destroy(instance);
        }
        private IEnumerator DashingUppercutCR()
        {
            isMovementLocked = true;

            //  rb.AddForce(characterAbilities.uppercut.playerDirection * characterAbilities.uppercut.playerMoveSpeed, ForceMode.VelocityChange);
            DoVFX(characterAbilities.uppercut.abilityVFX, transform.position, transform.rotation);
            yield return new WaitForSeconds(characterAbilities.uppercut.duration);

            isMovementLocked = false;
        }
        private IEnumerator DashingSlideCR()
        {
            isMovementLocked = true;

            //   rb.AddForce(characterAbilities.slide.playerDirection * characterAbilities.slide.playerMoveSpeed, ForceMode.VelocityChange);
            DoVFX(characterAbilities.slide.abilityVFX, transform.position, transform.rotation);

            yield return new WaitForSeconds(characterAbilities.slide.duration);

            isMovementLocked = false;
        }

        //
        // Methode 1 - Jede Taste ist ein bestimmter, richtungsunabängiger Move
        //


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

        void DoVFX(GameObject VFX, Vector3 pos, Quaternion rot)
        {
            GameObject VFXInstance = Instantiate(VFX, pos, rot);
            Destroy(VFXInstance, VFXDuration(VFX) + 0.01f);
        }


        #endregion

        void TakeDamage(int damage)
        {
            Health -= damage;
        }


        // Start is called before the first frame update
        void Start()
        {
            input = GetComponent<PlayerInput>();
            rb = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {
            // Fuer double Jump detection, sonst nur wenn erster jump gemacht wurde
            isGrounded = IsGrounded();

            ApplyAdditionalGravity();

            // Wenn sich der Character in der Luft befindet, checke erst DANN ob er an der Wand haengt
            if (!isGrounded)
                WallSlide();

            // Teste, ob allgemeiner MovementLock gut ist oder was raus muss TO DO !!!!
            if (!isMovementLocked)
            {
                if (IsDirectionChange() && isDashing == false)
                {
                    rb.velocity = new Vector2(-rb.velocity.x, rb.velocity.y);
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
                        Jump();
                    }
                    else if (isWallSliding)
                    {
                        WallJump();
                        canDoubleJump = true;
                    }
                    else if (canDoubleJump)
                    {
                        Jump();
                        canDoubleJump = false;
                    }
                }

                if (input.IsDash && isDashing == false)
                {
                    Dash();
                }

                if (input.Vertical < 0f && input.IsVerticalDown)
                {
                    if (isGrounded)
                        StopMovement();
                    else
                        FastFall();
                }
                if (input.Vertical > 0 && input.IsVerticalDown)
                {
                    //if (isWallSliding) ;
                    //   Climb();

                }

                //
                // Attacks
                //

                if (input.IsAttackLeft)
                {
                    if (!isDashing)
                        Punch();
                    else
                        DashingUppercut();
                }

                if (input.IsAttackRight)
                {
                    if (!isDashing)
                        Kick();
                    else
                        DashingSlide();
                }
                if (input.IsAttackDown)
                {
                    // Do i dont know
                }
                if (input.IsAttackUp)
                {
                    Throw();
                }
            }
        }

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
    }
}
