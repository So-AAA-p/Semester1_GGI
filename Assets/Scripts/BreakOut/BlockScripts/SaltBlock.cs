using UnityEngine;

namespace BreakOut
{
    public class SaltBlock : BreakOutBlock
    {
        protected override void OnBreak()
        {
            BreakOutManager.instance.ToggleControls();
            base.OnBreak();
        }
    }
}
