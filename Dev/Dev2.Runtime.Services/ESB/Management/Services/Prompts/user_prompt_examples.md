Assign Tool
----------------
set userid = [[newid]], total = 10 * 30

set id = 10, name = [[yogesh]]

set data = 10, id = 100, part(1).code = C, part(1).qty = 20 * 3. 
On Error store in [[part_err]], post to [[www.abc.com]] and stop execution.


Set [[oid]] to [[dataid]], [[part(3).code]] to C199, [[part(*).name]] to Car, [[@order.id]] to oid, [[@order.qty]] to 10, [[@order.total]] to 8200 and result to "okay". 
On error store in [[order_err]], post error to http://orders.report.ai?d=[[order_err]], stop on error. 
Place at 100, 250.



## EXAMPLES OF VALID USER PROMPTS YOU CAN PARSE

- "Set [[username]] to John, [[age]] to 30, [[score]] to [[rawScore]] * 0.5. Label it User Init. Post errors to https://logs.myapp.com/error and stop on error."
- "Assign empname = Sachin, empid = S001. On error capture in [[err]] and don't end workflow."
- "Initialize [[total]] with 100 * 1.15 and [[status]] with Active. Call this tool Tax Assign."
- "store [[city]] = Mumbai and [[country]] = India. place at 300, 400."
- "put Hello World into [[greeting]]. Error webhook: https://hook.io/err"


Http Post Method
--------------------

HTTP POST with headers Content-Type=application/json and User-Agent=Mozilla/5.0. Source WebSource.
Body is {"id": 6, "name": "Apple AirPods", "data": {"generation": "3rd", "price": 120}}. 
Map id, name, createdAt, data.generation, data.price to [[person()]]. 
Conditions: orderid=o1, qty=10, total=100. 
On error store in [[post_err]], post error to http://persons.ai?p=[[post_err]], stop on error. 
Place at 100, 400.






HTTP POST with headers Content-Type=application/json and User-Agent=Mozilla/5.0. Source WebSourceToPost. 
Body is {"id": 6, "name": "Apple AirPods", "data": {"generation": "3rd", "price": 120}}. 
Map id, name, createdAt, data.generation, data.price to [[person()]]. 
Conditions: orderid=o1, qty=10, total=100. On error store in [[post_err]], post error to http://persons.ai?p=[[post_err]], stop on error. 
Place at 100, 400.



Http Get Method
---------------------------
HTTP GET with headers auth=Test and agent=WW. Query string ?q=give. 
Source WebSourceToPost. 
Map id, body, postId, likes, user.id, user.username, user.fullName to [[data()]]. 
On error store in [[comments_err]], post error to http://comments.ai?d=[[comments_err]], stop on error. 
Place at 100, 400.


SQL database
-------------------------------
Execute stored procedure dbo.GetSalesByQuantity. Source SQLServerTestDB (fetch source id from resource SQLServerTestDB)
Input MinimumQuantity = [[minQty]]. Recordset Name SalesData Output SaleId, ProductName, TotalAmount. On error [[sql_err]], post to http://err.io."


Execute stored procedure dbo.GetSalesByQuantity. Source OrdersDB (fetch source id from resource OrdersDB)
Input MinimumQuantity = [[minQty]]. Recordset Name SalesData Output SaleId, ProductName, TotalAmount. On error [[sql_err]], post to http://err.io."


Execute stored procedure dbo.GetSalesByQuantity. Source SqlServerMaster(fetch source id from resource SqlServerMaster)
Input MinimumQuantity = [[minQty]]. Recordset Name SalesData Output SaleId, ProductName, TotalAmount. On error [[sql_err]], post to http://err.io."