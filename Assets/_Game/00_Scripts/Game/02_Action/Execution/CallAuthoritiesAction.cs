using UnityEngine;
using Slafurry.Systems.Scene;

[CreateAssetMenu(fileName = "Call Authorities", menuName = "Action Data/Call Authorities")]
public class CallAuthoritiesAction : Action
{
    [Header("Proof Threshold")]
    [Tooltip("Minimum proof (MemoryBank.CurrentTotalSize) needed to call authorities successfully.")]
    [SerializeField] private float _minProofForSuccess = 10f;

    [Header("Ending Scenes")]
    [SerializeField] private SceneReference _winScene;
    [SerializeField] private SceneReference _gameOverScene;

    public override void ExecuteAction(Team team)
    {
        if (team == null) return;

        float proof = team.MemoryBank?.CurrentTotalSize ?? 0f;
        int memoryCount = team.MemoryBank?.memories?.Length ?? 0;

        // Founder reacts to the player calling authorities
        var chat = UnityEngine.Object.FindFirstObjectByType<ChatManager>();
        if (chat != null)
        {
            if (proof >= _minProofForSuccess)
                chat.AddLine("Founder", $"You have {memoryCount} proof. The authorities will act on this.");
            else
                chat.AddLine("Founder", $"Only {memoryCount} proof? The authorities won't care.");
        }

        // Win if enough proof, Game Over otherwise
        if (proof >= _minProofForSuccess && _winScene != null)
            SceneSystem.Load(_winScene);
        else if (_gameOverScene != null)
            SceneSystem.Load(_gameOverScene);
        else
            Debug.Log("[CallAuthoritiesAction] No ending scene assigned.");
    }
}
