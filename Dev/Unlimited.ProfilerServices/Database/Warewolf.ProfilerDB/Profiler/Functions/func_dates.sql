CREATE FUNCTION [Profiler].[func_dates]
(
	@FIRST_DATE		datetime,
	@LAST_DATE		datetime
)
/*
Function: dbo.func_dates
This function returns a date table containing all dates
from @FIRST_DATE through @LAST_DATE inclusive.
@FIRST_DATE must be less than or equal to @LAST_DATE.
The valid date range is 1754-01-01 through 9997-12-31.
If any input parameters are invalid, the fuction will produce
an error.
The table returned by func_dates contains a date and
columns with many calculated attributes of that date.
It is designed to make it convenient to get various commonly
needed date attributes without having to program and test
the same logic in many applications.
func_dates is primarily intended to load a permanant
date table, but it can be used directly by an application
when the date range needed falls outside the range loaded
in a permanant table.
If func_dates is used to load a permanant table, the create
table code can be copied from this function.  For a permanent
date table, most columns should be indexed to produce the
best application performance.
Column Descriptions
------------------------------------------------------------------
DATE_ID               
	Unique ID = Days since 1753-01-01
DATE                            
	Date at Midnight(00:00:00.000)
NEXT_DAY_DATE                   
	Next day after DATE at Midnight(00:00:00.000)
	Intended to be used in queries against columns
	containing datetime values (1998-12-13 14:35:16)
	that need to join to a DATE.
	Example:
	from
		MyTable a
		join
		DATE b
		on	a.DateTimeCol >= b. DATE	and
			a.DateTimeCol < b.NEXT_DAY_DATE
YEAR                            
	Year number in format YYYY, Example = 2005
YEAR_QUARTER                    
	Year and Quarter number in format YYYYQ, Example = 20052
YEAR_MONTH                      
	Year and Month number in format YYYYMM, Example = 200511
YEAR_DAY_OF_YEAR                
	Year and Day of Year number in format YYYYDDD, Example = 2005364
QUARTER                         
	Quarter number in format Q, Example = 4
MONTH                           
	Month number in format MM, Example = 11
DAY_OF_YEAR                     
	Day of Year number in format DDD, Example = 362
DAY_OF_MONTH                    
	Day of Month number in format DD, Example = 31
DAY_OF_WEEK                     
	Day of week number, Sun=1, Mon=2, Tue=3, Wed=4, Thu=5, Fri=6, Sat=7
YEAR_NAME                       
	Year name text in format YYYY, Example = 2005
YEAR_QUARTER_NAME               
	Year Quarter name text in format YYYY QQ, Example = 2005 Q3
YEAR_MONTH_NAME                 
	Year Month name text in format YYYY MMM, Example = 2005 Mar
YEAR_MONTH_NAME_LONG            
	Year Month long name text in format YYYY MMMMMMMMM,
	Example = 2005 September
QUARTER_NAME                    
	Quarter name text in format QQ, Example = Q1
MONTH_NAME                      
	Month name text in format MMM, Example = Mar
MONTH_NAME_LONG                 
	Month long name text in format MMMMMMMMM, Example = September
WEEKDAY_NAME                    
	Weekday name text in format DDD, Example = Tue
WEEKDAY_NAME_LONG               
	Weekday long name text in format DDDDDDDDD, Example = Wednesday
START_OF_YEAR_DATE              
	First Day of Year that DATE is in
END_OF_YEAR_DATE                
	Last Day of Year that DATE is in
START_OF_QUARTER_DATE           
	First Day of Quarter that DATE is in
END_OF_QUARTER_DATE             
	Last Day of Quarter that DATE is in
START_OF_MONTH_DATE             
	First Day of Month that DATE is in
END_OF_MONTH_DATE               
	Last Day of Month that DATE is in
*** Start and End of week columns allow selections by week
*** for any week start date needed.
START_OF_WEEK_STARTING_SUN_DATE 
	First Day of Week starting Sunday that DATE is in
END_OF_WEEK_STARTING_SUN_DATE   
	Last Day of Week starting Sunday that DATE is in
START_OF_WEEK_STARTING_MON_DATE 
	First Day of Week starting Monday that DATE is in
END_OF_WEEK_STARTING_MON_DATE   
	Last Day of Week starting Monday that DATE is in
START_OF_WEEK_STARTING_TUE_DATE 
	First Day of Week starting Tuesday that DATE is in
END_OF_WEEK_STARTING_TUE_DATE   
	Last Day of Week starting Tuesday that DATE is in
START_OF_WEEK_STARTING_WED_DATE 
	First Day of Week starting Wednesday that DATE is in
END_OF_WEEK_STARTING_WED_DATE   
	Last Day of Week starting Wednesday that DATE is in
START_OF_WEEK_STARTING_THU_DATE 
	First Day of Week starting Thursday that DATE is in
END_OF_WEEK_STARTING_THU_DATE   
	Last Day of Week starting Thursday that DATE is in
START_OF_WEEK_STARTING_FRI_DATE 
	First Day of Week starting Friday that DATE is in
END_OF_WEEK_STARTING_FRI_DATE   
	Last Day of Week starting Friday that DATE is in
START_OF_WEEK_STARTING_SAT_DATE 
	First Day of Week starting Saturday that DATE is in
END_OF_WEEK_STARTING_SAT_DATE   
	Last Day of Week starting Saturday that DATE is in
*** Sequence No columns are intended to allow easy offsets by
*** Quarter, Month, or Week for applications that need to look at
*** Last or Next Quarter, Month, or Week.  Thay can also be used to
*** generate dynamic cross tab results by Quarter, Month, or Week.
QUARTER_SEQ_NO                  
	Sequential Quarter number as offset from Quarter starting 1753/01/01
MONTH_SEQ_NO                    
	Sequential Month number as offset from Month starting 1753/01/01
WEEK_STARTING_SUN_SEQ_NO        
	Sequential Week number as offset from Week starting Sunday, 1753/01/07
WEEK_STARTING_MON_SEQ_NO        
	Sequential Week number as offset from Week starting Monday, 1753/01/01
WEEK_STARTING_TUE_SEQ_NO        
	Sequential Week number as offset from Week starting Tuesday, 1753/01/02
WEEK_STARTING_WED_SEQ_NO        
	Sequential Week number as offset from Week starting Wednesday, 1753/01/03
WEEK_STARTING_THU_SEQ_NO        
	Sequential Week number as offset from Week starting Thursday, 1753/01/04
WEEK_STARTING_FRI_SEQ_NO        
	Sequential Week number as offset from Week starting Friday, 1753/01/05
WEEK_STARTING_SAT_SEQ_NO        
	Sequential Week number as offset from Week starting Saturday, 1753/01/06
JULIAN_DATE                     
	Julian Date number as offset from noon on January 1, 4713 BCE
	to noon on day of DATE in system of Joseph Scaliger
MODIFIED_JULIAN_DATE            
	Modified Julian Date number as offset from midnight(00:00:00.000) on
	1858/11/17 to midnight(00:00:00.000) on day of DATE
ISO_DATE                        
	ISO 8601 Date in format YYYY-MM-DD, Example = 2004-02-29
ISO_YEAR_WEEK_NO                
	ISO 8601 year and week in format YYYYWW, Example = 200403
ISO_WEEK_NO                     
	ISO 8601 week of year in format WW, Example = 52
ISO_DAY_OF_WEEK                 
	ISO 8601 Day of week number, 
	Mon=1, Tue=2, Wed=3, Thu=4, Fri=5, Sat=6, Sun=7
ISO_YEAR_WEEK_NAME              
	ISO 8601 year and week in format YYYY-WNN, Example = 2004-W52
ISO_YEAR_WEEK_DAY_OF_WEEK_NAME  
	ISO 8601 year, week, and day of week in format YYYY-WNN-D,
	Example = 2004-W52-2
DATE_FORMAT_YYYY_MM_DD          
	Text date in format YYYY/MM/DD, Example = 2004/02/29
DATE_FORMAT_YYYY_M_D            
	Text date in format YYYY/M/D, Example = 2004/2/9
DATE_FORMAT_MM_DD_YYYY          
	Text date in format MM/DD/YYYY, Example = 06/05/2004
DATE_FORMAT_M_D_YYYY            
	Text date in format M/D/YYYY, Example = 6/5/2004
DATE_FORMAT_MMM_D_YYYY          
	Text date in format MMM D, YYYY, Example = Jan 4, 2006
DATE_FORMAT_MMMMMMMMM_D_YYYY    
	Text date in format MMMMMMMMM D, YYYY, Example = September 3, 2004
DATE_FORMAT_MM_DD_YY            
	Text date in format MM/DD/YY, Example = 06/05/97
DATE_FORMAT_M_D_YY              
	Text date in format M/D/YY, Example = 6/5/97
*/
returns  @DATE table 
(
[DATE_ID]				[int]		not null 
	primary key clustered,
[DATE]					[datetime]	not null ,
[NEXT_DAY_DATE]				[datetime]	not null ,
[YEAR]					[smallint]	not null ,
[YEAR_QUARTER]				[int]	not null ,
[YEAR_MONTH]				[int]		not null ,
[YEAR_DAY_OF_YEAR]			[int]		not null ,
[QUARTER]				[tinyint]	not null ,
[MONTH]					[tinyint]	not null ,
[DAY_OF_YEAR]				[smallint]	not null ,
[DAY_OF_MONTH]				[smallint]	not null ,
[DAY_OF_WEEK]				[tinyint]	not null ,
[YEAR_NAME]				[varchar] (4)	not null ,
[YEAR_QUARTER_NAME]			[varchar] (7)	not null ,
[YEAR_MONTH_NAME]			[varchar] (8)	not null ,
[YEAR_MONTH_NAME_LONG]			[varchar] (14)	not null ,
[QUARTER_NAME]				[varchar] (2)	not null ,
[MONTH_NAME]				[varchar] (3)	not null ,
[MONTH_NAME_LONG]			[varchar] (9)	not null ,
[WEEKDAY_NAME]				[varchar] (3)	not null ,
[WEEKDAY_NAME_LONG]			[varchar] (9)	not null ,
[START_OF_YEAR_DATE]			[datetime]	not null ,
[END_OF_YEAR_DATE]			[datetime]	not null ,
[START_OF_QUARTER_DATE]			[datetime]	not null ,
[END_OF_QUARTER_DATE]			[datetime]	not null ,
[START_OF_MONTH_DATE]			[datetime]	not null ,
[END_OF_MONTH_DATE]			[datetime]	not null ,
[START_OF_WEEK_STARTING_SUN_DATE]	[datetime]	not null ,
[END_OF_WEEK_STARTING_SUN_DATE]		[datetime]	not null ,
[START_OF_WEEK_STARTING_MON_DATE]	[datetime]	not null ,
[END_OF_WEEK_STARTING_MON_DATE]		[datetime]	not null ,
[START_OF_WEEK_STARTING_TUE_DATE]	[datetime]	not null ,
[END_OF_WEEK_STARTING_TUE_DATE]		[datetime]	not null ,
[START_OF_WEEK_STARTING_WED_DATE]	[datetime]	not null ,
[END_OF_WEEK_STARTING_WED_DATE]		[datetime]	not null ,
[START_OF_WEEK_STARTING_THU_DATE]	[datetime]	not null ,
[END_OF_WEEK_STARTING_THU_DATE]		[datetime]	not null ,
[START_OF_WEEK_STARTING_FRI_DATE]	[datetime]	not null ,
[END_OF_WEEK_STARTING_FRI_DATE]		[datetime]	not null ,
[START_OF_WEEK_STARTING_SAT_DATE]	[datetime]	not null ,
[END_OF_WEEK_STARTING_SAT_DATE]		[datetime]	not null ,
[QUARTER_SEQ_NO]			[int]		not null ,
[MONTH_SEQ_NO]				[int]		not null ,
[WEEK_STARTING_SUN_SEQ_NO]		[int]		not null ,
[WEEK_STARTING_MON_SEQ_NO]		[int]		not null ,
[WEEK_STARTING_TUE_SEQ_NO]		[int]		not null ,
[WEEK_STARTING_WED_SEQ_NO]		[int]		not null ,
[WEEK_STARTING_THU_SEQ_NO]		[int]		not null ,
[WEEK_STARTING_FRI_SEQ_NO]		[int]		not null ,
[WEEK_STARTING_SAT_SEQ_NO]		[int]		not null ,
[JULIAN_DATE]				[int]		not null ,
[MODIFIED_JULIAN_DATE]			[int]		not null ,
[ISO_DATE]				[varchar](10)	not null ,
[ISO_YEAR_WEEK_NO]			[int]		not null ,
[ISO_WEEK_NO]				[smallint]	not null ,
[ISO_DAY_OF_WEEK]			[tinyint]	not null ,
[ISO_YEAR_WEEK_NAME]			[varchar](8)	not null ,
[ISO_YEAR_WEEK_DAY_OF_WEEK_NAME]	[varchar](10)	not null ,
[DATE_FORMAT_YYYY_MM_DD]		[varchar](10)	not null ,
[DATE_FORMAT_YYYY_M_D]			[varchar](10)	not null ,
[DATE_FORMAT_MM_DD_YYYY]		[varchar](10)	not null ,
[DATE_FORMAT_M_D_YYYY]			[varchar](10)	not null ,
[DATE_FORMAT_MMM_D_YYYY]		[varchar](12)	not null ,
[DATE_FORMAT_MMMMMMMMM_D_YYYY]		[varchar](18)	not null ,
[DATE_FORMAT_MM_DD_YY]			[varchar](8)	not null ,
[DATE_FORMAT_M_D_YY]			[varchar](8)	not null 
) 
as
begin
declare @cr			varchar(2)
select @cr			= char(13)+Char(10)
declare @ErrorMessage		varchar(400)
declare @START_DATE		datetime
declare @END_DATE		datetime
declare @LOW_DATE	datetime
declare	@start_no	int
declare	@end_no	int
-- Verify @FIRST_DATE is not null 
if @FIRST_DATE is null
	begin
	select @ErrorMessage =
		'@FIRST_DATE cannot be null'
	goto Error_Exit
	end
