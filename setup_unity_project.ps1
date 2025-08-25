# Virtual Tokyo Matching - Unity Project Setup Script (PowerShell)
# For Windows environments with VCC/VPM CLI

param(
    [string]$ProjectPath = "$env:USERPROFILE\projects\VirtualTokyoMatching",
    [switch]$SkipLaunch = $false
)

# Configuration
$ProjectName = "VirtualTokyoMatching"
$VCCSettingsPath = "$env:LOCALAPPDATA\VRChatCreatorCompanion\settings.json"

Write-Host "=== Virtual Tokyo Matching - Unity Project Setup ===" -ForegroundColor Green
Write-Host "Setting up VRChat world development environment..." -ForegroundColor Yellow

# Phase 1: Prerequisites Check
Write-Host "`n=== Phase 1: Prerequisites Check ===" -ForegroundColor Cyan

Write-Host "Checking .NET SDK..." -ForegroundColor Green
try {
    $dotnetVersion = dotnet --version
    Write-Host ".NET SDK version: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Error ".NET SDK not found. Please install .NET 8.0 or higher."
    exit 1
}

Write-Host "Checking Unity Hub..." -ForegroundColor Green
$unityHubPath = Get-Command "Unity Hub" -ErrorAction SilentlyContinue
if (-not $unityHubPath) {
    # Try common Unity Hub locations on Windows
    $commonPaths = @(
        "$env:ProgramFiles\Unity Hub\Unity Hub.exe",
        "$env:ProgramFiles(x86)\Unity Hub\Unity Hub.exe",
        "$env:LOCALAPPDATA\Programs\Unity Hub\Unity Hub.exe"
    )
    
    foreach ($path in $commonPaths) {
        if (Test-Path $path) {
            $unityHubPath = $path
            break
        }
    }
    
    if (-not $unityHubPath) {
        Write-Error "Unity Hub not found. Please install Unity Hub."
        exit 1
    }
} else {
    $unityHubPath = $unityHubPath.Source
}

Write-Host "Unity Hub found at: $unityHubPath" -ForegroundColor Green

Write-Host "Checking Unity Editor..." -ForegroundColor Green
$unityEditorPaths = Get-ChildItem "$env:ProgramFiles\Unity\Hub\Editor" -Directory -ErrorAction SilentlyContinue | 
    Where-Object { $_.Name -match "2022\.3\.\d+f\d+" } |
    ForEach-Object { Join-Path $_.FullName "Editor\Unity.exe" } |
    Where-Object { Test-Path $_ }

if (-not $unityEditorPaths -or $unityEditorPaths.Count -eq 0) {
    Write-Error "Unity 2022.3 LTS Editor not found. Please install via Unity Hub."
    exit 1
}

$unityEditorPath = $unityEditorPaths[0]
$unityVersion = Split-Path (Split-Path (Split-Path $unityEditorPath)) -Leaf
Write-Host "Unity Editor found: $unityVersion at $unityEditorPath" -ForegroundColor Green

# Phase 2: VPM CLI Setup
Write-Host "`n=== Phase 2: VPM CLI Setup ===" -ForegroundColor Cyan

Write-Host "Checking VPM CLI installation..." -ForegroundColor Green
try {
    $vpmVersion = vpm --version
    Write-Host "VPM CLI found: $vpmVersion" -ForegroundColor Green
} catch {
    Write-Host "Installing VPM CLI..." -ForegroundColor Yellow
    dotnet tool install --global vrchat.vpm.cli
}

Write-Host "Installing VPM templates..." -ForegroundColor Green
vpm install templates

Write-Host "Listing available repositories..." -ForegroundColor Green
vpm list repos

# Phase 3: VCC Settings Configuration
Write-Host "`n=== Phase 3: VCC Settings Configuration ===" -ForegroundColor Cyan

