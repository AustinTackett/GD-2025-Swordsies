using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class FighterBehaviour : MonoBehaviour
{
    public float speed = 300;
    public float attackRange = 4;
    public float attackRadius = 1;
    public LifeMeterBehaviour LifeMeter;
    public PauseMenuBehaviour PauseMenu;

    private bool isDead;

    private Animator animator;
    private Rigidbody2D rigidBody;
    private Vector2 moveDir;
    private Collider2D selfCollider;
    private int selfExcludedLayerMask;
    private Vector3 startingPosition;
    private Quaternion startingRotation;
    private AudioSource hitAudio;
    private AudioSource deathAudio;
    private AudioSource attack1Audio;
    private AudioSource pauseMenuAudio;

    void Awake()
    {
        isDead = false;
        moveDir = Vector2.zero;
        selfExcludedLayerMask = ~(1 << gameObject.layer);
        startingPosition = transform.position;
        startingRotation = transform.rotation;
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody2D>();
        selfCollider = GetComponent<Collider2D>();
        AudioSource[] audioSources = GetComponents<AudioSource>();
        hitAudio = audioSources[0];
        deathAudio = audioSources[1];
        attack1Audio = audioSources[2];
        pauseMenuAudio = audioSources[3];
    }

    void FixedUpdate()
    {
        if(isDead)
        {
            rigidBody.linearVelocity = Vector3.zero;
            return;
        }

        Vector3 velocity = moveDir.normalized * speed * Time.fixedDeltaTime;

        if(animator.GetBool("Attack1") || animator.GetBool("Hit"))
        {
            rigidBody.linearVelocity = Vector3.zero;
        }
        else
        {
            rigidBody.linearVelocity = velocity;
            FaceMovementDir(velocity);
        }

        if(velocity == Vector3.zero)
        {
            animator.SetBool("Run", false);
        }
        else
        {
            animator.SetBool("Run", true);
        }
    }

    public void FaceMovementDir(Vector3 velocity)
    {
        if(velocity.x > 0)
            transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (velocity.x < 0)
            transform.rotation = Quaternion.Euler(0, 180, 0);
    }

    public void OnAttack1()
    {
        Vector3 playerCenter = selfCollider.bounds.center;
        Vector3 direction;
        if (transform.rotation.eulerAngles.y == 180)
            direction = Vector3.left;
        else    
            direction = Vector3.right;
         
        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            playerCenter, 
            attackRadius, 
            direction, 
            attackRange, 
            selfExcludedLayerMask
        );

        foreach(RaycastHit2D hit in hits)
        {
            GameObject collisionGO = hit.collider.gameObject;
            if(collisionGO.TryGetComponent<FighterCPUBehaviour>(out FighterCPUBehaviour opponent))
            {
                opponent.LifeMeter.RemoveHeart();
                if(opponent.LifeMeter.lifeCount > 0)
                    opponent.OnHit();
                else   
                    opponent.OnDeath();
            }
        }
    }

    public void OnEndAttack1()
    {
        animator.SetBool("Attack1", false);
    }


    public void OnHit()
    {
        animator.SetBool("Hit", true);
        // Make sure if attack is interrupted the attacking state is released
        animator.SetBool("Attack1", false);

        hitAudio.Play();
    }

    public void OnEndHit()
    {
        animator.SetBool("Hit", false);
    }

    public void OnAttack1Input()
    {
        if(isDead) return;
        if(!enabled) return;

        if(!animator.GetBool("Attack1") && !animator.GetBool("Hit"))
        {
            animator.SetBool("Attack1", true);
            attack1Audio.Play();
            OnAttack1();
        }
    }

    public void OnDashInput()
    {
        // Needs to be implemented
    }


    public void OnDeath()
    {
        if(isDead) return;

        deathAudio.Play();
        isDead = true;
        animator.SetBool("Attack1", false);
        animator.SetBool("Hit", false);
        animator.SetBool("Run", false);
        animator.SetTrigger("Death");      
    }

    public void OnPauseMenu()
    {
        Debug.Log(PauseMenu.isActiveAndEnabled);
        if(!PauseMenu.isActiveAndEnabled)
        {
            pauseMenuAudio.Play();
            PauseMenu.Open();
        }
    }

    public void OnMoveInput(InputValue value)
    {
        moveDir = value.Get<Vector2>();
    }

    public void ResetPosition()
    {
        isDead = false;
        transform.position = startingPosition;
        transform.rotation = startingRotation;
        rigidBody.linearVelocity = Vector3.zero;
        animator.SetBool("Attack1", false);
        animator.SetBool("Hit", false);
        animator.SetBool("Run", false);
    }

    public void ResetAnimation()
    {
        animator.SetBool("Attack1", false);
        animator.SetBool("Hit", false);
        animator.SetBool("Run", false);
    }

    public void ResetVelocity()
    {
        rigidBody.linearVelocity = Vector3.zero;
    }
}