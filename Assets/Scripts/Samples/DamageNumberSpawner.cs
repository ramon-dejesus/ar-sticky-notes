using PrimeTween;
using UnityEngine;
using UnityEngine.Pool;
using UnityUtils;

public class DamageNumberSpawner : MonoBehaviour
{
    [SerializeField] WorldSpaceUIDocument uiDocumentPrefab;
    [SerializeField] float positionRandomness = 0.2f;

    IObjectPool<WorldSpaceUIDocument> uiDocumentPool;
    const string k_labelName = "DamageLabel";

    void Awake() => uiDocumentPool = new ObjectPool<WorldSpaceUIDocument>(
        Create,
        OnTake,
        OnReturn,
        OnDestroy
    );

    WorldSpaceUIDocument Create() => Instantiate(uiDocumentPrefab, transform, true);
    void OnTake(WorldSpaceUIDocument uiDocument) => uiDocument.gameObject.SetActive(true);
    void OnReturn(WorldSpaceUIDocument uiDocument) => uiDocument.gameObject.SetActive(false);
    void OnDestroy(WorldSpaceUIDocument uiDocument)
    {
        if (uiDocument == null) return;
        Destroy(uiDocument.gameObject);
    }

    public void SpawnDamageNumber(int amount, Vector3 worldPosition)
    {
        Vector3 spawnPosition = worldPosition.RandomOffset(positionRandomness);

        WorldSpaceUIDocument instance = uiDocumentPool.Get();
        instance.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
        instance.SetLabelText(k_labelName, amount.ToString());

        Vector3 upPosition = instance.transform.position.Add(y: 1f);

        PrimeTweenConfig.warnEndValueEqualsCurrent = false;
        Sequence.Create(cycles: 1, CycleMode.Yoyo)
            .Group(Tween.PositionY(instance.transform, endValue: upPosition.y, duration: 0.3f))
            .Group(Tween.Scale(instance.transform, endValue: 1.5f, duration: 0.3f))
            .Chain(Tween.PositionY(instance.transform, endValue: -upPosition.y, duration: 0.5f))
            .ChainCallback(() => uiDocumentPool.Release(instance));
    }
}