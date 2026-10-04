using UnityEngine;

public class TestBehaviour : MonoBehaviour
{
    // this case should get suppressed by the roslyn analyzer reporting suppression at this location for diagnostic id 'CS8618'
    // unity itself does not report it as a warning anymore thanks to the suppressor, however rider still does (probably because of additional resharper diagnostics that do not get suppressed by roslyn analyzers)
    [SerializeField] private MonoBehaviour testObject1;

    // You can prevent this report by disabling the diagnostic with a pragma, however this is very manual.
    // Disabling 'CS8618' solution wide is not an option, since we only want to suppress this exact case
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    [SerializeField] private MonoBehaviour testObject2;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
}