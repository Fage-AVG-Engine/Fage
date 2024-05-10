# FAGE
Fantastic Adv Game Engine(neither "FAGE Adv Game Engine" nor My nickname FaGe) #(滑稽)  

## ReadMe of Other Languages
- [简体中文](README.md)
- ~~English~~ not completed yet, supposed to be README_en.md.


## 构建步骤
`
1. `git clone`这个仓库，先不要急着cd，还有别的依赖项。
1. 在同一个目录`git clone`本组织的FageScript。
1. 如果一切正常，得到的应该是这样的目录结构：  
  \- 最外层的文件夹  
  -- Fage  
  -- FageScript
1. `dotnet build`，介意乱码可以把`DOTNET_CLI_UI_LANGUAGE`环境变量设置成`en`。
1. 完成，去`Fage.Runtime\bin\{配置，默认Debug}\{.NET框架}`找输出。
1. 满意可以执行`dotnet pack`，创建NuGet包。

### 其它注意事项
现阶段代码库中多处硬编码了`LXGWNeoXiHei.ttf`。要在不设置默认字体的情况下使用该引擎，需要去[霞鹜新晰黑发布页面](https://github.com/lxgw/LxgwNeoXiHei/releases)下载它，并将它放置在游戏项目的`Contents`目录中，然后将它作为“内容”（MSBuild Content项），最后设置成复制到输出才行。以后可能会做一个资产NuGet包吧，但我现在想摸鱼。

## 文档
tan90°，bug还没修完

## 致谢
* 感谢GitHub @lxgw 设计的[霞鹜新晰黑 / LXGW Neo XiHei](https://github.com/lxgw/LxgwNeoXiHei/)，
  本项目将它作为系统默认字体和文本默认字体使用。使用请遵守IPA Font License 1.0许可。