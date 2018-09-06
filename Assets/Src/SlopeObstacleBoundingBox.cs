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