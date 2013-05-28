/* Array Remove*/
// Array Remove - By John Resig (MIT Licensed)
Array.prototype.remove = function(from, to) {
  var rest = this.slice((to || from) + 1 || this.length);
  this.length = from < 0 ? this.length + from : from;
  return this.push.apply(this, rest);
};


/* General Error Handlers */
(function(){

	var _payload = "";

	this.Dev2ErrorHandler = function(payload){
		
		_payload = "";
		_payload = payload;
		
	},
	
	Dev2ErrorHandler.prototype.Has200ErrorPayload = function(){
		var errMsg = $(_payload).find("Error").text();
		var result = false;
		if(errMsg != ""){
			result = true;
		}
		return result;
	},
	
	Dev2ErrorHandler.prototype.HandleNon200ErrorPayload = function(){
		// first check for JSON data
		var toDisplay = _payload;
		try{
			toDisplay = JSON.parse(payload);
		}catch(e){
			// now try XML
			try{
				toDisplay = $(payload).text();
			}catch(e1){}
		}
		
		// object Object is what happens when we have bad JSON or XML data ;)
		if(toDisplay != "" && (toDisplay+"") != "[object Object]"){
			alert(toDisplay,"Error");
		}else{
			alert("An unknown error occured, please try again", "Error");
		}
	},
	
	Dev2ErrorHandler.prototype.Handle200ErrorPayload = function(){
		var errMsg = $(_payload).find("Error").text();
		if(errMsg != ""){
			alert(errMsg);
		}else{
			alert("An unknown error has occured");
		}
	}

})();

// alert over-ride instead of nasty pop-up dialog ;)
window.alert = function(message, myTitle){
		if(myTitle == undefined){
			myTitle = "Alert";
		}
		try{
			$(document.createElement('div'))
				.attr({title: myTitle, 'class': 'alert'})
				.html(message)
				.dialog({
					buttons: {OK: function(){$(this).dialog('close');}},
					close: function(){$(this).remove();},
					draggable: true,
					modal: true,
					resizable: true,
					width: 600,
					height: 350
				});
		}catch(e){
			// default to the normal JS alert
			alert(message);
		}
	};

// Attach Ajax spinner to page loads
/*$(document).ready(function(){
	
	$('<div></div>').appendTo('body')
		.html("<div id='#Dev2SourceManagementLoadingDialog'> <center> <img src='/scripts/3.gif' /> </center> </div>")
		.dialog({
		autoOpen: false,
		bgiframe:true,
		modal:true,
		width:96,
		height:96,
		title : "Loading...",
		resizable:false
		}).bind("ajaxStart",function(){
			$(this).dialog("open");
		}).bind("ajaxStop", function(){
			$(this).dialog("close");
		});


});*/

// Travis : Function to append help dialog :)
var uiForm = "uiForm";
var helpDiv = "helpDialog";
var defaultTitle = "Tell Me More Dialog";
var helpCreated = false;
var gridAddCreated = false;
var newBinding;
var tellMeMore = "tellMeMore";
var defaultHelpTxt = "In this dialog help data will appear for the Wizard element you are currently working on. \
		<br/>Simply click the Tell Me More button to close this window."
	
// Travis : Bootstrap the help dialog	
function bootStrapHelp(){
	try{
		if(!helpCreated){
			var bindingDiv = document.getElementById(uiForm);
			
			// create the wrapper div
			var newBinding = document.createElement("div");
			newBinding.setAttribute("id", helpDiv);
			newBinding.setAttribute("title", defaultTitle);
			newBinding.innerHTML = defaultHelpTxt;

			bindingDiv.appendChild(newBinding);
			helpCreated = true;
		}
	}catch(e){}
}

// Travis : Create the datagrid add dialog
function createGridEditDialog(bindingRegion, gridTitle, editService, optType, webServer, payload, jsonCols){
	try{
		if(!gridAddCreated){
			var bindingDiv = document.getElementById(bindingRegion);
			
			// create the wrapper div
			newBinding = document.createElement("div");
			newBinding.setAttribute("id", "gridAddDialog");
			//newBinding.setAttribute("title", gridTitle + " " +optType + " Row");
			
			bindingDiv.appendChild(newBinding);
			gridAddCreated = true;
		}
		
		var myCols = eval(jsonCols);
		var dataToSend = "";
		
		var xDoc = $.parseXML("<tr>" + payload +"</tr>");
		
		var pos = 0;
		
		$(xDoc).find("td").each(function(){
			// process each cell
			var val = $(this).text();
			var field = myCols.cols[pos].Dev2Field;
			dataToSend += field+"="+val;

			if((pos+1) < myCols.cols.length){
			   dataToSend += "&";
			}
		
			pos++;
		});
		
		newBinding.innerHTML = "<iframe id='gridCRUDDialog' src='" + webServer +"/services/"+editService+"?operation=" + optType +"&" + dataToSend + "' style='width:100%; height:95%' scrolling='yes'></iframe>";
		
	}catch(e){
		prompt(e);
	}
}

