using UnityEngine;
using Slafurry.Systems.Scene;

public class CallAuthoritiesExecution : ActionExecution
{
    [Header("Proof Thresholds (MemoryBank.CurrentTotalSize)")]
    [Tooltip("Minimum proof to avoid the worst ending.")]
    [SerializeField] private float _minProofForAnySuccess = 10f;
    [Tooltip("Minimum proof for a mediocre ending.")]
    [SerializeField] private float _minProofForMediocreEnding = 20f;
    [Tooltip("Minimum proof for a good ending.")]
    [SerializeField] private float _minProofForGoodEnding = 35f;
    [Tooltip("Minimum proof for the best ending.")]
    [SerializeField] private float _minProofForBestEnding = 50f;

    [Header("Ending Scenes")]
    [SerializeField] private SceneReference _bestEndingScene;
    [SerializeField] private SceneReference _goodEndingScene;
    [SerializeField] private SceneReference _mediocreEndingScene;
    [SerializeField] private SceneReference _badEndingScene;
    [SerializeField] private SceneReference _worstEndingScene;

    public override void ExecuteAction(Team team)
    {
        if (team == null) return;

        float proof = team.MemoryBank?.CurrentTotalSize ?? 0f;
        int memoryCount = team.MemoryBank?.memories?.Length ?? 0;

        // Founder reacts to the player calling authorities
        var chat = UnityEngine.Object.FindFirstObjectByType<ChatManager>();
        if (chat != null)
        {
            if (proof >= _minProofForBestEnding)
                chat.AddLine("Founder", "You have enough evidence. The authorities will bury me.");
            else if (proof >= _minProofForGoodEnding)
                chat.AddLine("Founder", "Some proof... The authorities may investigate.");
            else if (proof >= _minProofForMediocreEnding)
                chat.AddLine("Founder", "Barely any evidence. They'll laugh at you.");
            else
                chat.AddLine("Founder", "You call the authorities with nothing? How pathetic.");
        }

        // Evaluate ending based on proof collected
        if (proof >= _minProofForBestEnding && _bestEndingScene != null)
            SceneSystem.Load(_bestEndingScene);
        else if (proof >= _minProofForGoodEnding && _goodEndingScene != null)
            SceneSystem.Load(_goodEndingScene);
        else if (proof >= _minProofForMediocreEnding && _mediocreEndingScene != null)
            SceneSystem.Load(_mediocreEndingScene);
        else if (proof >= _minProofForAnySuccess && _badEndingScene != null)
            SceneSystem.Load(_badEndingScene);
        else if (_worstEndingScene != null)
            SceneSystem.Load(_worstEndingScene);
        else
            Debug.Log("[CallAuthoritiesExecution] No ending scene assigned for this proof level.");
    }
}
