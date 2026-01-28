/*using Godot;
using System;
using System.Transactions;

public partial class enemy : Node2D
{
    public float direction = 0;
    public float baseDecelaration = 1;
    public float baseAccelaration = 10;
    public float baseGrip = 1;
    public float baseBrake = 50;
    public float tractionForce = 1;
    public float overflowSpeed = 20;
    public float speedSoftcap = 1000;
    public float grip = 0;
    Vector2 velocity = Vector2.Zero;
    float PI = 3.141592653589793f;

    public override void _PhysicsProcess(double delta)
    {
        //Position += velocity * (float)delta;
        //Rotation = -direction;
    }

    public override void _Process(double delta)
    {
        int rightKey = 0;
        int upKey = 0;

        if (Input.IsActionPressed("Right"))
        {
            rightKey += 1;
        }
        if (Input.IsActionPressed("Left"))
        {
            rightKey -= 1;
        }
        if (Input.IsActionPressed("Up"))
        {
            upKey += 1;
        }
        if (Input.IsActionPressed("Down"))
        {
            upKey -= 1;
        }

        Vector2 undirectedVelocity = new Vector2(0, 0);
        float currentAngle = vectorToDirection(velocity);
        float currentSpeed = absoluteVector(velocity);
        float currentDirectionalSpeed = Math.Abs(currentSpeed * (float)Math.Cos(currentAngle - direction));
        Vector2 directionalVelocity = directionToVector(direction, (float)-currentDirectionalSpeed);

        if (upKey != -1)
        {
            Vector2 projection = directionalVelocity * (directionalVelocity.Dot(velocity) / (directionalVelocity.Length() * directionalVelocity.Length()));
            undirectedVelocity = velocity - projection;
            float testGrip = undirectedVelocity.Length() / velocity.Length();
            grip = float.IsNaN(testGrip) ? 0 : testGrip;

            if (undirectedVelocity.Length() < velocity.Length())
            {
                if (undirectedVelocity.Length() * 1.1 > baseGrip)
                {
                    accelerate(-undirectedVelocity / 15);
                    accelerate(-undirectedVelocity.Angle(), baseGrip / 3);
                }
                else
                {
                    accelerate(-undirectedVelocity / 2);
                }
            }
        }
        else
        {
            accelerate(currentAngle, Math.Max(-currentSpeed, -baseBrake) / 4);
            grip = 1;
        }

        if (rightKey != 0)
        {
            float turnSpeed = currentSpeed < speedSoftcap ? currentSpeed / speedSoftcap : 1;
            direction = (direction + 2f * PI - 3f * turnSpeed * rightKey / 180f * PI * (0.5f + grip)) % (2f * PI);
        }
        if (upKey == 1)
        {
            float acceleration = currentDirectionalSpeed < speedSoftcap ? baseAccelaration : baseAccelaration * (overflowSpeed - (currentDirectionalSpeed - speedSoftcap)) / (overflowSpeed * 3);
            accelerate(direction, acceleration);
        }
    }
    public void accelerate(float angle, float speed)
    {
        velocity = velocity - directionToVector(angle, (float)speed);
    }
    public void accelerate(Vector2 vector)
    {
        velocity = velocity + vector;
    }
    public Vector2 directionToVector(float angle, float length = 1)
    {
        return new Vector2((float)Math.Sin(angle), (float)Math.Cos(angle)) * length;
    }
    public float vectorToDirection(Vector2 vector)
    {
        return (float)Math.Atan2(vector.X, vector.Y) + PI;
    }
    public float absoluteVector(Vector2 vector) {
        return (float)Math.Sqrt(vector.X*vector.X + vector.Y*vector.Y);
    }
}*/