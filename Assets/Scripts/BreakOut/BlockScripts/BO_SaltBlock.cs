using UnityEngine;

namespace BreakOut
{
    public class BO_SaltBlock : BO_Block
    {
        protected override void OnBreak()
        {
            BO_Manager.instance.ToggleControls();
            base.OnBreak();
        }
    }
}