-- Verify @LAST_DATE is not null 
if @LAST_DATE is null
	begin
	select @ErrorMessage =
		'@LAST_DATE cannot be null'
	goto Error_Exit
	end
-- Verify @FIRST_DATE is not before 1754-01-01
IF  @FIRST_DATE < '17540101'	begin
	select @ErrorMessage =
		'@FIRST_DATE cannot before 1754-01-01'+
		', @FIRST_DATE = '+
		isnull(convert(varchar(40),@FIRST_DATE,121),'NULL')
	goto Error_Exit
	end
-- Verify @LAST_DATE is not after 9997-12-31
IF  @LAST_DATE > '99971231'	begin
	select @ErrorMessage =
		'@LAST_DATE cannot be after 9997-12-31'+
		', @LAST_DATE = '+
		isnull(convert(varchar(40),@LAST_DATE,121),'NULL')
	goto Error_Exit
	end
-- Verify @FIRST_DATE is not after @LAST_DATE
if @FIRST_DATE > @LAST_DATE
	begin
	select @ErrorMessage =
		'@FIRST_DATE cannot be after @LAST_DATE'+
		', @FIRST_DATE = '+
		isnull(convert(varchar(40),@FIRST_DATE,121),'NULL')+
		', @LAST_DATE = '+
		isnull(convert(varchar(40),@LAST_DATE,121),'NULL')
	goto Error_Exit
	end