Write-Host "Creating VCC settings directory..." -ForegroundColor Green
$settingsDir = Split-Path $VCCSettingsPath
if (-not (Test-Path $settingsDir)) {
    New-Item -ItemType Directory -Path $settingsDir -Force | Out-Null
}

if (Test-Path $VCCSettingsPath) {
    $backupPath = "$VCCSettingsPath.backup.$(Get-Date -Format 'yyyyMMdd_HHmmss')"
    Write-Host "Backing up existing settings to: $backupPath" -ForegroundColor Yellow
    Copy-Item $VCCSettingsPath $backupPath
}

Write-Host "Generating VCC settings.json..." -ForegroundColor Green
$settings = @{
    pathToUnityHub = $unityHubPath
    pathToUnityExe = $unityEditorPath
    userProjects = @()
    unityEditors = @(
        @{
            version = $unityVersion
            path = $unityEditorPath
        }
    )
    preferredUnityEditors = @{
        ($unityVersion -replace '\.\d+f\d+$', '') = $unityVersion
    }
    defaultProjectPath = "$env:USERPROFILE\projects"
    lastUIState = 0
    skipUnityAutoFind = $false
    userPackageFolders = @()
    windowSizeData = @{
        width = 0
        height = 0
        x = 0
        y = 0
    }
    skipRequirements = $false
    lastNewsUpdate = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ss.fffZ')
    allowPii = $false
    projectBackupPath = "$env:LOCALAPPDATA\VRChatCreatorCompanion\ProjectBackups"
    showPrereleasePackages = $false
    trackCommunityRepos = $true
    selectedProviders = 3
    userRepos = @()
}

$settings | ConvertTo-Json -Depth 10 | Set-Content $VCCSettingsPath -Encoding UTF8

Write-Host "Settings JSON created successfully" -ForegroundColor Green

# Phase 4: Project Creation
Write-Host "`n=== Phase 4: VirtualTokyoMatching Project Creation ===" -ForegroundColor Cyan

Write-Host "Creating projects directory..." -ForegroundColor Green
$projectsDir = Split-Path $ProjectPath
if (-not (Test-Path $projectsDir)) {
    New-Item -ItemType Directory -Path $projectsDir -Force | Out-Null
}

if (Test-Path $ProjectPath) {
    Write-Warning "Project directory already exists: $ProjectPath"
    $response = Read-Host "Continue and potentially overwrite? (y/N)"
    if ($response -ne 'y' -and $response -ne 'Y') {
        Write-Host "Setup cancelled by user" -ForegroundColor Yellow
        exit 0
    }
}

Write-Host "Creating VRChat World project..." -ForegroundColor Green
Set-Location $projectsDir
vpm new $ProjectName World -p $projectsDir

if (-not (Test-Path $ProjectPath)) {
    Write-Error "Project creation failed"
    exit 1
}

Set-Location $ProjectPath

Write-Host "Verifying project structure..." -ForegroundColor Green
vpm check project .

# Phase 5: Package Installation
Write-Host "`n=== Phase 5: VRChat SDK Package Installation ===" -ForegroundColor Cyan

Write-Host "Installing VRChat Worlds SDK..." -ForegroundColor Green
vpm add package com.vrchat.worlds -p .

Write-Host "Resolving dependencies..." -ForegroundColor Green
vpm resolve project .

Write-Host "Installing UdonSharp..." -ForegroundColor Green
try {
    vpm add package com.vrchat.udonsharp -p .
} catch {
    Write-Warning "UdonSharp installation failed, trying specific version..."
    vpm add package com.vrchat.udonsharp@1.1.8 -p .
}

vpm resolve project .

Write-Host "Installing ClientSim for testing..." -ForegroundColor Green
try {
    vpm add package com.vrchat.clientsim -p .
} catch {
    Write-Warning "ClientSim installation failed, trying specific version..."
    vpm add package com.vrchat.clientsim@1.2.6 -p .
}

vpm resolve project .

