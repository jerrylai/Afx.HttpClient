@echo off
set Build="%SYSTEMDRIVE%\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MsBuild.exe"
if exist publish rd /s /q publish
%Build% "NET20/Afx.HttpClient/Afx.HttpClient.csproj" /t:Rebuild /p:Configuration=Release
%Build% "NET40/Afx.HttpClient/Afx.HttpClient.csproj" /t:Rebuild /p:Configuration=Release
%Build% "NET45/Afx.HttpClient/Afx.HttpClient.csproj" /t:Rebuild /p:Configuration=Release
%Build% "NET451/Afx.HttpClient/Afx.HttpClient.csproj" /t:Rebuild /p:Configuration=Release
%Build% "NET452/Afx.HttpClient/Afx.HttpClient.csproj" /t:Rebuild /p:Configuration=Release
%Build% "NET46/Afx.HttpClient/Afx.HttpClient.csproj" /t:Rebuild /p:Configuration=Release
%Build% "NET461/Afx.HttpClient/Afx.HttpClient.csproj" /t:Rebuild /p:Configuration=Release
%Build% "NET462/Afx.HttpClient/Afx.HttpClient.csproj" /t:Rebuild /p:Configuration=Release
%Build% "NET47/Afx.HttpClient/Afx.HttpClient.csproj" /t:Rebuild /p:Configuration=Release
%Build% "NET471/Afx.HttpClient/Afx.HttpClient.csproj" /t:Rebuild /p:Configuration=Release
%Build% "NET472/Afx.HttpClient/Afx.HttpClient.csproj" /t:Rebuild /p:Configuration=Release
%Build% "NET48/Afx.HttpClient/Afx.HttpClient.csproj" /t:Rebuild /p:Configuration=Release
dotnet build "NETStandard2.0/Afx.HttpClient/Afx.HttpClient.csproj" -c Release
dotnet build "NETStandard2.1/Afx.HttpClient/Afx.HttpClient.csproj" -c Release
cd publish
del /q/s *.pdb
pause