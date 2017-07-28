CREATE TABLE [dbo].[SqlBulkInsertSpecFlowTestTable_for_Import_data_into_Table_Batch_size_is_1_[[tableNameUniqueNameGuid]]](
	[Col1] [int] IDENTITY(1,1) NOT NULL,
	[Col2] [varchar](10) NULL,
	[Col3] [uniqueidentifier] NULL,
 CONSTRAINT [PK_SqlBulkInsertSpecFlowTestTable_for_Import_data_into_Table_Batch_size_is_1_[[tableNameUniqueNameGuid]]] PRIMARY KEY CLUSTERED 
(
	[Col1] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[SqlBulkInsertSpecFlowTestTableForeign_for_Import_data_into_Table_Batch_size_is_1_[[tableNameUniqueNameGuid]]](
	[Col1] [uniqueidentifier] NOT NULL,
	[Col2] [varchar](50) NULL,
 CONSTRAINT [PK_SqlBulkInsertSpecFlowTestTableForeign_for_Import_data_into_Table_Batch_size_is_1_[[tableNameUniqueNameGuid]]] PRIMARY KEY CLUSTERED 
(
	[Col1] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[SqlBulkInsertSpecFlowTestTable_for_Import_data_into_Table_Batch_size_is_1_[[tableNameUniqueNameGuid]]]  WITH NOCHECK ADD  CONSTRAINT [FK_SqlBulkInsertSpecFlowTestTable_SqlBulkInsertSpecFlowTestTableForeign_for_Import_data_into_Table_Batch_size_is_1_[[tableNameUniqueNameGuid]]] FOREIGN KEY([Col3])
REFERENCES [dbo].[SqlBulkInsertSpecFlowTestTableForeign_for_Import_data_into_Table_Batch_size_is_1_[[tableNameUniqueNameGuid]]] ([Col1])
GO

ALTER TABLE [dbo].[SqlBulkInsertSpecFlowTestTable_for_Import_data_into_Table_Batch_size_is_1_[[tableNameUniqueNameGuid]]] CHECK CONSTRAINT [FK_SqlBulkInsertSpecFlowTestTable_SqlBulkInsertSpecFlowTestTableForeign_for_Import_data_into_Table_Batch_size_is_1_[[tableNameUniqueNameGuid]]]
GO

CREATE TRIGGER [dbo].[InsertACountColumnOnSqlBulkInsertSpecFlowTestTableForeign_for_Import_data_into_Table_Batch_size_is_1_[[tableNameUniqueNameGuid]]] ON  [dbo].[SqlBulkInsertSpecFlowTestTable_for_Import_data_into_Table_Batch_size_is_1_[[tableNameUniqueNameGuid]]]
   AFTER INSERT
AS 
BEGIN
   update SqlBulkInsertSpecFlowTestTableForeign_for_Import_data_into_Table_Batch_size_is_1_[[tableNameUniqueNameGuid]]
		set Col2 = (select col2 + 1 from SqlBulkInsertSpecFlowTestTableForeign_for_Import_data_into_Table_Batch_size_is_1_[[tableNameUniqueNameGuid]]
		where Col1 = '23EF3ADB-5A4F-4785-B311-E121FF7ACB67')
		where Col1 = '23EF3ADB-5A4F-4785-B311-E121FF7ACB67'
END
GO

ALTER TABLE [dbo].[SqlBulkInsertSpecFlowTestTable_for_Import_data_into_Table_Batch_size_is_1_[[tableNameUniqueNameGuid]]] ENABLE TRIGGER [InsertACountColumnOnSqlBulkInsertSpecFlowTestTableForeign_for_Import_data_into_Table_Batch_size_is_1_[[tableNameUniqueNameGuid]]]
GO

CREATE TRIGGER [dbo].[InsertCol2OnSqlBulkInsertSpecFlowTestTableWhenIsNull_for_Import_data_into_Table_Batch_size_is_1_[[tableNameUniqueNameGuid]]] ON  [dbo].[SqlBulkInsertSpecFlowTestTable_for_Import_data_into_Table_Batch_size_is_1_[[tableNameUniqueNameGuid]]]
   AFTER UPDATE, INSERT
AS 
BEGIN
    
    update SqlBulkInsertSpecFlowTestTable_for_Import_data_into_Table_Batch_size_is_1_[[tableNameUniqueNameGuid]] set col2 = 'XXXXXXXX'
    where col1 in (    
    select col1
    AS IDS FROM SqlBulkInsertSpecFlowTestTable_for_Import_data_into_Table_Batch_size_is_1_[[tableNameUniqueNameGuid]]
    WHERE Col2 = ''
    )
END
GO

ALTER TABLE [dbo].[SqlBulkInsertSpecFlowTestTable_for_Import_data_into_Table_Batch_size_is_1_[[tableNameUniqueNameGuid]]] ENABLE TRIGGER [InsertCol2OnSqlBulkInsertSpecFlowTestTableWhenIsNull_for_Import_data_into_Table_Batch_size_is_1_[[tableNameUniqueNameGuid]]]
GO

CREATE TRIGGER [dbo].[WaitForAWhileOnSqlBulkInsertSpecFlowTestTable_for_Import_data_into_Table_Batch_size_is_1_[[tableNameUniqueNameGuid]]] ON  [dbo].[SqlBulkInsertSpecFlowTestTable_for_Import_data_into_Table_Batch_size_is_1_[[tableNameUniqueNameGuid]]]
    AFTER INSERT
AS 
BEGIN
   WAITFOR DELAY '00:00:002';
END
GO

ALTER TABLE [dbo].[SqlBulkInsertSpecFlowTestTable_for_Import_data_into_Table_Batch_size_is_1_[[tableNameUniqueNameGuid]]] ENABLE TRIGGER [WaitForAWhileOnSqlBulkInsertSpecFlowTestTable_for_Import_data_into_Table_Batch_size_is_1_[[tableNameUniqueNameGuid]]]
GO

INSERT INTO [dbo].[SqlBulkInsertSpecFlowTestTableForeign_for_Import_data_into_Table_Batch_size_is_1_[[tableNameUniqueNameGuid]]] ([Col1] ,[Col2]) VALUES ('279C690E-3304-47A0-8BDE-5D3CA2520A34' ,'First Row')
GO

INSERT INTO [dbo].[SqlBulkInsertSpecFlowTestTableForeign_for_Import_data_into_Table_Batch_size_is_1_[[tableNameUniqueNameGuid]]] ([Col1] ,[Col2]) VALUES ('BC7A9611-102E-4899-82B8-97FF1517D268' ,'Second Row')
GO

INSERT INTO [dbo].[SqlBulkInsertSpecFlowTestTableForeign_for_Import_data_into_Table_Batch_size_is_1_[[tableNameUniqueNameGuid]]] ([Col1] ,[Col2]) VALUES ('23EF3ADB-5A4F-4785-B311-E121FF7ACB67' ,'0')
GO