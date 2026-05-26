using System;
using System.Collections.Generic;
using R3;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

public class BallSimulationManager : MonoBehaviour
{
    [Header("Simulation Ball Settings")]
    public int initialBallCount = 100;
    public int ballsPerClick = 10; 
    public float ballRadius = 0.2f;
    public Vector3 boxSize = new Vector3(20f, 20f, 1f);
    public Vector3 ballSpawnPosition = new Vector3(20f, 20f, 1f);
    public float minSpeed = 5f;
    public float maxSpeed = 15f;
    public float ballGravity = 9.80665f;
    
    private ObjectPool<GameObject> _ballPool;
    
    [Header("Simulation Pin Settings")]
    public Vector3 pinStartPosition = Vector3.zero;
    public int initialPinCount = 100;
    public float pinRadius = 0.2f;
    public int pinRowCount = 5; 
    public float pinOffsetX = 2f;
    public float pinOffsetY = 2f;
    
    [Header("Rendering")]
    public GameObject ballPrefab;
    public GameObject pinPrefab;
    
    public ReactiveProperty<ulong> Money = new ReactiveProperty<ulong>(0);

    // Switched to NativeList for dynamic resizing
    private NativeList<float3> _positions;
    private NativeList<float3> _velocities;
    private NativeArray<float3> _inputPositionsCopy;
    private NativeArray<float3> _inputVelocitiesCopy;
    private NativeArray<float3> _pinPositions;
    private NativeArray<int> _pinBeHit;
    
    private JobHandle _jobHandle;

    private Vector3 _boxMin;
    private Vector3 _boxMax;
    
    private List<Transform> _ballTransforms;
    private List<PinController> _pins;


    private void Awake()
    {
        _ballPool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(ballPrefab), 
            actionOnGet: (obj) => obj.SetActive(true),  
            actionOnRelease: (obj) => obj.SetActive(false), 
            actionOnDestroy: (obj) => Destroy(obj),
            collectionCheck: false, 
            defaultCapacity: 2000, 
            maxSize: 5000
        );
    }
    void Start()
    {
        _boxMin = -boxSize / 2f;
        _boxMax = boxSize / 2f;

        // Initialize lists with an initial capacity to prevent frequent early reallocations
        _positions = new NativeList<float3>(initialBallCount * 2, Allocator.Persistent);
        _velocities = new NativeList<float3>(initialBallCount * 2, Allocator.Persistent);
        _pinPositions =  new NativeArray<float3>(initialPinCount * 2, Allocator.Persistent);
        
        // List
        _ballTransforms = new List<Transform>();
        _pins = new List<PinController>();
        
        // Spawn initial batch
        SpawnBalls(ballSpawnPosition, initialBallCount);
        SpawnPins(pinStartPosition, initialPinCount);
    }

    void Update()
    {
        // 1. Detect Click to Spawn
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleClickSpawn();
        }

        // If we have no balls, skip physics calculations
        if (_positions.Length == 0) return;
        
        int count = _positions.Length;

        // Create temporary allocations for the ReadOnly parameters for this frame's calculations
        _inputPositionsCopy = new NativeArray<float3>(count, Allocator.TempJob);
        _inputVelocitiesCopy = new NativeArray<float3>(count, Allocator.TempJob);
        _pinBeHit = new NativeArray<int>(_pins.Count, Allocator.TempJob);

        _inputPositionsCopy.CopyFrom(_positions.AsArray());
        _inputVelocitiesCopy.CopyFrom(_velocities.AsArray());

        // 2. Setup & Run Burst Job
        BallPhysicsJob job = new BallPhysicsJob
        {
            Positions = _positions.AsArray(),
            Velocities = _velocities.AsArray(),
            PinPositions = _pinPositions,
            PinBeHit = _pinBeHit,
            InputPositions = _inputPositionsCopy,
            InputVelocities = _inputVelocitiesCopy,
            Radius = ballRadius,
            PinRadius = pinRadius,
            BoxMin = _boxMin,
            BoxMax = _boxMax,
            DeltaTime = Time.deltaTime,
            Gravity = ballGravity
        };
        _jobHandle = job.Schedule(count, 64);
    }

    private void LateUpdate()
    {
        _inputPositionsCopy.Dispose(_jobHandle);
        _inputVelocitiesCopy.Dispose(_jobHandle);
        
        _jobHandle.Complete();

        for (int i = 0; i < _positions.Length; i++)
        {
            _ballTransforms[i].position = _positions[i];
            if (_positions[i].y < -15f)
            {
                GameObject ballGo = _ballTransforms[i].gameObject;
                _ballPool.Release(ballGo);
            
                _positions.RemoveAtSwapBack(i);
                _velocities.RemoveAtSwapBack(i);
                
                int lastIndex = _ballTransforms.Count - 1;
                _ballTransforms[i] = _ballTransforms[lastIndex];
                _ballTransforms.RemoveAt(lastIndex);
            }
        }
        
        int totalHitsThisFrame = 0;
        for (int i = 0; i < _pinBeHit.Length; i++)
        {
            if (_pinBeHit[i] == 0) continue;
        
            totalHitsThisFrame += 1; 
        
            _pins[i].LightUpPin();
        }
        
        if (totalHitsThisFrame > 0)
        {
            Money.Value += (ulong)totalHitsThisFrame; 
        }
        _pinBeHit.Dispose(_jobHandle);
    }

    private void HandleClickSpawn()
    {
        SpawnBalls(ballSpawnPosition, ballsPerClick);
    }

    void SpawnBalls(Vector3 origin, int count)
    {
        for (int i = 0; i < count; i++)
        {   
            GameObject g = _ballPool.Get();
            g.transform.position = origin;
            g.transform.rotation = Quaternion.identity;
            g.transform.localScale = Vector3.one * (ballRadius * 2f);
            
            _ballTransforms.Add(g.transform);
        
            _positions.Add(origin);
        
            Vector3 randomDir = UnityEngine.Random.onUnitSphere;
            randomDir.z = 0;
            float speed = UnityEngine.Random.Range(minSpeed, maxSpeed);
            _velocities.Add(randomDir * speed);
        }
    }

    private void SpawnPins(Vector3 startPoint, int totalPinsToSpawn)
    {   
        int currentPinCount = 0;
        int row = 0;
        int col = 0;

        while (currentPinCount < totalPinsToSpawn)
        {
            float staggerX = (row % 2 == 1) ? (pinOffsetX * 0.5f) : 0f;
        
            Vector3 position = startPoint + new Vector3(
                (pinOffsetX * col) + staggerX, 
                -pinOffsetY * row, 
                0
            );
            
            GameObject g = Instantiate(pinPrefab, position, Quaternion.identity);
            g.transform.localScale = Vector3.one * (pinRadius * 2f);
            
            _pinPositions[currentPinCount] = position;
            _pins.Add(g.GetComponent<PinController>());
            
            currentPinCount++;
            col++;
            
            if (col >= pinRowCount)
            {
                col = 0;
                row++;
            }
        }
    }

    private void OnDestroy()
    {
        if (_positions.IsCreated) _positions.Dispose();
        if (_velocities.IsCreated) _velocities.Dispose();
        if (_pinPositions.IsCreated) _pinPositions.Dispose();
        if(_pinBeHit.IsCreated) _pinBeHit.Dispose();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
    }
}