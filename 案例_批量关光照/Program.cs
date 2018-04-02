using Gvitech.CityMaker.FdeCore;
using Gvitech.CityMaker.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

/*
 *  7.0版处理FDB中的模型，主要内容为取出模型，关闭材质的光照属性
 *  需要加密锁，需要装7.0Runtime
 *  增加COM引用，所有Gvitech开头的库
 */

namespace 案例_批量关光照
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ConnectionInfo ci = new ConnectionInfo();
                ci.ConnectionType = gviConnectionType.gviConnectionFireBird2x;
                ci.Database = "../XXX.FDB";
                IDataSource dataSouce = new DataSourceFactory().OpenDataSource(ci);
                string[] names = (string[])dataSouce.GetFeatureDatasetNames();
                IFeatureDataSet fds = dataSouce.OpenFeatureDataset(names[0]);
                IResourceManager resManager = fds as IResourceManager;

                //没有锁时这句会报错
                IEnumResName myEnumNames = resManager.GetModelNames();
                while (myEnumNames.MoveNext())
                {
                    string modelName = myEnumNames.Current;
                    IModel myModel = resManager.GetModel(modelName);
                    if (myModel != null)
                    {
                        for (int i = 0; i < myModel.GroupCount; i++)
                        {
                            IDrawGroup myGroup = myModel.GetGroup(i);
                            for (int j = 0; j < myGroup.PrimitiveCount; j++)
                            {
                                IDrawPrimitive myPv = myGroup.GetPrimitive(j);
                                IDrawMaterial myDM = myPv.Material;
                                if (myDM != null)
                                {
                                    myDM.EnableLight = false;
                                }
                                myPv.Material = myDM;
                                myGroup.SetPrimitive(j, myPv);
                            }
                            myModel.SetGroup(i, myGroup);
                        }
                        resManager.UpdateModel(modelName, myModel);
                    }
                }

                //手动释放COM对象
                Marshal.ReleaseComObject(fds);
                MessageBox.Show("完成！");
            }
            catch { MessageBox.Show("必须插入加密锁！必须安装CityMaker Builder 7.1"); }
        }
    }
}
