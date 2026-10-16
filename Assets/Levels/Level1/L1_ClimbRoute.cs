using UnityEngine;

namespace Jam.Levels.Level1
{
    public sealed class L1_ClimbRoute : MonoBehaviour
    {
        public Transform bottom;
        public Transform top;
        public Transform landing;
        public Collider upperFloor;
        public Transform hatch;
        public void SetOpen(bool open)
        {
            if (hatch != null) hatch.localRotation = Quaternion.Euler(open ? 78f : 0f, 0f, 0f);
        }
    }
}
