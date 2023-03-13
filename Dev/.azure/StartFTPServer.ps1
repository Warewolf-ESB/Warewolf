if (!(Test-Path "C:\ftp_home\dev2\FORUNZIPTESTING")) {
	mkdir "C:\ftp_home\dev2\FORUNZIPTESTING"
}
pip install pyftpdlib
if (!(Test-Path "C:\ftp_entrypoint.py")) {
@"
import os, random, string

from pyftpdlib.authorizers import DummyAuthorizer
from pyftpdlib.handlers import FTPHandler
from pyftpdlib.servers import FTPServer

PASSIVE_PORTS = '17000-17007'

def main():
	authorizer = DummyAuthorizer()
	user_dir = "C:/ftp_home/dev2"
	if not os.path.isdir(user_dir):
		os.mkdir(user_dir)
	authorizer.add_user("dev2", "Q/ulw&]", user_dir, perm="elradfmw")

	handler = FTPHandler
	handler.authorizer = authorizer
	handler.permit_foreign_addresses = True

	passive_ports = list(map(int, PASSIVE_PORTS.split('-')))
	handler.passive_ports = range(passive_ports[0], passive_ports[1])

	server = FTPServer(('0.0.0.0', 21), handler)
	server.serve_forever()
	
if __name__ == '__main__':
	main()
"@ | Out-File -LiteralPath "C:\ftp_entrypoint.py" -Encoding utf8 -Force
}
pythonw -u "C:\ftp_entrypoint.py"