// Travis : Function to swap help text 
function displayHelp(title,helpTxt){
    try{
		var help = document.getElementById(helpDiv);
		$( "#"+helpDiv ).dialog( "option", "title", title );
		help.innerHTML = helpTxt;
	}catch(e){}
}

// Travis : Exit the wizard
function exitWizard(){
	try{
		Dev2Awesomium.Cancel();
		//window.external.Cancel();
		//window.external.close();
	}catch(e){
		try{
			window.close();
		}catch(e1){}
	}
}

// Travis : Hide/Show advanced regions
var advOn = false;
function toggleAdvanced(){

	if(!advOn){
		advOn = true;
		$('.advancedRegion').show();
	}else{
		advOn = false;
		$('.advancedRegion').hide();
	}
	
	// now turn on disabled elemetns
	toggleAdvancedFormElements(advOn);
}

// Travis : Assist function to enable all disabled form element for advanced settings
var arrayToggleFields = new Array();
function toggleAdvancedFormElements(advOn){

	var formData = document.forms[0].elements;
	
	// clear array to be safe
	if(advOn){
		arrayToggleFields = new Array();
	}
	
	if(advOn){
		for(var i = 0; i < formData.length; i++){	
			if(formData[i].disabled){
				arrayToggleFields[(arrayToggleFields.length)] = i;
				formData[i].disabled = false;
			}
		}
	}else{
		for(var q = 0; q < arrayToggleFields.length; q++){
			formData[arrayToggleFields[q]].disabled = true;
		}
	}
}

/** Dynamic Custom Data Function **/
var bindingIdx = 0;
var newBindingRegionDiv = "newBinding";
var newBindingRegionTxt = "newBindingTxt";
var newBindingDeleteBtn = "newBindingDel";

// Travis : Function to boot strap any bound values for idx
function bootStrapIdx(){

	var idx = 0;
	var i = 0;
	var error = false;
	
	while(!error){
		try{
			var tmp = document.getElementById(newBindingRegionDiv+(i));
			if(tmp == null){
				error = true;
			}else{
				i++;
			}
		}catch(e){
			error = true;
		}
	}

	idx = i;
	
	return idx;
}

// Travis : Dynamic row add for select, checkbox, radio button additions
var simpleEntryBinding = function addEntrySimple(newBinding, jsonArgs, valueOf){

	// create the new binding text
	var newBindingTxt = document.createElement("span");
	newBindingTxt.setAttribute("id", newBindingRegionTxt+bindingIdx);
	newBindingTxt.innerHTML = jsonArgs.bindingTextPrefix + " # " + (bindingIdx + 1)+" ";
	
	// now create the input element
	var newBindingInput = document.createElement("input");
	newBindingInput.setAttribute("type", "text");
	newBindingInput.setAttribute("name", jsonArgs.inputName);
	
	if(valueOf != undefined){
		if(valueOf.length > 0){
			newBindingInput.setAttribute("value", valueOf);
		}
	}
	
	// now build up the object
	newBinding.appendChild(newBindingTxt);
	newBinding.appendChild(newBindingInput);
	
	return newBinding;
	
};

// Travis : Dynamic row add for data grid
var dataGridEntryBinding = function(newBinding, jsonArgs, preConfig){
	// create the header input textbox
	var rowID = document.createElement("label");
	rowID.innerHTML = (bindingIdx+1) + " &nbsp; &nbsp;";
	
	var val = '';

	if(preConfig == undefined){
		var headerInput = generateInputTextElm(jsonArgs.header, val);
		var dataInput = generateInputTextElm(jsonArgs.data, val);
		var widthInput = generateInputTextElm(jsonArgs.width, val);
		var alignInput = generateSelectElm(jsonArgs.align,["Left", "Right", "Center"], val);
	}else{
		val = preConfig.Dev2ColumnHeader;
		var headerInput = generateInputTextElm(jsonArgs.header, val);
		val = preConfig.Dev2Field;
		var dataInput = generateInputTextElm(jsonArgs.data, val);
		val = preConfig.Dev2GridWidth;
		var widthInput = generateInputTextElm(jsonArgs.width, val);
		val = preConfig.Dev2ColumnAlignment;
		var alignInput = generateSelectElm(jsonArgs.align,["Left", "Right", "Center"], val);
	}
	
	var tmp = newBinding.appendChild(rowID);
	tmp.className = "gridColNumHeader";
	
	tmp = newBinding.appendChild(headerInput);
	tmp.className = jsonArgs.header.css;
	
	tmp = newBinding.appendChild(dataInput);
	tmp.className = jsonArgs.data.css;
	
	tmp = newBinding.appendChild(widthInput);
	tmp.className = jsonArgs.width.css;
	
	tmp = newBinding.appendChild(alignInput);
	tmp.className = jsonArgs.align.css;
	
	
	return newBinding;
};

