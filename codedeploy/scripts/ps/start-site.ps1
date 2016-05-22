import-module WebAdministration

foreach($webapp in get-childitem IIS:\Sites\)
{
    $siteName = $webapp.name
    $sitePath = "IIS:\Sites\" + $webapp.name
    $poolName = (Get-ItemProperty -Path $sitePath).ApplicationPool

    Start-WebAppPool -Name $poolName
    Start-WebSite -Name $siteName
}
