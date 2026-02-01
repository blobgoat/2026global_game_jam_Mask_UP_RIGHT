using System.Collections;
using System.Diagnostics;
using System.Runtime;
using Debug = UnityEngine.Debug;

//
using UnityEngine;
using UnityEngine.InputSystem;


//Vector2Int.right // (1, 0)
//Vector2Int.left  // (-1, 0)
//Vector2Int.up    // (0, 1)
//Vector2Int.down  // (0, -1)

public class PlanRunner : MonoBehaviour
{
    // --- Minimal state (grid positions) ---
    public Vector2Int playerPos;
    public Vector2Int targetPos;
    [SerializeField] GameObject Drag;
    [SerializeField] GameObject progressButton;
    [SerializeField] GameObject ChangeMoveDia;
    [SerializeField] GameObject GoButton;

    [SerializeField] GameObject WinScreen;

    [SerializeField] GameObject planSlotsUI;
    [SerializeField] GameObject coughPallete;


    // --- Visual references ---
    public Transform playerVisual;
    public Transform targetVisual;

    // --- Configuration ---
    public float tileSize = 1f;
    public float moveDuration = 0.15f; // how long each step animation takes

    // --- Plan: 5 actions ---
    public PlanAction[] plan = new PlanAction[5];

    public PlanAction[] cpuPlan = new PlanAction[5];

    private bool isExecuting = false;
    private bool phaseTwo = false;

    public void ExecutePhaseTwo()
    {
        Debug.Log("Executing phase two");
        progressButton.SetActive(false);
        ChangeMoveDia.SetActive(true);
        phaseTwo = true;
        GoButton.SetActive(true);
        CreatePlanCPU();
    }

    public void CreatePlanCPU()
    {
        //create a random plan for the cpu
        for (int i = cpuPlan.Length - 1; i >= 0; i--)
        {
            int randomIndex = Random.Range(0, 5); // 0 to 4
            PlanAction action = actionTypeFromIndex(randomIndex);

            cpuPlan[i] = action;

            //instantiate the corresponding prefab in the planSlotsUI
            var prefabGo = PrefabSwitch(randomIndex);
            if (prefabGo != null)
            {

                var slotTransform = coughPallete.transform.GetChild(i);
                GameObject go = Instantiate(prefabGo, coughPallete.transform);
                GameObject.Destroy(slotTransform.gameObject);
                go.transform.SetSiblingIndex(i);

            }
            else
            {
                Debug.LogError("Prefab not found for index: " + randomIndex);
            }

        }
    }

    void Awake()
    {
        Instance = this;
        Debug.Assert(Instance != null, "PlanRunner Instance is null in Awake!");
    }

    void initializePlan()
    {
        for (int i = 0; i < plan.Length; i++)
        {
            plan[i] = new PlanAction { type = ActionType.None, v = Vector2Int.zero };
        }
        //destroy all children childrens of planSlotsUI

        int childCount = planSlotsUI.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            var chp = planSlotsUI.transform.GetChild(i);
            for (int j = chp.childCount - 1; j >= 0; j--)
            {
                var child = chp.GetChild(j);
                GameObject.Destroy(child.gameObject);
            }
        }

        //need to raise curtains

