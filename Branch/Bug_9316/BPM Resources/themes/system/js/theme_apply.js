/*
 * Boot strap the theming of form elements
*/

$(function(){
	// Travis apply theme
	
	// Button
	$("button").addClass("ui-widget ui-state-default ui-corner-all");

	//$("checkbox").checkbox();
	// style selects
	$("select").selectmenu({
		style: 'dropdown'
	});
	
	
	// style text, password and textareas
	$("input").addClass("ui-widget ui-state-default ui-corner-all");
	// .ui-widget-header 
	$("textarea").addClass("ui-widget-header");
	
	// bind checkbox and radio button
	$("input[type=checkbox]").checkbox();
	
	$("input[type=radio]").checkbox();
		
	//all hover and click logic for buttons
	$("button")
	.hover(
		function(){ 
			$(this).addClass("ui-state-hover"); 
		},
		function(){ 
			$(this).removeClass("ui-state-hover"); 
		}
	)
	.mousedown(function(){
			$(this).parents('.fg-buttonset-single:first').find(".fg-button.ui-state-active").removeClass("ui-state-active");
			if( $(this).is('.ui-state-active.fg-button-toggleable, .fg-buttonset-multi .ui-state-active') ){ $(this).removeClass("ui-state-active"); }
			else { $(this).addClass("ui-state-active"); }	
	})
	.mouseup(function(){
		if(! $(this).is('.fg-button-toggleable, .fg-buttonset-single .fg-button,  .fg-buttonset-multi .fg-button') ){
			$(this).removeClass("ui-state-active");
		}
	});
	$("input")
	.hover(
		function(){ 
			$(this).addClass("ui-state-hover"); 
		},
		function(){ 
			$(this).removeClass("ui-state-hover"); 
		}
	);
	
	// handle file upload controls
	/*var W3CDOM = (document.createElement && document.getElementsByTagName);

	if (!W3CDOM) return;
	var fakeFileUpload = document.createElement('div');
	fakeFileUpload.className = 'fakefile';
	fakeFileUpload.appendChild(document.createElement('input'));
	var image = document.createElement('img');
	image.src='pix/button_select.gif';
	fakeFileUpload.appendChild(image);
	var x = document.getElementsByTagName('input');
	for (var i=0;i<x.length;i++) {
		if (x[i].type != 'file') continue;
		if (x[i].parentNode.className != 'fileinputs') continue;
		x[i].className = 'file hidden';
		var clone = fakeFileUpload.cloneNode(true);
		x[i].parentNode.appendChild(clone);
		x[i].relatedElement = clone.getElementsByTagName('input')[0];
		x[i].onchange = x[i].onmouseout = function () {
			this.relatedElement.value = this.value;
		}
	}*/

});