-- Set @START_DATE = @FIRST_DATE at midnight
select @START_DATE	= dateadd(dd,datediff(dd,0,@FIRST_DATE),0)
-- Set @END_DATE = @LAST_DATE at midnight
select @END_DATE	= dateadd(dd,datediff(dd,0,@LAST_DATE),0)
-- Set @LOW_DATE = earliest possible SQL Server datetime
select @LOW_DATE	= convert(datetime,'17530101')
-- Find the number of day from 1753-01-01 to @START_DATE and @END_DATE
select	@start_no	= datediff(dd,@LOW_DATE,@START_DATE) ,
	@end_no	= datediff(dd,@LOW_DATE,@END_DATE)
-- Declare number tables
declare @num1 table (NUMBER int not null primary key clustered)
declare @num2 table (NUMBER int not null primary key clustered)
declare @num3 table (NUMBER int not null primary key clustered)
-- Declare table of ISO Week ranges
declare @ISO_WEEK table
(
[ISO_WEEK_YEAR] 		int		not null
	primary key clustered,
[ISO_WEEK_YEAR_START_DATE]	datetime	not null,
[ISO_WEEK_YEAR_END_DATE]	Datetime	not null
)
-- Find rows needed in number tables
declare	@rows_needed		int
declare	@rows_needed_root	int
select	@rows_needed		= @end_no - @start_no + 1
select  @rows_needed		=
		case
		when @rows_needed < 10
		then 10
		else @rows_needed
		end
