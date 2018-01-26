$(document).ready(function () {
    let APIurl = "http://localhost:51387/API/";
    let DomainURl = "http://localhost:51359/Demo/"
    Vote = function (i) {
        var refid = window.location.href.slice(DomainURl.length).split('/')[0]
        $.ajax({
            type: "GET",
            url: APIurl + 'CountUserInfo',
            data: { "sourceID": refid, "useremail": $("#detailemail").val() },
            dataType: "json",
            success: function (data) {
                //alert(data);
            }
        });
    }
});