using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct BallPhysicsJob : IJobParallelFor
{
    public NativeArray<float3> Positions;
    public NativeArray<float3> Velocities;
    public NativeArray<int> PinBeHit;
    [ReadOnly] public NativeArray<float3> PinPositions;
    
    [ReadOnly] public NativeArray<float3> InputPositions;
    [ReadOnly] public NativeArray<float3> InputVelocities;
    
    [ReadOnly] public float Radius;
    [ReadOnly] public float PinRadius;
    [ReadOnly] public float3 BoxMin;
    [ReadOnly] public float3 BoxMax;
    [ReadOnly] public float DeltaTime;
    [ReadOnly] public float Gravity;

    public unsafe void Execute(int index)
    {
        float3 pos = InputPositions[index];
        float3 vel = InputVelocities[index];
        
        vel.y -= Gravity * DeltaTime;
        pos += vel * DeltaTime;
        
        float targetDist = Radius * 2f;
        float targetDistSq = targetDist * targetDist;

        for (int i = 0; i < InputPositions.Length; i++)
        {
            if (i == index) continue;

            float3 otherPos = InputPositions[i];
            float3 toOther = pos - otherPos; // 方向：自球 -> 他球
            float distSq = math.lengthsq(toOther);

            if (distSq < targetDistSq)
            {
                float dist = math.sqrt(distSq);
                float3 normal = dist > 0.001f ? -toOther / dist : new float3(0, 1, 0);

                float3 otherVel = InputVelocities[i];
                float3 relativeVel = otherVel - vel; 
                float velAlongNormal = math.dot(relativeVel, normal);
                
                if (velAlongNormal < 0)
                {
                    float restitution = 0.8f; 
                    float impulseMagnitude = -(1f + restitution) * velAlongNormal / 2f;
                    if (impulseMagnitude < 0.05f)
                    {
                        impulseMagnitude = 0.1f;
                    }
                    vel -= impulseMagnitude * normal;
                    
                    float overlap = targetDist - dist;
                    pos += normal * (overlap * 0.6f); 
                }
            }
        }
        
        float pinDist = PinRadius + Radius;
        float pinDistSq = pinDist * pinDist;

        for (var i = 0; i < PinPositions.Length; i++)
        {
            var pinPos = PinPositions[i];
            float3 toPin = pinPos - pos;
            float distSq = math.lengthsq(toPin);

            if (distSq < pinDistSq)
            {
                float dist = math.sqrt(distSq);
                float3 normal = dist > 0.001f ? -toPin / dist : new float3(0, 1, 0);

                float velAlongNormal = math.dot(vel, normal);

                if (velAlongNormal < 0)
                {
                    float pinRestitution = 0.6f;
                    vel -= (1f + pinRestitution) * velAlongNormal * normal;

                    float overlap = pinDist - dist;
                    pos += normal * overlap;

                    System.Threading.Interlocked.Increment(ref ((int*)PinBeHit.GetUnsafePtr())[i]);
                }
            }
        }

        float boundRestitution = 0.6f; 

        if (pos.x - Radius < BoxMin.x) { pos.x = BoxMin.x + Radius; vel.x = math.abs(vel.x) * boundRestitution; }
        else if (pos.x + Radius > BoxMax.x) { pos.x = BoxMax.x - Radius; vel.x = -math.abs(vel.x) * boundRestitution; }

        if (pos.y - Radius < BoxMin.y) { pos.y = BoxMin.y + Radius; vel.y = math.abs(vel.y) * boundRestitution; }
        else if (pos.y + Radius > BoxMax.y) { pos.y = BoxMax.y - Radius; vel.y = -math.abs(vel.y) * boundRestitution; }

        if (pos.z - Radius < BoxMin.z) { pos.z = BoxMin.z + Radius; vel.z = math.abs(vel.z) * boundRestitution; }
        else if (pos.z + Radius > BoxMax.z) { pos.z = BoxMax.z - Radius; vel.z = -math.abs(vel.z) * boundRestitution; }
        
        Positions[index] = pos;
        Velocities[index] = vel;
    }
}