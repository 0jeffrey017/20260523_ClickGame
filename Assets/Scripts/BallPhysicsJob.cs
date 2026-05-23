using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct BallPhysicsJob : IJobParallelFor
{
    // 1. These are our writable outputs for the current thread index
    public NativeArray<float3> Positions;
    public NativeArray<float3> Velocities;
    
    // 2. These are the ReadOnly copies used to look up the other balls safely
    [ReadOnly] public NativeArray<float3> InputPositions;
    [ReadOnly] public NativeArray<float3> InputVelocities;
    
    [ReadOnly] public float Radius;
    [ReadOnly] public float3 BoxMin;
    [ReadOnly] public float3 BoxMax;
    [ReadOnly] public float DeltaTime;
    [ReadOnly] public float Gravity;

    public void Execute(int index)
    {
        // Read initial state from our input copies
        float3 pos = InputPositions[index];
        float3 vel = InputVelocities[index];

        // Apply Gravity and Movement
        vel.y -= Gravity * DeltaTime;
        pos += vel * DeltaTime;

        float targetDist = Radius * 2f;
        float targetDistSq = targetDist * targetDist;

        // Loop through the safe [ReadOnly] arrays
        for (int i = 0; i < InputPositions.Length; i++)
        {
            if (i == index) continue;

            float3 otherPos = InputPositions[i];
            float3 toOther = otherPos - pos;
            float distSq = math.lengthsq(toOther);

            if (distSq < targetDistSq)
            {
                float dist = math.sqrt(distSq);
                float3 normal = dist > 0.001f ? toOther / dist : new float3(0, 1, 0);

                float3 otherVel = InputVelocities[i];
                float3 relativeVel = otherVel - vel;
                float velAlongNormal = math.dot(relativeVel, normal);

                if (velAlongNormal < 0)
                {
                    float restitution = 0.8f; 
                    float impulseMagnitude = -(1f + restitution) * velAlongNormal / 2f;

                    vel -= impulseMagnitude * normal;
                    
                    float overlap = targetDist - dist;
                    pos -= normal * (overlap * 0.5f);
                }
            }
        }

        // Box Boundary Collisions
        if (pos.x - Radius < BoxMin.x) { pos.x = BoxMin.x + Radius; vel.x = math.abs(vel.x) * 1.2f; }
        else if (pos.x + Radius > BoxMax.x) { pos.x = BoxMax.x - Radius; vel.x = -math.abs(vel.x) * 1.2f; }

        if (pos.y - Radius < BoxMin.y) { pos.y = BoxMin.y + Radius; vel.y = math.abs(vel.y) * 1.2f; }
        else if (pos.y + Radius > BoxMax.y) { pos.y = BoxMax.y - Radius; vel.y = -math.abs(vel.y) * 1.2f; }

        if (pos.z - Radius < BoxMin.z) { pos.z = BoxMin.z + Radius; vel.z = math.abs(vel.z) * 1.2f; }
        else if (pos.z + Radius > BoxMax.z) { pos.z = BoxMax.z - Radius; vel.z = -math.abs(vel.z) * 1.2f; }

        // Write safely to our unique index
        Positions[index] = pos;
        Velocities[index] = vel;
    }
}