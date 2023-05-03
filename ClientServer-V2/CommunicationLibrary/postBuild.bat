@echo off

set user_folder=m4ss

for /f %%a in ('powershell -command "Write-Output $env:USERPROFILE"') do set user_profile=%%a

if "%user_profile:~-4%"=="%user_folder%" (
	cd "C:\Users\m4ss\source\repos\Thesis2023\2223-otj-12-project-repo-csharp-mssevov18\ClientServerAPI\ClientServer-V2\CommunicationLibrary"
	xcopy ".\bin\Release\net6.0\CommunicationLibrary.dll" "..\..\..\..\..\..\..\Documents\libraries\CommunicationLibrary.dll" /Y
	xcopy ".\bin\Release\net6.0\CommunicationLibrary.dll" "..\..\..\libraries\CommunicationLibrary.dll" /Y
) else (
	echo You are not logged into the correct user folder.
)

