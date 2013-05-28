<script>
/* PluginSourceManagement wizard javascript   */
	$(document).ready(function(){
		try{
			/* region FIELDS */
			var path="";
			var domain="";
			var username="";
			var password="";
			var plugins=[];
			var serverPath="";
			var serverUsername ="";
			var isValid=false;
			var errorMessage="";
			/* endregion FIELDS */

			/* region AJAX SERVICE CALLS */
			/* get the netbios name of the computer the application server is running on */
			/* show animation when ajax event has fired  */
			$("#Dev2SourceManagementLoadingDialog").dialog({
				autoOpen: false,
				bgiframe:true,
				modal:true,
				width:96,
				height:96,
				resizable:false
			}).bind("ajaxStart",function(){
				$(this).dialog("open");
			}).bind("ajaxStop", function(){
				$(this).dialog("close");
			});
			$.ajax({
				url:"FindMachineNameService",
				dataType:"xml",
				async:false,
				success: function(xml){
					$("#Dev2SourceManagementMachineName").text("Path from " + $(xml).find("Dev2MachineName").text()+":");
				}
			});
			/* populate "source" drop down list */
			$.ajax({
				type:"GET",
				url:"FindResourcesService",
				data:{"ResouceName":"*","ResourceType":"Source","Roles":"Business Design Studio Developers"},
				dataType:"xml",
				async:false,
				success:function(xml){
					$("#Dev2SourceManagementSource").html('');
					$("#Dev2SourceManagementSource").append("<option>New....</option>");						
					$(xml).find("Dev2Resource").each(function(){
						var type = $(this).find("Dev2SourceType").text();
						if(type.indexOf("Plugin") != -1){
							$("#Dev2SourceManagementSource").append("<option>"+$(this).find("Dev2SourceName").text()+"</option>");
						}	
					});
				}
			});
			/* populate populate GAC listbox */
			$.ajax({
				type:"GET",
				url:"RegisteredAssemblyService",
				dataType:"json",
				async:false,
				success:function(data){
					$("#Dev2SourceManagementRegisteredName").html('');
					$.each(data, function(index, element){
						plugins.push(element.AssemblyName);
					});
					$(plugins).each(function(index, element){
						$("#Dev2SourceManagementRegisteredName").append("<option>"+element+"</option>");
					});
				}
			});			
			/* get username of user logged in to the server  */
			$.ajax({
				type:"GET",
				url:"FindServerUsernameService",
				dataType:"xml",
				async:false,
				success:function(xml){
					serverUsername = $(xml).find("Result").text();
				}
			});			
			/* endregion AJAX SERVICE CALLS */
			
			/* region SET PROPERTIES */
			/* hide dialog contents before it's shown */
			$("#Dev2SourceManagementServerDialog").hide();
			$("#Dev2SourceManagementPasswordPrompt").hide();
			/* if dialog is open, bind tabs to the dialog */
			$("#Dev2SourceManagementServerDialog").bind('dialogopen',function(){
					$("#Dev2SourceManagementServerDialog").tabs();
			});
			/* if dialog is closing, destroy tabs */
			$("#Dev2SourceManagementServerDialog").bind('dialogclose',function(){
					$("#Dev2SourceManagementServerDialog").tabs('destroy');
			});
			/* build application server path recursively */
			/* endregion SET PROPERTIES */
			
			/* region HELPER FUNCTIONS */
			function getPath(node){
				var pNode = node.parent;
				if(path==""){
					path=node.data.title;
				}
				if(pNode.data.title!=null){
					path=pNode.data.title+"/"+path;
					getPath(pNode);
				}
			}
			/* get the credentials entered in the username and password textboxes */
			function setCredentials(){
				username = $("#Dev2SourceManagementUsername").val();
				password = $("#Dev2SourceManagementPassword").val();
				if(username.indexOf("\\") == -1){
					if(username.length > 0){
						domain=".";
					}
				}else{
					domain=username.split("\\")[0];
					username=username.split("\\")[1];
				}			
				if((/^\s*$/).test(username)){
					isValid=false;
				}
			}
			/* test username and password entered test */
			function checkCredentials(){				
				$.ajax({
					url:"CheckCredentialsService",
					data: {"Domain":domain,"Username":username,"Password":password},
					dataType:"xml",
					async:false,
					success: function(xml){
						errorMessage = $(xml).find("Result").text();
						if(errorMessage.indexOf("failed") != -1){
							isValid = false;
						}else{
							isValid = true;
						}
					}
				});
			}
			/* open server dialog */
			function serverDialog(){
				/* If the value Dev2SourceManagementServer textbox is a path, set the value of Dev2SourceManagementPath to it */
				if($("#Dev2SourceManagementResourceType").text().indexOf("GAC:") == -1){
					$("#Dev2SourceManagementPath").val($("#Dev2SourceManagementFullPath").val().replace("\\",""));
				}else{
					$("#Dev2SourceManagementPath").val("");
				}
				if($("#Dev2SourceManagementRegisteredName option").length == 0){
					/* populate populate GAC listbox */
					$.ajax({
						type:"GET",
						url:"RegisteredAssemblyService",
						dataType:"json",
						async:false,
						success:function(data){
							$("#Dev2SourceManagementRegisteredName").html('');
							plugins=[];
							$.each(data, function(index, element){
								plugins.push(element.AssemblyName);
							});
							$(plugins).each(function(index, element){
								$("#Dev2SourceManagementRegisteredName").append("<option>"+element+"</option>");
							});
						}
					});			
				}
				/* set the value of GAC filter textbox to what is selected in the GAC listbox */ 
				$("#Dev2SourceManagementFilter").val($("#Dev2SourceManagementRegisteredName option:selected").text());
				/* build application server file system tree */
				$("#Dev2SourceManagementServerTree").dynatree({
					autoCollapse:false,
					checkbox: false,
					selectMode: 1,
					keyPathSeparator: "/",
					initAjax:{
						url:"FindDriveService",
						data: {"Domain":domain,"Username":username,"Password":password}
					},
					onPostInit: function(isReloading, isError) {
						this.reactivate();
						if($("#Dev2SourceManagementFullPath").val().length > 0){
							var tree = $("#Dev2SourceManagementServerTree").dynatree("getTree");
							tree.loadKeyPath($("#Dev2SourceManagementFullPath").val().replace(/ /g, "_").replace("(", "40").replace(")", "41"),function(node,status){
								if(status == "loaded"){
									node.expand(true);
								}else if(status == "ok"){
									node.activate();
								}else{
									alertDialog(status);
								}
							});
						}
					},
					onLazyRead: function(node){
						path="";
						getPath(node);
						node.appendAjax({
							url:"FindDirectoryService",
							data: {"DirectoryPath":path,"Domain":domain,"Username":username,"Password":password}
						});
						node.setLazyNodeStatus(DTNodeStatus_Ok);
					},
					onActivate: function(node){
						path="";
						getPath(node);
						$.ajax({
							url:"CheckPermissionsService",
							data: {"Path":path},
							success:function(xml){
								errorMessage = $(xml).find("Error").text();
								if(errorMessage.length>0){
									alertDialog(errorMessage);
									$("#Dev2SourceManagementPath").val("");
								}else{
									$("#Dev2SourceManagementPath").val(path.replace("\\",""));
								}
							}
						});
						serverPath=path;
					}
				});
				/* build application server/GAC dialog */
				$("#Dev2SourceManagementServerDialog").dialog({
					autoOpen: false,
					bgiframe:true,
					modal:true,
					width:480,
					height:'auto',
					resizable:false,
					buttons:
					[
						{text:"Done",
							click : function(){
								if(serverPath.indexOf("GAC") != -1){
									$("#Dev2SourceManagementResourceType").innerHTML("<div style='vertical-align:middle;' >GAC</div>");
									$("#Dev2SourceManagementServer").val(serverPath.substring(serverPath.indexOf("GAC:")+("GAC:").length));
									$("#Dev2SourceManagementFullPath").val("");
								}else{
									$("#Dev2SourceManagementResourceType").innerHTML("<div style='vertical-align:middle;' >Path</div>");
									$("#Dev2SourceManagementFullPath").val(serverPath);
									$("#Dev2SourceManagementServer").val(serverPath.substring(serverPath.lastIndexOf("/")+1));
								}
								$(this).dialog("close"); 
							}
						},
						{text:"Cancel",
							click : function(){ 
								$(this).dialog("close"); 
							}
						}
					]
				});
				/* hide the dialog title bar */
				$(".ui-dialog-titlebar").hide();
				/* open dialog */
				$("#Dev2SourceManagementServerDialog").dialog('open');
			}
			function alertDialog(message,callback){
				$("#Dev2SourceManagementAlert").html(message);
				/* password prompt dialog */
				$("#Dev2SourceManagementAlertDialog").dialog({
					autoOpen: false,
					bgiframe:true,
					modal:true,
					width:'auto',
					height:'auto',
					resizable:false,
					buttons:
					[
						{text:"OK",
							click : function(){
								$(this).dialog("close"); 
								callback();
							}
						}
					]
				});
				/* hide the dialog title bar */
				$(".ui-dialog-titlebar").hide();
				/* open dialog */
				$("#Dev2SourceManagementAlertDialog").dialog('open');
			}
			function passwordDialog(callback){				
				/* password prompt dialog */
				$("#Dev2SourceManagementPasswordPrompt").dialog({
					autoOpen: false,
					bgiframe:true,
					modal:true,
					width:280,
					height:'auto',
					resizable:false,
					buttons:
					[
						{text:"Done",
							click : function(){
								password=$("#Dev2SourceManagementPasswordPromptValue").val();
								checkCredentials();
								$(this).dialog("close"); 
								callback();
							}
						},
						{text:"Cancel",
							click : function(){ 
								$(this).dialog("close"); 
							}
						}
					]
				});
				/* hide the dialog title bar */
				$(".ui-dialog-titlebar").hide();
				/* open dialog */
				$("#Dev2SourceManagementPasswordPrompt").dialog('open');
			}
			/* endregion HELPER FUNCTIONS */
			
			/* region EVENTS FIRED */
			/* Test connection button clicked */
			$("#Dev2SourceManagementTestConnection").click(function(){
				setCredentials();
				if(username.length > 0){
					checkCredentials();
					if(!isValid){
						alertDialog(errorMessage);
						return false;
					}else{
						alertDialog(errorMessage);					
					}
				}else{
					alertDialog("Please enter a username");
					return false;					
				}
			});
			/* Close button clicked */
			$("#Dev2SourceManagementCloseButton").click(function(){
				window.close();
			});
			/* Set the value of Dev2SourceManagementFilter textbox to the Dev2SourceManagementRegisteredName listbox selected item value */
			$("#Dev2SourceManagementRegisteredName").click(function(){
				$("#Dev2SourceManagementFilter").val($("#Dev2SourceManagementRegisteredName option:selected").text());
				serverPath="GAC:"+$("#Dev2SourceManagementRegisteredName option:selected").text();
			});
			/* Browser server button is clicked */
			$("#Dev2SourceManagementBrowseServer").click(function(){
				$("#Dev2SourceManagementServerDialog").dialog('destroy');
				$("#Dev2SourceManagementServerTree").dynatree('destroy');
				$("#Dev2SourceManagementPasswordPromptValue").val('');
				setCredentials();
				if(username.length > 0){
					checkCredentials();
					if(!isValid){
						alertDialog(errorMessage);
						return false;
					}else{
						serverDialog();
					}
				}else /*if(isValid)*/{
					serverDialog();
				}/*else{
					domain=serverUsername.split("\\")[0];
					username=serverUsername.split("\\")[1];
					if(password.length > 0){
						serverDialog();
					}else{
						passwordDialog(function(){
							if(!isValid){
								alertDialog(errorMessage);
								return false;
							}else{
								serverDialog();
							}							
						});
					}
				}
				serverDialog();*/
			});
			/* done button clicked, update/add resource to application server */
			$("#Dev2SourceManagementDoneButton").click(function(){
				var registeredName = $("#Dev2SourceManagementRegisteredName option:selected").val();
				var path = $("#Dev2SourceManagementServer").val();
				if(path.length==0){
					alertDialog("Please select the resource from server");
					return false;
				}
				var alias = $("#Dev2SourceManagementAlias").val();
				if(alias.length==0){
					alertDialog("Please enter Alias");
					return false;
				}
				var sourceTemplate =  '<Source Name="'+alias+'" Type="Plugin" AssemblyName="Dummy" AssemblyLocation="'+path+'">'+
									  '<AuthorRoles>Domain Users,Windows SBS Remote Web Workplace Users,Windows SBS Fax Users,Windows SBS Folder Redirection Accounts,All Users,Windows SBS SharePoint_MembersGroup,Windows SBS Link Users,Company Users,Business Design Studio Developers,Test Engineers,DEV2 Limited Internet Access,</AuthorRoles>'+
									  '<Comment>Plugin</Comment>'+
									  '<Category>Utility</Category>'+
									  '<HelpLink>http://s</HelpLink>'+
									  '<Tags></Tags>'+
									  '<UnitTestTargetWorkflowService></UnitTestTargetWorkflowService>'+
									  '<BizRule />'+
									  '<WorkflowActivityDef />'+
									  '<XamlDefinition />'+
									  '<DataList />'+
									  '<DisplayName>Source</DisplayName>'+
									  '</Source>';
				$.ajax({
					type:"GET",
					url:"AddResourceService",
					data:{"ResourceXml":sourceTemplate,"Roles":"Domain Users,Windows SBS Remote Web Workplace Users,Windows SBS Fax Users,Windows SBS Folder Redirection Accounts,All Users,Windows SBS SharePoint_MembersGroup,Windows SBS Link Users,Company Users,Business Design Studio Developers,Test Engineers,DEV2 Limited Internet Access"},
					dataType:"xml",
					success:function(xml){
						alertDialog($(xml).text(),function(){
							window.close();
						});
					}
				});
			});	
			/* if source selected items changes hide, show items */
			$("#Dev2SourceManagementSource").change(function(){
				if($("#Dev2SourceManagementSource option:selected").val()=="New...."){
					$("#Dev2SourceManagementRegisteredName").show();
					$("#Dev2SourceManagementRegisteredNameLabel").show();
					$("#Dev2SourceManagementServer").show();
					$("#Dev2SourceManagementResourceType").show();					
					$("#Dev2SourceManagementMachineName").show();
					$("#Dev2SourceManagementBrowseServer").show();
					$("#Dev2SourceManagementResourceType").text("");
					$("#Dev2SourceManagementFullPath").val("");
				}else{
					$("#Dev2SourceManagementRegisteredName").hide();
					$("#Dev2SourceManagementRegisteredNameLabel").hide();
					$("#Dev2SourceManagementServer").hide();
					$("#Dev2SourceManagementResourceType").hide();					
					$("#Dev2SourceManagementMachineName").hide();
					$("#Dev2SourceManagementBrowseServer").hide();
				}					
			});
			/* populate web form based on selected source */
			$("#Dev2SourceManagementSource").change(function(){
				$.ajax({
					type:"GET",
					url:"FindResourcesService",
					data:{"ResouceName":"*","ResourceType":"Source","Roles":"Business Design Studio Developers"},
					dataType:"xml",
					success:function(xml){
						password="";
						var sourceName = $("#Dev2SourceManagementSource option:selected").val();						
						$("#Dev2SourceManagementRegisteredName").html('');
						$(xml).find("Dev2Resource").each(function(){
							var type = $(this).find("Dev2SourceType").text();
							if($(this).find("Dev2SourceName").text()==sourceName){
								var contents = $(this).find("Dev2SourceContents");								
								$("#Dev2SourceManagementRegisteredName").append("<option>"+contents.find("Source").attr("AssemblyName")+"</option>");
								$("#Dev2SourceManagementAlias").val(contents.find("Source").attr("Name"));
								$("#Dev2SourceManagementServer").val(contents.find("Source").attr("AssemblyLocation"));
								return false;
							}else{
								$("#Dev2SourceManagementAlias").val("");
								$("#Dev2SourceManagementServer").val("");
								$("#Dev2SourceManagementUsername").val("");
								$("#Dev2SourceManagementPassword").val("");					
							}
						});
					}
				});
			});	
			/* GAC filter function */
			$("#Dev2SourceManagementFilter").keyup(function(){ 		
				$("#Dev2SourceManagementRegisteredName").html(''); 
				var value = $("#Dev2SourceManagementFilter").val().toUpperCase(); 
				var opt="";
				for(var i=0; i < plugins.length; i++){
					var element = plugins[i].toUpperCase();
					if(element.indexOf(value) >= 0){
						opt += "<option>"+plugins[i]+"</option>";
					}
				}
				$("#Dev2SourceManagementRegisteredName").html(opt); 				
			});
			/* endregion EVENTS FIRED */
		}catch(e){
			alertDialog(e);
		}
	});
</script>