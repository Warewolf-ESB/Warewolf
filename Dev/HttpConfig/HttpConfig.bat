@Echo off
cls

@Echo Removing ... http://*:3142/
netsh http delete urlacl url=http://+:3142/

@Echo Removing ... https://+:3143/
netsh http delete urlacl url=https://+:3143/

@Echo Removing ... http://*:3142/
netsh http delete urlacl url=http://*:3142/

@Echo Removing ... https://*:3143/
netsh http delete urlacl url=https://*:3143/

@echo Adding ..... http://*:3142/
netsh http add urlacl url=http://*:3142/ user=\Everyone

@echo Adding ..... https://*:3143/
netsh http add urlacl url=https://*:3143/ user=\Everyone

@echo Adding ..... firewall rule port 3142
netsh advfirewall firewall add rule name="Wareworlf Server Port 3142" dir=in action=allow protocol=TCP localport=3142

@echo Adding ..... firewall rule port 3143
netsh advfirewall firewall add rule name="Wareworlf Server Port 3143" dir=in action=allow protocol=TCP localport=3143

Pause
