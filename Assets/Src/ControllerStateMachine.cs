/* ############################################################################## *\

    Copyright (c) 2018 Karim Takieddine

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.

 \* ############################################################################## */

using UnityEngine;
using System;

public class ControllerStateMachine
{
	public enum ControlScheme
    {
        KEYBOARD = 0,
        JOYSTICK = 1
    };

    [Flags]
    public enum MovementState
    {
        NEUTRAL = 0,
        LEFT    = 1,
        RIGHT   = 1 << 1,
        UP      = 1 << 2,
        DOWN    = 1 << 3
    };

    [Serializable]
    public struct JoystickConfiguration
    {
        public string JumpButtonName, HorizontalAxisName, VerticalAxisName;
        public float Deadzone;
    }

    [Serializable]
    public struct KeyboardConfiguration
    {
        public KeyCode JumpKeyCode, LeftKeyCode, RightKeyCode, UpKeyCode, DownKeyCode;
    }

    public ControlScheme CurrentControlScheme   { get; private set; }
    public MovementState CurrentMovementState   { get; private set; }
    public Vector2 JoystickInput                { get; private set; } 

    public ControllerStateMachine()
    {
        CurrentControlScheme = ControlScheme.KEYBOARD;
        CurrentMovementState = MovementState.NEUTRAL;
    }

    public bool IsMonitoringKeyboard()
    {
        return CurrentControlScheme == ControlScheme.KEYBOARD;
    }

    public bool IsMonitoringJoystick()
    {
        return CurrentControlScheme == ControlScheme.JOYSTICK;
    }

    public bool IsMovingLeft()
    {
        return ( ( CurrentMovementState & MovementState.LEFT ) == MovementState.LEFT );
    }

    public bool IsMovingRight()
    {
        return ( ( CurrentMovementState & MovementState.RIGHT ) == MovementState.RIGHT );
    }

    public bool IsMovingUp()
    {
        return ( ( CurrentMovementState & MovementState.UP ) == MovementState.UP );
    }

    public bool IsMovingDown()
    {
        return ( ( CurrentMovementState & MovementState.DOWN ) == MovementState.DOWN );
    }

    public void MonitorJoystickInput(JoystickConfiguration configuration)
    {
        JoystickInput = Joystick.GetInput2D(
            configuration.Deadzone,
            configuration.HorizontalAxisName,
            configuration.VerticalAxisName
        );
        
        if (JoystickInput.magnitude != 0.0f)
        {
            if (IsMonitoringKeyboard())
            {
                CurrentControlScheme = ControlScheme.JOYSTICK;
            }

            float joystickInputX = JoystickInput.x;

            if (joystickInputX > 0)
            {
                if (IsMovingLeft())
                {
                    CurrentMovementState &= ~MovementState.LEFT;
                }

                CurrentMovementState |= MovementState.RIGHT;
            }
            else if (joystickInputX < 0)
            {
                if (IsMovingRight())
                {
                    CurrentMovementState &= ~MovementState.RIGHT;
                }

                CurrentMovementState |= MovementState.LEFT;
            }
            else if (joystickInputX == 0.0f)
            {
                CurrentMovementState &= ~ ( MovementState.LEFT | MovementState.RIGHT );
            }

            float joystickInputY = JoystickInput.y;

            if (joystickInputY > 0)
            {
                if (IsMovingDown())
                {
                    CurrentMovementState &= ~MovementState.DOWN;
                }

                CurrentMovementState |= MovementState.UP;
            }
            else if (joystickInputY < 0)
            {
                if (IsMovingUp())
                {
                    CurrentMovementState &= ~MovementState.UP;
                }

                CurrentMovementState |= MovementState.DOWN;
            }
            else if (joystickInputY == 0.0f)
            {
                CurrentMovementState &= ~( MovementState.UP | MovementState.DOWN );
            }
        }
        else
        {
            CurrentMovementState = MovementState.NEUTRAL;
        }
    }

    public void MonitorKeyboardInput(KeyboardConfiguration keyboardConfiguration)
    {
        if (Input.GetKey(keyboardConfiguration.RightKeyCode))
        {
            if (IsMonitoringJoystick())
            {
                CurrentControlScheme = ControlScheme.KEYBOARD;
            }

            if (IsMovingLeft())
            {
                CurrentMovementState &= ~MovementState.LEFT;
            }

            CurrentMovementState |= MovementState.RIGHT;
        }
        else if (Input.GetKey(keyboardConfiguration.LeftKeyCode))
        {
            if (IsMonitoringJoystick())
            {
                CurrentControlScheme = ControlScheme.KEYBOARD;
            }

            if (IsMovingRight())
            {
                CurrentMovementState &= ~MovementState.RIGHT;
            }

            CurrentMovementState |= MovementState.LEFT;
        }
        else if (!IsMonitoringJoystick())
        {
            CurrentMovementState &= ~ ( MovementState.LEFT | MovementState.RIGHT );
        }

        if (Input.GetKey(keyboardConfiguration.UpKeyCode))
        {
            if (IsMonitoringJoystick())
            {
                CurrentControlScheme = ControlScheme.KEYBOARD;
            }

            if (IsMovingDown())
            {
                CurrentMovementState &= ~MovementState.DOWN;
            }

            CurrentMovementState |= MovementState.UP;
        }
        else if (Input.GetKey(keyboardConfiguration.DownKeyCode))
        {
            if (IsMonitoringJoystick())
            {
                CurrentControlScheme = ControlScheme.KEYBOARD;
            }

            if (IsMovingUp())
            {
                CurrentMovementState &= ~MovementState.UP;
            }

            CurrentMovementState |= MovementState.DOWN;
        }
        else if (!IsMonitoringJoystick())
        {
            CurrentMovementState &= ~ ( MovementState.UP | MovementState.DOWN );
        }
    }

    public void Monitor(
        JoystickConfiguration joystickConfiguration,
        KeyboardConfiguration keyboardConfiguration
    )
    {
        MonitorJoystickInput(joystickConfiguration);
        MonitorKeyboardInput(keyboardConfiguration);
    }
};