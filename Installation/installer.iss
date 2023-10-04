; Uncomment the following lines to run via Inno Setup IDE.
;#define DMAppName "BRIO Docs"
;#define DMAppVersion "dev"
;#define DMSourceBuild "..\Brio.Docs.Api\bin\Release\net5.0\"
;#define DMUpdater "..\Brio.Docs.Updater\bin\Release\net5.0\"

#define DMAppExeName "Brio.Docs.Api.exe"
#define MrsPublisher "BRIO MRS"
#define DMProgramDataPath "\" + MrsPublisher + "\" + DMAppName + "\"
#define DMDatabaseName "DocumentManagement.db"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{6D64C7AD-5072-4141-992E-CACB4901C3C5}
AppMutex=Global\Brio.Docs.Api
AppName={#DMAppName}
AppVersion={#DMAppVersion}
AppPublisher={#MrsPublisher}
DefaultDirName={autopf}\{#MrsPublisher}\{#DMAppName}
DefaultGroupName={#MrsPublisher}\{#DMAppName}
DisableProgramGroupPage=yes
OutputBaseFilename="{#DMAppName} Setup {#DMAppVersion}"
OutputDir=..\
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[Files]
Source: "{#DMSourceBuild}{#DmAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#DMSourceBuild}*"; Excludes: "{#DMDatabaseName}*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#DMSourceBuild}{#DMDatabaseName}*"; DestDir: "{commonappdata}{#DMProgramDataPath}"; Permissions: users-modify; Flags: onlyifdoesntexist uninsneveruninstall
Source: "{#DMUpdater}*"; DestDir: "{commonappdata}{#DMProgramDataPath}"; Flags: deleteafterinstall
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#DMAppName}"; Filename: "{app}\{#DMAppExeName}"

[CustomMessages]
english.RemoveDB=Do you want to remove all projects and objectives from this local machine?
russian.RemoveDB=Вы хотите удалить все проекты и задачи с этого локального компьютера?

[Run]
Filename: "{commonappdata}{#DMProgramDataPath}Brio.Docs.Updater.exe"; Flags: runhidden
Filename: "briolauncher://remove/{#DMAppName}/{code:GetPreviousVersion}"; Check: NeedRemoveFromBrioLauncher(); Flags: shellexec runascurrentuser nowait
Filename: "briolauncher://register/{#DMAppName}/{#DMAppVersion}/{app}\{#DMAppExeName}?background=true"; Flags: shellexec runascurrentuser nowait
Filename: "{app}\{#DMAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(DMAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]  
Name: "{commonappdata}{#DMProgramDataPath}Logs"; Type: filesandordirs          
Name: "{commonappdata}{#DMProgramDataPath}"; Type: dirifempty
Name: "{commonappdata}\{#MrsPublisher}"; Type: dirifempty

[UninstallRun]
Filename: "briolauncher://remove/{#DMAppName}/{#DMAppVersion}"; Flags: shellexec runascurrentuser nowait;

[Code]
var
  hasInstalledVersion: Boolean;
  installedVersion: String;

function InitializeSetup(): Boolean;
  begin
    hasInstalledVersion:=RegQueryStringValue(HKLM, ExpandConstant('SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{#SetupSetting("AppId")}_is1'), 'DisplayVersion', installedVersion);
    Result:=True;
  end;

{https://stackoverflow.com/a/20181221}
function FileReplaceString(const FileName, SearchString, ReplaceString: string):boolean;
var
  MyFile : TStrings;
  MyText : string;
begin
  MyFile := TStringList.Create;

  try
    result := true;

    try
      MyFile.LoadFromFile(FileName);
      MyText := MyFile.Text;

      { Only save if text has been changed. }
      if StringChangeEx(MyText, SearchString, ReplaceString, True) > 0 then
      begin;
        MyFile.Text := MyText;
        MyFile.SaveToFile(FileName);
      end;
    except
      result := false;
    end;
  finally
    MyFile.Free;
  end;
end;

function EncodePathForJson(var path : string) : string;
  begin
    StringChangeEx(result, '\', '\\', True);
  end;

procedure CurStepChanged(CurStep: TSetupStep);
  var
    newPath : string;
    jsonPath : string;
  begin
    case CurStep of
    ssPostInstall:
      begin
        jsonPath := ExpandConstant('{app}\appsettings.json');
        newPath := ExpandConstant('{commonappdata}{#DMProgramDataPath}{#DMDatabaseName}');
        newPath := EncodePathForJson(newPath);
        FileReplaceString(jsonPath, ExpandConstant('{#DMDatabaseName}'), newPath);
        newPath := ExpandConstant('{commonappdata}{#DMProgramDataPath}Logs\main.log');
        newPath := EncodePathForJson(newPath);
        FileReplaceString(jsonPath, 'Logs\\main.log', newPath);
      end;
    end;
  end;

function GetPreviousVersion(Param: string): String;
  begin
    Result := installedVersion;
  end;

function NeedRemoveFromBrioLauncher(): Boolean;
  var
    isVersionEmpty: Boolean;
  begin
    isVersionEmpty := installedVersion<>'';
    Result := hasInstalledVersion AND isVersionEmpty;
  end;

{https://stackoverflow.com/a/30815615}
procedure CurUninstallStepChanged (CurUninstallStep: TUninstallStep);
 var
     mres : integer;
 begin
    case CurUninstallStep of                   
      usPostUninstall:
        begin
          mres := MsgBox(ExpandConstant('{cm:RemoveDB}'), mbConfirmation, MB_YESNO or MB_DEFBUTTON2)
          if mres = IDYES then
            DelTree(ExpandConstant('{commonappdata}{#DMProgramDataPath}{#DMDatabaseName}*'), False, True, False);
       end;
   end;
end;