# Hello_CityMaker

## plugin
	1、配置一个.addin文件，如例connectTestPlugin.addin
	2、创建一个类，继承AbstractCommand，其中run对应一个按钮的动作
	3、dll和addin 放在 connect/AddIns下
	4、所有引用库编译时不拷贝

## toolBox
	1、创建一个类，继承AbstractCommand，其中run对应一条功能，CommandName 是显示出来的工具名称
	2、dll 放在 connect/ToolBox下
	3、所有引用库编译时不拷贝

## FDE
	创建FDE数据库,增删改查
	ILicenseServer lic = new LicenseServer();
	lic.SetHost("192.168.2.200",8588,"")

## 批量处理FDB中的模型
	案例为关闭光照
	需要加密锁及7.1Runtime
