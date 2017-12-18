using Gvitech.CityMaker.Common;
using Gvitech.CityMaker.Controls;
using Gvitech.CityMaker.FdeCore;
using Gvitech.CityMaker.FdeGeometry;
using Gvitech.CityMaker.Math;
using Gvitech.CityMaker.RenderControl;
using Gvitech.CityMaker.Resource;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace renderControle
{
    public partial class Form1 : Form
    {
        //三维对象
        AxRenderControl axRenderControl;
        //坐标系父
        CRSFactory CRSFactory = new CRSFactory();

        public Form1()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            //* 创建三维对象
            axRenderControl = new AxRenderControl();

            //* 创建三维对象的属性
            IPropertySet ps = new PropertySet();
            ps.SetProperty("RenderSystem", gviRenderSystem.gviRenderOpenGL);

            //* 初始化三维对象
            axRenderControl.Initialize2("wkt", ps);

            //* 塞入Form
            this.panel1.Controls.Add(axRenderControl);

            //* 设置天空盒子
            ISkyBox sb = axRenderControl.ObjectManager.GetSkyBox(0);
            sb.SetImagePath(gviSkyboxImageIndex.gviSkyboxImageBack, "BK.jpg");
            sb.SetImagePath(gviSkyboxImageIndex.gviSkyboxImageBottom, "DN.jpg");
            sb.SetImagePath(gviSkyboxImageIndex.gviSkyboxImageFront, "FR.jpg");
            sb.SetImagePath(gviSkyboxImageIndex.gviSkyboxImageLeft, "LF.jpg");
            sb.SetImagePath(gviSkyboxImageIndex.gviSkyboxImageRight, "RT.jpg");
            sb.SetImagePath(gviSkyboxImageIndex.gviSkyboxImageTop, "UP.jpg");
        }

        #region 授权相关

        /// <summary>
        /// 设置授权服务器，本地锁不需要设置
        /// </summary>
        void setLicense()
        {
            ILicenseServer ls = new LicenseServer();
            ls.SetHost("192.168.2.200", 8588, "");
        }

        /// <summary>
        /// 验证授权
        /// </summary>
        /// <returns></returns>
        bool hasLicense()
        {
            IInternalLicenseKey licenseKey = axRenderControl as IInternalLicenseKey;
            int version = licenseKey.SetLicenseKey("授权字符串");
            return version > 0;
        }

        #endregion

        #region 坐标系相关

        /// <summary>
        /// 创建一个坐标系对象
        /// </summary>
        /// <param name="wkt"></param>
        /// <returns></returns>
        ISpatialCRS CreateCRS(string wkt)
        {
            // return CRSFactory.CreateFromWKID(2364);
            return (ISpatialCRS)CRSFactory.CreateFromWKT(wkt);
        }

        /// <summary>
        /// 判断坐标系类型
        /// </summary>
        /// <param name="srs"></param>
        /// <returns></returns>
        bool isENU(ICoordinateReferenceSystem srs)
        {
            return srs.IsENU();
        }

        /// <summary>
        /// 手动设置坐标系
        /// </summary>
        /// <returns></returns>
        string setCoordSys()
        {
            return new CoordSysDialog().ShowDialog(gviLanguage.gviLanguageChineseSimple);
        }

        /// <summary>
        /// 重投影
        /// </summary>
        /// <param name="tagWKT"></param>
        bool ProjectCoord(IPoint p, string tagWKT)
        {
            ISpatialCRS tagCRS = (ISpatialCRS)new CRSFactory().CreateFromWKT(tagWKT);
            return p.Project(tagCRS);
        }
        #endregion

        #region renderControl 相关

        /// <summary>
        /// 创建项目树节点
        /// </summary>
        /// <param name="NodeName"></param>
        /// <returns></returns>
        Guid CreateProjectTreeNode(string NodeName)
        {
            Guid root = axRenderControl.ProjectTree.RootID;
            return axRenderControl.ProjectTree.CreateGroup(NodeName, root);
        }

        /// <summary>
        /// 创建并加载点云
        /// </summary>
        /// <returns></returns>
        IRenderModelPoint CreatePointCloud()
        {
            //准备容器
            IModel model = new ResourceFactory().CreateModel();
            IDrawGroup drawGroup = new DrawGroup();
            IDrawPrimitive drawPrimitive = new DrawPrimitive();
            IFloatArray verList = new FloatArray(); //点集
            IUInt32Array colorList = new UInt32Array(); //点色

            //点坐标
            float x = 3.3f, y = 4.4f, z = 5.5f;
            //点色
            byte a = 128, r = 255, g = 255, b = 255;
            uint col = (uint)(b | g << 8 | r << 16 | a << 24); //argb => uint
            for (int i = 0; i < 10; i++)
            {
                verList.Append(x);
                verList.Append(y);
                verList.Append(z);
                colorList.Append(col);
            }

            //塞入容器
            drawPrimitive.VertexArray = verList;
            drawPrimitive.ColorArray = colorList;
            drawPrimitive.Material.EnableBlend = false;//关闭融合
            drawPrimitive.Material.EnableLight = false;//关闭光照
            drawPrimitive.PrimitiveMode = gviPrimitiveMode.gviPrimitiveModeLineList; //设置绘制模式为点
            drawGroup.AddPrimitive(drawPrimitive); //塞入渲染组
            model.AddGroup(drawGroup);//塞入model
            axRenderControl.ObjectManager.AddModel("modelName", model);//塞入三维对象，与modelPoint通过名称匹配

            //创建modelPoint,用于索引模型
            IModelPoint mp = (IModelPoint)new GeometryFactory().CreateGeometry(gviGeometryType.gviGeometryModelPoint, gviVertexAttribute.gviVertexAttributeZ);
            mp.SpatialCRS = (SpatialCRS)CRSFactory.CreateFromWKT("wkt");//设置坐标系
            mp.ModelEnvelope = model.Envelope; // 排除不显示BUG
            mp.SetCoords(3.3, 4.4, 5.5, 0, 0);
            mp.ModelName = "modelName";//匹配模型
            return axRenderControl.ObjectManager.CreateRenderModelPoint(mp, null, Guid.Empty);//创建完成
        }

        /// <summary>
        /// 创建点
        /// </summary>
        /// <returns></returns>
        IRenderPoint CreateRenderPoint()
        {
            //创建一个点类型实例，设置坐标值
            GeometryFactory GF = new GeometryFactory();
            IPoint p = GF.CreatePoint(gviVertexAttribute.gviVertexAttributeZ);
            p.SetCoords(3.3, 4.4, 5.5, 0, 0);

            //创建一个点类型的样式实例
            ISimplePointSymbol sps = new SimplePointSymbol();
            sps.Color = Color.Blue; //颜色
            sps.Size = 10;          //大小
            sps.Alignment = gviPivotAlignment.gviPivotAlignBottomCenter;    //对齐

            return axRenderControl.ObjectManager.CreateRenderPoint(p, sps, Guid.Empty);
        }

        /// <summary>
        /// 创建POI
        /// </summary>
        /// <returns></returns>
        IRenderPOI createPOI()
        {
            IPOI poi = (IPOI)new GeometryFactory().CreateGeometry(gviGeometryType.gviGeometryPOI, gviVertexAttribute.gviVertexAttributeZ);
            poi.SetCoords(3.3, 4.4, 5.5, 0, 0);
            poi.SpatialCRS = (SpatialCRS)new CRSFactory().CreateFromWKID(2463);
            poi.ImageName = "poi.png";
            poi.Name = "POI_01";
            poi.Size = 10;
            return axRenderControl.ObjectManager.CreateRenderPOI(poi);
        }

        /// <summary>
        /// 加载TDBX
        /// </summary>
        /// <returns></returns>
        I3DTileLayer addTDBX()
        {
            return axRenderControl.ObjectManager.Create3DTileLayer("test.tdbx", null, Guid.Empty);
        }

        /// <summary>
        /// 打开FDB数据
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        List<IFeatureLayer> openFDB(string filePath)
        {
            Hashtable featuerClassList = new Hashtable();

            #region 提取数据
            //连接器
            IConnectionInfo ci = new ConnectionInfo();
            ci.ConnectionType = gviConnectionType.gviConnectionFireBird2x;//FDB
            //ci.ConnectionType = gviConnectionType.gviConnectionSQLite3;//SDB
            ci.Password = null;
            ci.Database = filePath;

            //通过连接器打开数据源
            IDataSourceFactory dsFac = new DataSourceFactory();
            IDataSource ds = dsFac.OpenDataSource(ci);

            //查找数据源中所有的数据集
            string[] featureDataSetNames = ds.GetFeatureDatasetNames();
            if (featureDataSetNames.Length == 0)
            {
                Console.WriteLine("打开失败！");
                return null;
            }

            //获取数据集，一般只有一个，多个时要遍历
            IFeatureDataSet dataset = ds.OpenFeatureDataset(featureDataSetNames[0]);

            //查找数据集中的所有要素类
            string[] featureClassNames = dataset.GetNamesByType(gviDataSetType.gviDataSetFeatureClassTable);
            if (featureClassNames.Length == 0)
            {
                Console.WriteLine("没有找到要素类！");
                return null;
            }
            for (int fci = 0; fci < featureClassNames.Length; fci++)
            {
                IFeatureClass fc = dataset.OpenFeatureClass(featureClassNames[fci]);
                //创建一个容器，放fc的几何属性名称
                List<string> geoNames = new List<string>();
                //查找要素类中所有属性
                IFieldInfoCollection fieldInfos = fc.GetFields();
                for (int fieldi = 0; fieldi < fieldInfos.Count; fieldi++)
                {
                    IFieldInfo fieldinfo = fieldInfos.Get(fieldi);
                    if (fieldinfo == null || fieldinfo.GeometryDef == null)
                        continue;
                    geoNames.Add(fieldinfo.Name);
                }
                featuerClassList[fc] = geoNames;
            }
            #endregion

            List<IFeatureLayer> fls = new List<IFeatureLayer>();
            #region 塞入三维对象
            bool hasfly = false;
            foreach (IFeatureClass fc in featuerClassList.Keys)
            {
                List<string> geoNames = (List<string>)featuerClassList[fc];
                foreach (string geoName in geoNames)
                {
                    if (geoName.Equals("Geometry"))
                    {
                        IFeatureLayer fl = axRenderControl.ObjectManager.CreateFeatureLayer(fc, geoName, null, null, Guid.Empty);
                        if (fl != null)
                            fls.Add(fl);
                    }

                    //jump once!
                    if (!hasfly)
                    {
                        IFieldInfoCollection fieldInfos = fc.GetFields();
                        IFieldInfo fieldinfo = fieldInfos.Get(fieldInfos.IndexOf(geoName));
                        IGeometryDef gd = fieldinfo.GeometryDef;
                        IEnvelope enve = gd.Envelope;
                        if (enve != null)
                        {
                            axRenderControl.Camera.LookAtEnvelope(enve);
                            hasfly = true;
                        }
                    }
                }
            }
            #endregion

            return fls;
        }

        /// <summary>
        /// 一些常用的方法
        /// </summary>
        void RenderControl_SomeUsfull_Motheds()
        {
            // 重置三维对象
            axRenderControl.Reset2("wkt");

            //隐藏、显示
            IRenderable obj = (IRenderable)axRenderControl.ObjectManager.GetObjectById(Guid.Empty);
            obj.VisibleMask = gviViewportMask.gviView0;
            obj.VisibleMask = gviViewportMask.gviViewNone;

            //切换到测量模式
            axRenderControl.InteractMode = gviInteractMode.gviInteractMeasurement;
            axRenderControl.MeasurementMode = gviMeasurementMode.gviMeasureCoordinate;//点坐标
            axRenderControl.MeasurementMode = gviMeasurementMode.gviMeasureAerialDistance;//直线距离
            axRenderControl.MeasurementMode = gviMeasurementMode.gviMeasureVerticalDistance;//垂直
            axRenderControl.MeasurementMode = gviMeasurementMode.gviMeasureArea;//面积

        }
        #endregion

        #region 数据相关

        /// <summary>
        /// 通过modelPoint查找model,修改model顶点，塞回数据源
        /// </summary>
        /// <param name="fdbPath"></param>
        void editModleNodeCoord(string fdbPath)
        {

            IConnectionInfo ci = new ConnectionInfo();
            ci.ConnectionType = gviConnectionType.gviConnectionFireBird2x;
            ci.Database = fdbPath;

            //获取数据源
            IDataSource ds = new DataSourceFactory().OpenDataSource(ci);
            string[] dataSetNames = ds.GetFeatureDatasetNames();
            IFeatureDataSet fds = ds.OpenFeatureDataset("fdsName");
            IResourceManager resourceM = (IResourceManager)fds;

            //获取所有featureClass
            string[] fcNames = fds.GetNamesByType(gviDataSetType.gviDataSetFeatureClassTable);
            for (int fci = 0; fci < fcNames.Length; fci++)
            {
                IFeatureClass fc = fds.OpenFeatureClass(fcNames[fci]);
                IFieldInfoCollection fieldInfos = fc.GetFields();
                for (int fieldi = 0; fieldi < fieldInfos.Count; fieldi++)
                {
                    IFieldInfo finfo = fieldInfos.Get(fieldi);
                    if (finfo.FieldType == gviFieldType.gviFieldGeometry)
                    {
                        //获取fc的几何属性及其索引，索引用于更新模型
                        aaa(fc, fieldi, resourceM);
                        break;
                    }
                }
            }
        }
        void aaa(IFeatureClass fc, int index, IResourceManager resourceM)
        {
            //无条件查询，即全选
            IFdeCursor sourceCursor = fc.Update(null);
            IRowBuffer row = null;
            //遍历feature
            while ((row = sourceCursor.NextRow()) != null)
            {
                //从feature中拿到几何属性
                IGeometry geom = (IGeometry)row.GetValue(index);
                //确定是模型
                if (geom.GeometryType == gviGeometryType.gviGeometryModelPoint)
                {
                    //转换为modelPoint
                    IModelPoint mp = (IModelPoint)geom;

                    //mp 的转换矩阵
                    IMatrix mx = mp.AsMatrix();

                    //获取模型实例
                    //注意：
                    //model可以被不同的modelPoint多次引用，需要修改模型时
                    //需要通过modelName判断一下，这个模型被修改过没有
                    IModel model = resourceM.GetModel(mp.ModelName);

                    //提取模型节点属性
                    if (model != null)
                    {
                        #region modelInside
                        //遍历DrawGroup
                        for (int dgrpi = 0; dgrpi < model.GroupCount; dgrpi++)
                        {
                            IDrawGroup dgrp = model.GetGroup(dgrpi);
                            if (dgrp != null)
                            {
                                //遍历DrawPrimitive
                                for (int dpri = 0; dpri < dgrp.PrimitiveCount; dpri++)
                                {
                                    IDrawPrimitive dpr = dgrp.GetPrimitive(dpri);
                                    if (dpr != null)
                                    {
                                        //获取顶点数组
                                        float[] verArray = dpr.VertexArray.Array;
                                        //创建新的顶点数组，替换原来的
                                        IFloatArray newArr = new FloatArray();

                                        //遍历数组，转为点，三个成员为一组，xyz
                                        for (int veri = 0; veri < verArray.Length; veri += 3)
                                        {
                                            Vector3 vec = new Vector3();
                                            vec.X = verArray[veri];
                                            vec.Y = verArray[veri + 1];
                                            vec.Z = verArray[veri + 2];

                                            //用矩阵转到决对坐标,并修改
                                            IVector3 refVec = mx.MultiplyVector(vec);

                                            //修改部份
                                            refVec.X = 3.3;
                                            refVec.Y = 4.4;
                                            refVec.Z = 5.5;

                                            //修改完，减掉mp中的位移，准备塞回modle
                                            newArr.Append((float)(refVec.X - mp.X));
                                            newArr.Append((float)(refVec.Y - mp.Y));
                                            newArr.Append((float)(refVec.Z - mp.Z));
                                        }
                                        //把新顶点数组塞入Primitive
                                        dpr.VertexArray = newArr;
                                        //再把Primitive更新到当前Group
                                        dgrp.SetPrimitive(dpri, dpr);
                                    }
                                }
                                //把组更新到当前model
                                model.SetGroup(dgrpi, dgrp);
                            }
                        }
                        //更新数据源
                        resourceM.UpdateModel(mp.ModelName, model);
                        resourceM.RebuildSimplifiedModel(mp.ModelName);//重建简模
                        //释放资源
                        model.Dispose();
                        model = null;
                        #endregion
                    }

                    //修改mp
                    mp.SetCoords(3.3, 4.4, 5.5, mp.M, mp.Id);
                    //塞回row
                    row.SetValue(index, mp);
                }
            }
        }
        #endregion

        #region 交互事件

        void action_ON()
        {
            //切换到选择模式
            axRenderControl.InteractMode = gviInteractMode.gviInteractSelect;
            //设置鼠标选择模式类型
            axRenderControl.MouseSelectMode = gviMouseSelectMode.gviMouseSelectClick //点选
                                            | gviMouseSelectMode.gviMouseSelectDrag; //框选 
            //设置选择目标类型过滤
            axRenderControl.MouseSelectObjectMask = gviMouseSelectObjectMask.gviSelectTileLayer //tdbx
                                            | gviMouseSelectObjectMask.gviSelectRenderGeometry; //renderObject
            //注册事件
            axRenderControl.RcMouseClickSelect += new _IRenderControlEvents_RcMouseClickSelectEventHandler(AxRenderControl_MouseClick);
            axRenderControl.RcMouseDragSelect += new _IRenderControlEvents_RcMouseDragSelectEventHandler(AxRenderControl_RcMouseDragSelect);
        }
        void action_OFF()
        {
            //切换到普通模式
            axRenderControl.InteractMode = gviInteractMode.gviInteractNormal;
            //注销事件
            axRenderControl.RcMouseClickSelect -= new _IRenderControlEvents_RcMouseClickSelectEventHandler(AxRenderControl_MouseClick);
            axRenderControl.RcMouseDragSelect -= new _IRenderControlEvents_RcMouseDragSelectEventHandler(AxRenderControl_RcMouseDragSelect);
        }

        private void AxRenderControl_RcMouseDragSelect(IPickResultCollection PickResults, gviModKeyMask Mask)
        {
            if (PickResults != null)
            {
                int i = 0;
                IPickResult t = null;
                while ((t = PickResults.Get(i)) != null)
                {
                    if (t.Type == gviObjectType.gviObjectRenderPoint)
                    {
                        IRenderPoint temp = ((IRenderPointPickResult)t).Point;
                    }

                    if (t.Type == gviObjectType.gviObject3DTileLayer)
                    {
                        I3DTileLayer temp = ((I3DTileLayerPickResult)t).TileLayer;
                    }

                    if (t.Type == gviObjectType.gviObjectRenderModelPoint)
                    {
                        IRenderModelPoint temp = ((IRenderModelPointPickResult)t).ModelPoint;
                    }
                }
            }
        }

        private void AxRenderControl_MouseClick(IPickResult IPickResult, IPoint IntersectPoint, gviModKeyMask Mask, gviMouseSelectMode EventSender)
        {
            if (IPickResult != null)
            {
                if (IPickResult.Type == gviObjectType.gviObjectRenderPoint)
                {
                    IRenderPoint temp = ((IRenderPointPickResult)IPickResult).Point;
                }

                if (IPickResult.Type == gviObjectType.gviObject3DTileLayer)
                {
                    I3DTileLayer temp = ((I3DTileLayerPickResult)IPickResult).TileLayer;
                }

                if (IPickResult.Type == gviObjectType.gviObjectRenderModelPoint)
                {
                    IRenderModelPoint temp = ((IRenderModelPointPickResult)IPickResult).ModelPoint;
                }
            }
        }

        #endregion
    }
}
