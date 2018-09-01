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
using System.Collections.Generic;

public class CharacterTileSprite : AgentTileSprite
{
    [System.Flags]
    public enum MovementStateFlags
    {
        NONE    = 0,
        JUMPING = 1
    };
    
    public Dictionary<int, CollisionStateFlags> CollisionStateMap   { get; private set; }
    public ControllerStateMachine ControllerState                   { get; private set; }

    public ControllerStateMachine.JoystickConfiguration JoystickConfiguration;
    public ControllerStateMachine.KeyboardConfiguration KeyboardConfiguration;

    public CollisionStateFlags CurrentCollisionState    { get; private set; }
    public MovementStateFlags CurrentMovementState      { get; private set; }
    public float JumpTimer                              { get; private set; }

    public float GroundSpeedX, Gravity, JumpHeight, DurationToJumpPeak;

    protected override void InitializeState()
    {
        CollisionStateMap       = new Dictionary<int, CollisionStateFlags>();
        ControllerState         = new ControllerStateMachine();
        CurrentCollisionState   = CollisionStateFlags.NONE;
        CurrentMovementState    = MovementStateFlags.NONE;
        JumpTimer               = 0.0f;
    }

    public override void ComputeVelocity()
    {
        ControllerState.Monitor(JoystickConfiguration, KeyboardConfiguration);
        
        if ( ( CurrentMovementState & MovementStateFlags.JUMPING ) == MovementStateFlags.JUMPING )
        {
            float initialVelocityY = JumpHeight * 2.0f / DurationToJumpPeak;

            VelocityY = Gravity * JumpTimer + initialVelocityY;
        }
        else
        {
            if (ControllerState.IsMovingLeft())
            {
                VelocityX = -GroundSpeedX;
            }
            else if (ControllerState.IsMovingRight())
            {
                VelocityX = GroundSpeedX;
            }
            else
            {
                VelocityX = 0.0f;
            }
        }

        if (
            ((CurrentCollisionState & CollisionStateFlags.FLOOR) == CollisionStateFlags.FLOOR) &&
            (Input.GetButtonDown(JoystickConfiguration.JumpButtonName) || Input.GetKeyDown(KeyboardConfiguration.JumpKeyCode))
        )
        {
            JumpTimer = 0.0f;

            CurrentMovementState |= MovementStateFlags.JUMPING;
        }

        JumpTimer += Time.deltaTime;
    }

    protected override void OnCollisionEntered(
        CollisionStateFlags collisionState,
        int pixelPenetrationDepth,
        AgentTileSprite other
    )
    {
        int otherInstanceID = other.GetInstanceID();

        if ( CollisionStateMap.ContainsKey(otherInstanceID) )
        {
            return;
        }

        CurrentCollisionState |= collisionState;

        if ( ( collisionState & CollisionStateFlags.FLOOR ) == CollisionStateFlags.FLOOR )
        {
            CurrentMovementState &= ~MovementStateFlags.JUMPING;
        }

        CollisionStateMap.Add(otherInstanceID, collisionState);
    }

    protected override void OnCollisionExited(AgentTileSprite other)
    {
        int otherInstanceID = other.GetInstanceID();

        if ( !CollisionStateMap.ContainsKey(otherInstanceID) )
        {
            return;
        }

        CollisionStateFlags collisionState = CollisionStateMap[otherInstanceID];

        if ( ( collisionState & CollisionStateFlags.FLOOR ) == CollisionStateFlags.FLOOR )
        {
            VelocityY = Gravity;
        }

        CurrentCollisionState &= ~collisionState;

        CollisionStateMap.Remove(otherInstanceID);
    }
};