function generateCheckBox(def){
	var result = document.createElement("input");
	result.setAttribute("type", "checkbox");
	result.setAttribute("name", def.name+bindingIdx);
	result.setAttribute("id", def.name);
	return result;
}

function generateSelectElm(def, args, val){

	var result = document.createElement("select");
	result.setAttribute("name", def.name+bindingIdx);
	result.setAttribute("id", def.name);
	
	for(var i = 0; i < args.length; i++){
		var opt = document.createElement("option");
		opt.setAttribute("value", args[i]);
		if(val == args[i]){
			opt.setAttribute("selected", "true");
		}
		opt.innerHTML = args[i];
		result.appendChild(opt);
	}
	
	return result;
}

function generateInputTextElm(def, val){
	var result = document.createElement("input");
	result.setAttribute("type", "text");
	result.setAttribute("name", def.name+bindingIdx);
	result.setAttribute("id", def.name);
	result.setAttribute("value", val);
	return result;
}

// Travis : Returns the new binding for custom data regions
function generateBindingWrapper(){
	// create the wrapper div
	var newBinding = document.createElement("div");
	newBinding.setAttribute("id", newBindingRegionDiv+bindingIdx);
	newBinding.setAttribute("class", "customData");
	
	return newBinding;
}

// Travis : Dynamically add a new data row to DOM ( select, checkbox, radio button )
// dataGenFn return a div element to be appended to the binding region
function addEntry(bindingRegion, dataGenFn, jsonArgs, preConfig){
	if(bindingIdx == 0){
		// account for re-bound data
		bindingIdx = bootStrapIdx();
	}
	
	var bindingDiv = document.getElementById(bindingRegion);
	
	// call the passed dataGenFn with the pre-built bindable region
	var newBinding = dataGenFn(generateBindingWrapper(), jsonArgs, preConfig);
	
	// now add row to binding region
	bindingDiv.appendChild(newBinding);
	
	bindingIdx++;
	
}

// Travis : Delete the last row
function deleteLastRow(bindingRegion){
	if(bindingIdx == 0){
		// account for re-bound data
		bindingIdx = bootStrapIdx();
		
	}

	var toDeleteDiv = document.getElementById(newBindingRegionDiv+(bindingIdx-1));
	var parentDiv = document.getElementById(bindingRegion);
	
	try{
		parentDiv.removeChild(toDeleteDiv);
		bindingIdx--;
	}catch(e){}

}

// Travis : place 1st row if empty data
function bootStrapDataGrid(baseConfig){

	var preConfig = eval(baseConfig);
	// parse the json object and populate data regions
	if(preConfig != undefined){
		for(var i = 0; i < preConfig.cols.length; i++){
			addEntry("bindingRegion",dataGridEntryBinding,{header: { name : "Dev2ColumnHeader", css: "genericGridInput"}, data : { name : "Dev2Field", css: "genericGridInput"}, width: { name: "Dev2GridWidth", css: "gridInputWidth"}, align: { name: "Dev2ColumnAlignment", css: "genericGridSelect"}}, preConfig.cols[i]);
		}
	}else{
		// virgin config
		addEntry("bindingRegion",dataGridEntryBinding,{header: { name : "Dev2ColumnHeader", css: "genericGridInput"}, data : { name : "Dev2Field", css: "genericGridInput"}, width: { name: "Dev2GridWidth", css: "gridInputWidth"}, align: { name: "Dev2ColumnAlignment", css: "genericGridSelect"}});
	}
}


// Testing Function
function sanityCheck(){
	var result = "";
	
	for(var i = 0; i < (bindingIdx+2); i++){
		try{
			var tmp = document.getElementById(newBindingDeleteBtn+(i));
			result += i + " -> " + tmp.getAttribute("onClick");
		}catch(e){
			result += i + " -> " + e;
		}
		
		result += "\n";
	}
	
	alert(result);
}

function buildDataGridConfig(){

	var list = $("#bindingRegion").children();
	
	// ( {cols: [{
	var config = "({ cols: [ ";
	
	for(var i = 0; i < list.length; i++){
		var id = list[i].id;		
		var children = $("#"+id).children();
		
		config += "{ ";
		for(var q = 0; q < children.length; q++){
			// how process each row of data and extract files for json build up
			var elm = children[q];
			if(elm.name != undefined){
				if(elm.type == 'checkbox'){
					var val = false;
					if(elm.checked){
						val = true;
					}
					config += "" + children[q].id + ": \"" + val + "\"";
				}else{
					config += "" + children[q].id + ": \"" + children[q].value + "\"";
				}
				if((q+1) < children.length){
					config += ",";
				}
			}
		}		
		config += " }";
		if((i+1) < list.length){
			config += ", ";
		}
	}
	
	config += " ]})";
	document.getElementById("Dev2GridMappingJson").value = config;

	// now build table columns
	//var tConfig  = eval(config);

}

/** End Dynamic Custom Data Functions **/

