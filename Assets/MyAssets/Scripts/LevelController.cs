using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LevelController : MonoBehaviour
{

    PlayerInput playerInput;

    private void Start()
    {
        playerInput = new PlayerInput();
    }

    void Update()
    {
        
        if (playerInput.CharacterControls.Quit != null)
        {
            Application.Quit();
        }
    }
}
