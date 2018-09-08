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

[RequireComponent(typeof(SpriteRenderer))]
public class SlopeObstacleBoundingBox : ActiveBoundingBox
{
    public SpriteRenderer RendererComponent { get; private set; }

    public List<List<bool>> TextureBitmap { get; private set; }

    protected override void InitializeLocalState()
    {
        RendererComponent   = GetComponent<SpriteRenderer>();
        TextureBitmap       = new List<List<bool>>();

        Sprite sprite = RendererComponent.sprite;

        if (!sprite)
        {
            return;
        }

        Texture2D texture   = sprite.texture;
        int textureWidth    = texture.width;
        int textureHeight   = texture.height;
        int textureSize     = textureWidth * textureHeight;

        if (textureSize == 0)
        {
            return;
        }

        TextureBitmap.Capacity  = textureHeight;
        Color32[] pixels        = texture.GetPixels32();

        for (int i = 0; i < textureHeight; ++i)
        {
            List<bool> bitmapRow = new List<bool>();

            for (int j = 0; j < textureWidth; ++j)
            {
                int colorIndex  = i * textureWidth + j;
                Color32 pixel   = pixels[colorIndex];

                bitmapRow.Add(pixel.a != 0x00);
            }

            TextureBitmap.Add(bitmapRow);
        }
    }

    protected override void SetBehaviourFlags()
    {
        BehaviourFlag = BehaviourFlags.COLLIDER | BehaviourFlags.SLOPE;	
	}

    public override void ComputeVelocity()
    {
        VelocityX = 0.0f;
        VelocityY = 0.0f;
    }
};