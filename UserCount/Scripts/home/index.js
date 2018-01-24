$(document).ready(function () {
    let APIurl = ""
    Vote = function (i) {
        $.ajax({
            type: "POST",
            url: APIurl + 'Vote',
            data: { currentUser: id, refUser: id, order: player },
            dataType: "json",
            success: function (data) {
                alert(data);
            }
        });
    }
});