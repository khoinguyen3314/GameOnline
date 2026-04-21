using Fusion;
using UnityEngine;

public class MoCua : NetworkBehaviour
{
    [SerializeField] GameObject Cong;
    [SerializeField] GameObject Enemy;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Enemy == null)
        {
            Cong.SetActive(false);
        }
    }
}
