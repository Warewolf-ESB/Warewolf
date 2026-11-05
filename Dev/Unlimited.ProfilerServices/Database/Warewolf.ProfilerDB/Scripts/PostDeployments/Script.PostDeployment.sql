/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/


print('Post deployment script')
--:r  .\Masters\dbo\SeedProfiles.sql
--:r  .\Masters\dbo\SeedProfileDates.sql
--:r  .\Masters\dbo\SeedDebitRunIds.sql
--:r  .\Masters\dbo\SeedHoliday.sql
--:r  .\Masters\dbo\SeedProfileTypes.sql
:r  .\Masters\Profiler\SeedDebitRunIds.sql
:r  .\Masters\Profiler\SeedProfiles.sql
:r  .\Masters\Profiler\SeedProfileDates.sql
:r  .\Masters\Profiler\SeedSpecialHolidays.sql
:r  .\Masters\Profiler\SeedRecommendationType.sql
:r  .\Masters\Profiler\SeedGrantTypes.sql
:r  .\Masters\Profiler\SeedGrantDates.sql
