using UnityEngine;
using System;

public class Joystick
{
    public static Vector2 GetInput2D
    (
        float   deadzone,
        string  horizontalAxis,
        string  verticalAxis
    )
    {
        Vector2 input           = new Vector2(Input.GetAxis(horizontalAxis), Input.GetAxis(verticalAxis));
        float   inputMagnitude  = input.magnitude;
        return  inputMagnitude < deadzone ? Vector2.zero : input.normalized * ((inputMagnitude - deadzone) / (1.0f - deadzone));
    }

    public static Vector2 GetSmoothInput2D
    (
        Vector2 input
    )
    {
        Vector2 smoothInput     = input;
        float   inputMagnitude  = input.magnitude;
        float   inputXComponent = input.x;
        float   inputYComponent = input.y;

        if (inputXComponent != 0.0f && inputYComponent != 0.0f)
        {
            float   maxComponent        = Math.Max(inputXComponent, inputYComponent);
            bool    xComponentNegative  = inputXComponent < 0.0f;
            bool    yComponentNegative  = inputYComponent < 0.0f;

            if (maxComponent == inputXComponent)
            {
                float adjustedYComponent    = Mathf.Sqrt((inputMagnitude * inputMagnitude) - (inputXComponent * inputXComponent));
                smoothInput                 = new Vector2(inputXComponent, yComponentNegative ? -adjustedYComponent : adjustedYComponent);
            }
            else if (maxComponent == inputYComponent)
            {
                float adjustedXComponent    = Mathf.Sqrt((inputMagnitude * inputMagnitude) - (inputYComponent * inputYComponent));
                smoothInput                 = new Vector2(xComponentNegative ? -adjustedXComponent : adjustedXComponent, inputYComponent);
            }
            else
            {
                smoothInput = smoothInput.normalized;
            }
        }
        return smoothInput;
    }
};