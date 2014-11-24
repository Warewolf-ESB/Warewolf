makecert -pe -n "CN=Test Root Authority{0}" -ss my -sr LocalMachine -a sha1 -sky signature -r "Test and Dev Root Authority.cer"
makecert -pe -n "CN=warewolf.local" -ss my -sr LocalMachine -a sha1 -sky exchange -eku 1.3.6.1.5.5.7.3.1 -in "Test Root Authority{0}" -is MY -ir LocalMachine -sp "Microsoft RSA SChannel Cryptographic Provider" -sy 12 "WarewolfServer.cer"
move /Y WarewolfServer.cer ..
del "Test and Dev Root Authority.cer"