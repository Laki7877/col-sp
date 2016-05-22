import-module WebAdministration

foreach($webapp in get-childitem IIS:\Sites\)
{
    $siteName = $webapp.name
    $sitePath = "IIS:\Sites\" + $webapp.name
    $poolName = (Get-ItemProperty -Path $sitePath).ApplicationPool

    Stop-WebSite -Name $siteName
    Stop-WebAppPool -Name $poolName
}
