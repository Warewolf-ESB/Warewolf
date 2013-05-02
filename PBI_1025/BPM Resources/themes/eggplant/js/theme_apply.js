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
});