NET localgroup "Warewolf Administrators" /ADD
NET user "%SERVER_USERNAME%" "%SERVER_PASSWORD%" /ADD /Y
NET localgroup "Administrators" "%SERVER_USERNAME%" /ADD
NET localgroup "Warewolf Administrators" "%SERVER_USERNAME%" /ADD
"%SERVER_PATH%" --interactive