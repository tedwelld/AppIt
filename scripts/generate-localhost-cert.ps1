param(
    [string]$PfxPath = "AppIt.Api\certs\localhost.pfx",
    [string]$Password = "password"
)

$ErrorActionPreference = 'Stop'

$securePwd = ConvertTo-SecureString $Password -AsPlainText -Force

# Ensure target directory exists
$dir = Split-Path $PfxPath
if (!(Test-Path $dir)) {
    New-Item -ItemType Directory -Path $dir | Out-Null
}

# Create self-signed certificate for localhost
$cert = New-SelfSignedCertificate -DnsName "localhost" -CertStoreLocation "Cert:\CurrentUser\My" -NotAfter (Get-Date).AddYears(10) -KeyExportPolicy Exportable -FriendlyName "AppIt Localhost Dev Cert"

# Export PFX
Export-PfxCertificate -Cert $cert -FilePath $PfxPath -Password $securePwd -Force

# Import the PFX into the CurrentUser\My store (personal)
try {
    Import-PfxCertificate -FilePath $PfxPath -CertStoreLocation "Cert:\CurrentUser\My" -Password $securePwd -Exportable | Out-Null
} catch {
    Write-Warning "Failed to import PFX into CurrentUser\\My: $_"
}

# Also attempt to add the certificate to CurrentUser\Root (trusted) so browsers accept it for local testing
try {
    Import-PfxCertificate -FilePath $PfxPath -CertStoreLocation "Cert:\CurrentUser\Root" -Password $securePwd -Exportable | Out-Null
} catch {
    Write-Warning "Failed to import PFX into CurrentUser\\Root (may require elevated privileges): $_"
}

Write-Output "Created PFX at: $PfxPath. Imported into CurrentUser\\My and attempted CurrentUser\\Root." 
