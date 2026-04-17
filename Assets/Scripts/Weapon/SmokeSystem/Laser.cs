using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets
{
    // 激光脚本，挂在你的激光物体上
    public class Laser : MonoBehaviour
    {
        public LineRenderer line;
        public SmokeGenerator smokeGen;

        private void Update()
        {
            if (line.enabled && smokeGen != null)
            {
                Vector3 start = line.GetPosition(0);
                Vector3 end = line.GetPosition(1);
                // 每帧持续推开烟雾
                smokeGen.OnLaserHit(start, end);
            }
        }
    }
}
