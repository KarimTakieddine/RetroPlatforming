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

public class MovingPlatformTileSprite : AgentTileSprite
{
    public Vector2 Start, End;
    public float TraversalTime;

    public Vector2 CurrentStart { get; private set; }
    public Vector2 CurrentEnd   { get; private set; }

    protected override void InitializeState()
    {
        CurrentStart    = Start;
        CurrentEnd      = End;
    }

    public override void ComputeVelocity()
    {
        Vector2 position    = transform.position;
        Vector2 distance    = CurrentEnd - CurrentStart;
        Vector2 direction   = distance.normalized;
        float magnitude     = distance.magnitude;
        float coefficient   = (position - CurrentStart).magnitude / magnitude;

        if (coefficient >= 1.0f)
        {
            VelocityX = 0.0f;
            VelocityY = 0.0f;

            if ( ( CurrentStart == Start ) && CurrentEnd == End )
            {
                CurrentStart    = End;
                CurrentEnd      = Start;
            }
            else if ( ( CurrentStart == End ) && ( CurrentEnd == Start ) )
            {
                CurrentStart    = Start;
                CurrentEnd      = End;
            }
            
        }
        else
        {
            float speed = TraversalTime == 0.0f ? 0.0f : ( magnitude / TraversalTime );

            VelocityX = direction.x * speed;
            VelocityY = direction.y * speed;
        }
    }
};