// Travis : display element name as it would be referenced in a workflow
function displayWorkflowName(elmUpdate, elmFetch, nameList, origValue, parseForError){

	try{
		var elm = document.getElementById(elmFetch);
		var elm2 = document.getElementById(elmUpdate);
		
		elm2.innerHTML = $.trim(elm.value);
		
		if(parseForError){
			
			
			var idx = nameList.indexOf(">"+elm.value+"<");
			var error = "";
			
			if(idx >= 0 && elm.value != origValue){
				error = "Sorry, there is already a webpart on this page with the Name <i> " + elm.value + "</i>. Please use another."
			}

			if(error != ""){
				nameErrorMsg = error;
				isValidName = false;
				isValidForm = false; 
			}else{
				isValidForm = true; 
				isValidName = true;
				nameErrorMsg = "";
			}
		}
	}catch(e){}
}

function isUniqueName(nameList, val, origValue){
	var error = "";
	try{
		var xmlDoc;
		
		if (window.DOMParser)
		{
		  parser=new DOMParser();
		  xmlDoc=parser.parseFromString(nameList,"text/xml");
		}else { //IE
		  xmlDoc=new ActiveXObject("MSXML.DOMDocument");
		  xmlDoc.async=false;
		  xmlDoc.loadXML(nameList);
		}
	
		var tags = xmlDoc.getElementsByTagName("Dev2ElementName");
		var i = 0;
		
		while(i < tags.length && error == ""){
			var tmpVal = tags[i].childNodes[0].nodeValue;
			if(tmpVal == val && val != origValue){
				error = "Sorry, there is already a webpart on this page with the Name " + val + ". Please use another."
			}
			i++;
		}
		
	}catch(e){
		//alert(e);
	}
	
	return error;
}

// Travis : toggle help in wizard
var helpOn = false;
function toggleHelp(){

	if(!helpOn){
		helpOn = true;
		bootStrapHelp();			
		// now make a dialog
		$( "#" + helpDiv ).dialog({ hide: 'slide' });
		// now avoid closing
		$(".ui-dialog-titlebar-close", this.parentNode).hide();
		
		try{
			var position =  $("#"+tellMeMore).position();
			var y = position.top + 25;
			var x = position.left + 70;
			$("#" + helpDiv).dialog("option", "position", [x, y]);
		}catch(e){}
		$("#" + helpDiv ).dialog( "option", "height", 100 );
		$("#" + helpDiv ).dialog( "option", "width", 400 );
		document.getElementById(helpDiv).innerHTML = defaultHelpTxt;
		
	}else{
		helpOn = false;
		$("#"+helpDiv).dialog("close");
	}
}

// Travis  : clear grid message region
var clearRegionDiv
function clearRegion(){
	$("#"+clearRegionDiv).html("");
}

// Travis :  toggle grid add dialog
var gridOn = false;
function toggleGridEdit(bindingRegion, gridTitle, editService,op, webServer, payload, jsonCols, updatePos){
	var dID = "gridAddDialog";

	createGridEditDialog(bindingRegion, gridTitle, editService, op, webServer, payload, jsonCols);			
	// now make a dialog
	$( "#" + dID ).dialog({ modal: true });
	
	var position =  $("#container").position();
	var y = position.top + 45;
	var x = position.left + 20;
	$("#" + dID).dialog("option", "position", [x, y]);
	$("#" + dID).dialog( "option", "height", 550 );
	$("#" + dID).dialog( "option", "width", 800 );
	$("#" + dID).dialog( "option", "title", gridTitle + " " +op + " Row" );
	// gridCRUDDialog
	$("#" + dID).dialog( "option", "buttons", [
		{
			text: "Cancel",
			click: function() { 
				$(this).dialog("close"); 
			}
		},
		{
			text: "Ok",
			click: function() { 
				var theDialog = $(this);
				if(window.parent.$("#gridCRUDDialog")[0].contentWindow.checkRequired()){
					
					var iframeFormAction = $("#gridCRUDDialog").contents().find('form').attr('action');
					
					$("#gridCRUDDialog").contents().find('form').submit(function(){
						// fetch inner iframe data for post back
						var sendData = $(this).serialize();
						
						$.ajax({
							type: 'POST',
							url: webServer+iframeFormAction,
							data : sendData,
							async:false,
							success: function (payload) {
								
								// TODO : Process XML for inclusion back into the table 
								
								if(op == "Add" || op == "Edit"){
									// http://datatables.net/api
									var myCols = eval(jsonCols);
									var dataToAdd = "";
									
									for(var i = 0; i < myCols.cols.length; i++){
										var field = myCols.cols[i].Dev2Field;

										var val = $(payload).find(field).text();
										if(val == ""){
											val = " ";
										}
									   
									   dataToAdd += "'"+val+"'";
									   if((i+1) < myCols.cols.length){
										   dataToAdd += ",";
									   }
									}

									dataToAdd = "[" + dataToAdd + "]"

								   // now build and post back add data
								   if(op == "Add"){
									   $('#example').dataTable().fnAddData(eval(dataToAdd));
								   }else if(op == "Edit"){
									 // oTable.fnUpdate( ['a', 'b', 'c', 'd', 'e'], 1, 0 ); // Row
									 $('#example').dataTable().fnUpdate(eval(dataToAdd), updatePos, 0);
								   }
								}
								
								theDialog.dialog("close"); 
							},
							error : function(data){ alert('Problems submiting form'); }
						});						
						return false;
					
					});
					
					//submit the form to invoke the handler
					$("#gridCRUDDialog").contents().find('form').submit();
				}
			}
		}
	] );
}

