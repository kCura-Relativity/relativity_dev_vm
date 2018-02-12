[System.Int32]$global:maxRetry = 1
[System.Int32]$global:count = 1

function New-DevVm {
  try {
    Write-Host  "-----> [$(Get-Date -Format g)] Attempt #$($global:count)" -ForegroundColor Blue
    
    Write-Host  "-----> [$(Get-Date -Format g)] Creating VM" -ForegroundColor Blue
    vagrant up
    Write-Host  "-----> [$(Get-Date -Format g)] Created VM" -ForegroundColor Blue

    Write-Host  "-----> [$(Get-Date -Format g)] Stopping VM" -ForegroundColor Blue
    Stop-VM -Name "RelativityDevVm"
    Write-Host  "-----> [$(Get-Date -Format g)] Stopped VM" -ForegroundColor Blue

    Write-Host  "-----> [$(Get-Date -Format g)] Creating VM Checkpoint" -ForegroundColor Blue
    Checkpoint-VM -Name $vmName -SnapshotName $vmCheckpointName
    Write-Host  "-----> [$(Get-Date -Format g)] Created VM Checkpoint" -ForegroundColor Blue

    Write-Host  "-----> [$(Get-Date -Format g)] Export VM" -ForegroundColor Blue
    Export-VM -Name $vmName -Path $vmExportPath
    Write-Host  "-----> [$(Get-Date -Format g)] Exported VM" -ForegroundColor Blue

    Write-Host  "-----> [$(Get-Date -Format g)] Compressing Exported VM to Zip" -ForegroundColor Blue
    Install-Module -NugetPackageId 7Zip4Powershell -PackageVersion 1.8.0
    Compress-7Zip -Path $compressPath -ArchiveFileName $zipFileName
    Write-Host  "-----> [$(Get-Date -Format g)] Compressed Exported VM to Zip" -ForegroundColor Blue
  }
  finally {
    Write-Host  "-----> [$(Get-Date -Format g)] Deleting VM" -ForegroundColor Blue
    vagrant destroy -f $vmName
    Write-Host  "-----> [$(Get-Date -Format g)] Deleted VM" -ForegroundColor Blue
  }
}

function Start-DevVm-Process() {
  $stopWatch = [System.Diagnostics.Stopwatch]::StartNew() 

  while ($global:count -le $global:maxRetry) {
    try {
      $vmName = "RelativityDevVm"
      $vmCheckpointName = "RelativityDevVm Created"
      $vmExportPath = "C:\DevVmExport"
      $compressPath = "$($vmExportPath)\$($vmName)"
      $zipFileName = "$($vmExportPath)\$($vmName).7z"
      
      # Create New DevVm
      New-DevVm
    }
    catch {
      $global:count++ 
    }
    finally {
      Write-Host  "-----> [$(Get-Date -Format g)] Total time: $($stopWatch.Elapsed.TotalMinutes) minutes" -ForegroundColor Blue
      $stopWatch.Stop() 
    }
  }
}

Start-DevVm-Process