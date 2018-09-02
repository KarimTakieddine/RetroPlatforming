﻿/* ############################################################################## *\

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

public class ControllerBoundingBox : ActiveBoundingBox
{
    private Dictionary<int, CollisionStateFlags> m_collisionStateMap;

    public Dictionary<int, CollisionStateFlags> CollisionStateMap
    {
        get
        {
            if (m_collisionStateMap == null)
            {
                m_collisionStateMap = new Dictionary<int, CollisionStateFlags>();
            }

            return m_collisionStateMap;
        }
    }

    public float HorizontalVelocity;
    public float JumpVelocity;

    public float VelocityIncrementX { get; private set; }
    public float VelocityIncrementY { get; private set; }
    public float JumpTimer          { get; private set; }

    public bool IsJumping { get; private set; }

    protected override void SetBehaviourFlags()
    {
        BehaviourFlag = BehaviourFlags.MOVING | BehaviourFlags.CONTROLLER;
    }

    public override void ComputeVelocity()
    {
        if (IsJumping)
        {
            VelocityY = -10.0f * JumpTimer + JumpVelocity + VelocityIncrementY;
        }
        else
        {
            if (Input.GetKey(KeyCode.D))
            {
                VelocityX = HorizontalVelocity;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                VelocityX = -HorizontalVelocity;
            }
            else
            {
                VelocityX = 0.0f;
            }

            VelocityX += VelocityIncrementX;
        }

        if ( Input.GetButtonDown("Jump") && ( ( CollisionState & CollisionStateFlags.GROUND ) == CollisionStateFlags.GROUND ) )
        {
            JumpTimer = 0.0f;
            IsJumping = true;
        }

        JumpTimer += Time.deltaTime;
    }

    public override void OnBoundingBoxEnter(CollisionStateFlags collisionState, int pixelPenetration, ActiveBoundingBox other)
    {
        if ( CollisionStateMap.ContainsKey(other.GetInstanceID()) )
        {
            return;
        }

        VelocityIncrementX = other.VelocityX;
        VelocityIncrementY = other.VelocityY;

        if ((collisionState & CollisionStateFlags.GROUND) == CollisionStateFlags.GROUND)
        {
            IsJumping = false;
        }

        CollisionState |= collisionState;

        CollisionStateMap.Add(other.GetInstanceID(), collisionState);
    }

    public override void OnBoundingBoxExit(ActiveBoundingBox other)
    {
        int otherInstanceID = other.GetInstanceID();

        if ( !CollisionStateMap.ContainsKey(otherInstanceID) )
        {
            return;
        }

        CollisionStateFlags collisionState = CollisionStateMap[otherInstanceID];

        if ( (collisionState & CollisionStateFlags.GROUND ) == CollisionStateFlags.GROUND )
        {
            VelocityY = -10.0f;
        }

        CollisionState &= ~collisionState;

        CollisionStateMap.Remove(otherInstanceID);
    }
};