// Show Text
<span>
  <script>
	var isChecked = "[[showText]]";
	if(isChecked.length > 0){
		document.write('<input type="checkbox" name="showText" id="showText" checked="true" /> Show');
	}else{
		document.write('<input type="checkbox" name="showText" id="showText" /> Show');
	}
  </script>
</span>

// Required
<span>
  <script>
	var r = "[[required]]";
	if(r.length > 0){
		document.write('<input type="checkbox" name="required" id="required" value="on" checked="true" /> Required');
	}else{
		document.write('<input type="checkbox" name="required" id="required" value="on" /> Required');
	}
  </script>
  
</span>

// Allow Edit
<span>
  <script>
  var edit = "[[allowEdit]]";
  
  if(edit == "yes"){
	document.write('<input type="radio" name="allowEdit" id="allowEditY" value="yes" checked="true" /> Yes ');
	document.write('<input type="radio" name="allowEdit" id="allowEditN" value="no" /> No ');
  }else{
	document.write('<input type="radio" name="allowEdit" id="allowEditY" value="yes"  /> Yes ');
	document.write('<input type="radio" name="allowEdit" id="allowEditN" value="no" checked="true" /> No ');
  }
  </script>
</span>

// Alignment
<script>
	var isV  = "[[alignment]]";
	
	if(isV == 'v'){
		document.writeln('<input type="radio" name="alignment" id="alignV" checked="true" value="v" /> Vertical');
		document.writeln('<input type="radio" name="alignment" id="alignH" value="h" /> Horizontial');
	}else{
		document.writeln('<input type="radio" name="alignment" id="alignV" value="v" /> Vertical');
		document.writeln('<input type="radio" name="alignment" id="alignH" value="h" checked="true" /> Horizontial');
	
	}
</script>