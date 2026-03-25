using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using THNetworkLibrary;
using THNetworkLibrary.Tools;
using THGameClient;

public class InputManager : IManager, IInputPoller
{
    private InputState _currentState;

    private static KeyCode[] ButtonInputs =
    {
        COUNT,
		
        KeyCode.A,				// ButtonName.Left
        KeyCode.D,				// ButtonName.Right
        KeyCode.W,				// ButtonName.Up
        KeyCode.S,				// ButtonName.Down

        KeyCode.Tab,			// ButtonName.Jump
        KeyCode.LeftControl,	// ButtonName.Guard

        KeyCode.Alpha1,			// ButtonName.Primary
        KeyCode.Alpha2,			// ButtonName.Secondary
        KeyCode.Alpha3,			// ButtonName.Tertiary	
        KeyCode.Alpha4,			// ButtonName.Class

        KeyCode.T,				// ButtonName.Tool
        KeyCode.Space			// ButtonName.Interact
    };

    public override ManagerInitializeErrorCode Initialize()
    {
        _currentState = new InputState();

        return ManagerInitializeErrorCode.NONE;
    }

    public override void OnDestroy()
    {

    }

    public override void Update(float delta)
    {
        for(int i = 0; i < InputState.BUTTON_NAME_COUNT; i++)
        {
            SetButtonState((ButtonName)i, ButtonInputs[i]);
        }
    }

    public override void FixedUpdate(float fixedDelta)
    {

    }

    private void SetButtonState(ButtonName button, KeyCode keyCode)
    {
        var state = _currentState.GetButtonState(button);

        if(Input.GetKey(keyCode))
        {
            switch (state)
            {
                case ButtonState.Released:
                case ButtonState.None:
                    state = ButtonState.Pressed;
                    break;
                case ButtonState.Pressed:
                    state = ButtonState.Held;
                    break;
            }
        }
        else
        {
            switch(state)
            {
                case ButtonState.Pressed:
                case ButtonState.Held:
                    state = ButtonState.Released;
                    break;
                case ButtonState.Released:
                    state = ButtonState.None;
                    break;
            }
        }

        _currentState.SetButtonState(button, state);
    }

    public ButtonState GetButtonState(ButtonName button)
    {
        return _currentState.GetButtonState(button);
    }

    public bool GetButtonDown(ButtonName button)
    {
        return _currentState.GetButtonDown(button);
    }

    public bool GetButtonUp(ButtonName button)
    {
        return _currentState.GetButtonUp(button);
    }

    public bool GetButton(ButtonName button)
    {
        return _currentState.GetButton(button);
    }

    public InputState GetInputState()
    {
        return _currentState;
    }
}
