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

public abstract class AgentTileSprite : TileSprite
{
    [Flags]
    public enum CollisionLayerFlags
    {
        ART         = 0,
        OBSTACLE    = 1,
        CHARACTER   = 1 << 1
    };

    [Flags]
    public enum CollisionStateFlags
    {
        NONE        = 0,
        ENTERED     = 1,
        EXITED      = 1 << 1,
        LEFT_WALL   = 1 << 2,
        RIGHT_WALL  = 1 << 3,
        CEILING     = 1 << 4,
        FLOOR       = 1 << 5
    }

    public float VelocityX          { get; protected set; }
    public float VelocityY          { get; protected set; }
    public int PixelPositionX       { get; set; }
    public int PixelPositionY       { get; set; }
    public float PixelRemainderX    { get; set; }
    public float PixelRemainderY    { get; set; }

    public CollisionLayerFlags CollisionLayerFlag;

    public bool IsObstacle()
    {
        return ( CollisionLayerFlag & CollisionLayerFlags.OBSTACLE ) == CollisionLayerFlags.OBSTACLE;
    }

    public bool IsCharacter()
    {
        return ( CollisionLayerFlag & CollisionLayerFlags.CHARACTER ) == CollisionLayerFlags.CHARACTER;
    }

    public abstract void ComputeVelocity();

    public void IncrementPositions()
    {
        PixelPositionX = PixelGeometry.MinimumX;
        PixelPositionY = PixelGeometry.MinimumY;

        float pixelIncrementX   = PixelsPerUnit * VelocityX * Time.deltaTime;
        int iPixelIncrementX    = (int)pixelIncrementX;

        PixelRemainderX += ( pixelIncrementX - iPixelIncrementX );

        float pixelIncrementY   = PixelsPerUnit * VelocityY * Time.deltaTime;
        int iPixelIncrementY    = (int)pixelIncrementY;

        PixelRemainderY += ( pixelIncrementY - iPixelIncrementY );

        PixelPositionX += iPixelIncrementX;

        if ( Mathf.Abs(PixelRemainderX) >= 1.0f )
        {
            if (PixelRemainderX > 0)
            {
                PixelPositionX++;
            }
            else if (PixelRemainderX < 0)
            {
                PixelPositionX--;
            }

            PixelRemainderX = 0.0f;
        }

        PixelPositionY += iPixelIncrementY;

        if ( Mathf.Abs(PixelRemainderY) >= 1.0f )
        {
            if (PixelRemainderY > 0)
            {
                PixelPositionY++;
            }
            else if (PixelRemainderY < 0)
            {
                PixelPositionY--;
            }

            PixelRemainderY = 0.0f;
        }
    }

    public void SetPixelPosition(
        int pixelPositionX,
        int pixelPositionY
    )
    {
        Vector3 position            = transform.position;
        float inversePixelsPerUnit  = 1.0f / PixelsPerUnit;
        transform.position          = new Vector3(
            pixelPositionX * inversePixelsPerUnit + ( Width * 0.5f ),
            pixelPositionY * inversePixelsPerUnit + ( Height * 0.5f ),
            position.z
        );
    }

    public void UpdatePosition()
    {
        SetPixelPosition(PixelPositionX, PixelPositionY);
    }

    protected virtual void OnCollisionEntered(CollisionStateFlags collisionState, int pixelPenetrationDepth, AgentTileSprite other) { }

    public void SignalCollisionEntered(CollisionStateFlags collisionState, int pixelPenetrationDepth, AgentTileSprite other)
    {
        OnCollisionEntered(collisionState, pixelPenetrationDepth, other);
    }

    protected virtual void OnCollisionExited(AgentTileSprite other) { }

    public void SignalCollisionExited(AgentTileSprite other)
    {
        OnCollisionExited(other);
    }
};