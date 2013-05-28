/**
 * placeMe.js - A jQuery polyfill for the placeholder attribute.
 *
 * @author Matt Sparks <matt@mattsparks.com>
 * @license Unlicense <http://unlicense.org/>
 */

$(document).ready(function () {

    //Check to see if placeholder is supported
    //Hat tip: http://diveintohtml5.info/detect.html#input-placeholder
    function supports_input_placeholder() {
        var i = document.createElement('input');
        return 'placeholder' in i;
    }

    if (supports_input_placeholder()) {
        // The placeholder attribute is supported
        //Do nothing. Smile. Go outside
    }
    else {
        // The "placeholder" attribute isn't supported. Let's fake it.

        // Get all the input elements with the "placeholder" attribute
        var $placeholder = $(":input[placeholder]");

        // Go through each element and assign the "value" attribute the value of the "placeholder"
        // This will allow users with no support to see the default text
        $placeholder.each(function () {

            // The password input type in IE is masked, so the solution for other input types won't work.
            // As a work around we'll create a new <input type="text"> element and add it to the DOM.
            // Once it's created we'll place it above the password feild to essentially "hide" it below our new element.
            // When the new element is focused, we'll hide it revealing the actual password element. 
            if ($(this).attr("type") == "password") {
                // Get x position
                var x = $(this).position().left;
                // Get y position
                var y = $(this).position().top;
                // Get class attributes
                var eclass = $(this).attr("class");
                // Get id attribute
                var id = $(this).attr("id");
                // Get value
                var val = $(this).attr("placeholder");
                // Get parent element
                var parent = $(this).parent();

                // Create new input and place it within the parent element
                // Using CSS positioning we'll place it above the original element
                // We'll also add a class of "placePass" to keep track of these new elements
                $(parent).prepend('<input type="text" id="' + id + '" class="' + eclass + ' placePass" value="' + val + '" style="position: absolute; top: ' + y + ' left: ' + x + ' z-index: 9999;">');

            }

            // Make sure the value attribute is empty and not a password input
            if (($(this).attr("value") == "") && ($(this).attr("type") != "password")) {
                // Get value of the placeholder attribute
                var $message = $(this).attr("placeholder");
                // Put that value in the "value" attribute
                $(this).attr("value", $message);
            }
        });

        // When an element with a class of "placePass" is clicked, hide it.
        $(".placePass").focus(function () {
            $(this).hide();
        });

        // When a user clicks the input (on focus) the default text will be removed
        $placeholder.focus(function () {
            var $value = $(this).attr("value");
            var $placeholderTxt = $(this).attr("placeholder");

            if ($value == $placeholderTxt) {
                $(this).attr("value", "");
            }
        });

        // When a user clicks/tabs away from the input (on blur) keep what they typed
        // Unless they didn't type anything, in that case put back in the default text
        $placeholder.blur(function () {
            var $value = $(this).attr("value");
            var $placeholderTxt = $(this).attr("placeholder");

            if (($value == '') && ($(this).attr("type") != "password")) {
                $(this).attr("value", $placeholderTxt);
            }
        });

        // Since we're inputing text into the "value" attribute we need to make 
        // sure that this default text isn't submitted with the form, potentially
        // causing validation issues. So we're going to remove the default text
        // and submit the inputs as blank.
        $("form").submit(function () {
            var $checkValue = $(":input");

            $checkValue.each(function () {
                if ($(this).attr("value") == $(this).attr("placeholder")) {
                    $(this).attr("value", "");
                }
            });
        });
    }

});