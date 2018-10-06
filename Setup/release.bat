@echo off
echo Scott Clayton 2015
echo CAPTCHA Breaking Library and Scripting Language

set version=v0.5.1

7z a "CBL-Examples-%DATE:~7,2%.%DATE:~4,2%.%DATE:~-4%-%version%".zip "..\Examples\*" -r
7z a "CBL-Interpreter-%DATE:~7,2%.%DATE:~4,2%.%DATE:~-4%-%version%".zip "..\CBL.Interpreter\bin\Release\*" -r -x!*.pdb -x!*.vshost.exe -x!*.xml -x!*.zip -x!*.manifest
7z a "CBL-Interface-%DATE:~7,2%.%DATE:~4,2%.%DATE:~-4%-%version%".zip "..\CBL.Interface\bin\Release\*" -r -x!*.pdb -x!*.vshost.exe -x!*.xml -x!*.zip -x!*.manifest

pause