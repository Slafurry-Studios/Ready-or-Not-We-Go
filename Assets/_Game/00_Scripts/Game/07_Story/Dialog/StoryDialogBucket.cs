using UnityEngine;


public class StoryDialogBucket : ScriptableObject
{
    [SerializeField] private string _bucketName; 
    [SerializeField] private StoryDialog[] _dialogs;
    [SerializeField] private StoryAction[] _actionsAfterDone;

    public string BucketName => _bucketName;
    public StoryDialog[] Dialogs => _dialogs;
    public StoryAction[] ActionsAfterDone => _actionsAfterDone;
}
