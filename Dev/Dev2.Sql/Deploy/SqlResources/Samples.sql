USE [AdventureWorks2008R2]
GO

------------------------------------------------------------------------------------------------
-- NOTE: Requests from SQL Server are made using this account: DomainName\DatabaseServerName$
------------------------------------------------------------------------------------------------
-- To grant permissions for the SQL Server do the following:
-- In Warewolf, click Settings, then add the Windows Group to which the database server belongs 
-- (typically Domain Computers) and select the Execute checkbox, and then click Save.
------------------------------------------------------------------------------------------------


------------------------------------------------------------------------------------------------
-- RunWorkflowForXml 
------------------------------------------------------------------------------------------------
-- This method runs the given workflow and returns it's results as XML, 
-- optionally replacing the root element name with the one given.
-- Select the Output checkbox on the Variables tab in Warewolf to add
-- that scalar variable or recordset to your ouput data.
------------------------------------------------------------------------------------------------
DECLARE 
	@ServerUri varchar(max)	-- Required, the absolute URI of the workflow service	
	,@RootName varchar(max)	-- Optional, the name used to override the root element
	,@WorkflowResult XML

-- Set parameter values
SELECT 
	@ServerUri = 'http://MyServerNameorIPAddress:3142/services/SampleEmployeesWorkflow?ResultType=Managers'
				--http://"SERVER NAME":"PORT"/services/"WORKFLOW NAME"?"WORKFLOW DECLARED INPUT VARIABLE"="INPUT VALUE"
	,@RootName = 'Sample'  -- The root element will now be called this

-- Run workflow and retrieve results
SELECT @WorkflowResult = [Warewolf].[RunWorkflowForXml]( @ServerUri, @RootName )


-- Get the raw XML
SELECT @WorkflowResult WorkflowResult

-- Extract a node value
SELECT @WorkflowResult.value('(/Sample/SplitChar)[1]', 'varchar(50)') SplitCharValue
	,@WorkflowResult.value('(/Sample/StringToSplit)[1]', 'varchar(1000)') StringToSplit

-- Extract employees XML using XPATH
SELECT @WorkflowResult.query('/Sample/Employees') AS EmployeesXml
GO
------------------------------------------------------------------------------------------------
-- RunWorkflowForXml END
------------------------------------------------------------------------------------------------


------------------------------------------------------------------------------------------------
-- RunWorkflowForSql - with RecordsetName (recommended usage)
------------------------------------------------------------------------------------------------
-- This method runs the given workflow and returns the results of the given recordset as a table
------------------------------------------------------------------------------------------------
DECLARE 
	@ServerUri varchar(max)			-- Required, the absolute URI of the workflow service	
	,@RecordsetName varchar(max)	-- Optional, the name of the recordset to be returned
									-- If specified then the children of elements with 
									-- matching names are returned as rows. Otherwise, 
									-- the XML is returned as a 'flattened' table.

-- Set parameter values
SELECT 
	@ServerUri = 'http://MyServerNameorIPAddress:3142/services/SampleEmployeesWorkflow?ResultType=Managers'
				--http://"SERVER NAME":"PORT"/services/"WORKFLOW NAME"?"WORKFLOW DECLARED INPUT VARIABLE"="INPUT VALUE"
	,@RecordsetName = 'Employees'  -- The name of the recordset to be returned

-- Run workflow and retrieve results
EXEC [Warewolf].[RunWorkflowForSql] @ServerUri, @RecordsetName
GO
------------------------------------------------------------------------------------------------
-- RunWorkflowForSql - with RecordsetName END
------------------------------------------------------------------------------------------------


------------------------------------------------------------------------------------------------
-- RunWorkflowForSql - without RecordsetName
------------------------------------------------------------------------------------------------
-- This method runs the given workflow and returns the results as a 'flattened' table
------------------------------------------------------------------------------------------------
DECLARE 
	@ServerUri varchar(max)			-- Required, the absolute URI of the workflow service	
	,@RecordsetName varchar(max)	-- Optional, the name of the recordset to be returned
									-- If specified then the children of elements with 
									-- matching names are returned as rows. Otherwise, 
									-- the XML is returned as a 'flattened' table.

-- Set parameter values
SELECT 
	@ServerUri = 'http://MyServerNameorIPAddress:3142/services/SampleEmployeesWorkflow?ResultType=Managers'
				--http://"SERVER NAME":"PORT"/services/"WORKFLOW NAME"?"WORKFLOW DECLARED INPUT VARIABLE"="INPUT VALUE"
	,@RecordsetName = NULL  -- NULL so that the XML is returned as a 'flattened' table.

-- Run workflow and retrieve results
EXEC [Warewolf].[RunWorkflowForSql] @ServerUri, @RecordsetName
GO
------------------------------------------------------------------------------------------------
-- RunWorkflowForSql - without RecordsetName END
------------------------------------------------------------------------------------------------


------------------------------------------------------------------------------------------------
-- RunWorkflowForSql - incorporate into a user function
------------------------------------------------------------------------------------------------
-- NOTE: Requires the following:
-- 1. Requires HARD-CODING of database name into string
-- 2. EXEC sp_configure 'Ad Hoc Distributed Queries', 1;RECONFIGURE;
------------------------------------------------------------------------------------------------
GO
CREATE FUNCTION [Warewolf].ufnGetEmployees()
RETURNS TABLE
AS
RETURN 
SELECT  * 
FROM OPENROWSET ('SQLNCLI','Server=(local);TRUSTED_CONNECTION=YES;',
	'set fmtonly off exec [AdventureWorks2008R2].[Warewolf].[RunWorkflowForSql] ''http://MyServerNameorIPAddress:3142/services/SampleEmployeesWorkflow?ResultType=Managers'', ''Employees'' ')
	-- http://"SERVER NAME":"PORT"/services/"WORKFLOW NAME"?"WORKFLOW DECLARED INPUT VARIABLE"="INPUT VALUE"
	-- 'Employees' = The name of the recordset to be returned
GO

SELECT * FROM [Warewolf].ufnGetEmployees() AS E 
INNER JOIN HumanResources.Employee AS HRE ON CAST(E.Number AS INT) = HRE.BusinessEntityID
GO
------------------------------------------------------------------------------------------------
-- RunWorkflowForSql - incorporate into a user function END
------------------------------------------------------------------------------------------------
