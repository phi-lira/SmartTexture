# SmartTexture Asset for Unity
SmartTexture is a custom asset for Unity that allows you a [channel packing](http://wiki.polycount.com/wiki/ChannelPacking) workflow in the editortextures and use them in the Unity editor for a streamlined workflow.
SmartTextures work as a regular 2D texture asset and you can assign it to material inspectors.

Dependency tracking is handled by SmartTexture, that means you can change input textures and the texture asset will be re-generated. The input textures are editor only dependencies, they will not be included in the build, unless they are referenced by another asset or scene.

<img width="347" alt="inspector" src="https://user-images.githubusercontent.com/7453395/82161433-dbe8ab00-989c-11ea-9003-10e8ca867bfe.png">

## Installation
SmartTexture is a unity package and you can install it from Package Manager.

Option 1: [Install package via Github](https://docs.unity3d.com/Manual/upm-ui-giturl.html).
Option 2: Clone or download this Github project and [install it as a local package](https://docs.unity3d.com/Manual/upm-ui-local.html).

## How to use
1) Create a SmartTexture asset by clicking on `Asset -> Create -> Smart Texture`, or by right-clicking on the Project Window and then `Create -> Smart Texture`.
<img width="870" alt="create" src="https://user-images.githubusercontent.com/7453395/82161430-d9865100-989c-11ea-9497-19d1cf77fed9.png">

2) An asset will be created in your project.
<img width="378" alt="asset" src="https://user-images.githubusercontent.com/7453395/82161427-d68b6080-989c-11ea-9fae-1d65e06ad3d6.png">

3) On the asset inspector you can configure input textures and texture settings for your smart texture.
4) Hit `Apply` button to generate the texture with selected settings.
<img width="347" alt="inspector" src="https://user-images.githubusercontent.com/7453395/82161433-dbe8ab00-989c-11ea-9003-10e8ca867bfe.png">

5) Now you can use this texture as any regular 2D texture in your project.
<img width="524" alt="assign" src="https://user-images.githubusercontent.com/7453395/82161435-de4b0500-989c-11ea-9784-e4a9b9403120.png">
