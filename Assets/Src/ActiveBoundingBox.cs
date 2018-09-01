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

public abstract class ActiveBoundingBox : BoundingBox
{
    public int PixelPositionX { get; private set; }
    public int PixelPositionY { get; private set; }

    public float RoundingRemainderX { get; private set; }
    public float RoundingRemainderY { get; private set; }

    public float VelocityX { get; protected set; }
    public float VelocityY { get; protected set; }

    protected abstract void ComputeVelocity();

    private void IncrementPositions()
    {
        PixelPositionX = PixelGeometry.MinimumX;
        PixelPositionY = PixelGeometry.MinimumY;

        float floatPixelIncrementX  = ( VelocityX * PixelsPerUnit * Time.deltaTime );
        int intPixelIncrementX      = (int)floatPixelIncrementX;
        RoundingRemainderX          += ( floatPixelIncrementX - intPixelIncrementX );

        PixelPositionX += intPixelIncrementX;

        if ( Mathf.Abs(RoundingRemainderX) >= 1.0f )
        {
            if (RoundingRemainderX < 0.0f)
            {
                --PixelPositionX;
            }
            else
            {
                ++PixelPositionX;
            }

            RoundingRemainderX = 0.0f;
        }

        float floatPixelIncrementY  = ( VelocityY * PixelsPerUnit * Time.deltaTime );
        int intPixelIncrementY      = (int)floatPixelIncrementY;
        RoundingRemainderY          += ( floatPixelIncrementY - intPixelIncrementY );

        PixelPositionY += intPixelIncrementY;

        if ( Mathf.Abs(RoundingRemainderY) >= 1.0f )
        {
            if (RoundingRemainderY < 0.0f)
            {
                --PixelPositionY;
            }
            else
            {
                ++PixelPositionY;
            }
            
            RoundingRemainderY = 0.0f;
        }
    }

	void Update ()
    {
        ComputeVelocity();
        IncrementPositions();
        SetPixelPosition(PixelPositionX, PixelPositionY);
        DrawGeometry();
	}
};