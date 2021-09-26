dotnet publish -r win-x64 -p:PublishSingleFile=true -c Release --self-contained

set "wpfl_updater_application_path=.\WPF_Minecraft_Launcher_Updater\bin\Release\net5.0\win-x64\publish"
set "wpfl_main_updater_directory_path=.\WPF-Minecraft-Launcher\bin\Release\net5.0\win-x64\publish\update"

if not exist %wpfl_main_updater_directory_path% mkdir %wpfl_main_updater_directory_path%
xcopy /s /Y %wpfl_updater_application_path%\clrcompression.dll %wpfl_main_updater_directory_path%
xcopy /s /Y %wpfl_updater_application_path%\clrjit.dll %wpfl_main_updater_directory_path%
xcopy /s /Y %wpfl_updater_application_path%\coreclr.dll %wpfl_main_updater_directory_path%
xcopy /s /Y %wpfl_updater_application_path%\mscordaccore.dll %wpfl_main_updater_directory_path%
xcopy /s /Y %wpfl_updater_application_path%\update.exe %wpfl_main_updater_directory_path%
xcopy /s /Y %wpfl_updater_application_path%\update.pdb %wpfl_main_updater_directory_path%

pause