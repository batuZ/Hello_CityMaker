# Hello_CityMaker

## plugin
	1、配置一个.addin文件，如例connectTestPlugin.addin
	2、创建一个类，继承AbstractCommand，其中run对应一个按钮的动作
	3、dll和addin 放在 connect/AddIns下

## toolBox
	1、创建一个类，继承AbstractCommand，其中run对应一条功能，CommandName 是显示出来的工具名称
	2、dll 放在 connect/ToolBox下