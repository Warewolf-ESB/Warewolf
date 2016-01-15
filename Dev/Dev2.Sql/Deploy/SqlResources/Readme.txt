Install
-------
1. Copy the contents of this folder to a folder on the SQL server that the SQL service account has permissions on.
2. Edit the following files and replace the database name AdventureWorks2008R2 with the relevant one:
		- \Warewolf\Install.sql
		- \Warewolf\Uninstall.sql
2. Login to the SQL Server and run install.cmd from the folder in step 1.
3. Open Samples.sql in SQL Management Studio to see examples of running workflows from SQL Server.

Uninstall
---------
1. Login to the SQL Server and run uninstall.cmd from the installation location  



NOTE: Requests from SQL Server are made using this account: DomainName\DatabaseServerName$
------------------------------------------------------------------------------------------------
To grant permissions for the SQL Server do the following:
- In Warewolf, click Settings, then add the Windows Group to which the database server belongs 
  (typically Domain Computers) and select the Execute checkbox, and then click Save.

