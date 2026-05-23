using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class BallSimulationManager : MonoBehaviour
{
    [Header("Simulation Settings")]
    public int initialBallCount = 100;
    public int ballsPerClick = 10; 
    public float ballRadius = 0.2f;
    public Vector3 boxSize = new Vector3(20f, 20f, 1f);
    public float minSpeed = 5f;
    public float maxSpeed = 15f;
    public float ballGravity = 9.80665f;

    [Header("Rendering")]
    public GameObject ballPrefab;

    // Switched to NativeList for dynamic resizing
    private NativeList<float3> positions;
    private NativeList<float3> velocities;

    private Vector3 boxMin;
    private Vector3 boxMax;
    
    private System.Collections.Generic.List<Transform> ballTransforms;

    void Start()
    {
        boxMin = -boxSize / 2f;
        boxMax = boxSize / 2f;

        // Initialize lists with an initial capacity to prevent frequent early reallocations
        positions = new NativeList<float3>(initialBallCount * 2, Allocator.Persistent);
        velocities = new NativeList<float3>(initialBallCount * 2, Allocator.Persistent);
        ballTransforms = new System.Collections.Generic.List<Transform>();
        
        // Spawn initial batch
        SpawnBalls(Vector3.zero, initialBallCount);
    }

    void Update()
    {
        // 1. Detect Click to Spawn
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleClickSpawn();
        }

        // If we have no balls, skip physics calculations
        if (positions.Length == 0) return;
        
        int count = positions.Length;

        // Create temporary allocations for the ReadOnly parameters for this frame's calculations
        NativeArray<float3> inputPositionsCopy = new NativeArray<float3>(count, Allocator.TempJob);
        NativeArray<float3> inputVelocitiesCopy = new NativeArray<float3>(count, Allocator.TempJob);
        
        inputPositionsCopy.CopyFrom(positions.AsArray());
        inputVelocitiesCopy.CopyFrom(velocities.AsArray());

        // 2. Setup & Run Burst Job
        BallPhysicsJob job = new BallPhysicsJob
        {
            Positions = positions.AsArray(),
            Velocities = velocities.AsArray(),
            InputPositions = inputPositionsCopy,
            InputVelocities = inputVelocitiesCopy,
            Radius = ballRadius,
            BoxMin = boxMin,
            BoxMax = boxMax,
            DeltaTime = Time.deltaTime,
            Gravity = ballGravity
        };
        
        JobHandle handle = job.Schedule(count, 64);
        
        inputPositionsCopy.Dispose(handle);
        inputVelocitiesCopy.Dispose(handle);
        
        handle.Complete();

        for (int i = 0; i < positions.Length; i++)
        {
            ballTransforms[i].position = positions[i];
        }
    }

    private void HandleClickSpawn()
    {
        Vector3 spawnPoint = Vector3.zero;
        SpawnBalls(spawnPoint, ballsPerClick);
    }

    void SpawnBalls(Vector3 origin, int count)
    {
        for (int i = 0; i < count; i++)
        {   
            GameObject g = Instantiate(ballPrefab, origin, Quaternion.identity);
            
            g.transform.localScale = Vector3.one * (ballRadius * 2f);
            ballTransforms.Add(g.transform);
            positions.Add(g.transform.position);
            
            Vector3 randomDir = UnityEngine.Random.onUnitSphere;
            randomDir.z = 0;
            float speed = UnityEngine.Random.Range(minSpeed, maxSpeed);
            velocities.Add(randomDir * speed);
        }
    }

    void OnDestroy()
    {
        if (positions.IsCreated) positions.Dispose();
        if (velocities.IsCreated) velocities.Dispose();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
    }
}