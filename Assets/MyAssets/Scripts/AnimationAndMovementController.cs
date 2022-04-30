using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationAndMovementController : MonoBehaviour
{
    [SerializeField] float walkingSpeed = 1.0f;
    [SerializeField] float runMultiplier = 3.0f;
    [SerializeField] float rotationFactorPerframe = 10.0f;
    [SerializeField] float maxJumpHeight = 10.0f;
    [SerializeField] float maxJumpTime = 0.5f;
    [SerializeField] float fallMultiplier = 2.0f;
    [SerializeField] float groundPoundMultiplier = 3.0f;
    [SerializeField] ParticleSystem groundPoundVFX = null;
    [SerializeField] TrailRenderer cloudJumpVFX = null;


    PlayerInput playerInput;
    CharacterController characterController;
    Animator animator;
    Dictionary<int, float> initialJumpVelocities = new Dictionary<int, float>();
    Dictionary<int, float> jumpGravities = new Dictionary<int, float>();
    Coroutine currentJumpResetRoutine = null;
    Coroutine currentGPResetRoutine = null;
    Vector3 currentTransform;

    int isWalkingHash;
    int isRunningHash;
    int isJumpingHash;
    int jumpCount = 0;
    int jumpCountHash;
    int isGroundPoundHash;
            
    Vector2 currentMovementInput;
    Vector3 currentMovement;
    Vector3 currentRunMovement;
    Vector3 currentJumpMovement;    

    bool isMovementPressed = false;
    bool isRunPressed = false;
    bool isJumpPressed = false;
    bool isGroundPoundPressed = false;
    bool IsJumping = false;
    bool isJumpAnimating = false;    
    bool isGroundPound = false;
    bool isGroundPoundAnimating = false;
    bool isGrounded = true;
    bool isSpinning = false;

    float gravity = -9.8f;
    float currentGravity;
    float groundedGravity = -5f;   
    float initialJumpVelocity;        
     
   
    // Awake is called earlier than Start in Unity's event cycle
    private void Awake()
    {        
        // initially set reference variables
        playerInput = new PlayerInput();
        characterController = GetComponent<CharacterController>();       
        animator = GetComponent<Animator>();

        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
        isJumpingHash = Animator.StringToHash("isJumping");
        jumpCountHash = Animator.StringToHash("jumpCount");
        isGroundPoundHash = Animator.StringToHash("isGroundPound");

        playerInput.CharacterControls.Move.started += onMovementInput;
        playerInput.CharacterControls.Move.canceled += onMovementInput;
        playerInput.CharacterControls.Move.performed += onMovementInput;
        playerInput.CharacterControls.Run.started += onRun;
        playerInput.CharacterControls.Run.canceled += onRun;
        playerInput.CharacterControls.Jump.started += onJump;
        playerInput.CharacterControls.Jump.canceled += onJump;
        playerInput.CharacterControls.GroundPound.started += onGroundPound;
        playerInput.CharacterControls.GroundPound.canceled += onGroundPound;

        setupJumpVariables();        
    }
    
    void setupJumpVariables()
    {
        float timeToApex = maxJumpTime / 2;

        gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);              
        float secondJumpGravity = (-2 * (maxJumpHeight + 2)) / Mathf.Pow((timeToApex * .95f), 2);
        float thirdJumpGravity = (-2 * (maxJumpHeight + 4)) / Mathf.Pow((timeToApex * .75f), 2);

        initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;
        float secondJumpInitialVelocity = (2 * (maxJumpHeight + 2)) / (timeToApex * .95f);        
        float thirdJumpInitialVelocity = (2 * (maxJumpHeight + 4)) / (timeToApex * .75f);              
        
        initialJumpVelocities.Add(1, initialJumpVelocity);
        initialJumpVelocities.Add(2, secondJumpInitialVelocity);
        initialJumpVelocities.Add(3, thirdJumpInitialVelocity);

        jumpGravities.Add(0, gravity);
        jumpGravities.Add(1, gravity);
        jumpGravities.Add(2, secondJumpGravity);
        jumpGravities.Add(3, thirdJumpGravity);
    }


    void handleGroundPound()
    {        
        // activated GP if press P while not grounded
        if (characterController.isGrounded == false && isGroundPoundPressed)
        {
            Debug.Log("GP activated");
            isGroundPound = true;
            animator.SetBool(isGroundPoundHash, true);            
            isGroundPoundAnimating = true;           
            StartCoroutine(groundPoundReset());
        }
       /* else if (isGroundPound == false && isGroundPoundPressed && characterController.isGrounded)
        {
            Debug.Log("GP De-activated");
            if (currentGPResetRoutine != null)
            {
                StopCoroutine(currentGPResetRoutine);
            }
            groundPoundVFX.Stop();
        }*/
        else if (characterController.isGrounded && isGroundPoundAnimating)
        {
            Debug.Log("GP De-activated");
            groundPoundVFX?.Play();            
            animator.SetBool(isGroundPoundHash, false);
            isGroundPound = false;
            isGroundPoundAnimating = false;
            if (currentGPResetRoutine != null)
            {
                StopCoroutine(currentGPResetRoutine);
            }                        
        }         
        
        
        /*else if (characterController.isGrounded && isGroundPoundPressed)
        {
            isGroundPound = false;
            animator.SetBool(isGroundPoundHash, false);
            if (currentGPResetRoutine != null)
            { 
                StopCoroutine(currentGPResetRoutine); 
            }
        }*/
    }

    void handleJump()
    {        
        if (IsJumping == false && characterController.isGrounded && isJumpPressed)
        {
            Debug.Log("isJumpPressed");
            cloudJumpVFX.enabled = true;
            if ( jumpCount < 3 && currentJumpResetRoutine != null)
            {                
                StopCoroutine(currentJumpResetRoutine);                
            }           
           
            animator.SetBool(isJumpingHash, true);
            isJumpAnimating = true;
            IsJumping = true;
            jumpCount += 1;
            animator.SetInteger(jumpCountHash, jumpCount);
            currentMovement.y = initialJumpVelocities[jumpCount] * .5f;
            currentRunMovement.y = initialJumpVelocities[jumpCount] * .5f;            

            // get current jump position
            // currentJumpMovement.y = currentMovement.y;

        }
        
        else if (isJumpPressed == false && IsJumping && characterController.isGrounded)
        {            
            IsJumping = false;
        }
        
    }
   
    IEnumerator jumpResetRoutine()
    {
        yield return new WaitForSeconds(.5f);
        jumpCount = 0;
    }

    IEnumerator groundPoundReset()
    {
        isSpinning = true;
        yield return new WaitForSeconds(.5f);
        isSpinning = false;

    }

    void onGroundPound(InputAction.CallbackContext context)
    {
        isGroundPoundPressed = context.ReadValueAsButton();
    }

    void onJump (InputAction.CallbackContext context)
    {
        isJumpPressed = context.ReadValueAsButton();
    }

    void onRun(InputAction.CallbackContext context)
    {
        isRunPressed = context.ReadValueAsButton();
    }

    void onMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        currentMovement.x = currentMovementInput.x * walkingSpeed;
        currentMovement.z = currentMovementInput.y * walkingSpeed;
        currentRunMovement.x = currentMovementInput.x * walkingSpeed * runMultiplier;
        currentRunMovement.z = currentMovementInput.y * walkingSpeed * runMultiplier;
        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }

    void handleRotation()
    {
        Vector3 positionToLookAt;
        // the change in position our character should point to
        positionToLookAt.x = currentMovement.x;
        positionToLookAt.y = 0.0f;
        positionToLookAt.z = currentMovement.z;
        // the current rotation of our character
        Quaternion currentRotation = transform.rotation;
        // create a new rotation based on where the player is currently pressing
       if (isMovementPressed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerframe * Time.deltaTime);
        }
        
    }
   
    void handleAnimation()
    {
        bool isWalking = animator.GetBool(isWalkingHash);
        bool isRunning = animator.GetBool(isRunningHash);
       
        if (isMovementPressed && isWalking == false)
        {
            animator.SetBool(isWalkingHash, true);
        }
        else if (isMovementPressed == false && isWalking)
        {
            animator.SetBool(isWalkingHash, false);
        }

        if ((isMovementPressed && isRunPressed) && isRunning == false)
        {
            animator.SetBool(isRunningHash, true);
        }
        else if ((isMovementPressed == false || isRunPressed == false) && isRunning)
        {
            animator.SetBool(isRunningHash, false);
        }               
       

        /*if (isGroundPoundPressed && isGroundPoundAnimating == false 
                && characterController.isGrounded == false)
        {
            Debug.Log("GP animation activated");
            animator.SetBool(isGroundPoundHash, true);
        }
        else if (isGroundPoundAnimating && characterController.isGrounded)
        {
            Debug.Log("GP animating turned off");
            animator.SetBool(isGroundPoundHash, false);
        }*/
    }

    void handleGravity()
    {        
        bool isFalling = currentMovement.y <= 0.0f || isJumpPressed == false;
                
        if (characterController.isGrounded && isGroundPound == false)
        {            
           
            // set animator here
            if (isJumpAnimating)
            {
                animator.SetBool(isJumpingHash, false);
                isJumpAnimating = false;               
                if(jumpCount == 3)
                {
                    jumpCount = 0;
                    animator.SetInteger(jumpCountHash, jumpCount);
                }
            }
            currentMovement.y = groundedGravity * Time.deltaTime;
            currentRunMovement.y = groundedGravity * Time.deltaTime;           
        }
        else if (isFalling && isGroundPound == false)
        {
            cloudJumpVFX.enabled = false;
            float previousYVelocity = currentMovement.y;
            float newYVelocity = currentMovement.y + (jumpGravities[jumpCount] * fallMultiplier * Time.deltaTime);
            float nextYVelocity = Mathf.Max((previousYVelocity + newYVelocity) * 0.5f, -100.0f);
            currentMovement.y = nextYVelocity;
            currentRunMovement.y = nextYVelocity;
        }
        else if (characterController.isGrounded == false && isGroundPound && isSpinning == true)
        {
            Debug.Log("Pausing");
            currentTransform = GetComponent<Transform>().position;
            currentMovement.y = currentTransform.y - (currentTransform.y * .8f); 
        } 
        else if (characterController.isGrounded == false  && isSpinning == false && isGroundPound)
        {
            Debug.Log("Un-Pausing");            
            float previousYVelocity = currentMovement.y;
            float newYVelocity = currentMovement.y + (jumpGravities[jumpCount] * groundPoundMultiplier * Time.deltaTime);
            float nextYVelocity = Mathf.Max((previousYVelocity + newYVelocity) * 0.5f, -100.0f);
            currentMovement.y = nextYVelocity;
            currentRunMovement.y = nextYVelocity;
        }
        else
        {
            float previousYVelocity = currentMovement.y;
            float newYVelocity = currentMovement.y + (gravity * Time.deltaTime);
            float nextYVelocity = (previousYVelocity + newYVelocity) * 0.5f;
            currentMovement.y = nextYVelocity;
            currentRunMovement.y = nextYVelocity;
        }        

    }

    private void FixedUpdate()
    {
        //isGrounded = characterController.SimpleMove(currentMovement);
       
        handleRotation();
        handleAnimation();
       
        if (isRunPressed)
        {
            characterController.Move(currentRunMovement * Time.deltaTime);
        }
        else
        {
            characterController.Move(currentMovement * Time.deltaTime);
        }

        handleGravity();
        handleJump();
        handleGroundPound();
        
    }

   
    

    private void OnEnable()
    {
        // enable character controls action map
        playerInput.CharacterControls.Enable();
    }

    private void OnDisable()
    {
        playerInput.CharacterControls.Disable();
    }

}
