; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{30814DF2-591F-4FDF-8706-580E476DE8A2}
AppName=TELEMAQUE - UnisBox
AppVersion=1.0.10.1
;AppVerName=TELEMAQUE - UnisBox 1.1.0.0
AppPublisher=Safeware
AppPublisherURL=http://www.safeware.fr/
AppSupportURL=http://www.safeware.fr/
;AppUpdatesURL=http://www.safeware.fr/
DefaultDirName={pf}\Safeware\TELEMAQUE - UnisBox
DisableDirPage=yes
DefaultGroupName=TELEMAQUE - UnisBox
DisableProgramGroupPage=yes
OutputBaseFilename=setupTELEMAQUEUnisBox
SetupIconFile=D:\ZwDev\SandBox\SandBox\bin\x86\Release\Resources\Telemaque.ico
Compression=lzma
SolidCompression=yes
AppCopyright=Copyright � 2015 Safeware.


[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
;Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Tasks]
;Name: "DotNetFramework"; Description: ".NET Framework 2.0"; GroupDescription: "If .NET is NOT installed:";

[Files]
Source: "D:\ZwDev\SandBox\SandBox\bin\x86\Release\TELEMAQUE - UnisBox.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\ZwDev\SandBox\SandBox\bin\x86\Release\*"; DestDir: "{app}"; Excludes: "\log\*, TELEMAQUE - UnisBox.application, TELEMAQUE - UnisBox.exe.manifest, user.config.lnk"; Flags: ignoreversion
Source: "D:\ZwDev\SandBox\SandBox\bin\x86\Release\Resources\Biovein\*"; DestDir: "{app}\Resources\Biovein"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "D:\ZwDev\SandBox\SandBox\bin\x86\Release\Resources\EasyRonde\*"; DestDir: "{app}\Resources\EasyRonde"; Flags: ignoreversion recursesubdirs createallsubdirs
;Source: "D:\ZwDev\SandBox\SandBox\bin\x86\Release\Resources\IrisCard\*"; DestDir: "{app}\Resources\IrisCard"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "D:\ZwDev\SandBox\SandBox\bin\x86\Release\Resources\MIL100\*"; DestDir: "{app}\Resources\MIL100"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "D:\ZwDev\SandBox\SandBox\bin\x86\Release\Resources\Tracing\*"; DestDir: "{app}\Resources\Tracing"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "D:\ZwDev\SandBox\SandBox\bin\x86\Release\Resources\ZalixVeinSecure\*"; DestDir: "{app}\Resources\ZalixVeinSecure"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "D:\ZwDev\SandBox\SandBox\bin\x86\Release\Resources\*"; DestDir: "{app}\Resources\"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{commonprograms}\Safeware\TELEMAQUE\TELEMAQUE - UnisBox"; Filename: "{app}\TELEMAQUE - UnisBox.exe"
Name: "{commonprograms}\Safeware\TELEMAQUE\Log"; Filename: "{app}\log"
Name: "{commonprograms}\Safeware\TELEMAQUE\app.config"; Filename: "{app}\TELEMAQUE - UnisBox.exe.config"
Name: "{commonprograms}\Safeware\TELEMAQUE\user.config"; Filename: "{app}"
Name: "{commonprograms}\Safeware\TELEMAQUE\{cm:UninstallProgram,TELEMAQUE - UnisBox}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\TELEMAQUE - UnisBox"; Filename: "{app}\TELEMAQUE - UnisBox.exe"

[Run]
;Filename: "{app}\TELEMAQUE - UnisBox.exe"; Parameters: "{param:url} ""TELEMAQUE"""; Description: "{cm:LaunchProgram,TELEMAQUE - UnisBox}"; Flags: postinstall ;


[Code]
var
lblURL: TLabel;
lblURLPortal: TLabel;
txtURL: TEdit; 
txtURLPortal: TEdit; 

// Run TELEMAQUE after install
function NextButtonClick(CurPageID: Integer): Boolean;
var
  ResultCode: Integer;
begin
  Result := True;
  if CurPageID = wpFinished then
  begin
    //MsgBox(ExpandConstant('{param:url}'), mbInformation, MB_OK);
    if txtURLPortal.Text <> '' then begin
      ExecAsOriginalUser(ExpandConstant('{app}\TELEMAQUE - UnisBox.exe'), txtURL.Text+ ' TELEMAQUE '+ txtURLPortal.Text, '', SW_SHOWNORMAL, ewNoWait, ResultCode);
    end else begin
      ExecAsOriginalUser(ExpandConstant('{app}\TELEMAQUE - UnisBox.exe'), txtURL.Text+ ' TELEMAQUE', '', SW_SHOWNORMAL, ewNoWait, ResultCode);
    end;
  end;
end;

