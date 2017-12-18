using CityMakerExplorer.AddIn.Core;
using CityMakerExplorer.WorkSpace;
using Gvitech.CityMaker.Controls;
using Gvitech.CityMaker.RenderControl;
using System;
using System.Collections.Generic;

namespace ConnectPlugin
{
    class Button01 : AbstractCommand
    {
        public override void RestoreEnv() { }

        public override void Run(object sender, EventArgs e)
        {
            //从图层树获取指定类型的实例
            List<IRObject> list = ProjectTreeServices.GetRObjectsFromTree(gviObjectType.gviObject3DTileLayer);
            I3DTileLayer t = (I3DTileLayer)list[0];
            new Form1($"Button1 -> listCount:{list.Count}").Show();
        }
    }
    class Button02 : AbstractCommand
    {
        public override void RestoreEnv() { }

        public override void Run(object sender, EventArgs e)
        {
            //获取axRenderControl
            AxRenderControl axRenderControl = RenderControlServices.Instance().AxRenderControl;
            axRenderControl.Camera.FlyToObject(Guid.Empty, gviActionCode.gviActionFlyTo);
        }
    }
    class Button03 : AbstractCommand
    {
        public override void RestoreEnv() { }

        public override void Run(object sender, EventArgs e)
        {
            new Form1("Button3").Show();
        }
    }
}
