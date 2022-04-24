param ([string]$SolutionsToCover="", [string]$RootDirectory="", [string]$IgnoreFilter="") # "All" to cover all solutions for first param, other names will change the coverage directory to whatever is specified...

Set-ExecutionPolicy -ExecutionPolicy AllSigned -Scope Process #Set policy only for this run of the script. Still dangerous.

try
{
    ##################################################################################################### Parsing directories to create abs paths for dotCover

    # Required to be in the root folder of the repo this won't work...
    $CurrentLocation = $MyInvocation.MyCommand.Path
    $HostDirectory = Split-Path $CurrentLocation

    if ($SolutionsToCover -eq "")
	{
		$SolutionsToCover = "All"
	}

    if ($RootDirectory -eq $null)
    {
        cd $RootDirectory
        $HostDirectory = $RootDirectory
        Write-Host "Root directory is " $RootDirectory
    }
    else #by default, the script is one folder down from the repo's root
    {
        $HostDirectory = $HostDirectory + "\..\"
    }

    $ParsedFilterArgs = ""

    if($IgnoreFilter -ne "")
    {
        $IgnoreFilter -split ';' | ForEach-Object {
            $ParsedFilterArgs = $ParsedFilterArgs + "-:module=" + $_ + ";"
        }
    }
	else
	{
		$ParsedFilterArgs = "-:module=*DDG*;module=*.UnitTest*;"
	}

    $CoverLogsLocation = $HostDirectory + '\logs\dotCover\'
    $DotCoverLocation = $HostDirectory + '\pipeline_scripts\dotCover\dotCover.exe'

    # Configurations for merging and report generation
    $DotCoverMergeConfig = '--Source="' + $CoverLogsLocation + 'coverLog*.dcvr" --Output="'+ $CoverLogsLocation +'coverLogComplete.dcvr"'
    $DotCoverReportHTMLConfig = '--Source="' + $CoverLogsLocation + 'coverLogComplete.dcvr" --Output="'+ $CoverLogsLocation +'coverLogComplete.html" --ReportType="HTML"'
    $DotCoverReportJSONConfig = '--Source="' + $CoverLogsLocation + 'coverLogComplete.dcvr" --Output="'+ $CoverLogsLocation +'coverLogComplete.json" --ReportType="JSON"'

    $DotCoverMergeExpression = "& '${DotCoverLocation}' merge ${DotCoverMergeConfig}"
    $DotCoverHTMLReportExpression = "& '${DotCoverLocation}' report ${DotCoverReportHTMLConfig}"
    $DotCoverJSONReportExpression = "& '${DotCoverLocation}' report ${DotCoverReportJSONConfig}"

    # Ensure the coverLogs location doesn't exist or the script has a 50/50 chance of working
    if (!(Test-Path -Path $CoverLogsLocation))
    {
        New-Item -ItemType Directory -Path $CoverLogsLocation
    }
    else
    {
        Remove-Item -Recurse -Path $CoverLogsLocation
        New-Item -ItemType Directory -Path $CoverLogsLocation
    }

    # Calling VS Dev shell to call dotcover nuget for solution parsing
    if (Test-Path -Path 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\Tools\Launch-VsDevShell.ps1')
    {
        & 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\Tools\Launch-VsDevShell.ps1'
    }
    elseif (Test-Path -Path 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\Common7\Tools\Launch-VsDevShell.ps1')
    {
        & 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\Common7\Tools\Launch-VsDevShell.ps1'
    }
    else
    {
        Write-Host ''
        Write-Host '***************************************************************************'
        Write-Host 'Unable to evoke a instance of Visual Studio 2019 Dev Shell for Professional/Enterprise versions. Check if either version is installed in their default locations'
        Write-Host '***************************************************************************'
        Write-Host ''
        exit
    }

    # Determine which directories to generate a coverage report for...
    cd $HostDirectory
   
    if($SolutionsToCover-eq 'All')
    {
        cd .\src
    }
    else
    {
        cd .\src\
        cd .\$SolutionsToCover
    }

    # Begin generating coverage report for the repo...
    # Recursively check subdirectories for solutions; have dotnet run the tests and report on coverage, giving each snapshot a unique name
    $i = 1
    Get-ChildItem -Recurse -Filter *.sln |
    ForEach-Object {
        $OutputLogName = "coverLog" + $i + ".dcvr"
        $OutputLogLocation = "$CoverLogsLocation\$OutputLogName"
        $ExecutionLog = $CoverLogsLocation + "execution" + $i + ".txt"
        New-Item  $ExecutionLog
        Write-Host "Checking unit tests and coverage for solution: " $_.Fullname.ToString()
        dotnet dotcover test --dcLogFile=$ExecutionLog --dcOutput=$OutputLogLocation --dcAttributeFilters="System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute;" --dcFilters=$ParsedFilterArgs --no-build $_.FullName
        $i = $i + 1
        }

    Invoke-Expression $DotCoverMergeExpression
    Invoke-Expression $DotCoverHTMLReportExpression
    Invoke-Expression $DotCoverJSONReportExpression

    # Delete all unnecessary files after execution
    $logsToRemove = Get-ChildItem -Path $CoverLogsLocation -Filter '*.dcvr,*.html' -Exclude '*complete.dcvr,*complete.html'

    ForEach($log in $logsToRemove)
    {
        Remove-Item $log
    }

    cd $HostDirectory
}
catch
{
    Write-Output $_.Exception.Message
    cd $HostDirectory
}