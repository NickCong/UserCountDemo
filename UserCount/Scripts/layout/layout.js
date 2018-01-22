var themeApp = {
    sidebarConfig: function () {
        if (sidebar_left == true) {
            $('.main-content').addClass('col-md-push-4');
            $('.sidebar').addClass('col-md-pull-8');
        }
    },
    backToTop: function () {
        $(window).scroll(function () {
            if ($(this).scrollTop() > 100) {
                $('#back-to-top').fadeIn();
            } else {
                $('#back-to-top').fadeOut();
            }
        });
        $('#back-to-top').on('click', function (e) {
            e.preventDefault();
            $('html, body').animate({ scrollTop: 0 }, 1000);
            return false;
        });
    },
    init: function () {      
        // themeApp.sidebarConfig();
        themeApp.backToTop();
    }
}

$(document).ready(function () {
    themeApp.init();
});