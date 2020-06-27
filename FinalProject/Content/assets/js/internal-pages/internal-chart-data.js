$.ajax({
    url: '/Admin/Dashboard/Read',
    type: 'POST',
    dataType: "json",
    contentType: "application/json; charset=utf-8",
}).done(function (response) {
    console.log(response);
    var arr = JSON.parse(response);
    console.log(arr);
    var dataset = [];
    for (var i = 1; i <= 12; i++) {
        if (i in arr) {
            dataset.push(arr[i]);
        } else {
            dataset.push(Math.floor(Math.random() * 25));
        }
    }
    if (dataset != null) {
        console.log(dataset);
        var statistics_chart = document.getElementById("myChart").getContext('2d');
        var myChart = new Chart(statistics_chart, {
            type: 'line',
            data: {
                labels: ["Jan", "Feb", "March", "April", "May", "June", "July", "Aug", "Sept", "Oct", "Nov", "Dec"],
                datasets: [{
                    label: 'Contributions',
                    data: dataset,
                    borderWidth: 5,
                    borderColor: '#6777ef',
                    backgroundColor: 'transparent',
                    pointBackgroundColor: '#fff',
                    pointBorderColor: '#6777ef',
                    pointRadius: 4
                }]
            },
            options: {
                legend: {
                    display: false
                },
                scales: {
                    yAxes: [{
                        gridLines: {
                            display: false,
                            drawBorder: false,
                        },
                        ticks: {
                            stepSize: 10
                        }
                    }],
                    xAxes: [{
                        gridLines: {
                            color: '#fbfbfb',
                            lineWidth: 2
                        }
                    }]
                },
            }
        });
    }
}).fail(function () {
    console.log("error in internal chart js");
});