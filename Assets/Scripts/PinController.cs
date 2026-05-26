using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PinController : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    [SerializeField] private int lightUpCount = 5;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color lightUpColor = Color.gold;
    private CancellationTokenSource _cts;
    private void Awake()
    {
        _spriteRenderer =  GetComponent<SpriteRenderer>();
    }

    public async void LightUpPin()
    {
        try
        {
            if(_cts != null)return;
            _cts = new CancellationTokenSource();
            await HandleLightUp(_cts.Token);
        }
        catch (Exception e)
        {
            throw; // TODO handle exception
        }
    }

    private async UniTask HandleLightUp(CancellationToken token = default)
    {
        float counter = 0;
        while (counter < lightUpCount)
        {
            _spriteRenderer.color = lightUpColor;
            await UniTask.Delay(TimeSpan.FromMilliseconds(100), cancellationToken:token);
            _spriteRenderer.color = normalColor;
            await UniTask.Delay(TimeSpan.FromMilliseconds(100), cancellationToken:token);
            counter++;
        }
        _spriteRenderer.color = normalColor;
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }
}