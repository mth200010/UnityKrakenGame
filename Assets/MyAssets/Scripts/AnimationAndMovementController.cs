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


    PlayerInput playerInput;
    CharacterController characterController;
    Animator animator;
    Dictionary<int, float> initialJumpVelocities = new Dictionary<int, float>();
    Dictionary<int, float> jumpGravities = new Dictionary<int, float>();
    Coroutine currentJumpResetRoutine = null;
    Coroutine currentGPResetRoutine = null;

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

    float gravity = -9.8f;
    float currentGravity;
    float groundedGravity = -.05f;   
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
        if ((characterController.isGrounded && isGroundPound) == false && isGroundPoundPressed)
        {            
            animator.SetBool(isGroundPoundHash, true);
            isGroundPound = true;            
            isGroundPoundAnimating = true;                                  

            // when grounded, deactivate GP
        }
        else if (isGroundPoundPressed == false && characterController.isGrounded
                && isGroundPoundAnimating)
        {
            StopCoroutine(currentGPResetRoutine);
            animator.SetBool(isGroundPoundHash, false);            
            isGroundPound = false;
            isGroundPoundAnimating = false;
        }
        else if (characterController.isGrounded && isGroundPoundPressed)
        {
            isGroundPound = false;
            animator.SetBool(isGroundPoundHash, false);
            StopCoroutine(currentGPResetRoutine);
        }
    }

    void handleJump()
    {        
        if (IsJumping == false && characterController.isGrounded && isJumpPressed)
        {            
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
        Debug.Log("Timer gravity Start" + gravity);
        Debug.Log("Timer Current gravity Start" + currentGravity);
        yield return new WaitForSeconds(.75f);
        gravity = currentGravity;
        Debug.Log("Timer gravity End" + gravity);
        Debug.Log("Timer Current gravity End" + currentGravity);

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
            if (currentGPResetRoutine != null)
            {
                StopCoroutine(currentGPResetRoutine);
            }
            // set animator here
            if (isJumpAnimating)
            {
                animator.SetBool(isJumpingHash, false);
                isJumpAnimating = false;
                currentJumpResetRoutine = StartCoroutine(jumpResetRoutine());
                if(jumpCount == 3)
                {
                    jumpCount = 0;
                    animator.SetInteger(jumpCountHash, jumpCount);
                }
            }
            currentMovement.y = groundedGravity;
            currentRunMovement.y = groundedGravity;           
        }
        else if (isFalling && isGroundPound == false)
        {
            float previousYVelocity = currentMovement.y;
            float newYVelocity = currentMovement.y + (jumpGravities[jumpCount] * fallMultiplier * Time.deltaTime);
            float nextYVelocity = Mathf.Max((previousYVelocity + newYVelocity) * 0.5f, -100.0f);
            currentMovement.y = nextYVelocity;
            currentRunMovement.y = nextYVelocity;
        }
        else if (isGroundPound && characterController.isGrounded == false)
        {
            Debug.Log("GP gravity activated");
            // turn off Gravity            
            float groundPoundGravity = 0.0f;
            // spin animation is in handleAnimation
            currentGravity = gravity;
            gravity = groundPoundGravity;
            // turn Gravity back on
            
            // set higher fallMultiplier to increase dropping down speed
            float previousYVelocity = currentMovement.y;
            float newYVelocity = currentMovement.y + (jumpGravities[jumpCount] * groundPoundMultiplier * Time.deltaTime);
            float nextYVelocity = Mathf.Max((previousYVelocity + newYVelocity) * 0.5f, -100.0f);
            currentMovement.y = nextYVelocity;
            currentRunMovement.y = nextYVelocity;

            currentGPResetRoutine = StartCoroutine(groundPoundReset());

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

    private void Update()
    {       
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
