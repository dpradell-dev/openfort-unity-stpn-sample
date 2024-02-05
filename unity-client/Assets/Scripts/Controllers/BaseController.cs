using TMPro;
using UnityEngine;

public abstract class BaseController : MonoBehaviour
{
    [Header("Base Components")]
    public GameObject viewPanel;
    public StatusTextBehaviour statusText;

    protected const string CurrentCloudModule = "CloudCodeModules";
}