select	@rows_needed_root	= convert(int,ceiling(sqrt(@rows_needed)))
-- Load number 0 to 16
insert into @num1 (NUMBER)
select NUMBER = 0 union all select  1 union all select  2 union all
select          3 union all select  4 union all select  5 union all
select          6 union all select  7 union all select  8 union all
select          9 union all select 10 union all select 11 union all
select         12 union all select 13 union all select 14 union all
select         15
order by
	1
-- Load table with numbers zero thru square root of the number of rows needed +1
insert into @num2 (NUMBER)
select
	NUMBER = a.NUMBER+(16*b.NUMBER)+(256*c.NUMBER)
from
	@num1 a cross join @num1 b cross join @num1 c
where
	a.NUMBER+(16*b.NUMBER)+(256*c.NUMBER) <
	@rows_needed_root
order by
	1
-- Load table with the number of rows needed for the date range
insert into @num3 (NUMBER)
select
	NUMBER = a.NUMBER+(@rows_needed_root*b.NUMBER)
from
	@num2 a
	cross join
	@num2 b
where
	a.NUMBER+(@rows_needed_root*b.NUMBER) < @rows_needed
order by
	1
declare	@iso_start_year	int
declare	@iso_end_year	int
select	@iso_start_year	= datepart(year,dateadd(year,-1,@start_date))
select	@iso_end_year	= datepart(year,dateadd(year,1,@end_date))
-- Load table with start and end dates for ISO week years
insert into @ISO_WEEK
	(
	[ISO_WEEK_YEAR],
	[ISO_WEEK_YEAR_START_DATE],
	[ISO_WEEK_YEAR_END_DATE]
	)
