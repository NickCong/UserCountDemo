var themeApp = {
    sidebarConfig: function () {
        if (sidebar_left === true) {
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
    getUserInfo: function () { 
        var email = $.session.get('email'); 
        var password = $.session.get('password');
        if (email != null && email != undefined && email != '')
        {
            $.ajax({
                type: "POST",
                url: 'home/GetUser',
                data: { email: $('#email').val(), password: $('#password').val() },
                dataType: "json",
                success: function (data) {
                    if (data.result) {
                        $('#Auth-login').hide();
                        bingUserDetail(data);
                        $('#shade').hide();
                        $('#userinfo').show();
                    }
                    else {
                        $("#login-waring").text("Login Failed, please check your eamil and password")
                        $('#userinfo').hide();
                        $("#login-waring").show();
                    }
                }
            });
        }
    },
    bingUserDetail: function (data) {
        $("#detailemail").val(data.email);
        $("#detailpersonalurl").text(data.personalurl);
        $("#detailphone").text(data.phonenumber);
    },
    init: function () {
        //themeApp.getUserInfo();
        themeApp.backToTop();
        $('#email').click(function () {
            $("#login-waring").hide();
        })
        $('#password').click(function () {
            $("#login-waring").hide();
        })
        $('#login').click(function () {
            var bootstrapValidator = $('.login-email').data('bootstrapValidator');
            bootstrapValidator.validate();
            if (bootstrapValidator.isValid()) {
                $.ajax({
                    type: "POST",
                    url: 'home/Login',
                    data: { email: $('#email').val(), password: $('#password').val(), source: window.location.href },
                    dataType: "json",
                    success: function (data) {
                        if (data.result) {
                            $('#Auth-login').hide();
                            themeApp.bingUserDetail(data);
                            $('#shade').hide();
                            $('#userinfo').show();
                        }
                        else {
                            $("#login-waring").text("Login Failed, please check your eamil and password")
                            $('#userinfo').hide();
                            $("#login-waring").show();
                        }
                    }
                });
            }
        });
        $('#register').click(function () {
            var bootstrapValidator = $('.login-email').data('bootstrapValidator');
            bootstrapValidator.validate();
            if (bootstrapValidator.isValid()) {
                $.ajax({
                    type: "POST",
                    url: 'home/Register',
                    data: { email: $('#email').val(), password: $('#password').val(), source: window.location.href },
                    dataType: "json",
                    success: function (data) {
                        if (data.result) {
                            $('#Auth-login').hide();
                            themeApp.bingUserDetail(data);
                            $('#shade').hide();
                            $('#userinfo').show();
                        }
                        else {
                            $("#login-waring").text("Register failed, the email has been registered!")
                            $('#userinfo').hide();
                            $("#login-waring").show();
                        }
                    }
                });
            }
        });
        $('.login-email').bootstrapValidator({
            feedbackIcons: {
                valid: 'glyphicon glyphicon-ok',
                //invalid: 'glyphicon glyphicon-remove',
                validating: 'glyphicon glyphicon-refresh'
            },
            fields: {
                email: {
                    validators: {
                        notEmpty: {
                            message: 'email cannot be empty'
                        },
                        emailAddress: {
                            message: 'Please enter a valid email address：123@qq.com'
                        }
                    }
                }
                , password: {
                    message: 'Password is invalid!',
                    validators: {
                        notEmpty: {
                            message: 'Password cannot be empty!'
                        },
                        stringLength: {
                            min: 4,
                            max: 10,
                            message: 'The length of password is 4 to 10!'
                        }
                    }
                }
            }
        })
    }
}

$(document).ready(function () {
    themeApp.init();    
});
