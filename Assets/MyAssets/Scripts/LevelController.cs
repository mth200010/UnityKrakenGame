using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LevelController : MonoBehaviour
{

    PlayerInput playerInput;
    bool isQuitPressed;

    private void Awake()
    {
        playerInput = new PlayerInput();
        playerInput.CharacterControls.Jump.started += onQuit;
        playerInput.CharacterControls.Jump.canceled += onQuit;
    }       

    void onQuit(InputAction.CallbackContext context)
    {
        isQuitPressed = context.ReadValueAsButton();
    }

    void Update()
    {        
        if (isQuitPressed)
        {
            Debug.Log("Quit");
            Application.Quit();
        }
    }
}
