using Core;
using UnityEngine;

public class SimpleCamera : MonoBehaviour
{
    [SerializeField] Transform _target;

    // Update is called once per frame
    void Update()
    {
        transform.position = _target.position.OpY(29f).OpZ(-9f);
    }
}
