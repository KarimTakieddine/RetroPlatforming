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

public abstract class AgentTileSprite : TileSprite
{
    public float VelocityX          { get; protected set; }
    public float VelocityY          { get; protected set; }
    public int PixelPositionX       { get; private set; }
    public int PixelPositionY       { get; private set; }
    public float PixelRemainderX    { get; private set; }
    public float PixelRemainderY    { get; private set; }

    protected abstract void ComputeVelocity();

    private void IncrementPositions()
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

        if (PixelRemainderX >= 1.0f)
        {
            PixelPositionX++;
            PixelRemainderX = 0.0f;
        }

        PixelPositionY += iPixelIncrementY;

        if (PixelRemainderY >= 1.0f)
        {
            PixelPositionY++;
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

    protected override void Update()
    {
        base.Update();
        ComputeVelocity();
        IncrementPositions();
        // TODO: ResolveCollsions();
        SetPixelPosition(PixelPositionX, PixelPositionY);
    }
};