/** Validation Methods **/

// Travis : Function to verify required elements populated
var clientValidation = "";
var isValidForm = false;
var isValidName = false;
var nameErrorMsg;
function checkRequired(){
	var list = $(".requiredClass");
	isValidForm = true;
	isValidName = true;
	var i = 0;
	
	while(i < list.length && isValidForm){
		
		try{
			// trim the string
			//if(list[i].id != ''){
				//var elmTo = document.getElementById(list[i].id);
				var elmTo = list[i];
				// do the nasty radio button  and checkbox check 
				if(elmTo.type == 'radio' || elmTo.type == 'checkbox'){
					
					var nodes = $("input[Name=" + elmTo.name +"]");
					
					var checkCount = 0;
					var q = 0;
					while(q < nodes.length && checkCount == 0){
						if(nodes[q].checked){
							checkCount++;
						}
						q++;
					}
					
					if(checkCount == 0){
						isValidForm = false;
					}
				}else{
					// default case, check date specific queries
					if(elmTo.value == '' || elmTo.value == 'dd/mm/yyyy' || elmTo.value == 'dd/mm/yy' || elmTo.value == 'mm/dd/yyyy' || elmTo.value == 'mm/dd/yy' || elmTo.value == 'yyyy/mm/dd' || elmTo.value == 'yy/mm/dd'){
						isValidForm = false;	
					}
				}
		}catch(e){
		}
		i++;
	}
	
	if(!isValidForm){
		alert("Please fill in all required values as indicated with a *", "Missing Required Information");	
		//alert("Please fill in all required values as indicated with a *");
		//return false;
	}else{
		// now do formating checks
		clientValidation = "";
		if(!validateEmail()){
			alert(clientValidation,"Problem With Information Format");
			//alert(clientValidation);
			isValidForm = false;
			//return false;
		}else{
			// check number regions
			if(!validateLetters()){
				//alertOverride(clientValidation,"Problem With Information Format");
				alert(clientValidation);
				isValidForm = false;
				//return false;
			}else{
				if(!validateWholeNumbers()){
					//alertOverride(clientValidation,"Problem With Information Format");
					alert(clientValidation);
					isValidForm = false;
					//return false;
				}else{
					if(!validateDecimalNumbers()){
						//alertOverride(clientValidation,"Problem With Information Format");
						alert(clientValidation);
						isValidForm = false;
						//return false;
					}else{
						if(!validateLetterAndNumber()){
							alert(clientValidation,"Problem With Information Format");
							//alert(clientValidation);
							isValidForm = false;
							//return false;
						}
					}
				}
			}
		}
	}
	
	
	
	if(!isValidName){
		alert(nameErrorMsg, "Webpart Name in Use Already");
		//alert(nameErrorMsg);
		isValidForm = false;
		//return false;
	}
	
	return isValidForm;
}

function fetchErrMsg(baseId){
	var errMsgID = baseId + "ErrMsg";
	var errElm = document.getElementById(errMsgID);
	var t = errElm.value;
	
	return t;
}

// Travis : Client side validation for format
function validateEmail(){

	//var list = $(".requiredClass").filter(".emailValidation");
    var list = $(".emailValidation");	
	var reg = /^([A-Za-z0-9_\-\.])+\@([A-Za-z0-9_\-\.])+\.([A-Za-z]{2,4})$/;
	var result = true;
	
    for(var i = 0; i < list.length; i++){
		try{
			var elmTo = document.getElementById(list[i].id);
			var val = elmTo.value;
			var t = fetchErrMsg(elmTo.id);
			
			if(!reg.test(val)){
				clientValidation += t;
				clientValidation += "\n";
				result = false;
			}
		}catch(e){}
	}   
	return result;
}

function validateLetters(){
	
	var reg = /^\s*([a-zA-Z]+)(\s)+([a-zA-Z]*)/;	
	//var list = $(".requiredClass").filter(".lettersValidation");
	var list = $(".lettersValidation");	
	var result = true;
	
    for(var i = 0; i < list.length; i++){
		try{
			var elmTo = document.getElementById(list[i].id);
			var val = elmTo.value;
			var t = fetchErrMsg(elmTo.id);
			
			if(!reg.test(val)){
				clientValidation += t;
				clientValidation += "\n";
				result = false;
			}
		}catch(e){}
	}   
	
	return result;
}

