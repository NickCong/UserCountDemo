var reportApp = {

    drawpiechart: function () {
        //var ctx = $("#reportpiechart").getContext("2d");
        let APIurl = "http://localhost:51387/API/";
        let DomainURl = "http://localhost:51359/Demo/"
        var refid = window.location.href.slice(DomainURl.length).split('/')[0]
        let chartdata=[50,50]
        $.ajax({
            type: "GET",
            url: APIurl + 'CountUserInfo',
            dataType: "json",
            success: function (data) {
                var refs = parseInt(data.Reference);
                var refn = parseInt(data.NoRerence);
                var sum = refn + refs;
                if (sum >= 1)
                {
                    chartdata = [refs, refn]
                }
            }
        });

        var ctx = document.getElementById("reportpiechart").getContext("2d");
        let chartJsOptions = {
            "type": "pie",
            "data": {
                "labels": ["Reference", "No Reference"],
                "datasets": [{
                    "label": "# of Votes",
                    "data": chartdata,
                    "backgroundColor": [
                        "#F38630",
                        "#69D2E7"                        
                    ],
                    "color": ["#F38630", "#69D2E7"]
                }]
            },
            "options": {
                "scales": {
                    "yAxes": [{
                        "ticks": {
                            "beginAtZero": true
                        }
                    }],
                    "xAxes": []
                }
            }
        };
        var newPieChart = new Chart(ctx, chartJsOptions)

    },

    init: function () {
        reportApp.drawpiechart();
        //$('#email').click(function () {
        //    $("#login-waring").hide();
        //})
        //$('#password').click(function () {
        //    $("#login-waring").hide();
        //})
    }
}

$(document).ready(function () {
    reportApp.init();
});