Write-Host "Final dependency resolution..." -ForegroundColor Green
vpm resolve project .

Write-Host "Listing installed packages..." -ForegroundColor Green
vpm list packages -p .

# Phase 6: Project Structure Setup
Write-Host "`n=== Phase 6: Project Structure Setup ===" -ForegroundColor Cyan

Write-Host "Creating VirtualTokyoMatching folder structure..." -ForegroundColor Green

# Create main asset folders
$folders = @(
    "Assets\VirtualTokyoMatching\Scripts",
    "Assets\VirtualTokyoMatching\Scripts\Core",
    "Assets\VirtualTokyoMatching\Scripts\Assessment", 
    "Assets\VirtualTokyoMatching\Scripts\Vector",
    "Assets\VirtualTokyoMatching\Scripts\Matching",
    "Assets\VirtualTokyoMatching\Scripts\UI",
    "Assets\VirtualTokyoMatching\Scripts\Safety",
    "Assets\VirtualTokyoMatching\Scripts\Session",
    "Assets\VirtualTokyoMatching\Scripts\Sync",
    "Assets\VirtualTokyoMatching\Scripts\Analysis",
    "Assets\VirtualTokyoMatching\Scripts\Performance",
    "Assets\VirtualTokyoMatching\ScriptableObjects",
    "Assets\VirtualTokyoMatching\Prefabs",
    "Assets\VirtualTokyoMatching\Prefabs\UI",
    "Assets\VirtualTokyoMatching\Prefabs\SessionRooms",
    "Assets\VirtualTokyoMatching\Prefabs\Systems",
    "Assets\VirtualTokyoMatching\Materials",
    "Assets\VirtualTokyoMatching\Scenes",
    "Assets\VirtualTokyoMatching\Resources",
    "Assets\VirtualTokyoMatching\Audio",
    "Assets\VirtualTokyoMatching\Textures"
)

foreach ($folder in $folders) {
    if (-not (Test-Path $folder)) {
        New-Item -ItemType Directory -Path $folder -Force | Out-Null
    }
}

# Phase 7: Completion
Write-Host "`n=== Phase 7: Unity Project Launch ===" -ForegroundColor Cyan

Write-Host "Project setup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Project Location: $ProjectPath" -ForegroundColor Yellow
Write-Host "Unity Version: $unityVersion" -ForegroundColor Yellow
Write-Host "VPM CLI Version: $(vpm --version)" -ForegroundColor Yellow
Write-Host ""

Write-Host "To open the project in Unity:" -ForegroundColor Green
Write-Host "Option 1: Unity Hub GUI - Add project from $ProjectPath" -ForegroundColor White
Write-Host "Option 2: Command line - `"$unityHubPath`" -- --projectPath `"$ProjectPath`"" -ForegroundColor White
Write-Host ""

Write-Host "Next Steps:" -ForegroundColor Green
Write-Host "1. Open project in Unity Editor" -ForegroundColor White
Write-Host "2. Import VirtualTokyoMatching scripts from the repository" -ForegroundColor White
Write-Host "3. Configure ScriptableObjects with your personality data" -ForegroundColor White
Write-Host "4. Set up the main scene using SCENE_SETUP.md guide" -ForegroundColor White
Write-Host "5. Test with ClientSim multi-client setup" -ForegroundColor White
Write-Host ""

# Optional: Launch Unity Hub
if (-not $SkipLaunch) {
    $response = Read-Host "Launch Unity Hub now? (y/N)"
    if ($response -eq 'y' -or $response -eq 'Y') {
        Write-Host "Launching Unity Hub..." -ForegroundColor Green
        Start-Process $unityHubPath -ArgumentList "-- --projectPath `"$ProjectPath`""
        Write-Host "Unity Hub launched with project loaded" -ForegroundColor Green
    }
}

Write-Host "Setup completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "=== Virtual Tokyo Matching Setup Complete ===" -ForegroundColor Green