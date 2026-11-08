using UnityEngine;

[CreateAssetMenu(fileName = "Story Node", menuName = "Story Node")]
public class StoryNode : ScriptableObject
{
    [Header("Story Node Information")]
    [SerializeField] private string _nodeId;
    [SerializeField] private string _nodeName;
    [TextArea] [SerializeField] private string _nodeDescription;
    [SerializeField] private StoryNodeType _nodeType;

    [Header("Trigger")]
    [SerializeField] private int _triggerDay;            // 1-7, 0 = any
    [SerializeField] private float _triggerHour;         // jam in-game, -1 = any
    [SerializeField] private StoryCondition[] _triggerConditions;
    [SerializeField] private bool _triggerOnce = true;

    [Header("Gameplay")]
    [SerializeField] private bool _pauseClock = true;    // VN = true
    [SerializeField] private StoryEffect[] _effects;
    [SerializeField] private Task[] _tasksToAdd;

    [Header("Branches")]
    [SerializeField] private StoryBranch[] _branches;

    public string NodeId => _nodeId;
    public string NodeName => _nodeName;
    public string NodeDescription => _nodeDescription;
    public StoryNodeType NodeType => _nodeType;
    public int TriggerDay => _triggerDay;
    public float TriggerHour => _triggerHour;
    public StoryCondition[] TriggerConditions => _triggerConditions;
    public bool TriggerOnce => _triggerOnce;
    public bool PauseClock => _pauseClock;
    public StoryEffect[] Effects => _effects;
    public Task[] TasksToAdd => _tasksToAdd;
    public StoryBranch[] Branches => _branches;
}

[System.Serializable]
public class StoryCondition
{
    public StatKey Stat;
    public float MinValue;
    public float MaxValue = 100f;
}

[System.Serializable]
public class StoryEffect
{
    public StatKey Stat;
    public float Delta;
}

[System.Serializable]
public class StoryBranch
{
    public StoryNode Next;
    public StoryCondition[] Conditions; 
}

public enum StatKey
{
    SUSPICION,
    BURNOUT,
    ENERGY,
    FAMILIARITY_PROGRAMMER,
    FAMILIARITY_DESIGNER,
    FAMILIARITY_COMPOSER,
    QUALITY_VS_CEILING,
    LOCATION_CONFIDENCE,
    REPORT_SUCCESS          // 0/1
}

public enum StoryNodeType
{
    VN_SCENE,
    EVENT,
    PANIC,
    ENDING
}