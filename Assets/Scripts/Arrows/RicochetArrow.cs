﻿namespace Assets.Scripts.Arrows
{
    public class RicochetArrow : ArrowProperty
    {
        public override void Effect()
        {
            if (colInfo.IsTrigger) return;
        }
    } 
}
