using UnityEngine;
using System.Collections;

namespace BreakOut
{
    public class BO_BossEntrance : MonoBehaviour
    {
        public static BO_BossEntrance Instance;

        [Header("Body Parts")]
        public Transform bossHead;
        public Transform[] bossPaws;

        [Header("Movement Settings")]
        public float startHeightOffset = 15f;
        public float slideDuration = 2.0f;
        public float pawDelay = 0.8f;

        private Vector3 headTargetPos;
        private Vector3[] pawsTargetPos;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // 1. SETUP: Record targets
            if (bossHead != null)
            {
                headTargetPos = bossHead.position;
                bossHead.position += Vector3.up * startHeightOffset;
            }

            pawsTargetPos = new Vector3[bossPaws.Length];
            for (int i = 0; i < bossPaws.Length; i++)
            {
                if (bossPaws[i] != null)
                {
                    pawsTargetPos[i] = bossPaws[i].position;
                    bossPaws[i].position += Vector3.up * startHeightOffset;
                }
            }

            // 2. DISABLE ATTACKS
            if (bossHead.GetComponent<BO_BossAttack>())
                bossHead.GetComponent<BO_BossAttack>().enabled = false;

            if (bossHead.GetComponent<Collider2D>())
                bossHead.GetComponent<Collider2D>().enabled = false;
        }

        public void StartBossEntrance()
        {
            StartCoroutine(EntranceRoutine());
        }

        private IEnumerator EntranceRoutine()
        {
            Debug.Log("[BossEntrance] Here comes the boy!");

            // --- ANIMATE HEAD ---
            StartCoroutine(SlideObject(bossHead, headTargetPos, slideDuration));

            yield return new WaitForSeconds(pawDelay);

            // --- ANIMATE PAWS ---
            foreach (int i in System.Linq.Enumerable.Range(0, bossPaws.Length))
            {
                if (bossPaws[i] != null)
                {
                    StartCoroutine(SlideObject(bossPaws[i], pawsTargetPos[i], slideDuration));
                }
            }

            // Wait for slide to finish
            yield return new WaitForSeconds(slideDuration);

            // --- NEW: UPDATE PAW LOGIC ---
            // Tell the paws they have arrived at their fighting position
            foreach (Transform paw in bossPaws)
            {
                if (paw != null)
                {
                    BO_BossPaw pawScript = paw.GetComponent<BO_BossPaw>();
                    if (pawScript != null)
                    {
                        pawScript.UpdateHomePosition();
                    }
                }
            }

            // --- FIGHT START ---
            if (BO_BossHead.Instance.healthSlider != null)
                BO_BossHead.Instance.healthSlider.gameObject.SetActive(true);

            if (bossHead.GetComponent<BO_BossAttack>())
                bossHead.GetComponent<BO_BossAttack>().enabled = true;

            if (bossHead.GetComponent<Collider2D>())
                bossHead.GetComponent<Collider2D>().enabled = true;

            Debug.Log("[BossEntrance] Boss is in position. Fight triggers enabled.");
        }

        private IEnumerator SlideObject(Transform obj, Vector3 target, float duration)
        {
            Vector3 start = obj.position;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = Mathf.SmoothStep(0f, 1f, t);

                obj.position = Vector3.Lerp(start, target, t);
                yield return null;
            }
            obj.position = target;
        }
    }
}