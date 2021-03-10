# ScriptGraphicHelper

**一款简单好用的图色助手,  快速生成多种脚本开发工具的图色格式代码**

&nbsp;

# 特色功能

- 调用模拟器命令行进行截图, 无需手动连接adb
- 调用大漠进行前后台截图
- 支持大漠、按键、触动、autojs、easyclick 以及自定义的格式代码生成
- 多分辨率适配的测试和代码生成(锚点格式)
- 跟随鼠标的放大镜

&nbsp;

# 展示



![](screenshot/record.gif)


&nbsp;

由于[runtimelab](https://github.com/dotnet/runtimelab/tree/feature/NativeAOT)的AOT编译目前不支持WPF, 所以此分支是基于[avalonia](https://github.com/AvaloniaUI/Avalonia)框架重构的实验性分支

句柄模式由gRPC方案实现, 原因是AOT编译目前不支持32位以及com互操作, 而大漠的com对象是32位的

&nbsp;

# 和master分支的区别

- 使用AOT编译的性能开销理论上较低
- 暂不支持裁图功能
- 去除偏色系列功能

&nbsp;

# AOT编译流程



在项目文件csproj所在目录打开终端, 运行

`dotnet publish -c Release -r win-x64`

