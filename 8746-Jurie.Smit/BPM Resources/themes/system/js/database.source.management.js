// initDatabaseSourceMgt('[[Dev2WebServer]]','[[Dev2SourceName]]', '[[Dev2ServiceName]]', "[[Dev2Category]]",'[[Dev2Help]]','[[Dev2Icon]]','[[Dev2Description]]','[[Dev2Tags]]','[[Dev2StudioExe]]');
//dbSourceCancelBtn('[[Dev2WebServer]]','[[Dev2SourceName]]', '[[Dev2ServiceName]]', "[[Dev2Category]]",'[[Dev2Help]]','[[Dev2Icon]]','[[Dev2Description]]','[[Dev2Tags]]','[[Dev2StudioExe]]','[[Dev2NewService]]');
function dbSourceCancelBtn(webServer, sourceName, serviceName, category, help, icon, desc, tags, Dev2StudioExe, isNew){
	var srcStatus = "equal";

	if(Dev2StudioExe == ''){
		bindDev2PluginStep2BackBtn(serviceName,"Database", sourceName,category, help, icon, desc, tags, srcStatus, isNew); 
	}else{
		Dev2Awesomium.Cancel();
	}	
}

// <script>bindDev2SourceManagementSourceForEdit('[[Dev2SourceManagementSource]]','[[Dev2ServiceName]]','[[Dev2Category]]','[[Dev2Help]]','[[Dev2Icon]]','[[Dev2Description]]','[[Dev2Tags]]','[[Dev2StudioExe]]'); </script>		
function initDatabaseSourceMgt(webServer, sourceName, serviceName, category, help, icon, desc, tags, Dev2StudioExe, isNew){
	/* DatabaseSourceManagement wizard javascript */ 
	$(document).ready(function(){			
		try{
			/* region FIELDS */
			var databaseType = "";
			var databaseAlias = "";
			var databaseServer = "";
			var databaseName = "";
			var username="";
			var password="";
			var errorMessage="";
			var databaseTypes=[];
			var computers=[];
			var sourceTemplate="";
			var sourceExists=false;
			var closeWindow=false;

			/* hide the dialog title bar */
			$(".ui-dialog-titlebar").hide();
			
			// set alias name
			if(sourceName == "undefined"){
				sourceName = "";
			}
			
			
			/* when category textbox has focus fire autocomplete feature */
			$( "#Dev2DatabaseSourceCategory" ).autocomplete({
				source: buildCategories(webServer) ,
				minLength: 0,
				delay: 0
			});
			
			$("#Dev2SourceManagementDatabaseAlias").val(sourceName);
			/* populate "source" drop down list */
			
			$("#Dev2SourceManagementDatabaseSource").html('Loading...');
			
			$.ajax({
				type:"GET",
				url:webServer+"/services/FindResourcesService",
				data:{"ResouceName":"*","ResourceType":"Source","Roles":"*"},
				dataType:"xml",
				async:false,
				success:function(xml){
				
					var errorHandler = new Dev2ErrorHandler(xml);

					if(!errorHandler.Has200ErrorPayload()){
						$("#Dev2SourceManagementDatabaseSource").html('');
						$("#Dev2SourceManagementDatabaseSource").append("<option>New....</option>");						
						$(xml).find("Dev2Resource").each(function(){
							var type = $(this).find("Dev2SourceType").text();
							var myCat = $(this).find("Category").text();
							
							// Dev2SourceManagementDatabaseSource
							if(type.indexOf("Database") != -1){
							
								var val = $(this).find("Dev2SourceName").text();
								
								
								if(val == sourceName){
								
									// Clean up the XML
									var cleanXML = $(this).find("Dev2SourceContents").text().replace("/&lt;/g/",">").replace("/&gt;/g/","<");
									
									var s1 = cleanXML.indexOf("ConnectionString=");
									
									if(s1 > 0){
										s1+=18;
										var e1 = cleanXML.indexOf('"', s1);
										var abc = cleanXML.substr(s1, (e1-s1));
										magicString = abc;
									}
								
									//magicString = $(this).find("Dev2SourceContents").find("Source").attr("ConnectionString");
									
									
									$("#Dev2DatabaseSourceCategory").val(myCat);
									$("#Dev2SourceManagementDatabaseSource").append("<option selected>"+val+"</option>");
								}else{
									$("#Dev2SourceManagementDatabaseSource").append("<option>"+val+"</option>");
								}	
							}	
						});
					}else{
						errorHandler.Handle200ErrorPayload();
					}
				}, 
				error:function(xml){
					var errorHandler = new Dev2ErrorHandler(xml);
					errorHandler.HandleNon200ErrorPayload();
				}
			});

			/* get computers on the network */
			$.ajax({
				type:"GET",
				url:webServer+"/services/"+"FindNetworkComputersService",
				dataType:"json",
				async:false,
				success:function(data){
					var errorHandler = new Dev2ErrorHandler(data);
				
					if(!errorHandler.Has200ErrorPayload()){
						$(data).each(function(index,element){
							computers.push(element.ComputerName);
						});
					}else{
						errorHandler.Handle200ErrorPayload();
					}
				},
				error:function(data){
					var errorHandler = new Dev2ErrorHandler(data);
					errorHandler.HandleNon200ErrorPayload();
				}
			});	

			$("#Dev2SourceManagementDatabaseDoneButton").css('background-color','#CCC');
			$("#Dev2SourceManagementDatabaseDoneButton").attr('disabled',true);
			/* disable username and password textboxes when windows radio button is selected */
			if($("#windows").is(":checked")){
				$("#Dev2SourceManagementDatabaseUsername").prop("disabled",true);
				$("#Dev2SourceManagementDatabasePassword").prop("disabled",true);
			}
			/* autocomplete for the "Server:" input textbox */
			$("#Dev2SourceManagementDatabaseServer").autocomplete({
				source: computers,
				minLength: 0,
				delay: 0
			});

			function setDoneButtonState(flag){
				if(flag){
					$("#Dev2SourceManagementDatabaseDoneButton").css('background-color','');
					$("#Dev2SourceManagementDatabaseDoneButton").attr('disabled',false);
				}else{
					$("#Dev2SourceManagementDatabaseDoneButton").css('background-color','#CCC');
					$("#Dev2SourceManagementDatabaseDoneButton").attr('disabled',true);
				}
			}
			function alertDialog(message, callback){
				$("#Dev2SourceManagementAlert").html(message);
				/* password prompt dialog */
				$("#Dev2SourceManagementAlertDialog").dialog({
					autoOpen: false,
					bgiframe:true,
					modal:true,
					width:280,
					height:'auto',
					resizable:false,
					buttons:
					[
						{text:"OK",
							click : function(){
								$(this).dialog("close");
								try{
									callback();
								}catch(e){}
							}
						}
					]
				});
				/* hide the dialog title bar */
				$(".ui-dialog-titlebar").hide();
				/* open dialog */
				$("#Dev2SourceManagementAlertDialog").dialog('open');
			}

			$('input[type=radio]').click(function(){			
				if($(this).hasClass('enable_auth_box')){
					$("#Dev2SourceManagementDatabaseUsername").attr("disabled",false);
					$("#Dev2SourceManagementDatabasePassword").prop("disabled",false);
					$("#Dev2SourceManagementDatabaseDataSource").html('');
				}else if($(this).hasClass('disable_auth_box')){
					$("#Dev2SourceManagementDatabaseUsername").prop("disabled",true);
					$("#Dev2SourceManagementDatabasePassword").prop("disabled",true);
					$("#Dev2SourceManagementDatabaseUsername").val("");
					$("#Dev2SourceManagementDatabasePassword").val("");
					$("#Dev2SourceManagementDatabaseDataSource").html('');
					username="";
					password="";
					$.ajax({
						type:"GET",
						url:webServer + "/services/FindSqlDatabasesService",
						data: {"ServerName":databaseServer,"Username":username,"Password":password},
						dataType:"xml",
						success:function(xml){
						
							var errorHandler = new Dev2ErrorHandler(xml);
				
							if(!errorHandler.Has200ErrorPayload()){
								setDoneButtonState(true);
								$(xml).find("Table").each(function(){
									$("#Dev2SourceManagementDatabaseDataSource").append("<option>"+$(this).find("DATABASE_NAME").text()+"</option>");
								});
							}else{
								setDoneButtonState(false);
								errorHandler.Handle200ErrorPayload();
								return false;
							}
						},
						error : function(data){
							var errorHandler = new Dev2ErrorHandler(data);
							errorHandler.HandleNon200ErrorPayload();
						}
					});
				}
			});	
			/* get sqlserver databases */
			$("#Dev2DatabaseSourceManagementTestConnection").click(function(){
				databaseServer = $("#Dev2SourceManagementDatabaseServer").val();
				username = $("#Dev2SourceManagementDatabaseUsername").val();
				password = $("#Dev2SourceManagementDatabasePassword").val();
				if($("#specified").is(":checked")){
					if(username.length == 0){
						$("#Dev2SourceManagementDatabaseUsername").focus();	
						setDoneButtonState(false);
						alertDialog("Please enter username");
						return false;
					}					
				}	
				if(databaseServer.length > 0){
					var selectedDB = $("#Dev2SourceManagementDatabaseDataSource option:selected").val();
					
					$.ajax({
						type:"GET",
						url: webServer + "/services/FindSqlDatabasesService",
						data: {"ServerName":databaseServer,"Username":username,"Password":password},
						dataType:"xml",
						success:function(xml){
						
							var errorHandler = new Dev2ErrorHandler(xml);
				
							if(!errorHandler.Has200ErrorPayload()){
								$("#Dev2SourceManagementDatabaseDataSource").html('');
								setDoneButtonState(true);
								alertDialog("Connection successful!");							
								$(xml).find("Table").each(function(){
									var val = $(this).find("DATABASE_NAME").text();
									
									if(val == selectedDB){
										$("#Dev2SourceManagementDatabaseDataSource").append("<option selected>"+val+"</option>");
									}else{
										$("#Dev2SourceManagementDatabaseDataSource").append("<option>"+val+"</option>");
									}
									
								});
							}else{
								setDoneButtonState(false);
								errorHandler.Handle200ErrorPayload();
							}
						},
						error : function(data){
							var errorHandler = new Dev2ErrorHandler(data);
							errorHandler.HandleNon200ErrorPayload();
						}
					});
				}else{
					setDoneButtonState(false);
					alertDialog("Please enter server name");
				}
			});
			
			/* populate web form based on selected source */
			$("#Dev2SourceManagementDatabaseSource").change(function(){
				$.ajax({
					type:"GET",
					url:webServer+"/services/FindResourcesService",
					data:{"ResouceName":"*","ResourceType":"Source","Roles":"Business Design Studio Developers"},
					dataType:"xml",
					
					success:function(xml){
					
						var sourceName = $("#Dev2SourceManagementDatabaseSource option:selected").val();
						$("#windows").attr("checked",true);
						databaseTypes=[];
						$(xml).find("Dev2Resource").each(function(){
							var type = $(this).find("Dev2SourceType").text();
							if($(this).find("Dev2SourceName").text()==sourceName){
								if(type.indexOf("Database") != -1){
									//$("#Dev2SourceManagementDatabaseType").html('');
									var contents = $(this).find("Dev2SourceContents");								
									$("#Dev2SourceManagementDatabaseType").append("<option>"+contents.find("Source").attr("Type")+"</option>");
									$("#Dev2SourceManagementDatabaseAlias").val(contents.find("Source").attr("Name"));
									var tokens = contents.find("Source").attr("ConnectionString").split(";");
									var svr = tokens[0].split("=")[1];
									$("#Dev2SourceManagementDatabaseServer").val(svr);
									$("#Dev2SourceManagementDatabaseDataSource").html('');
									
									
									$("#Dev2DatabaseSourceCategory").val(contents.find("Category").text());
									
									
									var db = tokens[1].split("=")[1];
									
									var magicString = contents.find("Source").attr("ConnectionString");
									// re-populate data
									if(magicString != ""){
				
										var magicStringUtil = new Dev2DBConStringUtil(magicString);
										
										if(magicStringUtil.IsDBAuth()){
											// set user and pass
											$("#Dev2SourceManagementDatabaseUsername").val(magicStringUtil.FetchUser());
											$("#Dev2SourceManagementDatabasePassword").val(magicStringUtil.FetchPassword());
											$("#windows").attr("checked", "false");
											$("#specified").attr("checked", "true");
										}
											
										// set the DB server
										$("#Dev2SourceManagementDatabaseServer").val(magicStringUtil.FetchServer());
										buildDBList(magicStringUtil.FetchDB());
									}
									
									return false;
								}
							}else{
								$("#Dev2SourceManagementDatabaseAlias").val("");
								$("#Dev2SourceManagementDatabaseServer").val("");
								$("#Dev2SourceManagementDatabaseUsername").val("");
								$("#Dev2SourceManagementDatabasePassword").val("");
								$("#Dev2SourceManagementDatabaseDataSource").html('');
								$("#Dev2DatabaseSourceCategory").val("");
							}
						});
					},
					error : function(data){
						var errorHandler = new Dev2ErrorHandler(data);
						errorHandler.HandleNon200ErrorPayload();
					}
				});
			});	
			
			/* done button clicked, add resource to application server */
			$("#Dev2SourceManagementDatabaseDoneButton").click(function(){
			
				// Dev2DatabaseSourceCategory
				databaseType = $("#Dev2SourceManagementDatabaseType option:selected").val();
				databaseAlias = stripNaughtyChars($("#Dev2SourceManagementDatabaseAlias").val());
				databaseServer = $("#Dev2SourceManagementDatabaseServer").val();
				databaseName = $("#Dev2SourceManagementDatabaseDataSource option:selected").val();
				if(sourceExists){
					$("#Dev2SourceManagementDatabaseAlias").focus();
					setDoneButtonState(false);
					alertDialog("Source already exists. Please enter a different Alias.");
					return;
				}
				if(databaseAlias.length == 0){
					setDoneButtonState(false);
					alertDialog("Please specify name for source file in the Alias field");
					return;	
				}
				if(databaseServer.length == 0){
					setDoneButtonState(false);
					alertDialog("Please specify name of server the database resides on in the Server field");
					return;	
				}
				if(databaseName == undefined){
					setDoneButtonState(false);
					alertDialog("Please ensure Data Source is populated");
					return;	
				}
				
				var cat = $("#Dev2DatabaseSourceCategory").val();
				
				if($("#specified").is(":checked")){
					if(username.length == 0){
						setDoneButtonState(false);
						alertDialog("Please enter username");
						return false;
					}else{
						if(databaseServer.length > 0){
						}else{
							setDoneButtonState(false);
							alertDialog("Please enter server name");
							return false;
						}						
					}					
					sourceTemplate =  '<Source Name="'+databaseAlias+'" Type="'+databaseType+'" ConnectionString="server='+databaseServer+';database='+databaseName+';integrated security=false;UID='+username+';Password='+password+';">'+
									  '<AuthorRoles>Domain Users,Windows SBS Remote Web Workplace Users,Windows SBS Fax Users,Windows SBS Folder Redirection Accounts,All Users,Windows SBS SharePoint_MembersGroup,Windows SBS Link Users,Company Users,Business Design Studio Developers,Test Engineers,DEV2 Limited Internet Access,</AuthorRoles>'+
									  '<Comment>SQL Database</Comment>'+
									  '<Category>' + cat + '</Category>'+
									  '<HelpLink>http://filler.link</HelpLink>'+
									  '<Tags></Tags>'+
									  '<UnitTestTargetWorkflowService></UnitTestTargetWorkflowService>'+
									  '<BizRule />'+
									  '<WorkflowActivityDef />'+
									  '<XamlDefinition />'+
									  '<DataList />'+
									  '<TypeOf>Database</TypeOf>' +
									  '<DisplayName>Source</DisplayName>'+
									  '</Source>';
				}else{	
					sourceTemplate =  '<Source Name="'+databaseAlias+'" Type="'+databaseType+'" ConnectionString="server='+databaseServer+';database='+databaseName+';integrated security=true;">'+
									  '<AuthorRoles>Domain Users,Windows SBS Remote Web Workplace Users,Windows SBS Fax Users,Windows SBS Folder Redirection Accounts,All Users,Windows SBS SharePoint_MembersGroup,Windows SBS Link Users,Company Users,Business Design Studio Developers,Test Engineers,DEV2 Limited Internet Access,</AuthorRoles>'+
									  '<Comment>SQL Database</Comment>'+
									  '<Category>' + cat +'</Category>'+
									  '<HelpLink>http://filler.link</HelpLink>'+
									  '<Tags></Tags>'+
									  '<UnitTestTargetWorkflowService></UnitTestTargetWorkflowService>'+
									  '<BizRule />'+
									  '<WorkflowActivityDef />'+
									  '<XamlDefinition />'+
									  '<DataList />'+
									  '<TypeOf>Database</TypeOf>' +
									  '<DisplayName>Source</DisplayName>'+
									  '</Source>';
				}
				
				$.ajax({
					type:"GET",
					url:webServer+"/services/AddResourceService",
					data:{"ResourceXml":sourceTemplate,"Roles":"Domain Users,Windows SBS Remote Web Workplace Users,Windows SBS Fax Users,Windows SBS Folder Redirection Accounts,All Users,Windows SBS SharePoint_MembersGroup,Windows SBS Link Users,Company Users,Business Design Studio Developers,Test Engineers,DEV2 Limited Internet Access"},
					dataType:"xml",
					async:false,
					success:function(xml){
						// now move back to the original screen
						
						
						var errorHandler = new Dev2ErrorHandler(xml);
				
						if(!errorHandler.Has200ErrorPayload()){
						
							var srcStatus = "changed";
							
							if(sourceName.toLowerCase() == $("#Dev2SourceManagementDatabaseAlias").val().toLowerCase()){
								srcStatus = "equal";
							}
							
							var resName = stripNaughtyChars($("#Dev2SourceManagementDatabaseAlias").val());
							
							if(Dev2StudioExe != 'yes'){
								SendSourceUpdate(resName);
								bindDev2PluginStep2BackBtn(serviceName,"Database", $("#Dev2SourceManagementDatabaseAlias").val(),category, help, icon, desc, tags, srcStatus, isNew); 
							}else{
							
								//if(srcStatus == "changed"){
									var result = "<Dev2WizardPayload>";
									
									//ResourceName
									result += "<ResourceName>" + resName + "</ResourceName>";
									//ResourceType { WorkflowService, Service { Plugin, Database }, Source, Website, HumanInterfaceProcess }
									result += "<ResourceType>Source</ResourceType>";
									//Category
									result += "<Category>" + cat + "</Category>";
									//Comment
									result += "<Comment>A database source</Comment>";
									//Tags
									result += "<Tags>MS SQL," + stripNaughtyChars(resName) +"</Tags>";
									//IconPath
									result += "<IconPath></IconPath>";
									//HelpLink
									result += "<HelpLink></HelpLink>";
									
									result += "</Dev2WizardPayload>";
									
									// pass data back to the studio to close
									ExitWithStudioPush(result);
									
								//}else{
								//	Dev2Awesomium.Cancel();
								//}
							}	
						}else{
							errorHandler.Handle200ErrorPayload();
						}
					},
					error : function(data){
						var errorHandler = new Dev2ErrorHandler(data);
						errorHandler.HandleNon200ErrorPayload();
					}
				});
			});	
			
			// build the list of databases
			function buildDBList(dbName){
				
				//alert(dbName);
			
				if($("#Dev2SourceManagementDatabaseDataSource option").length == 0){
					databaseServer = $("#Dev2SourceManagementDatabaseServer").val();
					username = $("#Dev2SourceManagementDatabaseUsername").val();
					password = $("#Dev2SourceManagementDatabasePassword").val();
					if($("#specified").is(":checked")){
						if(username.length == 0){
							$("#Dev2SourceManagementDatabaseUsername").focus();
							setDoneButtonState(false);		
							alertDialog("Please enter username");
							return false;
						}					
					}
					
					if(databaseServer.length > 0){
						$.ajax({
							type:"GET",
							url: webServer + "/services/FindSqlDatabasesService",
							data: {"ServerName":databaseServer,"Username":username,"Password":password},
							dataType:"xml",
							success:function(xml){
								$("#Dev2SourceManagementDatabaseDataSource").html('');
								var errorHandler = new Dev2ErrorHandler(xml);
				
								if(!errorHandler.Has200ErrorPayload()){
									setDoneButtonState(true);
									$(xml).find("Table").each(function(){
										var val = $(this).find("DATABASE_NAME").text();
										
										if(val == dbName){
											$("#Dev2SourceManagementDatabaseDataSource").append("<option selected>"+val+"</option>");
										}else{
											$("#Dev2SourceManagementDatabaseDataSource").append("<option>"+val+"</option>");
										}
									});
								}else{
									setDoneButtonState(false);
									$("#Dev2SourceManagementDatabaseUsername").focus();	
									errorHandler.Handle200ErrorPayload();
									return false;
								}
							},
							error : function(data){
								var errorHandler = new Dev2ErrorHandler(data);
								errorHandler.HandleNon200ErrorPayload();
							}
						});
					}else{
						alertDialog("Please enter server name");
					}
				}
			}
			
			/* populate data source drop down list when it has focus */
			$("#Dev2SourceManagementDatabaseDataSource").focus(function(){
				buildDBList();
			});	
			
			/* when alias texbox has lost focus, check value entered against available sources */
			$("#Dev2SourceManagementDatabaseAlias").focusout(function(e){
				sourceExists=false;
				
				var currentVal = stripNaughtyChars($("#Dev2SourceManagementDatabaseAlias").val());
				
				$("#Dev2SourceManagementAlias").val(currentVal);
				
				if($("#Dev2SourceManagementDatabaseSource option:selected").val()=="New...."){
					
					var serviceData = fetchMapping(webServer, currentVal, "*");
					
					// Name="travwebpage"
					if(serviceData.toLowerCase().indexOf('name="'+currentVal.toLowerCase()+'"') > 0 && currentVal.length > 0){
						// we have an issue, 
						alert("Please select a different name, <i>" + currentVal + "</i> is already in use.", "Service Naming Error");
						//$("#Dev2ServiceName").focus();
						setDoneButtonState(false);
						//$("#Dev2SourceManagementDoneButton").css("display", "none");
					}
				}else{
					//nameOk = true;
					//if(isWorkerServiceType(editServiceType)){
						//$("#Dev2SourceManagementDoneButton").css("display", "inline");
						setDoneButtonState(true);
					//}
				}
				
			});
			
			// Server=SAMEERC;Database=CollectionsProfilingEngine;Integrated Security=True
			// server;Database=;Integrated Security=False;user=olvUser;pwd=password;
			// build the data populated for all to use ;)
			
			if(magicString != ""){
				
				var magicStringUtil = new Dev2DBConStringUtil(magicString);
				
				if(magicStringUtil.IsDBAuth()){
					// set user and pass
					$("#Dev2SourceManagementDatabaseUsername").val(magicStringUtil.FetchUser());
					$("#Dev2SourceManagementDatabasePassword").val(magicStringUtil.FetchPassword());
					$("#windows").attr("checked", "false");
					$("#specified").attr("checked", "true");
				}
					
				// set the DB server
				$("#Dev2SourceManagementDatabaseServer").val(magicStringUtil.FetchServer());
				buildDBList(magicStringUtil.FetchDB());
			}
			
		}catch(e){
			//alertDialog(e);
		}	
	});
}
