using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SlopeObstacleBoundingBox : ActiveBoundingBox
{
    public SpriteRenderer RendererComponent { get; private set; }

    protected override void InitializeLocalState()
    {
        RendererComponent = GetComponent<SpriteRenderer>();
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