using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectScheduler : MonoBehaviour
{
    [System.Serializable]
    public class EffectItem
    {
        public GameObject effectPrefab; // Prefab hiệu ứng
        public float delay;             // Thời gian delay trước khi spawn
    }

    [Header("Danh sách hiệu ứng và thời gian xuất hiện")]
    public List<EffectItem> effects = new List<EffectItem>();

    [Header("Vị trí spawn hiệu ứng (để trống = vị trí object này)")]
    public Transform spawnPoint;

    private Coroutine playRoutine;

    private void Start()
    {
        // Nếu muốn tự chạy khi Start game, bật dòng dưới
        // PlayEffects();
    }

    // Gọi hàm này để bắt đầu phát hiệu ứng theo lịch
    public void PlayEffects()
    {
        if (playRoutine != null)
            StopCoroutine(playRoutine);

        playRoutine = StartCoroutine(PlayEffectSequence());
    }

    private IEnumerator PlayEffectSequence()
    {
        foreach (var item in effects)
        {
            if (item == null || item.effectPrefab == null)
                continue;

            // Delay riêng của từng hiệu ứng
            yield return new WaitForSeconds(item.delay);

            Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
            Quaternion rot = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

            Instantiate(item.effectPrefab, pos, rot);
        }
    }
}
