/*
 * Travis.Frisinger
 * Internal date validation cuz, date.js is not working!
 */
 
 function validateDate(value, format){
	var result = true;
	var splitToken = "/";
	var dateParts = format.split(splitToken);
	var valueParts = value.split(splitToken);
 
	var i = 0;
	var day = 0;
	var month = 0;
	var year = 0;
	
	if(value != format){
		while(i < dateParts.length && result){
			if(dateParts[i] == "mm"){
				month = valueParts[i];
			}else if(dateParts[i] == "dd"){
				day = valueParts[i];
			}else if(dateParts[i] == "yy"){
				year = "20" + valueParts[i];
			}else if(dateParts[i] == "yyyy"){
				year = valueParts[i];
			}
			i++;
		} 
		
		//alert(month + " " + day + " " + year + " ~ " + value );

		// extract date range for month
		var dateStart = 1;
		var dateEnd = 1;
		if(isLeap(year) && month == 2){
			dateEnd = 29;
		}else if(!isLeap(year) && month == 2){
			dateEnd = 28;
		}else if(month != 2){
			if(month == 1 || month == 3 || month == 5 || month == 7 || month == 8 || month == 10 || month == 12){
				dateEnd = 31;
			}else{
				dateEnd = 30;
			}
		}
		
		result = ( validatePart(year, 1900, 3000) && validatePart(month, 1, 12) && validatePart(day, dateStart, dateEnd) );
	}

	return result;
 }
 
 function isLeap(year){
	var result = false;
	
	// divisble by 4 and not 100
	if((year % 4) == 0 && !((year % 100)==0)){
		result = true;
	}
	
	// divisible by 4, 100 and 400 it is a leap year
	if( (year % 400) == 0){
		result = true;
	}
	
	return result;
 }
 
 function validatePart(part, startRange, endRange){
	var result = false;
 
	if(!isNaN(part)){
		if(part >= startRange && part <= endRange){
			result = true;
		}
	}
	
	return result;
 }
 