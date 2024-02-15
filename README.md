# PollingElasticsearch  
![png](https://imgur.com/goFzJJS.png)  

## Requirements  
- Unity 2022.3.12f1  
- URP 14.0.9  
- `UniTask`  
    - add Package from `UPM`  
Package Manager > Add packager from git URL ...  
https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask  
- `Json.NET (NuGet)`  
    1. preapare for `NuGet`  
        `manifest.json`
        ```json
        ...
        "scopedRegistries": [
            {
            "name": "Unity NuGet",
            "url": "https://unitynuget-registry.azurewebsites.net",
            "scopes": [
                "org.nuget"
                ]
            }
        ]
        ```
    2. reboot `Unity Editor`  
    3. add Package from `UPM`  
        Package Manager > Packages: My Registries > search "json"  
    - ref. https://www.newtonsoft.com/json  

## How to install  
1. add Package from `UPM`  
    Package Manager > Add packager from git URL ...  
    https://github.com/kasug623/PollingElasticsearch.git?path=Packages/PollingElasticsearch  
    ![png](https://imgur.com/YUnPUdB.png)  
2. allow http  
    Project Settings > Player > Allow donwload over HTTP*  
    ![png](https://imgur.com/uuRiSqA.png)  

## How to use
1. define QueryDSL
2. define Response
3. define DrawQueue
4. new polling agent

## Sample
- https://github.com/kasug623/ElasticTextVisualize