@ECHO OFF
Powershell -File ".\cake\build.ps1" -Script ".\cake\build.cake" -Verbosity Verbose
pause