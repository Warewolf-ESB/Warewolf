<script>	
	/* ServiceInputOutputDescription Javascript */ 
	$(document).ready(function(){			
		try{
			/* region FIELDS */
			var inputs = '[[Dev2ServiceSetupInputs]]';
			var outputs = '[[Dev2ServiceSetupOutputs]]';
			var serviceName = "[[Dev2ServiceDetailsName]]";
			var serviceType = "[[Dev2ServiceDetailsWorkType]]";
			if(serviceType=="Database"){
				serviceType="InvokeStoredProc";
			}
			var serviceSource = "[[Dev2ServiceDetailsSource]]";
			var serviceSourceMethod ="[[Dev2ServiceSetupSourceMethod]]";
			var serviceCategory = "[[Dev2ServiceDetailsCategory]]";
			var serviceHelp = "[[Dev2ServiceDetailsHelp]]";
			var serviceIcon = "[[Dev2ServiceDetailsIcon]]";
			var serviceComment = "[[Dev2ServiceDetailsDescription]]";
			var serviceTags = "[[Dev2ServiceDetailsTags]]";
			var serviceTooltipText = "[[Dev2ServiceDetailsTooltipText]]";
			var serviceOutputs="";
			var serviceInputs="";
			/* endregion FIELDS */
			
			/* region AJAX SERVICE CALLS */
			/* endregion AJAX SERVICE CALLS */
			
			/* region SET PROPERTIES */
			/* endregion SET PROPERTIES */
			
			/* region HELPER FUNCTIONS */
			/* return child nodes of element*/
			function getChildNodes(element){
				var xmlDoc;
				if(window.DOMParser){
					var parser = new DOMParser();
					xmlDoc=parser.parseFromString(element,"text/xml");
				}else{
					xmlDoc=new ActiveXObject("Microsoft.XMLDOM");
					xmlDoc.async=false;
					xmlDoc.loadXML(element);
				}
				return xmlDoc.documentElement.childNodes;				
			}
			/* Display input parameters, default value text box and required checkboxes */
			function displayInputs(){
				var nodes = getChildNodes(inputs);
				var numNodes = nodes.length;
				$('#Dev2ServiceSetupInputs').val('[[Dev2ServiceSetupInputs]]');
				$('#Dev2ServiceSetupInputParameters tr').not(':first').not(':last').remove();
				var html = '';
				for(var i = 0; i < numNodes; i++){
					var child = nodes[i].nodeName;
					html += '<tr><td>'+child+'</td><td>'+'<input type="text" id="'+child+'Textbox" name="'+child+'Textbox" />'+'</td><td><input type="checkbox" id="'+child+'Checkbox" /></td></tr>';
				}
				$('#Dev2ServiceSetupInputParameters tr').last().after(html);					
			}
			/* display output parameters */
			function displayOutputs(){
				var nodes = getChildNodes(outputs);
				var numNodes = nodes.length;
				$('#Dev2ServiceSetupOutputs').val('[[Dev2ServiceSetupOutputs]]');
				$('#Dev2ServiceSetupOutputParameters tr').not(':first').not(':last').remove();
				var html = '';
				for(var i = 0; i < numNodes; i++){			
					var child = nodes[i];
					if(child.childNodes.length > 0){
						for(var j=0;j<child.childNodes.length;j++){
							html += '<tr><td>'+child.nodeName+"()."+child.childNodes[j].nodeName+'</td></tr>';
						}	
					}else{
							html += '<tr><td>'+child.nodeName+'</td></tr>';
					}
				}
				html += '<tr><td><input type="checkbox" id="Dev2ServiceInputOutputDescriptionForceRecordset">Force results to Recordset?</input></td></tr>';
				$('#Dev2ServiceSetupOutputParameters tr').last().after(html);	
			}
			displayInputs();
			displayOutputs();
			/* endregion HELPER FUNCTIONS */
			
			/* region EVENTS FIRED */
			/* Done button clicked, add/update resource on application server */
			$("#Dev2ServiceInputOutputDescriptionDoneButton").click(function(){
				/* Build Service Outputs */
				if($('#Dev2ServiceInputOutputDescriptionForceRecordset').is(":checked")){
					var nodes = getChildNodes(outputs);
					var numNodes = nodes.length;
					var recordsetName = serviceName.replace(/\s/g,"");
					var outputsForceRecordset = "<Dev2ServiceSetupOutputs>";
					for(var i = 0; i < numNodes; i++){			
						var child = nodes[i];
						/* if child has no children, then add child to the recordset identified by recordsetName */
						if($(child).children().length == 0){
							if(i==0){
								outputsForceRecordset += "<"+recordsetName+">";
							}
							outputsForceRecordset += "<"+child.nodeName+" />";
							if(i==numNodes-1){
								outputsForceRecordset += "</"+recordsetName+">";
							}
						}else{
							if(i==0){
								outputsForceRecordset += "<"+child.nodeName+">";
							}
							for(var k=0;k<$(child).children().length;k++){
								outputsForceRecordset += "<"+child.childNodes[k].nodeName+" />";
							}
							if(i==numNodes-1){
								outputsForceRecordset += "</"+child.nodeName+">";
							}								
						}
					}
					outputsForceRecordset += "</Dev2ServiceSetupOutputs>";
					nodes = getChildNodes(outputsForceRecordset);
					numNodes = nodes.length;
					for(var j=0;j<numNodes;j++){
						var kid = nodes[j];
						var len_k = kid.childNodes.length;
						var left_sq = "[[";
						var right_sq = "]]";
						/*if kid is a scalar*/
						if(len_k==0){
							serviceOutputs += '<Output Name="'+kid.nodeName+'" MapsTo="'+kid.nodeName+'" Value="'+left_sq+kid.nodeName+right_sq+'" />';
						}else{
							/*if kid is a recordset*/
							for(var k=0;k<len_k;k++){
								serviceOutputs += '<Output Name="'+kid.childNodes[k].nodeName+'" MapsTo="'+kid.childNodes[k].nodeName+'" Value="'+left_sq+kid.nodeName+"()."+kid.childNodes[k].nodeName+right_sq+'" Recordset="'+kid.nodeName+'" />';						
							}
						}
					}
					serviceOutputs = "<Outputs>"+serviceOutputs+"</Outputs>";
				}else{
					var nodes = getChildNodes(outputs);
					var numNodes = nodes.length;
					for(var j=0;j<numNodes;j++){
						var kid = nodes[j];
						var len_k = kid.childNodes.length;
						var left_sq = "[[";
						var right_sq = "]]";
						/*if kid is a scalar*/
						if(len_k==0){
							serviceOutputs += '<Output Name="'+kid.nodeName+'" MapsTo="'+kid.nodeName+'" Value="'+left_sq+kid.nodeName+right_sq+'" />';
						}else{
							/*if kid is a recordset*/
							for(var k=0;k<len_k;k++){
								serviceOutputs += '<Output Name="'+kid.childNodes[k].nodeName+'" MapsTo="'+kid.childNodes[k].nodeName+'" Value="'+left_sq+kid.nodeName+"()."+kid.childNodes[k].nodeName+right_sq+'" Recordset="'+kid.nodeName+'" />';						
							}
						}
					}
					serviceOutputs = "<Outputs>"+serviceOutputs+"</Outputs>";
				}
				/* Build Service Inputs */
				var nodes = getChildNodes(inputs);
				var numNodes = nodes.length;
				for(var j=0;j<numNodes;j++){
					var kid = nodes[j];				
					var len_k = kid.childNodes.length;
					/* If kid does not have child nodes */
					if(len_k == 0){
						serviceInputs += '<Input Name="'+kid.nodeName+'" Source="'+kid.nodeName+'"';
						/* If the default value has been entered in the defaultValue field, add it to the input definition */
						if($('#'+kid.nodeName+'Textbox').val().length > 0){
							serviceInputs += ' DefaultValue="'+$('#'+kid.nodeName+'Textbox').val()+'"';
						}
						serviceInputs += '>';
						/* If required checkbox has been ticked, add it to the input definition */
						if($('#'+kid.nodeName+'Checkbox').is(":checked")){
							serviceInputs += '<Validator Type="Required" />';
						}
						serviceInputs += '</Input>';
					}else{
						for(var k=0;k<len_k;k++){
							serviceInputs += '<Input Name="'+kid.childNodes[k].nodeName+'" Source="'+kid.childNodes[k].nodeName+'"';
							/* If the default value has been entered in the defaultValue field, add it to the input definition */
							if($('#'+kid.childNodes[k].nodeName+'Textbox').val().length > 0){
								serviceInputs += ' DefaultValue="'+$('#'+kid.childNodes[k].nodeName+'Textbox').val()+'"';
							}
							serviceInputs += '>';
							/* If required checkbox has been ticked, add it to the input definition */
							if($('#'+kid.childNodes[k].nodeName+'Checkbox').is(":checked")){
								serviceInputs += '<Validator Type="Required" />';
							}
							serviceInputs += '</Input>';
						}
					}
				}
				serviceInputs = "<Inputs>"+serviceInputs+"</Inputs>";
				/* Build service definition template */
				var serviceTemplate =  	'<![CDATA[<Service Name="'+serviceName+'">'+
										'<Actions>'+
										'<Action Name="'+serviceName+'" Type="'+serviceType+'" SourceName="'+serviceSource+'" SourceMethod="'+serviceSourceMethod+'">'+
										serviceInputs+serviceOutputs+
										'</Action>'+
										'</Actions>'+
										'<AuthorRoles>Schema Admins,Enterprise Admins,Domain Admins,Domain Users,Windows SBS Remote Web Workplace Users,Windows SBS Fax Users,Windows SBS Fax Administrators,Windows SBS Virtual Private Network Users,All Users,Windows SBS Administrators,Windows SBS SharePoint_OwnersGroup,Windows SBS Link Users,Windows SBS Admin Tools Group,Company Users,Business Design Studio Developers,</AuthorRoles>'+
										'<Comment>'+serviceComment+'</Comment>'+
										'<Category>'+serviceCategory+'</Category>'+
										'<HelpLink>'+serviceHelp+'</HelpLink>'+
										'<Tags>'+serviceTags+'</Tags>'+
										'<UnitTestTargetWorkflowService></UnitTestTargetWorkflowService>'+
										'<BizRule />'+
										'<WorkflowActivityDef />'+
										'<Source />'+
										'<XamlDefinition />'+
										'</Service>]]>';
				/* Call AddResourceService to add/update service */
				$.ajax({
					type:"GET",
					url:"AddResourceService",
					data:{"ResourceXml":serviceTemplate,"Roles":"Domain Users,Windows SBS Remote Web Workplace Users,Windows SBS Fax Users,Windows SBS Folder Redirection Accounts,All Users,Windows SBS SharePoint_MembersGroup,Windows SBS Link Users,Company Users,Business Design Studio Developers,Test Engineers,DEV2 Limited Internet Access"},
					dataType:"xml",
					success:function(xml){
						alert($(xml).text());
					}
				});
			});
			/* endregion EVENTS FIRED */
		}catch(e){
			alert(e);
		}	
	});
</script>