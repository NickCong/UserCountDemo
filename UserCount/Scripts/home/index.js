$(document).ready(function () {
    let APIurl = "http://localhost:51387/home/";
    let DomainURl = "http://localhost:51359/Demo/"
    Vote = function (i) {
        var refid = window.location.href.slice(DomainURl.length).split('/')[0];
        var status = "N";
        var message = "Failed!";
        if (i <= 3) {
            status = "Y";
            message = "Suceessful!";
        }       
        $.ajax({
            type: "GET",
            url: APIurl + 'Book',
            data: { "sourceID": refid, "useremail": $("#detailemail").val(), "status": status },
            dataType: "json",
            success: function (data) {
            }
        });
        alert(message);
    }
});