function validateWholeNumbers(){
	var reg = /^\s*\d+\s*$/;
	//var list = $(".requiredClass").filter(".wholeValidation");
	var list = $(".wholeValidation");	
	var result = true;
	
    for(var i = 0; i < list.length; i++){
		try{
			var elmTo = document.getElementById(list[i].id);
			var val = elmTo.value;
			var t = fetchErrMsg(elmTo.id);
			
			if(!reg.test(val)){
				clientValidation += t;
				clientValidation += "\n";
				result = false;
			}
		}catch(e){}
	}   
	return result;
}

function validateDecimalNumbers(){
	var reg = /^\s*(\+|-)?((\d+(\.\d+)?)|(\.\d+))\s*$/;
	//var list = $(".requiredClass").filter(".decimalValidation");
	var list = $(".decimalValidation");	
	var result = true;
	
    for(var i = 0; i < list.length; i++){
		try{
			var elmTo = document.getElementById(list[i].id);
			var val = elmTo.value;
			var t = fetchErrMsg(elmTo.id);
			
			if(!reg.test(val)){
				clientValidation += t;
				clientValidation += "\n";
				result = false;
			}
		}catch(e){}
	}   
	return result;
}

function validateLetterAndNumber(){
	var reg = /^\s*([0-9a-zA-Z]+)\s*$/;
	//var list = $(".requiredClass").filter(".letterNumbersValidation");
	var list = $(".letterNumbersValidation");	
	var result = true;
	
    for(var i = 0; i < list.length; i++){
		try{
			var elmTo = document.getElementById(list[i].id);
			var val = elmTo.value;
			var t = fetchErrMsg(elmTo.id);
			
			if(!reg.test(val)){
				clientValidation += t;
				clientValidation += "\n";
				result = false;
			}
		}catch(e){}
	}   
	return result;
}

/* End Validation Methods */

// Travis : Allow to rebind validation options
var initRebind = true;
function rebindValidation(selectElm, validationOpt, msgSpan, msgTB){
	var elm = document.getElementById(selectElm);
	try{
		var opts = elm.options;
		for(var i = 0; i < opts.length; i++){
			if(opts[i].value == validationOpt){
				opts[i].selected = true;
				if(i == 0){ // strip required css and do not display error message box
					$('#'+msgTB).removeClass('requiredClass'); // text box
					$('#'+msgTB).val("");
					$('#'+msgSpan).removeClass('advancedRegion');
					$('#'+msgSpan).css("display", "none");
					
				}else{
					if(!initRebind){
						$('#'+msgSpan).css("display", "inline");
						$('#'+msgSpan).addClass("advancedRegion");
						$('#'+msgTB).addClass('requiredClass');
					}else{
						initRebind = false;
					}
				}
			}
		}
	}catch(e){}
} 

// Travis : Hide/show service and custom data bound regions
function toggleCustomDatabinding(isService){
	
	if(isService){
		
		$('.serviceDataBinding').show();
		$('.customDataBinding').hide();
		
		// add the requiredClass class to the customDataBinding elements
		var list = $(".serviceDataBinding");
		for(var i = 0; i < list.length; i++){
			
			try{
					var elmTo = document.getElementById(list[i].id);
					if(list[i].id != '' && list[i].id != 'Dev2ServiceParametersCB' && list[i].id != 'Dev2ServiceParametersRB' && list[i].id != 'Dev2ServiceParametersDD'){
						$('#' + list[i].id).addClass('requiredClass');
					}
			
			}catch(e){}
		}
	}else{
		$('.serviceDataBinding').hide();
		$('.customDataBinding').show();
		// strip the requiredClass class from customDataBinding elements
		var list = $(".serviceDataBinding");
		
		for(var i = 0; i < list.length; i++){
			try{
					if(list[i].id != ''){
						$('#' + list[i].id).removeClass('requiredClass');
					}
			
			}catch(e){}
		}
	}
}


// Travis : convert grid add form to XML for editService
function serializedFormToXML(data){
	var rootTag = "<gridEdit>";
	rootTag += "<operation>add</operation>";
	var pieces = data.split("=");
	
	var keys = [];
	var vals = [];
	
	var keyIndicator = 0;
	var idxK = 0;
	var idxV = 0;
	
	// build key value arrays
	for(var i = 0; i < pieces.length; i++){
		
		if(keyIndicator == 0){
			keys[idxK] = pieces[i];
			keyIndicator=1;
			idxK++;
		}else if(keyIndicator == 1){
			vals[idxV] = pieces[i];
			keyIndicator = 0;
			idxV++;
		}
	}
	
	// build XML fragment up
	for(var q =0; q < keys.length; q++){
		rootTag += "<" + keys[q] + ">" + vals[q] + "</" + keys[q] + ">";
	}
	
	rootTag += "</gridEdit>";
	
	return rootTag;
}