// Framework
function IsDotNetDetected(version: string; service: cardinal): boolean;
// Indicates whether the specified version and service pack of the .NET Framework is installed.
//
// version -- Specify one of these strings for the required .NET Framework version:
//    'v1.1.4322'     .NET Framework 1.1
//    'v2.0.50727'    .NET Framework 2.0
//    'v3.0'          .NET Framework 3.0
//    'v3.5'          .NET Framework 3.5
//    'v4\Client'     .NET Framework 4.0 Client Profile
//    'v4\Full'       .NET Framework 4.0 Full Installation
//    'v4.5'          .NET Framework 4.5
//
// service -- Specify any non-negative integer for the required service pack level:
//    0               No service packs required
//    1, 2, etc.      Service pack 1, 2, etc. required
var
    key: string;
    install, release, serviceCount: cardinal;
    check45, success: boolean;
begin
    // .NET 4.5 installs as update to .NET 4.0 Full
    if version = 'v4.5' then begin
        version := 'v4\Full';
        check45 := true;
    end else
        check45 := false;

    // installation key group for all .NET versions
    key := 'SOFTWARE\Microsoft\NET Framework Setup\NDP\' + version;

    // .NET 3.0 uses value InstallSuccess in subkey Setup
    if Pos('v3.0', version) = 1 then begin
        success := RegQueryDWordValue(HKLM, key + '\Setup', 'InstallSuccess', install);
    end else begin
        success := RegQueryDWordValue(HKLM, key, 'Install', install);
    end;

    // .NET 4.0/4.5 uses value Servicing instead of SP
    if Pos('v4', version) = 1 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Servicing', serviceCount);
    end else begin
        success := success and RegQueryDWordValue(HKLM, key, 'SP', serviceCount);
    end;

    // .NET 4.5 uses additional value Release
    if check45 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Release', release);
        success := success and (release >= 378389);
    end;

    result := success and (install = 1) and (serviceCount >= service);
end;


function InitializeSetup(): Boolean;
begin
    if not IsDotNetDetected('v2.0.50727', 0) then begin
        MsgBox('MyApp requires Microsoft .NET Framework 2.0.'#13#13
            'Please use Windows Update to install this version,'#13
            'and then re-run the setup program.', mbInformation, MB_OK);
        result := false;
    end else
        result := true;

end;


procedure frmDomainReg_Activate(Page: TWizardPage);
begin
end;

function frmDomainReg_ShouldSkipPage(Page: TWizardPage): Boolean;
begin
Result := False;
end;

function frmDomainReg_BackButtonClick(Page: TWizardPage): Boolean;
begin
Result := True;
end;

function frmDomainReg_NextButtonClick(Page: TWizardPage): Boolean;
begin
Result := True;
end;

procedure frmDomainReg_CancelButtonClick(Page: TWizardPage; var Cancel, Confirm: Boolean);
begin
end;

function frmDomainReg_CreatePage(PreviousPageId: Integer): Integer;
var
Page: TWizardPage;
begin
Page := CreateCustomPage(
PreviousPageId,
'URL Registration',
'Check or entrer URL TELEMAQUE'
);

{ lblURL }
lblURL := TLabel.Create(Page);
with lblURL do
begin
Parent := Page.Surface;
Left := ScaleX(0);
Top := ScaleY(25);
Width := ScaleX(35);
Height := ScaleY(13);
Caption := 'URL';
end;

{ lblURLPortal }
lblURLPortal := TLabel.Create(Page);
with lblURLPortal do
begin
Parent := Page.Surface;
Left := ScaleX(0);
Top := ScaleY(95);
Width := ScaleX(35);
Height := ScaleY(13);
Caption := 'URL Portal';
end;

{ txtURL }
txtURL := TEdit.Create(Page);
with txtURL do
begin
Parent := Page.Surface;
Left := ScaleX(60);
Top := ScaleY(23);
Width := ScaleX(350);
Height := ScaleY(21);
TabOrder := 0;
Text := ExpandConstant('{param:url}');
end;

if txtURl.text='' then begin
  txtURl.text:='http://'
end;

{ txtURLPortal }
txtURLPortal := TEdit.Create(Page);
with txtURLPortal do
begin
Parent := Page.Surface;
Left := ScaleX(60);
Top := ScaleY(93);
Width := ScaleX(350);
Height := ScaleY(21);
TabOrder := 0;
Text := ExpandConstant('{param:urlPortal}');
end;

with Page do
begin
OnActivate := @frmDomainReg_Activate;
OnShouldSkipPage := @frmDomainReg_ShouldSkipPage;
OnBackButtonClick := @frmDomainReg_BackButtonClick;
OnNextButtonClick := @frmDomainReg_NextButtonClick;
OnCancelButtonClick := @frmDomainReg_CancelButtonClick;
end;

Result := Page.ID;
end;

procedure InitializeWizard();
begin
{this page will come after welcome page}

  frmDomainReg_CreatePage(wpWelcome);
end;

