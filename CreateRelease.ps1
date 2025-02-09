# Class which holds the project entry
class ProjectEntry {
    [string]$SolutionFile
    [string]$ProjectFile
    [string]$PublishProfile
    [string]$BinDir
    [bool]$CreateRelease

    # Default constructor
    ProjectEntry() { }

    # Constructor
    ProjectEntry([string]$rootDir, [string]$solutionFile, [string]$projectFile, [bool]$createRelease) {
        $this.SolutionFile = $solutionFile
        $this.CreateRelease = $createRelease
        # Set the other properties
        $this.SetValues($rootDir, $projectFile)
    }

    # Sets the properties
    [void]SetValues([string]$rootDir, [string]$path) {
        $this.ProjectFile = Join-Path -Path $rootDir -ChildPath $path
        $this.PublishProfile = Join-Path -Path $rootDir -ChildPath "Properties\PublishProfiles\FolderProfile.pubxml"
        $this.BinDir = Join-Path -Path $rootDir -ChildPath "bin"
    }
}

# Checks if the specified file path is valid
function IsFileValid($filePath) {
    # Check if the value is empty or not
    if ([string]::IsNullOrEmpty($filePath)) {
        return $false;
    }

    # Get the complete path and check if the file exists
    $tmpPath = $filePath
    try {
        $tmpPath = Resolve-Path -Path $filePath
    }
    catch {
        return $false
    }

    if ([System.IO.File]::Exists($tmpPath)) {
        return $true
    }

    return $false
}

# Gets the current version number
function GetVersionNumber($filePath) {
    $fileValid = IsFileValid $filePath

    if (-not $fileValid) {
        Write-Host "Can't determine current version number."
        return "1.0.0.0"
    }

    $doc = [XML](Get-Content -Path $filePath)

    # Get the specified element
    $element = $doc.SelectSingleNode("//AssemblyVersion")

    if ($element -eq $null) {
        Write-Host "Can't determine current version number. Fallback to 1.0.0.0"
        return "1.0.0.0"
    }

    # Return the version number
    return $element.InnerText
}

# Generates a new version number
function GenerateVersionNumber($oldVersion) {
    $tmpVersion = [Version]::new()

    if (-not [string]::IsNullOrEmpty($oldVersion)) {
        try {
            $tmpVersion = [Version]::new($oldVersion)
        }
        catch {
            Write-Host "Can't determine old version. Fallback to new version"
        }
    }

    $year = Get-Date -Format "yy"
    $dayOfYear = (Get-Date).DayOfYear
    $build = 0
    $minutesSinceMidnight = [System.Math]::Round((Get-Date).ToUniversalTime().TimeOfDay.TotalMinutes)

    Write-Host "Compare old and new version"
    if ($year -eq $tmpVersion.Major -and $dayOfYear -eq $tmpVersion.Minor) {
        # Add on to the build number if the major and minor versions are equal
        $build = $tmpVersion.Build + 1
    }

    $newVersion = [Version]::new($year, $dayOfYear, $build, $minutesSinceMidnight)

    return $newVersion
}

# Changes the value of a specified XML node
function ChangeXmlNode($filePath, $nodeName, $newValue) {
    $fileValid = IsFileValid $filePath

    if (-not $fileValid) {
        Write-Host "ERROR > Can't load file '$filePath'"
        return
    }

    $doc = [XML](Get-Content -Path $filePath)

    # Get the specified element
    $element = $doc.SelectSingleNode($nodeName)

    Write-Host "Node: $nodeName - Old value: $($element.InnerText); New value: $newValue"

    # Change the element
    $element.InnerText = $newValue

    # Save the changes
    $doc.Save($filePath)
}

# Changes the version of the project file
function ChangeProjectFile($filePath, $newVersion) {
    # Assembly version
    ChangeXmlNode $filePath "//AssemblyVersion" $newVersion

    # File Version
    ChangeXmlNode $filePath "//FileVersion" $newVersion

    # Version
    ChangeXmlNode $filePath "//Version" $newVersion
}

# Creates a zip archive
function CreateZipArchive($sourceDir, $targetPath) {
    Compress-Archive -Path $sourceDir -DestinationPath $targetPath -CompressionLevel Optimal
}

function CreateRelease([ProjectEntry]$project) {
    $projectFile = $project.ProjectFile
    # Check if the file is valid
    $validProjectFile = IsFileValid $project.ProjectFile

    if (-not $validProjectFile) {
        Write-Host "The project file is not valid."
        return 1
    }

    # ===========================
    # Change version number
    # Set the complete path
    $projectFile = Resolve-Path -Path $projectFile

    # Get the old and new version number
    $oldVersion = GetVersionNumber $projectFile
    $newVersion = GenerateVersionNumber $oldVersion

    Write-Host "Change version from $oldVersion to $newVersion"

    # Update the files
    Write-Host "Update project file"
    ChangeProjectFile $projectFile $newVersion

    if ($project.CreateRelease -eq $false) {
        Write-Host "Skip release creation."
        return 0;
    }
    # ===========================
    # Create the release
    $dotnet = "C:\Program Files\dotnet\dotnet.exe"
    $currentLocation = Get-Location
    $output = Join-Path -Path $currentLocation -ChildPath $project.BinDir

    # Clear the previous builds
    Write-Host "Clear output directory $output"
    if ([System.IO.Directory]::Exists($output)) {
        Remove-Item "$output\*" -Recurse -Confirm:$false
    }    

    # Create the release
    Write-Host "Create release"
    $publishProfile = $project.PublishProfile
    & $dotnet publish $project.SolutionFile -p:PublishProfile=$publishProfile

    return 0
}

# ===========================
# Main entry point
# ===========================
$mainSolution = "MsSql.ClassGenerator.sln"
$core = [ProjectEntry]::new("MsSql.ClassGenerator.Core", $mainSolution, "MsSql.ClassGenerator.Core.csproj", $false)
$desktopApp = [ProjectEntry]::new("MsSql.ClassGenerator", $mainSolution, "MsSql.ClassGenerator.csproj", $true)
$cliApp = [ProjectEntry]::new("MsSql.ClassGenerator.Cli", $mainSolution, "MsSql.ClassGenerator.Cli.csproj", $true)
$projects = @($core, $desktopApp, $cliApp);

Write-Host "Start version update. Files:"
Write-Host " - Project files:"
foreach ($project in $projects) {
    Write-Host "   > $($project.ProjectFile)"
}

Write-Host "Create releases..."

try {

    foreach ($project in $projects) {

        $result = CreateRelease $project
    
        if ($result -eq 1) {
            Write-Host "An error has occurred while creating the release for $($project.ProjectFile)"
            return 1
        }
    }

    return 0
}
catch {
    Write-Host "An error has occurred during the process. Error: $_"
    return 1
}