// Travis  : Parse innerHTML fragement into DOM
function htmlToXML(op,data, value){
	var pieces = data.split("<td");
	var rootTag = "<gridEdit>";
	rootTag += "<operation>" + op + "</operation>";
	
	for(var i = 0; i < pieces.length; i++){
		var piece = $.trim(pieces[i]);
		if(piece != ''){
			// extract and check for the key regions
			var idStart = piece.indexOf("id=\"");
			if(idStart >= 0){
				idStart += 4;
				var endID = piece.substr(idStart).indexOf("\"");
				var id = piece.substr(idStart, endID);
				// id="type" class="editableGridRegion left"><form class="row_selected"><input name="value" autocomplete="off" style="width: 150px; height: 14px;"></form></td>
				if(piece.indexOf("<form") >= 0){
					// edited region
					rootTag += "<" + id + ">" + value +"</" + id + ">";
				}else{ // non-edited region
					var dataStart = piece.indexOf(">");
					dataStart += 1;
					var dataEnd = piece.indexOf("<");
					var data = piece.substr( dataStart, (dataEnd - dataStart));
					rootTag += "<" + id + ">" + data + "</" + id + ">";
				}
			}
		}
	}
	
	rootTag += "</gridEdit>";
	
	return rootTag;
}

// Travis : address html fragment generation for grid in workflow
function emitGridToolBar(editMode){
	var result = '<div style="display:block;"><div id="addRow" style="float:left;"><button type="button" class="editGridButton">Add</button></div><div id="editRow" style="float:left;"><button type="button" class="editGridButton">Edit</button></div><div id="deleteRow" style="float:left;"><button type="button" class="editGridButton"> Delete</button></div><div id="errorRegion" style="float:left; margin-left:10px; color:red;"></div></div>';

	if(!editMode){
		result = '<div style="display:block;"><div id="errorRegion" style="float:left; margin-left:10px; color:red;"></div>';
	}
	
	return result;
}


//function bindAutoComplete(sourceFieldId, searchTermTag, AutoCompleteTextProperty, RelatedFieldName, rowDelimiter, boundServiceName) {
// removed searchTermTag due to complexity
function bindAutoComplete(sourceFieldId, displayField, relatedFieldName, rowDelimiter, boundServiceName, serverURL) {	
	var tags = [];

	$("#"+sourceFieldId).autocomplete({
		source: function(req, add){
			//var dialBackTemplate = "http://127.0.0.1:1234/services/AutocompleteBinder?Dev2BoundServiceName="+boundServiceName+"&Dev2RowDelimiter="+rowDelimiter+"&Dev2Field="+displayField;
			// we have a linked
			var dialBackTemplate = "http://127.0.0.1:1234/services/" + boundServiceName;
			
			if(relatedFieldName.length > 0){
				var v = $("#" + relatedFieldName).val();
				dialBackURI = dialBackTemplate + "&" + relatedFieldName + "=" +  v;
			}else{
				dialBackURI = dialBackTemplate;
			}
			// encode dialback URI
			dialBackURI = encodeURI(dialBackURI);
			$.ajax({
				type: 'GET',
				url: dialBackURI,
				dataType: "XML",
				success: function (payload) {
					// clear the list
					while(tags.length > 0){
						tags.pop();
					}

					var value = $("#"+sourceFieldId).val().toUpperCase();
					$(payload).find(displayField).each(function(){
						var tmpSearch = $(this).text().toUpperCase();
						if(tmpSearch.indexOf(value) >= 0){
							tags.push($(this).text());
						}
					});					
					
					add(tags);
				},
				error : function(data){ /*alert("Problem contacting service [ " + boundServiceName + " ]. Please ensure it returns XML data! " + data + " " + data2 + " " + data3);*/ }
			});
		}
	});
}

