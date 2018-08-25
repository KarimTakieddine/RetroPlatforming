using UnityEngine;

public class CharacterTileSprite : AgentTileSprite
{
    public ControllerStateMachine ControllerState { get; private set; }

    public ControllerStateMachine.JoystickConfiguration JoystickConfiguration;
    public ControllerStateMachine.KeyboardConfiguration KeyboardConfiguration;

    public float SpeedX, SpeedY;

    protected override void Awake()
    {
        base.Awake();
        ControllerState = new ControllerStateMachine();
    }

    public override void ComputeVelocity()
    {
        ControllerState.Monitor(JoystickConfiguration, KeyboardConfiguration);
        
        if (ControllerState.IsMovingLeft())
        {
            VelocityX = -SpeedX;
        }
        else if (ControllerState.IsMovingRight())
        {
            VelocityX = SpeedX;
        }
        else
        {
            VelocityX = 0.0f;
        }

        if (ControllerState.IsMovingUp())
        {
            VelocityY = SpeedY;
        }
        else if (ControllerState.IsMovingDown())
        {
            VelocityY = -SpeedY;
        }
        else
        {
            VelocityY = 0.0f;
        }
    }
};