select
	[ISO_WEEK_YEAR] = a.NUMBER,
	[0ISO_WEEK_YEAR_START_DATE]	=
		dateadd(dd,(datediff(dd,@LOW_DATE,
		dateadd(day,3,dateadd(year,a.[NUMBER]-1900,0))
		)/7)*7,@LOW_DATE),
	[ISO_WEEK_YEAR_END_DATE]	=
		dateadd(dd,-1,dateadd(dd,(datediff(dd,@LOW_DATE,
		dateadd(day,3,dateadd(year,a.[NUMBER]+1-1900,0))
		)/7)*7,@LOW_DATE))
from
	(
	select
		NUMBER = NUMBER+@iso_start_year
	from
		@num3
	where
		NUMBER+@iso_start_year <= @iso_end_year
	) a
order by
	a.NUMBER
-- Load Date table
insert into @DATE
select
	[DATE_ID]			= a.[DATE_ID] ,
	[DATE]				= a.[DATE] ,
	[NEXT_DAY_DATE]			=
		dateadd(day,1,a.[DATE]) ,
	[YEAR]			=
		datepart(year,a.[DATE]) ,
	[YEAR_QUARTER]		=
		(10*datepart(year,a.[DATE]))+datepart(quarter,a.[DATE]) ,
	[YEAR_MONTH]		=
		(100*datepart(year,a.[DATE]))+datepart(month,a.[DATE]) ,
	[YEAR_DAY_OF_YEAR]		=
		(1000*datepart(year,a.[DATE]))+
		datediff(dd,dateadd(yy,datediff(yy,0,a.[DATE]),0),a.[DATE])+1 ,
	[QUARTER]		=
		datepart(quarter,a.[DATE]) ,
	[MONTH]		=
		datepart(month,a.[DATE]) ,
	[DAY_OF_YEAR]			=
		datediff(dd,dateadd(yy,datediff(yy,0,a.[DATE]),0),a.[DATE])+1 ,
	[DAY_OF_MONTH]			=
		datepart(day,a.[DATE]) ,
	[DAY_OF_WEEK]		=
		-- Sunday = 1, Monday = 2, ,,,Saturday = 7
		(datediff(dd,'17530107',a.[DATE])%7)+1  ,
	[YEAR_NAME]		=
		datename(year,a.[DATE]) ,
	[YEAR_QUARTER_NAME]	=
		datename(year,a.[DATE])+' Q'+datename(quarter,a.[DATE]) ,
	[YEAR_MONTH_NAME]	=
		datename(year,a.[DATE])+' '+left(datename(month,a.[DATE]),3) ,
	[YEAR_MONTH_NAME_LONG]	=
		datename(year,a.[DATE])+' '+datename(month,a.[DATE]) ,
	[QUARTER_NAME]		=
		'Q'+datename(quarter,a.[DATE]) ,
	[MONTH_NAME]		=
		left(datename(month,a.[DATE]),3) ,
	[MONTH_NAME_LONG]	=
		datename(month,a.[DATE]) ,
	[WEEKDAY_NAME]		=
		left(datename(weekday,a.[DATE]),3) ,
	[WEEKDAY_NAME_LONG]	=
		datename(weekday,a.[DATE]),
	[START_OF_YEAR_DATE]	=
		dateadd(year,datediff(year,0,a.[DATE]),0) ,
	[END_OF_YEAR_DATE]	=
		dateadd(day,-1,dateadd(year,datediff(year,0,a.[DATE])+1,0)) ,
	[START_OF_QUARTER_DATE]	=
		dateadd(quarter,datediff(quarter,0,a.[DATE]),0) ,
	[END_OF_QUARTER_DATE]	=
		dateadd(day,-1,dateadd(quarter,datediff(quarter,0,a.[DATE])+1,0)) ,
	[START_OF_MONTH_DATE]	=
		dateadd(month,datediff(month,0,a.[DATE]),0) ,
	[END_OF_MONTH_DATE]	=
		dateadd(day,-1,dateadd(month,datediff(month,0,a.[DATE])+1,0)),
	[START_OF_WEEK_STARTING_SUN_DATE]	=
		dateadd(dd,(datediff(dd,'17530107',a.[DATE])/7)*7,'17530107'),
	[END_OF_WEEK_STARTING_SUN_DATE]		=
		dateadd(dd,((datediff(dd,'17530107',a.[DATE])/7)*7)+6,'17530107'),
	[START_OF_WEEK_STARTING_MON_DATE]	=
		dateadd(dd,(datediff(dd,'17530101',a.[DATE])/7)*7,'17530101'),
	[END_OF_WEEK_STARTING_MON_DATE]		=
		dateadd(dd,((datediff(dd,'17530101',a.[DATE])/7)*7)+6,'17530101'),
	[START_OF_WEEK_STARTING_TUE_DATE]	=
		dateadd(dd,(datediff(dd,'17530102',a.[DATE])/7)*7,'17530102'),
	[END_OF_WEEK_STARTING_TUE_DATE]		=
		dateadd(dd,((datediff(dd,'17530102',a.[DATE])/7)*7)+6,'17530102'),
	[START_OF_WEEK_STARTING_WED_DATE]	=
		dateadd(dd,(datediff(dd,'17530103',a.[DATE])/7)*7,'17530103'),
	[END_OF_WEEK_STARTING_WED_DATE]		=
		dateadd(dd,((datediff(dd,'17530103',a.[DATE])/7)*7)+6,'17530103'),
	[START_OF_WEEK_STARTING_THU_DATE]	=
		dateadd(dd,(datediff(dd,'17530104',a.[DATE])/7)*7,'17530104'),
	[END_OF_WEEK_STARTING_THU_DATE]		=
		dateadd(dd,((datediff(dd,'17530104',a.[DATE])/7)*7)+6,'17530104'),
	[START_OF_WEEK_STARTING_FRI_DATE]	=
		dateadd(dd,(datediff(dd,'17530105',a.[DATE])/7)*7,'17530105'),
	[END_OF_WEEK_STARTING_FRI_DATE]		=
		dateadd(dd,((datediff(dd,'17530105',a.[DATE])/7)*7)+6,'17530105'),
	[START_OF_WEEK_STARTING_SAT_DATE]	=
		dateadd(dd,(datediff(dd,'17530106',a.[DATE])/7)*7,'17530106'),
	[END_OF_WEEK_STARTING_SAT_DATE]		=
		dateadd(dd,((datediff(dd,'17530106',a.[DATE])/7)*7)+6,'17530106'),
	[QUARTER_SEQ_NO]			= 
		datediff(quarter,@LOW_DATE,a.[DATE]),
	[MONTH_SEQ_NO]				=
		datediff(month,@LOW_DATE,a.[DATE]),
	[WEEK_STARTING_SUN_SEQ_NO]		=
		datediff(day,'17530107',a.[DATE])/7,
	[WEEK_STARTING_MON_SEQ_NO]		=
		datediff(day,'17530101',a.[DATE])/7,
	[WEEK_STARTING_TUE_SEQ_NO]		=
		datediff(day,'17530102',a.[DATE])/7,
	[WEEK_STARTING_WED_SEQ_NO]		=
		datediff(day,'17530103',a.[DATE])/7,
	[WEEK_STARTING_THU_SEQ_NO]		=
		datediff(day,'17530104',a.[DATE])/7,
	[WEEK_STARTING_FRI_SEQ_NO]		=
		datediff(day,'17530105',a.[DATE])/7,
	[WEEK_STARTING_SAT_SEQ_NO]		=
		datediff(day,'17530106',a.[DATE])/7,
	[JULIAN_DATE]			=
		datediff(day,@LOW_DATE,a.[DATE])+2361331,
	[MODIFIED_JULIAN_DATE]		=
		datediff(day,'18581117',a.[DATE]),
