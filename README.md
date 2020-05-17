# SmartTexture Asset for Unity
SmartTexture is a custom asset for Unity that allows you a [channel packing](http://wiki.polycount.com/wiki/ChannelPacking) workflow in the editortextures and use them in the Unity editor for a streamlined workflow.
SmartTextures work as a regular 2D texture asset and you can assign it to material inspectors.

Dependency tracking is handled by SmartTexture, that means you can change input textures and the texture asset will be re-generated. The input textures are editor only dependencies, they will not be included in the build, unless they are referenced by another asset or scene.

## Installation
SmartTexture is a unity package and you can install it from Package Manager.
To install it from Github package, [follow these instructions](https://docs.unity3d.com/Manual/upm-ui-giturl.html).
To install if as a local package, clone or download this Github project and install it as a local package [following these instructions](https://docs.unity3d.com/Manual/upm-ui-local.html).

## How to use
1) Create a SmartTexture asset by clicking on `Asset -> Create -> Smart Texture`, or by right-clicking on the Project Window and then `Create -> Smart Texture`.
2) An asset will be created in your project.
3) On the asset inspector you can configure input textures and texture settings for your smart texture.
4) Hit `Apply` button to generate the texture with selected settings.
5) Now you can use this texture as any regular 2D texture in your project.

