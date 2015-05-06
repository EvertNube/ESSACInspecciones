$(window).load(function () {
    $('.nav-primary').on('click', 'li', function () {
        $('.nav-primary li.active').removeClass('active');
        $(this).addClass('active');
    });
});