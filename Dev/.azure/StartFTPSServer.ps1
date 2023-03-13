if (!(Test-Path "C:\ftps_home\dev2\FORFILERENAMETESTING")) {
	mkdir "C:\ftps_home\dev2\FORFILERENAMETESTING"
}
if (!(Test-Path "C:\ftps_home\dev2\FORUNZIPTESTING")) {
	mkdir "C:\ftps_home\dev2\FORUNZIPTESTING"
}
if (!(Test-Path "C:\cert.crt")) {
@"
-----BEGIN CERTIFICATE-----
MIID+TCCAuGgAwIBAgIUMjnF+Uh4NhKoRO425/Sgjbs7xs0wDQYJKoZIhvcNAQEL
BQAwgYsxCzAJBgNVBAYTAlpBMQwwCgYDVQQIDANLWk4xEjAQBgNVBAcMCUhpbGxj
cmVzdDERMA8GA1UECgwIV2FyZXdvbGYxDzANBgNVBAsMBkRldk9wczEUMBIGA1UE
AwwLb3Bzd29sZi5jb20xIDAeBgkqhkiG9w0BCQEWEWFkbWluQG9wc3dvbGYuY29t
MB4XDTIxMDQxODA2MzYzMVoXDTIyMDQxODA2MzYzMVowgYsxCzAJBgNVBAYTAlpB
MQwwCgYDVQQIDANLWk4xEjAQBgNVBAcMCUhpbGxjcmVzdDERMA8GA1UECgwIV2Fy
ZXdvbGYxDzANBgNVBAsMBkRldk9wczEUMBIGA1UEAwwLb3Bzd29sZi5jb20xIDAe
BgkqhkiG9w0BCQEWEWFkbWluQG9wc3dvbGYuY29tMIIBIjANBgkqhkiG9w0BAQEF
AAOCAQ8AMIIBCgKCAQEA2eWOl6OjY/V6xPKYKC8NwrtOYfmr04KYR+5xuzZhNPXV
ICDZrHg3UfidSU9yiB8hRrZYlQ1YZw6kdfxYFiBqQV+450CHS2R9RbvPQTGxL0/I
lO4LQVodiTW7Khiemye0OId04Ak6yVz6wF+UScPb2HLRM7dW2OMbDpUcb/6QSCBK
1zdr6Co8O+okDdlXFSmqVuK5gIfT6lOKiny2XLaO6zPni4o6E5HzsX47YJiaTLCZ
J9X5oCWhB0wIVgX7vkdBxiwXACaHWlN32//wya1h1dQQpGUvttzEHl+wc0Fk6R9f
HKmP9owzuw40PPjdoOXhzqr7hCqszp/aTCqVFJU9xQIDAQABo1MwUTAdBgNVHQ4E
FgQUk+fn8dM59dkM0u6ZWnRp70TDupwwHwYDVR0jBBgwFoAUk+fn8dM59dkM0u6Z
WnRp70TDupwwDwYDVR0TAQH/BAUwAwEB/zANBgkqhkiG9w0BAQsFAAOCAQEAR05k
Ab9atURsGOHKZbKPFnwj6oKak3CcDSeB0wGAu75hKeGFBqisDg+s5pTcAlGgq8Md
fv6AzFtmskYeHqzt3TtZ091kLXGPrEf4Gv0zYdJ5kEi5RKIxNz57BnntlG/YA1FC
DAFen4U8zhavo4tQk04LkgnV4sHPutUMKqNNX64GAIfmeltr7yBaWs34nZ3+4OiF
c5/UqCGPmHgd2paDzQ3qc5tpCy86mY0zy7FreP/Z8VrnoOKIoH8ULjQAxiopl6zg
6bCLcDayKmfwBKrCgJobb76B7HJ5SKWpQCmgJeI/pFiQv67SsF63xtsPwtdmaY+T
SfOUJf/1oE9T9vp1yQ==
-----END CERTIFICATE-----
"@ | Out-File -LiteralPath "C:\cert.crt" -Encoding ascii -Force
}
if (!(Test-Path "C:\cert.key")) {
@"
-----BEGIN PRIVATE KEY-----
MIIEvwIBADANBgkqhkiG9w0BAQEFAASCBKkwggSlAgEAAoIBAQDZ5Y6Xo6Nj9XrE
8pgoLw3Cu05h+avTgphH7nG7NmE09dUgINmseDdR+J1JT3KIHyFGtliVDVhnDqR1
/FgWIGpBX7jnQIdLZH1Fu89BMbEvT8iU7gtBWh2JNbsqGJ6bJ7Q4h3TgCTrJXPrA
X5RJw9vYctEzt1bY4xsOlRxv/pBIIErXN2voKjw76iQN2VcVKapW4rmAh9PqU4qK
fLZcto7rM+eLijoTkfOxfjtgmJpMsJkn1fmgJaEHTAhWBfu+R0HGLBcAJodaU3fb
//DJrWHV1BCkZS+23MQeX7BzQWTpH18cqY/2jDO7DjQ8+N2g5eHOqvuEKqzOn9pM
KpUUlT3FAgMBAAECggEAVzFN8w4vRsOnggIVsxbJKeBsCDaxdGzw5O/coO6szVWG
GFos4KAmeu3CeuCI00GpvjMflV2Gv46TbwcwdII6IrjcM+WVfizTGEGEOPFalrUV
bcsnw9n8sbhHkhvR9AJaUriZo0DuPj+vs6VLoIz4f0/KuSgnX5jZbedrPsGeGM3e
HYGY/eCB1D6JzbDrW8jHe63SOPOizVA9m/c2CoH/YbL4rVN6+8aSAJaWnVzSUvPD
mRdY15EtF9VURU3C549Pw4C1RC0op2xvP8vlOFGsWDd2HHzxuo13UXd8NIes5zAE
VKIhLsEFkFIRwp7rTVaf9n6KCvvVuuG6N1Kxyv2roQKBgQD1Md/8BJIieFfd/fbq
zL+uAttBGuM88IUgx8c9usldWGYXvDOkmMQvkH7lnxFpOyZDynZyIw4ILrnh1T2+
f5g/qHxEabU59//aAbYoBAXxUUI7ZdBwzmn0yL6KU9hILDhRsEbVLOROC1tmUbUk
GIqTNMFBUimfy7LJJGTdxciouQKBgQDjf7kMijEbqP8bLUATdtlVoiOfuqOlMHYm
FzKsp75+rxSAjUyGvzBNe+HzSlfwPlD6bSYIm3Do80FVG+AawPN7uBApsTb32Lmj
nB1b1GUuN7VGQYJNxmrYe1m8VdLHN5kNM8hwOCpyYMdkFf6aYXAEtEO+iL9ghn1N
+tmW9e8fbQKBgQDIaFGIrTu8TNyUp4Vv+JYa5l7K4e0l2/kUB/YDsG3xi9U2RS94
sxx3PAVcLR2QAzaNZihVte08JuTrft2OnL+WGGIpkLT9goRubcOzBUbOLPqTje5G
pY/Y8VM7wLgglXQa4JekmaKpX4L/KH2D2UM6en4So9M9tsKUwNhoo8YUkQKBgQDQ
KQfrP28bvhBej5L3vGG0hz1NY/tkpOkWhVdqv7oANLbvwVpqWPobi+T9NeMtAfga
jFCmw4QWwq3e8DiogjDH3W18mJiRQ47o82mxorBKD9MgS8Ss4YbWOlerimPowSib
+evHMr00FvWa0L08CTf0NfVem8Vwzt5MweDiznlUKQKBgQCb/2zy03hepOHmr8oZ
2gUv8764Y865wFryfXoOlb+664sgMkNKJGzX/v97NQIeM4vFUb6FMVQO9z4pcXVq
w+jDPTUUugs8MOyE1bUNJutBgEjkeKN8bQt3mQlIhC6HSuwS+NHcku5sKobvjohj
SxVGgsgXs58fKq0k6khAOa4asQ==
-----END PRIVATE KEY-----
"@ | Out-File -LiteralPath "C:\cert.key" -Encoding ascii -Force
}
pip install pyftpdlib
pip install 'cryptography==38.0.4'
pip install 'pyOpenSSL==22.0.0'
if (!(Test-Path "C:\ftps_entrypoint.py")) {
@"
import os, random, string

from pyftpdlib.authorizers import DummyAuthorizer
from pyftpdlib.handlers import TLS_FTPHandler
from pyftpdlib.servers import FTPServer

PASSIVE_PORTS = '56001-56008'

def main():
	user_dir = "C:/ftps_home/dev2"
	if not os.path.isdir(user_dir):
		os.mkdir(user_dir)
	authorizer = DummyAuthorizer()
	authorizer.add_user('dev2', 'Q/ulw&]', user_dir, perm="elradfmw")

	handler = TLS_FTPHandler
	handler.authorizer = authorizer
	handler.permit_foreign_addresses = True
	handler.certfile = 'C:/cert.crt'
	handler.keyfile = 'C:/cert.key'

	passive_ports = list(map(int, PASSIVE_PORTS.split('-')))
	handler.passive_ports = range(passive_ports[0], passive_ports[1])

	server = FTPServer(('0.0.0.0', 1010), handler)
	server.serve_forever()
	
if __name__ == '__main__':
	main()
"@ | Out-File -LiteralPath "C:\ftps_entrypoint.py" -Encoding utf8 -Force
}
pythonw -u "C:\ftps_entrypoint.py"