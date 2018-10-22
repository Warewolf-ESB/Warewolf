import { Component, OnInit } from '@angular/core';

declare let google: any;

@Component({
  selector: 'dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {

  cards = [
    { title: 'Pie Chart', cols: 1, rows: 1, chart: "<div id='chart_div'></div>" },
    { title: 'Bar Chart', cols: 1, rows: 1, chart: "<div id='barchart_div'></div>" },
    { title: 'Line chart', cols: 1, rows: 1, chart: "<div id='tablechart_div'></div>" },
    { title: 'Scatter Chart', cols: 1, rows: 1, chart: "<div id='scatterChart_div'></div>" }
  ];
  
  constructor() {
  
  }

  ngOnInit() {
           // Load the Visualization API and the corechart package.
    google.charts.load('current', { 'packages': ['corechart'] });

    // Set a callback to run when the Google Visualization API is loaded.
    google.charts.setOnLoadCallback(drawChart);

    // Callback that creates and populates a data table,
    // instantiates the pie chart, passes in the data and
    // draws it.
    function drawChart() {

      var data = new google.visualization.DataTable();

      data.addColumn('string', 'Topping');
      data.addColumn('number', 'Slices');
      data.addRows([
        ['Mushrooms', 3],
        ['Onions', 1],
        ['Olives', 1],
        ['Zucchini', 1],
        ['Pepperoni', 2]
      ]);

      // Set chart options
      var options = {
       
      };
      // Instantiate and draw our chart, passing in some options.
      var chart = new google.visualization.PieChart(document.getElementById('chart_div'));
      chart.draw(data, options);

      var barchart = new google.visualization.BarChart(document.getElementById('barchart_div'));
      barchart.draw(data, options);

      var tablechart = new google.visualization.LineChart(document.getElementById('tablechart_div'));
      tablechart.draw(data, options);

      var scatterChart = new google.visualization.ScatterChart(document.getElementById('scatterChart_div'));
      scatterChart.draw(data, options);
    }
  }
 

}
