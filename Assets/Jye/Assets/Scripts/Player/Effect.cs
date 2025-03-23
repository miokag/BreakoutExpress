using UnityEngine;
using System.Collections;
using BreakoutExpress;

public abstract class Effect
{
    public float Duration { get; protected set; }
    public float StartTime { get; protected set; }

    public Effect(float duration)
    {
        Duration = duration;
        StartTime = Time.time;
    }

    public abstract void ApplyEffect(PlayerController player);
    public abstract void RemoveEffect(PlayerController player);
}

public class SlowEffect : Effect
{
    private float slowAmount;

    public SlowEffect(float duration, float slowAmount) : base(duration)
    {
        this.slowAmount = slowAmount;
    }

    public override void ApplyEffect(PlayerController player)
    {
        player.WalkSpeed *= slowAmount;
        player.RunSpeed *= slowAmount;
    }

    public override void RemoveEffect(PlayerController player)
    {
        player.WalkSpeed /= slowAmount;
        player.RunSpeed /= slowAmount;
    }
}

public class PushbackEffect : Effect
{
    private Vector3 pushDirection;
    private float pushForce;

    public PushbackEffect(float duration, Vector3 direction, float force) : base(duration)
    {
        pushDirection = direction.normalized;
        pushForce = force;
    }

    public override void ApplyEffect(PlayerController player)
    {
        player.ApplyForce(pushDirection * pushForce);
    }

    public override void RemoveEffect(PlayerController player)
    {
        // No cleanup needed for pushback
    }
}