--/*
	[ISO_DATE]		=
		replace(convert(char(10),a.[DATE],111),'/','-') ,
	[ISO_YEAR_WEEK_NO]		=
		(100*b.[ISO_WEEK_YEAR])+
		(datediff(dd,b.[ISO_WEEK_YEAR_START_DATE],a.[DATE])/7)+1 ,
	[ISO_WEEK_NO]		=
		(datediff(dd,b.[ISO_WEEK_YEAR_START_DATE],a.[DATE])/7)+1 ,
	[ISO_DAY_OF_WEEK]		=
		-- Sunday = 1, Monday = 2, ,,,Saturday = 7
		(datediff(dd,@LOW_DATE,a.[DATE])%7)+1  ,
	[ISO_YEAR_WEEK_NAME]		=
		convert(varchar(4),b.[ISO_WEEK_YEAR])+'-W'+
		right('00'+convert(varchar(2),(datediff(dd,b.[ISO_WEEK_YEAR_START_DATE],a.[DATE])/7)+1),2) ,
	[ISO_YEAR_WEEK_DAY_OF_WEEK_NAME]		=
		convert(varchar(4),b.[ISO_WEEK_YEAR])+'-W'+
		right('00'+convert(varchar(2),(datediff(dd,b.[ISO_WEEK_YEAR_START_DATE],a.[DATE])/7)+1),2) +
		'-'+convert(varchar(1),(datediff(dd,@LOW_DATE,a.[DATE])%7)+1) ,
--*/
	[DATE_FORMAT_YYYY_MM_DD]		=
		convert(char(10),a.[DATE],111) ,
	[DATE_FORMAT_YYYY_M_D]		= 
		convert(varchar(10),
		convert(varchar(4),year(a.[DATE]))+'/'+
		convert(varchar(2),day(a.[DATE]))+'/'+
		convert(varchar(2),month(a.[DATE]))),
	[DATE_FORMAT_MM_DD_YYYY]		= 
		convert(char(10),a.[DATE],101) ,
	[DATE_FORMAT_M_D_YYYY]		= 
		convert(varchar(10),
		convert(varchar(2),month(a.[DATE]))+'/'+
		convert(varchar(2),day(a.[DATE]))+'/'+
		convert(varchar(4),year(a.[DATE]))),
	[DATE_FORMAT_MMM_D_YYYY]		= 
		convert(varchar(12),
		left(datename(month,a.[DATE]),3)+' '+
		convert(varchar(2),day(a.[DATE]))+', '+
		convert(varchar(4),year(a.[DATE]))),
	[DATE_FORMAT_MMMMMMMMM_D_YYYY]	= 
		convert(varchar(18),
		datename(month,a.[DATE])+' '+
		convert(varchar(2),day(a.[DATE]))+', '+
		convert(varchar(4),year(a.[DATE]))),
	[DATE_FORMAT_MM_DD_YY]		=
		convert(char(8),a.[DATE],1) ,
	[DATE_FORMAT_M_D_YY]		= 
		convert(varchar(8),
		convert(varchar(2),month(a.[DATE]))+'/'+
		convert(varchar(2),day(a.[DATE]))+'/'+
		right(convert(varchar(4),year(a.[DATE])),2))
from
	(
	-- Derived table is all dates needed for date range
	select	top 100 percent
		[DATE_ID]	= aa.[NUMBER],
		[DATE]		=
			dateadd(dd,aa.[NUMBER],@LOW_DATE)
	from
		(
		select
			NUMBER = NUMBER+@start_no 
		from
			@num3
		where
			NUMBER+@start_no <= @end_no
		) aa
	order by
		aa.[NUMBER]
	) a
	join
	-- Match each date to the proper ISO week year
	@ISO_WEEK b
	on a.[DATE] between 
		b.[ISO_WEEK_YEAR_START_DATE] and 
		b.[ISO_WEEK_YEAR_END_DATE]
order by
	a.[DATE_ID]
return
Error_Exit:
-- Return a pesudo error message by trying to
-- convert an error message string to an int.
-- This method is used because the error displays
-- the string it was trying to convert, and so the
-- calling application sees a formatted error message.
declare @error int
set @error = convert(int,@cr+@cr+
'*******************************************************************'+@cr+
'* Error in function func_dates:'+@cr+'* '+
isnull(@ErrorMessage,'Unknown Error')+@cr+
'*******************************************************************'+@cr+@cr)
return
end