// Travis : XML formater used for pushing menu configuration for persist
var formatXml = function (xml) {
	var reg = /(>)(<)(\/*)/g;
	var wsexp = / *(.*) +\n/g;
	var contexp = /(<.+>)(.+\n)/g;
	xml = xml.replace(reg, '$1\n$2$3').replace(wsexp, '$1\n').replace(contexp, '$1\n$2');
	var pad = 0;
	var formatted = '';
	var lines = xml.split('\n');
	var indent = 0;
	var lastType = 'other';
	// 4 types of tags - single, closing, opening, other (text, doctype, comment) - 4*4 = 16 transitions 
	var transitions = {
		'single->single': 0,
		'single->closing': -1,
		'single->opening': 0,
		'single->other': 0,
		'closing->single': 0,
		'closing->closing': -1,
		'closing->opening': 0,
		'closing->other': 0,
		'opening->single': 1,
		'opening->closing': 0,
		'opening->opening': 1,
		'opening->other': 1,
		'other->single': 0,
		'other->closing': -1,
		'other->opening': 0,
		'other->other': 0
	};

	for (var i = 0; i < lines.length; i++) {
		var ln = lines[i];
		var single = Boolean(ln.match(/<.+\/>/)); // is this line a single tag? ex. <br />
		var closing = Boolean(ln.match(/<\/.+>/)); // is this a closing tag? ex. </a>
		var opening = Boolean(ln.match(/<[^!].*>/)); // is this even a tag (that's not <!something>)
		var type = single ? 'single' : closing ? 'closing' : opening ? 'opening' : 'other';
		var fromTo = lastType + '->' + type;
		lastType = type;
		var padding = '';

		indent += transitions[fromTo];
		for (var j = 0; j < indent; j++) {
			padding += '\t';
		}
		if (fromTo == 'opening->closing')
			formatted = formatted.substr(0, formatted.length - 1) + ln + '\n'; // substr removes line break (\n) from prev loop
		else
			formatted += padding + ln + '\n';
	}

	return formatted;
};

// Travis : Strip Dev2HTML for render
function prepDev2Config(part){
	part = part.replace(new RegExp("&lt;", 'g'),'<');
	part = part.replace(new RegExp("&gt;", 'g'),'>');
	return part;
}

// Travis : Strip out <script> tags
function stripScript(){
	var v = $("#customButtonCode").val();
	v = v.replace(new RegExp('\n', 'g'), "\\n");
	v = v.replace(new RegExp('<script[^>]*?>', 'g'), '');  
	v = v.replace(new RegExp('<\/script>', 'g'), ''); 
	v = v.replace(new RegExp("'", 'g'), '"');
	$("#customButtonCode").val(v);
}

// Travis : Strip Dev2HTML for render
function prepDev2Config(part){
	part = part.replace(new RegExp("&lt;", 'g'),'<');
	part = part.replace(new RegExp("&gt;", 'g'),'>');
	return part;
}

function hackPushMenu(){
	pushMenuForPersist("menuManager", "serializedTreeViewXML", "serializedTreeViewJSON");
}

// Travis : Rebind a menu back for editing
function pushMenuForPersist(menuRoot, hiddenElmXML, hiddenElmJSON){
	var dict = $("#"+menuRoot).dynatree("getTree").toDict();
	var json = JSON.stringify(dict.children);
	json = json.substring(1, json.length-1);
	
	//var childrenJSON = JSON.stringify(dict.children);
	//var json = eval("(" + JSON.stringify(dict) +")");
	
	var xotree = new XML.ObjTree();
	var formatedXML = formatXml(xotree.writeXML(dict.children));
	var pos = 0;
	
	// replace <x> with <Dev2Menu>
	while(formatedXML.indexOf(pos) >= 0){
		formatedXML = formatedXML.replace("<" + pos + ">", "<Dev2Menu>");
		formatedXML = formatedXML.replace("</" + pos + ">", "</Dev2Menu>");
		pos++;
	}
	
	// replace <children> with <Dev2MenuOption>
	formatedXML = formatedXML.replace(new RegExp("<children>", 'g'), "<Dev2MenuOption>");
	formatedXML = formatedXML.replace(new RegExp("</children>", 'g'), "</Dev2MenuOption>");
	formatedXML = formatedXML.replace(new RegExp("\n", 'g'), " ");
	
	//alert(formatedXML);
	
	// push XML and JSON formated data into hidden form elements for persist.
	$("#"+hiddenElmXML).val(formatedXML);
	$("#"+hiddenElmJSON).val(json);
}

// Travis : Push static data collection for easy manip
function pushStaticCollection(fieldName, placeRegion){
	
	// IE not compliant with this tactic
	//var list = $("input[name=" + fieldName+"]");
	
	var list = $("input");
	var result = "<itemCollection>";
	
	for(var q = 0; q < list.length; q++){
		if(list[q].name == fieldName){
			result += "<item>" + list[q].value + "</item>";
		}
	}
	result += "</itemCollection>";

	// save to hidden field
	try{
		$("#" + placeRegion).val(result);
	}catch(e){}
	
}

// webpage no longer allow for data to be passed in on render
function rebindStaticItems(boundData, bindingRegion, generateFn, jsonConfig){

	var r = boundData.replace("<itemCollection>","").replace("</itemCollection>",""); 
	var sOn = "<item>";
	var elms = r.split(sOn);
	var res = "";

	var bindingDiv = document.getElementById(bindingRegion);
	
	if(elms.length == 1){
		var newBinding = generateFn(generateBindingWrapper(), jsonConfig, "");
		// now add row to binding region
		bindingDiv.appendChild(newBinding);
		bindingIdx++;
	}else{
		for(var i = 1; i < elms.length; i++){
			
			var pos = elms[i].indexOf("</item>");
			var data = elms[i].substr(0, pos);
			
			var newBinding = generateFn(generateBindingWrapper(), jsonConfig, data);
			// now add row to binding region
			bindingDiv.appendChild(newBinding);
			bindingIdx++;
		}
	}
}