        int coughPalleteChildCount = coughPallete.transform.childCount;
        for (int i = coughPalleteChildCount - 1; i >= 0; i--)
        {
            var curtainImagePrefab = Resources.Load<GameObject>("prefabs/Slot1");


            var child = coughPallete.transform.GetChild(i);
            var curtainClone = Instantiate(curtainImagePrefab, coughPallete.transform);

            GameObject.Destroy(child.gameObject);
            //set cpu plan to none
            cpuPlan[i] = new PlanAction { type = ActionType.None, v = Vector2Int.zero };
        }

    }
    bool IsAnyActionEmpty()
    {
        for (int i = 0; i < plan.Length; i++)
        {
            if (plan[i].type == ActionType.None)
            {
                return true;
            }

        }
        return false;
    }
    bool IsAnyCpuActionEmpty()
    {
        for (int i = 0; i < cpuPlan.Length; i++)
        {
            if (cpuPlan[i].type == ActionType.None)
            {
                return true;
            }

        }
        return false;
    }

    public void restartLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void SetAction(int index, PlanAction action)
    {
        if (phaseTwo)
        {
            //initiate Go
            plan[index] = action;
            //Todo add dialogue or warning about starting move action
            Go();

        }
        else
        {
            if (index < 0 || index >= plan.Length) return;
            plan[index] = action;
            if (!IsAnyActionEmpty())
            {

                Drag.SetActive(false);
                progressButton.SetActive(true);
            }
        }
    }

    void Start()
    {
        // Default plan: move right 5 times
        initializePlan();
        //resize the player and target visuals according to tile size
        // double scaleFactor = 0.6; // to make them slightly smaller than the tile
        // playerVisual.localScale = new Vector3((float)(tileSize * scaleFactor), (float)(tileSize * scaleFactor), 1f);
        // targetVisual.localScale = new Vector3((float)(tileSize * scaleFactor), (float)(tileSize * scaleFactor), 1f);

        playerVisual.position = GridToWorld(playerPos);
        targetVisual.position = GridToWorld(targetPos);

        Debug.Log("PlanRunner ready. Press SPACE to execute plan.");
        Debug.Log($"Start: player={playerPos}, target={targetPos}");
    }

    void Update()
    {
        if (!isExecuting && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && !IsAnyActionEmpty())
        {
            if (!phaseTwo)
            {
                ExecutePhaseTwo();
            }
            else
            {
                Go();
            }
            // }
            // Vector2Int originalPlayerPos = playerPos; // Save original position
            // Vector2Int originalTargetPos = targetPos; // Save original target position

            // PlanAction[] CarriedOutActions = ExecutePlan();
            // StartCoroutine(animatePlan(CarriedOutActions, originalPlayerPos, originalTargetPos));

        }
    }

    public void Go()
    {
        if (!isExecuting && !IsAnyActionEmpty() && !IsAnyCpuActionEmpty())
        {
            Vector2Int originalPlayerPos = playerPos; // Save original position
            Vector2Int originalTargetPos = targetPos; // Save original target position

            PlanAction[] CarriedOutActions = ExecutePlan();

            //copy cpu plan for animation
            PlanAction[] coughPlan = new PlanAction[cpuPlan.Length];
            cpuPlan.CopyTo(coughPlan, 0);
            //copy player plan for animation
            PlanAction[] playerPlan = new PlanAction[plan.Length];
            plan.CopyTo(playerPlan, 0);



            StartCoroutine(animatePlan(CarriedOutActions, originalPlayerPos, originalTargetPos, coughPlan, playerPlan));

            initializePlan();

            Drag.SetActive(true);
            GoButton.SetActive(false);
            phaseTwo = false;
        }
    }

    IEnumerator animatePlan(PlanAction[] CarriedOutActions, Vector2Int ogPlPos, Vector2Int ogTPos, PlanAction[] coughPlan, PlanAction[] playerPlan)
    {
        {

            Vector2Int anPlayerPos = ogPlPos;
            Vector2Int anTargetPos = ogTPos;
            isExecuting = true;

            for (int i = 0; i < playerPlan.Length; i++)
            {
                //animate target
                Vector3 fromTarget = GridToWorld(anTargetPos);
                anTargetPos = ApplyOne(coughPlan[i], anTargetPos);
                Vector3 toTarget = GridToWorld(anTargetPos);
                yield return StartCoroutine(AnimateMove(fromTarget, toTarget, moveDuration, targetVisual));


                if (CarriedOutActions[i].type == ActionType.Collide)
                {
                    Debug.Log("Collision detected would show winning screen.");
                    WinScreen.SetActive(true);
                    break;
                }
                Vector3 from = GridToWorld(anPlayerPos);
                anPlayerPos = ApplyOne(CarriedOutActions[i], anPlayerPos);
                Vector3 to = GridToWorld(anPlayerPos);
                Debug.Log($"Step {i + 1}: {CarriedOutActions[i].type} {CarriedOutActions[i].v} -> player={anPlayerPos}");
                yield return StartCoroutine(AnimateMove(from, to, moveDuration, playerVisual));
                if (anPlayerPos == anTargetPos)
                {
                    throw new System.Exception("Unexpected collision during animation.");

                }
            }
            Debug.Log("Done animating.");
            isExecuting = false;
        }

    }

    PlanAction[] ExecutePlan()
    {

        var gridBg = FindObjectOfType<GridBackground>();
        if (gridBg == null)
        {
            Debug.LogError("GridBackground not found in scene!");
            return plan;
        }

        isExecuting = true;
        //build an array so that we can animate later
        PlanAction[] returning = new PlanAction[plan.Length];

        for (int i = 0; i < plan.Length; i++)
        {
            var nextMovePos = ApplyOne(plan[i], playerPos);
            var nextTargetPos = ApplyOne(cpuPlan[i], targetPos);

            if (nextTargetPos.x < 0 || nextTargetPos.x >= gridBg.width || nextTargetPos.y < 0 || nextTargetPos.y >= gridBg.height)
            {
                Debug.Log("CPU out of bounds move detected, replacing with Wait action.");
                nextTargetPos = targetPos;
                cpuPlan[i] = new PlanAction { type = ActionType.Wait, v = Vector2Int.zero };
            }
            else
            {
                targetPos = nextTargetPos;
                //just for sake of consistency and may change this later
                cpuPlan[i] = cpuPlan[i];
            }

            Debug.Log($"Step {i + 1}: {plan[i].type} {plan[i].v} -> player={playerPos}");


            if (nextMovePos == nextTargetPos || playerPos == nextTargetPos)
            {
                Debug.Log("COLLISION! Reached the target.");
                targetPos = nextTargetPos;
                cpuPlan[i] = new PlanAction { type = ActionType.Collide, v = Vector2Int.zero };
                returning[i] = new PlanAction { type = ActionType.Collide, v = Vector2Int.zero };
                break;
            }
            else
            {
                //if it would go out of bounds stop it and replace with a wait action



                if (plan[i].type == ActionType.Move && (nextMovePos.x < 0 || nextMovePos.x >= gridBg.width || nextMovePos.y < 0 || nextMovePos.y >= gridBg.height))
                {
                    Debug.Log("Out of bounds move detected, replacing with Wait action.");
                }

                if (plan[i].type == ActionType.Move && (nextMovePos.x < 0 || nextMovePos.x >= gridBg.width || nextMovePos.y < 0 || nextMovePos.y >= gridBg.height))
                {
                    returning[i] = new PlanAction { type = ActionType.Wait, v = Vector2Int.zero };
                }
                else
                {
                    playerPos = nextMovePos;
                    returning[i] = plan[i];
                }
            }

            //yield return new WaitForSeconds(stepDelay);
        }

        Debug.Log("Done executing.");
        return returning;
    }

    Vector2Int ApplyOne(PlanAction action, Vector2Int iPlayerPos)
    {
        switch (action.type)
        {
            case ActionType.Move:
                return iPlayerPos + action.v;

            case ActionType.Wait:
                // do nothing
                return iPlayerPos;
            case ActionType.Collide:
                // do nothing special for now
                return iPlayerPos;
            case ActionType.None:
                Debug.LogError("Attempted to apply 'None' action type.");
                return iPlayerPos;
            default:
                Debug.LogError($"Unknown action type: {action.type}");
                return iPlayerPos;

        }
    }
    Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * tileSize, gridPos.y * tileSize, 0f);
    }

    IEnumerator AnimateMove(Vector3 from, Vector3 to, float duration, Transform spriteVisual)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            spriteVisual.position = Vector3.Lerp(from, to, t);
            yield return null;
        }
        spriteVisual.position = to;
    }
    public static PlanRunner Instance;

    //switch board for deciding the cpu's action plan
    GameObject PrefabSwitch(int index)
    {
        switch (index)
        {
            case 0:
                return Resources.Load<GameObject>("prefabs/wait");
            case 1:
                return Resources.Load<GameObject>("prefabs/right");
            case 2:
                return Resources.Load<GameObject>("prefabs/left");
            case 3:
                return Resources.Load<GameObject>("prefabs/up");
            case 4:
                return Resources.Load<GameObject>("prefabs/down");
            default:
                Debug.LogError("Invalid prefab index: " + index);
                return null;
        }
    }

    PlanAction actionTypeFromIndex(int index)
    {
        switch (index)
        {
            case 0:
                return new PlanAction { type = ActionType.Wait, v = Vector2Int.zero };
            case 1:
                return new PlanAction { type = ActionType.Move, v = Vector2Int.right };
            case 2:
                return new PlanAction { type = ActionType.Move, v = Vector2Int.left };
            case 3:
                return new PlanAction { type = ActionType.Move, v = Vector2Int.up };
            case 4:
                return new PlanAction { type = ActionType.Move, v = Vector2Int.down };
            default:
                Debug.LogError("Invalid action type index: " + index);
                return new PlanAction { type = ActionType.None, v = Vector2Int.zero };
        }
    }

}