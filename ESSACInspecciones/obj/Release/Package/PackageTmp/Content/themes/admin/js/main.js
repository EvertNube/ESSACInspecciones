function displayInactive() {
    $("#btn-showInactive").click(function () {
        $(".inactive").fadeToggle(300);
    });   
}

$(document).ready(function () {
    displayInactive();
});

$.fn.datepicker.defaults.format = "dd/mm/yyyy";