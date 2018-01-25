﻿using Gvitech.CityMaker.FdeCore;
using Gvitech.CityMaker.FdeGeometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDE
{
    class Program
    {
        ///联接器：gviConnectionSQLite3 不设置database就是内存数据库，超快，但数据不能固化到磁盘
        ///         设置database就是SDB
        ///         gviConnectionFireBird2x ,设置database就是FDB，可以给build或connect用
        static ConnectionInfo ci = new ConnectionInfo() { ConnectionType = gviConnectionType.gviConnectionSQLite3 };

        //数据源
        static IDataSource DS = new DataSourceFactory().CreateDataSource(ci, null);

        //数据集
        static IFeatureDataSet FDS = DS.CreateFeatureDataset("FDS", new CRSFactory().CreateCRS(gviCoordinateReferenceSystemType.gviCrsUnknown) as ISpatialCRS);

        static void Main(string[] args)
        {
            //创建要素类要先定义属性字段
            IFieldInfoCollection fields = new FieldInfoCollection();
            {
                //名称列
                IFieldInfo field = new FieldInfo();
                field.Name = "Name";
                field.FieldType = gviFieldType.gviFieldString;
                field.Length = 255;
                fields.Add(field);

                //空间列
                field = new FieldInfo();
                field.Name = "Geometry";
                field.FieldType = gviFieldType.gviFieldGeometry;    //列类型为空间类型
                field.GeometryDef = new GeometryDef();              //需要实例化一个几何定义对象,里面有大量与空间有关的内容，比如：
                field.GeometryDef.HasZ = true;                      //是否在Z值，默认为false,只能存XY
                field.GeometryDef.GeometryColumnType = gviGeometryColumnType.gviGeometryColumnPoint;//空间列的几何类型
                field.RegisteredRenderIndex = true;                 //注册渲染索引，允许在renderControl中显示
                fields.Add(field);

                //多空间列
                field = new FieldInfo();
                field.Name = "Model";
                field.FieldType = gviFieldType.gviFieldGeometry;    //列类型为空间类型
                field.GeometryDef = new GeometryDef();              //需要实例化一个几何定义对象,里面有大量与空间有关的内容，比如：
                field.GeometryDef.HasZ = true;                      //是否在Z值，默认为false,只能存XY
                field.GeometryDef.GeometryColumnType = gviGeometryColumnType.gviGeometryColumnModelPoint;//空间列的几何类型
                field.RegisteredRenderIndex = true;                 //注册渲染索引，允许在renderControl中显示
                fields.Add(field);
                //... 其它列
            }

            //创建要素类
            IFeatureClass FC = FDS.CreateFeatureClass("FC", fields);

            int nameID = FC.GetFields().IndexOf("Name");        //获取对应列的索引，
            int geomID = FC.GetFields().IndexOf("Geometry");

            //增
            IFdeCursor fcu = FC.Insert();                       //通过FC创建一个插入游标，用来操作rowBuffer
            IRowBuffer rb = FC.CreateRowBuffer();               //通过FC创建一条空要素实例，用来设置数据，设置完成后将其塞入FC
            IPoint p = new GeometryFactory().CreatePoint(gviVertexAttribute.gviVertexAttributeZ);   // 将要塞入空间列的几何对象，类型要对应 FC的field.GeometryDef.GeometryColumnType ，否则塞不进
            rb.SetValue(nameID, "testPointA");                  //塞名称
            rb.SetValue(geomID, p);                             //塞几何
            fcu.InsertRow(rb);                                  //塞进FC
            int oid = fcu.LastInsertId;                         //成功后反回这条buffer的主键ID

            //查
            IQueryFilter qf = new QueryFilter();            //过滤器
            IQueryDef qd = new QueryDef();
            IFdeCursor res = FC.Search(qf, true);           //条件查
            res = FC.Search(null, true);                    //全选
            IRowBuffer aBuffer = res.NextRow();             //遍历查询结果

            //改
            IFdeCursor upCu = FC.Update(qf);
            IRowBuffer upBuffer = upCu.NextRow();
            upCu.UpdateRow(upBuffer);

            //删
            FC.Delete(qf);
            FC.DeleteRow(0);
            FC.Truncate();          //保留表结构，清空表

            
        }
    }
}
