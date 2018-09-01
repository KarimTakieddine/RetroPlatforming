using UnityEngine;

public class BoundingBoxManager : MonoBehaviour
{
    public static BoundingBoxManager Instance { get; private set; }
   
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (!Instance != this)
        {
            Destroy(this);
        }

        DontDestroyOnLoad(this);
    }

    private void Start()
    {

    }
    
    void Update ()
    {
		
	}
};