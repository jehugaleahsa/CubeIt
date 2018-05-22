&dotnet pack "..\CubeIt\CubeIt.csproj" --configuration Release --output $PWD

.\NuGet.exe push CubeIt.*.nupkg -Source https://www.nuget.org/api/v2/package

Remove-Item CubeIt.*.nupkg