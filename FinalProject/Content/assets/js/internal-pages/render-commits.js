$('#branch_select').change(function () {
    var branch_owner_user_id = $(this).val();
    var doc_id = $(this).data(doc_id).doc_id;
    //console.log("fired by " + branch_owner_user_id + " " + doc_id);

    $.ajax({
        url: '/DocumentStream/UpdateCommits',
        type: 'POST',
        data: JSON.stringify({ doc_id: doc_id, branch_owner_user_id: branch_owner_user_id }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
    }).done(function (response) {
        //console.log(response);
        var obj = JSON.parse(response);
        $('.commit_row').remove();
        for (var i = 0; i < obj.length; i++) {
            var finalrow = "";
            var date = new Date(obj[i].RevDate).getDate() + "-" + (new Date(obj[i].RevDate).getMonth() + 1) + "-" + new Date(obj[i].RevDate).getFullYear() + " " + new Date(obj[i].RevDate).getHours() + ":" + new Date(obj[i].RevDate).getMinutes() + ":" + new Date(obj[i].RevDate).getSeconds();
            finalrow = "<tr class='commit_row'><td>" + obj[i].Version + "</td>" + "<td>" + obj[i].Comment + "</td>" + "<td>" + date + "</td>"
            finalrow += "<td><a href='/DocumentStream/DownloadFile?doc_ver_id=" + obj[i].Id + "' class='btn btn-primary mr-2'><i class='fa fa-download'></i></a><a href='#' class='btn btn-info mr-2' data-doc_ver_id='" + obj[i].Id + "'><i class='fa fa-eye'></i></a></td></tr>";
            $('#commit_table_body').append(finalrow);
        }
    }).fail(function () {
        console.log("error");
    });
    $.ajax({
        url: '/DocumentStream/isBranchUser',
        type: 'POST',
        data: JSON.stringify({ user_id: branch_owner_user_id }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
    }).done(function (response) {
        //console.log(response);
        if (response.result == "true") {
            $('#actions').removeAttr("hidden");
        }
        if (response.result == "false") {
            $('#actions').attr("hidden",true);
        }
    }).fail(function () {
        console.log("error");
    });
});

