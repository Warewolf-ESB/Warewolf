function filterId(n) {
    // Declare variables
    var input, filter, table, tr, td, i;
    input = document.getElementById("filterId");
    filter = input.value.toUpperCase();
    table = document.getElementById("executionList");
    tr = table.getElementsByTagName("tr");

    // Loop through all table rows, and hide those who don't match the search query
    for (i = 2; i < tr.length; i++) {
        td = tr[i].getElementsByTagName("td")[n];
        if (td) {
            if (td.innerHTML.toUpperCase().indexOf(filter) > -1) {
                tr[i].style.display = "";
            } else {
                tr[i].style.display = "none";
            }
        }
    }
}
function filterTime(n) {
    // Declare variables
    var input, filter, table, tr, td, i;
    input = document.getElementById("filterTime");
    filter = input.value.toUpperCase();
    table = document.getElementById("executionList");
    tr = table.getElementsByTagName("tr");

    // Loop through all table rows, and hide those who don't match the search query
    for (i = 2; i < tr.length; i++) {
        td = tr[i].getElementsByTagName("td")[n];
        if (td) {
            if (td.innerHTML.toUpperCase().indexOf(filter) > -1) {
                tr[i].style.display = "";
            } else {
                tr[i].style.display = "none";
            }
        }
    }
}
function filterStatus(n) {
    // Declare variables
    var input, filter, table, tr, td, i;
    input = document.getElementById("filterStatus");
    filter = input.value.toUpperCase();
    table = document.getElementById("executionList");
    tr = table.getElementsByTagName("tr");

    // Loop through all table rows, and hide those who don't match the search query
    for (i = 2; i < tr.length; i++) {
        td = tr[i].getElementsByTagName("td")[n];
        if (td) {
            if (td.innerHTML.toUpperCase().indexOf(filter) > -1) {
                tr[i].style.display = "";
            } else {
                tr[i].style.display = "none";
            }
        }
    }
}
function filterStart(n) {
    // Declare variables
    var input, filter, table, tr, td, i;
    input = document.getElementById("filterStart");
    filter = input.value.toUpperCase();
    table = document.getElementById("executionList");
    tr = table.getElementsByTagName("tr");

    // Loop through all table rows, and hide those who don't match the search query
    for (i = 2; i < tr.length; i++) {
        td = tr[i].getElementsByTagName("td")[n];
        if (td) {
            if (td.innerHTML.toUpperCase().indexOf(filter) > -1) {
                tr[i].style.display = "";
            } else {
                tr[i].style.display = "none";
            }
        }
    }
}
function filterComplete(n) {
    // Declare variables
    var input, filter, table, tr, td, i;
    input = document.getElementById("filterComplete");
    filter = input.value.toUpperCase();
    table = document.getElementById("executionList");
    tr = table.getElementsByTagName("tr");

    // Loop through all table rows, and hide those who don't match the search query
    for (i = 2; i < tr.length; i++) {
        td = tr[i].getElementsByTagName("td")[n];
        if (td) {
            if (td.innerHTML.toUpperCase().indexOf(filter) > -1) {
                tr[i].style.display = "";
            } else {
                tr[i].style.display = "none";
            }
        }
    }
}
function filterUrl(n) {
    // Declare variables
    var input, filter, table, tr, td, i;
    input = document.getElementById("filterUrl");
    filter = input.value.toUpperCase();
    table = document.getElementById("executionList");
    tr = table.getElementsByTagName("tr");

    // Loop through all table rows, and hide those who don't match the search query
    for (i = 2; i < tr.length; i++) {
        td = tr[i].getElementsByTagName("td")[n];
        if (td) {
            if (td.innerHTML.toUpperCase().indexOf(filter) > -1) {
                tr[i].style.display = "";
            } else {
                tr[i].style.display = "none";
            }
        }
    }
}
function filterUser(n) {
    // Declare variables
    var input, filter, table, tr, td, i;
    input = document.getElementById("filterUser");
    filter = input.value.toUpperCase();
    table = document.getElementById("executionList");
    tr = table.getElementsByTagName("tr");

    // Loop through all table rows, and hide those who don't match the search query
    for (i = 2; i < tr.length; i++) {
        td = tr[i].getElementsByTagName("td")[n];
        if (td) {
            if (td.innerHTML.toUpperCase().indexOf(filter) > -1) {
                tr[i].style.display = "";
            } else {
                tr[i].style.display = "none";
            }
        }
    }
}