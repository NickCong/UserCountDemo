var reportApp = {

    AllUser: [],

    GetCurrentInfo: function () {
        let APIurl = "http://localhost:51387/home/";
        let DomainURl = "http://localhost:51359/Demo/"
        var refid = window.location.href.slice(DomainURl.length).split('/')[0]
        $.ajax({
            type: "GET",
            url: APIurl + 'GetReferenceAllInfo',
            data: { "useremail": $('#hidden-email').val() },
            dataType: "json",
            success: function (data) {
                var all = parseInt(data.AllUser);
                var nobook = parseInt(data.NoBookUser);
                var book = all - nobook;
                var rs = parseInt(data.AllRefSuccess);
                var rf = parseInt(data.AllRefFail);

                $('#alluser').val(data.AllUser)
                $('#nooperateuser').val(data.NoBookUser)

                $('#operatesuccess').val(data.AllRefSuccess)
                $('#operatefail').val(data.AllRefFail)
                //GeneratePieChart(book, nobook, rfrate, rsrate)

                var rsrate = parseFloat(rs) * book / parseFloat(rs + rf);
                var rfrate = parseFloat(rf) * book / parseFloat(rs + rf);
                var reftable = [];
                reftable = reftable.concat(data.UserReferenceSuccess);
                reftable = reftable.concat(data.UserReferenceFail);
                var sourcetable = [];
                sourcetable = sourcetable.concat(data.UserSourceReferenceSuccess);
                sourcetable = sourcetable.concat(data.UserSourceReferenceFail);
                $('#reftable').bootstrapTable({
                    "columns": [
                        {
                            field: 'Email',
                            title: 'Email'
                        }, {
                            field: 'PersonalID',
                            title: 'Personal ID'
                        }, {
                            field: 'BTime',
                            title: 'Time'
                        }, {
                            field: 'BStatus',
                            title: 'Status'
                        }],
                    "data": reftable
                });
                $('#soutable').bootstrapTable({
                    "columns": [
                        {
                            field: 'Email',
                            title: 'Email'
                        }, {
                            field: 'PersonalID',
                            title: 'Personal ID'
                        }, {
                            field: 'BTime',
                            title: 'Time'
                        }, {
                            field: 'BStatus',
                            title: 'Status'
                        }],
                    "data": sourcetable
                });
            }
        })
    },

    GetAllInfo: function () {
        reportApp.GetAllUserEmail();
    },

    GetAllUserReference: function () {
        let APIurl = "http://localhost:51387/home/";
        $.ajax({
            type: "GET",
            url: APIurl + 'GetAllUserReference',
            dataType: "json",
            success: function (data) {
                $("#admin-reftable").bootstrapTable('destroy');
                $("#admin-soutable").bootstrapTable('destroy');
                var sourcetable = [];
                sourcetable = sourcetable.concat(data.UserSourceReferenceSuccess);
                sourcetable = sourcetable.concat(data.UserSourceReferenceFail);
                $('#admin-soutable').bootstrapTable({
                    "sortName": ['BTime', 'Email', 'ReferenceEmail', 'BStatus'],
                    "columns": [
                        {
                            field: 'Email',
                            title: 'Email'
                        }, {
                            field: 'ReferenceEmail',
                            title: 'Reference Email'
                        }, {
                            field: 'BTime',
                            title: 'Time'
                        }, {
                            field: 'BStatus',
                            title: 'Status'
                        }, {
                            field: 'TTime',
                            title: 'Transform Time'
                        }, {
                            field: 'TStatus',
                            title: 'Transform Status'
                        }, {
                            title: "Action",
                            formatter: reportApp.FormatterTransform()
                        }],
                    "data": sourcetable
                });
            }
        })

        $("#admin-soutable-label").text("Reference Mapping");
        $('#admin-reftable').hide()
        $("#admin-reftable-label").hide()
    },

    FormatterTransform: function (value, row) {
        if (row["TTime"] == null || row["TTime"] == undefined || row["TTime"] == '') {
            return "<span class='glyphicon glyphicon-ban-circle'></span>"
        } else {
            return "<button click=\"reportApp.TransformReferenceStatus(this)\" title='Transform Stauts'><span class='glyphicon glyphicon-transfer'></span></button>";
        }
    },

    TransformReferenceStatus: function (event) {
        let APIurl = "http://localhost:51387/home/";
        $.ajax({
            type: "GET",
            url: APIurl + 'TransformReferenceStatus',
            data: { "useremail": email },
            dataType: "json",
            success: function (data) {
                $("#admin-reftable").bootstrapTable('destroy');
                $("#admin-soutable").bootstrapTable('destroy');
                var reftable = [];
                reftable = reftable.concat(data.UserReferenceSuccess);
                reftable = reftable.concat(data.UserReferenceFail);
                var sourcetable = [];
                sourcetable = sourcetable.concat(data.UserSourceReferenceSuccess);
                sourcetable = sourcetable.concat(data.UserSourceReferenceFail);
                $('#admin-reftable').bootstrapTable({
                    "sortName": ['BTime', 'Email', 'ReferenceEmail', 'BStatus'],
                    "columns": [
                        {
                            field: 'Email',
                            title: 'Email'
                        }, {
                            field: 'PersonalID',
                            title: 'Personal ID'
                        }, {
                            field: 'BTime',
                            title: 'Time'
                        }, {
                            field: 'BStatus',
                            title: 'Status'
                        }],
                    "data": reftable
                });
                $('#admin-soutable').bootstrapTable({
                    "sortName": ['BTime', 'Email', 'ReferenceEmail', 'BStatus'],
                    "columns": [
                        {
                            field: 'Email',
                            title: 'Email'
                        }, {
                            field: 'PersonalID',
                            title: 'Personal ID'
                        }, {
                            field: 'BTime',
                            title: 'Time'
                        }, {
                            field: 'BStatus',
                            title: 'Status'
                        }],
                    "data": sourcetable
                });
            }
        })
    },

    GetChooseUserReference: function (email) {
        let APIurl = "http://localhost:51387/home/";
        $.ajax({
            type: "GET",
            url: APIurl + 'GetUserReference',
            data: { "useremail": email },
            dataType: "json",
            success: function (data) {
                $("#admin-reftable").bootstrapTable('destroy');
                $("#admin-soutable").bootstrapTable('destroy');
                var reftable = [];
                reftable = reftable.concat(data.UserReferenceSuccess);
                reftable = reftable.concat(data.UserReferenceFail);
                var sourcetable = [];
                sourcetable = sourcetable.concat(data.UserSourceReferenceSuccess);
                sourcetable = sourcetable.concat(data.UserSourceReferenceFail);
                $('#admin-reftable').bootstrapTable({
                    "sortName": ['BTime', 'Email', 'ReferenceEmail', 'BStatus'],
                    "columns": [
                        {
                            field: 'Email',
                            title: 'Email'
                        }, {
                            field: 'PersonalID',
                            title: 'Personal ID'
                        }, {
                            field: 'BTime',
                            title: 'Time'
                        }, {
                            field: 'BStatus',
                            title: 'Status'
                        }],
                    "data": reftable
                });
                $('#admin-soutable').bootstrapTable({
                    "sortName": ['BTime', 'Email', 'ReferenceEmail', 'BStatus'],
                    "columns": [
                        {
                            field: 'Email',
                            title: 'Email'
                        }, {
                            field: 'PersonalID',
                            title: 'Personal ID'
                        }, {
                            field: 'BTime',
                            title: 'Time'
                        }, {
                            field: 'BStatus',
                            title: 'Status'
                        }],
                    "data": sourcetable
                });
            }
        })
    },

    GetAllUserEmail: function () {
        let DomainURl = "http://localhost:51359/Demo/admin/"
        $.ajax({
            type: "GET",
            url: 'GetAllUserEmail',
            dataType: "json",
            success: function (data) {
                reportApp.AllUser = data;
                $.each(data, function (i, e) {
                    let liele = document.createElement('li');
                    let aLink = document.createElement('a');
                    aLink.setAttribute('href', "javascript:void(0)");
                    aLink.setAttribute('onclick', "reportApp.UserFilter(this)")
                    aLink.innerHTML = e;
                    liele.appendChild(aLink);
                    document.getElementById("userlist").appendChild(liele);
                })
            }
        })
    },

    UserFilter: function (a) {
        $('#admin-reftable').show()
        $("#admin-reftable-label").show()
        $('#admin-reference label')[0].innerText = "Reference from " + a.text;
        $('#admin-reference label')[1].innerText = a.text + "'s reference";
        reportApp.GetChooseUserReference(a.text)

    },

    GeneratePieChart: function (book, nobook, rfrate, rsrate) {
        if (book >= 1) {
            var ctx = document.getElementById("reportpiechart").getContext("2d");
            let chartJsOptions = {
                "type": "pie",
                "data": {
                    "labels": ["No Book", "Book Fail", "Book Success"],
                    "datasets": [{
                        "data": [nobook, rfrate, rsrate],
                        "backgroundColor": [
                            "#F38630",
                            "#69D2E7",
                            "#E0E4CC"
                        ],
                        "color": ["#F38630", "#69D2E7", "#E0E4CC"]
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
            var newPieChart = new Chart(ctx, chartJsOptions);
        }
        else {
            var ctx = document.getElementById("reportpiechart").getContext("2d");
            let chartJsOptions = {
                "type": "pie",
                "data": {
                    "labels": ["No Data"],
                    "datasets": [{
                        "data": [1],
                        "backgroundColor": [
                            "#69D2E7"
                        ],
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
        }
    },

    init: function () {
        if ($('#hidden-permission').val() == 'admin') {
            reportApp.GetAllInfo();
            reportApp.GetCurrentInfo();
        }
        else {
            reportApp.GetChooseUserReference($('#hidden-email').val());
        }
    }
}

$(document).ready(function () {
    reportApp.init();
});
