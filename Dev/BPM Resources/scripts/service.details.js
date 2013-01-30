<script>	
	/* NewServiceDetails javascript */ 
	$(document).ready(function(){			
		try{
			/* region FIELDS */
			var categories=[];
			var databaseTypes=[];
			var serviceName = "";
			var serviceType = "";
			var serviceSource = "";
			var serviceSourceMethod ="";
			var serviceCategory = "";
			var serviceHelp = "";
			var serviceIcon = "";
			var serviceComment = "";
			var serviceTags = "";
			var serviceTooltipText = "";
			var webServer = '[[Dev2WebServer]]';	
			/* endregion FIELDS */
			
			/* region AJAX SERVICE CALLS */
			/* populate source drop down list */
			$.ajax({
				type:"GET",
				url:webServer+"/services/FindResourcesService",
				data:{"ResouceName":"*","ResourceType":"Source","Roles":"Business Design Studio Developers"},
				dataType:"xml",
				success:function(xml){
					$("#Dev2ServiceDetailsSource").html('');
					$(xml).find("Dev2Resource").each(function(){
						var type = $(this).find("Dev2SourceType").text();
						if(type.indexOf("Database") != -1){
							$("#Dev2ServiceDetailsSource").append("<option>"+$(this).find("Dev2SourceName").text()+"</option>");
							databaseTypes["'"+$(this).find("Dev2SourceName").text()+"'"]=$(this).find("Dev2SourceContents").find("Source").attr("Type");
						}
					});
				}
			});
			/* populate type of work drop down list */
			$.ajax({
				type:"GET",
				url:webServer+"/services/WorkerServiceWorkType",
				dataType:"xml",
				success:function(xml){
					var services = $.parseJSON($(xml).find("Dev2WorkerServiceWorkType").text());
					var data = services.WorkType.split(",");		
					$("#Dev2ServiceDetailsWorkType").html('');
					$(data).each(function(index,element){			
						$("#Dev2ServiceDetailsWorkType").append("<option>"+element+"</option>");		
					});	
				}
			});
			/* compile a list of available worker service categories */
			$.ajax({
				type:"GET",
				url:webServer+"/services/FindResourcesService",
				data:{"ResouceName":"*","ResourceType":"Service","Roles":"Business Design Studio Developers"},
				dataType:"xml",
				async:false,
				success:function(xml){
					var data=[];
					$(xml).find("Dev2Resource").each(function(){
						data.push($(this).find("Dev2WorkerServiceCategory").text());
					});
					$(data).each(function(index,element){
						var categoriesLength = categories.length;
						if(index==0){
							categories.push(element);
						}else{
							for (var i=0;i < categoriesLength;i++) {
								if(categories[i]==element){
									break;
								}
								if(i == categoriesLength-1){
									categories.push(element);
									break;
								}
							}
						}
					});
					categories.pop(); /*remove blank element at end of array*/
				}
			});
			/* endregion AJAX SERVICE CALLS */
			
			/* region SET PROPERTIES */
			/* set autocomplete on the "Cateory" textbox */
			/* when category textbox has focus fire autocomplete feature */
			$( "#Dev2ServiceDetailsCategory" ).autocomplete({
				source: categories,
				minLength: 0,
				delay: 0
			});
			/* hide done buttons */
			$("#Dev2ServiceDetailsDoneButton").hide();
			/* hide dialog contents before it's shown */
			$("#Dev2ServiceDetailsServerDialog").hide();
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
			/* open server dialog */
			function serverDialog(){
				/* build application server file system tree */
				$("#Dev2ServiceDetailsServerTree").dynatree({
					autoCollapse:false,
					checkbox: false,
					selectMode: 1,
					keyPathSeparator: "/",
					initAjax:{
						url:webServer+"/services/FindDriveService"
					},
					onPostInit: function(isReloading, isError) {
						this.reactivate();
						if($("#Dev2ServiceDetailsIcon").val().length > 0){
							var tree = $("#Dev2ServiceDetailsServerTree").dynatree("getTree");
							tree.loadKeyPath($("#Dev2ServiceDetailsIcon").val().replace(/ /g, "_").replace("(", "40").replace(")", "41"),function(node,status){
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
							url:webServer+"/services/FindDirectoryService",
							data: {"DirectoryPath":path}
						});
						node.setLazyNodeStatus(DTNodeStatus_Ok);
					},
					onActivate: function(node){
						path="";
						getPath(node);
						$.ajax({
							url:webServer+"/services/CheckPermissionsService",
							data: {"Path":path},
							success:function(xml){
								errorMessage = $(xml).find("Error").text();
								if(errorMessage.length>0){
									alertDialog(errorMessage);
									$("#Dev2SourceManagementPath").val("");
								}
							}
						});
						serverPath=path.replace("\\","");
					}
				});
				/* build application server dialog */
				$("#Dev2ServiceDetailsServerDialog").dialog({
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
								$("#Dev2ServiceDetailsIcon").val(serverPath);
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
				$("#Dev2ServiceDetailsServerDialog").dialog('open');
			}
			/* endregion HELPER FUNCTIONS */
			
			/* region EVENTS FIRED */
			/* Browser server button is clicked */
			$("#Dev2ServiceDetailsSelectIconButton").click(function(){
				$("#Dev2ServiceDetailsServerDialog").dialog('destroy');
				$("#Dev2ServiceDetailsServerTree").dynatree('destroy');
				serverDialog();
			});
			$( "#Dev2ServiceDetailsCategory" ).focus(function(){     
				$(this).data("autocomplete").search($(this).val());
			});
			/* filter source by type of work */
			$('#Dev2ServiceDetailsWorkType').change(function(){
				var work = $('#Dev2ServiceDetailsWorkType option:selected').val();
				$.ajax({
					type:"GET",
					url:webServer+"/services/FindResourcesService",
					data:{"ResouceName":"*","ResourceType":"Source","Roles":"Business Design Studio Developers"},
					dataType:"xml",
					success:function(xml){
						$("#Dev2ServiceDetailsSource").html('');
						$(xml).find("Dev2Resource").each(function(){
							var type = $(this).find("Dev2SourceType").text();
							if(type.indexOf(work) != -1){
								$("#Dev2ServiceDetailsSource").append("<option>"+$(this).find("Dev2SourceName").text()+"</option>");
							}
						});
					}
				});			
			});	
			/* next button clicked, populate worker service template */
			$("#Dev2ServiceDetailsNext").submit(function(){			    
			});
			/* endregion EVENTS FIRED */
		}catch(e){}	
	});
</script>