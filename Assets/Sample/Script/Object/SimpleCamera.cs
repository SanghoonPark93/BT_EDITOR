using Core;
using UnityEngine;

namespace BT.Sample
{
    public class SimpleCamera : MonoBehaviour
    {
        [SerializeField] Transform _target;

        // Update is called once per frame
        void Update()
        {
            transform.position = _target.position.OpY(29f).OpZ(-9f);
        }
    }
}