var Mustache = require('mustache');

const template = `
    <ul class="list-group">
      {{#.}}
      <li class="list-group-item d-flex justify-content-between align-items-center">
      <a class="nav-link text-dark" href="/Map/Index/{{showId}}">{{title}}</a>
      <span class="badge badge-primary badge-pill">{{availableSeats}}</span>
      </li>
    {{/.}}
    </ul>
`;

$(document).ready(function () {
    $.get(dataUrl).done(function (d) {
        var v = Mustache.render(template, d);
        document.getElementById("nav").innerHTML=v;
    })
    .fail(function (d) {

    })
})