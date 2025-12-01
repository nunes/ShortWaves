using UnityEngine;
using Fungus;

public class ForceFungusInit : MonoBehaviour
{
    void Awake()
    {
        // Simply accessing the .Instance property forces it to spawn 
        // and move to DontDestroyOnLoad immediately.
        var manager = FungusManager.Instance;
    }
}