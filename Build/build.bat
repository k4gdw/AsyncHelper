msbuild AsyncHelper.xml /t:Deploy,Package
copy package\*.nupkg ..\..\nugetPackages
del package\*.nupkg
