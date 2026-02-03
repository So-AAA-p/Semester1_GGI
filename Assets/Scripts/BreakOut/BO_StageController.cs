using BreakOut;
using System.Collections;
using UnityEngine;

public class BO_StageController : MonoBehaviour
{
    public enum StageType
    {
        Stage1,
        Stage2
    }

    public static BO_StageController Instance;
    public StageType currentStage;

    private void Awake()
    {
        Instance = this;
    }

    public bool IsStage2 => currentStage == StageType.Stage2;

    public void StartStage(StageType stage)
    {
        currentStage = stage;
        Debug.Log($"[Stage] Starting {stage}");

        if (stage == StageType.Stage2)
        {
            StartCoroutine(InitializeStage2());
        }
    }

    private IEnumerator InitializeStage2()
    {
        // wait until all Start() methods have run
        yield return null; // one frame

        BO_BlueberryManager.Instance.InitializeBlueberries();
    }


    public void EndStage()
    {
        Debug.Log($"[Stage] {currentStage} complete!");
        // later: transitions, UI, next stage
    }
}