echo off

:: set your unity path here
set unityPath=D:\Program Files\Unity\Editor

:: set your unity project path here
set projectPath=F:\XWorld\UI

:: invoke this method to build
:: don't change this manually
set command=JC.BuildAssetBundle.BuildDo

echo startup unity and build asset bundle
echo this step may take several minutes depends on the size of your asset bundle
echo please wait...

:: start up unity and build
"%unityPath%"\Unity.exe -projectPath %projectPath% -quit -batchmode -executeMethod %command%

echo build asset bundle complete

pause