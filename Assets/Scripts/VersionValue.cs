using UnityEngine;
using UnityEngine.UI;

public class VersionValue : MonoBehaviour
{
    public Text Text;

    private void Awake()
    {
        Text.text = Application.version;
    }
}
