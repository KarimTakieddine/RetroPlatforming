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

public class BoundingBox : MonoBehaviour
{
    public struct Geometry
    {
        public int MinimumX { get; set; }
        public int MaximumX { get; set; }

        public int MinimumY { get; set; }
        public int MaximumY { get; set; }

        public Geometry(
            int minimumX,
            int minimumY,
            int maximumX,
            int maximumY
        )
        {
            MinimumX = minimumX;
            MaximumX = maximumX;
            MinimumY = minimumY;
            MaximumY = maximumY;
        }
    };

    public Geometry PixelGeometry { get; private set; }

    public float HalfWidth  { get; private set; }
    public float HalfHeight { get; private set; }

    public float InversePixelsPerUnit { get; private set; }

    public uint Width, Height;
    public uint PixelsPerUnit;

	private void ValidateInspectorState()
    {
        if (Width == 0)
        {
            Width = 1;
        }

        HalfWidth = Width * 0.5f;

        if (Height == 0)
        {
            Height = 1;
        }

        HalfHeight = Height * 0.5f;

        if (PixelsPerUnit == 0)
        {
            PixelsPerUnit = 1;
        }

        InversePixelsPerUnit = 1.0f / PixelsPerUnit;
    }

    private void ComputeGeometry()
    {
        Vector3 position = transform.position;

        int pixelMinimumX = (int)( ( position.x - HalfWidth ) * PixelsPerUnit );
        int pixelMinimumY = (int)( ( position.y - HalfWidth ) * PixelsPerUnit );

        PixelGeometry = new Geometry(
            pixelMinimumX,
            pixelMinimumY,
            pixelMinimumX + (int)( Width * PixelsPerUnit ),
            pixelMinimumY + (int)( Height * PixelsPerUnit )
        );
    }

    private void UpdatePosition()
    {
        Vector3 currentPosition = transform.position;

        transform.position = new Vector3(
            ( PixelGeometry.MinimumX * InversePixelsPerUnit ) + HalfWidth,
            ( PixelGeometry.MinimumY * InversePixelsPerUnit ) + HalfHeight,
            currentPosition.z
        );
    }

    public void DrawGeometry()
    {
        int pixelMinimumX = PixelGeometry.MinimumX;
        int pixelMaximumX = PixelGeometry.MaximumX;

        int pixelMinimumY = PixelGeometry.MinimumY;
        int pixelMaximumY = PixelGeometry.MaximumY;

        Vector3 bottomLeft  = new Vector3(pixelMinimumX, pixelMinimumY) * InversePixelsPerUnit;
        Vector3 topLeft     = new Vector3(pixelMinimumX, pixelMaximumY) * InversePixelsPerUnit;
        Vector3 topRight    = new Vector3(pixelMaximumX, pixelMaximumY) * InversePixelsPerUnit;
        Vector3 bottomRight = new Vector3(pixelMaximumX, pixelMinimumY) * InversePixelsPerUnit;

        Debug.DrawLine(
            bottomLeft,
            topLeft
        );

        Debug.DrawLine(
            topLeft,
            topRight
        );

        Debug.DrawLine(
            topRight,
            bottomRight
        );

        Debug.DrawLine(
            bottomRight,
            bottomLeft
        );
    }

    public void SetPixelPosition(
        int x,
        int y
    )
    {
        PixelGeometry = new Geometry(
            x,
            y,
            x + (int)( Width * PixelsPerUnit ),
            y + (int)( Height * PixelsPerUnit )
        );

        UpdatePosition();
    }

    public void SetWorldPosition(
        float x,
        float y
    )
    {
        Vector3 currentPosition = transform.position;

        transform.position = new Vector3(
            x,
            y,
            currentPosition.z
        );

        ComputeGeometry();
    }

    private void Awake()
    {
        ValidateInspectorState();
    }
};