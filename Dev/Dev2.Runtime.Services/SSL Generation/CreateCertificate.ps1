New-SelfSignedCertificate -Subject "CN=Test Root Authority" -KeyExportPolicy Exportable -KeyUsage CertSign, CRLSign, DigitalSignature -KeyAlgorithm RSA -KeyLength 2048 -HashAlgorithm SHA256 -CertStoreLocation "Cert:\LocalMachine\My" -NotAfter (Get-Date).AddYears(10)
$issuer = Get-ChildItem Cert:\LocalMachine\My | Where-Object { $_.Subject -eq "CN=Test Root Authority" }
$cert = New-SelfSignedCertificate `
    -Subject "CN=warewolf.local" `
    -CertStoreLocation "Cert:\LocalMachine\My" `
    -KeyExportPolicy Exportable `
    -KeyAlgorithm RSA `
    -KeyLength 2048 `
    -HashAlgorithm SHA256 `
    -KeyUsage DigitalSignature, KeyEncipherment `
    -Type Custom `
    -TextExtension @(
        "2.5.29.37={text}1.3.6.1.5.5.7.3.1"   # EKU: Server Authentication
    ) `
    -Signer $issuer
Export-Certificate -Cert $cert -FilePath "WarewolfServer.cer"