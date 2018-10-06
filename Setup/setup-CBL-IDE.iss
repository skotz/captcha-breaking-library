; CBL Installer Script
; Scott Clayton 2012

#define SetupBaseName  "Setup.CBL-Full."    
#define DateTimeString GetDateTimeString('yyyy.mm.dd.hh.nn', '', '');

[Setup]
AppId={{54C9870B-6049-44C2-A5A1-24F0B0EDDDDD}
AppName=CAPTCHA Breaking Scripting Language IDE
AppVersion=1.0
AppPublisher=Scott Clayton
AppPublisherURL=http://code.google.com/p/captcha-breaking-library/
AppSupportURL=http://code.google.com/p/captcha-breaking-library/
AppUpdatesURL=http://code.google.com/p/captcha-breaking-library/
DefaultDirName={pf}\CAPTCHA Breaking Scripting Language
DefaultGroupName=CAPTCHA Breaking Scripting Language with IDE
OutputDir=.
OutputBaseFilename={#SetupBaseName + DateTimeString}
SetupIconFile=.\ART\ide001.ico
Compression=lzma
SolidCompression=yes        
LicenseFile=.\LICENSE.txt
;WizardImageFile=.\art\setup.bmp
;WizardSmallImageFile=.\art\setupUR.bmp

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]   
Source: ".\CAPTCHA Breaker Compiled\bin\Release\CBL-GUI.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\CAPTCHA Breaker IDE\bin\Release\CBL-IDE.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\CAPTCHA Breaker Compiled\bin\Release\AForge.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\CAPTCHA Breaker Compiled\bin\Release\AForge.Imaging.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\CAPTCHA Breaker Compiled\bin\Release\AForge.Math.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\CAPTCHA Breaker Compiled\bin\Release\ContourAnalysis.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\CAPTCHA Breaker Compiled\bin\Release\ContourAnalysisProcessing.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\CAPTCHA Breaker Compiled\bin\Release\Emgu.CV.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\CAPTCHA Breaker Compiled\bin\Release\Emgu.Util.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\CAPTCHA Breaker Compiled\bin\Release\opencv_core220.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\CAPTCHA Breaker Compiled\bin\Release\opencv_highgui220.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\CAPTCHA Breaker Compiled\bin\Release\opencv_imgproc220.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\CAPTCHA Breaker Compiled\bin\Release\ScottClayton.CAPTCHA.dll"; DestDir: "{app}"; Flags: ignoreversion   
Source: ".\CAPTCHA Breaker Compiled\bin\Release\ScottClayton.Interpreter.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\ART\icon04d2.ico"; DestDir: "{app}"; Flags: ignoreversion

[Icons]  
Name: "{group}\CBL Script Runner"; Filename: "{app}\CBL-GUI.exe"
Name: "{group}\CBL Developer IDE"; Filename: "{app}\CBL-IDE.exe"
Name: "{group}\{cm:ProgramOnTheWeb,CAPTCHA Breaking Scripting Language}"; Filename: "http://code.google.com/p/captcha-breaking-library/"
Name: "{group}\{cm:UninstallProgram,CAPTCHA Breaking Scripting Language}"; Filename: "{uninstallexe}"    
Name: "{commondesktop}\CBL Script Runner"; Filename: "{app}\CBL-GUI.exe"; Tasks: desktopicon
Name: "{commondesktop}\CBL Developer IDE"; Filename: "{app}\CBL-IDE.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\CBL-IDE.exe"; Description: "{cm:LaunchProgram,CBL Developer IDE}"; Flags: nowait postinstall skipifsilent    
Filename: "{app}\CBL-GUI.exe"; Description: "{cm:LaunchProgram,CBL Script Runner}"; Flags: nowait postinstall skipifsilent

[Registry]
Root: HKCR; Subkey: ".captcha"; ValueType: string; ValueName: ""; ValueData: "CBLScript"; Flags: uninsdeletevalue
Root: HKCR; Subkey: "CBLScript"; ValueType: string; ValueName: ""; ValueData: "CBL Script"; Flags: uninsdeletekey
Root: HKCR; Subkey: "CBLScript\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: """{app}\icon04d2.ico"""
Root: HKCR; Subkey: "CBLScript\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\CBL-GUI.exe"" ""%1"""

