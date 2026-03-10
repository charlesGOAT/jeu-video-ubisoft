using System.Collections.Generic;
using UnityEngine;

namespace SOG.CVDFilter
{
    public class CVDProfilesSO : ScriptableObject
    {
        [field: SerializeField] public List<VisionTypeInfo> VisionTypes { get; private set; }
    }
}



