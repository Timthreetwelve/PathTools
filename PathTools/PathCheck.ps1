#!############################################################
#  PowerShell script to verify PATH variable
#  This script is part of Path Monitor it needs
#  be located in the same directory
#!############################################################

# If running as 32 bit on 64 bit machine restart as 64 bit
if ($Env:PROCESSOR_ARCHITECTURE -ne "AMD64")
{
    Write-Host "Restarting as 64 bit`n" -ForegroundColor Cyan

    if ($myInvocation.Line)
    {
        &"$Env:WINDIR\sysnative\windowspowershell\v1.0\powershell.exe" -NoProfile $myInvocation.Line
    }
    else
    {
        &"$Env:WINDIR\sysnative\windowspowershell\v1.0\powershell.exe" -NoProfile -File "$($myInvocation.InvocationName)" $args
    }
exit $lastexitcode
}

# Get the path from the environment and split it into individual directories
[array]$path = [System.Environment]::GetEnvironmentVariable('PATH') -split ';'
[Int16]$maxlen = 0
[Int16]$counter = 0

# Find the length of the longest directory
foreach ($item in $path)
{
    if ($item.Length -gt $maxlen)
    {
        $maxlen = $item.Length
    }
}

# Header
Write-Host " PATH Verification" -ForegroundColor Yellow
Write-Host " =================" -ForegroundColor Yellow
Write-Host ""

# Loop through each directory and verify that it exists
foreach ($item in $path)
{
    if ($item -ne "")
    {
        $counter ++
        if (Test-Path -Path $item -PathType Container)
        {
            $okmsg = "<- Valid"
        }
        else
        {
            $okmsg = "<- Not Found"
        }
        $msg = "{0,3}  {1,$($maxlen * -1)}  {2}" -f $counter, $item, $okmsg
        Write-Output $msg
    }
}
Write-Output ""
Write-Output "Type EXIT and press enter to close this window."
