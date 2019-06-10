﻿namespace FF8
{
    public partial class Module_main_menu_debug
    {
        #region Classes

        public class IGMData_Group : IGMData
        {
            public IGMData_Group( params IGMData[] d) : base(d.Length, 1)
            {
                for (int i = 0; i < d.Length; i++)
                {
                    ITEM[i, 0] = d[i];
                }
            }
            public virtual bool ITEMInputs(IGMDataItem i, int pos = 0)
            {
                return i.Inputs();
            }
            public override bool Inputs()
            {
                if (Enabled)
                {
                    bool ret = base.Inputs();
                    if (!skipdata)
                    {
                        int pos = 0;
                        foreach (IGMDataItem i in ITEM)
                        {
                            ret = ITEMInputs(i,pos++) || ret;
                        }
                    }
                    return ret;
                }
                return false;
            }

            public override void ReInit()
            {
                base.ReInit();
                if (!skipdata)
                    foreach (var i in ITEM)
                    {
                        if (i != null)
                            i.ReInit();
                    }
            }

            public override bool Update()
            {
                if (Enabled)
                {
                    bool ret = base.Update();
                    if (!skipdata)
                        foreach (var i in ITEM)
                        {
                            if (i != null)
                                ret = i.Update() || ret;
                        }
                    return ret;
                }
                return false;
            }
        }
        #endregion Classes
    }
}