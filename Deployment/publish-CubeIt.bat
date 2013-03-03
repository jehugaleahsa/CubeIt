msbuild ../CubeIt.sln /p:Configuration=Release
nuget pack ../CubeIt/CubeIt.csproj -Properties Configuration=Release
nuget push *.nupkg
del *.nupkg