using CityMakerExplorer.AddIn.Core;
using CityMakerExplorer.WorkSpace;
using Gvitech.CityMaker.Controls;
using Gvitech.CityMaker.RenderControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectToolBox
{
    public class ToolBoxTest1 : AbstractCommand
    {
        public override void RestoreEnv() { }

        public override void Run(object sender, EventArgs e)
        {
            //do something...
        }
        public override string CommandName
        {
            get
            {
                return "fffffff";
            }
        }
    }
    public class ToolBoxTest2 : AbstractCommand
    {
        public override void RestoreEnv() { }

        public override void Run(object sender, EventArgs e)
        {
            //do something...
        }
        public override string CommandName
        {
            get
            {
                return "dddddd";
            }
        }
    }
}
