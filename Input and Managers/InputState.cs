using System;
using System.Collections.Generic;
using System.Text;

namespace THNetworkLibrary.Tools
{
	public enum ButtonState
    {
        None,
        Pressed,
        Held,
        Released
    }
	
    public enum ButtonName : ushort
    {
        Left,
        Right,
        Up,
        Down,
        
        Jump,
        Guard,

        Primary,
        Secondary,
        Tertiary,
        Class,

        Tool,
        Interact,
        COUNT,
    }

    /// <summary>
    /// Collection of various input states
    /// </summary>
    public class InputState
    {
        public const int BUTTON_NAME_COUNT = (int)ButtonName.COUNT;

        protected ButtonState[] Buttons = new ButtonState[BUTTON_NAME_COUNT];

        public void SetButtonState(ButtonName button, ButtonState state)
        {
            Buttons[(int)button] = state;
        }

        public bool GetButtonDown(ButtonName button)
        {
            return Buttons[(int)button] == ButtonState.Pressed;
        }

        public bool GetButtonUp(ButtonName button)
        {
            return Buttons[(int)button] == ButtonState.Released;
        }

        public bool GetButtonHeld(ButtonName button)
        {
            return Buttons[(int)button] == ButtonState.Held;
        }

        public bool GetButton(ButtonName button)
        {
            return Buttons[(int)button] == ButtonState.Held || Buttons[(int)button] == ButtonState.Pressed;
        }

        public ButtonState GetButtonState(ButtonName button)
        {
            return Buttons[(int)button];
        }
    }
}
